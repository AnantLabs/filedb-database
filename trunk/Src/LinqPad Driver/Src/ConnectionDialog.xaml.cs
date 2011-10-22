using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;

using LINQPad.Extensibility.DataContext;

namespace FileDbDynamicDriverNs
{
	/// <summary>
	/// Interaction logic for ConnectionDialog.xaml
	/// </summary>
	public partial class ConnectionDialog : Window
	{
        const string StrDefaultExtension = "fdb";
        const string StrInvalidInput = "Invalid";
        const string StrMustSelectFolder = "You must select a folder with at least one FileDb database file";
        const string StrInvalidFileExtension = "You must enter a valid file extension, eg: fdb";
        const string StrFolderMustExist = "The specified folder does not exist.  You must select a folder which is reachable.";

        FileDbDynamicDriverProperties _properties;

        public class ListViewData
        {
            public ListViewData()
            {
                // default constructor
            }

            public ListViewData( string name, string size )
            {
                Name = name;
                Size = size;
            }

            public string Name { get; set; }
            public string Size { get; set; }
        }

        public ConnectionDialog()
        {
            init( null );
        }

        public ConnectionDialog( IConnectionInfo cxInfo )
        {
            init( cxInfo );
        }

        void init( IConnectionInfo cxInfo )
        {
            _properties = new FileDbDynamicDriverProperties( cxInfo );

            Background = SystemColors.ControlBrush;
            InitializeComponent();

            TxtExtension.Text = _properties.Extension = StrDefaultExtension;

#if DEBUG
            TxtFolder.Text = _properties.Folder = @"C:\Dev\EzTools.NET\FileDb\LinqPad Driver\Northwind Database";
#else
            //string path = System.IO.Path.GetDirectoryName( System.Reflection.Assembly.GetExecutingAssembly().Location );
            //path = System.IO.Path.Combine( path, "Northwind Database" );
            //if( Directory.Exists( path ) )
            //    TxtFolder.Text = _properties.Folder = path;
#endif

            DataContext = _properties;

            refreshList();
        }

		void BtnOK_Click (object sender, RoutedEventArgs e)
		{
            string folderName = TxtFolder.Text.Trim();

            if( string.IsNullOrEmpty( folderName ) )
            {
                System.Windows.MessageBox.Show( StrMustSelectFolder, StrInvalidInput, MessageBoxButton.OK, MessageBoxImage.Information );
                return;
            }

            if( !Directory.Exists( folderName ) )
            {
                System.Windows.MessageBox.Show( StrFolderMustExist, StrInvalidInput, MessageBoxButton.OK, MessageBoxImage.Information );
                return;
            }

            string extension = TxtExtension.Text.Trim();

            if( string.IsNullOrEmpty( extension ) )
            {
                System.Windows.MessageBox.Show( StrInvalidFileExtension, StrInvalidInput, MessageBoxButton.OK, MessageBoxImage.Information );
                return;
            }

            DirectoryInfo dirInfo = new DirectoryInfo( folderName );
            FileInfo[] files = dirInfo.GetFiles( String.Format( "*.{0}", TxtExtension.Text ) );
            if( files.Length == 0 )
            {
                System.Windows.MessageBox.Show( StrMustSelectFolder, StrInvalidInput, MessageBoxButton.OK, MessageBoxImage.Information );
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
			DialogResult = true;
		}

        private void BtnBrowse_Click( object sender, RoutedEventArgs e )
        {
            FolderBrowserDialog folderDlg = new FolderBrowserDialog();
            folderDlg.ShowNewFolderButton = false;
            folderDlg.SelectedPath = _properties.Folder;

            if( folderDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK )
            {
                TxtFolder.Text = folderDlg.SelectedPath;
                setFriendlyName( TxtFolder.Text );
                refreshList();
            }
        }

        private void refreshList()
        {
            string extension = TxtExtension.Text.Trim();

            if( string.IsNullOrEmpty( extension ) )
            {
                System.Windows.MessageBox.Show( StrInvalidFileExtension, StrInvalidInput,
                    MessageBoxButton.OK, MessageBoxImage.Information );
                TxtExtension.Text = StrDefaultExtension;
                return;
            }

            string folderName = TxtFolder.Text.Trim();
            /*
            if( string.IsNullOrEmpty( folderName ) )
            {
                System.Windows.MessageBox.Show( StrMustSelectFolder, StrInvalidInput, 
                    MessageBoxButton.OK, MessageBoxImage.Information );
                return;
            }*/

            LstFiles.Items.Clear();

            if( !Directory.Exists( folderName ) )
            {
                //System.Windows.MessageBox.Show( StrFolderMustExist, StrInvalidInput, MessageBoxButton.OK, MessageBoxImage.Information );
                return;
            }

            DirectoryInfo dirInfo = new DirectoryInfo( folderName );
            FileInfo[] files = dirInfo.GetFiles( String.Format( "*.{0}", TxtExtension.Text ) );

            foreach( FileInfo fi in files )
            {
                LstFiles.Items.Add( new ListViewData( fi.Name, fi.Length.ToString("N0")  ) );
            }
        }

        private void TxtFolder_KeyDown( object sender, System.Windows.Input.KeyEventArgs e )
        {
            if( e.Key == Key.Enter )
            {
                refreshList();
            }
        }

        private void TxtExtension_KeyDown( object sender, System.Windows.Input.KeyEventArgs e )
        {
            if( e.Key == Key.Enter )
            {
                refreshList();
            }
        }

        private void BtnRefresh_Click( object sender, RoutedEventArgs e )
        {
            refreshList();
        }

        private void TxtFolder_LostFocus( object sender, RoutedEventArgs e )
        {
            if( TxtFriendlyName.Text.Trim().Length == 0 ) // assume first time
                refreshList();
            setFriendlyName( TxtFolder.Text );
        }

        void setFriendlyName( string folderPath )
        {
            // only set it if it was previously empty
            if( string.IsNullOrEmpty( TxtFriendlyName.Text ) )
                TxtFriendlyName.Text = folderPath.Substring( folderPath.LastIndexOf( '\\' ) + 1 );
        }

        private void Hyperlink_RequestNavigate( object sender, RequestNavigateEventArgs e )
        {
            string navigateUri = LnkDownload.NavigateUri.ToString();
            Process.Start( new ProcessStartInfo( navigateUri ) );
            e.Handled = true;
        }

	}
}
