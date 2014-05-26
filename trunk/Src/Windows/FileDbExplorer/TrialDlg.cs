using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Reflection;
using System.IO;


namespace FileDbExplorer
{
    public partial class TrialDlg : Form
    {
        public TrialDlg( string sMsg )
        {
            InitializeComponent();
            LblMsg.Text = sMsg;
        }

        private void TrialDlg_Load( object sender, EventArgs e )
        {
            MainFrm.TheAppWnd.SetNagged();
            
            TxtTrialMsg.Text = Properties.Resources.EvalMsg;
            LnkPurchase.Tag = "http://order.kagi.com?2FE";
        }

        private void LnkPurchase_LinkClicked( object sender, LinkLabelLinkClickedEventArgs e )
        {
            Process.Start( (string) LnkPurchase.Tag );
        }

        private void BtnLicensing_Click( object sender, EventArgs e )
        {
            LicenseInfoDlg licenseDlg = new LicenseInfoDlg( MainFrm.Version );
            licenseDlg.ShowDialog( this );
        }
    }
}