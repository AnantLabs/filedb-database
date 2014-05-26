using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using Microsoft.Win32;
using System.Security.Cryptography;

using Utils;

namespace FileDbExplorer
{
    public partial class LicenseInfoDlg : Form
    {
        string _sVer;

        public LicenseInfoDlg( string sVer )
        {
            InitializeComponent();
            _sVer = sVer;
        }

        private void LicenseInfoDlg_Load( object sender, EventArgs e )
        {
            string[] vsMacs = MacAddr.GetMacs();
            StringBuilder sb = new StringBuilder();
            sb.Append( _sVer ); // version
            sb.Append( '|' );
            sb.Append( vsMacs.Length.ToString() );
            sb.Append( '|' );
            foreach( string sMac in vsMacs )
            {
                sb.Append( sMac );
                sb.Append( '|' );
            }
            sb.Append( SystemInformation.ComputerName );
            
            string str;
            // Attempt to get Windows Registration info
            RegistryKey regKey = Registry.LocalMachine.OpenSubKey( MainFrm.WIN_REG_KEY, true );
            if( regKey != null )
            {
                str = regKey.GetValue( "RegisteredOwner" ) as string;
                sb.Append( '|' );
                sb.Append( str );
                str = regKey.GetValue( "RegisteredOrganization" ) as string;
                sb.Append( '|' );
                sb.Append( str );
            }
            else
            {
                sb.Append( "||" );
            }
            str = sb.ToString();

            Rijndael encryptor = Encryption.GetLicenseEncryptor( MainFrm.ProdKey );
            byte[] vData = Encryption.EncryptString( encryptor, str );
            str = HexEncoding.ToString( vData );

            TxtMachineId.Text = str;

            if( !string.IsNullOrEmpty( MainFrm.TheAppWnd.LicenseInfo.LicName ) )
                LblRegName.Text = MainFrm.TheAppWnd.LicenseInfo.LicName;

            if( MainFrm.TheAppWnd.LicenseInfo.ExpiryDate > DateTime.MinValue )
                LblExpiryDate.Text = MainFrm.TheAppWnd.LicenseInfo.ExpiryDate.ToShortDateString();
        }

        private void LnkEmail_LinkClicked( object sender, LinkLabelLinkClickedEventArgs e )
        {
            string s = string.Format( @"mailto:{0}?subject=FileDb%20Machine%20ID&body={1}",
                LnkEmail.Tag.ToString(), TxtMachineId.Text );
            Process.Start( s );
        }
    }

}