using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;

/* TODO
    
    Support for array field searches by adding BoolOp.In 
*/

namespace FileDbNs
{
    //=====================================================================
    internal class FileDbEngine
    {
        ///////////////////////////////////////////////////////////////////////
        #region Consts

        const string StrIndex = "index";

        const int NoLock = 0,
                  ReadLock = 1,
                  WriteLock = 2;

        // Automatically incremented Int32 type
        internal const Int32 AutoIncField = 0x1;

        // Array type
        internal const Int32 ArrayField = 0x2;       

        // Major version of the FFDB package
        const byte VERSION_MAJOR = 2;

        // Minor version of the FFDB package
        const byte VERSION_MINOR = 0;

        // Signature to help validate file
        const Int32 SIGNATURE = 0x0123BABE;

        // Location of the 'records count' offset in the FFDB index. Internal use only.
        const Int32 SCHEMA_OFFSET = 6;
        const Int32 NUM_RECS_OFFSET = SCHEMA_OFFSET;

        // Location of the 'deleted count' offset in the FFDB index.
        // Always the next 'Int32 size' offset after the 'records count' offset.
        // Internal use only.
        const Int32 INDEX_DELETED_OFFSET = SCHEMA_OFFSET + 4;

        // Location of the Index offset, which is always written at the end of the data file
        const Int32 INDEX_OFFSET = INDEX_DELETED_OFFSET + 4;

        // Size of the field specifing the size of a record in the index.
        // Internal use only.
        const Int32 INDEX_RBLOCK_SIZE = 4;

        #endregion Consts

        ///////////////////////////////////////////////////////////////////////
        #region Fields

        bool _isOpen,
             _disposed,
             _openReadOnly;

        string _dbName;

#if SILVERLIGHT
        IsolatedStorageFileStream _dataStrm;
#else
        FileStream _dataStrm;
#endif
        BinaryReader _dataReader;

        BinaryWriter _dataWriter;

        MemoryStream _testStrm;
        BinaryWriter _testWriter;

        Encryptor _encryptor;

        Int32 _numRecords,
              _numDeleted,
              _autoCleanThreshold,
              _dataStartPos,
              _indexStartPos,
              _iteratorIndex;

        Int32 _ver_major,
              _ver_minor;

        Fields _fields;

        string _primaryKey;

        Field _primaryKeyField;

        List<Int32> _index,
                    _deletedRecords;

        object _metaData;

        #endregion Fields

        ///////////////////////////////////////////////////////////////////////
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
                // If disposing equals true, dispose all managed and unmanaged resources

                if( disposing )
                {
                    // Dispose managed resources.
                    close();
                }

                // Call the appropriate methods to clean up unmanaged resources here.
                // If disposing is false, only the following code is executed.
                // if this is used, implement C# destructor and call Dispose(false) in it

                // e.g. CloseHandle( handle );

                // Note disposing has been done.
                _disposed = true;
            }
        }

        ~FileDbEngine()
        {
            Dispose( false );
        }

        #endregion IDisposable

        ///////////////////////////////////////////////////////////////////////
        #region internal

        #region Properties

        internal string DateTimeFmt
        {
            get; set;
        }

        internal Fields Fields
        {
            get { checkIsDbOpen(); return _fields; }
        }

        internal Int32 NumDeleted
        {
            get { checkIsDbOpen(); return _numDeleted; }
            set { _numDeleted = value; }
        }

        internal Int32 NumRecords
        {
            get { checkIsDbOpen(); return _numRecords; }
            set { _numRecords = value; }
        }

        internal bool IsOpen
        {
            get { return _isOpen; }
            set { _isOpen = value; }
        }

        internal bool AutoFlush { get; set; }

        internal object MetaData
        {
            get { return _metaData; }
            set { _metaData = value; }
        }

        #endregion Properties

        /// <summary>
        /// Constructor
        /// </summary>
        internal FileDbEngine()
        {
            _autoCleanThreshold = -1;
            _isOpen = false;
            _openReadOnly = false;
            AutoFlush = false;
            DateTimeFmt = "yyyy-MM-dd hh:mm:ss.ffff";
        }

        internal void setEncryptionKey( string encryptionKey )
        {
            _encryptor = new Encryptor( encryptionKey, this.GetType().ToString() );
        }

        internal void open( string dbName, string encryptionKey, Encryptor encryptor, bool readOnly )
        {
            // Close existing databases first
            if( _isOpen )
                close();

            _openReadOnly = readOnly;

            try
            {
                // Open the database files

                openFiles( dbName, FileMode.Open );

                _isOpen = true;
                _dbName = dbName;
                _iteratorIndex = 0;

                _ver_major = 0;
                _ver_minor = 0;

                try
                {
                    //lockRead( false );

                    // Read and verify the signature

                    Int32 sig = _dataReader.ReadInt32();
                    if( sig != SIGNATURE )
                    {
                        throw new FileDbException( FileDbException.InvalidDatabaseSignature, FileDbExceptions.InvalidDatabaseSignature );
                    }

                    // Read the version
                    _ver_major = _dataReader.ReadByte();
                    _ver_minor = _dataReader.ReadByte();

                    // Make sure we only read databases of the same major version or less,
                    // because major version change means file format changed

                    if( _ver_major > VERSION_MAJOR )
                    {
                        throw new FileDbException( string.Format( FileDbException.CantOpenNewerDbVersion,
                                        _ver_major, _ver_minor, VERSION_MAJOR ), FileDbExceptions.CantOpenNewerDbVersion );
                    }

                    /* REVIEW: I think we will go by major version only for compatibility
                    if( ver_minor > VERSION_MINOR )
                    {
                        throw new SimpleDBException( "Cannot open database (of version ver_major.ver_minor), "
                          ."wrong version.", 
                           E_USER_ERROR
                        );
                    }*/

                    // Read the schema and database statistics
                    readSchema( _dataReader );

                    _index = readIndex();

                    if( encryptor != null )
                        _encryptor = encryptor;

                    else if( !string.IsNullOrEmpty( encryptionKey ) )
                        _encryptor = new Encryptor( encryptionKey, this.GetType().ToString() );


                    // now if the major version is older we must update the schema
                    // which means we must call cleanup to do the job for us

                    if( _ver_major < VERSION_MAJOR )
                        cleanup( true );
                }
                finally
                {
                    //unlock( false, false );
                }
            }
            catch( FileDbException ex )
            {
                close();
                throw ex;
            }
        }

        /// <summary>
        /// Open the database files
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="mode"></param>
        /// 
        void openFiles( string dbName, FileMode mode )
        {
            // Open the database files
            #if SILVERLIGHT
            IsolatedStorageFile isoFile = IsolatedStorageFile.GetUserStoreForApplication();
            if( !(mode == FileMode.Create || mode == FileMode.CreateNew || mode == FileMode.OpenOrCreate) &&
                !isoFile.FileExists( dbName ) )
            #else
            if( !(mode == FileMode.Create || mode == FileMode.CreateNew || mode == FileMode.OpenOrCreate) &&
                !File.Exists( dbName ) )
            #endif
                throw new FileDbException( FileDbException.DatabaseFileNotFound, FileDbExceptions.DatabaseFileNotFound );

            FileAccess access;
            if( _openReadOnly )
                access = FileAccess.Read;
            else
                access = FileAccess.ReadWrite;

            #if SILVERLIGHT
            _dataStrm = new IsolatedStorageFileStream( dbName, mode, access, isoFile );
            #else
            _dataStrm = File.Open( dbName, mode, access, FileShare.None );
            #endif

            _dataReader = new BinaryReader( _dataStrm );
            if( !_openReadOnly )
                _dataWriter = new BinaryWriter( _dataStrm );

            #if SILVERLIGHT
            isoFile.Dispose();
            isoFile = null;
            #endif
        }

        internal void close()
        {
            if( _isOpen )
            {
                try
                {
                    flush();
                    _dataStrm.Close();
                    _dataStrm.Dispose();
                }
                finally
                {
                    _autoCleanThreshold = -1;
                    _dataStartPos = 0;
                    _dataStrm = null;
                    _dataWriter = null;
                    _dataReader = null;
                    _dataReader = null;
                    _isOpen = false;
                    _dbName = null;
                    _fields = null;
                    _primaryKey = null;
                    _primaryKeyField = null;
                    _encryptor = null;
                    _metaData = null;
                }
            }
        }

        void checkIsDbOpen()
        {
            if( !_isOpen )
            {
                throw new FileDbException( FileDbException.NoOpenDatabase, FileDbExceptions.NoOpenDatabase );
            }
        }

        void checkReadOnly()
        {
            if( _openReadOnly )
            {
                throw new FileDbException( FileDbException.DatabaseReadOnlyMode, FileDbExceptions.NoOpenDatabase );
            }
        }
        
        internal void drop( string dbName )
        {
            if( dbName == _dbName && _isOpen )
            {
                close();
            }

            #if SILVERLIGHT
            IsolatedStorageFile isoFile = IsolatedStorageFile.GetUserStoreForApplication();
            try
            {
                isoFile.DeleteFile( dbName );
            }
            finally
            {
                isoFile.Dispose();
            }
            #else
            File.Delete( dbName );
            #endif
        }
        
        internal void create( string dbName, Field[] schema )
        {
            // Close any existing DB first
            if( _isOpen )
                close();

            // Find the primary key and do error checking on the schema
            _fields = new Fields();
            _primaryKey = string.Empty;

            for( Int32 i = 0; i < schema.Length; i++ )
            {
                Field field = schema[i];

                switch( field.DataType )
                {
                    case DataType.Byte:
                    case DataType.Int:
                    case DataType.UInt:
                    case DataType.String:
                    case DataType.Float:
                    case DataType.Double:
                    case DataType.Bool:
                    case DataType.DateTime:
                    break;

                    default: // Unknown type..!                        
                    throw new FileDbException( string.Format( FileDbException.InvalidTypeInSchema, (Int32) field.DataType ),
                                    FileDbExceptions.InvalidTypeInSchema );
                }

                if( field.IsPrimaryKey && string.IsNullOrEmpty( _primaryKey ) )
                {
                    // Primary key!
                    // Is the key an array or boolean?  
                    // If so, don't allow them to be primary keys...
                    
                    if( !(field.DataType == DataType.Int || field.DataType == DataType.String) )
                    {
                        throw new FileDbException( string.Format( FileDbException.InvalidPrimaryKeyType, field.Name ),
                                        FileDbExceptions.InvalidPrimaryKeyType );
                    }

                    if( field.IsArray )
                    {
                        throw new FileDbException( string.Format( FileDbException.InvalidPrimaryKeyType, field.Name ),
                                        FileDbExceptions.InvalidPrimaryKeyType );
                    }

                    _primaryKey = field.Name.ToUpper();
                    _primaryKeyField = field;
                }

                _fields.Add( field );
            }

            // Open the database files

            openFiles( dbName, FileMode.Create );
                        
            _isOpen = true;
            _dbName = dbName;
            _iteratorIndex = 0;

            _ver_major = VERSION_MAJOR;
            _ver_minor = VERSION_MINOR;

            try
            {
                //lockWrite( false );

                _numRecords = 0;
                _numDeleted = 0;

                // Write the schema
                writeDbHeader( _dataWriter );
                writeSchema( _dataWriter );
                
                // the indexStart is the same as dataStart until records are added
                _indexStartPos = _dataStartPos;
                
                // we must write the indexStart location AFTER writeSchema the first time
                writeIndexStart( _dataWriter );
                
                // brettg: read it back in because the field order may have changed if the primary key
                // wasn't the first field
                readSchema();

                _index = new List<int>(100);
                _deletedRecords = new List<int>(3);
            }
            finally
            {
                //unlock( false, true );
            }
        }

        internal int addRecord( FieldValues record )
        {
            int newIndex = -1;

            checkIsDbOpen();
            checkReadOnly();

            //record = normalizeFieldNames( record );

            // Verify record as compared to the schema
            verifyRecordSchema( record );

            try
            {
                // Add the item to the data file
                //lockWrite( false );

                // set the autoinc vals into the record
                foreach( Field field in _fields )
                {
                    if( field.IsAutoInc )
                    {
                        // if there are no records in the DB, start at the beginning
                        if( _numRecords == 0 )
                            field.CurAutoIncVal = field.AutoIncStart;
                        
                        // if the field is absent this will add it regardless which is what we want
                        // because its an autoinc field
                        record[field.Name] = field.CurAutoIncVal;
                    }
                }

                //List<Int32> vIndex = readIndex2();

                // Check the index.  To enable a binary search, we must read in the 
                // entire index, insert our item then write it back out.
                // Where there is no primary key, we can't do a binary search so skip
                // this sorting business.

                if( !string.IsNullOrEmpty( _primaryKey ) )
                {
                    if( _numRecords > 1 )
                    {
                        // Do a binary search to find the insertion position
                        object data = null;
                        if( record.ContainsKey( _primaryKey ) )
                            data = record[_primaryKey];

                        if( data == null )
                            throw new FileDbException( string.Format( FileDbException.MissingPrimaryKey,
                                _primaryKey ), FileDbExceptions.MissingPrimaryKey );

                        Int32 pos = bsearch( _index, 0, _index.Count - 1, data );

                        // Ensure we don't have a duplicate key in the database
                        if( pos > 0 )
                            // Oops... duplicate key
                            throw new FileDbException( string.Format( FileDbException.DuplicatePrimaryKey,
                                _primaryKey, data.ToString() ), FileDbExceptions.DuplicatePrimaryKey );

                        // Revert the result from bsearch to the proper insertion position
                        pos = (-pos) - 1;
                        newIndex = pos;
                    }
                }

                Int32 newOffset = _indexStartPos,
                        recordSize = getRecordSize( record ),
                        deletedIndex = -1;

                if( _numDeleted > 0 )
                {
                    // look for an existing deleted record hole big enough to hold the new record

                    for( int ndx = 0; ndx < _deletedRecords.Count; ndx++ )
                    {
                        Int32 holePos = _deletedRecords[ndx];
                        _dataStrm.Seek( holePos, SeekOrigin.Begin );
                        Int32 holeSize = _dataReader.ReadInt32();
                        Debug.Assert( holeSize < 0 );
                        holeSize = -holeSize;
                        if( holeSize >= recordSize )
                        {
                            newOffset = holePos;
                            deletedIndex = ndx;
                            break;
                        }
                    }
                }

                _dataStrm.Seek( newOffset, SeekOrigin.Begin );

                writeRecord( _dataWriter, record, recordSize, false );
                _dataWriter.Flush();

                if( newIndex < 0 )
                {
                    _index.Add( newOffset );
                    newIndex = _index.Count - 1;
                }
                else
                    _index.Insert( newIndex, newOffset );

                if( deletedIndex > -1 )
                {
                    // this means we have one less deleted record
                    _deletedRecords.RemoveAt( deletedIndex );
                    _numDeleted--;
                    Debug.Assert( _deletedRecords.Count == _numDeleted );
                }

                // update the autoinc vals
                foreach( Field field in _fields )
                {
                    if( field.IsAutoInc )
                        field.CurAutoIncVal += 1;
                }
                    
                // check to see if we went past the previous _indexStartPos
                int newDataEndPos = (int) _dataStrm.Position;
                if( newDataEndPos > _indexStartPos )
                {
                    // capture the new index pos - the end of the last record is the start of the index
                    _indexStartPos = newDataEndPos;
                    // writeSchema will write _indexStartPos to the file
                }

                // We have a new entry
                ++_numRecords;

                // Write out the newly updated schema (autoinc values, numRecords)
                writeSchema( _dataWriter );
            }
            finally
            {
                //unlock( false, true );
            }
            return newIndex;
        }
        
        ///----------------------------------------------------------------------------------------
        /// <summary>
        /// record must have all fields
        /// </summary>
        /// 
        internal void updateRecordByKey( FieldValues record, object key )
        {
            checkIsDbOpen();
            checkReadOnly();

            if( _numRecords == 0 )
                throw new FileDbException( FileDbException.DatabaseEmpty, FileDbExceptions.DatabaseEmpty );

            // find the index of the record
            // we only need to get a single field because really just want the index
            string fieldToGet = null;
            if( string.IsNullOrEmpty( _primaryKey ) )
                fieldToGet = _fields[0].Name;
            else
                fieldToGet = _fields[_primaryKey].Name;
            object[] existingRecord = this.getRecordByKey( key, new string[] { fieldToGet }, true );
            if( existingRecord == null )
                throw new FileDbException( FileDbException.PrimaryKeyValueNotFound, FileDbExceptions.PrimaryKeyValueNotFound );

            // the index is in the last column

            updateRecordByIndex( record, (int) existingRecord[existingRecord.Length - 1] );
        }
        
        ///----------------------------------------------------------------------------------------
        /// <summary>
        /// record must have all fields
        /// </summary>
        /// 
        internal void updateRecordByIndex( FieldValues record, Int32 index )
        {
            checkIsDbOpen();
            checkReadOnly();

            if( _numRecords == 0 )
                throw new FileDbException( FileDbException.DatabaseEmpty, FileDbExceptions.DatabaseEmpty );

            bool indexUpdated;
            updateRecordByIndex( record, index, _index, true, true, out indexUpdated );

            if( indexUpdated && AutoFlush ) flush();

            // Do an auto-cleanup if required
            checkAutoClean();
        }

        // Helper for the other updateRecord methods - if you call this you MUST call writeIndex if indexUpdated is true
        //
        void updateRecordByIndex( FieldValues record, Int32 index, List<Int32> lstIndex, bool bNormalizeFieldNames, bool bVerifyRecordSchema, 
            out bool indexUpdated )
        {
            indexUpdated = false;

            // make field names uppercase
            //if( bNormalizeFieldNames )
            //    record = normalizeFieldNames( record );

            // Verify record as compared to the schema
            if( bVerifyRecordSchema )
                verifyRecordSchema( record );

            try
            {
                Int32 oldSize = 0;

                //lockWrite( false );
                
                if( !string.IsNullOrEmpty( _primaryKey ) && record.ContainsKey( _primaryKey ) )
                {
                    // Do a binary search to find the index position of any other records that may already
                    // have this key so as to not allow duplicate keys

                    Int32 pos = bsearch( lstIndex, 0, _numRecords - 1, record[_primaryKey] );

                    // Ensure the item to edit IS in the database, 
                    // as the new one takes its place.

                    if( pos >= 0 )
                    {
                        pos -= 1;

                        // a record was found - check if its the same one
                        if( pos != index )
                        {
                            // its not the same record and we cannot allow a duplicate key
                            throw new FileDbException( string.Format( FileDbException.DuplicatePrimaryKey,
                                            _primaryKey, record[_primaryKey].ToString() ), FileDbExceptions.DuplicatePrimaryKey );
                        }
                    }

                    // Revert the result from bsearch to the proper position
                    //recordNum = pos;
                }
                else
                {
                    // Ensure the record number is a number within range
                    if( (index < 0) || (index > _numRecords - 1) )
                    {
                        throw new FileDbException( string.Format( FileDbException.RecordNumOutOfRange, index ), FileDbExceptions.IndexOutOfRange );
                    }
                }

                // Read the size of the record.  If it is the same or bigger than 
                // the new one, then we can just place it in its original position
                // and not worry about a deleted record.

                int origRecordOffset = lstIndex[index];
                _dataStrm.Seek( origRecordOffset, SeekOrigin.Begin );
                oldSize = _dataReader.ReadInt32();
                Debug.Assert( oldSize >= 0 );

                // fill in any field values from the DB that were not supplied
                FieldValues fullRecord = record;
                bool isFullRecord = record.Count >= _fields.Count; // the index field may be in there

                if( !isFullRecord )
                {
                    object[] row = readRecord( origRecordOffset, false );

                    fullRecord = new FieldValues( row.Length );

                    // copy record to fullRecord
                    foreach( string fieldName in record.Keys )
                    {
                        fullRecord.Add( fieldName, record[fieldName] );
                    }

                    foreach( Field field in _fields )
                    {
                        if( !fullRecord.ContainsKey( field.Name ) )
                        {
                            fullRecord.Add( field.Name, row[field.Ordinal] );
                        }
                    }
                }

                // Get the size of the new record for calculations below
                Int32 newSize = getRecordSize( fullRecord ),
                      deletedIndex = -1,
                      newPos = _indexStartPos;

                if( newSize > oldSize )
                {
                    // Record is too big for the "hole" - look through the deleted records
                    // for a hole large enough to hold the new record

                    int ndx = 0;
                    foreach( Int32 holePos in _deletedRecords )
                    {
                        _dataStrm.Seek( holePos, SeekOrigin.Begin );
                        Int32 holeSize = _dataReader.ReadInt32();
                        Debug.Assert( holeSize < 0 );
                        holeSize = -holeSize;
                        if( holeSize >= newSize )
                        {
                            newPos = holePos;
                            deletedIndex = ndx;
                            break;
                        }
                        ndx++;
                    }
                }
                else
                    newPos = origRecordOffset;

                // Write the record to the database file
                _dataStrm.Seek( newPos, SeekOrigin.Begin );
                writeRecord( _dataWriter, fullRecord, newSize, false );

                // check to see if we went past the previous _indexStartPos
                int newDataEndPos = (int) _dataStrm.Position;
                if( newDataEndPos > _indexStartPos )
                {
                    // capture the new index pos - the end of the last record is the start of the index
                    _indexStartPos = newDataEndPos;
                    writeIndexStart( _dataWriter );
                }

                if( newSize > oldSize )
                {
                    // add the previous offset to the deleted collection
                    _deletedRecords.Add( origRecordOffset );

                    // did we find a hole?
                    if( deletedIndex < 0 )
                    {
                        // no hole
                        // this means we have a new deleted entry (the old record) because we couldn't
                        // find a large enough hole and we are writing to the end of the data section
                        ++_numDeleted;
                    }
                    else
                    {
                        // found a hole - remove the old deleted index
                        _deletedRecords.RemoveAt( deletedIndex );
                    }

                    // update the index with new pos

                    lstIndex[index] = newPos;
                    indexUpdated = true;

                    // make the old record's size be negative to indicate deleted
                    _dataStrm.Seek( origRecordOffset, SeekOrigin.Begin );
                    _dataWriter.Write( -oldSize );

                    // Write the number of deleted records                    
                    _dataStrm.Seek( INDEX_DELETED_OFFSET, SeekOrigin.Begin );
                    _dataWriter.Write( _numDeleted );
                }
                _dataWriter.Flush();
            }
            finally
            {
                //unlock( false, true );
            }
        }
        
        // Update selected records
        //
        internal Int32 updateRecords( FilterExpression searchExp, FieldValues record )
        {
            var searchExpGrp = new FilterExpressionGroup();
            searchExpGrp.Add( BoolOp.And, searchExp );            
            return updateRecords( searchExpGrp, record );
        }
        
        // Update selected records
        //
        internal Int32 updateRecords( FilterExpressionGroup searchExpGrp, FieldValues record )
        {
            checkIsDbOpen();
            checkReadOnly();

            if( _numRecords == 0 )
                return 0;

            // make field names uppercase
            //record = normalizeFieldNames( record );

            // Verify record as compared to the schema
            verifyRecordSchema( record );

            bool isFullRecord = record.Count >= _fields.Count; // the index field may be in there
            Int32 updateCount = 0;
            bool indexUpdated = false;

            //lockWrite( false );

            try
            {
                // Read and delete selected records
                for( Int32 recordNum = 0; recordNum < _numRecords; ++recordNum )
                {
                    // Read the record
                    object[] row = readRecord( _index[recordNum], false );

                    bool isMatch = evaluate( searchExpGrp, row, _fields );
                    
                    if( isMatch )
                    {
                        FieldValues fullRecord = record;

                        if( !isFullRecord )
                        {
                            fullRecord = new FieldValues( row.Length );

                            // copy record to fullRecord
                            foreach( string fieldName in record.Keys )
                            {
                                fullRecord.Add( fieldName, record[fieldName] );
                            }

                            // ensure all fields are in the record so that updateRecord will not have to read them in again
                            foreach( Field field in _fields )
                            {
                                string upperName = field.Name.ToUpper();
                                if( !fullRecord.ContainsKey( upperName ) )
                                {
                                    fullRecord.Add( upperName, row[field.Ordinal] );
                                }
                            }
                        }

                        bool tempIndexUpdated;
                        updateRecordByIndex( fullRecord, recordNum, _index, false, false, out tempIndexUpdated );
                        if( tempIndexUpdated )
                            indexUpdated = tempIndexUpdated;
                        ++updateCount;
                    }
                }
            }
            finally
            {
                //unlock( false, true );
            }

            if( updateCount > 0 )
            {
                // Do an auto-cleanup if required
                checkAutoClean();
            }

            return updateCount;
        }

        /// <summary>
        /// Configures autoclean.  When an edit or delete is made, the
        /// record is normally not removed from the data file - only the index.
        /// After repeated edits/deletions, the data file may become very big with
        /// deleted (non-removed) records.  A cleanup is normally done with the
        /// cleanup() method.  Autoclean will do this automatically, keeping the
        /// number of deleted records to under the threshold value.
        /// To turn off autoclean, set threshold to a negative value.
        /// </summary>
        /// <param name="threshold">number of deleted records to have at any one time</param>
        ///
        internal void setAutoCleanThreshold( Int32 threshold )
        {
            _autoCleanThreshold = threshold;

            // Do an auto-cleanup if required
            if( (_isOpen) && 
                (_autoCleanThreshold >= 0) && 
                (_numDeleted > _autoCleanThreshold) )
            {
                cleanup( false );
            }
        }

        internal Int32 getAutoCleanThreshold()
        {
            return _autoCleanThreshold;
        }

        void checkAutoClean()
        {
            if( _isOpen && _autoCleanThreshold >= 0 && _numDeleted > _autoCleanThreshold )
            {
                cleanup( false );
            }
        }

        ///----------------------------------------------------------------------------------------
        /// <summary>
        /// Read all records to create new index.
        /// </summary>
        /// 
        internal void reindex()
        {
            checkIsDbOpen();
            checkReadOnly();

            if( _numRecords == 0 )
                return;

            try
            {
                //lockWrite( false );

                int numRecs = _numRecords + _numDeleted;

                var index = new List<int>( numRecs );
                _deletedRecords = new List<int>();

                _dataStrm.Seek( _dataStartPos, SeekOrigin.Begin );
                Int32 newOffset = _dataStartPos;

                for( Int32 recordNum = 0; recordNum < numRecs; ++recordNum )
                {
                    // Read in the size of the block allocated for the record
                    _dataStrm.Seek( newOffset, SeekOrigin.Begin );
                    Int32 recordSize;

                    // Read the record
                    bool deleted;
                    object[] record = readRecord( newOffset, false, out recordSize, out deleted );

                    if( !deleted )
                    {
                        if( _primaryKeyField != null ) // !string.IsNullOrEmpty( _primaryKey ) )
                        {
                            // Do a binary search to find the insertion position
                            object data = record[_primaryKeyField.Ordinal];

                            Int32 pos = bsearch( index, 0, index.Count - 2, data );

                            // Revert the result from bsearch to the proper insertion position
                            pos = (-pos) - 1;

                            // Insert the new item to the correct position
                            index.Insert( pos, newOffset );
                        }
                        else
                        {
                            index.Add( newOffset );
                        }
                    }
                    else
                        _deletedRecords.Add( newOffset );

                    newOffset += recordSize + sizeof( Int32 ); // recordSize doesn't include the length of the int size
                }

                _indexStartPos = newOffset;

                _dataStrm.Seek( SCHEMA_OFFSET, SeekOrigin.Begin );

                _dataWriter.Write( _numRecords = index.Count );

                _dataWriter.Write( _numDeleted = _deletedRecords.Count );

                _dataWriter.Write( _indexStartPos );

                _index = index;
            }
            finally
            {
                //unlock( false, true );
            }
        }

        /// <summary>
        /// Remove all deleted records
        /// </summary>
        /// 
        internal void cleanup( bool force )
        {
            checkIsDbOpen();
            checkReadOnly();

            // Don't bother if the database is clean
            if( !force && _numDeleted == 0 )
                return;

            //lockWrite( false );

            // Read in the index, and rebuild it along with the database data
            // into a separate file.  Then move that new file back over the old
            // database.

            // Note that we attempt the file creation under the DB lock, so
            // that another process doesn't try to create the same file at the
            // same time.
            string tmpFilename = Path.GetFileNameWithoutExtension( _dbName ) + ".tmp";
            tmpFilename = Path.Combine( Path.GetDirectoryName( _dbName ), tmpFilename );
            #if SILVERLIGHT
            IsolatedStorageFile isoFile = IsolatedStorageFile.GetUserStoreForApplication();
                #if true // !WINDOWS_PHONE
                var tmpdb = new IsolatedStorageFileStream( tmpFilename, FileMode.OpenOrCreate, FileAccess.Write, isoFile );
                #else
                var tmpdb = new MemoryStream( (int) _dataStrm.Length );
                #endif
            #else
            var tmpdb = File.Open( tmpFilename, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None );
            #endif
            tmpdb.SetLength( 0 );

            int tempNumDeleted = _numDeleted,
                tempIndexStart = _indexStartPos;

            try
            {
                var tmpDataWriter = new BinaryWriter( tmpdb );

                // create a new index list
                var newIndex = new List<Int32>( _numRecords );

                // Set the number of (unclean) deleted items to zero and write the schema
                _numDeleted = 0;

                // Write the schema
                writeDbHeader( tmpDataWriter );
                writeSchema( tmpDataWriter );

                var dicRecord = new FieldValues();

                // For each item in the index, move it from the current database
                // file to the new one.

                for( Int32 idx = 0; idx < _index.Count; ++idx )
                {
                    Int32 offset = _index[idx];

                    // Read in the entire record
                    bool deleted;
                    byte[] record = readRecordRaw( _dataReader, offset, out deleted );                        
                    Debug.Assert( !deleted );

                    // Save the new file offset
                    newIndex.Add( (Int32) tmpdb.Position );

                    writeRecordRaw( tmpDataWriter, record, false );
                }
                _indexStartPos = (int) tmpdb.Position;
                writeIndexStart( tmpDataWriter );
                _deletedRecords = new List<Int32>();
                _index = newIndex;
                tmpdb.Flush();
                writeIndex( tmpdb, tmpDataWriter, _index );
            }
            catch
            {
                // set everything back the way it was
                _indexStartPos = tempIndexStart;
                _numDeleted = tempNumDeleted;
                tmpdb.Close();
                #if SILVERLIGHT
                isoFile.DeleteFile( tmpFilename );
                #else
                File.Delete( tmpFilename );
                #endif
                throw;
            }

            // get the dbName, etc. before we close
            string dbName = _dbName;
            Encryptor encryptor = _encryptor;
            close();

            // Move the temporary file over the original database file
            #if SILVERLIGHT
            isoFile.DeleteFile( dbName );

            #if true // !WINDOWS_PHONE
            tmpdb.Close();
            tmpdb = null;
            isoFile.MoveFile( tmpFilename, dbName );
            #else
            try
            {
                // workaround for WP because we can't rename/move a file
                byte[] buf = new byte[Math.Min( tmpdb.Length, 4024 )];
                _dataStrm = isoFile.CreateFile( dbName );
                tmpdb.Seek( 0, SeekOrigin.Begin );
                int nRead = 0;
                while( (nRead = tmpdb.Read( buf, 0, buf.Length )) > 0 )
                {
                    _dataStrm.Write( buf, 0, nRead );
                }
            }
            finally
            {
                _dataStrm.Close();
                _dataStrm = null;
                tmpdb.Close();
                tmpdb = null;
            }
            #endif
            isoFile.Dispose();
            #else
            tmpdb.Close();
            tmpdb = null;
            File.Delete( dbName );
            File.Move( tmpFilename, dbName );
            #endif

            // Re-open the database
            open( dbName, null, encryptor, _openReadOnly );
        }

        private void setRecordDeleted( int pos, bool deleted )
        {
            _dataStrm.Seek( pos, SeekOrigin.Begin );
            Int32 size = _dataReader.ReadInt32();
            if( size > 0 )
                size = -size;
            _dataStrm.Seek( pos, SeekOrigin.Begin );
            _dataWriter.Write( size );
        }

        internal Int32 removeAll()
        {
            checkIsDbOpen();
            checkReadOnly();

            Int32 numDeleted = 0;

            if( _numRecords == 0 )
                return numDeleted;

            try
            {
                //lockWrite( false );

                _dataStrm.Seek( SCHEMA_OFFSET, SeekOrigin.Begin );

                _numRecords = _numDeleted = 0;
                _indexStartPos = _dataStartPos;

                _index.Clear();
                _deletedRecords.Clear();

                writeSchema( _dataWriter );
            }
            finally
            {
                //unlock( false, true );
            }

            return numDeleted;
        }

        /// <summary>
        /// Removes an entry from the database INDEX only - it appears
        /// deleted, but the actual data is only removed from the file when a 
        /// cleanup() is called.
        /// </summary>
        /// <param name="key">Int32 or string primary key used to identify record to remove.  For
        /// databases without primary keys, it is the record number (zero based) in
        /// the table.</param>
        /// <returns>true if a record was removed, false otherwise</returns>
        /// 
        internal bool removeByKey( object key )
        {
            checkIsDbOpen();
            checkReadOnly();

            if( _numRecords == 0 )
                return false;

            try
            {
                //lockWrite( false );

                Int32 pos = -1;

                if( !string.IsNullOrEmpty( _primaryKey ) )
                {
                    // Do a binary search to find the item
                    pos = bsearch( _index, 0, _numRecords - 1, key );

                    if( pos < 0 )
                    {
                        // Not found!
                        return false;
                    }

                    // Revert the result from bsearch to the proper insertion position
                    --pos;
                }
                else
                {
                    if( key.GetType() != typeof( Int32 ) )
                    {
                        throw new FileDbException( FileDbException.InvalidKeyFieldType, FileDbExceptions.InvalidKeyFieldType );
                    }

                    pos = (Int32) key;

                    // Ensure the "key" is the item number within range
                    if( pos < 0 || pos >= _numRecords )
                    {
                        throw new FileDbException( string.Format( FileDbException.RecordNumOutOfRange, pos ), FileDbExceptions.IndexOutOfRange );
                    }
                }

                setRecordDeleted( _index[pos], true );
                _deletedRecords.Add( _index[pos] );
                _index.RemoveAt( pos );
                
                _dataStrm.Seek( SCHEMA_OFFSET, SeekOrigin.Begin );

                // Write the number of records
                _dataWriter.Write( --_numRecords );

                // Write the number of (unclean) deleted records
                _dataWriter.Write( ++_numDeleted );
                Debug.Assert( _deletedRecords.Count == _numDeleted );
            }
            finally
            {
                //unlock( false, true );
            }

            // Do an auto-cleanup if required
            checkAutoClean();

            return true;
        }

        /// <summary>
        /// Removes an entry from the database INDEX only - it appears
        /// deleted, but the actual data is only removed from the file when a 
        /// cleanup() is called.
        /// </summary>
        /// <param name="recordNum">The record number (zero based) in the table to remove</param>
        /// <returns>true on success, false otherwise</returns>
        /// 
        internal bool removeByIndex( Int32 recordNum )
        {
            checkIsDbOpen();
            checkReadOnly();

            if( _numRecords == 0 )
            {
                return false;
            }

            // All we do here is remove the item from the index.
            // Read in the index, check to see if it exists, delete the item,
            // then rebuild the index on disk.

            try
            {
                //lockWrite( false );

                // Ensure it is within range
                if( (recordNum < 0) || (recordNum >= _numRecords) )
                {
                    throw new FileDbException( string.Format( FileDbException.RecordNumOutOfRange, recordNum ), FileDbExceptions.IndexOutOfRange );
                }

                setRecordDeleted( _index[recordNum], true );
                _deletedRecords.Add( _index[recordNum] );

                _index.RemoveAt( recordNum );

                _dataStrm.Seek( SCHEMA_OFFSET, SeekOrigin.Begin );

                // Write the number of records
                _dataWriter.Write( --_numRecords );

                // Write the number of (unclean) deleted records
                _dataWriter.Write( ++_numDeleted );
                Debug.Assert( _deletedRecords.Count == _numDeleted );

            }
            finally
            {
                //unlock( false, true );
            }

            // Do an auto-cleanup if required
            checkAutoClean();

            return true;
        }

        /// <summary>
        /// Removes entries from the database INDEX only, based on the
        /// result of a regular expression match on a given field - records appear 
        /// deleted, but the actual data is only removed from the file when a 
        /// cleanup() is called.
        /// </summary>
        /// <param name="searchExp"></param>
        /// <returns>number of records removed</returns>
        internal Int32 removeByValue( FilterExpression searchExp )
        {
            checkIsDbOpen();
            checkReadOnly();

            if( _numRecords == 0 )
                return 0;

            string fieldName = searchExp.FieldName;
            if( fieldName[0] == '~' )
            {
                fieldName = fieldName.Substring( 1 );
                searchExp.MatchType = MatchType.IgnoreCase;
            }

            // Check the field name is valid
            if( !_fields.ContainsKey( fieldName ) )
                throw new FileDbException( string.Format( FileDbException.InvalidFieldName, searchExp.FieldName ), FileDbExceptions.InvalidFieldName );

            Field field = _fields[fieldName];
            Int32 deleteCount = 0;

            try
            {
                //lockWrite( false );

                //List<Int32> vIndex = readIndex2();

                Regex regex = null;

                // Read and delete selected records
                for( Int32 recordNum = 0; recordNum < _numRecords; ++recordNum )
                {
                    // Read the record
                    bool deleted;
                    object[] record = readRecord( _index[recordNum], false, out deleted );
                    Debug.Assert( !deleted );

                    object val = record[field.Ordinal].ToString();

                    if( (searchExp.Equality == Equality.Like || searchExp.Equality == Equality.NotLike) && regex == null )
                        regex = new Regex( searchExp.SearchVal.ToString(), RegexOptions.IgnoreCase );

                    bool isMatch = evaluate( field, searchExp, record, regex );
                    
                    if( isMatch )
                    {
                        setRecordDeleted( _index[recordNum], true );
                        _deletedRecords.Add( _index[recordNum] );

                        _index.RemoveAt( recordNum );

                        --_numRecords;
                        ++_numDeleted;

                        // Make sure we don't skip over the next item in the for() loop
                        --recordNum;
                        ++deleteCount;
                    }
                }

                if( deleteCount > 0 )
                {
                    _dataStrm.Seek( SCHEMA_OFFSET, SeekOrigin.Begin );

                    // Write the number of records
                    _dataWriter.Write( _numRecords );

                    // Write the number of (unclean) deleted records
                    _dataWriter.Write( _numDeleted );

                    Debug.Assert( _deletedRecords.Count == _numDeleted );
                }
            }
            finally
            {
                //unlock( false, true );
            }

            // Do an auto-cleanup if required
            checkAutoClean();

            return deleteCount;
        }

        // Read in each record once at a time, and remove it from
        // the index if the select function determines it to be deleted
        // Rebuild the index on disc if there items were deleted
        //
        internal Int32 removeByValues( FilterExpressionGroup searchExpGrp )
        {
            checkIsDbOpen();
            checkReadOnly();

            if( _numRecords == 0 )
                return 0;

            Int32 deleteCount = 0;

            try
            {
                //lockWrite( false );

                //List<Int32> vIndex = readIndex2();

                // Read and delete selected records
                for( Int32 recordNum = 0; recordNum < _numRecords; ++recordNum )
                {
                    // Read the record
                    bool deleted;
                    object[] record = readRecord( _index[recordNum], false, out deleted );
                    Debug.Assert( !deleted );

                    bool isMatch = evaluate( searchExpGrp, record, _fields );
                    
                    if( isMatch )
                    {
                        setRecordDeleted( _index[recordNum], true );
                        _deletedRecords.Add( _index[recordNum] );

                        _index.RemoveAt( recordNum );

                        --_numRecords;
                        ++_numDeleted;

                        // Make sure we don't skip over the next item in the for() loop
                        --recordNum;
                        ++deleteCount;
                    }
                }

                if( deleteCount > 0 )
                {
                    _dataStrm.Seek( SCHEMA_OFFSET, SeekOrigin.Begin );

                    // Write the number of records
                    _dataWriter.Write( _numRecords );

                    // Write the number of (unclean) deleted records
                    _dataWriter.Write( _numDeleted );

                    Debug.Assert( _deletedRecords.Count == _numDeleted );
                }
            }
            finally
            {
                //unlock( false, true );
            }

            // Do an auto-cleanup if required
            checkAutoClean();

            return deleteCount;
        }
        
        private bool isEof
        {
            get
            {
                return _iteratorIndex >= _numRecords;
            }
        }

        /// <summary>
        /// move to the first index position
        /// </summary>
        /// 
        internal bool moveFirst()
        {
            checkIsDbOpen();

            _iteratorIndex = 0;
            return !isEof; 
        }

        /// <summary>
        ///  Move the current index position to the next database item.
        /// </summary>
        /// <returns>true if advanced to a new item, false if there are none left</returns>
        /// 
        internal bool moveNext()
        {
            checkIsDbOpen();

            bool result = false;

            // No items?
            if( _numRecords == 0 || isEof )
                return result;

            _iteratorIndex++;
            result = _iteratorIndex < _numRecords;

            return result;
        }
        
        
        /// <summary>
        /// Return the current record in the database.  Note that the current iterator pointer is not moved in any way.
        /// </summary>
        /// <returns></returns>
        /// 
        internal object[] getCurrentRecord( bool includeIndex )
        {
            checkIsDbOpen();

            // No items?
            if( _numRecords == 0 )
                return null;

            object[] record = null;

            try
            {
                //lockRead( false );

                // No more records left?
                if( isEof )
                {
                    throw new FileDbException( FileDbException.IteratorPastEndOfFile, FileDbExceptions.IteratorPastEndOfFile );
                }

                int indexOffset = _index[_iteratorIndex];
                record = readRecord( indexOffset, includeIndex );

                if( includeIndex )
                {
                    // set the index into the record
                    record[record.Length - 1] = _iteratorIndex;
                }
            }
            finally
            {
                //unlock( false, false );
            }

            // Return the record
            return record;
        }

        /// <summary>
        /// retrieves a record based on the specified key
        /// </summary>
        /// <param name="key">primary key used to identify record to retrieve.  For
        /// databases without primary keys, it is the record number (zero based) in 
        /// the table.</param>
        /// <param name="fieldList"></param>
        /// <param name="includeIndex">if true, an extra field called 'IFIELD' will
        /// be added to each record returned.  It will contain an Int32 that specifies
        /// the original position in the database (zero based) that the record is 
        /// positioned.  It might be useful when an orderby is used, and a future 
        /// operation on a record is required, given it's index in the table.</param>
        /// <returns>record if found, or false otherwise</returns>
        /// 
        internal object[] getRecordByKey( object key, string[] fieldList, bool includeIndex )
        {
            checkIsDbOpen();

            object[] record = null;

            try
            {
                //lockRead( false );

                // Read the index
                //Int32[] vIndex = readIndex();

                Int32 offset, idx;

                if( !string.IsNullOrEmpty( _primaryKey ) )
                {
                    // Do a binary search to find the item
                    idx = bsearch( _index, 0, _numRecords - 1, key );

                    if( idx < 0 )
                    {
                        // Not found!
                        return null;
                    }

                    // bsearch always returns the real position + 1
                    --idx;

                    // Get the offset of the record in the database
                    offset = _index[idx];
                }
                else
                {
                    idx = (Int32) key;

                    // Ensure the record number is an Int32 and within range
                    if( (idx < 0) || (idx >= _numRecords) )
                    {
                        //user_error("Invalid record number (key).", E_USER_ERROR);
                        return null;
                    }

                    offset = _index[idx];
                }

                // Read the record
                //bool includeIndex = fieldListContainsIndex( fieldList );
                bool deleted;
                record = readRecord( offset, includeIndex, out deleted );
                Debug.Assert( !deleted );

                if( fieldList != null )
                {
                    object[] tmpRecord = new object[fieldList.Length + (includeIndex ? 1 : 0)]; // one extra for the index
                    int n=0;
                    foreach( string fieldName in fieldList )
                    {
                        if( !_fields.ContainsKey( fieldName ) )
                            throw new FileDbException( string.Format( FileDbException.InvalidFieldName, fieldName ), FileDbExceptions.InvalidFieldName );
                        Field fld = _fields[fieldName];
                        tmpRecord[n++] = record[fld.Ordinal];
                    }
                    record = tmpRecord;
                }

                if( includeIndex )
                {
                    // set the index into the record
                    record[record.Length - 1] = idx++;
                }
            }
            finally
            {
                //unlock( false, false );
            }

            return record;
        }

        /// <summary>
        /// retrieves a record based on the record number in the table
        /// (zero based)
        /// </summary>
        /// <param name="idx">zero based record number to retrieve</param>
        /// <param name="fieldList"></param>
        /// <param name="includeIndex"></param>
        /// <returns></returns>
        /// 
        internal object[] getRecordByIndex( Int32 idx, string[] fieldList, bool includeIndex )
        {
            checkIsDbOpen();

            // Ensure the record number is within range
            if( (idx < 0) || (idx >= _numRecords) )
            {
                //throw new FileDbException( string.Format( FileDbException.RecordNumOutOfRange, idx ), FileDbExceptions.IndexOutOfRange );
                return null;
            }

            object[] record = null;

            try
            {
                //lockRead( false );

                // Read the index
                //Int32[] vIndex = readIndex();
                Int32 offset = _index[idx];

                // Read the record
                //bool includeIndex = fieldListContainsIndex( fieldList );
                bool deleted;
                record = readRecord( offset, includeIndex, out deleted );
                Debug.Assert( !deleted );

                if( fieldList != null )
                {
                    object[] tmpRecord = new object[fieldList.Length + (includeIndex ? 1 : 0)]; // one extra for the index
                    int n=0;
                    foreach( string fieldName in fieldList )
                    {
                        if( !_fields.ContainsKey( fieldName ) )
                            throw new FileDbException( string.Format( FileDbException.InvalidFieldName, fieldName ), FileDbExceptions.InvalidFieldName );
                        Field fld = _fields[fieldName];
                        tmpRecord[n++] = record[fld.Ordinal];
                    }
                    record = tmpRecord;
                }

                if( includeIndex )
                {
                    // set the index into the record
                    record[record.Length - 1] = idx++;
                }
            }
            finally
            {
                //unlock( false, false );
            }

            return record;
        }

        internal object[][] getRecordByField( FilterExpression searchExp, string[] fieldList, bool includeIndex, string[] orderByList )
        {
            checkIsDbOpen();

            string fieldName = searchExp.FieldName;
            if( fieldName[0] == '~' )
            {
                fieldName = fieldName.Substring( 1 );
                searchExp.MatchType = MatchType.IgnoreCase;
            }

            // Check the field name is valid
            if( !_fields.ContainsKey( fieldName ) )
                throw new FileDbException( string.Format( FileDbException.InvalidFieldName, searchExp.FieldName ), FileDbExceptions.InvalidFieldName );

            Field field = _fields[fieldName];

            // If there are no records, return
            if( _numRecords == 0 )
                return null;

            object[][] result = null;

            try
            {
                //lockRead( false );

                // Read the index
                //Int32[] vIndex = readIndex();

                var lstResults = new List<object[]>();

                // Read each record and add it to an array
                Int32 idx = 0;          
                Regex regex = null;

                //bool includeIndex = fieldListContainsIndex( fieldList );

                foreach( Int32 offset in _index )
                {
                    // Read the record
                    bool deleted;
                    object[] record = readRecord( offset, includeIndex, out deleted );
                    Debug.Assert( !deleted );

                    if( (searchExp.Equality == Equality.Like || searchExp.Equality == Equality.NotLike) && regex == null )
                        regex = new Regex( searchExp.SearchVal.ToString(), RegexOptions.IgnoreCase );

                    bool isMatch = evaluate( field, searchExp, record, regex );

                    if( isMatch )
                    {
                        if( fieldList != null )
                        {
                            object[] tmpRecord = new object[fieldList.Length + (includeIndex ? 1 : 0)]; // one extra for the index
                            int n=0;
                            foreach( string fldName in fieldList )
                            {
                                if( !_fields.ContainsKey( fldName ) )
                                    throw new FileDbException( string.Format( FileDbException.InvalidFieldName, fldName ), FileDbExceptions.InvalidFieldName );
                                Field fld = _fields[fldName];
                                tmpRecord[n++] = record[fld.Ordinal];
                            }
                            record = tmpRecord;
                        }

                        if( includeIndex )
                        {
                            // set the index into the record
                            record[record.Length - 1] = idx++;
                        }

                        lstResults.Add( record );
                    }

                    idx++;
                }

                result = lstResults.ToArray();
            }
            finally
            {
                //unlock( false, false );
            }

            // Re-order as required
            if( result != null && orderByList != null )
                orderBy( result, fieldList, orderByList );

            return result;
        }

        internal object[][] getRecordByFields( FilterExpressionGroup searchExpGrp, string[] fieldList, bool includeIndex, string[] orderByList )
        {
            checkIsDbOpen();

            // If there are no records, return
            if( _numRecords == 0 )
                return null;

            object[][] result = null;

            try
            {
                //lockRead( false );

                // Read the index
                //Int32[] vIndex = readIndex();

                var lstResults = new List<object[]>();

                // Read each record and add it to an array
                Int32 idx = 0;
                //bool includeIndex = fieldListContainsIndex( fieldList );

                foreach( Int32 offset in _index )
                {
                    // Read the record
                    bool deleted;
                    object[] record = readRecord( offset, includeIndex, out deleted );
                    Debug.Assert( !deleted );

                    bool isMatch = evaluate( searchExpGrp, record, _fields );

                    if( isMatch )
                    {
                        if( fieldList != null )
                        {
                            object[] tmpRecord = new object[fieldList.Length + (includeIndex ? 1 : 0)]; // one extra for the index
                            int n=0;
                            foreach( string fieldName in fieldList )
                            {
                                if( !_fields.ContainsKey( fieldName ) )
                                    throw new FileDbException( string.Format( FileDbException.InvalidFieldName, fieldName ), FileDbExceptions.InvalidFieldName );
                                Field fld = _fields[fieldName];
                                tmpRecord[n++] = record[fld.Ordinal];
                            }
                            record = tmpRecord;
                        }

                        if( includeIndex )
                        {
                            // set the index into the record
                            record[record.Length - 1] = idx++;
                        }

                        lstResults.Add( record );
                    }

                    idx++;
                }

                result = lstResults.ToArray();
            }
            finally
            {
                //unlock( false, false );
            }

            // Re-order as required
            if( result != null && orderByList != null )
                orderBy( result, fieldList, orderByList );
    
            return result;
        }

        internal static bool evaluate( FilterExpressionGroup searchExpGrp, object[] record, Fields fields )
        {
            if( searchExpGrp.Expressions.Count == 0 )
                return true;

            bool isMatch = false;

            // if an express is null its automatically a match
            int ndx = 0;
            foreach( object searchExpressionOrGroup in searchExpGrp.Expressions )
            {
                bool thisMatch = false;

                if( searchExpressionOrGroup == null )
                    continue;

                BoolOp boolOp;

                if( searchExpressionOrGroup.GetType() == typeof( FilterExpressionGroup ) )
                {
                    var sexg = searchExpressionOrGroup as FilterExpressionGroup;
                    thisMatch = evaluate( sexg, record, fields );
                    boolOp = sexg.BoolOp;
                }
                else
                {
                    var searchExp = searchExpressionOrGroup as FilterExpression;
                    boolOp = searchExp.BoolOp;

                    string fieldName = searchExp.FieldName;
                    if( fieldName[0] == '~' )
                    {
                        fieldName = fieldName.Substring( 1 );
                        searchExp.MatchType = MatchType.IgnoreCase;
                    }

                    // Check the field name is valid
                    if( !fields.ContainsKey( fieldName ) )
                        throw new FileDbException( string.Format( FileDbException.InvalidFieldName, searchExp.FieldName ), FileDbExceptions.InvalidFieldName );

                    Field field = fields[fieldName];
                    thisMatch = evaluate( field, searchExp, record, null );
                }

                if( ndx == 0 )
                {
                    // the first time through the loop there is no boolean test
                    isMatch = thisMatch;
                }
                else
                {
                    if( boolOp == BoolOp.And )
                    {
                        isMatch = isMatch && thisMatch;
                        // we can stop as soon as one doesn't match when ANDing
                        if( !isMatch )
                            break;
                    }
                    else
                    {
                        isMatch = isMatch || thisMatch;
                        // we can stop as soon as a match is found when ORing
                        if( isMatch )
                            break;
                    }
                }
                ndx++;
            }

            return isMatch;
        }

        internal static bool evaluate( Field field, FilterExpression searchExp, object[] record, Regex regex )
        {
            // we currently don't support array searches
            if( field.IsArray )
                return false;

            Equality compareResult = Equality.NotEqual;

            // get the field value
            object val = record[field.Ordinal];

            if( val == null && searchExp.SearchVal == null ) // both null - I don't know if this is possible
            {
                return true;
            }
            else if( val != null && searchExp.SearchVal != null ) // neither null
            {
                // putting the RegEx search first (outside of the test below) allows RegEx searches on non-string fields
                if( (searchExp.Equality == Equality.Like || searchExp.Equality == Equality.NotLike) )
                {
                    // hopefully searching for strings

                    if( regex == null )
                        regex = new Regex( searchExp.SearchVal.ToString(), RegexOptions.IgnoreCase );

                    // See if the record matches the regular expression
                    compareResult = regex.IsMatch( val.ToString() ) ? Equality.Like : Equality.NotLike;
                }
                else if( searchExp.Equality == Equality.In || searchExp.Equality == Equality.NotIn )
                {
                    HashSet<object> hashSet = searchExp.SearchVal as HashSet<object>;
                    if( hashSet == null )
                        throw new FileDbException( FileDbException.HashSetExpected, FileDbExceptions.HashSetExpected );

                    // If the HashSet was created by the FilterExpression parser, the Field type wasn't
                    // yet known so all of the values will be string.  We must convert them to the
                    // Field type now.

                    if( field.DataType != DataType.String )
                    {
                        HashSet<object> tempHashSet = new HashSet<object>();
                        foreach( object obj in hashSet )
                        {
                            tempHashSet.Add( convertValueToType( obj, field.DataType ) );
                        }
                        hashSet = tempHashSet;
                        searchExp.SearchVal = tempHashSet;
                    }
                    else
                    {
                        if( searchExp.MatchType == MatchType.IgnoreCase )
                            val = val.ToString().ToUpper();
                    }

                    compareResult = hashSet.Contains( val ) ? Equality.In : Equality.NotIn;
                    
                    #if DEBUG
                    if( compareResult == Equality.In )
                    {
                        int debug = 0;
                    }
                    #endif
                }
                else if( field.DataType == DataType.String )
                {
                    int ncomp = string.Compare( searchExp.SearchVal.ToString(), val.ToString(),
                        searchExp.MatchType == MatchType.UseCase ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase );

                    compareResult = ncomp == 0 ? Equality.Equal : (ncomp > 0 ? Equality.GreaterThan : Equality.LessThan);
                }
                else
                {
                    compareResult = compareVals( field, val, searchExp.SearchVal );
                }
            }

            bool isMatch = false;

            // compareResult should only be one of: Equal, NotEqual, GreaterThan, LessThan

            // first check for NotEqual since it would be anythying which is not Equal

            if( searchExp.Equality == Equality.NotEqual )
            {
                if( compareResult != Equality.Equal )
                    isMatch = true;
            }
            else
            {
                // are they the same?
                if( compareResult == searchExp.Equality )
                    isMatch = true;
                else
                {
                    if( compareResult == Equality.Equal )
                    {
                        if( searchExp.Equality == Equality.Equal ||
                            searchExp.Equality == Equality.LessThanOrEqual ||
                            searchExp.Equality == Equality.GreaterThanOrEqual )
                        {
                            isMatch = true;
                        }
                    }
                    else if( compareResult == Equality.NotEqual )
                    {
                        if( searchExp.Equality == Equality.NotEqual ||
                            searchExp.Equality == Equality.LessThan ||
                            searchExp.Equality == Equality.GreaterThan )
                        {
                            isMatch = true;
                        }
                    }
                    else if( compareResult == Equality.LessThan &&
                             (searchExp.Equality == Equality.LessThan || searchExp.Equality == Equality.LessThanOrEqual) )
                    {
                        isMatch = true;
                    }
                    else if( compareResult == Equality.GreaterThan &&
                             (searchExp.Equality == Equality.GreaterThan || searchExp.Equality == Equality.GreaterThanOrEqual) )
                    {
                        isMatch = true;
                    }
                }
            }
            return isMatch;
        }

        static Equality compareVals( Field field, object val1, object val2 )
        {
            Equality retVal = Equality.NotEqual;

            switch( field.DataType )
            {
                case DataType.Byte:
                {
                    Byte b1 = Convert.ToByte( val1 ),
                         b2 = Convert.ToByte( val2 );

                    retVal = b1 == b2 ? Equality.Equal : (b1 > b2 ? Equality.GreaterThan : Equality.LessThan);
                }
                break;

                case DataType.Bool:
                {
                    bool b1 = Convert.ToBoolean( val1 ),
                         b2 = Convert.ToBoolean( val2 );

                    retVal = b1 == b2 ? Equality.Equal : Equality.NotEqual;
                }
                break;

                case DataType.Float:
                {
                    float f1 = Convert.ToSingle( val1 ),
                          f2 = Convert.ToSingle( val2 );

                    retVal = f1 == f2 ? Equality.Equal : (f1 > f2 ? Equality.GreaterThan : Equality.LessThan);
                }
                break;

                case DataType.Double:
                {
                    double d1 = Convert.ToDouble( val1 ),
                           d2 = Convert.ToDouble( val2 );

                    retVal = d1 == d2 ? Equality.Equal : (d1 > d2 ? Equality.GreaterThan : Equality.LessThan);
                }
                break;

                case DataType.Int:
                {
                    Int32 i1 = Convert.ToInt32( val1 ),
                          i2 = Convert.ToInt32( val2 );

                    /*if( i1 > 65 )
                    {
                        int debug = 0;
                    }*/
                    retVal = i1 == i2 ? Equality.Equal : (i1 > i2 ? Equality.GreaterThan : Equality.LessThan);
                }
                break;

                case DataType.UInt:
                {
                    UInt32 i1 = Convert.ToUInt32( val1 ),
                           i2 = Convert.ToUInt32( val2 );

                    retVal = i1 == i2 ? Equality.Equal : (i1 > i2 ? Equality.GreaterThan : Equality.LessThan);
                }
                break;

                case DataType.DateTime:
                {
                    DateTime dt1 = Convert.ToDateTime( val1 ),
                             dt2 = Convert.ToDateTime( val2 );
                    
                    retVal = dt1 == dt2 ? Equality.Equal : (dt1 > dt2 ? Equality.GreaterThan : Equality.LessThan);
                }
                break;

                case DataType.String:
                {                    
                    int ncomp = string.Compare( val1.ToString(), val2.ToString(), StringComparison.CurrentCulture );
                    retVal = ncomp == 0 ? Equality.Equal : (ncomp > 0 ? Equality.GreaterThan : Equality.LessThan);
                }
                break;
            }

            return retVal;
        }

        static object convertValueToType( object value, DataType dataType )
        {
            object retVal = null;

            switch( dataType )
            {
                case DataType.Byte:
                {
                    retVal = Convert.ToByte( value );
                }
                break;

                case DataType.Bool:
                {
                    retVal = Convert.ToBoolean( value );
                }
                break;

                case DataType.Float:
                {
                    retVal = Convert.ToSingle( value );
                }
                break;

                case DataType.Double:
                {
                    retVal = Convert.ToDouble( value );
                }
                break;

                case DataType.Int:
                {
                    retVal = Convert.ToInt32( value );
                }
                break;

                case DataType.UInt:
                {
                    retVal = Convert.ToUInt32( value );
                }
                break;

                case DataType.DateTime:
                {
                    retVal = Convert.ToDateTime( value );
                }
                break;

                case DataType.String:
                {
                    retVal = value.ToString();
                }
                break;
            }
            return retVal;
        }

        internal object[][] getAllRecords( string[] fieldList, bool includeIndex, string[] orderByList )
        {
            checkIsDbOpen();

            // If there are no records, return
            if( _numRecords == 0 )
                return null;

            object[][] result = null;

            try
            {
                //lockRead( false );

                // Read the index
                //Int32[] vIndex = readIndex();

                result = new object[_numRecords][];

                //bool includeIndex = fieldListContainsIndex( fieldList );

                // Read each record and add it to an array
                Int32 idx = 0,          
                      nRow = 0;

                foreach( Int32 offset in _index )
                {
                    // Read the record
                    bool deleted;
                    object[] record = readRecord( offset, includeIndex, out deleted );
                    Debug.Assert( !deleted );
                    
                    if( fieldList != null )
                    {
                        object[] tmpRecord = new object[fieldList.Length + (includeIndex ? 1 : 0)]; // one extra for the index
                        int n=0;
                        foreach( string fieldName in fieldList )
                        {
                            if( !_fields.ContainsKey( fieldName ) )
                                throw new FileDbException( string.Format( FileDbException.InvalidFieldName, fieldName ), FileDbExceptions.InvalidFieldName );
                            Field fld = _fields[fieldName];
                            tmpRecord[n++] = record[fld.Ordinal];
                        }
                        record = tmpRecord;
                    }

                    if( includeIndex )
                    {
                        // set the index into the record
                        record[record.Length - 1] = idx++;
                    }

                    // Add it to the result
                    result[nRow++] = record;
                }
            }
            finally
            {
                //unlock( false, false );
            }

            // Re-order as required
            if( result != null && orderByList != null )
                orderBy( result, fieldList, orderByList );

            return result;
        }

#if false
        /*
         * retrieves all keys in the database, each in an array.
         * returns: all database record keys as an array, in order, or false
         * if the database does not use keys.
         */
        internal object[] getKeys()
        {
            checkIsDbOpen();

            // If there is no key, return false
            if( string.IsNullOrEmpty( _primaryKey ) )
                return null;

            // If there are no records, return
            if( _numRecords == 0 )
                return null;

            object[] record = null;

            try
            {
                //lockRead( false );

                // Read the index
                Int32[] vIndex = readIndex();

                var lstRecords = new List<object>();

                // Read each record key and add it to an array
                foreach( Int32 offset in vIndex )
                {
                    // Read the record key and add it to the result
                    object key = readRecordKey( _dataReader, offset );
                    lstRecords.Add( key );
                }

                if( lstRecords.Count > 0 )
                    record = lstRecords.ToArray();
            }
            finally
            {
                //unlock( false, false );
            }

            return record;
        }
#endif
        /// <summary>
        /// Searches the database for an item, and returns true if found, false otherwise.
        /// </summary>
        /// <param name="key">rimary key of record to search for, or the record
        /// number (zero based) for databases without a primary key</param>
        /// <returns>true if found, false otherwise</returns>
        /// 
        internal bool recordExists( Int32 key )
        {
            checkIsDbOpen();

            // Assume we won't find it until proven otherwise
            bool result = false;

            try
            {
                //lockRead( false );

                // Read the index
                //Int32[] vIndex = readIndex();

                if( !string.IsNullOrEmpty( _primaryKey ) )
                {
                    // Do a binary search to find the item
                    Int32 pos = bsearch( _index, 0, _numRecords - 1, key );

                    if( pos > 0 )
                    {
                        // Found!
                        result = true;
                    }
                }
                else
                {
                    // if there is no primary key, the record number must be Int32
                    if( key.GetType() != typeof( int ) )
                    {
                        throw new FileDbException( FileDbException.NeedIntegerKey, FileDbExceptions.NeedIntegerKey );
                    }

                    Int32 nkey = (Int32) key;

                    // Ensure the record number is within range
                    if( (nkey < 0) || (nkey >= _numRecords) )
                    {
                        throw new FileDbException( string.Format( FileDbException.RecordNumOutOfRange, nkey ), FileDbExceptions.IndexOutOfRange );
                    }

                    // ... must be found!
                    result = true;
                }
            }
            finally
            {
                //unlock( false, false );
            }

            return result;
        }

        /// <summary>
        /// Returns the number of records in the database
        /// </summary>
        /// <returns>the number of records in the database</returns>
        /// 
        internal Int32 getRecordCount()
        {
            checkIsDbOpen();
            return _numRecords;
        }

        /// <summary>
        /// Returns the number of deleted records in the database, that would be removed if cleanup() is called.
        /// </summary>
        /// <returns>the number of deleted records in the database</returns>
        /// 
        internal Int32 getDeletedRecordCount()
        {
            checkIsDbOpen();
            return _numDeleted;
        }

        /// <summary>
        /// Returns the current database schema in the same form
        /// as that used in the parameter for the create(...) method.
        /// </summary>
        /// <returns></returns>
        /// 
        internal Field[] getSchema()
        {
            checkIsDbOpen();
            return _fields.ToArray();
        }

#if false
        // TODO: implement
        internal void addfield( Field fieldToAdd, object defaultVal )
        {
            checkIsDbOpen();
            checkReadOnly();

            if( fieldToAdd.IsPrimaryKey && ( !string.IsNullOrEmpty(_primaryKey) ) )
                throw new FileDbException( string.Format( FileDbException.DatabaseAlreadyHasPrimaryKey, _primaryKey ),
                    FileDbExceptions.DatabaseAlreadyHasPrimaryKey );

            // Only allow keys if the database has no records
            if( fieldToAdd.IsPrimaryKey && (_numRecords > 0) )
                throw new FileDbException( FileDbException.PrimaryKeyCannotBeAdded, FileDbExceptions.PrimaryKeyCannotBeAdded );

            // ensure no deleted records either
            if( _numDeleted > 0 )
                throw new FileDbException( FileDbException.CantAddOrRemoveFieldWithDeletedRecords, FileDbExceptions.PrimaryKeyCannotBeAdded );

            // Make sure the name of the field is unique
            foreach( Field f in _fields )
            {
                if( string.Compare( fieldToAdd.Name, f.Name, StringComparison.CurrentCultureIgnoreCase ) == 0 )
                    throw new FileDbException( FileDbException.FieldNameAlreadyExists, FileDbExceptions.FieldNameAlreadyExists );
            }

            // Make sure that the array or boolean value is NOT the key
            if( fieldToAdd.IsPrimaryKey && 
                    (fieldToAdd.IsArray ||
                        !(fieldToAdd.DataType == DataType.Int || fieldToAdd.DataType == DataType.String)) )
                throw new FileDbException( string.Format( FileDbException.InvalidPrimaryKeyType, fieldToAdd.Name ),
                    FileDbExceptions.InvalidPrimaryKeyType );

            verifyFieldSchema( fieldToAdd, defaultVal );

            try
            {
                //lockWrite( false );

                // Note that we attempt the file creation under the DB lock, so
                // that another process doesn't try to create the same file at the
                // same time.
                IsolatedStorageFile isoFile = IsolatedStorageFile.GetUserStoreForApplication();
                string tmpFilename = Path.GetFileNameWithoutExtension( _dbName ) + ".tmp";
                var tmpdb = new IsolatedStorageFileStream( tmpFilename, FileMode.CreateNew, FileAccess.Write, isoFile );

                fieldToAdd.Ordinal = _fields.Count;

                // Add the field to the schema
                _fields.Add( fieldToAdd );

                // Do we have a new primary key?
                if( fieldToAdd.IsPrimaryKey )
                {
                    _primaryKeyField = fieldToAdd;
                    _primaryKey = fieldToAdd.Name.ToUpper();
                }

                // Since we've effectively done a cleanup(), set the number 
                // of (unclean) deleted items to zero.
                _numDeleted = 0;
// TODO: finish - not yet completed
                var tmpDataWriter = new BinaryWriter( tmpdb );

                // Write the new schema
                writeDbHeader( tmpDataWriter );
                writeSchema( tmpDataWriter );

                var newFields = _fields;

                // Read in the current index
                List<Int32> vIndex = readIndex();

                // Now translate the data file.  For each index entry, read in
                // the record, add in the new default value, then write it back
                // out to a new temporary file.  Then move that temporary file
                // back over the old data file.

                // For each item in the index, move it from the current database
                // file to the new one.  Also update the new file offset in the index
                // so we can write it back out to the index file

                var oldFields = _fields;

                for( Int32 idx = 0; idx < _numRecords; ++idx )
                {
                    Int32 offset = vIndex[idx];

                    // Read in the entire record
                    _fields = oldFields;
                    bool deleted;
                    object[] record = readRecord( offset, false, out deleted );
                    Debug.Assert( !deleted );

                    // Save the new file offset

                    vIndex[idx] = (Int32) tmpdb.Position;

                    // Add in the new field to the record
                    if( fieldToAdd.IsAutoInc )
                    {
                        // brettg: REVIEW
                        fieldToAdd.CurAutoIncVal += 1;
                        record[fieldToAdd.Ordinal] = fieldToAdd.AutoIncStart;
                    }
                    else
                        record[fieldToAdd.Ordinal] = defaultVal;

                    try
                    {
                        // Write out the record to the temporary file
                        writeRecord( tmpDataWriter, record, -1, false );
                    }
                    catch
                    {
                        // read the old schema back in
                        readSchema();

                        // Error writing item to the database
                        tmpdb.Close();
                        isoFile.DeleteFile( tmpFilename );
                        throw;
                    }
                }

                // Move the temporary file over the original database file.
                tmpdb.Close();
                _dataStrm.Close();
                isoFile.DeleteFile( _dbName );
                // TODO: must have workaround for WP
                isoFile.MoveFile( tmpFilename, _dbName );

                // Write out the index (which may have been overwritten)
                writeIndex( _metaWriter, vIndex );

                // get all the new schema
                readSchema();

                // Re-open the database data file
                _dataStrm = new IsolatedStorageFileStream( _dbName, FileMode.Open, FileAccess.ReadWrite, isoFile );
            }
            finally
            {
                //unlock( false, false );
            }
        }
#endif

#if false
        // TODO: implement
        internal bool removeField( string fieldName )
        {
            checkIsDbOpen();
            checkReadOnly();

            Field fieldToRemove = _fields[fieldName.ToUpper()];

            if( fieldToRemove == null )
            {
                throw new FileDbException( string.Format( InvalidFieldName, fieldName ), FileDbExceptions.InvalidFieldName );
            }

            try
            {
                //lockWrite( false );

                // Note that we attempt the file creation under the DB lock, so
                // that another process doesn't try to create the same file at the
                // same time.
                IsolatedStorageFile isoFile = IsolatedStorageFile.GetUserStoreForApplication();
                string tmpFilename = Path.GetFileNameWithoutExtension( _dbName ) + ".tmp";
                var tmpdb = new IsolatedStorageFileStream( tmpFilename, FileMode.CreateNew, FileAccess.Write, isoFile );

                // Save a copy of the field list.  It is needed in the main
                // loop to play with the fields[] array and {read, write}_record(...)
                var oldFields = _fields;
                var newFields = new List<Field>( _fields );
                newFields.Remove( fieldToRemove );                    

                // Read in the current index
                Int32[] vIndex = readIndex();
// TODO: test
                var tmpDataWriter = new BinaryWriter( tmpdb );

                if( fieldToRemove == _primaryKeyField )
                    _primaryKeyField = null;

                // Is the field to be removed the primary key?
                if( string.Compare( _primaryKey, fieldName, StringComparison.CurrentCultureIgnoreCase ) == 0 )
                    _primaryKey = string.Empty;

                // Write the new schema
                _fields = newFields;
                writeDbHeader( tmpDataWriter );
                writeSchema( tmpDataWriter );

                // Now translate the data file.  For each index entry, read in
                // the record, remove the deleted field, then write it back
                // out to a new temporary file.  Then move that temporary file
                // back over the old data file.

                // For each item in the index, move it from the current database
                // file to the new one.  Also update the new file offset in the index
                // so we can write it back out to the index file

                if( _fields.Count > 1 )
                {
                    for( Int32 i = 0; i < _numRecords; i++ )
                    {
                        // Use the original fields[] array so read_record(...) will 
                        // operate correctly for the old-format database .dat file.
                        _fields = oldFields;

                        // Read in the entire record
                        bool deleted;
                        object[] record = readRecord( vIndex[i], out deleted );
                        Debug.Assert( !deleted );

                        var newRecord = new object[record.Length - 1];

                        // Save the new file offset
                        vIndex[i] = (Int32) tmpdb.Position;

                        // Remove the field from the record

                        for( Int32 n = 0; n < _fields.Count; n++ )
                        {
                            Field fld = _fields[n];

                            if( fld != fieldToRemove )
                            {
                                newRecord[n] = record[n];
                            }
                        }
                        // Write out the record to the temporary file
                        _fields = newFields;
                        writeRecord( tmpDataWriter, record, -1, false );
                    }
                }
                else
                {
                    // We have deleted the last field, so there are essentially no records left
                    _numRecords = 0;
                }

                _fields = newFields;

                // update numRecords
                writeNumRecords( tmpDataWriter );

                tmpDataWriter.Flush();

                // Move the temporary file over the original database file.
                tmpdb.Close();
                _dataStrm.Close();
                isoFile.DeleteFile( _dbName );
                // TODO: must have workaround for WP
                isoFile.MoveFile( tmpFilename, _dbName );

                // Since we've effectively done a cleanup(), set the number 
                // of (unclean) deleted items to zero.
                _numDeleted = 0;

                // Write out the index
                writeIndex( _metaWriter, vIndex );

                // Re-open the database data file
                _dataStrm = new IsolatedStorageFileStream( _dbName, FileMode.Open, FileAccess.ReadWrite, isoFile );
            }
            finally
            {
                //unlock( false );
            }

            return true;
        }
#endif

        /// <summary>
        /// Flush the in-memory buffers to disk
        /// </summary>
        /// 
        internal void flush()
        {
            if( !_openReadOnly )
            {
                _dataWriter.Flush();
                _dataStrm.Flush();
                writeIndex( _dataStrm, _dataWriter, _index );
            }
        }

        #endregion internal

        ///////////////////////////////////////////////////////////////////////
        #region private methods

        // make all fieldnames uppercase
        FieldValues normalizeFieldNames( FieldValues record )
        {
            var newRecord = new FieldValues( record.Count );

            foreach( string fieldName in record.Keys )
            {
                newRecord.Add( fieldName.ToUpper(), record[fieldName] );
            }
            return newRecord;
        }

        /// <summary>
        /// Helper
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        ///
        void verifyRecordSchema( FieldValues record )
        {
            // Verify record as compared to the schema

            foreach( string fieldName in record.Keys )
            {
                if( string.Compare( fieldName, StrIndex, StringComparison.OrdinalIgnoreCase ) == 0 )
                    continue;

                if( !_fields.ContainsKey( fieldName ) )
                    throw new FileDbException( string.Format( FileDbException.InvalidFieldName, fieldName ), FileDbExceptions.InvalidFieldName );

                Field field = _fields[fieldName];
                verifyFieldSchema( field, record[fieldName] );
            }
        }

        /// <summary>
        /// Helper
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        ///
        void verifyFieldSchema( Field field, object value )
        {
            // We don't mind if they include a AUTOINC field,
            // as we determine its value in any case

            if( field.IsAutoInc )
                return;  // no value should be passed for autoinc fields, but we'll allow it rather than throwing error

            // Ensure they have included an entry for each record field
            /* brettg: I'm gonna allow sparse records and just write 0 or null for the value of missing fields
            if( !record.ContainsKey( field.Name ) )
            {
                throw("Missing field during add: key");
            }*/

            if( value != null )
            {
                if( field.IsArray && !value.GetType().IsArray )
                    throw new FileDbException( string.Format( FileDbException.NonArrayValue, field.Name ), FileDbExceptions.NonArrayValue );

                // Verify the type
                switch( field.DataType )
                {
                    // hopefully these will throw err if wrong type

                    case DataType.Byte:
                        if( field.IsArray )
                        {
                            value = (Byte[]) value;
                        }
                        else
                        {
                            Byte b = (value == null? (Byte) 0 : Convert.ToByte( value ));
                        }
                    break;

                    case DataType.Int:
                        if( field.IsArray )
                        {
                            value = (Int32[]) value;
                        }
                        else
                        {
                            Int32 n = (value == null ? 0 : Convert.ToInt32( value ));
                        }
                        break;

                    case DataType.UInt:
                        if( field.IsArray )
                        {
                            value = (UInt32[]) value;
                        }
                        else
                        {
                            UInt32 n = (value == null? 0 : Convert.ToUInt32( value ));
                        }
                        break;

                    case DataType.String:
                        if( field.IsArray )
                        {
                            value = (string[]) value;
                        }
                        else
                        {
                            // any object can be converted to string
                            string s = (value == null? string.Empty : value.ToString());
                        }
                        break;

                    case DataType.Float:
                        if( field.IsArray )
                        {
                            value = (float[]) value;
                        }
                        else
                        {
                            float f = (value == null ? 0 : Convert.ToSingle( value ));
                        }
                        break;

                    case DataType.Double:
                        if( field.IsArray )
                        {
                            value = (double[]) value;
                        }
                        else
                        {
                            double f = (value == null? 0 : Convert.ToDouble( value ));
                        }
                        break;

                    case DataType.Bool:
                        if( field.IsArray )
                        {
                            // can be Byte[] or bool[]
                            if( value.GetType() == typeof( Byte[] ) )                            
                                value = (Byte[]) value;
                            else
                                value = (bool[]) value;
                        }
                        else
                        {
                            bool b = (value == null? false : Convert.ToBoolean( value )); // brettg: should be 1 or 0
                        }
                        break;

                    case DataType.DateTime:
                        if( field.IsArray )
                        {
                            // can be string[] or DateTime[]
                            if( value.GetType() == typeof( String[] ) )                            
                                value = (String[]) value;
                            else
                                value = (DateTime[]) value;
                        }
                        else
                        {
                            DateTime d = (value == null? DateTime.MinValue : Convert.ToDateTime( value ));
                        }
                        break;

                    default:
                        // Unknown type...!
                        throw new FileDbException( string.Format( FileDbException.StrInvalidDataType2, field.Name, field.DataType.ToString(), value.GetType().Name ),
                            FileDbExceptions.InvalidDataType );
                }
            }
        }
        
        /// <summary>
        /// Function to return the index values.  We assume the
        /// database has been locked before calling this function;
        /// </summary>
        /// <returns></returns>
        /*
        Int32[] readIndex()
        {
            // I think ToArray may be efficient, just returning its internal array
            return readIndex2().ToArray();
            #if false
            _dataStrm.Seek( _indexStartPos, SeekOrigin.Begin );

            // Read in the index
            Int32[] vIndex = new Int32[_numRecords];

            // brettg: must wrap with try/catch because there may be one less indices than _numRecords
            // e.g. when called from addRecord
            try
            {
                for( Int32 i = 0; i < _numRecords; i++ )
                    vIndex[i] = _dataReader.ReadInt32();
            }
            catch { }

            _deletedRecords = new List<Int32>( _numDeleted );
            if( _numDeleted > 0 )
            {
                try
                {
                    for( Int32 i = 0; i < _numDeleted; i++ )
                        _deletedRecords.Add( _dataReader.ReadInt32() );
                }
                catch { }
            }

            return vIndex;
            #endif
        }*/
        
        /// <summary>
        /// Use this version if you will need to add/remove indices
        /// </summary>
        ///
        List<Int32> readIndex()
        {
            _dataStrm.Seek( _indexStartPos, SeekOrigin.Begin );

            // Read in the index
            var vIndex = new List<Int32>(_numRecords);

            // brettg: must wrap with try/catch because there may be one less indices than _numRecords
            // e.g. when called from addRecord
            try
            {
                for( Int32 i = 0; i < _numRecords; i++ )
                    vIndex.Add( _dataReader.ReadInt32() );
            }
            catch ( Exception ex )
            {
                Debug.Assert( false );
            }

            readMetaData( _dataReader );

            _deletedRecords = new List<Int32>( _numDeleted );
            if( _numDeleted > 0 )
            {
                try
                {
                    for( Int32 i = 0; i < _numDeleted; i++ )
                        _deletedRecords.Add( _dataReader.ReadInt32() );
                }
                catch { }
            }

            return vIndex;
        }

        /// <summary>
        /// function to write the index values.  We assume the
        /// database has been locked before calling this function.
        /// </summary>
        /*
        void writeIndex( Stream fileStrm, BinaryWriter writer, Int32[] index )
        {
            fileStrm.Seek( _indexStartPos, SeekOrigin.Begin );

            // bg NOTE: we use the min of _numRecords and the index.Length
            // because with some callers the index has an extra index at the end 
            // to effectively truncate the index

            for( Int32 i = 0; i < Math.Min( _numRecords, index.Length ); i++ )
                writer.Write( index[i] );

            Debug.Assert( _numDeleted == _deletedRecords.Count );
            
            for( Int32 i = 0; i < _deletedRecords.Count; i++ )
                writer.Write( _deletedRecords[i] );

            // we'll require that readIndex be called after this so its not hanging around in memory
            _deletedRecords = null;

            writer.Flush();

            fileStrm.SetLength( fileStrm.Position );
        }*/

        /// <summary>
        /// function to write the index values.  We assume the
        /// database has been locked before calling this function.
        /// </summary>
        /// 
        void writeIndex( Stream fileStrm, BinaryWriter writer, List<Int32> index )
        {
            fileStrm.Seek( _indexStartPos, SeekOrigin.Begin );

            // bg NOTE: we use the min of _numRecords and the index.Length
            // because with some callers the index has an extra index at the end 
            // to effectively truncate the index

            for( Int32 i = 0; i < Math.Min( _numRecords, index.Count ); i++ )
                writer.Write( index[i] );

            Debug.Assert( _numDeleted == _deletedRecords.Count );

            for( Int32 i = 0; i < Math.Min( _numDeleted, _deletedRecords.Count ); i++ )
                writer.Write( _deletedRecords[i] );

            // whenever we write the index we must write the MetaData
            writeMetaData( writer );

            writer.Flush();

            fileStrm.SetLength( fileStrm.Position );
        }

        void writeRecord( BinaryWriter dataWriter, FieldValues record, Int32 size, bool deleted )
        {
            // Auto-calculate the record size
            if( size < 0 && _encryptor == null )
                size = getRecordSize( record );

            #if DEBUG
            int proposedSize = size;
            #endif

            MemoryStream memStrm = null;
            BinaryWriter origDataWriter = dataWriter;

            if( _encryptor != null )
            {
                memStrm = new MemoryStream( 200 );
                dataWriter = new BinaryWriter( memStrm );
            }
            else
            {
                if( deleted )
                    size = -size;
                // Write out the size of the record
                dataWriter.Write( size );
            }

            #if DEBUG
            int startPos;
            if( _encryptor != null )
                startPos = (int) memStrm.Position;
            else
                startPos = (int) _dataStrm.Position;
            #endif

            // Write out the entire record in field order
            foreach( Field field in _fields )
            {
                // all fieldnames in the record should now be upper case
                string uprName = field.Name.ToUpper();
                object data = null;

                if( record.ContainsKey( uprName ) )
                    data = record[uprName];

                writeItem( dataWriter, field, data );
            }
            
            if( AutoFlush ) flush();

            if( _encryptor != null )
            {
                memStrm.Seek( 0, SeekOrigin.Begin );
                //using( var outStrm = new MemoryStream( (int) memStrm.Length * 2 ) )
                {
                    byte[] bytes = _encryptor.Encrypt( memStrm.ToArray() );
                    //byte[] bytes = outStrm.ToArray();
                    size = bytes.Length;
                    if( deleted )
                        size = -size;
                    // Write out the size of the record
                    origDataWriter.Write( size );
                    origDataWriter.Write( bytes );
                }
            }

            #if DEBUG
            int endPos;
            if( _encryptor != null )
                endPos = (int) memStrm.Length;
            else
                endPos = (int) _dataStrm.Position;                
            int actualSize = endPos - startPos;
            Debug.Assert( actualSize == proposedSize );
            #endif
        }

        /// <summary>
        /// Use this version only if all of the fields are present and in the correct order in the array
        /// </summary>
        /// <param name="dataWriter"></param>
        /// <param name="record"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        ///
        void writeRecord( BinaryWriter dataWriter, object[] record, Int32 size, bool deleted )
        {
            // Auto-calculate the record size
            if( size < 0 )
                size = getRecordSize( record );

            MemoryStream memStrm = null;
            BinaryWriter origDataWriter = dataWriter;

            if( _encryptor != null )
            {
                memStrm = new MemoryStream( 200 );
                dataWriter = new BinaryWriter( memStrm );
            }
            else
            {
                if( deleted )
                    size = -size;
                // Write out the size of the record
                dataWriter.Write( size );
            }

            // Write out the entire record
            foreach( Field field in _fields )
            {
                object data = record[field.Ordinal];
                writeItem( dataWriter, field, data );
            }

            if( AutoFlush ) flush();

            if( _encryptor != null )
            {
                memStrm.Seek( 0, SeekOrigin.Begin );
                //using( var outStrm = new MemoryStream( (int) memStrm.Length * 2 ) )
                {
                    byte[] bytes = _encryptor.Encrypt( memStrm.ToArray() );
                    //byte[] bytes = outStrm.ToArray();
                    size = bytes.Length;
                    if( deleted )
                        size = -size;
                    // Write out the size of the record
                    origDataWriter.Write( size );
                    origDataWriter.Write( bytes );
                }
            }
        }

        void writeRecordRaw( BinaryWriter dataWriter, byte[] record, bool deleted )
        {
            BinaryWriter origDataWriter = dataWriter;

            int size = record.Length;

            if( deleted )
                size = -size;

            dataWriter.Write( size );
            dataWriter.Write( record );
        }

        Int32 getRecordSize( FieldValues record )
        {
            Int32 size = 0;

            // Size up each field

            foreach( Field field in _fields )
            {
                object data = null;
                string uprName = field.Name.ToUpper();
                if( record.ContainsKey( uprName ) )
                    data = record[uprName];
                size += getItemSize( field, data );
            }
            return size;
        }

        /// <summary>
        /// Use this version only if all of the fields are present and in the correct order in the array
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        ///
        Int32 getRecordSize( object[] record )
        {
            Int32 size = 0;

            // Size up each field

            foreach( Field field in _fields )
            {
                object data = record[field.Ordinal];
                size += getItemSize( field, data );
            }
            return size;
        }

        Int32 getItemSize( Field field, object data )
        {
            Int32 size = 0;

            switch( field.DataType )
            {
                case DataType.Byte:
                    if( field.IsArray )
                    {
                        size = sizeof( Int32 );
                        Byte[] arr = (Byte[]) data;
                        if( arr != null )
                            size += arr.Length;
                    }
                    else
                        size = 1;
                break;

                case DataType.Int:
                    if( field.IsArray )
                    {
                        size = sizeof( Int32 );
                        Int32[] arr = (Int32[]) data;
                        if( arr != null )
                            size += arr.Length * sizeof( Int32 );
                    }
                    else
                        size = sizeof( Int32 );
                break;

                case DataType.UInt:
                if( field.IsArray )
                {
                    size = sizeof( UInt32 );
                    UInt32[] arr = (UInt32[]) data;
                    if( arr != null )
                        size += arr.Length * sizeof( UInt32 );
                }
                else
                    size = sizeof( UInt32 );
                break;

                case DataType.Float:
                    if( field.IsArray )
                    {
                        size = sizeof( Int32 );
                        float[] arr = (float[]) data;
                        if( arr != null )
                        {
                            size += arr.Length * sizeof( float );
                            #if DEBUG
                            _testWriter = getTestWriter();
                            foreach( float d in arr )
                            {
                                _testWriter.Write( d );
                            }
                            _testWriter.Flush();
                            int testSize = (Int32) _testStrm.Position;
                            Debug.Assert( testSize == size - sizeof( Int32 ) );
                            #endif
                        }
                    }
                    else
                        size = sizeof( float );
                break;

                case DataType.Double:
                if( field.IsArray )
                {
                    size = sizeof( Int32 );
                    double[] arr = (double[]) data;
                    if( arr != null )
                    {
                        size += arr.Length * sizeof( double );
                        #if DEBUG
                        _testWriter = getTestWriter();
                        foreach( double d in arr )
                        {
                            _testWriter.Write( d );
                        }
                        _testWriter.Flush();
                        int testSize = (Int32) _testStrm.Position;
                        Debug.Assert( testSize == size - sizeof( Int32 ) );
                        #endif
                    }
                }
                else
                    size = sizeof( double );
                break;

                case DataType.Bool:
                    if( field.IsArray )
                    {
                        size = sizeof( Int32 );
                        if( data != null )
                        {
                            if( data.GetType() == typeof( bool[] ) )
                            {
                                bool[] arr = (bool[]) data;
                                if( arr != null )
                                    size += arr.Length;
                            }
                            else if( data.GetType() == typeof( Byte[] ) )
                            {
                                Byte[] arr = (Byte[]) data;
                                if( arr != null )
                                    size += arr.Length;
                            }
                        }
                    }
                    else
                        size = 1;
                break;

                case DataType.DateTime:
                {
                    // DateTimes are stored as string
                    _testWriter = getTestWriter();

                    if( field.IsArray )
                    {
                        size = sizeof( Int32 );

                        if( data != null )
                        {
                            if( data.GetType() == typeof( DateTime[] ) )
                            {
                                DateTime[] arr = (DateTime[]) data;
                                if( arr != null )
                                {
                                    foreach( DateTime dt in arr )
                                        _testWriter.Write( dt.ToString( DateTimeFmt ) );
                                }
                            }
                            else // must be string
                            {
                                string[] arr = (string[]) data;
                                if( arr != null )
                                {
                                    foreach( string s in arr )
                                    {
                                        DateTime dt = DateTime.Parse( s );
                                        _testWriter.Write( dt.ToString( DateTimeFmt ) );
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if( data == null )
                            _testWriter.Write( string.Empty );
                        else
                        {
                            DateTime dt;
                            if( data.GetType() == typeof( DateTime ) )
                            {
                                dt = (DateTime) data;
                            }
                            else // must be string
                            {
                                dt = Convert.ToDateTime( data );
                            }
                            _testWriter.Write( dt.ToString( DateTimeFmt ) );
                        }
                    }

                    _testWriter.Flush();
                    size += (Int32) _testStrm.Position;
                }
                break;

                case DataType.String:
                {
                    _testWriter = getTestWriter();

                    if( field.IsArray )
                    {
                        size = sizeof( Int32 );
                        string[] arr = (string[]) data;
                        if( arr != null )
                        {
                            foreach( string s in arr )
                            {
                                _testWriter.Write( s == null ? string.Empty : s ); // can't write null strings
                            }
                        }
                    }
                    else
                    {
                        if( data == null )
                            _testWriter.Write( string.Empty );  // can't write null strings
                        else
                            _testWriter.Write( data.ToString() );
                    }

                    if( _testStrm.Position > 0 )
                    {
                        _testWriter.Flush();
                        size += (Int32) _testStrm.Position;
                    }
                }
                break;

                default:
                    throw new FileDbException( string.Format( FileDbException.StrInvalidDataType, (Int32) field.DataType ), FileDbExceptions.InvalidDataType );

            }

            return size;
        }

        private BinaryWriter getTestWriter()
        {
            if( _testStrm == null )
            {
                _testStrm = new MemoryStream();
                _testWriter = new BinaryWriter( _testStrm );
            }
            else
                _testStrm.Seek( 0, SeekOrigin.Begin );

            return _testWriter;
        }

        /// <summary>
        /// Write a single field to the file
        /// </summary>
        /// <param name="dataWriter"></param>
        /// <param name="field"></param>
        /// <param name="data"></param>
        /// 
        void writeItem( BinaryWriter dataWriter, Field field, object data )
        {
            switch( field.DataType )
            {
                case DataType.Byte:
                    if( field.IsArray )
                    {
                        Byte[] arr = (Byte[]) data;
                        // write the length
                        dataWriter.Write( arr != null ? arr.Length : (Int32) (-1) );

                        if( arr != null )
                        {
                            foreach( Byte b in arr )
                            {
                                dataWriter.Write( b );
                            }
                        }
                    }
                    else
                    {
                        if( data == null )
                        {
                            dataWriter.Write( (Byte) 0 );
                        }
                        else
                        {
                            Byte val;
                            if( data.GetType() != typeof( Byte ) )
                                val = Convert.ToByte( data );
                            else
                                val = (Byte) data;
                            dataWriter.Write( val );
                        }
                    }
                    break;

                case DataType.Int:
                    if( field.IsArray )
                    {
                        Int32[] arr = (Int32[]) data;
                        // write the length
                        dataWriter.Write( arr != null ? arr.Length : (Int32) (-1) );

                        if( arr != null )
                        {
                            foreach( Int32 i in arr )
                            {
                                dataWriter.Write( i );
                            }
                        }
                    }
                    else
                    {
                        if( data == null )
                        {
                            dataWriter.Write( (Int32) 0 );
                        }
                        else
                        {
                            Int32 val;
                            if( data.GetType() != typeof( Int32 ) )
                                val = Convert.ToInt32( data );
                            else
                                val = (Int32) data;
                            dataWriter.Write( val );
                        }
                    }
                    break;

                case DataType.UInt:
                    if( field.IsArray )
                    {
                        UInt32[] arr = (UInt32[]) data;
                        // write the length
                        dataWriter.Write( arr != null ? arr.Length : (Int32) (-1) );

                        if( arr != null )
                        {
                            foreach( UInt32 i in arr )
                            {
                                dataWriter.Write( i );
                            }
                        }
                    }
                    else
                    {
                        if( data == null )
                        {
                            dataWriter.Write( (UInt32) 0 );
                        }
                        else
                        {
                            UInt32 val;
                            if( data.GetType() != typeof( UInt32 ) )
                                val = Convert.ToUInt32( data );
                            else
                                val = (UInt32) data;
                            dataWriter.Write( val );
                        }
                    }
                    break;

                case DataType.Float:
                    if( field.IsArray )
                    {
                        float[] arr = (float[]) data;
                        // write the length
                        dataWriter.Write( arr != null ? arr.Length : (Int32) (-1) );

                        if( arr != null )
                        {
                            foreach( float d in arr )
                            {
                                dataWriter.Write( d );
                            }
                        }
                    }
                    else
                    {
                        if( data == null )
                        {
                            dataWriter.Write( 0f );
                        }
                        else
                        {
                            float val;
                            if( data.GetType() != typeof( float ) )
                                val = Convert.ToSingle( data );
                            else
                                val = (float) data;
                            dataWriter.Write( val );
                        }
                    }
                    break;

                case DataType.Double:
                    if( field.IsArray )
                    {
                        double[] arr = (double[]) data;
                        // write the length
                        dataWriter.Write( arr != null ? arr.Length : (Int32) (-1) );

                        if( arr != null )
                        {
                            foreach( double d in arr )
                            {
                                dataWriter.Write( d );
                            }
                        }
                    }
                    else
                    {
                        if( data == null )
                        {
                            dataWriter.Write( 0d );
                        }
                        else
                        {
                            double val;
                            if( data.GetType() != typeof( double ) )
                                val = Convert.ToDouble( data );
                            else
                                val = (double) data;
                            dataWriter.Write( val );
                        }
                    }
                    break;

                case DataType.Bool:
                {
                    if( field.IsArray )
                    {
                        if( data != null )
                        {
                            if( data.GetType() == typeof( bool[] ) )
                            {
                                bool[] arr = (bool[]) data;
                                // write the length
                                dataWriter.Write( arr.Length );

                                if( arr != null )
                                {
                                    foreach( bool b in arr )
                                    {
                                        dataWriter.Write( b ? (Byte) 1 : (Byte)0 );
                                    }
                                }
                            }
                            else if( data.GetType() == typeof( Byte[] ) )
                            {
                                Byte[] arr = (Byte[]) data;
                                // write the length
                                dataWriter.Write( arr.Length );

                                if( arr != null )
                                {
                                    foreach( Byte b in arr )
                                    {
                                        dataWriter.Write( b );
                                    }
                                }
                            }
                            else
                                throw new FileDbException( FileDbException.InValidBoolType, FileDbExceptions.InvalidDataType );
                        }
                        else
                            dataWriter.Write( (Int32) (-1) );
                    }
                    else
                    {
                        if( data == null )
                        {
                            dataWriter.Write( (Byte) 0 );
                        }
                        else
                        {
                            bool val;
                            if( data.GetType() == typeof( bool ) )
                                val = (bool) data;
                            else if( data.GetType() == typeof( Byte ) )
                                val = ((Byte) data) == 0 ? true : false;
                            else
                                throw new FileDbException( FileDbException.InValidBoolType, FileDbExceptions.InvalidDataType );

                            dataWriter.Write( val ? (Byte) 1 : (Byte) 0 );
                        }
                    }
                }
                break;

                case DataType.DateTime:
                {
                    if( field.IsArray )
                    {
                        if( data != null )
                        {
                            if( data.GetType() == typeof( DateTime[] ) )
                            {
                                DateTime[] arr = (DateTime[]) data;
                                // write the length
                                dataWriter.Write( arr != null ? arr.Length : (Int32) (-1) );

                                if( arr != null )
                                {
                                    foreach( DateTime dt in arr )
                                    {
                                        dataWriter.Write( dt.ToString( DateTimeFmt ) );
                                    }
                                }
                            }
                            else if( data.GetType() == typeof( string[] ) )
                            {
                                string[] arr = (string[]) data;
                                // write the length
                                dataWriter.Write( arr != null ? arr.Length : (Int32) (-1) );

                                if( arr != null )
                                {
                                    // convert each string to DateTime then write it in our format
                                    foreach( string s in arr )
                                    {
                                        DateTime dt = DateTime.Parse( s );
                                        dataWriter.Write( dt.ToString( DateTimeFmt ) );
                                    }
                                }
                            }
                            else
                                throw new FileDbException( FileDbException.InvalidDateTimeType, FileDbExceptions.InvalidDataType );
                        }
                        else
                            dataWriter.Write( (Int32) (-1) );
                    }
                    else
                    {
                        if( data == null )
                            dataWriter.Write( String.Empty );  // can't write null
                        else
                        {
                            if( data.GetType() == typeof( DateTime ) )
                            {
                                DateTime dt = (DateTime) data;
                                dataWriter.Write( dt.ToString( DateTimeFmt ) );
                            }
                            else if( data.GetType() == typeof( String ) )
                            {
                                DateTime dt = DateTime.Parse( data.ToString() );
                                dataWriter.Write( dt.ToString( DateTimeFmt ) );
                            }
                            else
                                throw new FileDbException( FileDbException.InvalidDateTimeType, FileDbExceptions.InvalidDataType );
                        }
                    }
                }
                break;

                case DataType.String:
                    if( field.IsArray )
                    {
                        string[] arr = (string[]) data;
                        writeStringArray( dataWriter, arr );
                    }
                    else
                    {
                        if( data == null )
                            dataWriter.Write( String.Empty );  // can't write null
                        else
                            dataWriter.Write( data.ToString() );
                    }
                    break;

                default:
                    // Unknown type
                    throw new FileDbException( string.Format( FileDbException.StrInvalidDataType2,
                        field.Name, field.DataType.ToString(), data.GetType().Name ), FileDbExceptions.InvalidDataType );
            }
        }

        void writeStringArray( BinaryWriter dataWriter, string[] arr )
        {
            // write the length
            dataWriter.Write( arr != null ? arr.Length : (Int32) (-1) );

            if( arr != null )
            {
                foreach( string s in arr )
                {
                    dataWriter.Write( s == null? String.Empty : s );
                }
            }
        }
        
        ///------------------------------------------------------------------------------
        /// <summary>
        /// Private function to perform a binary search
        /// </summary>
        /// <param name="index">file offsets into the .dat file, it must be ordered 
        /// by primary key.</param>
        /// <param name="left">the left most index to start searching from</param>
        /// <param name="right">the right most index to start searching from</param>
        /// <param name="target">the search target we're looking for</param>
        /// <returns>-[insert pos+1] when not found, or the array index+1 
        /// when found. Note that we don't return the normal position, because we 
        /// can't differentiate between -0 and +0.</returns>
        /// 
        Int32 bsearch( List<Int32> lstIndex, Int32 left, Int32 right, object target )
        {
            Int32 middle = 0; // todo: What should this default to?

            while( left <= right )
            {
                middle = (Int32) ((left + right) / 2);

                // Read in the record key at the given offset
                object key = readRecordKey( _dataReader, lstIndex[middle] );

                Int32 nComp = compareKeys( target, key );

                if( left == right && nComp != 0 ) //( key != target ) )
                {
                    if( nComp < 0 )
                        return -(left + 1);
                    else
                        return -(left + 1 + 1);
                }
                else if( nComp == 0 )
                {
                    // Found!
                    return middle + 1;
                }
                else if( nComp < 0 )
                {
                    // Try the left side
                    right = middle - 1;
                }
                else // target > key
                {
                    // Try the right side
                    left = middle + 1;
                }
            }

            // Not found: return the insert position (as negative)
            return -(middle + 1);
        }

        /// <summary>
        /// Helper
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        ///
        Int32 compareKeys( object left, object right )
        {
            Type tleft = left.GetType(),
                 tright = right.GetType();

            if( tleft != tright )
                throw new FileDbException( FileDbException.MismatchedKeyFieldTypes, FileDbExceptions.MismatchedKeyFieldTypes );

            if( tleft == typeof( string ) )
                return string.Compare( left as string, right as string );

            else if( tleft == typeof( Int32 ) )
            {
                Int32 nleft = (Int32) left,
                    nright = (Int32) right;
                return nleft < nright ? -1 : (nleft > nright ? 1 : 0); // todo: check if this is correct
            }
            else
                throw new FileDbException( FileDbException.InvalidKeyFieldType, FileDbExceptions.InvalidKeyFieldType );
        }

        /// <summary>
        /// Private function to read a record from the database
        /// </summary>
        ///
        object[] readRecord( Int32 offset, bool includeIndex )
        {
            Int32 size;
            bool deleted;
            return readRecord( _dataReader, offset, includeIndex, out size, out deleted );
        }

        object[] readRecord( Int32 offset, bool includeIndex, out bool deleted )
        {
            Int32 size;
            return readRecord( _dataReader, offset, includeIndex, out size, out deleted );
        }

        /// <summary>
        /// size does not include the 4 byte record length, but only the total size of the fields
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        /// 
        object[] readRecord( Int32 offset, bool includeIndex, out Int32 size, out bool deleted )
        {
            return readRecord( _dataReader, offset, includeIndex, out size, out deleted );
        }

        object[] readRecord( BinaryReader dataReader, Int32 offset, bool includeIndex, out Int32 size, out bool deleted )
        {
            // Read in the record at the given offset.
            _dataStrm.Seek( offset, SeekOrigin.Begin );

            // Read in the size of the block allocated for the record
            size = dataReader.ReadInt32();

            if( size < 0 )
            {
                deleted = true;
                size= -size;
            }
            else
                deleted = false;

            int numFields = _fields.Count;
            if( includeIndex )
                numFields++;

            object[] record = new object[numFields]; // one extra for the index

            if( size > 0 )
            {
                MemoryStream memStrm = null;

                if( _encryptor != null )
                {
                    byte[] bytes = dataReader.ReadBytes( size );
                    //memStrm = new MemoryStream( size * 2 );
                    bytes = _encryptor.Decrypt( bytes );
                    //bytes = memStrm.ToArray();
                    memStrm = new MemoryStream( bytes );
                    dataReader = new BinaryReader( memStrm );
                }

                // Read in the entire record
                for( Int32 n = 0; n < _fields.Count; n++ )
                {
                    Field field = _fields[n];
                    record[n] = readItem( dataReader, field );
                }
            }
            return record;
        }

        byte[] readRecordRaw( BinaryReader dataReader, Int32 offset, out bool deleted )
        {
            // Read in the record at the given offset.
            _dataStrm.Seek( offset, SeekOrigin.Begin );

            // Read in the size of the block allocated for the record
            int size = dataReader.ReadInt32();

            if( size < 0 )
            {
                deleted = true;
                size= -size;
            }
            else
                deleted = false;

            byte[] bytes = null;

            if( size > 0 )
                bytes = dataReader.ReadBytes( size );

            return bytes;
        }

        /// <summary>
        /// function to read a record KEY from the database.  Note
        /// that this function relies on the fact that they key is ALWAYS the first
        /// item in the database record as stored on disk.
        /// </summary>
        /// <param name="dataReader"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        /// 
        object readRecordKey( BinaryReader dataReader, Int32 offset )
        {
            // Read in the record KEY only

            if( _encryptor == null )
            {
                _dataStrm.Seek( offset + INDEX_RBLOCK_SIZE, SeekOrigin.Begin );
            }
            else // must first decrypt the record
            {
                // Read in the size of the block allocated for the record
                _dataStrm.Seek( offset, SeekOrigin.Begin );
                int size = dataReader.ReadInt32();

                if( size < 0 )
                    size = -size;

                MemoryStream memStrm = null;

                if( _encryptor != null )
                {
                    byte[] bytes = dataReader.ReadBytes( size );
                    //memStrm = new MemoryStream( size * 2 );
                    bytes = _encryptor.Decrypt( bytes );
                    //bytes = memStrm.ToArray();
                    memStrm = new MemoryStream( bytes );
                    dataReader = new BinaryReader( memStrm );
                }
            }
            return readItem( dataReader, _primaryKeyField );
        }

        /// <summary>
        /// Reads a data type from a file.  Note that arrays can only 
        /// consist of other arrays, ints, and strings.
        /// </summary>
        /// <param name="dataReader"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        /// 
        object readItem( BinaryReader dataReader, Field field )
        {
            switch( field.DataType )
            {
                case DataType.Byte:
                    if( field.IsArray )
                    {
                        Int32 elements = dataReader.ReadInt32();
                        Byte[] arr = null;
                        if( elements >= 0 )
                        {
                            arr = dataReader.ReadBytes( elements );
                        }
                        return arr;
                    }
                    else
                        return dataReader.ReadByte();

                case DataType.Int:
                    if( field.IsArray )
                    {
                        Int32 elements = dataReader.ReadInt32();
                        Int32[] arr = null;
                        if( elements >= 0 )
                        {
                            arr = new Int32[elements];
                            for( Int32 i = 0; i < elements; i++ )
                            {
                                arr[i] = dataReader.ReadInt32();
                            }
                        }
                        return arr;
                    }
                    else
                        return dataReader.ReadInt32();

                case DataType.UInt:
                    if( field.IsArray )
                    {
                        Int32 elements = dataReader.ReadInt32();
                        UInt32[] arr = null;
                        if( elements >= 0 )
                        {
                            arr = new UInt32[elements];
                            for( UInt32 i = 0; i < elements; i++ )
                            {
                                arr[i] = dataReader.ReadUInt32();
                            }
                        }
                        return arr;
                    }
                    else
                        return dataReader.ReadUInt32();

                case DataType.Float:
                    if( field.IsArray )
                    {
                        Int32 elements = dataReader.ReadInt32();
                        float[] arr = null;
                        if( elements >= 0 )
                        {
                            arr = new float[elements];
                            for( Int32 i = 0; i < elements; i++ )
                            {
                                arr[i] = dataReader.ReadSingle();
                            }
                        }
                        return arr;
                    }
                    else
                        return dataReader.ReadSingle();

                case DataType.Double:
                    if( field.IsArray )
                    {
                        Int32 elements = dataReader.ReadInt32();
                        double[] arr = null;
                        if( elements >= 0 )
                        {
                            arr = new double[elements];
                            for( Int32 i = 0; i < elements; i++ )
                            {
                                arr[i] = dataReader.ReadDouble();
                            }
                        }
                        return arr;
                    }
                    else
                        return dataReader.ReadDouble();

                case DataType.Bool:
                    if( field.IsArray )
                    {
                        Int32 elements = dataReader.ReadInt32();
                        bool[] arr = null;
                        if( elements >= 0 )
                        {
                            arr = new bool[elements];
                            for( Int32 i = 0; i < elements; i++ )
                            {
                                arr[i] = dataReader.ReadByte() == 1;
                            }
                        }
                        return arr;
                    }
                    else
                        return (dataReader.ReadByte() == 1);

                case DataType.DateTime:
                    if( field.IsArray )
                    {
                        Int32 elements = dataReader.ReadInt32();
                        DateTime[] arr = null;
                        if( elements >= 0 )
                        {
                            arr = new DateTime[elements];
                            for( Int32 i = 0; i < elements; i++ )
                            {
                                string s = dataReader.ReadString();
                                arr[i] = DateTime.Parse( s );
                            }
                        }
                        return arr;
                    }
                    else
                    {
                        string s = dataReader.ReadString();
                        if( s.Length == 0 )
                            return DateTime.MinValue;
                        else
                            return DateTime.Parse( s );
                    }

                case DataType.String:
                    if( field.IsArray )
                    {
                        Int32 elements = dataReader.ReadInt32();
                        string[] arr = null;
                        if( elements >= 0 )
                        {
                            arr = new string[elements];
                            for( Int32 i = 0; i < elements; i++ )
                            {
                                arr[i] = dataReader.ReadString();
                            }
                        }
                        return arr;
                    }
                    else
                        return dataReader.ReadString();

                default:
                    // Error in type
                    throw new FileDbException( string.Format( FileDbException.StrInvalidDataType, (Int32) field.DataType ), FileDbExceptions.InvalidDataType );
            }
        }

        void writeDbHeader( BinaryWriter writer )
        {
            writer.Seek( 0, SeekOrigin.Begin );

            // Write the signature
            writer.Write( SIGNATURE );

            // Write the version
            writer.Write( VERSION_MAJOR );
            writer.Write( VERSION_MINOR );
        }
                
        /// <summary>
        /// Write the database schema and other meta information.
        /// </summary>
        /// <param name="writer"></param>
        ///
        void writeSchema( BinaryWriter writer )
        {
            writer.Seek( SCHEMA_OFFSET, SeekOrigin.Begin );

            writer.Write( _numRecords );
            writer.Write( _numDeleted );
            writer.Write( _indexStartPos );

            // Write the schema
            //
            // Schema format:
            //   [primary key field name]
            //   [number of fields]
            //     [field 1: name]
            //     [field 1: type]
            //     [field 1: flags]
            //     <field 1: possible autoinc value>
            //     [field 1: possible comment]
            //     ...
            //     [field n: name]
            //     [field n: type]
            //     [field n: flags]
            //     <field n: possible autoinc value>
            //     [field n: possible comment]
            //
            // For auto-incrementing fields, there is an extra Int32 specifying
            // the last value used in the last record added.

            writer.Write( _primaryKey );
            writer.Write( _fields.Count );

            // always write the key entry first
            // brettg REVIEW: this will complicate adding a new field later which is a primary key

            if( !string.IsNullOrEmpty( _primaryKey ) )
            {
                Debug.Assert( _primaryKeyField != null );
                Debug.Assert( string.Compare( _primaryKeyField.Name, _primaryKey, StringComparison.CurrentCultureIgnoreCase ) == 0 );

                writeField( writer, _primaryKeyField );
            }

            // Write out all of the other entries
            foreach( Field field in _fields )
            {
                if( field != _primaryKeyField )
                    writeField( writer, field );
            }

            _dataStartPos = (Int32) _dataStrm.Position;
        }

        private void writeMetaData( BinaryWriter dataWriter )
        {
            if( _metaData == null )
            {
                // don't write anything to indicate no metadata
                return;
            }

            Type metaType = _metaData.GetType();

            if( metaType == typeof( String ) )
            {
                dataWriter.Write( (Int32) DataType.String );
                dataWriter.Write( (String) _metaData );
            }
            else if( metaType == typeof( Byte[] ) )
            {
                dataWriter.Write( (Int32) DataType.Byte );
                Byte[] arr = (Byte[]) _metaData;
                dataWriter.Write( (Int32) arr.Length );
                dataWriter.Write( arr );
            }
        }

        private void readMetaData( BinaryReader reader )
        {
            _metaData = null;

            try
            {
                if( _ver_major >= 2 )
                {
                    DataType dataType;

                    try
                    {
                        dataType = (DataType) reader.ReadInt32();
                    }
                    catch( EndOfStreamException ex )
                    {
                        // just means there was no metadata
                        return;
                    }

                    switch( dataType )
                    {
                        case DataType.String:
                            _metaData = reader.ReadString();
                            break;

                        case DataType.Byte:
                            // read the length
                            Int32 len = reader.ReadInt32();
                            _metaData = reader.ReadBytes( len );
                            break;
                    }
                }
            }
            catch( Exception ex )
            {
                // brettg TODO: we need a way to report this error without throwing an exception
                // its best to continue on since this is the last thing we read when opening a db
                // and its better to get the other data rather than none at all
            }
        }

        /// <summary>
        /// Helper
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="field"></param>
        ///
        void writeField( BinaryWriter writer, Field field )
        {
            writer.Write( field.Name );
            writer.Write( (short) field.DataType );

            Int32 flags=0;
            if( field.IsAutoInc )
                flags |= AutoIncField;
            if( field.IsArray )
                flags |= ArrayField;
            writer.Write( flags );

            if( field.IsAutoInc )
            {
                writer.Write( field.AutoIncStart );
                writer.Write( field.CurAutoIncVal );
            }
            // ver 2.0
            writer.Write( field.Comment == null ? string.Empty : field.Comment );
        }

        void orderBy( object[][] result, string[] fieldList, string[] orderByList )
        {
            List<Field> sortFields = new List<Field>( orderByList.Length );
            List<bool> sortDirLst = new List<bool>( orderByList.Length );
            List<bool> caseLst = new List<bool>( orderByList.Length );

            GetOrderByLists( _fields, fieldList, orderByList, sortFields, sortDirLst, caseLst );

            Array.Sort( result, new RowComparer( sortFields, sortDirLst, caseLst ) );
        }

        internal static void GetOrderByLists( Fields fields, string[] fieldList, string[] orderByList, List<Field> sortFields,
                                List<bool> sortDirLst, List<bool> caseLst )
        {
            foreach( string s in orderByList )
            {
                string orderByField = s;

                // Do we want reverse or case-insensitive sort?

                bool rev_sort = false,
                     caseInsensitive = false;

                int ndx = 0;
                if( orderByField.Length > 0 )
                {
                    rev_sort = orderByField[0] == '!';
                    caseInsensitive = orderByField[0] == '~';

                    if( orderByField.Length > 1 )
                    {
                        if( !rev_sort )
                            rev_sort = orderByField[1] == '!';

                        if( !caseInsensitive )
                            caseInsensitive = orderByField[1] == '~';
                    }
                }

                // Remove the control code from the order by field
                if( rev_sort ) ndx++;
                if( caseInsensitive ) ndx++;

                if( ndx > 0 )
                    orderByField = orderByField.Substring( ndx );

                sortDirLst.Add( rev_sort );
                caseLst.Add( caseInsensitive );

                string origOrderByName = orderByField;
                //orderByField = orderByField.ToUpper();

                // Check the orderby field name
                if( !fields.ContainsKey( orderByField ) )
                {
                    throw new Exception( string.Format( "Invalid OrderBy field name - {0}", origOrderByName ) );
                }

                Field sortField = fields[orderByField];
                if( sortField.IsArray )
                {
                    throw new Exception( "Cannot OrderBy on an array field." );
                }

                if( fieldList != null )
                {
                    // if fieldList is not null, it means its not all fields (subset) and so
                    // we must adjust the field ordinals to match the requested field columns
                    sortField = new Field( sortField.Name, sortField.DataType, -1 );
                    for( int n = 0; n < fieldList.Length; n++ )
                    {
                        if( string.Compare( fieldList[n], sortField.Name, StringComparison.OrdinalIgnoreCase ) == 0 )
                        {
                            sortField.Ordinal = n;
                            break;
                        }
                    }
                    if( sortField.Ordinal == -1 )
                        throw new Exception( string.Format( "Invalid OrderBy field name - {0}", sortField ) );
                }

                sortFields.Add( sortField );
            }
        }

        void readSchema()
        {
            readSchema( _dataReader );
        }

        void readSchema( BinaryReader reader )
        {
            _dataStrm.Seek( SCHEMA_OFFSET, SeekOrigin.Begin );

            // Read the database statistics
            //
            // Statistics format:
            //    [number of valid records: Int32]
            //    [number of (unclean) deleted records: Int32]

            _numRecords = reader.ReadInt32();
            _numDeleted = reader.ReadInt32();
            _indexStartPos = reader.ReadInt32();

            // Read the schema
            //
            // Schema format:
            //   [primary key field name]
            //   [number of fields]
            //     [field 1: name]
            //     [field 1: type]
            //     [field 1: flags]
            //     [field 1: possible comment]
            //     <field 1: possible autoinc value>
            //     ...
            //     [field n: name]
            //     [field n: type]
            //     [field n: flags]
            //     [field n: possible comment]
            //     <field n: possible autoinc value>
            //
            // For auto-incrementing fields, there is an extra Int32 specifying
            // the last value used in the last record added.

            _primaryKey = reader.ReadString();
            Int32 field_count = reader.ReadInt32();

            _fields = new Fields();

            for( Int32 i=0; i < field_count; i++ )
            {
                // Read the fields in
                string name = reader.ReadString();
                DataType type = (DataType) reader.ReadInt16();
                Field field = new Field( name, type, i );
                _fields.Add( field );

                if( string.Compare( _primaryKey, name, StringComparison.CurrentCultureIgnoreCase ) == 0 )
                {
                    field.IsPrimaryKey = true;
                    _primaryKeyField = field;
                }

                Int32 flags = reader.ReadInt32();

                if( (flags & AutoIncField) == AutoIncField )
                {
                    field.AutoIncStart = reader.ReadInt32();
                    field.CurAutoIncVal = reader.ReadInt32();
                }

                if( (flags & ArrayField) == ArrayField )
                    field.IsArray = true;

                if( _ver_major >= 2 )
                    field.Comment = reader.ReadString();
                else
                    field.Comment = String.Empty;

            }

            // Save where the index starts
            _dataStartPos = (Int32) _dataStrm.Position;
        }

        void writeNumRecords( BinaryWriter writer )
        {
            writer.Seek( NUM_RECS_OFFSET, SeekOrigin.Begin );
            writer.Write( _numRecords );
        }
        
        void writeIndexStart( BinaryWriter writer )
        {
            writer.Seek( INDEX_OFFSET, SeekOrigin.Begin );
            writer.Write( _indexStartPos );
        }

        #endregion private methods

        ///////////////////////////////////////////////////////////////////////
        #region RowComparer
        //=====================================================================
        class RowComparer : IComparer
        {
            List<Field> _fieldLst;
            List<bool> _sortDirLst,
                       _caseLst;

            internal RowComparer( List<Field> fieldLst, List<bool> sortDirLst, List<bool> caseLst )
            {
                _fieldLst = fieldLst;
                _caseLst = caseLst;
                _sortDirLst = sortDirLst;
            }

            // Calls CaseInsensitiveComparer.Compare with the parameters reversed

            Int32 IComparer.Compare( Object x, Object y )
            {
                Int32 nRet = 0;
                object v1, v2;
                object[] row1 = x as object[],
                         row2 = y as object[];

                if( row1 == null || row2 == null )
                    return 0;

                for( int n=0; n < _fieldLst.Count; n++ )
                {
                    Field field = _fieldLst[n];
                    bool reverseSort = _sortDirLst[n];
                    bool caseInsensitive = _caseLst[n];

                    v1 = row1[field.Ordinal];
                    v2 = row2[field.Ordinal];

                    int compVal = CompareVals( v1, v2, field.DataType, caseInsensitive );

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

        internal static int CompareVals( object v1, object v2, DataType dataType, bool caseInsensitive )
        {
            int compVal = 0;

            switch( dataType )
            {
                case DataType.Byte:
                {
                    Byte b1 = (Byte) v1,
                         b2 = (Byte) v2;
                    compVal = b1 < b2 ? -1 : (b1 > b2 ? 1 : 0);
                }
                break;

                case DataType.Int:
                {
                    Int32 i1 = (Int32) v1,
                        i2 = (Int32) v2;
                    compVal = i1 < i2 ? -1 : (i1 > i2 ? 1 : 0);
                }
                break;

                case DataType.UInt:
                {
                    UInt32 i1 = (UInt32) v1,
                           i2 = (UInt32) v2;
                    compVal = i1 < i2 ? -1 : (i1 > i2 ? 1 : 0);
                }
                break;

                case DataType.Bool:
                {
                    Byte b1 = (Byte) v1,
                         b2 = (Byte) v2;
                    compVal = b1 < b2 ? -1 : (b1 > b2 ? 1 : 0);
                }
                break;

                case DataType.Float:
                {
                    float f1 = (float) v1,
                          f2 = (float) v2;
                    compVal = f1 < f2 ? -1 : (f1 > f2 ? 1 : 0);
                }
                break;

                case DataType.Double:
                {
                    double d1 = (double) v1,
                           d2 = (double) v2;
                    compVal = d1 < d2 ? -1 : (d1 > d2 ? 1 : 0);
                }
                break;

                case DataType.DateTime:
                {
                    DateTime dt1, dt2;

                    if( v1.GetType() == typeof( String ) )
                    {
                        dt1 = DateTime.Parse( v1.ToString() );
                        dt2 = DateTime.Parse( v2.ToString() );
                    }
                    else if( v1.GetType() == typeof( DateTime ) )
                    {
                        Debug.Assert( v1.GetType() == typeof( DateTime ) );
                        dt1 = (DateTime) v1;
                        dt2 = (DateTime) v2;
                    }
                    else
                        throw new FileDbException( FileDbException.InvalidDateTimeType, FileDbExceptions.InvalidDataType );

                    compVal = dt1 < dt2 ? -1 : (dt1 > dt2 ? 1 : 0);
                }
                break;

                case DataType.String:
                {
                    string s1 = (string) v1,
                           s2 = (string) v2;

                    // TODO: allow for culture sort rules
                    if( caseInsensitive )
                        compVal = string.Compare( s1, s2, StringComparison.CurrentCultureIgnoreCase );
                    else
                        compVal = string.Compare( s1, s2, StringComparison.CurrentCulture );
                }
                break;

                default:
                Debug.Assert( false );
                break;
            }

            return compVal;
        }
        #endregion RowComparer
    }

}
