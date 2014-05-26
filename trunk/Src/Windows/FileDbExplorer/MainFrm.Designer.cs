namespace FileDbExplorer
{
    partial class MainFrm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainFrm));
            this.TbMain = new System.Windows.Forms.ToolStrip();
            this.spacer1 = new System.Windows.Forms.ToolStripLabel();
            this.BtnOpenDb = new System.Windows.Forms.ToolStripButton();
            this.BtnNewQueryWnd = new System.Windows.Forms.ToolStripButton();
            this.BtnHelp = new System.Windows.Forms.ToolStripDropDownButton();
            this.MnuHelp = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.Tabs = new System.Windows.Forms.TabControl();
            this.page1 = new System.Windows.Forms.TabPage();
            this.Splitter = new System.Windows.Forms.Splitter();
            this.DbTree = new System.Windows.Forms.TreeView();
            this.ImgLstTreeNodes = new System.Windows.Forms.ImageList();
            this.CtxMnuDatabase = new System.Windows.Forms.ContextMenuStrip();
            this.MnuOpenTable = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuRefreshTable = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuCleanDb = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuReindexDb = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuSetEncryptionKey = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuAddField = new System.Windows.Forms.ToolStripMenuItem();
            this.sep1 = new System.Windows.Forms.ToolStripSeparator();
            this.MnuCloseDb = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolTips = new System.Windows.Forms.ToolTip();
            this.timer = new System.Windows.Forms.Timer();
            this.CtxMnuField = new System.Windows.Forms.ContextMenuStrip();
            this.MnuDeleteField = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuRenameField = new System.Windows.Forms.ToolStripMenuItem();
            this.queryCtrl1 = new FileDbExplorer.QueryCtrl();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.TbMain.SuspendLayout();
            this.Tabs.SuspendLayout();
            this.page1.SuspendLayout();
            this.CtxMnuDatabase.SuspendLayout();
            this.CtxMnuField.SuspendLayout();
            this.SuspendLayout();
            // 
            // TbMain
            // 
            this.TbMain.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.TbMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.spacer1,
            this.BtnOpenDb,
            this.BtnNewQueryWnd,
            this.BtnHelp});
            this.TbMain.Location = new System.Drawing.Point(0, 0);
            this.TbMain.Name = "TbMain";
            this.TbMain.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.TbMain.Size = new System.Drawing.Size(1034, 25);
            this.TbMain.TabIndex = 2;
            // 
            // spacer1
            // 
            this.spacer1.Enabled = false;
            this.spacer1.Name = "spacer1";
            this.spacer1.Size = new System.Drawing.Size(10, 22);
            this.spacer1.Text = " ";
            // 
            // BtnOpenDb
            // 
            this.BtnOpenDb.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.BtnOpenDb.Image = ((System.Drawing.Image)(resources.GetObject("BtnOpenDb.Image")));
            this.BtnOpenDb.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.BtnOpenDb.Name = "BtnOpenDb";
            this.BtnOpenDb.Size = new System.Drawing.Size(23, 22);
            this.BtnOpenDb.ToolTipText = "Open Database (Ctrl+O)";
            this.BtnOpenDb.Click += new System.EventHandler(this.BtnOpenDb_Click);
            // 
            // BtnNewQueryWnd
            // 
            this.BtnNewQueryWnd.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.BtnNewQueryWnd.Image = ((System.Drawing.Image)(resources.GetObject("BtnNewQueryWnd.Image")));
            this.BtnNewQueryWnd.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.BtnNewQueryWnd.Name = "BtnNewQueryWnd";
            this.BtnNewQueryWnd.Size = new System.Drawing.Size(23, 22);
            this.BtnNewQueryWnd.ToolTipText = "New SQL Window";
            this.BtnNewQueryWnd.Click += new System.EventHandler(this.BtnNewQueryWnd_Click);
            // 
            // BtnHelp
            // 
            this.BtnHelp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.BtnHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MnuHelp,
            this.MnuAbout});
            this.BtnHelp.Image = ((System.Drawing.Image)(resources.GetObject("BtnHelp.Image")));
            this.BtnHelp.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.BtnHelp.Name = "BtnHelp";
            this.BtnHelp.Size = new System.Drawing.Size(29, 22);
            this.BtnHelp.ToolTipText = "Help";
            // 
            // MnuHelp
            // 
            this.MnuHelp.Name = "MnuHelp";
            this.MnuHelp.Size = new System.Drawing.Size(107, 22);
            this.MnuHelp.Text = "Help";
            this.MnuHelp.Click += new System.EventHandler(this.MnuHelp_Click);
            // 
            // MnuAbout
            // 
            this.MnuAbout.Name = "MnuAbout";
            this.MnuAbout.Size = new System.Drawing.Size(107, 22);
            this.MnuAbout.Text = "About";
            this.MnuAbout.Click += new System.EventHandler(this.MnuAbout_Click);
            // 
            // Tabs
            // 
            this.Tabs.Controls.Add(this.page1);
            this.Tabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Tabs.Location = new System.Drawing.Point(216, 25);
            this.Tabs.Margin = new System.Windows.Forms.Padding(2);
            this.Tabs.Name = "Tabs";
            this.Tabs.SelectedIndex = 0;
            this.Tabs.Size = new System.Drawing.Size(818, 512);
            this.Tabs.TabIndex = 12;
            this.ToolTips.SetToolTip(this.Tabs, "right-click to close");
            this.Tabs.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Tabs_MouseClick);
            // 
            // page1
            // 
            this.page1.Controls.Add(this.queryCtrl1);
            this.page1.Location = new System.Drawing.Point(4, 22);
            this.page1.Margin = new System.Windows.Forms.Padding(2);
            this.page1.Name = "page1";
            this.page1.Padding = new System.Windows.Forms.Padding(2);
            this.page1.Size = new System.Drawing.Size(810, 486);
            this.page1.TabIndex = 0;
            this.page1.Text = "Query 1";
            this.page1.UseVisualStyleBackColor = true;
            // 
            // Splitter
            // 
            this.Splitter.Location = new System.Drawing.Point(212, 25);
            this.Splitter.Margin = new System.Windows.Forms.Padding(2);
            this.Splitter.Name = "Splitter";
            this.Splitter.Size = new System.Drawing.Size(4, 512);
            this.Splitter.TabIndex = 11;
            this.Splitter.TabStop = false;
            // 
            // DbTree
            // 
            this.DbTree.AllowDrop = true;
            this.DbTree.Dock = System.Windows.Forms.DockStyle.Left;
            this.DbTree.Font = new System.Drawing.Font("Tahoma", 9F);
            this.DbTree.HideSelection = false;
            this.DbTree.ImageIndex = 0;
            this.DbTree.ImageList = this.ImgLstTreeNodes;
            this.DbTree.Location = new System.Drawing.Point(0, 25);
            this.DbTree.Margin = new System.Windows.Forms.Padding(2);
            this.DbTree.Name = "DbTree";
            this.DbTree.SelectedImageIndex = 0;
            this.DbTree.ShowRootLines = false;
            this.DbTree.Size = new System.Drawing.Size(212, 512);
            this.DbTree.TabIndex = 10;
            this.DbTree.MouseClick += new System.Windows.Forms.MouseEventHandler(this.DbTree_MouseClick);
            // 
            // ImgLstTreeNodes
            // 
            this.ImgLstTreeNodes.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ImgLstTreeNodes.ImageStream")));
            this.ImgLstTreeNodes.TransparentColor = System.Drawing.Color.Transparent;
            this.ImgLstTreeNodes.Images.SetKeyName(0, "Folder.gif");
            this.ImgLstTreeNodes.Images.SetKeyName(1, "Db.gif");
            this.ImgLstTreeNodes.Images.SetKeyName(2, "Table.gif");
            this.ImgLstTreeNodes.Images.SetKeyName(3, "field.gif");
            // 
            // CtxMnuDatabase
            // 
            this.CtxMnuDatabase.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MnuOpenTable,
            this.MnuRefreshTable,
            this.MnuCleanDb,
            this.MnuReindexDb,
            this.MnuSetEncryptionKey,
            this.MnuAddField,
            this.sep1,
            this.MnuCloseDb});
            this.CtxMnuDatabase.Name = "CtxMnuTable";
            this.CtxMnuDatabase.Size = new System.Drawing.Size(182, 164);
            // 
            // MnuOpenTable
            // 
            this.MnuOpenTable.Name = "MnuOpenTable";
            this.MnuOpenTable.Size = new System.Drawing.Size(181, 22);
            this.MnuOpenTable.Text = "Open Table";
            this.MnuOpenTable.Click += new System.EventHandler(this.MnuOpenTable_Click);
            // 
            // MnuRefreshTable
            // 
            this.MnuRefreshTable.Name = "MnuRefreshTable";
            this.MnuRefreshTable.Size = new System.Drawing.Size(181, 22);
            this.MnuRefreshTable.Text = "Refresh";
            this.MnuRefreshTable.Click += new System.EventHandler(this.MnuRefreshTable_Click);
            // 
            // MnuCleanDb
            // 
            this.MnuCleanDb.Name = "MnuCleanDb";
            this.MnuCleanDb.Size = new System.Drawing.Size(181, 22);
            this.MnuCleanDb.Text = "Clean";
            this.MnuCleanDb.Click += new System.EventHandler(this.MnuCleanDb_Click);
            // 
            // MnuReindexDb
            // 
            this.MnuReindexDb.Name = "MnuReindexDb";
            this.MnuReindexDb.Size = new System.Drawing.Size(181, 22);
            this.MnuReindexDb.Text = "Reindex";
            this.MnuReindexDb.Click += new System.EventHandler(this.MnuReindexDb_Click);
            // 
            // MnuSetEncryptionKey
            // 
            this.MnuSetEncryptionKey.Name = "MnuSetEncryptionKey";
            this.MnuSetEncryptionKey.Size = new System.Drawing.Size(181, 22);
            this.MnuSetEncryptionKey.Text = "Set Encryption Key...";
            this.MnuSetEncryptionKey.Click += new System.EventHandler(this.MnuSetEncryptionKey_Click);
            // 
            // MnuAddField
            // 
            this.MnuAddField.Name = "MnuAddField";
            this.MnuAddField.Size = new System.Drawing.Size(181, 22);
            this.MnuAddField.Text = "Add Field";
            this.MnuAddField.Click += new System.EventHandler(this.MnuAddField_Click);
            // 
            // sep1
            // 
            this.sep1.Name = "sep1";
            this.sep1.Size = new System.Drawing.Size(178, 6);
            // 
            // MnuCloseDb
            // 
            this.MnuCloseDb.Name = "MnuCloseDb";
            this.MnuCloseDb.Size = new System.Drawing.Size(181, 22);
            this.MnuCloseDb.Text = "Close";
            this.MnuCloseDb.Click += new System.EventHandler(this.MnuCloseDb_Click);
            // 
            // timer
            // 
            this.timer.Tick += new System.EventHandler(this.timer_Tick);
            // 
            // CtxMnuField
            // 
            this.CtxMnuField.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MnuRenameField,
            this.toolStripSeparator1,
            this.MnuDeleteField});
            this.CtxMnuField.Name = "CtxMnuTable";
            this.CtxMnuField.Size = new System.Drawing.Size(118, 54);
            // 
            // MnuDeleteField
            // 
            this.MnuDeleteField.Name = "MnuDeleteField";
            this.MnuDeleteField.Size = new System.Drawing.Size(117, 22);
            this.MnuDeleteField.Text = "Delete";
            this.MnuDeleteField.Click += new System.EventHandler(this.MnuRemoveField_Click);
            // 
            // MnuRenameField
            // 
            this.MnuRenameField.Name = "MnuRenameField";
            this.MnuRenameField.Size = new System.Drawing.Size(117, 22);
            this.MnuRenameField.Text = "Rename";
            this.MnuRenameField.Click += new System.EventHandler(this.MnuRenameField_Click);
            // 
            // queryCtrl1
            // 
            this.queryCtrl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.queryCtrl1.Location = new System.Drawing.Point(2, 2);
            this.queryCtrl1.Margin = new System.Windows.Forms.Padding(4);
            this.queryCtrl1.Name = "queryCtrl1";
            this.queryCtrl1.Size = new System.Drawing.Size(806, 482);
            this.queryCtrl1.TabIndex = 0;
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(114, 6);
            // 
            // MainFrm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1034, 537);
            this.Controls.Add(this.Tabs);
            this.Controls.Add(this.Splitter);
            this.Controls.Add(this.DbTree);
            this.Controls.Add(this.TbMain);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "MainFrm";
            this.Text = "FileDb Explorer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainFrm_FormClosing);
            this.Load += new System.EventHandler(this.MainFrm_Load);
            this.TbMain.ResumeLayout(false);
            this.TbMain.PerformLayout();
            this.Tabs.ResumeLayout(false);
            this.page1.ResumeLayout(false);
            this.CtxMnuDatabase.ResumeLayout(false);
            this.CtxMnuField.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.ToolStrip TbMain;
        internal System.Windows.Forms.ToolStripLabel spacer1;
        internal System.Windows.Forms.ToolStripButton BtnNewQueryWnd;
        private System.Windows.Forms.ToolStripDropDownButton BtnHelp;
        private System.Windows.Forms.ToolStripMenuItem MnuHelp;
        private System.Windows.Forms.ToolStripMenuItem MnuAbout;
        private System.Windows.Forms.TabControl Tabs;
        private System.Windows.Forms.TabPage page1;
        private System.Windows.Forms.Splitter Splitter;
        internal System.Windows.Forms.TreeView DbTree;
        private System.Windows.Forms.ToolStripButton BtnOpenDb;
        private QueryCtrl queryCtrl1;
        internal System.Windows.Forms.ContextMenuStrip CtxMnuDatabase;
        internal System.Windows.Forms.ToolStripMenuItem MnuOpenTable;
        private System.Windows.Forms.ToolStripSeparator sep1;
        private System.Windows.Forms.ToolStripMenuItem MnuRefreshTable;
        private System.Windows.Forms.ToolTip ToolTips;
        private System.Windows.Forms.ToolStripMenuItem MnuCleanDb;
        private System.Windows.Forms.ToolStripMenuItem MnuCloseDb;
        private System.Windows.Forms.Timer timer;
        private System.Windows.Forms.ToolStripMenuItem MnuSetEncryptionKey;
        internal System.Windows.Forms.ContextMenuStrip CtxMnuField;
        private System.Windows.Forms.ToolStripMenuItem MnuDeleteField;
        private System.Windows.Forms.ImageList ImgLstTreeNodes;
        private System.Windows.Forms.ToolStripMenuItem MnuReindexDb;
        private System.Windows.Forms.ToolStripMenuItem MnuAddField;
        private System.Windows.Forms.ToolStripMenuItem MnuRenameField;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;

    }
}

