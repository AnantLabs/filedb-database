﻿/* Copyright (C) EzTools Software - All Rights Reserved
 * Proprietary and confidential source code.
 * This is not free software.  Any copying of this file 
 * via any medium is strictly prohibited except as allowed
 * by the FileDb license agreement.
 * Written by Brett Goodman <eztools-software.com>, October 2014
 */
using System;
using System.IO;

namespace FileDbNs
{
    //=====================================================================
    /// <summary>
    /// Represents an open FileDb database file.  All of the FileDb classes/methods are re-entrant -
    /// there is no need to syncronise access to the class objects by the calling application.
    /// However you should use the try-finally pattern when you open a FileDb to ensure
    /// prompt closing in the finally code block.
    /// </summary>
    /// 
    public partial class FileDb : IDisposable
    {
        #region static Events
        //
        /// <summary>
        /// Handler for static DbRecordUpdated event.
        /// </summary>
        /// <param name="dbFileName">The name of the updated database</param>
        /// <param name="index">The record index</param>
        /// <param name="fieldValues">The fields and new values which were updated</param>
        /// 
        public delegate void DbRecordUpdatedHandler( string dbFileName, int index, FieldValues fieldValues );
        /// <summary>
        /// Static event fired when a record has been updated.
        /// </summary>
        public static event DbRecordUpdatedHandler DbRecordUpdated;

        /// <summary>
        /// Handler for static DbRecordAdded event.
        /// </summary>
        /// <param name="dbFileName">The name of the updated database</param>
        /// <param name="index">The record index</param>
        public delegate void DbRecordAddedHandler( string dbFileName, int index );
        /// <summary>
        /// Static event fired when a record has been inserted.
        /// </summary>
        public static event DbRecordAddedHandler DbRecordAdded;

        /// <summary>
        /// Handler for static DbRecordDeleted event.
        /// </summary>
        /// <param name="dbFileName">The name of the updated database</param>
        /// <param name="index">The record index</param>
        public delegate void DbRecordDeletedHandler( string dbFileName, int index );
        /// <summary>
        /// Static event fired when a record has been deleted.
        /// </summary>
        public static event DbRecordDeletedHandler DbRecordDeleted;
        //
        #endregion static Events

        #region non-static Events
        //
        /// <summary>
        /// Handler for RecordUpdated event.
        /// </summary>
        /// <param name="index">The record index</param>
        /// <param name="fieldValues">The fields and new values which were updated</param>
        /// 
        public delegate void RecordUpdatedHandler( int index, FieldValues fieldValues );
        /// <summary>
        /// Fired when a record has been updated.
        /// </summary>
        /// 
        public event RecordUpdatedHandler RecordUpdated;

        /// <summary>
        /// Handler for RecordAdded event.
        /// </summary>
        /// <param name="index">The record index</param>
        /// 
        public delegate void RecordAddedHandler( int index );
        /// <summary>
        /// Fired when a record has been inserted.
        /// </summary>
        /// 
        public event RecordAddedHandler RecordAdded;

        /// <summary>
        /// Handler for RecordDeleted event.
        /// </summary>
        /// <param name="index">The record index</param>
        /// 
        public delegate void RecordDeletedHandler( int index );
        /// <summary>
        /// Fired when a record has been deleted.
        /// </summary>
        /// 
        public event RecordDeletedHandler RecordDeleted;
        //
        #endregion non-static Events

        #region Fields
        const string StrIndex = "index";

        internal FileDbEngine _dbEngine = new FileDbEngine();

        Encryptor _encryptor;
        int _encryptKeyHashCode = 0;

        bool _disposed;

        #endregion Fields

        #region Public Properties

        //----------------------------------------------------------------------------------------
        /// <summary>
        /// The full filename of the DB file
        /// </summary>
        /// 
        public string DbFileName
        {
            get { return _dbEngine.DbFileName; }
        }

        //----------------------------------------------------------------------------------------
        /// <summary>
        /// A value which can be used to keep track of the database version for changes
        /// </summary>
        public float UserVersion
        {
            get
            {
                return _dbEngine.UserVersion;
            }
            set
            {
                _dbEngine.UserVersion = value;
            }
        }

        //----------------------------------------------------------------------------------------
        /// <summary>
        /// The fields of the database (table).
        /// </summary>
        /// 
        public Fields Fields
        {
            get { return _dbEngine.Fields; }
        }

        //----------------------------------------------------------------------------------------
        /// <summary>
        /// The number of records in the database (table).  Doesn't include deleted records.
        /// </summary>
        /// 
        public Int32 NumRecords
        {
            get
            {
                return _dbEngine.NumRecords;
            }
        }

        //----------------------------------------------------------------------------------------
        /// <summary>
        /// The number of deleted records which not yet cleaned from the file.  Call the
        /// Clean method to remove all deleted records and compact the file.
        /// </summary>
        /// 
        public Int32 NumDeleted
        {
            get
            {
                return _dbEngine.NumDeleted;
            }
        }

        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Configures autoclean.  When an edit or delete is made, the
        /// record is normally not removed from the data file - only the index.
        /// After repeated edits/deletions, the data file may become very big with
        /// deleted (non-removed) records.  A cleanup is normally done with the
        /// cleanup() method.  Autoclean will do this automatically, keeping the
        /// number of deleted records to under the threshold value.
        /// To turn off autoclean, set threshold to a negative value.
        /// </summary>
        /// 
        public Int32 AutoCleanThreshold
        {
            get { return _dbEngine.getAutoCleanThreshold(); }
            set { _dbEngine.setAutoCleanThreshold( value ); }
        }

        /// <summary>
        /// Specifies whether to automatically flush data buffers and write the index after each
        /// operation in which the file was updated.  When AutoFlush is not On, the index is not
        /// written until the file is closed or Flush is called.
        /// You can set AutoFlush to Off just before performing a bulk operation to dramatically
        /// increase performance, then set it back On after.  When you set it back On after it was
        /// Off, everything is flushed immediately because the assumption is that it was needed,
        /// so you don't need to call Flush in this case.
        /// 
        /// Setting AutoFlush On is most useful for when you aren't able to guarantee that you will
        /// be able to call Close before the program closes.  This way the file won't become corrupt
        /// in this case.
        /// </summary>
        /// 
        public bool AutoFlush
        {
            get { return _dbEngine.AutoFlush; }
            set { _dbEngine.AutoFlush = value; }
        }

        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Tests to see if a database is currently open.
        /// </summary>
        /// 
        public bool IsOpen
        {
            get { return _dbEngine.IsOpen; }
        }

        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Allow ability to store meta data in the DB file.  MetaData must be one of the supported
        /// DataTypes: String, Byte, Int, UInt, Float, Double, Bool, DateTime and also Byte[]
        /// </summary>
        /// 
        public object MetaData
        {
            get { return _dbEngine.MetaData; }
            set
            {
                if( value != null )
                {
                    Type metaType = value.GetType();
                    if( metaType != typeof( String ) && metaType != typeof( Byte[] ) )
                        throw new FileDbException( FileDbException.InvalidMetaDataType, FileDbExceptionsEnum.InvalidMetaDataType );
                }
                _dbEngine.MetaData = value;
            }
        }

        #endregion Public Properties

        #region Constructors

        /// <summary>
        /// Constructor for FileDb
        /// </summary>
        /// 
        public FileDb()
        {
            AutoFlush = true;
            _dbEngine.RecordUpdated += onDbUpdated;
            _dbEngine.RecordAdded += onRecordAdded;
            _dbEngine.RecordDeleted += onRecordDeleted;
        }

        void onDbUpdated( int index, FieldValues fieldValues )
        {
            if( RecordUpdated != null )
                RecordUpdated( index, fieldValues );

            if( DbRecordUpdated != null )
                DbRecordUpdated( _dbEngine.DbFileName, index, fieldValues );
        }

        void onRecordAdded( int index )
        {
            if( RecordAdded != null )
                RecordAdded( index );

            if( DbRecordAdded != null )
                DbRecordAdded( _dbEngine.DbFileName, index );
        }

        void onRecordDeleted( int index )
        {
            if( RecordDeleted != null )
                RecordDeleted( index );

            if( DbRecordDeleted != null )
                DbRecordDeleted( _dbEngine.DbFileName, index );
        }

        #endregion Constructors

        #region Overrides

        /// <summary>
        /// ToString override - returns the DB filename or a string indicating its a memory DB
        /// </summary>
        /// <returns></returns>
        /// 
        public override string ToString()
        {
            return _dbEngine.DbFileName;
        }

        #endregion Overrides

        #region IDisposable

        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        //
        public void Dispose()
        {
            Dispose( true );

            // This object will be cleaned up by the Dispose method.
            // Therefore you should call GC.SupressFinalize to take this object off the finalization queue
            // and prevent finalization code for this object from executing a second time.

            GC.SuppressFinalize( this );
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        //
        protected virtual void Dispose( bool disposing )
        {
            // Check to see if Dispose has already been called.

            if( !this._disposed )
            {
                // If disposing equals true, dispose all managed and unmanaged resources.

                if( disposing )
                {
                    // Dispose managed resources.
                    Close();
                }

                // Call the appropriate methods to clean up unmanaged resources here.
                // If disposing is false, only the following code is executed.
                // if this is used, implement C# destructor and call Dispose(false) in it
                // e.g. CloseHandle( handle );

                // Note disposing has been done.
                _disposed = true;
            }
        }

        ~FileDb()
        {
            Dispose( false );
        }

        #endregion IDisposable

        #region Private Methods

        #if NETFX_CORE || PCL

        // Create database from a Table.  Called by Table.SaveToDb
        //
        internal void CreateFromTable( Table table, Stream dataStream )
        {
            _dbEngine.create( dataStream, table.Fields.ToArray() );

            foreach( Record record in table )
            {
                FieldValues fieldValues = record.GetFieldValues();
                AddRecord( fieldValues );
            }
        }
        #else
        // Create database from a Table.  Called by Table.SaveToDb
        //
        internal void CreateFromTable( Table table, string dbFileName )
        {
            _dbEngine.create( dbFileName, table.Fields.ToArray() );

            foreach( Record record in table )
            {
                FieldValues fieldValues = record.GetFieldValues();
                AddRecord( fieldValues );
            }
        }
        #endif

        //----------------------------------------------------------------------------------------
        // Create a table from the raw records
        //
        private Table createTable( object[][] records, string[] fieldList, bool includeIndex, string[] orderByList )
        {
            Table table = null;

            int nExtra = includeIndex ? 1 : 0;
            Fields fields = null;
            if( fieldList != null )
            {
                fields = new Fields( fieldList.Length + nExtra );
                int n = 0;
                foreach( string fieldName in fieldList )
                {
                    if( fields.ContainsKey( fieldName ) )
                        throw new FileDbException( string.Format( FileDbException.FieldSpecifiedTwice, fieldName ),
                            FileDbExceptionsEnum.FieldSpecifiedTwice );
                    var field = _dbEngine.Fields[fieldName].Clone();
                    fields.Add( field );

                    // fix up the ordinal index
                    #if DEBUG
                    if( field.Ordinal != n ) { }
                    #endif
                    field.Ordinal = n++;
                }
            }
            else
            {
                fields = new Fields( _dbEngine.Fields.Count + nExtra );
                foreach( Field field in _dbEngine.Fields )
                {
                    fields.Add( field.Clone() );
                }
            }

            if( includeIndex )
                fields.Add( new Field( StrIndex, DataTypeEnum.Int32, fields.Count ) ); 

            table = new Table( fields, records, true );

            return table;
        }

        #endregion Private Methods

        #region Public Methods

        #region Open/Close/Drop/Exists

        #if NETFX_CORE || PCL
        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Open with an existing database stream
        /// </summary>
        /// <param name="stream">The database stream to use - normally a FileStream</param>
        /// 
        public void Open( Stream stream )
        {
            lock( this )
            {
                _dbEngine.open( stream, null, null );
            }
        }
        #else
        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Open the indicated database file.
        /// If dbFileName is null an in-memory database will be created.
        /// </summary>
        /// <param name="dbFileName">The filename of the database file to open.
        /// It can be a fully qualified path or, if no path is specified the current folder will be used.</param>
        /// <param name="readOnly">Indicates whether to open read only or not</param>
        /// 
        public void Open( string dbFileName, bool readOnly )
        {
            lock( this )
            {
                _dbEngine.open( dbFileName, null, null, null, readOnly );
            }
        }

        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Open the indicated database file for encryption. Encryption is "all or nothing",
        /// meaning all records are either encrypted or not.
        /// If dbFileName is null an in-memory database will be created.
        /// </summary>
        /// <param name="dbFileName">The filename of the database file to open.
        /// It can be a fully qualified path or, if no path is specified the current folder will be used.</param>
        /// <param name="encryptionKey">A string value to use as the encryption key</param>
        /// <param name="readOnly">Indicates whether to open read only or not</param>
        /// 
        public void Open( string dbFileName, string encryptionKey, bool readOnly )
        {
            lock( this )
            {
                _dbEngine.open( dbFileName, null, encryptionKey, null, readOnly );
            }
        }
        #endif

        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Close an open database.
        /// </summary>
        /// 
        public void Close()
        {
            lock( this )
            {
                _dbEngine.close();
            }
        }

        #if NETFX_CORE || PCL

        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Create a new database using the passed stream, or if null and in-memory DB
        /// </summary>
        /// <param name="stream">The stream to use or null to create a memory DB</param>
        /// <param name="fields">Array of Fields for the new database.</param>
        /// 
        public void Create( Stream stream, Field[] fields )
        {
            lock( this )
            {
                _dbEngine.create( stream, fields );
            }
        }

        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Create a new database using the passed stream, or if null and in-memory DB
        /// </summary>
        /// <param name="stream">The stream to use or null to create a memory DB</param>
        /// <param name="fields">List of Fields for the new database.</param>
        /// 
        public void Create( Stream stream, Fields fields )
        {
            lock( this )
            {
                _dbEngine.create( stream, fields.ToArray() );
            }
        }
        #else

        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Create a new database file. If the file exists, it will be overwritten.
        /// If dbFileName is null an in-memory database will be created.
        /// </summary>
        /// <param name="dbFileName">The full pathname of the file or null/empty to create a memory DB</param>
        /// <param name="fields">Array of Fields for the new database.</param>
        /// 
        public void Create( string dbFileName, Field[] fields )
        {
            lock( this )
            {
                _dbEngine.create( dbFileName, fields );
            }
        }

        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Create a new database file. If the file exists, it will be overwritten.
        /// If dbFileName is null an in-memory database will be created.
        /// </summary>
        /// <param name="dbFileName">The full pathname of the file or null/empty to create a memory DB</param>
        /// <param name="fields">List of Fields for the new database.</param>
        /// 
        public void Create( string dbFileName, Fields fields )
        {
            lock( this )
            {
                _dbEngine.create( dbFileName, fields.ToArray() );
            }
        }

        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Delete an existing database.
        /// </summary>
        /// <param name="dbFileName">The pathname of the file to delete.</param>
        /// TODO: make static
        public void Drop( string dbFileName )
        {
            lock( this )
            {
                _dbEngine.drop( dbFileName );
            }
        }
        #endif

        #if !(NETFX_CORE || PCL)
        /// <summary>
        /// Indicates if the DB filename's existence
        /// </summary>
        /// <param name="dbFileName"></param>
        /// <returns>True if the file exists, false otherwise</returns>
        /// 
        public static bool Exists( string dbFileName )
        {
            return FileDbEngine.exists( dbFileName );
        }
        #endif

        #endregion Open/Close/Drop

        #region Transaction

        /// <summary>
        /// Start a transaction - a backup of the whole database file is made until the transaction is completed.
        /// Be sure to call either CommitTrans or RollbackTrans so the backup can be disposed
        /// </summary>
        /// 
        public void BeginTrans()
        {
            lock( this )
            {
                _dbEngine.beginTrans();
            }
        }

        /// <summary>
        /// Commit the changes since the transaction was begun
        /// </summary>
        /// 
        public void CommitTrans()
        {
            lock( this )
            {
                _dbEngine.commitTrans();
            }
        }

        /// <summary>
        /// Roll back the changes since the transaction was begun
        /// </summary>
        /// 
        public void RollbackTrans()
        {
            lock( this )
            {
                _dbEngine.rollbackTrans();
            }
        }

        #endregion Transaction

        #region Add
        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Add a new record to the database using the name-value pairs in the FieldValues object.
        /// Note that not all fields must be represented.  Missing fields will be set to default
        /// values (0, empty or null).  Note that only Array datatypes can NULL.
        /// </summary>
        /// <param name="values">The name-value pairs to add.</param>
        /// <returns>The volatile index of the newly added record.</returns>
        /// 
        public int AddRecord( FieldValues values )
        {
            lock( this )
            {
                return _dbEngine.addRecord( values );
            }
        }
        #endregion Add

        #region SelectRecords

        #region SelectRecords FilterExpression

        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Return a Table of Records filtered by the filter parameter.
        /// </summary>
        /// <param name="filter">A FilterExpression representing the desired filter.</param>
        /// <returns>A new Table with the requested Records</returns>
        ///
        public Table SelectRecords( FilterExpression filter )
        {
            return SelectRecords( filter, null, null, false );
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
            return SelectRecords( filter, fieldList, null, false );
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
            return SelectRecords( filter, fieldList, orderByList, false );
        }

        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Get all records matching the search expression in the indicated order, if any.
        /// </summary>
        /// <param name="filter">Represents a single search expression, such as ID = 3</param>
        /// <param name="fieldList">The list of fields to return or null for all fields</param>
        /// <param name="includeIndex">If true, an additional Field named "index" will be returned
        /// which is the ordinal index of the Record in the database, which can be used in
        /// GetRecordByIndex and UpdateRecordByIndex.</param>
        /// <param name="orderByList">A list of one or more fields to order the returned table by, 
        /// or null for default order. If an orderByField is prefixed with "!", that field will sorted
        /// in reverse order.</param>
        /// <returns>A new Table with the requested Records and Fields</returns>
        /// 
        public Table SelectRecords( FilterExpression filter, string[] fieldList, string[] orderByList, bool includeIndex )
        {
            lock( this )
            {
                object[][] records = _dbEngine.getRecordByField( filter, fieldList, includeIndex, orderByList );
                return createTable( records, fieldList, includeIndex, orderByList );
            }
        }

        #endregion SelectRecords FilterExpression

        #region SelectRecords FilterExpressionGroup

        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Return a Table of Records filtered by the filter parameter.
        /// </summary>
        /// <param name="filter">A FilterExpressionGroup representing the desired filter.</param>
        /// <returns>A new Table with the requested Records</returns>
        /// 
        public Table SelectRecords( FilterExpressionGroup filter )
        {
            return SelectRecords( filter, null, null, false );
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
            return SelectRecords( filter, fieldList, null, false );
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
            return SelectRecords( filter, fieldList, orderByList, false );
        }

        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Get all records matching the FilterExpressionGroup in the indicated order, if any.
        /// </summary>
        /// <param name="filter">Represents a compound search expression, such as FirstName = "John" AND LastName = "Smith"</param>
        /// <param name="fieldList">The list of fields to return or null for all fields</param>
        /// <param name="includeIndex">Specify whether to include the record index as one of the Fields</param>
        /// <param name="orderByList">A list of one or more fields to order the returned table by, 
        /// or null for default order. If an orderByField is prefixed with "!", that field will sorted
        /// in reverse order.</param>
        /// <returns>A new Table with the requested Records and Fields</returns>
        ///
        public Table SelectRecords( FilterExpressionGroup filter, string[] fieldList, string[] orderByList, bool includeIndex )
        {
            lock( this )
            {
                object[][] records = _dbEngine.getRecordByFields( filter, fieldList, includeIndex, orderByList );
                return createTable( records, fieldList, includeIndex, orderByList );
            }
        }

        #endregion SelectRecords FilterExpressionGroup

        #region SelectRecords string
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
            return SelectRecords( filterExpGrp, fieldList );
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

        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Return a Table of Records filtered by the filter parameter.
        /// </summary>
        /// <param name="filter">A string representing the desired filter, eg. LastName = 'Fuller'</param>
        /// <param name="fieldList">The desired fields to be in the returned Table</param>
        /// <param name="includeIndex">If true, an additional Field named "index" will be returned
        /// which is the ordinal index of the Record in the database, which can be used in
        /// GetRecordByIndex and UpdateRecordByIndex</param>
        /// <param name="orderByList">A list of one or more fields to order the returned table by, 
        /// or null for default order. If an orderByField is prefixed with "!", that field will sorted
        /// in reverse order</param>
        /// <returns>A new Table with the requested Records and Fields</returns>
        /// 
        public Table SelectRecords( string filter, string[] fieldList, string[] orderByList, bool includeIndex )
        {
            FilterExpressionGroup filterExpGrp = FilterExpressionGroup.Parse( filter );
            return SelectRecords( filterExpGrp, fieldList, orderByList, includeIndex );
        }
        #endregion SelectRecords string

        #region SelectAllRecords
        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Return all records in the database (table).
        /// </summary>
        /// <returns>A table containing all Records and Fields.</returns>
        /// 
        public Table SelectAllRecords()
        {
            return SelectAllRecords( null, null, false );
        }

        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Return all records in the database (table).
        /// </summary>
        /// <param name="fieldList">The list of Fields to return or null for all Fields</param>
        /// <returns>A table containing all rows.</returns>
        /// 
        public Table SelectAllRecords( string[] fieldList )
        {
            return SelectAllRecords( fieldList, null, false );
        }

        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Return all records in the database (table).
        /// </summary>
        /// <param name="fieldList">The list of fields to return or null for all Fields</param>
        /// <param name="orderByList">A list of one or more fields to order the returned table by, 
        /// or null for default order</param>
        /// <returns>A table containing all rows.</returns>
        /// 
        public Table SelectAllRecords( string[] fieldList, string[] orderByList )
        {
            return SelectAllRecords( fieldList, orderByList, false );
        }

        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Return all records in the database (table).
        /// </summary>
        /// <param name="includeIndex">Specify whether to include the Record index as one of the Fields</param>
        /// <returns>A table containing all rows.</returns>
        /// 
        public Table SelectAllRecords( bool includeIndex )
        {
            return SelectAllRecords( null, null, includeIndex );
        }

        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Return all records in the database (table).
        /// </summary>
        /// <param name="fieldList">The list of fields to return or null for all fields</param>
        /// <param name="includeIndex">Specify whether to include the record index as one of the Fields</param>
        /// <param name="orderByList">A list of one or more fields to order the returned table by, 
        /// or null for default order</param>
        /// <returns>A table containing all Records and the specified Fields.</returns>
        /// 
        public Table SelectAllRecords( string[] fieldList, string[] orderByList, bool includeIndex )
        {
            lock( this )
            {
                object[][] records = _dbEngine.getAllRecords( fieldList, includeIndex, orderByList );
                return createTable( records, fieldList, includeIndex, orderByList );
            }
        }
        #endregion SelectAllRecords

        #region SelectNoRecords

        /// <summary>
        /// Sometimes you may need to get an empty table just for the field definitions.
        /// Use this method because its much more efficient than using a contrived filter 
        /// which is designed to return no results.
        /// </summary>
        /// <returns>An empty table containing all fields</returns>
        /// 
        public Table SelectEmptyTable()
        {
            var table = new Table( _dbEngine.Fields, true );
            return table;
        }

        #endregion SelectNoRecords

        #endregion SelectRecords

        #region GetRecord

        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Returns a single Record object at the current location.  Meant to be used ONLY in conjunction
        /// with the MoveFirst/MoveNext methods.
        /// </summary>
        /// <param name="fieldList">The list of fields to return or null for all fields</param>
        /// <param name="includeIndex">Specify whether to include the record index as one of the Fields</param>
        /// <returns>A Record object or null</returns>
        /// 
        public Record GetCurrentRecord( string[] fieldList, bool includeIndex )
        {
            lock( this )
            {
                object[] record = _dbEngine.getCurrentRecord( includeIndex );
                return createRecord( record, fieldList, includeIndex );
            }
        }

        //----------------------------------------------------------------------------------------
        /// 
        /// <summary>
        /// Returns a single Record object specified by the index.
        /// </summary>
        /// <param name="index">The index of the record to return. This value can be obtained from
        /// Record returning queries by specifying true for the includeIndex parameter.</param>
        /// <param name="fieldList">The list of fields to return or null for all fields</param>
        /// <returns>A Record object or null</returns>
        /// 
        public Record GetRecordByIndex( Int32 index, string[] fieldList )
        {
            lock( this )
            {
                object[] record = _dbEngine.getRecordByIndex( index, fieldList, false );
                return createRecord( record, fieldList, false );
            }
        }

        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Returns a single Record object specified by the primary key value or record number.
        /// </summary>
        /// <param name="key">The primary key value.  For databases without a primary key, 
        /// 'key' is the zero-based record number in the table.</param>
        /// <param name="fieldList">The list of fields to return or null for all fields</param>
        /// <param name="includeIndex">Specify whether to include the record index as one of the Fields</param>
        /// <returns>A Record object or null</returns>
        /// 
        public Record GetRecordByKey( object key, string[] fieldList, bool includeIndex )
        {
            lock( this )
            {
                object[] record = _dbEngine.getRecordByKey( key, fieldList, includeIndex );
                return createRecord( record, fieldList, includeIndex );
            }
        }

        Record createRecord( object[] record, string[] fieldList, bool includeIndex )
        {
            Record row = null;
            
            if( record != null )
            {
                int nExtra = includeIndex ? 1 : 0;
                Fields fields = null;
                if( fieldList != null )
                {
                    fields = new Fields( fieldList.Length + nExtra );
                    int n = 0;
                    foreach( string fieldName in fieldList )
                    {
                        if( fields.ContainsKey( fieldName ) )
                            throw new FileDbException( string.Format( FileDbException.FieldSpecifiedTwice, fieldName ), FileDbExceptionsEnum.FieldSpecifiedTwice );
                        var field = _dbEngine.Fields[fieldName].Clone();
                        fields.Add( field );

                        // fix up the ordinal index
                        #if DEBUG
                        if( field.Ordinal != n ) { }
                        #endif
                        field.Ordinal = n++;
                    }
                }
                else
                {
                    fields = new Fields( _dbEngine.Fields.Count + nExtra );
                    foreach( Field field in _dbEngine.Fields )
                    {
                        fields.Add( field.Clone() );
                    }
                }

                if( includeIndex )
                    fields.Add( new Field( StrIndex, DataTypeEnum.Int32, fields.Count ) );

                row = new Record( fields, record );
            }

            return row;
        }
        
        #endregion GetRecord

        #region Update

        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Update the record at the indicated index. To get the index, you would need to first
        /// get a record from the database then use the index field from it.  The index is only
        /// valid until a database operation which would invalidate it, such as adding/deleting
        /// a record, or changing the value of a primary key.
        /// </summary>
        /// <param name="values">The record values to update</param>
        /// <param name="index">The index of the record to update</param>
        /// 
        public void UpdateRecordByIndex( Int32 index, FieldValues values )
        {
            lock( this )
            {
                _dbEngine.updateRecordByIndex( values, index );
            }
        }

        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Update the record with the indicated primary key value.
        /// </summary>
        /// <param name="values">The record values to update</param>
        /// <param name="key">The primary key value of the record to update</param>
        /// 
        public void UpdateRecordByKey( object key, FieldValues values )
        {
            lock( this )
            {
                _dbEngine.updateRecordByKey( values, key );
            }
        }

        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Update all records which match the search criteria using the values in record.
        /// </summary>
        /// <param name="filter">The search expression, e.g. ID = 100</param>
        /// <param name="values">A list of name-value pairs to use to update the matching records</param>
        /// <returns>The number of records which were updated.</returns>
        /// 
        public Int32 UpdateRecords( FilterExpression filter, FieldValues values )
        {
            lock( this )
            {
                return _dbEngine.updateRecords( filter, values );
            }
        }

        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Update all records which match the compound search criteria using the values in record.
        /// </summary>
        /// <param name="filter">The compound search expression, e.g. FirstName = "John" AND LastName = "Smith"</param>
        /// <param name="values">A list of name-value pairs to use to update the matching records</param>
        /// <returns>The number of records which were updated.</returns>
        /// 
        public Int32 UpdateRecords( FilterExpressionGroup filter, FieldValues values )
        {
            lock( this )
            {
                return _dbEngine.updateRecords( filter, values );
            }
        }

        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Update all records which match the filter expression using the values in record.
        /// </summary>
        /// <param name="filter">The filter to use, eg. "~LastName = 'peacock' OR ~FirstName = 'nancy'".
        /// This filter string will be parsed using FilterExpressionGroup.Parse.</param>
        /// <param name="values">A list of name-value pairs to use to update the matching records</param>
        /// <returns>The number of records which were updated.</returns>
        /// 
        public Int32 UpdateRecords( string filter, FieldValues values )
        {
            lock( this )
            {
                FilterExpressionGroup filterExpGrp = FilterExpressionGroup.Parse( filter );
                return _dbEngine.updateRecords( filterExpGrp, values );
            }
        }
        
        #endregion Update

        #region Delete

        /// <summary>
        /// Delete all records in the database
        /// </summary>
        /// <returns>The number of records deleted</returns>
        /// 
        public Int32 DeleteAllRecords()
        {
            lock( this )
            {
                return _dbEngine.removeAll();
            }
        }

        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Delete the record at the specified index.  You would normally get the index from a previous
        /// query.  This index is only valid until a record has been deleted.
        /// </summary>
        /// <param name="index">The zero-based index of the record to delete.</param>
        /// <returns>true if the record was deleted, false otherwise</returns>
        /// 
        public bool DeleteRecordByIndex( Int32 index )
        {
            lock( this )
            {
                return _dbEngine.removeByIndex( index );
            }
        }

        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Delete the record with the specified primary key value
        /// </summary>
        /// <param name="key">The primary key value of the record to delete</param>
        /// <returns>true if the record was deleted, false otherwise</returns>
        /// 
        public bool DeleteRecordByKey( object key )
        {
            lock( this )
            {
                return _dbEngine.removeByKey( key );
            }
        }

        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Delete all records which match the search criteria.
        /// </summary>
        /// <param name="filter">The search expression, e.g. ID = 100</param>
        /// <returns>The number of records deleted</returns>
        /// 
        public Int32 DeleteRecords( FilterExpression filter )
        {
            lock( this )
            {
                return _dbEngine.removeByValue( filter );
            }
        }

        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Delete all records which match the compound search criteria.
        /// </summary>
        /// <param name="filter">The compound search expression, e.g. FirstName = "John" AND LastName = "Smith"</param>
        /// <returns>The number of records deleted</returns>
        /// 
        public Int32 DeleteRecords( FilterExpressionGroup filter )
        {
            lock( this )
            {
                return _dbEngine.removeByValues( filter );
            }
        }

        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Delete all records which match the filter criteria.
        /// </summary>
        /// <param name="filter">The filter to use, eg. "~LastName = 'peacock' OR ~FirstName = 'nancy'".
        /// This filter string will be parsed using FilterExpressionGroup.Parse.</param>
        /// <returns>The number of records deleted</returns>
        /// 
        public Int32 DeleteRecords( string filter )
        {
            lock( this )
            {
                FilterExpressionGroup filterExpGrp = FilterExpressionGroup.Parse( filter );
                return _dbEngine.removeByValues( filterExpGrp );
            }
        }

        #endregion Delete

        #region Iteration
        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Move to the first record in the index.  Use this in conjunction with MoveNext and GetCurrentRecord
        /// </summary>
        /// 
        public bool MoveFirst()
        {
            lock( this )
            {
                return _dbEngine.moveFirst();
            }
        }

        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Move to the next record in the index.  Use this in conjunction with MoveFirst and GetCurrentRecord
        /// </summary>
        public bool MoveNext()
        {
            lock( this )
            {
                return _dbEngine.moveNext();
            }
        }
        #endregion Iteration

        #region Maintenance
        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Call this to remove deleted records from the file (compact).
        /// </summary>
        /// 
        public void Clean()
        {
            lock( this )
            {
                _dbEngine.cleanup( false );
            }
        }

        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Call this to write the index and flush the stream buffer to disk.
        /// Flushing will be done automatically if AutoFlush is On (and only writes the index
        /// if necessary), whereas this call always writes the index.
        /// You can use this to periodically write everything to disk rather than each time
        /// as with AutoFlush.  Flush is always called when the file is closed, however in that
        /// case the index is only written if AutoFlush is set to Off.
        /// </summary>
        /// 
        public void Flush()
        {
            lock( this )
            {
                _dbEngine.flush( true );
            }
        }

        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Call this method to reindex the database if your index file should be deleted or corrupted.
        /// </summary>
        /// 
        public void Reindex()
        {
            lock( this )
            {
                _dbEngine.reindex();
            }
        }

        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Add the specified Field to the database.
        /// </summary>
        /// <param name="newField">The new Field to add</param>
        /// <param name="defaultVal">A default value to use for the values of existing records for the new Field</param>
        /// 
        public void AddField( Field newField, object defaultVal )
        {
            lock( this )
            {
                object[] defaultVals = null;
                if( defaultVal != null )
                    defaultVals = new object[] { defaultVal };
                this.AddFields( new Field[] { newField }, defaultVals );
            }
        }

        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Add the specified Field to the database.
        /// </summary>
        /// <param name="newFields">The new Fields to add</param>
        /// <param name="defaultVals">Default values to use for the values of existing records for the new Fields.
        /// Can be null but if not then you must provide a value for each field in the Fields array</param>
        /// 
        public void AddFields( Field[] newFields, object[] defaultVals )
        {
            lock( this )
            {
                _dbEngine.addFields( this, newFields, defaultVals );
            }
        }

        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Delete the specified Field from the database.
        /// </summary>
        /// <param name="fieldName">The name of the Field to delete</param>
        /// 
        public void DeleteField( string fieldName )
        {
            lock( this )
            {
                this.DeleteFields( new string[] { fieldName } );
            }
        }

        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Delete the specified Fields from the database.
        /// </summary>
        /// <param name="fieldNames">The Fields to delete</param>
        /// 
        public void DeleteFields( string[] fieldNames )
        {
            lock( this )
            {
                _dbEngine.deleteFields( this, fieldNames );
            }
        }

        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Rename the specified Field.
        /// </summary>
        /// <param name="fieldName">The name of the Field to rename</param>
        /// 
        public void RenameField( string fieldName, string newFieldName )
        {
            lock( this )
            {
                _dbEngine.renameField( this, fieldName, newFieldName );
            }
        }

        #endregion Maintenance

        #region Encryption

        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Allows you to set an encryption key after the database has been opened.  You must set
        /// the encryption key before reading or writing to the database.  Encryption is "all or nothing",
        /// meaning all records are either encrypted or not.
        /// </summary>
        /// <param name="encryptionKey">A string value to use as the encryption key</param>
        /// 
        public void SetEncryptionKey( string encryptionKey )
        {
            lock( this )
            {
                _dbEngine.setEncryptionKey( encryptionKey );
            }
        }

        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Encrypt a string value.
        /// Not syncronized.
        /// </summary>
        /// <param name="encryptKey">The key to use for encryption</param>
        /// <param name="value">The value to encrypt</param>
        /// <returns>The encrypted value as a string</returns>
        /// 
        public string EncryptString( string encryptKey, string value )
        {
            string str = null;
            MemoryStream inStrm = new MemoryStream();

            int encryptKeyHashCode = encryptKey.GetHashCode();

            if( _encryptor == null || _encryptKeyHashCode != encryptKeyHashCode )
                _encryptor = new Encryptor( encryptKey, this.GetType().ToString() );

            _encryptKeyHashCode = encryptKeyHashCode;

            using( BinaryWriter writer = new BinaryWriter( inStrm ) )
            {
                writer.Write( value );
                byte[] bytes = _encryptor.Encrypt( inStrm.ToArray() );
                str = Utils.HexEncoding.ToString( bytes );
            }
            return str;
        }

        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Decrypt a string value.
        /// Not syncronized.
        /// </summary>
        /// <param name="encryptKey">The key to use for decryption</param>
        /// <param name="value">The value to decrypt</param>
        /// <returns>The decrypted value as a string</returns>
        /// 
        public string DecryptString( string encryptKey, string value )
        {
            string str = null;

            int encryptKeyHashCode = encryptKey.GetHashCode();

            if( _encryptor == null || _encryptKeyHashCode != encryptKeyHashCode )
                _encryptor = new Encryptor( encryptKey, this.GetType().ToString() );

            _encryptKeyHashCode = encryptKeyHashCode;

            int discarded;
            byte[] bytes = Utils.HexEncoding.GetBytes( value, out discarded );
            bytes = _encryptor.Decrypt( bytes );
            MemoryStream outStrm = new MemoryStream( bytes );
            using( BinaryReader reader = new BinaryReader( outStrm ) )
            {
                str = reader.ReadString();
            }
            return str;
        }

        #endregion Encryption

        #endregion Public Methods
    }
}
