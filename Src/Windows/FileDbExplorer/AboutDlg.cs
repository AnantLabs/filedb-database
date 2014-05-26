using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Reflection;


namespace FileDbExplorer
{
    public partial class AboutDlg : Form
    {
        public AboutDlg()
        {
            InitializeComponent();
        }

        private void AboutDlg_Load( object sender, EventArgs e )
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            AssemblyName asmName = asm.GetName();
            LblVer.Text = asmName.Version.ToString();
        }

        private void LnkWeb_LinkClicked( object sender, LinkLabelLinkClickedEventArgs e )
        {
            Process.Start( "http://" + LnkWeb.Text );
        }

        private void LnkSupport_LinkClicked( object sender, LinkLabelLinkClickedEventArgs e )
        {
            Process.Start( "mailto:" + LnkSupport.Text );
        }

        private void BtnLicenseDetails_Click( object sender, EventArgs e )
        {
            LicenseInfoDlg licenseDlg = new LicenseInfoDlg( LblVer.Text );
            licenseDlg.ShowDialog( this );
        }
    }
}