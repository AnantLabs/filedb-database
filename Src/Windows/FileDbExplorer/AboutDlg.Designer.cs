namespace FileDbExplorer
{
    partial class AboutDlg
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose( bool disposing )
        {
            if( disposing && (components != null) )
            {
                components.Dispose();
            }
            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnClose = new System.Windows.Forms.Button();
            this.l1 = new System.Windows.Forms.Label();
            this.l2 = new System.Windows.Forms.Label();
            this.LblVer = new System.Windows.Forms.Label();
            this.l4 = new System.Windows.Forms.Label();
            this.LnkWeb = new System.Windows.Forms.LinkLabel();
            this.LnkSupport = new System.Windows.Forms.LinkLabel();
            this.BtnLicenseDetails = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.iGrid1DefaultCellStyle = new TenTec.Windows.iGridLib.iGCellStyle(true);
            this.iGrid1DefaultColHdrStyle = new TenTec.Windows.iGridLib.iGColHdrStyle(true);
            this.iGrid1RowTextColCellStyle = new TenTec.Windows.iGridLib.iGCellStyle(true);
            this.SuspendLayout();
            // 
            // btnClose
            // 
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnClose.Location = new System.Drawing.Point(164, 198);
            this.btnClose.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(86, 46);
            this.btnClose.TabIndex = 0;
            this.btnClose.Text = "OK";
            this.btnClose.UseVisualStyleBackColor = true;
            // 
            // l1
            // 
            this.l1.AutoSize = true;
            this.l1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.l1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(94)))), ((int)(((byte)(230)))));
            this.l1.Location = new System.Drawing.Point(45, 15);
            this.l1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.l1.Name = "l1";
            this.l1.Size = new System.Drawing.Size(157, 24);
            this.l1.TabIndex = 1;
            this.l1.Text = "FileDb Explorer";
            // 
            // l2
            // 
            this.l2.AutoSize = true;
            this.l2.Font = new System.Drawing.Font("Tahoma", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.l2.ForeColor = System.Drawing.Color.Gray;
            this.l2.Location = new System.Drawing.Point(80, 49);
            this.l2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.l2.Name = "l2";
            this.l2.Size = new System.Drawing.Size(60, 17);
            this.l2.TabIndex = 2;
            this.l2.Text = "Version";
            this.l2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // LblVer
            // 
            this.LblVer.AutoSize = true;
            this.LblVer.Font = new System.Drawing.Font("Tahoma", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LblVer.ForeColor = System.Drawing.Color.Gray;
            this.LblVer.Location = new System.Drawing.Point(136, 49);
            this.LblVer.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.LblVer.Name = "LblVer";
            this.LblVer.Size = new System.Drawing.Size(38, 17);
            this.LblVer.TabIndex = 3;
            this.LblVer.Text = "XXX";
            this.LblVer.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // l4
            // 
            this.l4.AutoSize = true;
            this.l4.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.l4.Location = new System.Drawing.Point(9, 77);
            this.l4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.l4.Name = "l4";
            this.l4.Size = new System.Drawing.Size(200, 17);
            this.l4.TabIndex = 4;
            this.l4.Text = "Copyright � EzTools Software.";
            // 
            // LnkWeb
            // 
            this.LnkWeb.AutoSize = true;
            this.LnkWeb.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LnkWeb.Location = new System.Drawing.Point(107, 129);
            this.LnkWeb.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.LnkWeb.Name = "LnkWeb";
            this.LnkWeb.Size = new System.Drawing.Size(153, 15);
            this.LnkWeb.TabIndex = 5;
            this.LnkWeb.TabStop = true;
            this.LnkWeb.Text = "www.eztools-software.com";
            this.LnkWeb.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LnkWeb_LinkClicked);
            // 
            // LnkSupport
            // 
            this.LnkSupport.AutoSize = true;
            this.LnkSupport.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LnkSupport.Location = new System.Drawing.Point(86, 155);
            this.LnkSupport.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.LnkSupport.Name = "LnkSupport";
            this.LnkSupport.Size = new System.Drawing.Size(176, 15);
            this.LnkSupport.TabIndex = 6;
            this.LnkSupport.TabStop = true;
            this.LnkSupport.Text = "support@eztools-software.com";
            this.LnkSupport.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LnkSupport_LinkClicked);
            // 
            // BtnLicenseDetails
            // 
            this.BtnLicenseDetails.Location = new System.Drawing.Point(12, 206);
            this.BtnLicenseDetails.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.BtnLicenseDetails.Name = "BtnLicenseDetails";
            this.BtnLicenseDetails.Size = new System.Drawing.Size(87, 29);
            this.BtnLicenseDetails.TabIndex = 7;
            this.BtnLicenseDetails.Text = "Licensing";
            this.BtnLicenseDetails.UseVisualStyleBackColor = true;
            this.BtnLicenseDetails.Click += new System.EventHandler(this.BtnLicenseDetails_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(9, 98);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(126, 17);
            this.label1.TabIndex = 8;
            this.label1.Text = "All rights reserved.";
            // 
            // AboutDlg
            // 
            this.AcceptButton = this.btnClose;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(260, 255);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.BtnLicenseDetails);
            this.Controls.Add(this.LnkSupport);
            this.Controls.Add(this.LnkWeb);
            this.Controls.Add(this.l4);
            this.Controls.Add(this.LblVer);
            this.Controls.Add(this.l2);
            this.Controls.Add(this.l1);
            this.Controls.Add(this.btnClose);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutDlg";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "About FileDb Explorer";
            this.Load += new System.EventHandler(this.AboutDlg_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label l1;
        private System.Windows.Forms.Label l2;
        private System.Windows.Forms.Label LblVer;
        private System.Windows.Forms.Label l4;
        private System.Windows.Forms.LinkLabel LnkWeb;
        private System.Windows.Forms.LinkLabel LnkSupport;
        private System.Windows.Forms.Button BtnLicenseDetails;
        private System.Windows.Forms.Label label1;
        private TenTec.Windows.iGridLib.iGCellStyle iGrid1DefaultCellStyle;
        private TenTec.Windows.iGridLib.iGColHdrStyle iGrid1DefaultColHdrStyle;
        private TenTec.Windows.iGridLib.iGCellStyle iGrid1RowTextColCellStyle;
    }
}