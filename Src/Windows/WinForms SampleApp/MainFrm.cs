using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

using FileDbNs;

namespace SampleApp
{
    public partial class MainFrm : Form
    {
        FileDb _db = new FileDb();

        const string StrDbName = "Employees.fdb";

        const string StrEncryptionKey = "my encryption key";

        const string StrNorthwindRelPath = @"..\..\..\..\Northwind Database\";

        //------------------------------------------------------------------------------

        public MainFrm()
        {
            InitializeComponent();
        }

        #region DB Creation
        private void BtnClose_Click( object sender, EventArgs e )
        {
            this.Close();
        }

        private void BtnCreate_Click( object sender, EventArgs e )
        {
            try
            {
#if true
                // Add records using the project XML file

                Stream s = this.GetType().Assembly.GetManifestResourceStream( this.GetType(), "data.xml" );
                string xml;
                using( StreamReader reader = new StreamReader( s ) )
                {
                    xml = reader.ReadToEnd();
                }

                XDocument xmlDoc = XDocument.Parse(xml);

                // find the columns
                var fields = from col in xmlDoc.Descendants( "field" )
                              select new
                              {
                                  name = col.Attribute( "name" ),
                                  type = col.Attribute( "type" ),
                                  isPrimaryKey = col.Attribute( "isPrimaryKey" ),
                                  isArray = col.Attribute( "isArray" ),
                                  autoIncStart = col.Attribute( "autoIncStart" )
                              };

                Field field;
                var fieldLst = new List<Field>( 20 );

                foreach( var col in fields )
                {
                    string name = (string) col.name,
                           type = (string) col.type;
                    bool isPrimaryKey = col.isPrimaryKey == null? false : (bool) col.isPrimaryKey,
                         isArray = col.isArray == null? false : (bool) col.isArray;
                    int autoIncStart = col.autoIncStart == null ? -1 : (int) col.autoIncStart;

                    DataTypeEnum dataType = DataTypeEnum.String;

                    switch( type.ToLower() )
                    {
                        case "int":
                            dataType = DataTypeEnum.Int;
                        break;
                        case "uint":
                            dataType = DataTypeEnum.UInt;
                        break;
                        case "string":
                            dataType = DataTypeEnum.String;
                        break;
                        case "datetime":
                            dataType = DataTypeEnum.DateTime;
                        break;
                        case "bool":
                            dataType = DataTypeEnum.Bool;
                        break;
                        case "float":
                            dataType = DataTypeEnum.Float;
                        break;
                        case "double":
                            dataType = DataTypeEnum.Double;
                        break;
                        case "byte":
                            dataType = DataTypeEnum.Byte;
                        break;
                    }

                    field = new Field( name, dataType );
                    field.IsPrimaryKey = isPrimaryKey;
                    field.IsArray = isArray;
                    if( autoIncStart > -1 )
                        field.AutoIncStart = autoIncStart;

                    fieldLst.Add( field );
                }

#else
                Field field;
                var fieldLst = new List<Field>( 20 );

                field = new Field( "ID", DataTypeEnum.Int );
                field.AutoIncStart = 0;
                field.IsPrimaryKey = true;
                fieldLst.Add( field );
                field = new Field( "FirstName", DataTypeEnum.String );
                //field.IsPrimaryKey = true;
                fieldLst.Add( field );
                field = new Field( "LastName", DataTypeEnum.String );
                fieldLst.Add( field );
                field = new Field( "BirthDate", DataTypeEnum.DateTime );
                fieldLst.Add( field );
                field = new Field( "IsCitizen", DataTypeEnum.Bool );
                fieldLst.Add( field );
                field = new Field( "Float", DataTypeEnum.Float );
                fieldLst.Add( field );
                field = new Field( "Byte", DataTypeEnum.Byte );
                fieldLst.Add( field );

                // array types
                field = new Field( "StringArray", DataTypeEnum.String );
                field.IsArray = true;
                fieldLst.Add( field );
                field = new Field( "ByteArray", DataTypeEnum.Byte );
                field.IsArray = true;
                fieldLst.Add( field );
                field = new Field( "IntArray", DataTypeEnum.Int );
                field.IsArray = true;
                fieldLst.Add( field );
                field = new Field( "FloatArray", DataTypeEnum.Float );
                field.IsArray = true;
                fieldLst.Add( field );
                field = new Field( "DateTimeArray", DataTypeEnum.DateTime );
                field.IsArray = true;
                fieldLst.Add( field );
                field = new Field( "BoolArray", DataTypeEnum.Bool );
                field.IsArray = true;
                fieldLst.Add( field );
#endif

                _db.Create( StrDbName, fieldLst.ToArray() );
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }
        }

        private void BtnAddRecords_Click( object sender, EventArgs e )
        {
            try
            {
#if true
                Stream s = this.GetType().Assembly.GetManifestResourceStream( this.GetType(), "data.xml" );
                string xml;
                using( StreamReader reader = new StreamReader( s ) )
                {
                    xml = reader.ReadToEnd();
                }

                XDocument xmlDoc = XDocument.Parse(xml);

                // find the columns
                var rows = xmlDoc.Descendants( "row" );
                char[] toks = "|".ToCharArray();
                
                foreach( var row in rows )
                {
                    var columns = from col in row.Descendants( "col" )
                                  select new
                                  {
                                      value = col.Value
                                  };

                    var record = new FieldValues( 20 );
                    int idx = 0;

                    foreach( var col in columns )
                    {
                        string value = (string) col.value;
                        Field field = _db.Fields[idx];

                        if( !string.IsNullOrEmpty( value ) )
                        {
                            if( field.DataType == DataTypeEnum.Bool )
                            {
                                if( field.IsArray )
                                {
                                    var list = new List<bool>( 10 );
                                    string[] vals = value.Split( toks );
                                    foreach( string val in vals )
                                    {
                                        list.Add( bool.Parse( val ) );
                                    }
                                    record.Add( field.Name, list.ToArray() );
                                }
                                else
                                {
                                    bool bval;
                                    if( bool.TryParse( value, out bval ) )
                                        record.Add( field.Name, bval );
                                }
                            }
                            else if( field.DataType == DataTypeEnum.Byte )
                            {
                                if( field.IsArray )
                                {
                                    var list = new List<Byte>( 10 );
                                    string[] vals = value.Split( toks );
                                    foreach( string val in vals )
                                    {
                                        list.Add( Byte.Parse( val ) );
                                    }
                                    record.Add( field.Name, list.ToArray() );
                                }
                                else
                                {
                                    Byte bval;
                                    if( Byte.TryParse( value, out bval ) )
                                        record.Add( field.Name, bval );
                                }
                            }
                            else if( field.DataType == DataTypeEnum.Int )
                            {
                                if( field.IsArray )
                                {
                                    var list = new List<Int32>( 10 );
                                    string[] vals = value.Split( toks );
                                    foreach( string val in vals )
                                    {
                                        list.Add( Int32.Parse( val ) );
                                    }
                                    record.Add( field.Name, list.ToArray() );
                                }
                                else
                                {
                                    Int32 ival;
                                    if( Int32.TryParse( value, out ival ) )
                                        record.Add( field.Name, ival );
                                }
                            }
                            else if( field.DataType == DataTypeEnum.UInt )
                            {
                                if( field.IsArray )
                                {
                                    var list = new List<UInt32>( 10 );
                                    string[] vals = value.Split( toks );
                                    foreach( string val in vals )
                                    {
                                        list.Add( UInt32.Parse( val ) );
                                    }
                                    record.Add( field.Name, list.ToArray() );
                                }
                                else
                                {
                                    UInt32 ival;
                                    if( UInt32.TryParse( value, out ival ) )
                                        record.Add( field.Name, ival );
                                }
                            }
                            else if( field.DataType == DataTypeEnum.Float )
                            {
                                if( field.IsArray )
                                {
                                    var list = new List<float>( 10 );
                                    string[] vals = value.Split( toks );
                                    foreach( string val in vals )
                                    {
                                        list.Add( float.Parse( val ) );
                                    }
                                    record.Add( field.Name, list.ToArray() );
                                }
                                else
                                {
                                    float dval;
                                    if( float.TryParse( value, out dval ) )
                                        record.Add( field.Name, dval );
                                }
                            }
                            else if( field.DataType == DataTypeEnum.Double )
                            {
                                if( field.IsArray )
                                {
                                    var list = new List<double>( 10 );
                                    string[] vals = value.Split( toks );
                                    foreach( string val in vals )
                                    {
                                        list.Add( double.Parse( val ) );
                                    }
                                    record.Add( field.Name, list.ToArray() );
                                }
                                else
                                {
                                    double dval;
                                    if( double.TryParse( value, out dval ) )
                                        record.Add( field.Name, dval );
                                }
                            }
                            else // DateTime or String - DateTime is convertable to string in FileDb
                            {
                                if( field.IsArray )
                                {
                                    var list = new List<string>( 10 );
                                    string[] vals = value.Split( toks );
                                    foreach( string val in vals )
                                    {
                                        list.Add( val );
                                    }
                                    record.Add( field.Name, list.ToArray() );
                                }
                                else
                                {
                                    record.Add( field.Name, value );
                                }
                            }
                        }

                        idx++;
                    }

                    _db.AddRecord( record );
                }

#else                            
                var record = new FieldValues( 20 );
                record.Add( "FirstName", "Nancy" );
                record.Add( "LastName", "Davolio" );
                record.Add( "BirthDate", new DateTime( 1968, 12, 8 ) );
                record.Add( "IsCitizen", true );
                record.Add( "Float", 1.23 );
                record.Add( "Byte", 1 );
                record.Add( "StringArray", new string[] { "s1", "s2", "s3" } );
                record.Add( "ByteArray", new Byte[] { 1, 2, 3, 4 } );
                record.Add( "IntArray", new int[] { 100, 200, 300, 400 } );
                record.Add( "FloatArray", new double[] { 1.2, 2.4, 3.6, 4.8 } );
                record.Add( "DateTimeArray", new DateTime[] { DateTime.Now, DateTime.Now, DateTime.Now, DateTime.Now } );
                record.Add( "BoolArray", new bool[] { true, false, true, false } );
                _db.AddRecord( record );

                record = new FieldValues( 20 );
                record.Add( "FirstName", "Andrew" );
                record.Add( "LastName", "Fuller" );
                record.Add( "BirthDate", new DateTime( 1962, 1, 12 ) );
                record.Add( "IsCitizen", true );
                record.Add( "Float", 2.567 );
                record.Add( "Byte", 2 );
                _db.AddRecord( record );

                record = new FieldValues( 20 );
                record.Add( "FirstName", "Janet" );
                record.Add( "LastName", "Leverling" );
                record.Add( "BirthDate", new DateTime( 1963, 8, 30 ) );
                record.Add( "IsCitizen", false );
                record.Add( "Float", 3.14 );
                record.Add( "Byte", 3 );
                _db.AddRecord( record );

                record = new FieldValues( 20 );
                record.Add( "FirstName", "Margaret" );
                record.Add( "LastName", "Peacock" );
                record.Add( "BirthDate", new DateTime( 1957, 9, 19 ) );
                record.Add( "IsCitizen", false );
                record.Add( "Float", 4.96 );
                record.Add( "Byte", 4 );
                _db.AddRecord( record );

                record = new FieldValues( 20 );
                record.Add( "FirstName", "Steven" );
                record.Add( "LastName", "Buchanan" );
                record.Add( "BirthDate", new DateTime( 1965, 3, 4 ) );
                record.Add( "IsCitizen", true );
                record.Add( "Float", 5.7 );
                record.Add( "Byte", 5 );
                _db.AddRecord( record );
#endif
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }
        }

        private void btnOpen_Click( object sender, EventArgs e )
        {
            try
            {
                _db.Open( StrDbName, false );
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }
        }

        private void BtnOpenEncrypted_Click( object sender, EventArgs e )
        {
            // Note:  To use encryption, you must follow these steps:
            // 1.  Create the database
            // 2.  Open the database with Encryption
            // 3.  Add records

            try
            {
                _db.Open( StrDbName, false );
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }
        }

        private void BtnCloseDb_Click( object sender, EventArgs e )
        {
            try
            {
                _db.Close();
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }
        }

        private void BtnDrop_Click( object sender, EventArgs e )
        {
            try
            {
                _db.Drop( StrDbName );
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }
        }
        #endregion DB Creation

        #region Searching
        private void BtnGetAllRecordsSorted_Click( object sender, EventArgs e )
        {
            try
            {
                // get all records sorted by LastName, Firstname.  Notice how prefix the fieldname with ~
                // in the OrderBy list to get case-insensitive sort

                FileDbNs.Table table = _db.SelectAllRecords( null, new string[] { "~LastName", "Firstname" } );
                displayRecords( table );
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }
        }

        private void BtnGetAllRecordsReverseSort_Click( object sender, EventArgs e )
        {
            try
            {
                // get all records sorted by LastName DESC, Firstname DESC
                // to get a reverse sort, prefix with !
                // to get a case-insensitive sort, prefix with ~

                FileDbNs.Table table = _db.SelectAllRecords( new string[] { "ID", "Firstname", "LastName" }, 
                    new string[] { "!~LastName", "~FirstName" } );
                displayRecords( table );
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }
        }

        private void BtnGetRecordsRegex_Click( object sender, EventArgs e )
        {
            try
            {
                string searchVal = @"\bFull";
                var fieldSearchExp = new FilterExpression( "LastName", searchVal, EqualityEnum.Like );

                FileDbNs.Table table = _db.SelectRecords( fieldSearchExp );
                displayRecords( table );
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }
        }

        private void BtnGetMatchingRecords_Click( object sender, EventArgs e )
        {
            try
            {
                // Use the FilterExpressionGroup's filter parser to create a FilterExpressionGroup
                // The syntax is similar to SQL (do not preceed with WHERE)
                // Note that we prefix the fieldname with ~ to get a case-INSENSITIVE search
                // (FileDb doesn't currently support UPPER or LOWER)
                // Note that we can also use LIKE when we want to ignore case, but the difference
                // is that LIKE will create a RegEx search which would be a little slower
                // Note also that each set of parentheses will create a child FilterExpressionGroup

                string filter = "(~FirstName = 'steven' OR [FirstName] LIKE 'NANCY') AND LastName = 'Fuller'";

                FilterExpressionGroup filterExpGrp = FilterExpressionGroup.Parse( filter );
                FileDbNs.Table table = _db.SelectRecords( filterExpGrp );
                displayRecords( table );

                // we can manually build the same FilterExpressionGroup
                var fname1Exp = new FilterExpression( "FirstName", "steven", EqualityEnum.Equal, MatchTypeEnum.IgnoreCase );
                // the following two lines produce the same FilterExpression
                var fname2Exp = FilterExpression.Parse( "FirstName LIKE 'NANCY'" );
                fname2Exp = new FilterExpression( "FirstName", "NANCY", EqualityEnum.Like );
                var lnameExp = new FilterExpression( "LastName", "Fuller", EqualityEnum.Equal, MatchTypeEnum.UseCase );

                var fnamesGrp = new FilterExpressionGroup();
                fnamesGrp.Add( BoolOpEnum.Or, fname1Exp );
                fnamesGrp.Add( BoolOpEnum.Or, fname2Exp );
                var allNamesGrp = new FilterExpressionGroup();
                allNamesGrp.Add( BoolOpEnum.And, lnameExp );
                allNamesGrp.Add( BoolOpEnum.And, fnamesGrp );

                table = _db.SelectRecords( allNamesGrp );
                displayRecords( table );

                // or just pass the string expression directly

                table = _db.SelectRecords( filter );
                displayRecords( table );
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }
        }

        private void BtnGetRecordByIndex_Click( object sender, EventArgs e )
        {
            try
            {
                // first find a record and get its index

                FilterExpression fieldSearchExp = FilterExpression.Parse( "LastName = Buchanan" );
                FileDbNs.Table table = _db.SelectRecords( fieldSearchExp, null, null, true );
                if( table.Count > 0 )
                {
                    int index = (int) table[0]["index"];
                    Record record = _db.GetRecordByIndex( index, null );
                    Records records = new Records( 1 );
                    records.Add( record );
                    displayRecords( records );
                }
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }
        }

        private void BtnGetRecordByKey_Click( object sender, EventArgs e )
        {
            try
            {
                Record record = _db.GetRecordByKey( 1, new string[] { "ID", "Firstname", "LastName" }, false );
                if( record != null )
                {
                    Records records = new Records( 1 );
                    records.Add( record );
                    displayRecords( records );
                }
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }
        }

        #endregion Searching

        #region Update functions

        private void BtnUpdateMatchingRecord_Click( object sender, EventArgs e )
        {
            try
            {
                FilterExpression fieldSearchExp = FilterExpression.Parse( "LastName = 'Peacock'" );
                FileDbNs.Table table = _db.SelectRecords( fieldSearchExp, new string[] { "ID", "Float" }, null, true );

                if( table.Count > 0 )
                {
                    Record record = table[0];
                    FieldValues fieldValues = record.GetFieldValues();
                    fieldValues["Float"] = 5.0;

                    // Note: Its actually more efficient to use a full record (one with all fields)
                    // if you have one because otherwise the full record will be read internally anyway

                    _db.UpdateRecordByIndex( (int) record["index"], fieldValues );

                    table = _db.SelectRecords( fieldSearchExp );
                    displayRecords( table );
                }
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }
        }

        private void BtnUpdateMatchingRecords_Click( object sender, EventArgs e )
        {
            try
            {
                string filter = "(~FirstName = 'andrew' OR ~FirstName = 'nancy') AND ~LastName = 'fuller'";

                FilterExpressionGroup filterExpGrp = FilterExpressionGroup.Parse( filter );
                FileDbNs.Table table = _db.SelectRecords( filterExpGrp );
                displayRecords( table );

                // equivalent building it manually
                var fname1Exp = new FilterExpression( "FirstName", "andrew", EqualityEnum.Equal, MatchTypeEnum.IgnoreCase );
                // the following two lines produce the same FilterExpression
                var fname2Exp = FilterExpression.Parse( "~FirstName = nancy" );
                fname2Exp = new FilterExpression( "FirstName", "nancy", EqualityEnum.Equal, MatchTypeEnum.IgnoreCase );
                var lnameExp = new FilterExpression( "LastName", "fuller", EqualityEnum.Equal, MatchTypeEnum.IgnoreCase );

                var fnamesGrp = new FilterExpressionGroup();
                fnamesGrp.Add( BoolOpEnum.Or, fname1Exp );
                fnamesGrp.Add( BoolOpEnum.Or, fname2Exp );
                var allNamesGrp = new FilterExpressionGroup();
                allNamesGrp.Add( BoolOpEnum.And, lnameExp );
                allNamesGrp.Add( BoolOpEnum.And, fnamesGrp );
                table = _db.SelectRecords( allNamesGrp );
                displayRecords( table );

                // now update the matching Record
                var fieldValues = new FieldValues();
                fieldValues.Add( "IsCitizen", false );
                int nRecs = _db.UpdateRecords( allNamesGrp, fieldValues );

                table = _db.SelectRecords( allNamesGrp );
                displayRecords( table );

                // or easiest of all use the filter string directly
                table = _db.SelectRecords( filter );
                displayRecords( table );
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }
        }

        private void BtnUpdateRecordsRegex_Click( object sender, EventArgs e )
        {
            try
            {
                string searchVal = @"\bFull";
                var fieldSearchExp = new FilterExpression( "LastName", searchVal, EqualityEnum.Like );

                var fieldValues = new FieldValues();
                fieldValues.Add( "IsCitizen", true );
                int nRecs = _db.UpdateRecords( fieldSearchExp, fieldValues );

                FileDbNs.Table table = _db.SelectRecords( fieldSearchExp );
                displayRecords( table );
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }
        }

        // Use FileDb.EncryptString/DecryptString to encrypt/decrypt field values without
        // encrypting the entire database.
        //
        private void BtnEncryptDecryptValue_Click( object sender, EventArgs e )
        {
            try
            {
                string value = _db.EncryptString( StrEncryptionKey, "A value to encrypt" );
                // you would set the value into the database here

                // you would get the value from the database and decrypt it
                value = _db.DecryptString( StrEncryptionKey, value );
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }
        }
        #endregion Update functions

        #region Table Copy functions

        /// <summary>
        /// Here we see how to convert a Table directly to a new database just by calling Table.SaveToDb. 
        /// </summary>
        ///
        private void BtnTableToDB_Click( object sender, EventArgs e )
        {
            try
            {
                FileDb employeesDb = new FileDb();
                string path = System.IO.Path.Combine( AppDomain.CurrentDomain.BaseDirectory, StrNorthwindRelPath );

                try
                {
                    employeesDb.Open( path + "Employees.fdb", false );
                    string searchVal = @"\bFull";
                    var fieldSearchExp = new FilterExpression( "LastName", searchVal, EqualityEnum.Like );

                    FileDbNs.Table table = employeesDb.SelectRecords( fieldSearchExp );

                    FileDb newDb = table.SaveToDb( "Employees2.fdb" );
                    table = newDb.SelectRecords( fieldSearchExp );
                    displayRecords( table );
                    newDb.Close();
                }
                finally
                {
                    employeesDb.Close();
                }
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }
        }

        /// <summary>
        /// One very powerful feature of FileDb is the ability to select records
        /// from a Table object.  This would be very handy if you have a Table
        /// and you want to perform Select operations against it directly rather
        /// than the database.
        /// </summary>
        /// 
        private void BtnTableFromTable_Click( object sender, EventArgs e )
        {
            try
            {
                FileDb customersDb = new FileDb();
                string path = System.IO.Path.Combine( AppDomain.CurrentDomain.BaseDirectory, StrNorthwindRelPath );

                try
                {
                    customersDb.Open( path + "Customers.fdb", false );

                    FileDbNs.Table customers = customersDb.SelectAllRecords();

                    FilterExpression filterExp = FilterExpression.Parse( "CustomerID <> 'ALFKI'" );

                    FileDbNs.Table subCusts = customers.SelectRecords( filterExp, // "CustomerID <> 'ALFKI'",
                        new string[] { "CustomerID", "CompanyName", "City" }, new string[] { "~City", "~CompanyName" } );

                    displayRecords( subCusts );
                }
                finally
                {
                    customersDb.Close();
                }
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }
        }

        #endregion Table Copy functions

        #region Remove functions
        private void BtnRemoveByKey_Click( object sender, EventArgs e )
        {
            try
            {
                var exp1 = new FilterExpression( "FirstName", "Nancy", EqualityEnum.Equal, MatchTypeEnum.IgnoreCase );
                var exp2 = new FilterExpression( "LastName", "Leverling", EqualityEnum.Equal, MatchTypeEnum.IgnoreCase );
                var expGrp = new FilterExpressionGroup();
                expGrp.Add( BoolOpEnum.Or, exp1 );
                expGrp.Add( BoolOpEnum.Or, exp2 );

                FileDbNs.Table table = _db.SelectRecords( expGrp, new string[] { "ID" } );

                foreach( Record record in table )
                {
                    int id = (int) record["ID"];
                    bool bRet = _db.DeleteRecordByKey( id );
                }
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }
        }

        private void BtnRemoveByIndex_Click( object sender, EventArgs e )
        {
            try
            {
                string searchVal = "Buchanan";

                FilterExpression fieldSearchExp = new FilterExpression( "LastName", searchVal, EqualityEnum.Equal, MatchTypeEnum.UseCase );
                FileDbNs.Table table = _db.SelectRecords( fieldSearchExp, null, null, true );

                Record record = table[0];
                _db.DeleteRecordByIndex( (int) record["index"] );
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }
        }

        private void BtnRemoveByValue1_Click( object sender, EventArgs e )
        {
            try
            {
                FilterExpression searchExp = new FilterExpression( "FirstName", "Nancy", EqualityEnum.Equal, MatchTypeEnum.UseCase );
                int numDeleted = _db.DeleteRecords( searchExp );
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }
        }

        private void BtnRemoveByValue2_Click( object sender, EventArgs e )
        {
            try
            {
                var lnameExp = new FilterExpression( "LastName", "peacock", EqualityEnum.Equal, MatchTypeEnum.IgnoreCase );
                var fname1Exp = new FilterExpression( "FirstName", "nancy", EqualityEnum.Equal, MatchTypeEnum.IgnoreCase );

                var allNamesGrp = new FilterExpressionGroup();
                allNamesGrp.Add( BoolOpEnum.Or, lnameExp );
                allNamesGrp.Add( BoolOpEnum.Or, fname1Exp );

                int numDeleted = _db.DeleteRecords( allNamesGrp );
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }
        }
        #endregion Remove functions

        #region Misc functions

        private void BtnClean_Click( object sender, EventArgs e )
        {
            try
            {
                _db.Clean();
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }
        }

        private void BtnNumRecords_Click( object sender, EventArgs e )
        {
            try
            {
                MessageBox.Show( String.Format( "NumRecords: {0}", _db.NumRecords.ToString() ) );
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }
        }

        private void BtnNumDeleted_Click( object sender, EventArgs e )
        {
            try
            {
                MessageBox.Show( String.Format( "NumDeleted: {0}", _db.NumDeleted.ToString() ) );
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }
        }

        private void BtnIterate_Click( object sender, EventArgs e )
        {
            try
            {
                if( _db.NumRecords > 0 )
                {
                    // we'll keep all the rows we find and display them in the DataGrid
                    Records records = new Records( _db.NumRecords );

                    _db.MoveFirst();

                    do
                    {
                        Record record = _db.GetCurrentRecord( null, true );
                        records.Add( record );

                        foreach( string fieldName in record.FieldNames )
                        {
                            Debug.WriteLine( string.Format( "Field: {0}  Value: {1}", fieldName, record[fieldName] ) );
                        }

                        // the index field won't be in the database Fields list, because it is not part of the database (table)
                        foreach( Field field in _db.Fields )
                        {
                            Debug.WriteLine( string.Format( "Field: {0}  Value: {1}", field.Name, record[field.Name] ) );
                        }

                    } while( _db.MoveNext() );

                    displayRecords( records );
                }
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }
        }

        private void BtnReindex_Click( object sender, EventArgs e )
        {
            try
            {
                _db.Reindex();
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }
        }
        #endregion Misc functions

        #region Linq functions

        /// <summary>
        /// Use Linq to Objects to get an anonymous list of selected Employees from the database.
        /// </summary>
        ///
        private void BtnLinqSelect_Click( object sender, EventArgs e )
        {
            try
            {
                FileDb employeesDb = new FileDb();

                string path = System.IO.Path.Combine( AppDomain.CurrentDomain.BaseDirectory, StrNorthwindRelPath );
                employeesDb.Open( path + "Employees.fdb", true );

                try
                {
                    FilterExpression filterExp = FilterExpression.Parse( "LastName in ('Fuller', 'Peacock')" );
                    FileDbNs.Table customers = employeesDb.SelectRecords( filterExp );

                    var query =
                        from record in customers
                        select new
                        {
                            ID = record["EmployeeId"],
                            Name = record["FirstName"] + " " + record["LastName"],
                            Title = record["Title"]
                        };

                    foreach( var rec in query )
                    {
                        Debug.WriteLine( rec.ToString() );
                    }
                }
                finally
                {
                    employeesDb.Close();
                }
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }
        }

        /// <summary>
        /// Use Linq to Objects to join 4 database tables to get an anonymous list of CustomerOrderDetails objects.
        /// The list is a flat set of rows, analogous to a INNER JOIN table.
        /// </summary>
        ///
        private void BtnLinqJoin_Click( object sender, EventArgs e )
        {
            try
            {
                FileDb customersDb = new FileDb(),
                       ordersDb = new FileDb(),
                       orderDetailsDb = new FileDb(),
                       productsDb = new FileDb();

                string path = System.IO.Path.Combine( AppDomain.CurrentDomain.BaseDirectory, StrNorthwindRelPath );

                customersDb.Open( path + "Customers.fdb", true );
                ordersDb.Open( path + "Orders.fdb", true );
                orderDetailsDb.Open( path + "OrderDetails.fdb", true );
                productsDb.Open( path + "Products.fdb", true );

                try
                {
                    // Step 1: Get the Tables we will use with Linq
                    // Note: we only get the columns we will be using so as to keep the size of the table data small

                    // get all Customer records
                    FilterExpression filterExp;
                    FileDbNs.Table customers = customersDb.SelectAllRecords( new string[] { "CustomerID", "CompanyName" } );

                    // now get only Order records for the target Customer records using FilterExpression.CreateInExpressionFromTable
                    // this method creates a HastSet which is extremely efficient when used by FileDb.SelectRecords

                    filterExp = FilterExpression.CreateInExpressionFromTable( "CustomerID", customers, "CustomerID" );
                    FileDbNs.Table orders = ordersDb.SelectRecords( filterExp, new string[] { "CustomerID", "OrderID", "OrderDate" } );

                    // now get only OrderDetails records for the target Order records
                    filterExp = FilterExpression.CreateInExpressionFromTable( "OrderID", orders, "OrderID" );
                    FileDbNs.Table orderDetails = orderDetailsDb.SelectRecords( filterExp, new string[] { "OrderID", "ProductID", "UnitPrice", "Quantity" } );

                    // now get only Product records for the target OrderDetails records
                    filterExp = FilterExpression.CreateInExpressionFromTable( "ProductID", orderDetails, "ProductID" );
                    FileDbNs.Table products = productsDb.SelectRecords( filterExp, new string[] { "ProductID", "ProductName" } );

                    // produces a flat/normalized sequence

                    var custOrderDetails =
                        from c in customers
                        join o in orders on c["CustomerID"] equals o["CustomerID"]
                        join od in orderDetails on o["OrderID"] equals od["OrderID"]
                        join p in products on od["ProductID"] equals p["ProductID"]
                        select new
                        {
                            ID = c["CustomerID"],
                            CompanyName = c["CompanyName"],
                            OrderID = o["OrderID"],
                            OrderDate = o["OrderDate"],
                            ProductName = p["ProductName"],
                            UnitPrice = od["UnitPrice"],
                            Quantity = od["Quantity"]
                        };

                    foreach( var custOrderDetail in custOrderDetails )
                    {
                        Debug.WriteLine( custOrderDetail.ToString() );
                    }
                }
                finally
                {
                    customersDb.Close();
                    ordersDb.Close();
                    orderDetailsDb.Close();
                    productsDb.Close();
                }
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }
        }

        /// <summary>
        /// Using Linq, we can group by Key fields.  In this example, we will group Employees by Country.
        /// </summary>
        ///
        private void BtnLinqGroupBy_Click( object sender, EventArgs e )
        {
            try
            {
                FileDb employeesDb = new FileDb();

                string path = System.IO.Path.Combine( AppDomain.CurrentDomain.BaseDirectory, StrNorthwindRelPath );
                employeesDb.Open( path + "Employees.fdb", true );

                try
                {
                    FileDbNs.Table customers = employeesDb.SelectAllRecords();

                    var employeeGrps =
                        from c in customers
                        group c by c["Country"] into custGrp
                        select new
                        {
                            Country = custGrp.Key,
                            Employees = custGrp     // this will contain a list of FileDb.Record objects
                        };

                    foreach( var grp in employeeGrps )
                    {
                        Debug.WriteLine( grp.Country );

                        foreach( var employee in grp.Employees )
                        {
                            // custRec is a FileDb.Record object
                            Debug.WriteLine( employee["FirstName"] + " " + employee["LastName"] + " - " + employee["Title"] );
                        }
                    }
                }
                finally
                {
                    employeesDb.Close();
                }
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }
        }

        /// <summary>
        /// Linq can produce hierarchical object graphs from relational data.
        /// In this example we want to get a hierarchical list of Customer objects, each of which has a list of Order objects,
        /// with each Order object having a list of OrderDetails objects.
        /// </summary>
        /// 
        private void BtnLinqHierarchicalObjects_Click( object sender, EventArgs e )
        {
            try
            {
                FileDb customersDb = new FileDb(),
                       ordersDb = new FileDb(),
                       orderDetailsDb = new FileDb(),
                       productsDb = new FileDb();

                string path = System.IO.Path.Combine( AppDomain.CurrentDomain.BaseDirectory, StrNorthwindRelPath );

                customersDb.Open( path + "Customers.fdb", true );
                ordersDb.Open( path + "Orders.fdb", true );
                orderDetailsDb.Open( path + "OrderDetails.fdb", true );
                productsDb.Open( path + "Products.fdb", true );

                try
                {
                    // Step 1: Get the Tables we will use with Linq
                    // Note: we only get the columns we will be using so as to keep the size of the table data small

                    // get all Customer records
                    FilterExpression filterExp;
                    FileDbNs.Table customers = customersDb.SelectAllRecords( new string[] { "CustomerID", "CompanyName" } );

                    // now get only Order records for the target Customer records using FilterExpression.CreateInExpressionFromTable
                    // this method creates a HastSet which is extremely efficient when used by FileDb.SelectRecords

                    filterExp = FilterExpression.CreateInExpressionFromTable( "CustomerID", customers, "CustomerID" );
                    FileDbNs.Table orders = ordersDb.SelectRecords( filterExp, new string[] { "CustomerID", "OrderID", "OrderDate" } );

                    // now get only OrderDetails records for the target Order records
                    filterExp = FilterExpression.CreateInExpressionFromTable( "OrderID", orders, "OrderID" );
                    FileDbNs.Table orderDetails = orderDetailsDb.SelectRecords( filterExp, new string[] { "OrderID", "ProductID", "UnitPrice", "Quantity" } );

                    // now get only Product records for the target OrderDetails records
                    filterExp = FilterExpression.CreateInExpressionFromTable( "ProductID", orderDetails, "ProductID" );
                    FileDbNs.Table products = productsDb.SelectRecords( filterExp, new string[] { "ProductID", "ProductName" } );


                    // Step 2:  Use Linq to generate the objects for us

                    var customerOrders =
                        from c in customers
                        join o in orders on c["CustomerID"] equals o["CustomerID"]
                        into ordersGrp  // "into" gives us hierarchical result sequence
                        select new
                        {
                            CustomerId = c["CustomerID"],
                            CustomerName = c["CompanyName"],
                            Orders =
                                from order in ordersGrp
                                join od in orderDetails on order["OrderID"] equals od["OrderID"]
                                into orderDetailsGrp  // "into" gives us hierarchical result sequence
                                select new
                                {
                                    OrderId = order["OrderID"],
                                    OrderDate = (DateTime) order["OrderDate"],
                                    OrderDetails =
                                        from ood in orderDetailsGrp
                                        join p in products on ood["ProductID"] equals p["ProductID"] // pull in the Product name
                                        select new
                                        {
                                            ProductName = p["ProductName"],
                                            UnitPrice = (float) ood["UnitPrice"],
                                            Quantity = (int) ood["Quantity"]
                                        }
                                }
                        };

                    foreach( var custOrder in customerOrders )
                    {
                        Debug.WriteLine( custOrder.CustomerName );

                        foreach( var order in custOrder.Orders )
                        {
                            Debug.WriteLine( '\t' + order.OrderId.ToString() + " - " + order.OrderDate.ToString( "dd MMM yyy" ) );

                            foreach( var orderDetail in order.OrderDetails )
                            {
                                Debug.WriteLine( string.Format( "\t\t{0}\t Qty: {1}\t  Unit Price: {2:0.00}\t  Total Sale: ${3:0.00}",
                                    orderDetail.ProductName, orderDetail.Quantity, orderDetail.UnitPrice, (orderDetail.Quantity * orderDetail.UnitPrice) ) );
                            }
                            Debug.WriteLine( string.Empty );
                        }
                    }
                }
                finally
                {
                    customersDb.Close();
                    ordersDb.Close();
                    orderDetailsDb.Close();
                    productsDb.Close();
                }
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }
        }

        /// <summary>
        /// Linq can apply aggreagate functions to grouped data, such as SUM, AVERAGE, etc.
        /// In this example we want to get a hierarchical list of Customer objects similar to the previous example.
        /// But this time we want to produce an aggregate SUM of each Order.  This time
        /// we will have a list of Customers each with a list of Orders which has only the OrderID and TotalSale.
        /// </summary>
        ///
        private void BtnLinqAggregates_Click( object sender, EventArgs e )
        {
            try
            {
                FileDb customersDb = new FileDb(),
                       ordersDb = new FileDb(),
                       orderDetailsDb = new FileDb();

                string path = System.IO.Path.Combine( AppDomain.CurrentDomain.BaseDirectory, StrNorthwindRelPath );

                customersDb.Open( path + "Customers.fdb", true );
                ordersDb.Open( path + "Orders.fdb", true );
                orderDetailsDb.Open( path + "OrderDetails.fdb", true );

                try
                {
                    // Step 1: Get the Tables we will use with Linq
                    // Note: we only get the columns we will be using so as to keep the size of the table data small

                    // get all Customer records
                    FilterExpression filterExp;
                    FileDbNs.Table customers = customersDb.SelectAllRecords( new string[] { "CustomerID", "CompanyName" } );

                    // now get only Order records for the target Customer records using FilterExpression.CreateInExpressionFromTable
                    // this method creates a HastSet which is extremely efficient when used by FileDb.SelectRecords

                    filterExp = FilterExpression.CreateInExpressionFromTable( "CustomerID", customers, "CustomerID" );
                    FileDbNs.Table orders = ordersDb.SelectRecords( filterExp, new string[] { "CustomerID", "OrderID", "OrderDate" } );

                    // now get only OrderDetails records for the target Order records
                    filterExp = FilterExpression.CreateInExpressionFromTable( "OrderID", orders, "OrderID" );
                    FileDbNs.Table orderDetails = orderDetailsDb.SelectRecords( filterExp, new string[] { "OrderID", "ProductID", "UnitPrice", "Quantity" } );

                    // Step 2:  Use Linq to generate the objects for us

                    var customerOrders =
                        from c in customers
                        join o in orders on c["CustomerID"] equals o["CustomerID"]
                        into ordersGrp  // "into" gives us hierarchical result sequence
                        select new
                        {
                            CustomerId = c["CustomerID"],
                            CustomerName = c["CompanyName"],
                            Orders =
                                from order in ordersGrp
                                join od in orderDetails on order["OrderID"] equals od["OrderID"]
                                into orderDetailsGrp  // "into" gives us hierarchical result sequence
                                select new
                                {
                                    OrderId = order["OrderID"],
                                    OrderDate = (DateTime) order["OrderDate"],
                                    NumItems = orderDetailsGrp.Count(),
                                    TotalSale = orderDetailsGrp.Sum( od => (float) od["UnitPrice"] * (int) od["Quantity"] ),
                                    AverageSale = orderDetailsGrp.Average( od => (float) od["UnitPrice"] * (int) od["Quantity"] )
                                }
                        };

                    foreach( var custOrder in customerOrders )
                    {
                        Debug.WriteLine( custOrder.CustomerName );

                        foreach( var order in custOrder.Orders )
                        {
                            Debug.WriteLine( string.Format( "\tOrder ID: {0}\t  NumItems: {2}\t  Total Sale: ${1:0.00}\t  Average Sale: ${3:0.00}",
                                order.OrderId, order.NumItems, order.TotalSale, order.AverageSale ) );
                        }
                        Debug.WriteLine( string.Empty );
                    }
                }
                finally
                {
                    customersDb.Close();
                    ordersDb.Close();
                    orderDetailsDb.Close();
                }
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }
        }

        #endregion Linq functions

        #region Helper functions

        void displayRecords( Records records )
        {
            grid.DataSource = null;
            grid.Columns.Clear();
            grid.AutoGenerateColumns = true;

            if( records != null )
            {
                // we'll leverage the DataTable for displaying the data in the DataGrid

                DataTable dataTable = new DataTable();

                foreach( string name in records[0].FieldNames )
                {
                    dataTable.Columns.Add( new DataColumn( name, typeof( string ) ) );
                }

                foreach( Record rec in records )
                {
                    DataRow dataRow = dataTable.NewRow();
                    int col = 0;
                    foreach( object obj in rec )
                    {
                        dataRow[col] = rec[col];
                        col++;
                    }
                    dataTable.Rows.Add( dataRow );
                }

                grid.DataSource = dataTable;
                grid.AutoResizeColumns();

                #if false
                foreach( Record record in Records )
                {
                    foreach( Field field in table.Columns )
                    {
                        Debug.WriteLine( string.Format( "Field: {0}  Value: {1}", field.Name, record[field.Name] ) );
                    }
                    
                    foreach( object val in record )
                    {
                        Debug.WriteLine( val.ToString() );
                    }
                }
                #endif
            }
        }
        #endregion Helpers

        private void BtnMetaData_Click( object sender, EventArgs e )
        {
            try
            {
                object metaData = _db.MetaData;
                // MetaData can only be string or byte[]
                _db.MetaData = new byte[] { 1, 2, 3, 4 };
                int debug = 0;
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }
        }

    }
}
