namespace FileDbExplorer
{
    partial class LicenseInfoDlg
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
            this.l1 = new System.Windows.Forms.Label();
            this.TxtMachineId = new System.Windows.Forms.TextBox();
            this.LnkEmail = new System.Windows.Forms.LinkLabel();
            this.btnClose = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.LblRegName = new System.Windows.Forms.Label();
            this.LblExpiryDate = new System.Windows.Forms.Label();
            this.iGrid1DefaultCellStyle1 = new TenTec.Windows.iGridLib.iGCellStyle( true );
            this.iGrid1DefaultColHdrStyle1 = new TenTec.Windows.iGridLib.iGColHdrStyle( true );
            this.iGrid1RowTextColCellStyle1 = new TenTec.Windows.iGridLib.iGCellStyle( true );
            this.SuspendLayout();
            // 
            // l1
            // 
            this.l1.AutoSize = true;
            this.l1.Font = new System.Drawing.Font( "Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)) );
            this.l1.Location = new System.Drawing.Point( 9, 14 );
            this.l1.Name = "l1";
            this.l1.Size = new System.Drawing.Size( 82, 18 );
            this.l1.TabIndex = 0;
            this.l1.Text = "Machine ID";
            // 
            // TxtMachineId
            // 
            this.TxtMachineId.Location = new System.Drawing.Point( 12, 35 );
            this.TxtMachineId.Multiline = true;
            this.TxtMachineId.Name = "TxtMachineId";
            this.TxtMachineId.ReadOnly = true;
            this.TxtMachineId.Size = new System.Drawing.Size( 424, 102 );
            this.TxtMachineId.TabIndex = 1;
            // 
            // LnkEmail
            // 
            this.LnkEmail.AutoSize = true;
            this.LnkEmail.Font = new System.Drawing.Font( "Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)) );
            this.LnkEmail.Location = new System.Drawing.Point( 313, 14 );
            this.LnkEmail.Name = "LnkEmail";
            this.LnkEmail.Size = new System.Drawing.Size( 123, 18 );
            this.LnkEmail.TabIndex = 7;
            this.LnkEmail.TabStop = true;
            this.LnkEmail.Tag = "license@eztools-software.com";
            this.LnkEmail.Text = "Email Machine ID";
            this.LnkEmail.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler( this.LnkEmail_LinkClicked );
            // 
            // btnClose
            // 
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnClose.Location = new System.Drawing.Point( 321, 247 );
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size( 115, 56 );
            this.btnClose.TabIndex = 8;
            this.btnClose.Text = "OK";
            this.btnClose.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font( "Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)) );
            this.label1.Location = new System.Drawing.Point( 45, 167 );
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size( 105, 18 );
            this.label1.TabIndex = 9;
            this.label1.Text = "Registered To:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font( "Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)) );
            this.label2.Location = new System.Drawing.Point( 34, 208 );
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size( 116, 18 );
            this.label2.TabIndex = 11;
            this.label2.Text = "License Expires:";
            // 
            // LblRegName
            // 
            this.LblRegName.Font = new System.Drawing.Font( "Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte) (0)) );
            this.LblRegName.ForeColor = System.Drawing.Color.FromArgb( ((int) (((byte) (64)))), ((int) (((byte) (64)))), ((int) (((byte) (64)))) );
            this.LblRegName.Location = new System.Drawing.Point( 152, 167 );
            this.LblRegName.Name = "LblRegName";
            this.LblRegName.Size = new System.Drawing.Size( 284, 20 );
            this.LblRegName.TabIndex = 13;
            this.LblRegName.Text = "*** Unregistered ***";
            // 
            // LblExpiryDate
            // 
            this.LblExpiryDate.Font = new System.Drawing.Font( "Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte) (0)) );
            this.LblExpiryDate.ForeColor = System.Drawing.Color.FromArgb( ((int) (((byte) (64)))), ((int) (((byte) (64)))), ((int) (((byte) (64)))) );
            this.LblExpiryDate.Location = new System.Drawing.Point( 155, 208 );
            this.LblExpiryDate.Name = "LblExpiryDate";
            this.LblExpiryDate.Size = new System.Drawing.Size( 187, 20 );
            this.LblExpiryDate.TabIndex = 14;
            this.LblExpiryDate.Text = "N/A";
            // 
            // LicenseInfoDlg
            // 
            this.AcceptButton = this.btnClose;
            this.AutoScaleDimensions = new System.Drawing.SizeF( 9F, 18F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size( 448, 315 );
            this.Controls.Add( this.LblExpiryDate );
            this.Controls.Add( this.LblRegName );
            this.Controls.Add( this.label2 );
            this.Controls.Add( this.label1 );
            this.Controls.Add( this.btnClose );
            this.Controls.Add( this.LnkEmail );
            this.Controls.Add( this.TxtMachineId );
            this.Controls.Add( this.l1 );
            this.Font = new System.Drawing.Font( "Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)) );
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LicenseInfoDlg";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Licensing Info";
            this.Load += new System.EventHandler( this.LicenseInfoDlg_Load );
            this.ResumeLayout( false );
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label l1;
        private System.Windows.Forms.TextBox TxtMachineId;
        private System.Windows.Forms.LinkLabel LnkEmail;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label LblRegName;
        private System.Windows.Forms.Label LblExpiryDate;
        private TenTec.Windows.iGridLib.iGCellStyle iGrid1DefaultCellStyle1;
        private TenTec.Windows.iGridLib.iGColHdrStyle iGrid1DefaultColHdrStyle1;
        private TenTec.Windows.iGridLib.iGCellStyle iGrid1RowTextColCellStyle1;
    }
}