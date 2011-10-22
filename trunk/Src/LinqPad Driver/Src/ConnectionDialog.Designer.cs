namespace FileDbDynamicDriverNs
{
    partial class ConnectionDialog
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
            this.label1 = new System.Windows.Forms.Label();
            this.TxtFolder = new System.Windows.Forms.TextBox();
            this.LstFiles = new System.Windows.Forms.ListView();
            this.ColName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.BtnCancel = new System.Windows.Forms.Button();
            this.BtnOK = new System.Windows.Forms.Button();
            this.BtnBrowse = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.TxtExtension = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(48, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Folder";
            // 
            // TxtFolder
            // 
            this.TxtFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TxtFolder.Location = new System.Drawing.Point(12, 39);
            this.TxtFolder.Name = "TxtFolder";
            this.TxtFolder.Size = new System.Drawing.Size(484, 22);
            this.TxtFolder.TabIndex = 1;
            this.TxtFolder.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TxtFolder_KeyDown);
            // 
            // LstFiles
            // 
            this.LstFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LstFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ColName,
            this.colSize});
            this.LstFiles.FullRowSelect = true;
            this.LstFiles.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.LstFiles.HideSelection = false;
            this.LstFiles.Location = new System.Drawing.Point(12, 83);
            this.LstFiles.Name = "LstFiles";
            this.LstFiles.Size = new System.Drawing.Size(561, 205);
            this.LstFiles.TabIndex = 3;
            this.LstFiles.UseCompatibleStateImageBehavior = false;
            this.LstFiles.View = System.Windows.Forms.View.Details;
            // 
            // ColName
            // 
            this.ColName.Text = "Name";
            this.ColName.Width = 311;
            // 
            // colSize
            // 
            this.colSize.Text = "Size";
            this.colSize.Width = 171;
            // 
            // BtnCancel
            // 
            this.BtnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.BtnCancel.Location = new System.Drawing.Point(489, 301);
            this.BtnCancel.Name = "BtnCancel";
            this.BtnCancel.Size = new System.Drawing.Size(84, 37);
            this.BtnCancel.TabIndex = 8;
            this.BtnCancel.Text = "Cancel";
            this.BtnCancel.UseVisualStyleBackColor = true;
            this.BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            // 
            // BtnOK
            // 
            this.BtnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnOK.Location = new System.Drawing.Point(388, 301);
            this.BtnOK.Name = "BtnOK";
            this.BtnOK.Size = new System.Drawing.Size(84, 37);
            this.BtnOK.TabIndex = 7;
            this.BtnOK.Text = "OK";
            this.BtnOK.UseVisualStyleBackColor = true;
            this.BtnOK.Click += new System.EventHandler(this.BtnOK_Click);
            // 
            // BtnBrowse
            // 
            this.BtnBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnBrowse.Location = new System.Drawing.Point(502, 37);
            this.BtnBrowse.Name = "BtnBrowse";
            this.BtnBrowse.Size = new System.Drawing.Size(71, 24);
            this.BtnBrowse.TabIndex = 2;
            this.BtnBrowse.Text = "Browse";
            this.BtnBrowse.UseVisualStyleBackColor = true;
            this.BtnBrowse.Click += new System.EventHandler(this.BtnBrowse_Click);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 296);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(153, 17);
            this.label2.TabIndex = 4;
            this.label2.Text = "File Extension (eg. fdb)";
            // 
            // TxtExtension
            // 
            this.TxtExtension.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.TxtExtension.Location = new System.Drawing.Point(12, 316);
            this.TxtExtension.Name = "TxtExtension";
            this.TxtExtension.Size = new System.Drawing.Size(98, 22);
            this.TxtExtension.TabIndex = 5;
            this.TxtExtension.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TxtExtension_KeyDown);
            this.TxtExtension.Leave += new System.EventHandler(this.TxtExtension_Leave);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(116, 320);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(149, 15);
            this.label3.TabIndex = 6;
            this.label3.Text = "(press Enter to refresh list)";
            // 
            // ConnectionDialog
            // 
            this.AcceptButton = this.BtnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(585, 347);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.TxtExtension);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.BtnBrowse);
            this.Controls.Add(this.BtnOK);
            this.Controls.Add(this.BtnCancel);
            this.Controls.Add(this.LstFiles);
            this.Controls.Add(this.TxtFolder);
            this.Controls.Add(this.label1);
            this.Name = "ConnectionDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FileDb Connection Dialog";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox TxtFolder;
        private System.Windows.Forms.ListView LstFiles;
        private System.Windows.Forms.Button BtnCancel;
        private System.Windows.Forms.Button BtnOK;
        private System.Windows.Forms.Button BtnBrowse;
        private System.Windows.Forms.ColumnHeader ColName;
        private System.Windows.Forms.ColumnHeader colSize;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox TxtExtension;
        private System.Windows.Forms.Label label3;
    }
}