using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using Microsoft.Win32;
using System.Diagnostics;

using Utils;
using FileDbNs;

namespace FileDbExplorer
{
    public partial class MainFrm : Form
    {
        // static
        static int s_tabCount = 1;

        //internal const string STR_VERSION = "1.0";
        static string s_version;

        internal const string DMRU = "DMRU";

        internal const string WIN_REG_KEY = @"Software\Microsoft\Windows NT\CurrentVersion";

        internal const string StrFileDbFilter = "FileDb files (*.fdb)|*.fdb|All files (*.*)|*.*";

        internal const string StrFileDbRegKey = @"SOFTWARE\EzTools\FileDb";

        internal const string StrLastUsedDir = "LastUsedDir";

        // internal
        internal static  MainFrm TheAppWnd;

        internal DbView DbView;

        string[] _args;

        // private
        TreeNode _rClickNode;

        internal const string ProdKey = "Elh8Psdr%f46wsHk3y4fk!nSF7C`4T3Q";  //5E9CE9F3-FD6B-4c82-9F0D-E0B4955B20C9";

        #if false
        internal LicInfo LicenseInfo = new LicInfo();

        internal class LicInfo
        {
            internal string LicName = "";
            internal bool Nagged = false;
            internal DateTime ExpiryDate = DateTime.MinValue;
            private bool _bIsLicensed = false;

            internal bool IsLicensed
            {
                get { return _bIsLicensed; }
                set { _bIsLicensed = value; }
            }
        }
        #endif

        internal static string Version
        {
            get
            {
                if( string.IsNullOrEmpty( s_version ) )
                {
                    Assembly asm = Assembly.GetExecutingAssembly();
                    AssemblyName asmName = asm.GetName();
                    s_version = asmName.Version.ToString();
                }
                return s_version;
            }
        }

        //---------------------------------------------------------------------
        #if false
        internal void SetNagged()
        {
            LicenseInfo.Nagged = true;
        }
        #endif

        //---------------------------------------------------------------------
        public MainFrm( string[] args )
        {
            _args = args;
            TheAppWnd = this;
            InitializeComponent();
        }

        //---------------------------------------------------------------------
        private void BtnOpenDb_Click( object sender, EventArgs e )
        {
            openFile();
        }

        //---------------------------------------------------------------------
        void openFile()
        {
            try
            {
                string filename = GetDbFilename( true );

                if( filename != null )
                {
                    DbView.OpenDatabase( filename );
                }
            }
            catch( Exception ex )
            {
                MessageBox.Show( this, ex.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Exclamation );
            }
        }

        internal static string GetDbFilename( bool fileMustExist )
        {
            string filename = null;
            OpenFileDialog fileDlg = new OpenFileDialog();
            fileDlg.Filter = StrFileDbFilter;
            fileDlg.SupportMultiDottedExtensions = true;
            fileDlg.CheckFileExists = fileMustExist;
            RegistryKey regKey = Registry.CurrentUser.OpenSubKey( StrFileDbRegKey, true );
            if( regKey == null )
                regKey = Registry.CurrentUser.CreateSubKey( StrFileDbRegKey );

            string lastUsedDir = null;
            if( regKey != null )
                lastUsedDir = (string) regKey.GetValue( StrLastUsedDir );

            if( lastUsedDir == null || !Directory.Exists( lastUsedDir ) )
                lastUsedDir = Application.StartupPath;

            fileDlg.InitialDirectory = lastUsedDir; // @"E:\EzTools.NET\FileDb\Windows\SampleApp\bin\Debug";

            if( fileDlg.ShowDialog() == DialogResult.OK )
            {
                filename = fileDlg.FileName;
                if( regKey != null )
                    regKey.SetValue( StrLastUsedDir, Path.GetDirectoryName( filename ) );
            }

            if( regKey != null )
                regKey.Close();
            
            return filename;
        }

        private void MainFrm_Load( object sender, EventArgs e )
        {
            DbTree.ShowRootLines = false;
            DbView = new DbView(DbTree);

            Helpers.RestoreFormPos( this, Path.Combine( StrFileDbRegKey, "FormSettings", "Main" ) );

            timer.Enabled = true;

            #if DEBUG
            DbView.OpenDatabase( Path.Combine( Application.StartupPath, "Employees.fdb" ) );
            #endif
        }

        QueryCtrl getQueryCtrlFromTabPage( TabPage tabPage )
        {
            return tabPage.Controls[0] as QueryCtrl;
        }

        private void MainFrm_FormClosing( object sender, FormClosingEventArgs e )
        {
            try
            {
                foreach( TabPage tabPage in Tabs.TabPages )
                {
                    QueryCtrl queryCtrl = getQueryCtrlFromTabPage( tabPage );
                    if( !queryCtrl.OkToClose() )
                    {
                        e.Cancel = true;
                        return;
                    }
                }

                DbView.CloseAll();
            }
            catch { }

            if( !e.Cancel )
            {
                Utils.Helpers.SaveFormPos( this, Path.Combine( StrFileDbRegKey, "FormSettings", "Main" ) );
            }

            // Keep track of last used date for licensing
            try
            {
                RegistryKey regKey = Registry.LocalMachine.OpenSubKey( MainFrm.WIN_REG_KEY, true );
                if( regKey == null )
                {
                    regKey = Registry.CurrentUser.OpenSubKey( MainFrm.WIN_REG_KEY, true );
                }
                int nDate = (int) DateTime.Now.ToOADate();
                regKey.SetValue( DMRU, nDate.ToString() );
            }
            catch { }
        }

        private void BtnNewQueryWnd_Click( object sender, EventArgs e )
        {
            createNewQueryTab( null );
        }

        private void MnuOpenTable_Click( object sender, EventArgs e )
        {
            if( _rClickNode != null )
            {
                createNewQueryTab( _rClickNode.Text );
                _rClickNode = null;
            }
        }

        void createNewQueryTab( string tableName )
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                this.Update();
                TabPage newPage = new TabPage( "Query " + (++s_tabCount).ToString() );
                var queryCtrl = new QueryCtrl();
                queryCtrl.Dock = DockStyle.Fill;
                newPage.Controls.Add( queryCtrl );
                Tabs.TabPages.Add( newPage );
                Tabs.SelectedTab = newPage;

                if( tableName != null )
                {
                    string sql;
                    //sql = string.Format( "SELECT ID, [FirstName], LastName FROM [{0}]", tableName );
                    //sql = string.Format( "SELECT * FROM [{0}] ORDERBY LastName", tableName );
                    //sql = string.Format( @"SELECT ID, [FirstName], LastName FROM [{0}] WHERE firstname LIKE '\bjanet' ORDER BY LastName", tableName );
                    sql = string.Format( "SELECT * FROM {0}", tableName );
                    queryCtrl.Execute( sql, true );
                }
                this.Cursor = Cursors.Default;
            }
            catch( Exception ex )
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show( this, ex.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Exclamation );
            }
        }

        private void MnuRefreshTable_Click( object sender, EventArgs e )
        {
            if( _rClickNode != null )
            {
                NodeInfo nodeInfo = _rClickNode.Tag as NodeInfo;
                DbView.CreateFieldNodes( nodeInfo.Tag as FileDb, _rClickNode );
                _rClickNode = null;
            }
        }

        private void DbTree_MouseClick( object sender, MouseEventArgs e )
        {
            if( e.Button == MouseButtons.Right )
            {
                _rClickNode = DbTree.GetNodeAt( e.X, e.Y );
                DbTree.SelectedNode = _rClickNode;
            }
        }

        private void Tabs_MouseClick( object sender, MouseEventArgs e )
        {
            if( e.Button == System.Windows.Forms.MouseButtons.Right )
            {
                Point pt = new Point( e.X, e.Y );
                for( int n=0; n < Tabs.TabCount; n++ )
                {
                    if( Tabs.GetTabRect( n ).Contains( pt ) )
                    {
                        TabPage tabPage = Tabs.TabPages[n];
                        QueryCtrl queryCtrl = getQueryCtrlFromTabPage( tabPage );
                        if( queryCtrl.OkToClose() )
                            Tabs.TabPages.Remove( tabPage );
                    }
                }
            }
        }

        private void MnuCleanDb_Click( object sender, EventArgs e )
        {
            if( _rClickNode != null )
            {
                try
                {
                    NodeInfo nodeInfo = _rClickNode.Tag as NodeInfo;
                    _rClickNode = null;
                    var fileDb = nodeInfo.Tag as FileDb;
                    fileDb.Clean();
                }
                catch( Exception ex )
                {
                    MessageBox.Show( this, ex.Message, null );
                }
            }
        }

        private void MnuReindexDb_Click( object sender, EventArgs e )
        {
            if( _rClickNode != null )
            {
                try
                {
                    NodeInfo nodeInfo = _rClickNode.Tag as NodeInfo;
                    _rClickNode = null;
                    var fileDb = nodeInfo.Tag as FileDb;
                    fileDb.Reindex();
                }
                catch( Exception ex )
                {
                    MessageBox.Show( this, ex.Message, null );
                }
            }
        }

        private void MnuCloseDb_Click( object sender, EventArgs e )
        {
            if( _rClickNode != null )
            {
                NodeInfo nodeInfo = _rClickNode.Tag as NodeInfo;
                var fileDb = nodeInfo.Tag as FileDb;
                fileDb.Close();
                DbTree.Nodes.Remove( _rClickNode );
                _rClickNode = null;
            }
        }

        private void timer_Tick( object sender, EventArgs e )
        {
            timer.Enabled = false;
            //CheckLicense();

            if( _args != null && _args.Length > 0 )
            {
                foreach( string arg in _args )
                {
                    DbView.OpenDatabase( arg );
                }
            }
        }

        //---------------------------------------------------------------------
#if false
        void CheckLicense()
        {
            const string LIC_NEWER = "This is a newer version than your license - you must upgrade your license to run this version.";
            const string LIC_MISMATCH = "License info mismatch - FileDb is not licensed to run on this machine.";
            const string LIC_NONE = "No license found - running in evaluation mode";
            const string LIC_ERR = "Unexpected error while checking license.";
            const string LIC_EXPIRED = "Your license has expired.";
            const string LIC_BAD_DATE = "Date manipulation problem encountered.";

            string sErr = LIC_NONE;

            try
            {
#if !DEBUG
                byte[] err = new byte[1];

                string sPath = Application.StartupPath + @"\FileDb.lic";
                if( !File.Exists( sPath ) )
                    err[1] += 1; // purposely out of bounds

                sErr = LIC_ERR;
                string sEncodedLic;

                using( StreamReader reader = new StreamReader( sPath ) )
                {
                    sEncodedLic = reader.ReadToEnd();
                }
                // decrypt
                Rijndael decryptor = Utils.Encryption.GetLicenseEncryptor( ProdKey );
                int nDiscarded;
                byte[] vData = Utils.HexEncoding.GetBytes( sEncodedLic, out nDiscarded );
                string str = Utils.Encryption.DecryptString( decryptor, vData );
                string[] vsDetails = str.Split( new Char[] { '|' }, StringSplitOptions.None );

                string sVersion, sGuid, sPcName, sRegName;
                DateTime dtIssueDate;

                sGuid = vsDetails[0];
                sVersion = vsDetails[1];
                int nMacs = Int32.Parse( vsDetails[2] );
                int ndx = 3;
                string[] vsLicMacs = new string[nMacs];
                for( int n = 0; n < nMacs; n++, ndx++ )
                {
                    vsLicMacs[n] = vsDetails[ndx];
                }
                sPcName = vsDetails[ndx++];
                sRegName = vsDetails[ndx++];
                dtIssueDate = DateTime.FromOADate( double.Parse( vsDetails[ndx++] ) );
                string sExpiryDate = vsDetails[ndx++];
                MainFrm.TheAppWnd.LicenseInfo.ExpiryDate = string.IsNullOrEmpty( sExpiryDate ) ? DateTime.MinValue :
                    DateTime.FromOADate( double.Parse( sExpiryDate ) );

                sErr = LIC_EXPIRED;
                DateTime dtExpiry = MainFrm.TheAppWnd.LicenseInfo.ExpiryDate;
                if( dtExpiry > DateTime.MinValue )
                {
                    // do date manipulatioin checks
                    if( dtExpiry < DateTime.Now )
                        err[1] += 1;

                    sErr = LIC_BAD_DATE;
                    int nNow = (int) DateTime.Now.ToOADate(),
                        nIssueDate = (int) dtIssueDate.ToOADate();
                    if( nIssueDate > nNow )
                        err[1] += 1;

                    // check the MRU date
                    RegistryKey regKey = Registry.LocalMachine.OpenSubKey( MainFrm.WIN_REG_KEY, false );
                    if( regKey == null )
                    {
                        regKey = Registry.CurrentUser.OpenSubKey( MainFrm.WIN_REG_KEY, false );
                    }
                    if( regKey != null )
                    {
                        int nMruDate = int.Parse( (string) regKey.GetValue( DMRU, "0" ) );
                        if( nMruDate > nNow )
                            err[1] += 1;
                    }
                }

                sErr = LIC_MISMATCH;

                if( true ) // coreApps == null || sRegName.ToUpper() != coreApps.ToUpper() )
                {
                    bool bMatchFound = false;

                    // the first check is the MAC address
                    string[] vsMachMacs = Utils.MacAddr.GetMacs();

                    foreach( string sMachMac in vsMachMacs )
                    {
                        foreach( string sLicMac in vsLicMacs )
                        {
                            if( sLicMac == sMachMac )
                            {
                                bMatchFound = true;
                                goto Z;
                            }
                        }
                    }

                    if( !bMatchFound )
                        err[1] += 1;
                }

            Z:
                if( !string.IsNullOrEmpty( sVersion ) )
                {
                    sErr = LIC_ERR;

                    string[] vsVer, vsLicVer;
                    Char[] toks = new Char[] { '.' };

                    vsVer = Version.Split( toks );
                    vsLicVer = sVersion.Split( toks );

                    int nMajor, nLicMajor, nMinor, nLicMinor;

                    nMajor = int.Parse( vsVer[0] );
                    nMinor = int.Parse( vsVer[1] );
                    nLicMajor = int.Parse( vsLicVer[0] );
                    nLicMinor = int.Parse( vsLicVer[1] );

                    sErr = LIC_NEWER;
                    if( nMajor > nLicMajor )
                        err[1] += 1;
                    /*else
                    {
                        if( nMajor == nLicMajor && nMinor > nLicMinor )
                            err[1] += 1;
                    }*/
                }
                LicenseInfo.LicName = sRegName;
#endif
                LicenseInfo.IsLicensed = true;

            }
            catch //( Exception ex )
            {
                TrialDlg trialDlg = new TrialDlg( sErr );
                trialDlg.ShowDialog( this );
            }
        }
#endif
        private void MnuSetEncryptionKey_Click( object sender, EventArgs e )
        {
            InputDlg dlg = new Utils.InputDlg( "Enter Encryption Key", "Enter the Encryption Key for this database file", true );

            try
            {
                if( dlg.ShowDialog( this ) == System.Windows.Forms.DialogResult.OK )
                {
                    if( _rClickNode != null )
                    {
                        NodeInfo nodeInfo = _rClickNode.Tag as NodeInfo;
                        _rClickNode = null;
                        var fileDb = nodeInfo.Tag as FileDb;
                        string key = dlg.TxtInput.Text.Trim();
                        if( string.IsNullOrEmpty( key ) )
                            throw new Exception( "The encryption key cannot be empty" );
                        fileDb.SetEncryptionKey( key );
                    }
                }
            }
            catch( Exception ex )
            {
                MessageBox.Show( this, ex.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Exclamation );
            }
        }

        private void MnuHelp_Click( object sender, EventArgs e )
        {
            try
            {
                Process.Start( Path.Combine( Application.StartupPath, "Help.html" ) );
            }
            catch( Exception ex )
            {
                MessageBox.Show( this, ex.Message, null );
            }
        }

        private void MnuAbout_Click( object sender, EventArgs e )
        {
            AboutDlg aboutDlg = new AboutDlg();
            aboutDlg.ShowDialog( this );
        }

        private void MnuAddField_Click( object sender, EventArgs e )
        {
            if( _rClickNode != null )
            {
                try
                {
                    NodeInfo nodeInfo = _rClickNode.Tag as NodeInfo;
                    var fileDb = nodeInfo.Tag as FileDb;

                    FrmAddField dlg = new FrmAddField( fileDb );
                    if( dlg.ShowDialog( this ) == DialogResult.OK )
                    {
                        // must reload the DB
                        DbTree.Nodes.Remove( _rClickNode );
                        DbView.OpenDatabase( fileDb );
                    }
                }
                catch( Exception ex )
                {
                    MessageBox.Show( this, ex.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Error );
                }
                _rClickNode = null;
            }
        }

        private void MnuRemoveField_Click( object sender, EventArgs e )
        {
            try
            {
                NodeInfo nodeInfo = _rClickNode.Tag as NodeInfo;
                string fieldName = nodeInfo.Tag as string;

                if( MessageBox.Show( this, 
                                string.Format( "Are you sure you want to permanently delete field '{0}' from the database and lose its data?",
                                        fieldName ),
                                "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question ) == DialogResult.No )
                    return;

                this.Cursor = Cursors.WaitCursor;
                this.Update();
                NodeInfo parentNodeInfo = _rClickNode.Parent.Tag as NodeInfo;
                var fileDb = parentNodeInfo.Tag as FileDb;

                fileDb.DeleteField( fieldName );

                // must reload the DB
                DbTree.Nodes.Remove( _rClickNode );

                this.Cursor = Cursors.Default;
                _rClickNode = null;
            }
            catch( Exception ex )
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show( this, ex.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Error );
            }
        }

        private void MnuRenameField_Click( object sender, EventArgs e )
        {
            try
            {
                NodeInfo nodeInfo = _rClickNode.Tag as NodeInfo;
                string fieldName = nodeInfo.Tag as string;

                NodeInfo parentNodeInfo = _rClickNode.Parent.Tag as NodeInfo;
                var fileDb = parentNodeInfo.Tag as FileDb;

                Utils.InputDlg dlg = new Utils.InputDlg( "Rename Field", "Enter the new Field name", true, fieldName );

                if( dlg.ShowDialog( this ) == DialogResult.OK )
                {
                    this.Cursor = Cursors.WaitCursor;
                    this.Update();
                    fileDb.RenameField( fieldName, dlg.Value );
                    var vs = _rClickNode.Text.Split( '[' );
                    string text = string.Format( "{0} [{1}", dlg.Value, vs[1] );
                    _rClickNode.Text = text;
                    nodeInfo.Tag = dlg.Value;
                    this.Cursor = Cursors.Default;
                }

                _rClickNode = null;
            }
            catch( Exception ex )
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show( this, ex.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Error );
            }
        }
    }
}
