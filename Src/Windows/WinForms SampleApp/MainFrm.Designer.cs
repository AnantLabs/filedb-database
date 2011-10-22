namespace SampleApp
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
            this.BtnCreate = new System.Windows.Forms.Button();
            this.BtnClose = new System.Windows.Forms.Button();
            this.BtnAddRecords = new System.Windows.Forms.Button();
            this.btnOpen = new System.Windows.Forms.Button();
            this.BtnOpenEncrypted = new System.Windows.Forms.Button();
            this.BtnCloseDb = new System.Windows.Forms.Button();
            this.BtnDrop = new System.Windows.Forms.Button();
            this.BtnGetRecordByKey = new System.Windows.Forms.Button();
            this.BtnGetRecordByIndex = new System.Windows.Forms.Button();
            this.BtnGetMatchingRecords = new System.Windows.Forms.Button();
            this.BtnGetRecordsRegex = new System.Windows.Forms.Button();
            this.BtnGetAllRecordsReverseSort = new System.Windows.Forms.Button();
            this.BtnGetAllRecordsSorted = new System.Windows.Forms.Button();
            this.grid = new System.Windows.Forms.DataGridView();
            this.BtnUpdateMatchingRecord = new System.Windows.Forms.Button();
            this.BtnUpdateMatchingRecords = new System.Windows.Forms.Button();
            this.BtnUpdateRecordsRegex = new System.Windows.Forms.Button();
            this.BtnEncryptDecryptValue = new System.Windows.Forms.Button();
            this.BtnTableToDB = new System.Windows.Forms.Button();
            this.BtnRemoveByKey = new System.Windows.Forms.Button();
            this.BtnRemoveByIndex = new System.Windows.Forms.Button();
            this.BtnRemoveByValue1 = new System.Windows.Forms.Button();
            this.BtnRemoveByValue2 = new System.Windows.Forms.Button();
            this.BtnReindex = new System.Windows.Forms.Button();
            this.BtnIterate = new System.Windows.Forms.Button();
            this.BtnNumDeleted = new System.Windows.Forms.Button();
            this.BtnNumRecords = new System.Windows.Forms.Button();
            this.BtnClean = new System.Windows.Forms.Button();
            this.BtnLinqAggregates = new System.Windows.Forms.Button();
            this.BtnLinqHierarchicalObjects = new System.Windows.Forms.Button();
            this.BtnLinqGroupBy = new System.Windows.Forms.Button();
            this.BtnLinqJoin = new System.Windows.Forms.Button();
            this.BtnLinqSelect = new System.Windows.Forms.Button();
            this.BtnTableFromTable = new System.Windows.Forms.Button();
            this.BtnMetaData = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize) (this.grid)).BeginInit();
            this.SuspendLayout();
            // 
            // BtnCreate
            // 
            this.BtnCreate.Location = new System.Drawing.Point( 13, 16 );
            this.BtnCreate.Margin = new System.Windows.Forms.Padding( 4 );
            this.BtnCreate.Name = "BtnCreate";
            this.BtnCreate.Size = new System.Drawing.Size( 161, 36 );
            this.BtnCreate.TabIndex = 0;
            this.BtnCreate.Text = "Create";
            this.BtnCreate.UseVisualStyleBackColor = true;
            this.BtnCreate.Click += new System.EventHandler( this.BtnCreate_Click );
            // 
            // BtnClose
            // 
            this.BtnClose.Location = new System.Drawing.Point( 958, 253 );
            this.BtnClose.Margin = new System.Windows.Forms.Padding( 4 );
            this.BtnClose.Name = "BtnClose";
            this.BtnClose.Size = new System.Drawing.Size( 175, 55 );
            this.BtnClose.TabIndex = 1;
            this.BtnClose.Text = "Close";
            this.BtnClose.UseVisualStyleBackColor = true;
            this.BtnClose.Click += new System.EventHandler( this.BtnClose_Click );
            // 
            // BtnAddRecords
            // 
            this.BtnAddRecords.Location = new System.Drawing.Point( 13, 60 );
            this.BtnAddRecords.Margin = new System.Windows.Forms.Padding( 4 );
            this.BtnAddRecords.Name = "BtnAddRecords";
            this.BtnAddRecords.Size = new System.Drawing.Size( 161, 36 );
            this.BtnAddRecords.TabIndex = 2;
            this.BtnAddRecords.Text = "Add Records";
            this.BtnAddRecords.UseVisualStyleBackColor = true;
            this.BtnAddRecords.Click += new System.EventHandler( this.BtnAddRecords_Click );
            // 
            // btnOpen
            // 
            this.btnOpen.Location = new System.Drawing.Point( 13, 121 );
            this.btnOpen.Margin = new System.Windows.Forms.Padding( 4 );
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size( 161, 36 );
            this.btnOpen.TabIndex = 3;
            this.btnOpen.Text = "Open";
            this.btnOpen.UseVisualStyleBackColor = true;
            this.btnOpen.Click += new System.EventHandler( this.btnOpen_Click );
            // 
            // BtnOpenEncrypted
            // 
            this.BtnOpenEncrypted.Location = new System.Drawing.Point( 13, 165 );
            this.BtnOpenEncrypted.Margin = new System.Windows.Forms.Padding( 4 );
            this.BtnOpenEncrypted.Name = "BtnOpenEncrypted";
            this.BtnOpenEncrypted.Size = new System.Drawing.Size( 161, 36 );
            this.BtnOpenEncrypted.TabIndex = 4;
            this.BtnOpenEncrypted.Text = "Open w/Encryption";
            this.BtnOpenEncrypted.UseVisualStyleBackColor = true;
            this.BtnOpenEncrypted.Click += new System.EventHandler( this.BtnOpenEncrypted_Click );
            // 
            // BtnCloseDb
            // 
            this.BtnCloseDb.Location = new System.Drawing.Point( 13, 209 );
            this.BtnCloseDb.Margin = new System.Windows.Forms.Padding( 4 );
            this.BtnCloseDb.Name = "BtnCloseDb";
            this.BtnCloseDb.Size = new System.Drawing.Size( 161, 36 );
            this.BtnCloseDb.TabIndex = 5;
            this.BtnCloseDb.Text = "Close";
            this.BtnCloseDb.UseVisualStyleBackColor = true;
            this.BtnCloseDb.Click += new System.EventHandler( this.BtnCloseDb_Click );
            // 
            // BtnDrop
            // 
            this.BtnDrop.Location = new System.Drawing.Point( 13, 253 );
            this.BtnDrop.Margin = new System.Windows.Forms.Padding( 4 );
            this.BtnDrop.Name = "BtnDrop";
            this.BtnDrop.Size = new System.Drawing.Size( 161, 36 );
            this.BtnDrop.TabIndex = 6;
            this.BtnDrop.Text = "Drop";
            this.BtnDrop.UseVisualStyleBackColor = true;
            this.BtnDrop.Click += new System.EventHandler( this.BtnDrop_Click );
            // 
            // BtnGetRecordByKey
            // 
            this.BtnGetRecordByKey.Location = new System.Drawing.Point( 194, 253 );
            this.BtnGetRecordByKey.Margin = new System.Windows.Forms.Padding( 4 );
            this.BtnGetRecordByKey.Name = "BtnGetRecordByKey";
            this.BtnGetRecordByKey.Size = new System.Drawing.Size( 161, 36 );
            this.BtnGetRecordByKey.TabIndex = 12;
            this.BtnGetRecordByKey.Text = "Get record by key";
            this.BtnGetRecordByKey.UseVisualStyleBackColor = true;
            this.BtnGetRecordByKey.Click += new System.EventHandler( this.BtnGetRecordByKey_Click );
            // 
            // BtnGetRecordByIndex
            // 
            this.BtnGetRecordByIndex.Location = new System.Drawing.Point( 194, 209 );
            this.BtnGetRecordByIndex.Margin = new System.Windows.Forms.Padding( 4 );
            this.BtnGetRecordByIndex.Name = "BtnGetRecordByIndex";
            this.BtnGetRecordByIndex.Size = new System.Drawing.Size( 161, 36 );
            this.BtnGetRecordByIndex.TabIndex = 11;
            this.BtnGetRecordByIndex.Text = "Get record by index";
            this.BtnGetRecordByIndex.UseVisualStyleBackColor = true;
            this.BtnGetRecordByIndex.Click += new System.EventHandler( this.BtnGetRecordByIndex_Click );
            // 
            // BtnGetMatchingRecords
            // 
            this.BtnGetMatchingRecords.Location = new System.Drawing.Point( 194, 165 );
            this.BtnGetMatchingRecords.Margin = new System.Windows.Forms.Padding( 4 );
            this.BtnGetMatchingRecords.Name = "BtnGetMatchingRecords";
            this.BtnGetMatchingRecords.Size = new System.Drawing.Size( 161, 36 );
            this.BtnGetMatchingRecords.TabIndex = 10;
            this.BtnGetMatchingRecords.Text = "Get matching records";
            this.BtnGetMatchingRecords.UseVisualStyleBackColor = true;
            this.BtnGetMatchingRecords.Click += new System.EventHandler( this.BtnGetMatchingRecords_Click );
            // 
            // BtnGetRecordsRegex
            // 
            this.BtnGetRecordsRegex.Location = new System.Drawing.Point( 194, 121 );
            this.BtnGetRecordsRegex.Margin = new System.Windows.Forms.Padding( 4 );
            this.BtnGetRecordsRegex.Name = "BtnGetRecordsRegex";
            this.BtnGetRecordsRegex.Size = new System.Drawing.Size( 161, 36 );
            this.BtnGetRecordsRegex.TabIndex = 9;
            this.BtnGetRecordsRegex.Text = "Get records Regex";
            this.BtnGetRecordsRegex.UseVisualStyleBackColor = true;
            this.BtnGetRecordsRegex.Click += new System.EventHandler( this.BtnGetRecordsRegex_Click );
            // 
            // BtnGetAllRecordsReverseSort
            // 
            this.BtnGetAllRecordsReverseSort.Location = new System.Drawing.Point( 194, 60 );
            this.BtnGetAllRecordsReverseSort.Margin = new System.Windows.Forms.Padding( 4 );
            this.BtnGetAllRecordsReverseSort.Name = "BtnGetAllRecordsReverseSort";
            this.BtnGetAllRecordsReverseSort.Size = new System.Drawing.Size( 161, 54 );
            this.BtnGetAllRecordsReverseSort.TabIndex = 8;
            this.BtnGetAllRecordsReverseSort.Text = "Get all records reverse sort";
            this.BtnGetAllRecordsReverseSort.UseVisualStyleBackColor = true;
            this.BtnGetAllRecordsReverseSort.Click += new System.EventHandler( this.BtnGetAllRecordsReverseSort_Click );
            // 
            // BtnGetAllRecordsSorted
            // 
            this.BtnGetAllRecordsSorted.Location = new System.Drawing.Point( 194, 16 );
            this.BtnGetAllRecordsSorted.Margin = new System.Windows.Forms.Padding( 4 );
            this.BtnGetAllRecordsSorted.Name = "BtnGetAllRecordsSorted";
            this.BtnGetAllRecordsSorted.Size = new System.Drawing.Size( 161, 36 );
            this.BtnGetAllRecordsSorted.TabIndex = 7;
            this.BtnGetAllRecordsSorted.Text = "Get all records sorted";
            this.BtnGetAllRecordsSorted.UseVisualStyleBackColor = true;
            this.BtnGetAllRecordsSorted.Click += new System.EventHandler( this.BtnGetAllRecordsSorted_Click );
            // 
            // grid
            // 
            this.grid.Anchor = ((System.Windows.Forms.AnchorStyles) ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grid.Location = new System.Drawing.Point( 13, 316 );
            this.grid.Margin = new System.Windows.Forms.Padding( 4 );
            this.grid.Name = "grid";
            this.grid.ReadOnly = true;
            this.grid.RowTemplate.Height = 24;
            this.grid.Size = new System.Drawing.Size( 1139, 216 );
            this.grid.TabIndex = 13;
            // 
            // BtnUpdateMatchingRecord
            // 
            this.BtnUpdateMatchingRecord.Location = new System.Drawing.Point( 373, 16 );
            this.BtnUpdateMatchingRecord.Margin = new System.Windows.Forms.Padding( 4 );
            this.BtnUpdateMatchingRecord.Name = "BtnUpdateMatchingRecord";
            this.BtnUpdateMatchingRecord.Size = new System.Drawing.Size( 175, 36 );
            this.BtnUpdateMatchingRecord.TabIndex = 14;
            this.BtnUpdateMatchingRecord.Text = "Update matching record";
            this.BtnUpdateMatchingRecord.UseVisualStyleBackColor = true;
            this.BtnUpdateMatchingRecord.Click += new System.EventHandler( this.BtnUpdateMatchingRecord_Click );
            // 
            // BtnUpdateMatchingRecords
            // 
            this.BtnUpdateMatchingRecords.Location = new System.Drawing.Point( 373, 60 );
            this.BtnUpdateMatchingRecords.Margin = new System.Windows.Forms.Padding( 4 );
            this.BtnUpdateMatchingRecords.Name = "BtnUpdateMatchingRecords";
            this.BtnUpdateMatchingRecords.Size = new System.Drawing.Size( 175, 36 );
            this.BtnUpdateMatchingRecords.TabIndex = 15;
            this.BtnUpdateMatchingRecords.Text = "Update matching records";
            this.BtnUpdateMatchingRecords.UseVisualStyleBackColor = true;
            this.BtnUpdateMatchingRecords.Click += new System.EventHandler( this.BtnUpdateMatchingRecords_Click );
            // 
            // BtnUpdateRecordsRegex
            // 
            this.BtnUpdateRecordsRegex.Location = new System.Drawing.Point( 373, 121 );
            this.BtnUpdateRecordsRegex.Margin = new System.Windows.Forms.Padding( 4 );
            this.BtnUpdateRecordsRegex.Name = "BtnUpdateRecordsRegex";
            this.BtnUpdateRecordsRegex.Size = new System.Drawing.Size( 175, 36 );
            this.BtnUpdateRecordsRegex.TabIndex = 16;
            this.BtnUpdateRecordsRegex.Text = "Update records Regex";
            this.BtnUpdateRecordsRegex.UseVisualStyleBackColor = true;
            this.BtnUpdateRecordsRegex.Click += new System.EventHandler( this.BtnUpdateRecordsRegex_Click );
            // 
            // BtnEncryptDecryptValue
            // 
            this.BtnEncryptDecryptValue.Location = new System.Drawing.Point( 373, 165 );
            this.BtnEncryptDecryptValue.Margin = new System.Windows.Forms.Padding( 4 );
            this.BtnEncryptDecryptValue.Name = "BtnEncryptDecryptValue";
            this.BtnEncryptDecryptValue.Size = new System.Drawing.Size( 175, 36 );
            this.BtnEncryptDecryptValue.TabIndex = 18;
            this.BtnEncryptDecryptValue.Text = "Encrypt/Decrypt Value";
            this.BtnEncryptDecryptValue.UseVisualStyleBackColor = true;
            this.BtnEncryptDecryptValue.Click += new System.EventHandler( this.BtnEncryptDecryptValue_Click );
            // 
            // BtnTableToDB
            // 
            this.BtnTableToDB.Location = new System.Drawing.Point( 373, 209 );
            this.BtnTableToDB.Margin = new System.Windows.Forms.Padding( 4 );
            this.BtnTableToDB.Name = "BtnTableToDB";
            this.BtnTableToDB.Size = new System.Drawing.Size( 175, 36 );
            this.BtnTableToDB.TabIndex = 19;
            this.BtnTableToDB.Text = "Table to DB";
            this.BtnTableToDB.UseVisualStyleBackColor = true;
            this.BtnTableToDB.Click += new System.EventHandler( this.BtnTableToDB_Click );
            // 
            // BtnRemoveByKey
            // 
            this.BtnRemoveByKey.Location = new System.Drawing.Point( 566, 16 );
            this.BtnRemoveByKey.Margin = new System.Windows.Forms.Padding( 4 );
            this.BtnRemoveByKey.Name = "BtnRemoveByKey";
            this.BtnRemoveByKey.Size = new System.Drawing.Size( 175, 36 );
            this.BtnRemoveByKey.TabIndex = 20;
            this.BtnRemoveByKey.Text = "Remove by Key";
            this.BtnRemoveByKey.UseVisualStyleBackColor = true;
            this.BtnRemoveByKey.Click += new System.EventHandler( this.BtnRemoveByKey_Click );
            // 
            // BtnRemoveByIndex
            // 
            this.BtnRemoveByIndex.Location = new System.Drawing.Point( 566, 60 );
            this.BtnRemoveByIndex.Margin = new System.Windows.Forms.Padding( 4 );
            this.BtnRemoveByIndex.Name = "BtnRemoveByIndex";
            this.BtnRemoveByIndex.Size = new System.Drawing.Size( 175, 36 );
            this.BtnRemoveByIndex.TabIndex = 21;
            this.BtnRemoveByIndex.Text = "Remove by Index";
            this.BtnRemoveByIndex.UseVisualStyleBackColor = true;
            this.BtnRemoveByIndex.Click += new System.EventHandler( this.BtnRemoveByIndex_Click );
            // 
            // BtnRemoveByValue1
            // 
            this.BtnRemoveByValue1.Location = new System.Drawing.Point( 566, 121 );
            this.BtnRemoveByValue1.Margin = new System.Windows.Forms.Padding( 4 );
            this.BtnRemoveByValue1.Name = "BtnRemoveByValue1";
            this.BtnRemoveByValue1.Size = new System.Drawing.Size( 175, 36 );
            this.BtnRemoveByValue1.TabIndex = 22;
            this.BtnRemoveByValue1.Text = "Remove by Value 1";
            this.BtnRemoveByValue1.UseVisualStyleBackColor = true;
            this.BtnRemoveByValue1.Click += new System.EventHandler( this.BtnRemoveByValue1_Click );
            // 
            // BtnRemoveByValue2
            // 
            this.BtnRemoveByValue2.Location = new System.Drawing.Point( 566, 165 );
            this.BtnRemoveByValue2.Margin = new System.Windows.Forms.Padding( 4 );
            this.BtnRemoveByValue2.Name = "BtnRemoveByValue2";
            this.BtnRemoveByValue2.Size = new System.Drawing.Size( 175, 36 );
            this.BtnRemoveByValue2.TabIndex = 23;
            this.BtnRemoveByValue2.Text = "Remove By Value 2";
            this.BtnRemoveByValue2.UseVisualStyleBackColor = true;
            this.BtnRemoveByValue2.Click += new System.EventHandler( this.BtnRemoveByValue2_Click );
            // 
            // BtnReindex
            // 
            this.BtnReindex.Location = new System.Drawing.Point( 761, 209 );
            this.BtnReindex.Margin = new System.Windows.Forms.Padding( 4 );
            this.BtnReindex.Name = "BtnReindex";
            this.BtnReindex.Size = new System.Drawing.Size( 175, 36 );
            this.BtnReindex.TabIndex = 28;
            this.BtnReindex.Text = "Reindex";
            this.BtnReindex.UseVisualStyleBackColor = true;
            this.BtnReindex.Click += new System.EventHandler( this.BtnReindex_Click );
            // 
            // BtnIterate
            // 
            this.BtnIterate.Location = new System.Drawing.Point( 761, 165 );
            this.BtnIterate.Margin = new System.Windows.Forms.Padding( 4 );
            this.BtnIterate.Name = "BtnIterate";
            this.BtnIterate.Size = new System.Drawing.Size( 175, 36 );
            this.BtnIterate.TabIndex = 27;
            this.BtnIterate.Text = "Iterate";
            this.BtnIterate.UseVisualStyleBackColor = true;
            this.BtnIterate.Click += new System.EventHandler( this.BtnIterate_Click );
            // 
            // BtnNumDeleted
            // 
            this.BtnNumDeleted.Location = new System.Drawing.Point( 761, 121 );
            this.BtnNumDeleted.Margin = new System.Windows.Forms.Padding( 4 );
            this.BtnNumDeleted.Name = "BtnNumDeleted";
            this.BtnNumDeleted.Size = new System.Drawing.Size( 175, 36 );
            this.BtnNumDeleted.TabIndex = 26;
            this.BtnNumDeleted.Text = "Num deleted";
            this.BtnNumDeleted.UseVisualStyleBackColor = true;
            this.BtnNumDeleted.Click += new System.EventHandler( this.BtnNumDeleted_Click );
            // 
            // BtnNumRecords
            // 
            this.BtnNumRecords.Location = new System.Drawing.Point( 761, 60 );
            this.BtnNumRecords.Margin = new System.Windows.Forms.Padding( 4 );
            this.BtnNumRecords.Name = "BtnNumRecords";
            this.BtnNumRecords.Size = new System.Drawing.Size( 175, 36 );
            this.BtnNumRecords.TabIndex = 25;
            this.BtnNumRecords.Text = "Num records";
            this.BtnNumRecords.UseVisualStyleBackColor = true;
            this.BtnNumRecords.Click += new System.EventHandler( this.BtnNumRecords_Click );
            // 
            // BtnClean
            // 
            this.BtnClean.Location = new System.Drawing.Point( 761, 16 );
            this.BtnClean.Margin = new System.Windows.Forms.Padding( 4 );
            this.BtnClean.Name = "BtnClean";
            this.BtnClean.Size = new System.Drawing.Size( 175, 36 );
            this.BtnClean.TabIndex = 24;
            this.BtnClean.Text = "Clean";
            this.BtnClean.UseVisualStyleBackColor = true;
            this.BtnClean.Click += new System.EventHandler( this.BtnClean_Click );
            // 
            // BtnLinqAggregates
            // 
            this.BtnLinqAggregates.Location = new System.Drawing.Point( 958, 209 );
            this.BtnLinqAggregates.Margin = new System.Windows.Forms.Padding( 4 );
            this.BtnLinqAggregates.Name = "BtnLinqAggregates";
            this.BtnLinqAggregates.Size = new System.Drawing.Size( 175, 36 );
            this.BtnLinqAggregates.TabIndex = 33;
            this.BtnLinqAggregates.Text = "Linq Aggregates";
            this.BtnLinqAggregates.UseVisualStyleBackColor = true;
            this.BtnLinqAggregates.Click += new System.EventHandler( this.BtnLinqAggregates_Click );
            // 
            // BtnLinqHierarchicalObjects
            // 
            this.BtnLinqHierarchicalObjects.Location = new System.Drawing.Point( 958, 165 );
            this.BtnLinqHierarchicalObjects.Margin = new System.Windows.Forms.Padding( 4 );
            this.BtnLinqHierarchicalObjects.Name = "BtnLinqHierarchicalObjects";
            this.BtnLinqHierarchicalObjects.Size = new System.Drawing.Size( 175, 36 );
            this.BtnLinqHierarchicalObjects.TabIndex = 32;
            this.BtnLinqHierarchicalObjects.Text = "Linq Hierarchical Objects";
            this.BtnLinqHierarchicalObjects.UseVisualStyleBackColor = true;
            this.BtnLinqHierarchicalObjects.Click += new System.EventHandler( this.BtnLinqHierarchicalObjects_Click );
            // 
            // BtnLinqGroupBy
            // 
            this.BtnLinqGroupBy.Location = new System.Drawing.Point( 958, 121 );
            this.BtnLinqGroupBy.Margin = new System.Windows.Forms.Padding( 4 );
            this.BtnLinqGroupBy.Name = "BtnLinqGroupBy";
            this.BtnLinqGroupBy.Size = new System.Drawing.Size( 175, 36 );
            this.BtnLinqGroupBy.TabIndex = 31;
            this.BtnLinqGroupBy.Text = "Linq GroupBy";
            this.BtnLinqGroupBy.UseVisualStyleBackColor = true;
            this.BtnLinqGroupBy.Click += new System.EventHandler( this.BtnLinqGroupBy_Click );
            // 
            // BtnLinqJoin
            // 
            this.BtnLinqJoin.Location = new System.Drawing.Point( 958, 60 );
            this.BtnLinqJoin.Margin = new System.Windows.Forms.Padding( 4 );
            this.BtnLinqJoin.Name = "BtnLinqJoin";
            this.BtnLinqJoin.Size = new System.Drawing.Size( 175, 36 );
            this.BtnLinqJoin.TabIndex = 30;
            this.BtnLinqJoin.Text = "Linq Join";
            this.BtnLinqJoin.UseVisualStyleBackColor = true;
            this.BtnLinqJoin.Click += new System.EventHandler( this.BtnLinqJoin_Click );
            // 
            // BtnLinqSelect
            // 
            this.BtnLinqSelect.Location = new System.Drawing.Point( 958, 16 );
            this.BtnLinqSelect.Margin = new System.Windows.Forms.Padding( 4 );
            this.BtnLinqSelect.Name = "BtnLinqSelect";
            this.BtnLinqSelect.Size = new System.Drawing.Size( 175, 36 );
            this.BtnLinqSelect.TabIndex = 29;
            this.BtnLinqSelect.Text = "Linq Select";
            this.BtnLinqSelect.UseVisualStyleBackColor = true;
            this.BtnLinqSelect.Click += new System.EventHandler( this.BtnLinqSelect_Click );
            // 
            // BtnTableFromTable
            // 
            this.BtnTableFromTable.Location = new System.Drawing.Point( 373, 253 );
            this.BtnTableFromTable.Margin = new System.Windows.Forms.Padding( 4 );
            this.BtnTableFromTable.Name = "BtnTableFromTable";
            this.BtnTableFromTable.Size = new System.Drawing.Size( 175, 36 );
            this.BtnTableFromTable.TabIndex = 34;
            this.BtnTableFromTable.Text = "Table from Table";
            this.BtnTableFromTable.UseVisualStyleBackColor = true;
            this.BtnTableFromTable.Click += new System.EventHandler( this.BtnTableFromTable_Click );
            // 
            // BtnMetaData
            // 
            this.BtnMetaData.Location = new System.Drawing.Point( 566, 209 );
            this.BtnMetaData.Margin = new System.Windows.Forms.Padding( 4 );
            this.BtnMetaData.Name = "BtnMetaData";
            this.BtnMetaData.Size = new System.Drawing.Size( 175, 36 );
            this.BtnMetaData.TabIndex = 35;
            this.BtnMetaData.Text = "Get/Set MetaData";
            this.BtnMetaData.UseVisualStyleBackColor = true;
            this.BtnMetaData.Click += new System.EventHandler( this.BtnMetaData_Click );
            // 
            // MainFrm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF( 8F, 16F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size( 1168, 545 );
            this.Controls.Add( this.BtnMetaData );
            this.Controls.Add( this.BtnTableFromTable );
            this.Controls.Add( this.BtnLinqAggregates );
            this.Controls.Add( this.BtnLinqHierarchicalObjects );
            this.Controls.Add( this.BtnLinqGroupBy );
            this.Controls.Add( this.BtnLinqJoin );
            this.Controls.Add( this.BtnLinqSelect );
            this.Controls.Add( this.BtnReindex );
            this.Controls.Add( this.BtnIterate );
            this.Controls.Add( this.BtnNumDeleted );
            this.Controls.Add( this.BtnNumRecords );
            this.Controls.Add( this.BtnClean );
            this.Controls.Add( this.BtnRemoveByValue2 );
            this.Controls.Add( this.BtnRemoveByValue1 );
            this.Controls.Add( this.BtnRemoveByIndex );
            this.Controls.Add( this.BtnRemoveByKey );
            this.Controls.Add( this.BtnTableToDB );
            this.Controls.Add( this.BtnEncryptDecryptValue );
            this.Controls.Add( this.BtnUpdateRecordsRegex );
            this.Controls.Add( this.BtnUpdateMatchingRecords );
            this.Controls.Add( this.BtnUpdateMatchingRecord );
            this.Controls.Add( this.grid );
            this.Controls.Add( this.BtnGetRecordByKey );
            this.Controls.Add( this.BtnGetRecordByIndex );
            this.Controls.Add( this.BtnGetMatchingRecords );
            this.Controls.Add( this.BtnGetRecordsRegex );
            this.Controls.Add( this.BtnGetAllRecordsReverseSort );
            this.Controls.Add( this.BtnGetAllRecordsSorted );
            this.Controls.Add( this.BtnDrop );
            this.Controls.Add( this.BtnCloseDb );
            this.Controls.Add( this.BtnOpenEncrypted );
            this.Controls.Add( this.btnOpen );
            this.Controls.Add( this.BtnAddRecords );
            this.Controls.Add( this.BtnClose );
            this.Controls.Add( this.BtnCreate );
            this.Margin = new System.Windows.Forms.Padding( 4 );
            this.Name = "MainFrm";
            this.Text = "FileDb Sample App";
            ((System.ComponentModel.ISupportInitialize) (this.grid)).EndInit();
            this.ResumeLayout( false );

        }

        #endregion

        private System.Windows.Forms.Button BtnCreate;
        private System.Windows.Forms.Button BtnClose;
        private System.Windows.Forms.Button BtnAddRecords;
        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.Button BtnOpenEncrypted;
        private System.Windows.Forms.Button BtnCloseDb;
        private System.Windows.Forms.Button BtnDrop;
        private System.Windows.Forms.Button BtnGetRecordByKey;
        private System.Windows.Forms.Button BtnGetRecordByIndex;
        private System.Windows.Forms.Button BtnGetMatchingRecords;
        private System.Windows.Forms.Button BtnGetRecordsRegex;
        private System.Windows.Forms.Button BtnGetAllRecordsReverseSort;
        private System.Windows.Forms.Button BtnGetAllRecordsSorted;
        private System.Windows.Forms.DataGridView grid;
        private System.Windows.Forms.Button BtnUpdateMatchingRecord;
        private System.Windows.Forms.Button BtnUpdateMatchingRecords;
        private System.Windows.Forms.Button BtnUpdateRecordsRegex;
        private System.Windows.Forms.Button BtnEncryptDecryptValue;
        private System.Windows.Forms.Button BtnTableToDB;
        private System.Windows.Forms.Button BtnRemoveByKey;
        private System.Windows.Forms.Button BtnRemoveByIndex;
        private System.Windows.Forms.Button BtnRemoveByValue1;
        private System.Windows.Forms.Button BtnRemoveByValue2;
        private System.Windows.Forms.Button BtnReindex;
        private System.Windows.Forms.Button BtnIterate;
        private System.Windows.Forms.Button BtnNumDeleted;
        private System.Windows.Forms.Button BtnNumRecords;
        private System.Windows.Forms.Button BtnClean;
        private System.Windows.Forms.Button BtnLinqAggregates;
        private System.Windows.Forms.Button BtnLinqHierarchicalObjects;
        private System.Windows.Forms.Button BtnLinqGroupBy;
        private System.Windows.Forms.Button BtnLinqJoin;
        private System.Windows.Forms.Button BtnLinqSelect;
        private System.Windows.Forms.Button BtnTableFromTable;
        private System.Windows.Forms.Button BtnMetaData;
    }
}

