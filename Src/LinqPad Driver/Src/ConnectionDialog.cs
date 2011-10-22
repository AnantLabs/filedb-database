using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

using LINQPad.Extensibility.DataContext;

namespace FileDbDynamicDriverNs
{
    public partial class ConnectionDialog : Form
    {
        const string StrDefaultExtension = "fdb";
        const string StrInvalidInput = "Invalid";
        const string StrMustSelectFolder = "You must select a folder with at least one FileDb database file";
        const string StrInvalidFileExtension = "You must enter a valid file extension, eg: fdb";

        FileDbDynamicDriverProperties _properties;

        bool _cancelOrOkClicked = false;

        public bool CancelOrOkClicked
        {
            get { return _cancelOrOkClicked; }
        }

        public ConnectionDialog()
        {
            init();
        }

        public ConnectionDialog( IConnectionInfo cxInfo )
        {
            init();

            //DataContext = // this was for WPF dialog
                _properties = new FileDbDynamicDriverProperties( cxInfo );
        }

        void init()
        {
            InitializeComponent();

            TxtExtension.Text = StrDefaultExtension;
        }

        private void BtnOK_Click( object sender, EventArgs e )
        {
            string folderName = TxtFolder.Text.Trim();

            if( string.IsNullOrEmpty( folderName ) )
            {
                MessageBox.Show( StrMustSelectFolder, StrInvalidInput, MessageBoxButtons.OK, MessageBoxIcon.Information );
                return;
            }

            string extension = TxtExtension.Text.Trim();

            if( string.IsNullOrEmpty( extension ) )
            {
                MessageBox.Show( StrInvalidFileExtension, StrInvalidInput, MessageBoxButtons.OK, MessageBoxIcon.Information );
                return;
            }

            // _properties will be null if I'm using my TestApp
            if( _properties != null )
            {
                _properties.Folder = folderName;
                _properties.Extension = extension;


            }

            #if false // DEBUG
            string code = SchemaBuilder.GenerateCode( folderName, extension, "LINQPad.User" );
            #endif

            _cancelOrOkClicked = true;

            DialogResult = DialogResult.OK;
        }

        private void BtnBrowse_Click( object sender, EventArgs e )
        {
            FolderBrowserDialog folderDlg = new FolderBrowserDialog();
            folderDlg.ShowNewFolderButton = false;
            #if DEBUG
            folderDlg.SelectedPath = @"C:\Dev\EzTools.NET\FileDb\LinqPad Driver\FileDbDynamicDriver\Northwind Database";
            #endif

            if( folderDlg.ShowDialog( this ) == DialogResult.OK )
            {
                TxtFolder.Text = folderDlg.SelectedPath;
                setFriendlyName( TxtFolder.Text );
                refreshList();
            }
        }

        void setFriendlyName( string folderPath )
        {
            // only set it if it was empty
            if( string.IsNullOrEmpty( TxtFriendlyName.Text ) )
                TxtFriendlyName.Text = folderPath.Substring( folderPath.LastIndexOf( '\\' ) + 1 );
        }

        private void refreshList()
        {
            string extension = TxtExtension.Text.Trim();

            if( string.IsNullOrEmpty( extension ) )
            {
                MessageBox.Show( StrInvalidFileExtension, StrInvalidInput, MessageBoxButtons.OK, MessageBoxIcon.Information );
                TxtExtension.Text = StrDefaultExtension;
                return;
            }

            string folderName = TxtFolder.Text.Trim();

            if( string.IsNullOrEmpty( folderName ) )
            {
                MessageBox.Show( StrMustSelectFolder, StrInvalidInput, MessageBoxButtons.OK, MessageBoxIcon.Information );
                return;
            }

            LstFiles.Items.Clear();

            DirectoryInfo dirInfo = new DirectoryInfo( TxtFolder.Text );
            FileInfo[] files = dirInfo.GetFiles( String.Format( "*.{0}", TxtExtension.Text ) );

            foreach( FileInfo fi in files )
            {
                ListViewItem item = LstFiles.Items.Add( fi.Name );
                item.SubItems.Add( fi.Length.ToString() );
            }
        }

        private void TxtExtension_KeyDown( object sender, KeyEventArgs e )
        {
            if( e.KeyCode == Keys.Enter )
            {
                refreshList();
            }
        }

        private void TxtExtension_Leave( object sender, EventArgs e )
        {
            refreshList();
        }

        private void TxtFolder_KeyDown( object sender, KeyEventArgs e )
        {
            if( e.KeyCode == Keys.Enter )
            {
                refreshList();
            }
        }

        private void BtnCancel_Click( object sender, EventArgs e )
        {
            _cancelOrOkClicked = true;
        }


    }
}
