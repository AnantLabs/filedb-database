using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Linq;
using System.Xml.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Diagnostics;

using FileDbNs;

namespace SampleApp
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
                _db.Open( StrDbName, false );
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
#if true
                // Add records using the project XML file

                Stream s = this.GetType().Assembly.GetManifestResourceStream( this.GetType(), "data.xml" );
                string xml;
                using( StreamReader reader = new StreamReader( s ) )
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

                    DataType dataType = DataType.String;

                    switch( type.ToLower() )
                    {
                        case "int":
                            dataType = DataType.Int;
                        break;
                        case "uint":
                            dataType = DataType.UInt;
                        break;
                        case "string":
                            dataType = DataType.String;
                        break;
                        case "datetime":
                            dataType = DataType.DateTime;
                        break;
                        case "bool":
                            dataType = DataType.Bool;
                        break;
                        case "float":
                            dataType = DataType.Float;
                        break;
                        case "double":
                            dataType = DataType.Double;
                        break;
                        case "byte":
                            dataType = DataType.Byte;
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
                Field field = new Field( "ID", DataType.Int );
                field.AutoIncStart = 0;
                field.IsPrimaryKey = true;
                fieldLst.Add( field );
                field = new Field( "FirstName", DataType.String );
                //field.IsPrimaryKey = true;
                fieldLst.Add( field );
                field = new Field( "LastName", DataType.String );
                fieldLst.Add( field );
                field = new Field( "BirthDate", DataType.DateTime );
                fieldLst.Add( field );
                field = new Field( "IsCitizen", DataType.Bool );
                fieldLst.Add( field );
                field = new Field( "Float", DataType.Float );
                fieldLst.Add( field );
                field = new Field( "Byte", DataType.Byte );
                fieldLst.Add( field );

                // array types
                field = new Field( "StringArray", DataType.String );
                field.IsArray = true;
                fieldLst.Add( field );
                field = new Field( "ByteArray", DataType.Byte );
                field.IsArray = true;
                fieldLst.Add( field );
                field = new Field( "IntArray", DataType.Int );
                field.IsArray = true;
                fieldLst.Add( field );
                field = new Field( "FloatArray", DataType.Float );
                field.IsArray = true;
                fieldLst.Add( field );
                field = new Field( "DateTimeArray", DataType.DateTime );
                field.IsArray = true;
                fieldLst.Add( field );
                field = new Field( "BoolArray", DataType.Bool );
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

                    var record = new FieldValues( 20 );
                    int idx = 0;

                    foreach( var col in columns )
                    {
                        string value = (string) col.value;
                        Field field = _db.Fields[idx];

                        if( !string.IsNullOrEmpty( value ) )
                        {
                            if( field.DataType == DataType.Bool )
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
                            else if( field.DataType == DataType.Byte )
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
                            else if( field.DataType == DataType.Int )
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
                            else if( field.DataType == DataType.UInt )
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
                            else if( field.DataType == DataType.Float )
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
                            else if( field.DataType == DataType.Double )
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
                var record = new FieldValues();
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

                record = new FieldValues();
                record.Add( "FirstName", "Andrew" );
                record.Add( "LastName", "Fuller" );
                record.Add( "BirthDate", new DateTime( 1962, 1, 12 ) );
                record.Add( "IsCitizen", true );
                record.Add( "Float", 2.567 );
                record.Add( "Byte", 2 );
                _db.AddRecord( record );

                record = new FieldValues();
                record.Add( "FirstName", "Janet" );
                record.Add( "LastName", "Leverling" );
                record.Add( "BirthDate", new DateTime( 1963, 8, 30 ) );
                record.Add( "IsCitizen", false );
                record.Add( "Float", 3.14 );
                record.Add( "Byte", 3 );
                _db.AddRecord( record );

                record = new FieldValues();
                record.Add( "FirstName", "Margaret" );
                record.Add( "LastName", "Peacock" );
                record.Add( "BirthDate", new DateTime( 1957, 9, 19 ) );
                record.Add( "IsCitizen", false );
                record.Add( "Float", 4.96 );
                record.Add( "Byte", 4 );
                _db.AddRecord( record );

                record = new FieldValues();
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

        private void BtnDrop_Click( object sender, RoutedEventArgs e )
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
                var fieldSearchExp = new FilterExpression( "LastName", searchVal, Equality.Like );

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
                // Note that we prefix the fieldname with ~ to get a case-INSENSITIVE search
                // (FileDb doesn't currently support UPPER or LOWER)
                // Note that we can also use LIKE when we want to ignore case, but the difference
                // is that LIKE will create a RegEx search which would be a little slower
                // Note also that each set of parentheses will create a child FilterExpressionGroup

                string filter = "(~FirstName = 'steven' OR [FirstName] LIKE 'NANCY') AND LastName = 'Fuller'";

                FilterExpressionGroup filterExpGrp = FilterExpressionGroup.Parse( filter );
                Table table = _db.SelectRecords( filterExpGrp );
                displayRecords( table );

                // we can manually build the same FilterExpressionGroup
                var lnameExp = new FilterExpression( "LastName", "Fuller", Equality.Equal, MatchType.UseCase );
                var fname1Exp = new FilterExpression( "FirstName", "steven", Equality.Equal, MatchType.IgnoreCase );
                // the following two lines produce the same FilterExpression
                var fname2Exp = FilterExpression.Parse( "FirstName LIKE 'NANCY'" );
                fname2Exp = new FilterExpression( "FirstName", "NANCY", Equality.Like );

                var fnamesGrp = new FilterExpressionGroup();
                fnamesGrp.Add( BoolOp.Or, fname1Exp );
                fnamesGrp.Add( BoolOp.Or, fname2Exp );
                var allNamesGrp = new FilterExpressionGroup();
                allNamesGrp.Add( BoolOp.And, lnameExp );
                allNamesGrp.Add( BoolOp.And, fnamesGrp );

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
                var fname1Exp = new FilterExpression( "FirstName", "andrew", Equality.Equal, MatchType.IgnoreCase );
                // the following two lines produce the same FilterExpression
                var fname2Exp = FilterExpression.Parse( "~FirstName = nancy" );
                fname2Exp = new FilterExpression( "FirstName", "nancy", Equality.Equal, MatchType.IgnoreCase );
                var lnameExp = new FilterExpression( "LastName", "fuller", Equality.Equal, MatchType.IgnoreCase );

                var fnamesGrp = new FilterExpressionGroup();
                fnamesGrp.Add( BoolOp.Or, fname1Exp );
                fnamesGrp.Add( BoolOp.Or, fname2Exp );
                var allNamesGrp = new FilterExpressionGroup();
                allNamesGrp.Add( BoolOp.And, lnameExp );
                allNamesGrp.Add( BoolOp.And, fnamesGrp );

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
                var fieldSearchExp = new FilterExpression( "LastName", searchVal, Equality.Like );

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
                exp1 = new FilterExpression( "FirstName", "nancy", Equality.Equal, MatchType.IgnoreCase );
                var exp2 = new FilterExpression( "LastName", "leverling", Equality.Equal, MatchType.IgnoreCase );
                var expGrp = new FilterExpressionGroup();
                expGrp.Add( BoolOp.Or, exp1 );
                expGrp.Add( BoolOp.Or, exp2 );

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
                FilterExpression searchExp = new FilterExpression( "FirstName", "Nancy", Equality.Equal, MatchType.UseCase );
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
            FilterExpression filterExp = FilterExpression.Parse( "LastName IN ('Fuller', 'Peacock')" );
            FileDbNs.Table employees = _db.SelectRecords( filterExp );

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
            FilterExpression filterExp = FilterExpression.Parse( "LastName IN ('Fuller', 'Peacock')" );
            IList<Person> employees = _db.SelectRecords<Person>( filterExp );

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