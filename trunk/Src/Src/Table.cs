﻿using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

#if !SILVERLIGHT && !WINDOWS_PHONE
namespace LINQPad
{
    public interface ICustomMemberProvider
    {
        // Each of these methods must return a sequence
        // with the same number of elements:
        IEnumerable<string> GetNames();
        IEnumerable<Type> GetTypes();
        IEnumerable<object> GetValues();
    }
}
#endif

namespace FileDbNs
{
    //=====================================================================
    /// <summary>
    /// Represents a column of the database (table).
    /// </summary>
    public class Field // : ICloneable - not available in Silverlight
    {
        /// <summary>
        /// Use this constructor when creating a new database. The ordinal index of the field will be the
        /// order it was added to the field list, unless a primary key field is specified and wasn't the 
        /// first in the list. In this case the primary key field will be moved to the first in the list
        /// so that its always first.
        /// </summary>
        /// <param name="name">The name of the field</param>
        /// <param name="type">The data type of the field</param>
        /// 
        public Field( string name, DataTypeEnum type )
            : this( name, type, -1 )
        {
        }

        /// <summary>
        /// Use this constructor when you need to create a Records list manually rather than
        /// using one retured from a query.  In this case, you must set the field ordinal index
        /// for each field, starting at zero.
        /// </summary>
        /// <param name="name">The name of the field</param>
        /// <param name="type">The data type of the field</param>
        /// <param name="ordinal">The zero-based ordinal index of the field</param>
        /// 
        public Field( string name, DataTypeEnum type, int ordinal )
        {
            Name = name;
            DataType = type;
            AutoIncStart = -1; // default to no AutoInc
            this.Ordinal = ordinal;
        }

        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// The name of the field.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The type of the field.
        /// </summary>
        public DataTypeEnum DataType { get; set; }

        /// <summary>
        /// The zero-based ordinal index of the field.
        /// </summary>
        public Int32 Ordinal { get; internal set; }

        /// <summary>
        /// Indicates if this is the one and only primary key field
        /// </summary>
        public bool IsPrimaryKey { get; set; }

        /// <summary>
        /// Indicate if this is an Array type field
        /// </summary>
        public bool IsArray { get; set; }
        
        /// <summary>
        /// Used for auto-increment fields. Set to the number which you want incrementing to begin.
        /// Set to -1 if not an auto-increment field.
        /// </summary>
        public Int32 AutoIncStart { get; set; }

        internal Int32 CurAutoIncVal { get; set; }

        /// <summary>
        /// Returns true if this is an auto-increment field, false otherwise
        /// </summary>
        public bool IsAutoInc
        {
            get { return AutoIncStart > -1; }
        }

        /// <summary>
        /// Comment for the field
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// User property to associate a value with this Field
        /// </summary>
        public object Tag { get; set; }


        public Field Clone()
        {
            Field newField = new Field( this.Name, this.DataType, this.Ordinal );

            newField.AutoIncStart = this.AutoIncStart;
            newField.IsArray = this.IsArray;
            newField.IsPrimaryKey = this.IsPrimaryKey;
            newField.CurAutoIncVal = this.CurAutoIncVal;
            newField.Comment = this.Comment;

            return newField;
        }
    }

    //=====================================================================
    /// <summary>
    /// Represents data for a row, a Record consists of name-value pairs.
    /// Used when adding data to the database and by the Record object
    /// to store data returned from queries.
    /// </summary>
    
    public class FieldValues : Dictionary<string, object> // note: switch to NameValueCollection someday when added to Silverlight/WP
    {
        public FieldValues() { }
        public FieldValues( Int32 count ) : base( count ) { }

        public new object this[string idx]
        {
            get
            {
                return base[idx.ToUpper()];
            }

            set
            {
                base[idx.ToUpper()] = value;
            }
        }

        public new void Add( string fieldName, object value )
        {
            base.Add( fieldName.ToUpper(), value );
        }

        public new bool ContainsKey( string fieldName )
        {
            return base.ContainsKey( fieldName.ToUpper() );
        }
    }
    
    //=====================================================================
    /// <summary>
    /// Represents a single row returned from a query.  It will
    /// contain one or more fields of the database (table).  The last column may be the index of the row (if requested),
    /// which is the zero-based position of the record in the index.  If there is no primary key specified for
    /// the database (table), then the index is the record number in the order in which the row was added to the database.
    /// </summary>
    /// 
    public class Record : IEnumerable
        #if !SILVERLIGHT && !WINDOWS_PHONE
        , LINQPad.ICustomMemberProvider
        #endif
    {
        Fields _fields;
        List<object> _values;

        /// <summary>
        /// Create a Record object with the indicated Fields and values.  If creating a list of Record objects 
        /// (for a Records list) be sure to use the same Fields list for each Record.
        /// </summary>
        /// <param name="fields"></param>
        /// <param name="values"></param>
        /// 
        public Record( Fields fields, object[] values )
        {
            _fields = fields;
            _values = new List<object>( fields.Count );

            for( int n=0; n < fields.Count; n++ )
            {
                Field field = fields[n];
                object val = null;                
                if( values != null )
                    val = values[n];
                _values.Add( val );
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for( int n=0; n < _fields.Count; n++ )
            {
                var f = _fields[n];
                if( sb.Length > 0 )
                    sb.Append( "\r\n" );
                sb.Append( string.Format( "{0}: {1}", f.Name, _values[n] ) );
            }
            return sb.ToString();
        }

        //
        internal List<object> Values
        {
            get { return _values; }
        }

        #region IEnumerable
        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator) GetEnumerator();
        }
                
        public ObjectEnumerator GetEnumerator()
        {
            return new ObjectEnumerator( _values.ToArray() );
        }
        #endregion IEnumerable

        #region ICustomMemberProvider
        // for LinqPad
        #if !SILVERLIGHT && !WINDOWS_PHONE                
        public IEnumerable<string> GetNames()
        {
            var names = new List<string>( _fields.Count );
            foreach( var f in _fields )
            {
                names.Add( f.Name );
            }
            return names;
        }
        public IEnumerable<Type> GetTypes()
        {
            var types = new List<Type>( _fields.Count );
            for( int n=0; n < _fields.Count; n++ )
            {
                Type type = null;
                Field f = _fields[0];
                switch( f.DataType )
                {
                    case DataTypeEnum.String:
                        type = typeof( String );
                        break;
                    case DataTypeEnum.Byte:
                        type = typeof( Byte );
                        break;
                    case DataTypeEnum.Int:
                        type = typeof( Int32 );
                        break;
                    case DataTypeEnum.UInt:
                        type = typeof( UInt32 );
                        break;
                    case DataTypeEnum.Float:
                        type = typeof( Single );
                        break;
                    case DataTypeEnum.Double:
                        type = typeof( Double );
                        break;
                    case DataTypeEnum.Bool:
                        type = typeof( Boolean );
                        break;
                    case DataTypeEnum.DateTime:
                        type = typeof( DateTime );
                        break;
                }
                types.Add( type );
            }
            return types;
        }
        public IEnumerable<object> GetValues()
        {
            var values = new List<object>( _fields.Count );
            for( int n = 0; n < _fields.Count; n++ )
            {
                values.Add( _values[n] );
            }
            return values;
        }
        #endif
        #endregion ICustomMemberProvider

        /*
        public void Add( string fieldName, object value )
        {
            //int idx = _values.Count;
            _values.Add( value );
            // we store the index of the value in the _values list
            //_record.Add( fieldName.ToUpper(), idx );
        }*/

        public object this[string name]
        {
            get
            {
                /*
                string upperName = name.ToUpper();
                if( !_record.ContainsKey( upperName ) )
                    throw new FileDbException( string.Format( FileDbException.InvalidFieldName, name ), FileDbExceptions.InvalidFieldName );
                return _values[_record[upperName]];
                */
                return _values[_fields[name].Ordinal];
            }

            ////////////////////////////////////////////
            set
            {
                /*
                string upperName = name.ToUpper();
                if( !_record.ContainsKey( upperName ) )
                    throw new FileDbException( string.Format( FileDbException.InvalidFieldName, name ), FileDbExceptions.InvalidFieldName );
                int idx = _record[upperName];
                _values[idx] = value;
                */
                _values[_fields[name].Ordinal] = value;
            }
        }

        public object this[int idx]
        {
            get
            {
                if( idx >= 0 && idx < _values.Count )
                    return _values[idx];
                else
                    throw new FileDbException( FileDbException.IndexOutOfRange, FileDbExceptionsEnum.IndexOutOfRange );
            }
            ///
            set
            {
                if( !(idx >= 0 && idx < _values.Count) )
                    throw new FileDbException( FileDbException.IndexOutOfRange, FileDbExceptionsEnum.IndexOutOfRange );

                _values[idx] = value;
            }
        }

        /// <summary>
        /// The number of fields in the Record
        /// </summary>
        /// 
        public int Length
        {
            get { return _values.Count; }
        }

        /// <summary>
        /// Tests to see if the indicated field is in this Record
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns>true if the field is in this Record</returns>
        /// 
        public bool ContainsField( string fieldName )
        {
            //return _record.ContainsKey( fieldName.ToUpper() );
            return _fields.ContainsKey( fieldName.ToUpper() );
        }

        public IList<string> FieldNames
        {
            get
            {
                List<string> fields = new List<string>( _fields.Count );
                foreach( Field field in _fields )
                    fields.Add( field.Name );
                return fields;
            }
        }

        public FieldValues GetFieldValues()
        {
            FieldValues fieldValues = new FieldValues( _fields.Count );
            foreach( Field field in _fields )
            {
                fieldValues.Add( field.Name, this[field.Name] );
            }
            return fieldValues;
        }

        /// <summary>
        /// A property which is used for integrating with the binding framework.
        /// </summary>
        /// 
        public object Data
        {
            get
            {
                // when the binding framework reads this property, simply return the Record instance. The
                // RowIndexConverter takes care of extracting the correct property value
                return this;
            }

            set
            {
                // the RowIndexConverter will signal property changes by providing an instance of PropertyValueChange.
                FieldSetter setter = value as FieldSetter;
                this[setter.PropertyName] = setter.Value;
            }
        }

        #region Get helpers

        /// <summary>
        /// Return the integer field value.
        /// </summary>
        /// <param name="fieldName">The name of the field</param>
        /// <returns>The integer field value</returns>
        /// 
        public Int32? GetInt( string fieldName )
        {
            return (Int32) this[fieldName];
        }

        /// <summary>
        /// Return the integer field value.
        /// </summary>
        /// <param name="index">The ordinal index of the field</param>
        /// <returns>The integer field value</returns>
        /// 
        public Int32? GetInt( int index )
        {
            return (Int32) this[index];
        }

        /// <summary>
        /// Return the unsigned integer field value.
        /// </summary>
        /// <param name="fieldName">The name of the field</param>
        /// <returns>The unsigned integer field value</returns>
        /// 
        public UInt32? GetUInt( string fieldName )
        {
            return (UInt32) this[fieldName];
        }

        /// <summary>
        /// Return the unsigned integer field value.
        /// </summary>
        /// <param name="index">The ordinal index of the field</param>
        /// <returns>The unsigned integer field value</returns>
        /// 
        public UInt32? GetUInt( int index )
        {
            return (UInt32) this[index];
        }

        /// <summary>
        /// Return the String field value.
        /// </summary>
        /// <param name="fieldName">The name of the field</param>
        /// <returns>The String field value</returns>
        /// 
        public String GetString( string fieldName )
        {
            return (String) this[fieldName];
        }

        /// <summary>
        /// Return the String field value.
        /// </summary>
        /// <param name="index">The ordinal index of the field</param>
        /// <returns>The String field value</returns>
        /// 
        public String GetString( int index )
        {
            return (String) this[index];
        }

        /// <summary>
        /// Return the Byte field value.
        /// </summary>
        /// <param name="fieldName">The name of the field</param>
        /// <returns>The Byte field value</returns>
        /// 
        public Byte? GetByte( string fieldName )
        {
            return (Byte) this[fieldName];
        }

        /// <summary>
        /// Return the Byte field value.
        /// </summary>
        /// <param name="index">The ordinal index of the field</param>
        /// <returns>The Byte field value</returns>
        /// 
        public Byte? GetByte( int index )
        {
            return (Byte) this[index];
        }

        /// <summary>
        /// Return the Single field value.
        /// </summary>
        /// <param name="fieldName">The name of the field</param>
        /// <returns>The Single field value</returns>
        /// 
        public Single? GetSingle( string fieldName )
        {
            return (Single) this[fieldName];
        }

        /// <summary>
        /// Return the Single field value.
        /// </summary>
        /// <param name="index">The ordinal index of the field</param>
        /// <returns>The Single field value</returns>
        /// 
        public Single? GetSingle( int index )
        {
            return (Single) this[index];
        }

        /// <summary>
        /// Return the Double field value.
        /// </summary>
        /// <param name="fieldName">The name of the field</param>
        /// <returns>The Double field value</returns>
        /// 
        public Double? GetDouble( string fieldName )
        {
            return (Double) this[fieldName];
        }

        /// <summary>
        /// Return the Double field value.
        /// </summary>
        /// <param name="index">The ordinal index of the field</param>
        /// <returns>The Double field value</returns>
        /// 
        public Double? GetDouble( int index )
        {
            return (Double) this[index];
        }

        /// <summary>
        /// Return the Boolean field value.
        /// </summary>
        /// <param name="fieldName">The name of the field</param>
        /// <returns>The Boolean field value</returns>
        /// 
        public Boolean? GetBoolean( string fieldName )
        {
            return (Boolean) this[fieldName];
        }

        /// <summary>
        /// Return the Boolean field value.
        /// </summary>
        /// <param name="index">The ordinal index of the field</param>
        /// <returns>The Boolean field value</returns>
        /// 
        public Boolean? GetBoolean( int index )
        {
            return (Boolean) this[index];
        }

        /// <summary>
        /// Return the DateTime field value.
        /// </summary>
        /// <param name="fieldName">The name of the field</param>
        /// <returns>The DateTime field value</returns>
        /// 
        public DateTime? GetDateTime( string fieldName )
        {
            return (DateTime) this[fieldName];
        }

        /// <summary>
        /// Return the DateTime field value.
        /// </summary>
        /// <param name="index">The ordinal index of the field</param>
        /// <returns>The DateTime field value</returns>
        /// 
        public DateTime? GetDateTime( int index )
        {
            return (DateTime) this[index];
        }
        #endregion Get helpers
    }

    //=====================================================================
    /// <summary>
    /// Used by the Record class for enumerating in foreach contructs
    /// </summary>
    /// 
    public class ObjectEnumerator : IEnumerator
    {
        object[] _row;

        // Enumerators are positioned before the first element
        // until the first MoveNext() call.
        int position = -1;

        public ObjectEnumerator( object[] list )
        {
            _row = list;
        }

        public bool MoveNext()
        {
            position++;
            return (position < _row.Length);
        }

        public void Reset()
        {
            position = -1;
        }

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public object Current
        {
            get
            {
                try
                {
                    return _row[position];
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }
    }

    //=====================================================================
    /// <summary>
    /// A List of Records
    /// </summary>
    /// 
    public class Records : List<Record>
    {
        // allow users to create rows so they can add their own Records and set into DataGrid
        public Records()
        {
        }

        // allow users to create rows so they can add their own Records and set into DataGrid
        public Records( int capacity ) : base( capacity )
        {
        }

        internal Records( Fields fields, object[][] records )
            : base( records.Length )
        {
            foreach( object[] record in records )
            {
                Record row = new Record( fields, record );
                this.Add( row );
            }
        }

        internal Records( Fields fields, object[] record ) : base( 1 )
        {
            Record row = new Record( fields, record );
            this.Add( row );
        }
    }

    //=====================================================================
    /// <summary>
    /// A list of Fields
    /// </summary>
    /// 
    public class Fields : List<Field>
    {
        Dictionary<string, Field> _fields;

        public Fields()
        {
            _fields = new Dictionary<string, Field>();
        }

        public Fields( int capacity )
            : base( capacity )
        {
            _fields = new Dictionary<string, Field>( capacity );
        }

        public Fields( Fields fields )
            : base( fields.Count )
        {
            _fields = new Dictionary<string, Field>( fields.Count );
            foreach( Field field in fields )
            {
                this.Add( field );
            }
        }

        public new void Add( Field field )
        {
            base.Add( field );
            _fields.Add( field.Name.ToUpper(), field );
        }

        public bool Remove( string fieldName )
        {
            bool ret = false;
            fieldName = fieldName.ToUpper();
            Field field = _fields[fieldName];
            if( field != null )
            {
                ret = base.Remove( field );
                _fields.Remove( fieldName );
            }
            return ret;
        }

        public Field this[string fieldName]
        {
            get
            {
                if( _fields.Count > 0 )
                    return _fields[fieldName.ToUpper()];
                else
                    return null;
            }
        }

        public bool ContainsKey( string fieldName )
        {
            return _fields.ContainsKey( fieldName.ToUpper() );
        }
    }

    //=====================================================================
    /// <summary>
    /// Represents a data table returned from a query.  A table is made up of Fields and Records.
    /// </summary>
    /// 
    public class Table : Records
    {
        const string Index = "index";
        Fields _fields;


        /// <summary>
        /// Create a table with the indicated Fields and records.  If copyFields is true, a new
        /// Fields list is created and a copy of each field is made and its ordinal adjusted.
        /// Otherwise the original Fields object is adopted.  You should pass false for copyFields
        /// only if you created the Fields list and its Field objects yourself.
        /// </summary>
        /// <param name="fields">The Fields list to use</param>
        /// <param name="records">The record data</param>
        /// <param name="copyFields">Indicates whether to make a copy of the Fields object and each Field.</param>
        /// 
        public Table( Fields fields, object[][] records, bool copyFields )
        {
            Create( fields, records, copyFields );
        }

        /// <summary>
        /// Create a table with the indicated Fields and records.  If copyFields is true, a new
        /// Fields list is created and a copy of each field is made and its ordinal adjusted.
        /// Otherwise the original Fields object is adopted.  You should pass false for copyFields
        /// only if you created the Fields list and its Field objects yourself and the Fields in 
        /// the Record objects match the data in the Record.
        /// </summary>
        /// <param name="fields">The Fields list to use</param>
        /// <param name="records">The record data</param>
        /// <param name="copyFields">Indicates whether to make a copy of the Fields object and each Field.</param>
        ///
        public Table( Fields fields, Records records, bool copyFields )
            : base( records.Count )
        {
            initFields( fields, copyFields );

            foreach( Record record in records )
            {
                this.Add( record );
            }
        }

        internal void Create( Fields fields, object[][] records, bool copyFields )
        {
            this.Clear();

            initFields( fields, copyFields );

            if( records != null )
            {
                // do what the Records constructor does
                foreach( object[] record in records )
                {
                    // very important to use member _fields NOT fields because ordinals have been adjusted
                    Record row = new Record( _fields, record );
                    this.Add( row );
                }
            }
        }

        void initFields( Fields fields, bool copyFields )
        {
            if( copyFields )
            {
                _fields = new Fields( fields.Count );
                // we must adjust the field ordinals
                int n = 0;
                foreach( Field field in fields )
                {
                    Field f = (Field) field.Clone();
                    f.Ordinal = n++;
                    _fields.Add( f );
                }
            }
            else
            {
                _fields = fields;
            }
        }

        public override string ToString()
        {
            return string.Format( "NumRecords = {0}", this.Count );
        }

        /// <summary>
        /// The Records of the Table
        /// </summary>
        /// 
        //public Records Records { get { return _records; } }

        /// <summary>
        /// The Fields of the Table
        /// </summary>
        /// 
        public Fields Fields { get { return _fields; } }

        /// <summary>
        /// Create a new Record with all of the fields of the Table.  The Record is not 
        /// added to the Table.  To do this, call Records.Add.
        /// </summary>
        /// <returns></returns>
        /// 
        public Record NewRow()
        {
            Record row = new Record( _fields, null );
            return row;
        }

        /// <summary>
        /// Save this Table to the indicated file as a new database.  If the file exists
        /// it will be overwritten.  The new database will be just as if you had created
        /// it from scratch.
        /// </summary>
        /// <param name="dbName">The full path and filename of the new database</param>
        /// 
        public FileDb SaveToDb( string dbName )
        {
            FileDb db = new FileDb();
            db.Create( this, dbName );
            return db;
        }

        #region SelectRecords

        #region String

        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Return a Table of Records filtered by the filter parameter.
        /// </summary>
        /// <param name="filter">A string representing the desired filter, eg. LastName = 'Fuller'</param>
        /// <returns>A new Table with the requested Records</returns>
        /// 
        public Table SelectRecords( string filter )
        {
            FilterExpressionGroup filterExpGrp = FilterExpressionGroup.Parse( filter );
            return SelectRecords( filterExpGrp );
        }

        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Return a Table of Records filtered by the filter parameter. Only the specified Fields
        /// will be in the Table.
        /// </summary>
        /// <param name="filter">A string representing the desired filter, eg. LastName = 'Fuller'</param>
        /// <param name="fieldList">The desired fields to be in the returned Table</param>
        /// <returns>A new Table with the requested Records and Fields</returns>
        /// 
        public Table SelectRecords( string filter, string[] fieldList )
        {
            FilterExpressionGroup filterExpGrp = FilterExpressionGroup.Parse( filter );
            return SelectRecords( filterExpGrp, fieldList, null );
        }

        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Return a Table of Records filtered by the filter parameter. Only the specified Fields
        /// will be in the Table.
        /// </summary>
        /// <param name="filter">A string representing the desired filter, eg. LastName = 'Fuller'</param>
        /// <param name="fieldList">The desired fields to be in the returned Table</param>
        /// <param name="orderByList">A list of one or more fields to order the returned table by, 
        /// or null for default order. If an orderByField is prefixed with "!", that field will sorted
        /// in reverse order.</param>
        /// <returns>A new Table with the requested Records and Fields ordered by the specified list</returns>
        /// 
        public Table SelectRecords( string filter, string[] fieldList, string[] orderByList )
        {
            FilterExpressionGroup filterExpGrp = FilterExpressionGroup.Parse( filter );
            return SelectRecords( filterExpGrp, fieldList, orderByList );
        }
        #endregion String

        #region FilterExpression

        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Return a Table of Records filtered by the filter parameter.
        /// </summary>
        /// <param name="filter">A FilterExpression representing the desired filter.</param>
        /// <returns>A new Table with the requested Records</returns>
        /// 
        public Table SelectRecords( FilterExpression filter )
        {
            return SelectRecords( filter, null, null );
        }

        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Return a Table of Records filtered by the filter parameter. Only the specified Fields
        /// will be in the Table.
        /// </summary>
        /// <param name="filter">A FilterExpression representing the desired filter.</param>
        /// <param name="fieldList">The desired fields to be in the returned Table</param>
        /// <returns>A new Table with the requested Records and Fields</returns>
        /// 
        public Table SelectRecords( FilterExpression filter, string[] fieldList )
        {
            return SelectRecords( filter, fieldList, null );
        }

        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Return a Table of Records filtered by the filter parameter. Only the specified Fields
        /// will be in the Table.
        /// </summary>
        /// <param name="filter">A FilterExpression representing the desired filter.</param>
        /// <param name="fieldList">The desired fields to be in the returned Table</param>
        /// <param name="orderByList">A list of one or more fields to order the returned table by, 
        /// or null for default order. If an orderByField is prefixed with "!", that field will sorted
        /// in reverse order.</param>
        /// <returns>A new Table with the requested Records and Fields ordered by the specified fields.</returns>
        /// 
        public Table SelectRecords( FilterExpression filter, string[] fieldList, string[] orderByList )
        {
            FilterExpressionGroup filterExpGrp = new FilterExpressionGroup();
            filterExpGrp.Add( BoolOpEnum.And, filter );

            return SelectRecords( filterExpGrp, fieldList, orderByList );
        }
        #endregion FilterExpression

        #region FilterExpressionGroup

        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Return a Table of Records filtered by the filter parameter.
        /// </summary>
        /// <param name="filter">A FilterExpressionGroup representing the desired filter.</param>
        /// <returns>A new Table with the requested Records</returns>
        /// 
        public Table SelectRecords( FilterExpressionGroup filter )
        {
            return SelectRecords( filter, null, null );
        }

        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Return a Table of Records filtered by the filter parameter. Only the specified Fields
        /// will be in the Table.
        /// </summary>
        /// <param name="filter">A FilterExpression representing the desired filter.</param>
        /// <param name="fieldList">The desired fields to be in the returned Table</param>
        /// <returns>A new Table with the requested Records and Fields</returns>
        /// 
        public Table SelectRecords( FilterExpressionGroup filter, string[] fieldList )
        {
            return SelectRecords( filter, fieldList, null );
        }

        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Return a Table of Records filtered by the filter parameter. Only the specified Fields
        /// will be in the Table.
        /// </summary>
        /// <param name="filter">A FilterExpression representing the desired filter.</param>
        /// <param name="fieldList">The desired fields to be in the returned Table</param>
        /// <param name="orderByList">A list of one or more fields to order the returned table by, 
        /// or null for default order. If an orderByField is prefixed with "!", that field will sorted
        /// in reverse order.</param>
        /// <returns>A new Table with the requested Records and Fields in the specified order</returns>
        /// 
        public Table SelectRecords( FilterExpressionGroup filter, string[] fieldList, string[] orderByList )
        {
            Table table = null;
            Fields fields = new Fields( _fields.Count );
            int numFields = 0;

            if( fieldList != null )
            {
                numFields = fieldList.Length;
                for( int n = 0; n < numFields; n++ )
                {
                    string fieldName = fieldList[n];
                    // Check the orderby field name
                    if( !_fields.ContainsKey( fieldName ) )
                        throw new Exception( string.Format( "Invalid field name - {0}", fieldName ) );

                    Field existingField = _fields[fieldName];
                    fields.Add( new Field( fieldName, existingField.DataType, n ) );
                }
            }
            else
            {
                numFields = _fields.Count;
                for( int n = 0; n < numFields; n++ )
                {
                    Field existingField = _fields[n];
                    fields.Add( new Field( existingField.Name, existingField.DataType, n ) );
                }
            }

            Records records = new Records( Math.Min( 10, this.Count ) );

            foreach( Record record in this )
            {
                bool isMatch = FileDbEngine.evaluate( filter, record.Values.ToArray(), _fields );

                if( isMatch )
                {
                    object[] values = new object[numFields];

                    // get the values

                    if( fieldList != null )
                    {
                        for( int n = 0; n < numFields; n++ )
                        {
                            string fieldName = fieldList[n];
                            object value = record[fieldName];
                            values[n] = value;
                        }
                    }
                    else
                    {
                        for( int n = 0; n < numFields; n++ )
                        {
                            object value = record[n];
                            values[n] = value;
                        }
                    }

                    Record newRecord = new Record( fields, values );
                    records.Add( newRecord );
                }
            }

            if( orderByList != null )
            {
                orderBy( records, fieldList, orderByList );
            }

            table = new Table( fields, records, false );

            return table;
        }
        #endregion FilterExpressionGroup

        void orderBy( Records records, string[] fieldList, string[] orderByList )
        {
            List<Field> sortFields = new List<Field>( orderByList.Length );
            List<bool> sortDirLst = new List<bool>( orderByList.Length );
            List<bool> caseLst = new List<bool>( orderByList.Length );

            FileDbEngine.GetOrderByLists( _fields, fieldList, orderByList, sortFields, sortDirLst, caseLst );

            records.Sort( new RecordComparer( sortFields, sortDirLst, caseLst ) );
        }

        #endregion Select

    }

    ///////////////////////////////////////////////////////////////////////
    #region RecordComparer
    //=====================================================================
    class RecordComparer : IComparer<Record>
    {
        List<Field> _fieldLst;
        List<bool> _sortDirLst,
                   _caseLst;

        internal RecordComparer( List<Field> fieldLst, List<bool> sortDirLst, List<bool> caseLst )
        {
            _fieldLst = fieldLst;
            _caseLst = caseLst;
            _sortDirLst = sortDirLst;
        }

        // Calls CaseInsensitiveComparer.Compare with the parameters reversed

        public int Compare( Record x, Record y )
        {
            Int32 nRet = 0;
            object v1, v2;
            Record row1 = x as Record,
                   row2 = y as Record;

            if( row1 == null || row2 == null )
                return 0;

            for( int n = 0; n < _fieldLst.Count; n++ )
            {
                Field field = _fieldLst[n];
                bool reverseSort = _sortDirLst[n];
                bool caseInsensitive = _caseLst[n];

                v1 = row1[field.Ordinal];
                v2 = row2[field.Ordinal];

                int compVal = FileDbEngine.CompareVals( v1, v2, field.DataType, caseInsensitive );

                if( reverseSort )
                    compVal = -compVal;

                // we go until we find mismatch

                if( compVal != 0 )
                {
                    nRet = compVal;
                    break;
                }
            }

            return nRet;
        }
    }
    #endregion RecordComparer

    //=====================================================================
    /// <summary>
    /// A simple class used to communicate property value changes to a Record.
    /// Used by WPF databinding.
    /// </summary>
    /// 
    public class FieldSetter
    {
        private string _propertyName;

        private object _value;

        public object Value
        {
            get { return _value; }
        }

        public string PropertyName
        {
            get { return _propertyName; }
        }

        public FieldSetter( string propertyName, object value )
        {
            _propertyName = propertyName;
            _value = value;
        }
    }
}