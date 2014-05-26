namespace FileDbExplorer
{
    partial class QueryCtrl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(QueryCtrl));
            this.CmdBar = new System.Windows.Forms.ToolStrip();
            this.spacer1 = new System.Windows.Forms.ToolStripLabel();
            this.BtnExecute = new System.Windows.Forms.ToolStripButton();
            this.BtnOpen = new System.Windows.Forms.ToolStripButton();
            this.BtnSave = new System.Windows.Forms.ToolStripButton();
            this.CodeEditor = new System.Windows.Forms.RichTextBox();
            this.Splitter = new System.Windows.Forms.Splitter();
            this.iGCellStyleDesign1 = new TenTec.Windows.iGridLib.iGCellStyleDesign();
            this.Grid = new TenTec.Windows.iGridLib.iGrid();
            this.CtxMnuGrid = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.MnuEditArrayValues = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuCopySelected = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuSaveBinaryData = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuImportBinaryData = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuCreateNewDb = new System.Windows.Forms.ToolStripMenuItem();
            this.CmdBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Grid)).BeginInit();
            this.CtxMnuGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // CmdBar
            // 
            this.CmdBar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.CmdBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.spacer1,
            this.BtnExecute,
            this.BtnOpen,
            this.BtnSave});
            this.CmdBar.Location = new System.Drawing.Point(0, 0);
            this.CmdBar.Name = "CmdBar";
            this.CmdBar.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.CmdBar.Size = new System.Drawing.Size(696, 25);
            this.CmdBar.TabIndex = 2;
            this.CmdBar.Text = "toolStrip1";
            // 
            // spacer1
            // 
            this.spacer1.Name = "spacer1";
            this.spacer1.Size = new System.Drawing.Size(13, 22);
            this.spacer1.Text = " ";
            // 
            // BtnExecute
            // 
            this.BtnExecute.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.BtnExecute.Image = ((System.Drawing.Image)(resources.GetObject("BtnExecute.Image")));
            this.BtnExecute.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.BtnExecute.Name = "BtnExecute";
            this.BtnExecute.Size = new System.Drawing.Size(23, 22);
            this.BtnExecute.ToolTipText = "Execute SQL (Ctrl+E)";
            this.BtnExecute.Click += new System.EventHandler(this.BtnExecute_Click);
            // 
            // BtnOpen
            // 
            this.BtnOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.BtnOpen.Image = ((System.Drawing.Image)(resources.GetObject("BtnOpen.Image")));
            this.BtnOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.BtnOpen.Name = "BtnOpen";
            this.BtnOpen.Size = new System.Drawing.Size(23, 22);
            this.BtnOpen.ToolTipText = "Open SQL file (Ctrl+O)";
            this.BtnOpen.Click += new System.EventHandler(this.BtnOpen_Click);
            // 
            // BtnSave
            // 
            this.BtnSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.BtnSave.Image = ((System.Drawing.Image)(resources.GetObject("BtnSave.Image")));
            this.BtnSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.BtnSave.Name = "BtnSave";
            this.BtnSave.Size = new System.Drawing.Size(23, 22);
            this.BtnSave.ToolTipText = "Save SQL (Ctrl+S)";
            this.BtnSave.Click += new System.EventHandler(this.BtnSave_Click);
            // 
            // CodeEditor
            // 
            this.CodeEditor.AcceptsTab = true;
            this.CodeEditor.DetectUrls = false;
            this.CodeEditor.Dock = System.Windows.Forms.DockStyle.Top;
            this.CodeEditor.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CodeEditor.Location = new System.Drawing.Point(0, 25);
            this.CodeEditor.Margin = new System.Windows.Forms.Padding(4);
            this.CodeEditor.Name = "CodeEditor";
            this.CodeEditor.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.CodeEditor.ShowSelectionMargin = true;
            this.CodeEditor.Size = new System.Drawing.Size(696, 94);
            this.CodeEditor.TabIndex = 3;
            this.CodeEditor.Text = "";
            // 
            // Splitter
            // 
            this.Splitter.Dock = System.Windows.Forms.DockStyle.Top;
            this.Splitter.Location = new System.Drawing.Point(0, 119);
            this.Splitter.Margin = new System.Windows.Forms.Padding(4);
            this.Splitter.Name = "Splitter";
            this.Splitter.Size = new System.Drawing.Size(696, 4);
            this.Splitter.TabIndex = 4;
            this.Splitter.TabStop = false;
            // 
            // Grid
            // 
            this.Grid.ContextMenuStrip = this.CtxMnuGrid;
            this.Grid.DefaultRow.Height = 20;
            this.Grid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Grid.FocusRectColor1 = System.Drawing.SystemColors.ControlDark;
            this.Grid.FocusRectColor2 = System.Drawing.SystemColors.ControlDark;
            this.Grid.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Grid.ForeColor = System.Drawing.SystemColors.WindowText;
            this.Grid.GroupBox.Visible = true;
            this.Grid.Header.Height = 23;
            this.Grid.Location = new System.Drawing.Point(0, 123);
            this.Grid.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Grid.Name = "Grid";
            this.Grid.RowHeader.Appearance = TenTec.Windows.iGridLib.iGControlPaintAppearance.StyleFlat;
            this.Grid.RowHeader.UseXPStyles = false;
            this.Grid.RowHeader.Visible = true;
            this.Grid.RowHeader.Width = 40;
            this.Grid.RowSelectionInCellMode = TenTec.Windows.iGridLib.iGRowSelectionInCellModeTypes.MultipleRows;
            this.Grid.ScrollGroupRows = true;
            this.Grid.SelCellsBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(183)))), ((int)(((byte)(219)))), ((int)(((byte)(255)))));
            this.Grid.SelCellsBackColorNoFocus = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.Grid.SelCellsForeColor = System.Drawing.Color.Black;
            this.Grid.SelCellsForeColorNoFocus = System.Drawing.Color.Black;
            this.Grid.SelectionMode = TenTec.Windows.iGridLib.iGSelectionMode.MultiExtended;
            this.Grid.ShowControlsInAllCells = false;
            this.Grid.Size = new System.Drawing.Size(696, 350);
            this.Grid.TabIndex = 5;
            this.Grid.TreeCol = null;
            this.Grid.TreeLines.Color = System.Drawing.SystemColors.WindowText;
            this.Grid.UIStrings.GroupBoxHintText = "Drag a column header here to group by that column.  Press Insert key to add new r" +
    "ows.";
            this.Grid.CurRowChanged += new System.EventHandler(this.Grid_CurRowChanged);
            this.Grid.CellMouseDown += new TenTec.Windows.iGridLib.iGCellMouseDownEventHandler(this.Grid_CellMouseDown);
            this.Grid.CellMouseUp += new TenTec.Windows.iGridLib.iGCellMouseUpEventHandler(this.Grid_CellMouseUp);
            this.Grid.RowHdrMouseDown += new TenTec.Windows.iGridLib.iGRowHdrMouseDownEventHandler(this.Grid_RowHdrMouseDown);
            this.Grid.RowHdrMouseUp += new TenTec.Windows.iGridLib.iGRowHdrMouseUpEventHandler(this.Grid_RowHdrMouseUp);
            this.Grid.CustomDrawRowHdrForeground += new TenTec.Windows.iGridLib.iGCustomDrawRowHdrForegroundEventHandler(this.Grid_CustomDrawRowHdrForeground);
            this.Grid.RequestEdit += new TenTec.Windows.iGridLib.iGRequestEditEventHandler(this.Grid_RequestEdit);
            this.Grid.BeforeCommitEdit += new TenTec.Windows.iGridLib.iGBeforeCommitEditEventHandler(this.Grid_BeforeCommitEdit);
            this.Grid.CancelEdit += new TenTec.Windows.iGridLib.iGCancelEditEventHandler(this.Grid_CancelEdit);
            this.Grid.CellDynamicFormatting += new TenTec.Windows.iGridLib.iGCellDynamicFormattingEventHandler(this.Grid_CellDynamicFormatting);
            this.Grid.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Grid_KeyDown);
            // 
            // CtxMnuGrid
            // 
            this.CtxMnuGrid.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MnuEditArrayValues,
            this.MnuCopySelected,
            this.MnuSaveBinaryData,
            this.MnuImportBinaryData,
            this.MnuCreateNewDb});
            this.CtxMnuGrid.Name = "CtxMnuGrid";
            this.CtxMnuGrid.Size = new System.Drawing.Size(275, 146);
            this.CtxMnuGrid.Opening += new System.ComponentModel.CancelEventHandler(this.CtxMnuGrid_Opening);
            // 
            // MnuEditArrayValues
            // 
            this.MnuEditArrayValues.Name = "MnuEditArrayValues";
            this.MnuEditArrayValues.Size = new System.Drawing.Size(274, 24);
            this.MnuEditArrayValues.Text = "Edit Array Values...";
            this.MnuEditArrayValues.Click += new System.EventHandler(this.MnuEditArrayValues_Click);
            // 
            // MnuCopySelected
            // 
            this.MnuCopySelected.Name = "MnuCopySelected";
            this.MnuCopySelected.Size = new System.Drawing.Size(274, 24);
            this.MnuCopySelected.Text = "Copy Selected";
            this.MnuCopySelected.Click += new System.EventHandler(this.MnuCopySelected_Click);
            // 
            // MnuSaveBinaryData
            // 
            this.MnuSaveBinaryData.Name = "MnuSaveBinaryData";
            this.MnuSaveBinaryData.Size = new System.Drawing.Size(274, 24);
            this.MnuSaveBinaryData.Text = "Save binary data to file...";
            this.MnuSaveBinaryData.Click += new System.EventHandler(this.MnuSaveBinaryData_Click);
            // 
            // MnuImportBinaryData
            // 
            this.MnuImportBinaryData.Name = "MnuImportBinaryData";
            this.MnuImportBinaryData.Size = new System.Drawing.Size(274, 24);
            this.MnuImportBinaryData.Text = "Import binary data from file...";
            this.MnuImportBinaryData.Click += new System.EventHandler(this.MnuImportBinaryData_Click);
            // 
            // MnuCreateNewDb
            // 
            this.MnuCreateNewDb.Name = "MnuCreateNewDb";
            this.MnuCreateNewDb.Size = new System.Drawing.Size(274, 24);
            this.MnuCreateNewDb.Text = "Create Database from Table...";
            this.MnuCreateNewDb.Click += new System.EventHandler(this.MnuCreateNewDb_Click);
            // 
            // QueryCtrl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.Grid);
            this.Controls.Add(this.Splitter);
            this.Controls.Add(this.CodeEditor);
            this.Controls.Add(this.CmdBar);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "QueryCtrl";
            this.Size = new System.Drawing.Size(696, 473);
            this.Load += new System.EventHandler(this.QueryCtrl_Load);
            this.CmdBar.ResumeLayout(false);
            this.CmdBar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Grid)).EndInit();
            this.CtxMnuGrid.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip CmdBar;
        private System.Windows.Forms.ToolStripLabel spacer1;
        private System.Windows.Forms.ToolStripButton BtnExecute;
        private System.Windows.Forms.ToolStripButton BtnOpen;
        private System.Windows.Forms.ToolStripButton BtnSave;
        private System.Windows.Forms.RichTextBox CodeEditor;
        private System.Windows.Forms.Splitter Splitter;
        private TenTec.Windows.iGridLib.iGCellStyleDesign iGCellStyleDesign1;
        private TenTec.Windows.iGridLib.iGrid Grid;
        private System.Windows.Forms.ContextMenuStrip CtxMnuGrid;
        private System.Windows.Forms.ToolStripMenuItem MnuEditArrayValues;
        private System.Windows.Forms.ToolStripMenuItem MnuCopySelected;
        private System.Windows.Forms.ToolStripMenuItem MnuSaveBinaryData;
        private System.Windows.Forms.ToolStripMenuItem MnuImportBinaryData;
        private System.Windows.Forms.ToolStripMenuItem MnuCreateNewDb;


    }
}
