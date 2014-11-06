using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Diagnostics;

using Utils;
using FileDbNs;
using TenTec.Windows.iGridLib;

namespace FileDbExplorer
{
    //===================================================================================
    public partial class QueryCtrl : UserControl
    {
        private enum Accelerators { Unspecified = 0, Execute, Open, Save };

        #region Fields
        Hashtable _accels = new Hashtable();
        
        string _filename;

        Table _table;
        Record _editRow;

        Color _rowBackColor = Color.White;

        int _nCurEditRow = -1;

        bool _bBeginEditNewRow;
        #endregion Fields

        //===============================================================================
        class ArrayHolder
        {
            internal DataTypeEnum DataType { get; set; }

            internal ArrayHolder( object array, DataTypeEnum dataType )
            {
                Array = array;
                DataType = dataType;
            }

            internal object Array
            {
                get;
                set;
            }

            public override string ToString()
            {
                string str = null; // string.Empty;

                /*if( DataType == DataType.Byte )
                {
                    if( Array != null )
                        str = "<binary>";
                }
                else*/
                {
                    if( Array != null )
                    {
                        int len = 0;
                        str = Array.ToString();

                        switch( DataType )
                        {
                            case DataTypeEnum.Byte:
                            {
                                Byte[] arr = (Byte[]) Array;
                                len = arr.Length;
                            }
                            break;

                            case DataTypeEnum.Bool:
                            {
                                bool[] arr = (bool[]) Array;
                                len = arr.Length;
                            }
                            break;

                            case DataTypeEnum.DateTime:
                            {
                                DateTime[] arr = (DateTime[]) Array;
                                len = arr.Length;
                            }
                            break;

                            case DataTypeEnum.Double:
                            {
                                double[] arr = (double[]) Array;
                                len = arr.Length;
                            }
                            break;

                            case DataTypeEnum.Int32:
                            {
                                Int32[] arr = (Int32[]) Array;
                                len = arr.Length;
                            }
                            break;

                            case DataTypeEnum.String:
                            {
                                string[] arr = (string[]) Array;
                                len = arr.Length;
                            }
                            break;
                        }

                        int ndx = str.LastIndexOf( '.' );
                        str = str.Substring( ndx + 1 );
                        string[] vstr = str.Split( "[]".ToCharArray() );
                        str = string.Format( "{0}[{1}]", vstr[0], len );
                    }
                }
                return str;
            }
        }

        //---------------------------------------------------------------------
        string Title
        {
            get { return (this.Parent as TabPage).Text; }
            set { (this.Parent as TabPage).Text = Path.GetFileName( value ); }
        }

        //---------------------------------------------------------------------
        public QueryCtrl()
        {
            InitializeComponent();

            _accels.Add( new AcceleratorKey( Keys.Control | Keys.O ), Accelerators.Open );
            _accels.Add( new AcceleratorKey( Keys.Control | Keys.S ), Accelerators.Save );
            _accels.Add( new AcceleratorKey( Keys.Control | Keys.E ), Accelerators.Execute );
            _accels.Add( new AcceleratorKey( Keys.F5 ), Accelerators.Execute );
        }

        //---------------------------------------------------------------------
        private void QueryCtrl_Load( object sender, EventArgs e )
        {
            try
            {
                Grid.DefaultRow.Height = (int) (Grid.Font.Height * 1.2); // Grid.GetPreferredRowHeight( true, false );
                Grid.DefaultAutoGroupRow.Height = (int) (Grid.DefaultRow.Height * 1.2);
                Grid.SilentValidation = true;
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Exclamation );
            }
        }

        //---------------------------------------------------------------------
        void BtnExecute_Click( object sender, EventArgs e )
        {
            execute();
        }

        void execute()
        {
            string queryText = CodeEditor.SelectedText.Trim();
            if( string.IsNullOrEmpty( queryText ) )
            {
                queryText = CodeEditor.Text.Trim();
            }
            Execute( queryText, false );
        }

        internal void Execute( string sql, bool setText )
        {
            try
            {
                FileDb fileDb = null;

                StringBuilder sb = new StringBuilder( sql.Length );
                for( int n = 0; n < sql.Length; n++ )
                {
                    if( sql[n] == '\t' || sql[n] == '\n' || sql[n] == '\r' )
                        sb.Append( ' ' );
                    else
                        sb.Append( sql[n] );
                }
                sql = sb.ToString();

                if( setText )
                    CodeEditor.Text = sql;

                // we only support SELECT currently
                string[] selFields;
                FilterExpressionGroup sexg;
                string[] orderByFields;

                parseSql( sql, out selFields, out fileDb, out sexg, out orderByFields );

                Grid.Tag = fileDb; // save away the db ref for later

                if( sexg == null )
                    _table = fileDb.SelectAllRecords( selFields, orderByFields );
                else
                    _table = fileDb.SelectRecords( sexg, selFields, orderByFields );

                Grid.BeginUpdate();

                Grid.Rows.Count = 0;
                Grid.Cols.Count = 0;

                if( _table != null )
                {
                    foreach( Field fld in _table.Fields )
                    {
                        iGCol col = Grid.Cols.Add( fld.Name );
                        if( fld.IsArray )
                        {
                            if( fld.IsArray && fld.DataType == DataTypeEnum.Byte )
                                col.CellStyle.ReadOnly = iGBool.True;

                            iGDropDownList dropList = new iGDropDownList();
                            col.CellStyle.DropDownControl = dropList;
                            col.CellStyle.TypeFlags = iGCellTypeFlags.NoTextEdit;
                        }
                    }
                    foreach( iGCol col in Grid.Cols )
                        col.AutoWidth( false );

                    Grid.Rows.Count = _table.Count;

                    iGCellCollection cells = Grid.Cells;

                    for( int nRow = 0; nRow < _table.Count; nRow++ )
                    {
                        setGridRowValues( nRow );

                        // Bind the grid's rows to the data rows
                        Record row = _table[nRow];
                        Grid.Rows[nRow].Tag = row;
                    }

                    //foreach( iGCol col in Grid.Cols )
                    //    col.AutoWidth( true );
                }

                // licensing
                #if false
                if( !MainFrm.TheAppWnd.LicenseInfo.IsLicensed )
                {
                    int nLimit = 1100 / 11,
                        nCount = Grid.Rows.Count;
                    if( nCount > nLimit )
                        Grid.Rows.RemoveRange( nLimit, nCount - nLimit );
                }
                #endif

                string snum = Grid.Rows.Count.ToString();
                int extraDigits = snum.Length - 4;
                if( extraDigits > 0 )
                    Grid.RowHeader.Width += extraDigits * 7;

                Grid.Invalidate();
            }
            catch( FileDbException ex )
            {
                MessageBox.Show( ex.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Exclamation );
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Exclamation );
            }
            finally
            {
                Grid.EndUpdate();
            }
        }

        private void parseSql( string sql, out string[] selFields, out FileDb fileDb, out FilterExpressionGroup sexg, out string[] orderByFields )
        {
            string select, dbName, from = null, where = null, orderBy = null;

            // we only support SELECT...

            if( string.Compare( sql.Substring( 0, 6 ), "SELECT", true ) != 0 )
                throw new Exception( "Command must begin with 'SELECT'" );

            // select f1, f2, f3 from employees where f1 = 'xxx'

            int idx1 = getIndexOfClause( "FROM", sql, 0 ),
                idx2, idxWhere;
            if( idx1 < 0 )
                throw new Exception( "Command must contain a 'FROM' clause" );

            select = sql.Substring( 0, idx1-1 );
            
            idxWhere = getIndexOfClause( "WHERE", sql, idx1 );
            if( idxWhere > -1 )
            {
                from = sql.Substring( idx1, idxWhere - idx1 );
                idx1 = idxWhere;

                idx2 = getIndexOfClause( "ORDERBY", sql, idx1 );
                if( idx2 < 0 )
                    idx2 = getIndexOfClause( "ORDER BY", sql, idx1 );

                if( idx2 > -1 )
                {
                    // found ORDERBY clause
                    where = sql.Substring( idx1, idx2 - idx1 );
                    orderBy = sql.Substring( idx2 );
                }
                else
                    where = sql.Substring( idx1 );
            }
            else
            {
                idx2 = getIndexOfClause( "ORDERBY", sql, idx1 );
                if( idx2 < 0 )
                    idx2 = getIndexOfClause( "ORDER BY", sql, idx1 );

                if( idx2 > -1 )
                {
                    from = sql.Substring( idx1, idx2 - idx1 );
                    orderBy = sql.Substring( idx2 );
                }
                else
                    from = sql.Substring( idx1 );
            }

            dbName = parseFrom( from );
            fileDb = MainFrm.TheAppWnd.DbView.GetDb( dbName );
            if( fileDb == null )
                throw new Exception( string.Format( "The specified database is not open\r\n\r\n\t{0}", dbName ) );

            selFields = parseSelect( fileDb, select );
            sexg = parseWhere( where );
            orderByFields = parseOrderBy( orderBy );
        }

        int getIndexOfClause( string searchClause, string sql, int idx )
        {
            int clauseLen = searchClause.Length;

            while( true )
            {
                idx = sql.IndexOf( searchClause, idx, StringComparison.OrdinalIgnoreCase );
                if( idx < 0 )
                    break;

                // check to see that its NOT a field name, which must be wrapped with []
                if( sql[idx - 1] == ' ' && sql[idx + clauseLen] == ' ' )
                {
                    break;
                }
            }
            return idx;
        }

        private string[] parseSelect( FileDb fileDb, string select )
        {
            string[] fields = null;
            
            // skip past SELECT
            select = select.Substring( 6 );
            fields = select.Split( ",".ToCharArray() );
            var selFields = new List<string>( fields.Length );
            int starIndex = -1;

            for( int n = 0; n < fields.Length; n++ )
            {
                string fieldName = fields[n].Trim();

                if( fieldName[0] == '[' )  // assume there is a matching closing bracket
                    fieldName = fieldName.Substring( 1, fieldName.Length - 2 );
                
                if( fieldName[0] == '*' )
                    starIndex = n;
                else
                    selFields.Add( fieldName );
            }

            if( starIndex > -1 )
            {
                // insert all of the fieldnames
                foreach( Field field in fileDb.Fields )
                {
                    // cannot specify the same field twice
                    foreach( string fieldName in selFields )
                    {
                        if( string.Compare( fieldName, field.Name, true )==0 )
                            goto Next;
                    }
                    selFields.Insert( starIndex++, field.Name );
                Next: ; // no-op
                }            
            }

            return selFields.ToArray();
        }

        private string parseFrom( string from )
        {
            string tableName = null;

            // skip past FROM
            tableName = from.Substring( 4 ).Trim();

            if( tableName[0] == '[' )  // assume there is a matching closing bracket
                tableName = tableName.Substring( 1, tableName.Length - 2 );

            return tableName;
        }

        private FilterExpressionGroup parseWhere( string where )
        {
            if( where == null )
                return null;

            // skip past WHERE
            where = where.Substring( 5 ).Trim();
            return FilterExpressionGroup.Parse( where );
        }

        private string[] parseOrderBy( string orderBy )
        {
            string[] orderByFields = null;

            if( orderBy != null )
            {
                // skip past ORDERBY

                if( orderBy[5] == ' ' )
                    orderBy = orderBy.Substring( 8 );
                else
                    orderBy = orderBy.Substring( 7 );

                orderByFields = orderBy.Split( ",".ToCharArray() );

                for( int n = 0; n < orderByFields.Length; n++ )
                {
                    orderByFields[n] = orderByFields[n].Trim();
                    if( orderByFields[n][0] == '[' )  // assume there is a matching closing bracket
                        orderByFields[n] = orderByFields[n].Substring( 1, orderByFields[n].Length - 2 );
                }
            }

            return orderByFields;
        }

        //---------------------------------------------------------------------
        private bool IsNewRow( int rowIndex )
        {
            return Grid.ReadOnly ? false : Grid.Rows[rowIndex].Tag == null;
        }

        //---------------------------------------------------------------------
        private bool IsEditingNewRow()
        {
            if( Grid.Rows.Count > 0 && Grid.CurRow != null && Grid.CurRow.Tag == null )
                return true;

            return false;
        }

        //---------------------------------------------------------------------
        protected override bool ProcessCmdKey( ref Message msg, Keys keyData )
        {
            // Check this key...
            bool bHandled = base.ProcessCmdKey( ref msg, keyData );

            if( !bHandled )
            {
                // Process local keys

                Accelerators accel = Accelerators.Unspecified;

                if( _accels.ContainsKey( new AcceleratorKey( keyData ) ) )
                {
                    accel = (Accelerators) _accels[keyData];

                    switch( accel )
                    {
                        case Accelerators.Execute:
                            execute();
                            bHandled = true;
                            break;

                        case Accelerators.Save:
                            save();
                            bHandled = true;
                            break;

                        case Accelerators.Open:
                            openFile();
                            bHandled = true;
                            break;

                        case Accelerators.Unspecified:
                        default:
                            break;

                    }
                }
            }
            return bHandled;
        }

        void save()
        {
            try
            {
                if( string.IsNullOrEmpty( _filename ) )
                {
                    SaveFileDialog fileDlg = new SaveFileDialog();

                    fileDlg.Filter = "SQL files (*.sql)|*.sql|All files (*.*)|*.*";
                    fileDlg.SupportMultiDottedExtensions = true;

                    if( fileDlg.ShowDialog() == DialogResult.OK )
                    {
                        _filename = fileDlg.FileName;
                        Title = _filename;
                    }
                    else return;
                }
                FileStream fStream = new FileStream( _filename, FileMode.Create, FileAccess.Write );

                // write the file
                StreamWriter writer = new StreamWriter( fStream );

                writer.Write( CodeEditor.Text );
                writer.Flush();
                writer.Close();
                fStream.Close();
                CodeEditor.Modified = false;
            }
            catch( Exception ex )
            {
                MessageBox.Show( this, ex.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Exclamation );
            }
        }

        void openFile()
        {
            try
            {
                OpenFileDialog fileDlg = new OpenFileDialog();
                fileDlg.Filter = "SQL files (*.sql)|*.sql|All files (*.*)|*.*";
                fileDlg.SupportMultiDottedExtensions = true;

                if( fileDlg.ShowDialog() == DialogResult.OK )
                {
                    Stream fStream;

                    if( (fStream = fileDlg.OpenFile()) != null )
                    {
                        StreamReader reader = new StreamReader( fStream );
                        CodeEditor.Text = reader.ReadToEnd();
                        reader.Close();
                        fStream.Close();
                        _filename = fileDlg.FileName;
                        Title = _filename;
                    }
                }
            }
            catch( Exception ex )
            {
                MessageBox.Show( this, ex.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Exclamation );
            }
        }

        internal bool OkToClose()
        {
            bool ok = true;

            if( CodeEditor.Modified )
            {
                DialogResult result = MessageBox.Show( this, string.Format( "Do you want to save the contents of: {0}", Title ), null,
                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation );

                if( result == DialogResult.Yes )
                {
                    save();
                    CodeEditor.Modified = false;
                }
                else if( result == DialogResult.Cancel )
                {
                    ok = false;
                }
            }
            return ok;
        }

        private void BtnOpen_Click( object sender, EventArgs e )
        {
            openFile();
        }

        private void BtnSave_Click( object sender, EventArgs e )
        {
            save();
        }

        void beginRowEdit( int rowIndex )
        {
            // create a new row regardless if adding or editing
            if( _editRow == null )
                _editRow = _table.AddRecord();

            #if DEBUG
            if( _nCurEditRow > -1 ) Debug.Assert( _nCurEditRow == rowIndex );
            #endif

            _nCurEditRow = rowIndex;
            iGRow gRow = Grid.Rows[_nCurEditRow];
            Record curRow = gRow.Tag as Record;

            if( curRow == null )
            {
                // must be adding a new row
                Debug.Assert( _nCurEditRow == Grid.Rows.Count-1 );
            }
            else
            {
                // copy the row data
                for( int n = 0; n < curRow.Length; n++ )
                {
                    _editRow[n] = curRow[n];                            
                }
            }

            _rowBackColor = gRow.BackColor;
            gRow.BackColor = Color.Pink;
        }

        void setDropListValues( iGDropDownList dropList, Field field,  ArrayHolder arrayHolder )
        {
            dropList.Items.Clear();

            if( arrayHolder == null )
                return; 

            switch( field.DataType )
            {
                case DataTypeEnum.Byte:
                {
                    // nothing to do because we don't display binary data
                }
                break;

                case DataTypeEnum.Bool:
                {
                    bool[] arr = (bool[]) arrayHolder.Array;
                    if( arr != null )
                    {
                        foreach( bool val in arr )
                        {
                            dropList.Items.Add( val.ToString() );
                        }
                    }
                }
                break;

                case DataTypeEnum.DateTime:
                {
                    DateTime[] arr = (DateTime[]) arrayHolder.Array;
                    if( arr != null )
                    {
                        foreach( DateTime val in arr )
                        {
                            dropList.Items.Add( val.ToString() );
                        }
                    }
                }
                break;

                case DataTypeEnum.Double:
                {
                    double[] arr = (double[]) arrayHolder.Array;
                    if( arr != null )
                    {
                        foreach( double val in arr )
                        {
                            dropList.Items.Add( val.ToString() );
                        }
                    }
                }
                break;

                case DataTypeEnum.Int32:
                {
                    Int32[] arr = (Int32[]) arrayHolder.Array;
                    if( arr != null )
                    {
                        foreach( Int32 val in arr )
                        {
                            dropList.Items.Add( val.ToString() );
                        }
                    }
                }
                break;

                case DataTypeEnum.String:
                {
                    String[] arr = (String[]) arrayHolder.Array;
                    if( arr != null )
                    {
                        foreach( String val in arr )
                        {
                            dropList.Items.Add( val );
                        }
                    }
                }
                break;
            }
        }

        private void Grid_RequestEdit( object sender, iGRequestEditEventArgs e )
        {
            if( _table == null )
                return;

            try
            {
                Field field = _table.Fields[e.ColIndex];

                if( !field.IsArray )
                {
                    if( _editRow == null )
                        beginRowEdit( e.RowIndex );
                }
                else // we edit array fields differently, so we must repopulate the DropList each time
                {
                    iGCol col = Grid.Cols[e.ColIndex];
                    iGDropDownList dropList = col.CellStyle.DropDownControl as iGDropDownList;
                    object values = Grid.Cells[e.RowIndex, e.ColIndex].Value;
                    setDropListValues( dropList, field, (ArrayHolder) values );
                }
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Error );
            }
        }

        private void Grid_BeforeCommitEdit( object sender, iGBeforeCommitEditEventArgs e )
        {
            // If there was an exception when iGrid tried to parse the entered text.
            if( e.Exception != null )
            {
                MessageBox.Show( e.Exception.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error );
                // Continue editing if the value is not accepted.
                e.Result = iGEditResult.Cancel;
                return;
            }
            
            if( _table == null )
                return;

            Field field = _table.Fields[e.ColIndex];

            if( field.IsArray )
            {
                e.Result = iGEditResult.Cancel;
            }
            else
            {
                //Debug.Assert( _nCurEditRow == e.RowIndex );

                if( _editRow == null )
                    beginRowEdit( e.RowIndex );

                object curVal = _editRow[e.ColIndex];
                if( curVal == null )
                {
                    if( !string.IsNullOrEmpty( e.NewText ) )
                    {
                        _editRow[e.ColIndex] = e.NewValue;
                    }
                }
                else if( curVal.ToString() != e.NewText )
                    _editRow[e.ColIndex] = e.NewValue;

            }
        }

        private void BeginEditNewRow( int rowIndex )
        {
            _bBeginEditNewRow = true;
            try
            {
                // Set the default values and current cell
                //Grid.Rows[0].Type = iGRowType.Normal;
                Grid.PerformAction( iGActions.DeselectAllCells );
                Grid.PerformAction( iGActions.DeselectAllRows );
                iGRow newRow = Grid.Rows.Insert( rowIndex );
                _nCurEditRow = newRow.Index;

                //foreach( iGCell myCell in Grid.Rows[0].Cells )
                foreach( iGCell myCell in newRow.Cells )
                {
                    myCell.Value = myCell.Col.DefaultCellValue;
                    if( myCell.ColIndex == rowIndex )
                        Grid.SetCurCell( newRow.Index, 0 );
                }
                Grid.SetCurCell( _nCurEditRow, 0 );
                Grid.RequestEditCurCell( (char) 0 );
            }
            finally
            {
                _bBeginEditNewRow = false;
            }
        }

        private void CancelEditNewRow()
        {
            int nEditRow = _nCurEditRow;
            _nCurEditRow = -1;
            _editRow = null;
            if( nEditRow > -1 )
                Grid.Rows.RemoveAt( nEditRow );
        }

        Field getPrimaryKeyField( Table table )
        {
            Field keyField = null;
            foreach( Field field in table.Fields )
            {
                if( field.IsPrimaryKey )
                {
                    keyField = field;
                    break;
                }
            }
            return keyField;
        }

        private bool CommitEditRow( bool bEnsureVisibleNewRow )
        {
            if( _nCurEditRow < 0 ) return true;

            try
            {
                iGRow gRow = Grid.Rows[_nCurEditRow];

                // Scroll to make sure the new row is visible
                if( bEnsureVisibleNewRow )
                    gRow.EnsureVisible();

                if( _editRow == null )
                    return true;

                FileDb fileDb = Grid.Tag as FileDb;
                Field keyField = getPrimaryKeyField( _table );

                if( keyField == null )
                    throw new Exception( "You can only edit rows in the Grid if there is a Primary Key on the database and its present in the Grid" );

                // If columns were sorted, remove the sorting as the new row was added.
                Grid.SortObject.Clear();

                //Grid.Rows[0].Type = iGRowType.ManualGroupRow;
                //AdjustSubTotals();

                Record curRow = (Record) gRow.Tag;

                if( curRow == null )
                {
                    curRow = _editRow;

                    int newIndex = fileDb.AddRecord( _editRow.GetFieldValues() );

                    // we must retrieve the new row so as to get all of the default values
                    var fieldNames = new List<string>( _table.Fields.Count-1 ); // don't add the "index" field
                    foreach( Field field in _table.Fields )
                    {
                        if( string.Compare( field.Name, "index", true ) != 0 )
                            fieldNames.Add( field.Name );
                    }
                    Record newRow = fileDb.GetRecordByIndex( newIndex, fieldNames.ToArray() );

                    curRow = newRow;

                    gRow.Tag = curRow;
                    _table.Add( curRow );
                    setGridRowValues( _nCurEditRow );
                }
                else
                {
                    bool hasChanged = false;

                    // check to see if any fields have changed - if not don't bother to update the DB
                    for( int n = 0; n < curRow.Length; n++ )
                    {
                        if( curRow[n] != _editRow[n] )
                        {
                            hasChanged = true;
                            break;
                        }
                    }

                    if( hasChanged )
                    {
                        // first update the DB to be sure the values are OK
                        fileDb.UpdateRecordByKey( _editRow[keyField.Name], _editRow.GetFieldValues() );

                        // now copy the edited row
                        for( int n = 0; n < curRow.Length; n++ )
                        {
                            curRow[n] = _editRow[n];
                        }
                    }
                }

                _editRow = null;
                _nCurEditRow = -1;

                gRow.BackColor = _rowBackColor;

                //_bModified = true;
            }
            catch( Exception ex )
            {
                //Debug.WriteLine( ex.Message );
                MessageBox.Show( this, ex.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Hand );
            }

            return _nCurEditRow == -1;
        }

        private void Grid_KeyDown( object sender, KeyEventArgs e )
        {
            try
            {
                if( Grid.CurCell != null )
                {
                    if( IsNewRow( Grid.CurCell.RowIndex ) )
                    {
                        if( e.KeyCode == Keys.Escape )
                        {
                            if( IsEditingNewRow() )
                            {
                                CancelEditNewRow();
                                e.Handled = true;
                            }
                        }
                        else if( e.KeyCode == Keys.Enter || e.KeyCode == Keys.Insert )
                        {
                            if( IsEditingNewRow() )
                            {
                                // Commit edit the new row
                                if( CommitEditRow( true ) )
                                    e.Handled = true;
                            }
                        }
                    }
                    else if( _nCurEditRow > -1 && e.KeyCode == Keys.Escape )
                    {
                        // aborting edit of existing row - restore orig values

                        setGridRowValues( _nCurEditRow );
                        Grid.Rows[_nCurEditRow].BackColor = _rowBackColor;
                        _nCurEditRow = -1;
                        _editRow = null;
                    }
                }

                if( e.KeyCode == Keys.Insert && !IsEditingNewRow() && !Grid.ReadOnly )
                {
                    if( CommitEditRow( true ) )
                    {
                        BeginEditNewRow( Grid.Rows.Count );
                        if( _nCurEditRow > -1 )
                            Grid.Rows[_nCurEditRow].EnsureVisible();
                    }
                    e.Handled = true;
                }

                #region Remove selected the rows

                if( e.KeyCode == Keys.Delete && !Grid.ReadOnly )
                {
                    if( Grid.SelectedCells.Count == 1 )
                    {
                        // we'll allow delete if only one cell is selected

                        iGCell cell = Grid.SelectedCells[0];
                        if( _editRow == null )
                        {
                            beginRowEdit( cell.RowIndex );
                        }

                        _editRow[cell.ColIndex] = null;
                        cell.Value = null;

                        e.Handled = true;
                    }
                    else if( Grid.SelectedRows.Count > 0 )
                    {
                        // Select All Rows in Groups and Deselect "Add New" Record

                        for( int mySelIndex = Grid.SelectedRows.Count - 1; mySelIndex >= 0; mySelIndex-- )
                        {
                            iGRow myRow = Grid.SelectedRows[mySelIndex];
                            if( IsNewRow( myRow.Index ) ) //|| IsSubTotal( myRow ) )
                            {
                                Grid.SelectedRows[mySelIndex].Selected = false;
                            }
                            else if( myRow.Type == iGRowType.AutoGroupRow )
                            {
                                for( int myRowIndex = myRow.Index + 1; myRowIndex < Grid.Rows.Count; myRowIndex++ )
                                {
                                    iGRow myRowInGroup = Grid.Rows[myRowIndex];
                                    if( myRowInGroup.Level <= myRow.Level )
                                        break;
                                    myRowInGroup.Selected = true;
                                }
                            }
                        }

                        // As a message box will be shown, and the grid will not be in focus,
                        // we make the non-focused row selection color the same as focused
                        // row selection color.
                        if( Grid.RowMode )
                        {
                            Grid.SelCellsBackColorNoFocus = Grid.SelCellsBackColor;
                            Grid.SelCellsForeColorNoFocus = Grid.SelCellsForeColor;
                        }
                        else
                        {
                            Grid.SelRowsBackColorNoFocus = Grid.SelRowsBackColor;
                            Grid.SelRowsForeColorNoFocus = Grid.SelRowsForeColor;
                        }

                        // Ask whether to remove the selected rows.
                        if( MessageBox.Show( this, "Remove the selected rows?", Text, MessageBoxButtons.OKCancel, MessageBoxIcon.Question ) == DialogResult.OK )
                        {
                            FileDb fileDb = Grid.Tag as FileDb;
                            Field keyField = getPrimaryKeyField( _table );

                            if( keyField == null )
                                throw new Exception( "You can only delete rows in the Grid if there is a Primary Key on the database and its present in the Grid" );

                            // Remove the selected rows
                            for( int mySelIndex = Grid.SelectedRows.Count - 1; mySelIndex >= 0; mySelIndex-- )
                            {
                                iGRow gridRow = Grid.SelectedRows[mySelIndex];
                                Record row = gridRow.Tag as Record;
                                object key = row[keyField.Name];
                                fileDb.DeleteRecordByKey( key );
                                _table.Remove( row );
                                Grid.Rows.RemoveAt( gridRow.Index );
                                //_bModified = true;
                            }
                            //AdjustSubTotals();
                        }

                        if( Grid.RowMode )
                        {
                            Grid.SelCellsBackColorNoFocus = SystemColors.Control;
                            Grid.SelCellsForeColorNoFocus = SystemColors.WindowText;
                        }
                        else
                        {
                            Grid.SelRowsBackColorNoFocus = Color.Empty;
                            Grid.SelRowsForeColorNoFocus = Color.Empty;
                        }

                        // Focus the grid after the message box was shown.
                        Grid.Focus();

                        e.Handled = true;
                    }
                }
                #endregion
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Exclamation );
            }
        }

        void setGridRowValues( int nRow )
        {
            Record row = _table[nRow];

            for( int nCol = 0; nCol < _table.Fields.Count; nCol++ )
            {
                Field field = _table.Fields[nCol];
                iGCell cell = Grid.Cells[nRow, nCol];
                object dataValue = row[nCol];
                setCellValue( cell, field, dataValue );
            }
        }

        void setCellValue( iGCell cell, Field field, object dataValue )
        {
            if( field.IsArray )
            {
                cell.Value = dataValue == null? null : new ArrayHolder( dataValue, field.DataType );
            }
            else
            {
                cell.Value = dataValue;
            }
        }

        private void Grid_CustomDrawRowHdrBackground( object sender, iGCustomDrawRowHdrBackgroundEventArgs e )
        {
            iGRow row = Grid.Rows[e.RowIndex];

            if( _nCurEditRow == e.RowIndex || row.ReadOnly == iGBool.True )
            {
                Grid.DrawRowHdrGlyph(
                    e.Graphics,
                    row.ReadOnly == iGBool.True ? iGRowHdrGlyph.NewRow : iGRowHdrGlyph.Editing,
                    e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height );
                e.DoDefault = false;
            }
        }

        private void Grid_CellDynamicFormatting( object sender, iGCellDynamicFormattingEventArgs e )
        {
            if( Grid.Rows.Count > 0 && Grid.Rows[e.RowIndex].ReadOnly == iGBool.True )
                e.BackColor = Color.PapayaWhip;
        }

        private void Grid_RowHdrMouseDown( object sender, iGRowHdrMouseDownEventArgs e )
        {
            short nKeyState = Win32.GetKeyState( (int) Keys.ShiftKey );

            if( (nKeyState & 0x8000) != 0x8000 )
            {
                nKeyState = Win32.GetKeyState( (int) Keys.ControlKey );

                if( (nKeyState & 0x8000) != 0x8000 )
                {
                    Grid.PerformAction( iGActions.DeselectAllCells );
                    Grid.PerformAction( iGActions.DeselectAllRows );
                    Grid.SetCurCell( -1, -1 );
                }
            }

            if( !Grid.RowMode && IsNewRow( e.RowIndex ) )
                e.DoDefault = false;
        }

        private void Grid_RowHdrMouseUp( object sender, iGRowHdrMouseUpEventArgs e )
        {
            if( !Grid.RowMode && IsNewRow( e.RowIndex ) )
                e.DoDefault = false;
        }

        private void Grid_CellMouseDown( object sender, iGCellMouseDownEventArgs e )
        {
            if( e.Button == MouseButtons.Right )
            {
                e.DoDefault = false;
                if( Grid.SelectedRows.Count == 0 && Grid.SelectedCells.Count <= 1 )
                {
                    // if one or no cells are selected then select the cell at the location
                    Grid.PerformAction( iGActions.DeselectAllCells );
                    Grid.Cells[e.RowIndex, e.ColIndex].Selected = true;
                }
            }
            else if( e.Button == MouseButtons.Left )
            {
                Grid.PerformAction( iGActions.DeselectAllCells );
                Grid.PerformAction( iGActions.DeselectAllRows );
            }
        }

        private void Grid_CellMouseUp( object sender, iGCellMouseUpEventArgs e )
        {
            if( !Grid.ReadOnly )
            {
                if( e.Button != MouseButtons.Right )
                {
                    if( e.RowIndex == _nCurEditRow )
                    {
                        Field field = _table.Fields[e.ColIndex];
                        iGRow row = Grid.Rows[e.RowIndex];
                        if( row.ReadOnly != iGBool.True && !field.IsArray )
                        {
                            Grid.SetCurCell( e.RowIndex, e.ColIndex );
                            Grid.RequestEditCurCell( (char) 0 );
                        }
                    }
                }
            }
        }

        private void Grid_CurRowChanged( object sender, EventArgs e )
        {
            if( !_bBeginEditNewRow )
            {
                if( !CommitEditRow( false ) )
                {
                    _bBeginEditNewRow = true;
                    try
                    {
                        Debug.Assert( _nCurEditRow != -1 );
                        Grid.SetCurCell( _nCurEditRow, 0 );
                    }
                    finally
                    {
                        _bBeginEditNewRow = false;
                    }
                }
            }
        }

        private void CtxMnuGrid_Opening( object sender, CancelEventArgs e )
        {
            try
            {
                foreach( ToolStripItem mnuItem in CtxMnuGrid.Items )
                {
                    mnuItem.Enabled = false;
                    if( _table == null )
                        continue;

                    switch( mnuItem.Name )
                    {
                        case "MnuEditArrayValues":
                        {
                            if( Grid.SelectedCells.Count == 1 )
                            {
                                Field field = _table.Fields[Grid.SelectedCells[0].ColIndex];
                                mnuItem.Enabled = field.IsArray && field.DataType != DataTypeEnum.Byte;
                            }
                        }
                        break;

                        case "MnuCopySelected":
                        {
                            // if there's only one cell selected and its a Byte Array then don't enable

                            if( Grid.SelectedCells.Count > 0 )
                            {
                                int maxColIdx = -1;

                                // we must go through one complete row of selected cells

                                foreach( iGCell cell in Grid.SelectedCells )
                                {
                                    // have we started a new row?
                                    if( cell.ColIndex < maxColIdx )
                                        break;

                                    maxColIdx = cell.ColIndex;

                                    Field field = _table.Fields[cell.ColIndex];

                                    if( field.DataType == DataTypeEnum.Byte && field.IsArray && Grid.SelectedCells.Count == 1 )
                                    {
                                        mnuItem.Enabled = false;
                                        break;
                                    }
                                    else
                                        mnuItem.Enabled = true;
                                }
                            }
                            else if( Grid.SelectedRows.Count > 0 )
                                mnuItem.Enabled = true;
                        }
                        break;

                        case "MnuSaveBinaryData":
                        {
                            if( Grid.SelectedCells.Count == 1 )
                            {
                                Field field = _table.Fields[Grid.SelectedCells[0].ColIndex];
                                mnuItem.Enabled = field.DataType == DataTypeEnum.Byte && field.IsArray;
                            }
                        }
                        break;

                        case "MnuImportBinaryData":
                        {
                            if( Grid.SelectedCells.Count == 1 )
                            {
                                Field field = _table.Fields[Grid.SelectedCells[0].ColIndex];
                                mnuItem.Enabled = field.DataType == DataTypeEnum.Byte && field.IsArray;
                            }
                        }
                        break;

                        case "MnuCreateNewDb":
                        {
                            mnuItem.Enabled = true;
                        }
                        break;
                    }
                }
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Exclamation );
            }
        }

        private void MnuEditArrayValues_Click( object sender, EventArgs e )
        {
            try
            {
                iGCell cell = Grid.SelectedCells[0];
                Field field = _table.Fields[cell.ColIndex];
                Debug.Assert( field.IsArray );
                
                ArrayHolder arrayHolder = (ArrayHolder) cell.Value;
                object arrayValues = arrayHolder == null ? null : arrayHolder.Array;
                var editArrayFrm = new EditArrayFrm( arrayValues, field.DataType );

                if( editArrayFrm.ShowDialog( this ) == DialogResult.OK )
                {
                    // are they both null? then nothing was done and nothing to do
                    if( arrayValues == null && editArrayFrm.Array == null )
                        return;

                    if( _editRow == null )
                    {
                        beginRowEdit( cell.RowIndex );
                    }
                    arrayValues = editArrayFrm.Array;
                    _editRow[cell.ColIndex] = arrayValues;
                    if( arrayHolder == null )
                        arrayHolder = new ArrayHolder( arrayValues, field.DataType );
                    else
                        arrayHolder.Array = arrayValues;
                    Grid.Cells[cell.RowIndex, cell.ColIndex].Value = arrayHolder;

                    iGCol col = Grid.Cols[cell.ColIndex];
                    iGDropDownList dropList = col.CellStyle.DropDownControl as iGDropDownList;
                    setDropListValues( dropList, field, arrayHolder );
                }
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Exclamation );
            }
        }

        private void MnuCopySelected_Click( object sender, EventArgs e )
        {
            try
            {
                if( Grid.SelectedCells.Count > 0 || Grid.SelectedRows.Count > 0 )
                {
                    int nCurRow=-1;
                    bool bNewLine = true;

                    StringBuilder sb = new StringBuilder();

                    if( Grid.SelectedRows.Count > 0 )
                    {
                        foreach( iGRow row in Grid.SelectedRows )
                        {
                            foreach( iGCell cell in row.Cells )
                            {
                                AddCopyCell( cell, sb, ref nCurRow, ref bNewLine );
                            }
                        }
                    }
                    else
                    {
                        foreach( iGCell cell in Grid.SelectedCells )
                        {
                            AddCopyCell( cell, sb, ref nCurRow, ref bNewLine );
                        }
                    }
                    if( sb.Length > 0 )
                        Clipboard.SetText( sb.ToString(), TextDataFormat.UnicodeText );
                }
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Exclamation );
            }
        }

        //---------------------------------------------------------------------
        void AddCopyCell( iGCell cell, StringBuilder sb, ref int nCurRow, ref bool bNewLine )
        {
            Field field = _table.Fields[cell.ColIndex];
            if( field.DataType == DataTypeEnum.Byte && field.IsArray )
                return;

            if( cell.RowIndex != nCurRow )
            {
                if( nCurRow > -1 )
                    sb.Append( Environment.NewLine );
                bNewLine = true;
                nCurRow = cell.RowIndex;
            }
            if( !bNewLine )
                sb.Append( '\t' );
            else
                bNewLine = false;

            object value = cell.Value;

            if( value != null )
            {
                if( field.IsArray )
                {
                    var arrayHolder = value as ArrayHolder;
                    int n=0;
                    switch( arrayHolder.DataType)
                    {
                        case DataTypeEnum.Bool:
                        {
                            bool[] arr = (bool[]) arrayHolder.Array;
                            foreach( bool val in arr )
                            {
                                if( n++ > 0 ) sb.Append( '\t' );
                                sb.Append( val.ToString() );                                
                            }
                        }
                        break;

                        case DataTypeEnum.DateTime:
                        {
                            DateTime[] arr = (DateTime[]) arrayHolder.Array;
                            foreach( DateTime val in arr )
                            {
                                if( n++ > 0 ) sb.Append( '\t' );
                                sb.Append( val.ToString() );
                            }
                        }
                        break;

                        case DataTypeEnum.Double:
                        {
                            double[] arr = (double[]) arrayHolder.Array;
                            foreach( double val in arr )
                            {
                                if( n++ > 0 ) sb.Append( '\t' );
                                sb.Append( val.ToString() );
                            }
                        }
                        break;

                        case DataTypeEnum.Int32:
                        {
                            Int32[] arr = (Int32[]) arrayHolder.Array;
                            foreach( Int32 val in arr )
                            {
                                if( n++ > 0 ) sb.Append( '\t' );
                                sb.Append( val.ToString() );
                            }
                        }
                        break;

                        case DataTypeEnum.String:
                        {
                            string[] arr = (string[]) arrayHolder.Array;
                            foreach( string val in arr )
                            {
                                if( n++ > 0 ) sb.Append( '\t' );
                                sb.Append( val );
                            }
                        }
                        break;
                    }
                }
                else
                    sb.Append( value.ToString() );
            }
        }

        private void MnuSaveBinaryData_Click( object sender, EventArgs e )
        {
            try
            {
                int nCount = Grid.SelectedCells.Count;
                if( nCount != 1 )
                    return;

                object value = Grid.SelectedCells[0].Value;
                var arrayHolder = value as ArrayHolder;
                if( arrayHolder == null )
                    return;

                Debug.Assert( arrayHolder.DataType == DataTypeEnum.Byte );

                byte[] arr = (byte[]) arrayHolder.Array;

                OpenFileDialog fileDlg = new OpenFileDialog();
                fileDlg.SupportMultiDottedExtensions = true;
                fileDlg.CheckFileExists = false;

                if( fileDlg.ShowDialog() == DialogResult.OK )
                {
                    using( FileStream fStream = new FileStream( fileDlg.FileName, FileMode.Create, FileAccess.Write ) )
                    {
                        BinaryWriter writer = new BinaryWriter( fStream );
                        writer.Write( arr );
                        writer.Flush();
                    }
                }
            }
            catch( Exception ex )
            {
                MessageBox.Show( this, ex.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Exclamation );
            }
        }

        private void MnuImportBinaryData_Click( object sender, EventArgs e )
        {
            try
            {
                int nCount = Grid.SelectedCells.Count;
                if( nCount != 1 )
                    return;

                OpenFileDialog fileDlg = new OpenFileDialog();
                fileDlg.SupportMultiDottedExtensions = true;
                fileDlg.CheckFileExists = true;

                if( fileDlg.ShowDialog() == DialogResult.OK )
                {
                    byte[] bytes = File.ReadAllBytes( fileDlg.FileName );

                    // set edit mode and the cell and table value

                    iGCell cell = Grid.SelectedCells[0];
                    if( _editRow == null )
                    {
                        beginRowEdit( cell.RowIndex );
                    }

                    object value = cell.Value;
                    ArrayHolder arrayHolder;
                    if( value == null )
                        arrayHolder = new ArrayHolder( bytes, DataTypeEnum.Byte );
                    else
                        arrayHolder = value as ArrayHolder;

                    _editRow[cell.ColIndex] = bytes;
                    arrayHolder.Array = bytes;
                    cell.Value = arrayHolder;
                }
            }
            catch( Exception ex )
            {
                MessageBox.Show( this, ex.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Exclamation );
            }
        }

        // this is called if you're in the middle of editing a field and hit Escape
        //
        private void Grid_CancelEdit( object sender, iGCancelEditEventArgs e )
        {
        }

        private void MnuCreateNewDb_Click( object sender, EventArgs e )
        {
            try
            {
                string filename = MainFrm.GetDbFilename( false );

                if( filename != null )
                {
                    FileDb db = _table.SaveToDb( filename );
                    db.Dispose();
                }
            }
            catch( Exception ex )
            {
                MessageBox.Show( this, ex.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Exclamation );
            }
        }

        private void Grid_CustomDrawRowHdrForeground( object sender, iGCustomDrawRowHdrForegroundEventArgs e )
        {
            if( e.RowIndex >= 0 )
            {
                // Draw the row numbers

                iGRow row = Grid.Rows[e.RowIndex];
                /*
                row.HdrBounds.Inflate( -2, -2 );
                Rectangle bounds = row.HdrBounds;
                bounds.X += 1;
                bounds.Y += 1;
                e.Graphics.FillRectangle( SystemBrushes.Control, bounds );
                */
                e.Graphics.DrawString(
                    (e.RowIndex + 1).ToString(),
                    Font,
                    SystemBrushes.ControlText,
                    new Rectangle( row.HdrBounds.X + 2, row.HdrBounds.Y, row.HdrBounds.Width - 2, row.HdrBounds.Height ) );
                e.DoDefault = false;
            }
        }
    }
}
