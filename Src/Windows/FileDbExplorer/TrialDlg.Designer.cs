namespace FileDbExplorer
{
    partial class TrialDlg
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
            this.BtnClose = new System.Windows.Forms.Button();
            this.LblMsg = new System.Windows.Forms.Label();
            this.LnkPurchase = new System.Windows.Forms.LinkLabel();
            this.TxtTrialMsg = new System.Windows.Forms.TextBox();
            this.BtnLicensing = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // BtnClose
            // 
            this.BtnClose.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.BtnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.BtnClose.Location = new System.Drawing.Point(345, 421);
            this.BtnClose.Name = "BtnClose";
            this.BtnClose.Size = new System.Drawing.Size(117, 55);
            this.BtnClose.TabIndex = 0;
            this.BtnClose.Text = "Close";
            this.BtnClose.UseVisualStyleBackColor = true;
            // 
            // LblMsg
            // 
            this.LblMsg.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LblMsg.ForeColor = System.Drawing.Color.Maroon;
            this.LblMsg.Location = new System.Drawing.Point(12, 9);
            this.LblMsg.Name = "LblMsg";
            this.LblMsg.Size = new System.Drawing.Size(463, 53);
            this.LblMsg.TabIndex = 2;
            this.LblMsg.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // LnkPurchase
            // 
            this.LnkPurchase.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.LnkPurchase.AutoSize = true;
            this.LnkPurchase.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LnkPurchase.LinkColor = System.Drawing.Color.Green;
            this.LnkPurchase.Location = new System.Drawing.Point(22, 433);
            this.LnkPurchase.Name = "LnkPurchase";
            this.LnkPurchase.Size = new System.Drawing.Size(166, 29);
            this.LnkPurchase.TabIndex = 6;
            this.LnkPurchase.TabStop = true;
            this.LnkPurchase.Tag = "";
            this.LnkPurchase.Text = "** Purchase **";
            this.LnkPurchase.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LnkPurchase_LinkClicked);
            // 
            // TxtTrialMsg
            // 
            this.TxtTrialMsg.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.TxtTrialMsg.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TxtTrialMsg.Location = new System.Drawing.Point(16, 65);
            this.TxtTrialMsg.Multiline = true;
            this.TxtTrialMsg.Name = "TxtTrialMsg";
            this.TxtTrialMsg.ReadOnly = true;
            this.TxtTrialMsg.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.TxtTrialMsg.Size = new System.Drawing.Size(459, 338);
            this.TxtTrialMsg.TabIndex = 8;
            // 
            // BtnLicensing
            // 
            this.BtnLicensing.Location = new System.Drawing.Point(206, 433);
            this.BtnLicensing.Name = "BtnLicensing";
            this.BtnLicensing.Size = new System.Drawing.Size(123, 31);
            this.BtnLicensing.TabIndex = 10;
            this.BtnLicensing.Text = "Licensing";
            this.BtnLicensing.UseVisualStyleBackColor = true;
            this.BtnLicensing.Click += new System.EventHandler(this.BtnLicensing_Click);
            // 
            // TrialDlg
            // 
            this.AcceptButton = this.BtnClose;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.BtnClose;
            this.ClientSize = new System.Drawing.Size(487, 488);
            this.Controls.Add(this.BtnLicensing);
            this.Controls.Add(this.TxtTrialMsg);
            this.Controls.Add(this.LnkPurchase);
            this.Controls.Add(this.LblMsg);
            this.Controls.Add(this.BtnClose);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TrialDlg";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "FileDb Explorer Evaluation";
            this.Load += new System.EventHandler(this.TrialDlg_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button BtnClose;
        private System.Windows.Forms.Label LblMsg;
        private System.Windows.Forms.LinkLabel LnkPurchase;
        private System.Windows.Forms.TextBox TxtTrialMsg;
        private System.Windows.Forms.Button BtnLicensing;
    }
}