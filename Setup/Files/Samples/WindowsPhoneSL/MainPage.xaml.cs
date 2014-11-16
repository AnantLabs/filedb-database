using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using FileDbNs;
using Microsoft.Phone.Controls;

namespace WindowsPhoneSL
{
    public partial class MainPage : PhoneApplicationPage
    {
        FileDb _db = new FileDb();

        string StrDbName = "SampleData.fdb";

        // Custom class to use with the SampleData database
        public class Person
        {
            public int ID { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public DateTime BirthDate { get; set; }
            public bool IsCitizen { get; set; }
            public int Index { get; set; }
        }

        //------------------------------------------------------------------------------

        public MainPage()
        {
            InitializeComponent();
        }

        #region DB Creation

        private void BtnOpen_Click( object sender, RoutedEventArgs e )
        {
            try
            {
                // the FileDb PCL cannot open files so we must open it first and pass the Stream in
                var dbStream = File.Open( StrDbName, FileMode.OpenOrCreate, FileAccess.ReadWrite );
                _db.Open( dbStream );
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }
        }

        private void BtnClose_Click( object sender, RoutedEventArgs e )
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

        private void BtnCreate_Click( object sender, RoutedEventArgs e )
        {
            try
            {
                // set AutoFlush Off to improve performance for bulk operations
                _db.AutoFlush = false;
#if true
                // Add records using the project XML file

                Stream dataStrm = this.GetType().Assembly.GetManifestResourceStream( this.GetType(), "data.xml" );
                string xml;
                using( StreamReader reader = new StreamReader( dataStrm ) )
                {
                    xml = reader.ReadToEnd();
                }

                XDocument xmlDoc = XDocument.Parse( xml );

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
                    bool isPrimaryKey = col.isPrimaryKey == null ? false : (bool) col.isPrimaryKey,
                         isArray = col.isArray == null ? false : (bool) col.isArray;
                    int autoIncStart = col.autoIncStart == null ? -1 : (int) col.autoIncStart;

                    DataTypeEnum dataType = DataTypeEnum.String;

                    switch( type.ToLower() )
                    {
                        case "int":
                        dataType = DataTypeEnum.Int32;
                        break;
                        case "uint":
                        dataType = DataTypeEnum.UInt32;
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
                var fieldLst = new List<Field>( 20 );
                Field field = new Field( "ID", DataTypeEnum.Int32 );
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
                field = new Field( "IntArray", DataTypeEnum.Int32 );
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

                var dbStream = File.Open( StrDbName, FileMode.OpenOrCreate, FileAccess.ReadWrite );
                _db.Create( dbStream, fieldLst.ToArray() );
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }
            finally
            {
                // set AutoFlush back On
                _db.AutoFlush = true;
            }
        }

        private void BtnAddRecords_Click( object sender, RoutedEventArgs e )
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

                XDocument xmlDoc = XDocument.Parse( xml );

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

                    var values = new FieldValues( 20 );
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
                                    values.Add( field.Name, list.ToArray() );
                                }
                                else
                                {
                                    bool bval;
                                    if( bool.TryParse( value, out bval ) )
                                        values.Add( field.Name, bval );
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
                                    values.Add( field.Name, list.ToArray() );
                                }
                                else
                                {
                                    Byte bval;
                                    if( Byte.TryParse( value, out bval ) )
                                        values.Add( field.Name, bval );
                                }
                            }
                            else if( field.DataType == DataTypeEnum.Int32 )
                            {
                                if( field.IsArray )
                                {
                                    var list = new List<Int32>( 10 );
                                    string[] vals = value.Split( toks );
                                    foreach( string val in vals )
                                    {
                                        list.Add( Int32.Parse( val ) );
                                    }
                                    values.Add( field.Name, list.ToArray() );
                                }
                                else
                                {
                                    Int32 ival;
                                    if( Int32.TryParse( value, out ival ) )
                                        values.Add( field.Name, ival );
                                }
                            }
                            else if( field.DataType == DataTypeEnum.UInt32 )
                            {
                                if( field.IsArray )
                                {
                                    var list = new List<UInt32>( 10 );
                                    string[] vals = value.Split( toks );
                                    foreach( string val in vals )
                                    {
                                        list.Add( UInt32.Parse( val ) );
                                    }
                                    values.Add( field.Name, list.ToArray() );
                                }
                                else
                                {
                                    UInt32 ival;
                                    if( UInt32.TryParse( value, out ival ) )
                                        values.Add( field.Name, ival );
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
                                    values.Add( field.Name, list.ToArray() );
                                }
                                else
                                {
                                    float dval;
                                    if( float.TryParse( value, out dval ) )
                                        values.Add( field.Name, dval );
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
                                    values.Add( field.Name, list.ToArray() );
                                }
                                else
                                {
                                    double dval;
                                    if( double.TryParse( value, out dval ) )
                                        values.Add( field.Name, dval );
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
                                    values.Add( field.Name, list.ToArray() );
                                }
                                else
                                {
                                    values.Add( field.Name, value );
                                }
                            }
                        }

                        idx++;
                    }

                    _db.AddRecord( values );
                }

#else
                var values = new FieldValues();
                values.Add( "FirstName", "Nancy" );
                values.Add( "LastName", "Davolio" );
                values.Add( "BirthDate", new DateTime( 1968, 12, 8 ) );
                values.Add( "IsCitizen", true );
                values.Add( "Float", 1.23 );
                values.Add( "Byte", 1 );
                values.Add( "StringArray", new string[] { "s1", "s2", "s3" } );
                values.Add( "ByteArray", new Byte[] { 1, 2, 3, 4 } );
                values.Add( "IntArray", new int[] { 100, 200, 300, 400 } );
                values.Add( "FloatArray", new double[] { 1.2, 2.4, 3.6, 4.8 } );
                values.Add( "DateTimeArray", new DateTime[] { DateTime.Now, DateTime.Now, DateTime.Now, DateTime.Now } );
                values.Add( "BoolArray", new bool[] { true, false, true, false } );
                _db.AddRecord( values );

                values = new FieldValues();
                values.Add( "FirstName", "Andrew" );
                values.Add( "LastName", "Fuller" );
                values.Add( "BirthDate", new DateTime( 1962, 1, 12 ) );
                values.Add( "IsCitizen", true );
                values.Add( "Float", 2.567 );
                values.Add( "Byte", 2 );
                _db.AddRecord( values );

                values = new FieldValues();
                values.Add( "FirstName", "Janet" );
                values.Add( "LastName", "Leverling" );
                values.Add( "BirthDate", new DateTime( 1963, 8, 30 ) );
                values.Add( "IsCitizen", false );
                values.Add( "Float", 3.14 );
                values.Add( "Byte", 3 );
                _db.AddRecord( values );

                values = new FieldValues();
                values.Add( "FirstName", "Margaret" );
                values.Add( "LastName", "Peacock" );
                values.Add( "BirthDate", new DateTime( 1957, 9, 19 ) );
                values.Add( "IsCitizen", false );
                values.Add( "Float", 4.96 );
                values.Add( "Byte", 4 );
                _db.AddRecord( values );

                values = new FieldValues();
                values.Add( "FirstName", "Steven" );
                values.Add( "LastName", "Buchanan" );
                values.Add( "BirthDate", new DateTime( 1965, 3, 4 ) );
                values.Add( "IsCitizen", true );
                values.Add( "Float", 5.7 );
                values.Add( "Byte", 5 );
                _db.AddRecord( record );
#endif
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }
        }

        private void BtnDrop_Click( object sender, RoutedEventArgs e )
        {
            try
            {
                // Drop not supported in PCL version
                //_db.Drop( StrDbName );
                File.Delete( StrDbName );
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }
        }

        #endregion DB Creation

        #region Search functions

        private void BtnGetAllRecords_Click( object sender, RoutedEventArgs e )
        {
            try
            {
                // get all records sorted by LastName, Firstname
                Table table = _db.SelectAllRecords( null, new string[] { "LastName", "Firstname" } );
                displayRecords( table );

                // get the same records again as a typed list using your own custom class
                IList<Person> persons = _db.SelectAllRecords<Person>( null, new string[] { "LastName", "Firstname" } );
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }
        }

        private void BtnGetRecordsRegex_Click( object sender, RoutedEventArgs e )
        {
            try
            {
                string searchVal = @"\bFull";
                var fieldSearchExp = new FilterExpression( "LastName", searchVal, ComparisonOperatorEnum.Regex );

                Table table = _db.SelectRecords( fieldSearchExp );
                displayRecords( table );
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }
        }

        private void BtnGetMatchingRecords_Click( object sender, RoutedEventArgs e )
        {
            try
            {
                // Use the FilterExpressionGroup's filter parser to create a FilterExpressionGroup
                // The syntax is similar to SQL (do not preceed with WHERE)
                // Note that there are 2 ways to get a case-INSENSITIVE search:
                // Use either ~= or you can prefix the fieldname with ~
                // Both methods are shown below
                // (FileDb doesn't currently support UPPER or LOWER)
                // Note also that each set of parentheses will create a child FilterExpressionGroup

                string filter = "(~FirstName = 'steven' OR [FirstName] ~= 'NANCY') AND LastName = 'Fuller'";

                FilterExpressionGroup filterExpGrp = FilterExpressionGroup.Parse( filter );
                Table table = _db.SelectRecords( filterExpGrp );
                displayRecords( table );

                // we can manually build the same FilterExpressionGroup

                var fname1Exp = new FilterExpression( "FirstName", "steven", ComparisonOperatorEnum.Equal, MatchTypeEnum.IgnoreCase );

                // the following two lines produce the same FilterExpression
                var fname2Exp = FilterExpression.Parse( "FirstName ~= 'NANCY'" );
                fname2Exp = new FilterExpression( "FirstName", "NANCY", ComparisonOperatorEnum.Equal, MatchTypeEnum.IgnoreCase );

                var lnameExp = new FilterExpression( "LastName", "Fuller", ComparisonOperatorEnum.Equal, MatchTypeEnum.UseCase );

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

                // get the same records again as a typed list using your own custom class
                IList<Person> persons = _db.SelectRecords<Person>( filter );
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }
        }

        private void BtnGetRecordByIndex_Click( object sender, RoutedEventArgs e )
        {
            try
            {
                Record record = _db.GetRecordByIndex( 1, null );
                Records records = new Records( 1 );
                records.Add( record );
                displayRecords( records );
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }
        }

        private void BtnGetRecordByKey_Click( object sender, RoutedEventArgs e )
        {
            try
            {
                Record record = _db.GetRecordByKey( 3, new string[] { "ID", "Firstname", "LastName" }, false );
                Records records = new Records( 1 );
                records.Add( record );
                displayRecords( records );
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }
        }

        #endregion Searching

        #region Update functions

        private void BtnUpdateMatchingRecords_Click( object sender, RoutedEventArgs e )
        {
            try
            {
                string filter = "(~FirstName = 'andrew' OR ~FirstName = 'nancy') AND ~LastName = 'fuller'";
                FilterExpressionGroup filterExpGrp = FilterExpressionGroup.Parse( filter );
                Table table = _db.SelectRecords( filterExpGrp );
                displayRecords( table );

                // equivalent building it manually
                var fname1Exp = new FilterExpression( "FirstName", "andrew", ComparisonOperatorEnum.Equal, MatchTypeEnum.IgnoreCase );
                // the following two lines produce the same FilterExpression
                var fname2Exp = FilterExpression.Parse( "~FirstName = nancy" );
                fname2Exp = new FilterExpression( "FirstName", "nancy", ComparisonOperatorEnum.Equal, MatchTypeEnum.IgnoreCase );
                var lnameExp = new FilterExpression( "LastName", "fuller", ComparisonOperatorEnum.Equal, MatchTypeEnum.IgnoreCase );

                var fnamesGrp = new FilterExpressionGroup();
                fnamesGrp.Add( BoolOpEnum.Or, fname1Exp );
                fnamesGrp.Add( BoolOpEnum.Or, fname2Exp );
                var allNamesGrp = new FilterExpressionGroup();
                allNamesGrp.Add( BoolOpEnum.And, lnameExp );
                allNamesGrp.Add( BoolOpEnum.And, fnamesGrp );

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

        private void BtnUpdateRecordsRegex_Click( object sender, RoutedEventArgs e )
        {
            try
            {
                string searchVal = @"\bFull";
                var fieldSearchExp = new FilterExpression( "LastName", searchVal, ComparisonOperatorEnum.Regex );

                var fieldValues = new FieldValues();
                fieldValues.Add( "IsCitizen", true );
                int nRecs = _db.UpdateRecords( fieldSearchExp, fieldValues );

                Table table = _db.SelectRecords( fieldSearchExp );
                displayRecords( table );
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }
        }
        #endregion Update functions

        #region Remove functions

        private void BtnRemoveByKey_Click( object sender, RoutedEventArgs e )
        {
            try
            {
                // the following two lines produce the same FilterExpression
                var exp1 = FilterExpression.Parse( "~FirstName = nancy" );
                exp1 = new FilterExpression( "FirstName", "nancy", ComparisonOperatorEnum.Equal, MatchTypeEnum.IgnoreCase );
                var exp2 = new FilterExpression( "LastName", "leverling", ComparisonOperatorEnum.Equal, MatchTypeEnum.IgnoreCase );
                var expGrp = new FilterExpressionGroup();
                expGrp.Add( BoolOpEnum.Or, exp1 );
                expGrp.Add( BoolOpEnum.Or, exp2 );

                Table table = _db.SelectRecords( expGrp, new string[] { "ID" } );

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

        private void BtnRemoveByValue_Click( object sender, RoutedEventArgs e )
        {
            try
            {
                FilterExpression searchExp = new FilterExpression( "FirstName", "Nancy", ComparisonOperatorEnum.Equal, MatchTypeEnum.UseCase );
                int numDeleted = _db.DeleteRecords( searchExp );
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }
        }
        #endregion Remove functions

        #region Misc functions

        private void BtnClean_Click( object sender, RoutedEventArgs e )
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

        private void BtnReindex_Click( object sender, RoutedEventArgs e )
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

        private void BtnMetaData_Click( object sender, RoutedEventArgs e )
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
        #endregion Misc functions

        #region Linq functions

        /// <summary>
        /// Using LINQ to Objects with FileDb gives you full relational capability.  We only have one table
        /// for this sample app so we can't demonstrate a join so we just do a simple Select.
        /// See the Other samples in this download for more LINQ examples.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnLinqSelect_Click( object sender, RoutedEventArgs e )
        {
            try
            {
                LinqSelect_Record();
                LinqSelect_Custom();
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }
        }

        // Built-in Record class version
        //
        void LinqSelect_Record()
        {
            //FilterExpression filterExp = FilterExpression.Parse( "LastName IN ('Fuller', 'Peacock')" );
            FileDbNs.Table employees = _db.SelectRecords( "LastName IN ('Fuller', 'Peacock')" );

            var query =
                from e in employees
                select new
                {
                    ID = e["ID"],
                    Name = e["FirstName"] + " " + e["LastName"],
                    BirthDate = e["BirthDate"]
                };

            foreach( var employee in query )
            {
                Debug.WriteLine( employee.ToString() );
            }
        }

        // Custom class version
        //
        void LinqSelect_Custom()
        {
            // either of these ways work the same
            //FilterExpression filterExp = FilterExpression.Parse( "LastName IN ('Fuller', 'Peacock')" );
            //IList<Person> employees = _db.SelectRecords<Person>( filterExp );
            IList<Person> employees = _db.SelectRecords<Person>( "LastName IN ('Fuller', 'Peacock')" );

            var query =
                from e in employees
                select new
                {
                    ID = e.ID,
                    Name = e.FirstName + " " + e.LastName,
                    BirthDate = e.BirthDate
                };

            foreach( var employee in query )
            {
                Debug.WriteLine( employee.ToString() );
            }
        }
        #endregion Linq functions

        #region Helper functions

        // Table derives from Records
        //
        void displayRecords( Records records )
        {
            if( records != null )
            {
                foreach( Record record in records )
                {
                    foreach( string name in records[0].FieldNames )
                    {
                        object val = record[name];
                        Debug.WriteLine( string.Format( "Field: {0}  Value: {1}", name, val == null ? "null" : val.ToString() ) );
                    }

                    foreach( object val in record )
                    {
                        Debug.WriteLine( val == null ? "null" : val.ToString() );
                    }
                }
            }
        }

        #endregion Helper functions
    }
}