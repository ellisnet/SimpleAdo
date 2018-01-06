/* Copyright 2017 Ellisnet - Jeremy Ellis (jeremy@ellisnet.com)

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

using DbProvider = SQLitePCL;

namespace SimpleAdo.Sqlite {

    /// <summary> A sqlite connection. This class cannot be inherited. </summary>
	public sealed class SqliteConnection : IDbConnection {

        /// <summary> Event queue for all listeners interested in StateChange events. </summary>
        public event StateChangeEventHandler StateChange;

        /// <summary> The database. </summary>
	    private SqliteDatabaseHandle _database;

        /// <summary> Gets the database. </summary>
        /// <value> The database. </value>
	    internal SqliteDatabaseHandle Database
	    {
	        get
	        {
	            VerifyNotDisposed();
	            return _database;
	        }
	    }

        // ReSharper disable once RedundantDefaultMemberInitializer
        /// <summary> True to using default. </summary>
        private bool _usingDefault = false;
        /// <summary> True to first time open. </summary>
	    private bool _firstTimeOpen = true;
        /// <summary> True if this object is disposed. </summary>
	    private bool _isDisposed;
        /// <summary> The transactions. </summary>
	    private readonly Stack<SqliteTransaction> _transactions;
        /// <summary> The handle. </summary>
	    private GCHandle _handle;
        /// <summary> The statement completed. </summary>
	    private StatementCompletedEventHandler _statementCompleted;

        /// <summary> The database open locker. </summary>
        private readonly object _databaseOpenLocker = new object();
        /// <summary> The static locker. </summary>
	    private static readonly object staticLocker = new object();

        /// <summary> ReSharper disable InconsistentNaming. </summary>
	    private static readonly DbProvider.delegate_profile _profileCallback = ProfileCallback;

        /// <summary> The default database. </summary>
        private static SqliteDatabaseHandle _defaultDatabase;

        /// <summary> Gets or sets the default database. </summary>
        /// <value> The default database. </value>
	    internal static SqliteDatabaseHandle DefaultDatabase => _defaultDatabase;

        /// <summary> True to default database first time open. </summary>
	    private static bool _defaultDatabaseFirstTimeOpen = true;

        // ReSharper disable once RedundantDefaultMemberInitializer
        /// <summary> The crypt engine. </summary>
        internal IObjectCryptEngine _cryptEngine = null;

	    // ReSharper restore InconsistentNaming

        /// <summary>
        /// Opening read-only throws "EntryPointNotFoundException: sqlite3_db_readonly" on Android API 15
        /// and below (JellyBean is API 16)
        /// </summary>
        /// <value> True if allow open readonly, false if not. </value>
        public bool AllowOpenReadOnly { get; set; } = true;

        /// <summary> State of the connection. </summary>
	    ConnectionState _connectionState;

        /// <summary> Gets the state. </summary>
        /// <value> The state. </value>
	    public ConnectionState State
	    {
	        get
	        {
                VerifyNotDisposed();
	            if (_connectionState == ConnectionState.Open && 
                    (!_database.IsDatabaseOpen) &&
	                (!_database.IsDatabaseInMaintenanceMode))
	            {
	                //database should be open but it's not
	                _database.CheckOpenDatabase();
	            }
	            if (_connectionState == ConnectionState.Open &&
	                (!_database.IsDatabaseOpen) &&
	                (!_database.IsDatabaseInMaintenanceMode))
	            {
	                _connectionState = ConnectionState.Broken;
	            }
	            return _connectionState;
	        }
	    }

        /// <summary> The connection string. </summary>
        private string _connectionString;

        /// <summary> Gets or sets the connection string. </summary>
        /// <exception cref="InvalidOperationException"> Thrown when the requested operation is invalid. </exception>
        /// <value> The connection string. </value>
	    public string ConnectionString
	    {
	        get => _connectionString ?? (_usingDefault ? (_connectionString = _defaultDatabase?.ConnectionString) : null);
	        set
	        {
	            _database = _database ?? (_usingDefault ? _defaultDatabase : null);
	            if (_database != null)
	            {
	                throw new InvalidOperationException($"The '{nameof(ConnectionString)}' property cannot be set after the database has been accessed.");
	            }
	            value = (String.IsNullOrWhiteSpace(value)) ? null : value.Trim();
	            if (_connectionString != null && value == null)
	            {
	                throw new InvalidOperationException($"The '{nameof(ConnectionString)}' property to a blank or null value.");
	            }
	            _connectionString = value;
	        }
	    }

        /// <summary> The database open flags. </summary>
	    private SqliteOpenFlags _databaseOpenFlags = SqliteOpenFlags.ReadWrite;

        /// <summary> Gets or sets the database open flags. </summary>
        /// <exception cref="InvalidOperationException"> Thrown when the requested operation is invalid. </exception>
        /// <value> The database open flags. </value>
	    public SqliteOpenFlags DatabaseOpenFlags
	    {
	        get => _databaseOpenFlags;
	        set
	        {
	            if (_database != null || (_usingDefault && _defaultDatabase != null))
	            {
	                throw new InvalidOperationException($"The '{nameof(DatabaseOpenFlags)}' property cannot be set after the database has been accessed.");
	            }
	            _databaseOpenFlags = value;
	        }
	    }

        /// <summary> Default constructor. </summary>
        public SqliteConnection() {
            if (_defaultDatabase != null)
            {
                lock (staticLocker)
                {
                    _database = _defaultDatabase;
                    _usingDefault = true;
                }
            }
            _transactions = new Stack<SqliteTransaction>();
		}

        /// <summary> Constructor. </summary>
        /// <param name="cryptEngine"> The crypt engine. </param>
        public SqliteConnection(IObjectCryptEngine cryptEngine) : this() {
            _cryptEngine = cryptEngine;
            _transactions = new Stack<SqliteTransaction>();
        }

        /// <summary> Constructor. </summary>
        /// <exception cref="InvalidOperationException"> Thrown when the requested operation is invalid. </exception>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <exception cref="ArgumentOutOfRangeException"> Thrown when one or more arguments are outside
        ///     the required range. </exception>
        /// <param name="connectionString"> The connection string. </param>
        /// <param name="connectionStringIsFilePath"> (Optional) True if connection string is file path. </param>
        /// <param name="openFlags"> (Optional) The open flags. </param>
        /// <param name="setAsDefaultDatabase"> (Optional) True to set as default database. </param>
	    public SqliteConnection(string connectionString, 
            bool connectionStringIsFilePath = false,
            SqliteOpenFlags openFlags = SqliteOpenFlags.ReadWrite,
            bool setAsDefaultDatabase = false)
	    {
	        if (setAsDefaultDatabase && _defaultDatabase != null)
	        {
	            throw new InvalidOperationException("Unable to set the database connection as default - there is already a default database specified.");
	        }

	        connectionString = connectionString?.Trim() ?? throw new ArgumentNullException(nameof(connectionString));
            if (connectionString == "") { throw new ArgumentOutOfRangeException(connectionString);}

	        string databaseFilePath;
	        SqliteConnectionStringBuilder csb;
	        if (connectionStringIsFilePath)
	        {
	            databaseFilePath = connectionString;
	            csb = new SqliteConnectionStringBuilder
	            {
	                BusyTimeout = 100,
	                DatabaseFilePath = databaseFilePath,
	                JournalMode = openFlags.HasFlag(SqliteOpenFlags.ReadWrite)
	                    ? SqliteJournalModeEnum.Wal
	                    : SqliteJournalModeEnum.Default,
                    StoreDateTimeAsTicks = true
	            };
                ConnectionString = csb.ConnectionString;
	        }
	        else
	        {
	            ConnectionString = connectionString;
	            csb = new SqliteConnectionStringBuilder {ConnectionString = connectionString};           
                databaseFilePath = csb.DatabaseFilePath;
	        }

	        csb.ReadOnly &= AllowOpenReadOnly;
	        //Want to add Create open flag, if database isn't ReadOnly or FailIfMissing
            _databaseOpenFlags = csb.ReadOnly
	            ? SqliteOpenFlags.ReadOnly
	            : ((csb.FailIfMissing || openFlags != SqliteOpenFlags.ReadWrite)
	                ? openFlags
	                : (openFlags | SqliteOpenFlags.Create));

            _database = new SqliteDatabaseHandle(databaseFilePath, _databaseOpenFlags, ConnectionString, csb.StoreDateTimeAsTicks);

	        if (setAsDefaultDatabase)
	        {
	            lock (staticLocker)
	            {
	                if (_defaultDatabase != null)
	                {
	                    throw new InvalidOperationException("Unable to set the database connection as default - there is already a default database specified.");
                    }
	                _defaultDatabase = _database;
	                _usingDefault = true;
	            }
	        }

            _transactions = new Stack<SqliteTransaction>();
        }

        /// <summary> Constructor. </summary>
        /// <param name="connectionString"> The connection string. </param>
        /// <param name="cryptEngine"> The crypt engine. </param>
        /// <param name="connectionStringIsFilePath"> (Optional) True if connection string is file path. </param>
        /// <param name="openFlags"> (Optional) The open flags. </param>
        /// <param name="setAsDefaultDatabase"> (Optional) True to set as default database. </param>
	    public SqliteConnection(string connectionString,
            IObjectCryptEngine cryptEngine,
	        bool connectionStringIsFilePath = false,
	        SqliteOpenFlags openFlags = SqliteOpenFlags.ReadWrite,
	        bool setAsDefaultDatabase = false)
            : this(connectionString, connectionStringIsFilePath, openFlags, setAsDefaultDatabase)
	    {
	        _cryptEngine = cryptEngine;
        }

        /// <summary> Begins a transaction. </summary>
        /// <returns> An IDbTransaction. </returns>
		public IDbTransaction BeginTransaction() {
            return BeginTransaction(IsolationLevel.Unspecified);
        }

        /// <summary> Begins a transaction. </summary>
        /// <exception cref="ArgumentOutOfRangeException"> Thrown when one or more arguments are outside
        ///     the required range. </exception>
        /// <param name="isolationLevel"> The il. </param>
        /// <returns> An IDbTransaction. </returns>
		public IDbTransaction BeginTransaction(IsolationLevel isolationLevel) {
			if (isolationLevel == IsolationLevel.Unspecified)
			{
			    isolationLevel = IsolationLevel.Serializable;
			}
		    if (isolationLevel != IsolationLevel.Serializable && isolationLevel != IsolationLevel.ReadCommitted)
		    {
		        throw new ArgumentOutOfRangeException(nameof(isolationLevel), isolationLevel, "Specified IsolationLevel value is not supported.");
		    }

		    if (_transactions.Count == 0)
		    {
		        this.ExecuteNonQuery(isolationLevel == IsolationLevel.Serializable ? "BEGIN IMMEDIATE" : "BEGIN");
		    }
		    _transactions.Push(new SqliteTransaction(this, isolationLevel));
			return CurrentTransaction;
		}

        /// <summary> Closes this object. </summary>
        /// <exception cref="InvalidOperationException"> Thrown when the requested operation is invalid. </exception>
		public void Close()
        {
            VerifyNotDisposed();
            if (State != ConnectionState.Open)
            {
                throw new InvalidOperationException("Cannot Close when State is {0}.".FormatInvariant(State));
            }

            if ((!_usingDefault) && State == ConnectionState.Open)
		    {
		        if (_database.IsDatabaseInMaintenanceMode)
		        {
		            _database.EndMaintenanceMode();
		        }
		        else
		        {
		            _database.CloseDatabase();
		        }
		    }
		    SetState(ConnectionState.Closed);
        }

        /// <summary> Safe close. </summary>
	    public void SafeClose()
        {
	        if ((!_isDisposed) && State == ConnectionState.Open)
	        {
	            Close();
	        }
        }

        /// <summary> Change database. </summary>
        /// <exception cref="NotSupportedException"> Thrown when the requested operation is not supported. </exception>
        /// <param name="databaseName"> Name of the database. </param>
		public void ChangeDatabase(string databaseName) {
			throw new NotSupportedException();
		}

        /// <summary> Queries if a given safe open. </summary>
	    public void SafeOpen()
        {
	        if (State == ConnectionState.Closed) {
	            Open();
	        }
	    }

        /// <summary> Gets database schema version - leaves the connection state in the state it was found (open/closed). </summary>
        /// <returns> The database schema version. </returns>
	    public long GetDatabaseSchemaVersion()
	    {
	        //Leave the connection in the state we found it - open or closed
	        bool wasClosed = (State == ConnectionState.Closed);
            SafeOpen();
	        long result = (long) this.ExecuteScalar("PRAGMA user_version;", Database.IsDatabaseInMaintenanceMode);
            if (wasClosed) { SafeClose();}
	        return result;
	    }

        /// <summary> Sets database schema version - leaves the connection state in the state it was found (open/closed). </summary>
        /// <param name="version"> The version. </param>
	    public void SetDatabaseSchemaVersion(long version)
	    {
            //Leave the connection in the state we found it - open or closed
	        bool wasClosed = (State == ConnectionState.Closed);
            SafeOpen();
	        _database.BeginMaintenanceMode();
            this.ExecuteNonQuery($"PRAGMA user_version = {version};", true);
	        if (wasClosed) { SafeClose(); }
            _database.EndMaintenanceMode();
	    }

        /// <summary> Queries if a given table exists. </summary>
        /// <param name="tableName"> Name of the table. </param>
        /// <returns> True if it succeeds, false if it fails. </returns>
	    public bool TableExists(string tableName)
	    {
	        tableName = CheckTableName(tableName);
	        string sql = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{tableName}';";
            SafeOpen();
	        string verifyTableName = (string)this.ExecuteScalar(sql, Database.IsDatabaseInMaintenanceMode);
	        return verifyTableName == tableName;
	    }

        /// <summary> Gets table column list. </summary>
        /// <param name="tableName"> Name of the table. </param>
        /// <returns> An array of i column information. </returns>
	    public IColumnInfo[] GetTableColumnList(string tableName)
	    {
	        tableName = CheckTableName(tableName);
	        List<SqliteColumnInfo> columnList = null;
	        if (TableExists(tableName))
	        {
                columnList = new List<SqliteColumnInfo>();
	            string sql = $"pragma table_info({tableName});";
	            using (var cmd = new SqliteCommand(sql, this, Database.IsDatabaseInMaintenanceMode))
	            {
	                using (var reader = new SqliteDataReader(cmd))
	                {
	                    while (reader.Read())
	                    {
                            columnList.Add(new SqliteColumnInfo
                            {
                                ColumnId = reader.GetInt32("cid"),
                                DeclaredTypeName = reader.GetString("type"),
                                DefaultValue = reader["dflt_value"],
                                IsNotNull = reader.GetBoolean("notnull"),
                                IsPrimaryKey = reader.GetBoolean("pk"),
                                Name = reader.GetString("name")
                            });
	                    }
	                }
	            }
            }
	        // ReSharper disable once CoVariantArrayConversion
	        return columnList?.ToArray();
	    }

        /// <summary> Queries if a given first time open. </summary>
        /// <exception cref="SqliteException"> Thrown when a Sqlite error condition occurs. </exception>
        /// <param name="database"> The database. </param>
        /// <param name="connectionStringBuilder"> The connection string builder. </param>
        /// <returns> True if it succeeds, false if it fails. </returns>
	    private bool FirstTimeOpen(SqliteDatabaseHandle database, SqliteConnectionStringBuilder connectionStringBuilder)
	    {
	        bool openSuccess = database.CheckOpenDatabase();
	        if (openSuccess)
	        {
	            database.BeginMaintenanceMode();

                //Not dealing with passwords at this time:
                //	if (!string.IsNullOrEmpty(connectionStringBuilder.Password))
                //	{
                //	    byte[] passwordBytes = Encoding.UTF8.GetBytes(connectionStringBuilder.Password);
                //	    _lockContext.sqlite3_key(m_db, passwordBytes, passwordBytes.Length).ThrowOnError();
                //	}

                if (AllowOpenReadOnly)
	            {
	                int isReadOnly = database.Context.sqlite3_db_readonly(database, "main", true);
	                if (isReadOnly == 1 && !connectionStringBuilder.ReadOnly)
	                {
	                    throw new SqliteException(SqliteResultCode.ReadOnly);
	                }
	            }

	            if (connectionStringBuilder.CacheSize != 0)
	            {
	                this.ExecuteNonQuery("pragma cache_size={0}".FormatInvariant(connectionStringBuilder.CacheSize), true);
	            }

	            if (connectionStringBuilder.PageSize != 0)
	            {
	                this.ExecuteNonQuery("pragma page_size={0}".FormatInvariant(connectionStringBuilder.PageSize), true);
	            }

	            if (connectionStringBuilder.ContainsKey(SqliteConnectionStringBuilder.MmapSizeKey))
	            {
	                this.ExecuteNonQuery("pragma mmap_size={0}".FormatInvariant(connectionStringBuilder.MmapSize), true);
	            }

	            if (connectionStringBuilder.ForeignKeys)
	            {
	                this.ExecuteNonQuery("pragma foreign_keys = on", true);
	            }

	            if (connectionStringBuilder.JournalMode != SqliteJournalModeEnum.Default)
	            {
	                this.ExecuteNonQuery("pragma journal_mode={0}".FormatInvariant(connectionStringBuilder.JournalMode), true);
	            }

	            if (connectionStringBuilder.ContainsKey(SqliteConnectionStringBuilder.SynchronousKey))
	            {
	                this.ExecuteNonQuery("pragma synchronous={0}".FormatInvariant(connectionStringBuilder.SyncMode), true);
	            }

	            if (connectionStringBuilder.TempStore != SqliteTemporaryStore.Default)
	            {
	                this.ExecuteNonQuery("pragma temp_store={0}".FormatInvariant(connectionStringBuilder.TempStore), true);
	            }

	            if (connectionStringBuilder.ContainsKey(SqliteConnectionStringBuilder.BusyTimeoutKey))
	            {
	                database.Context.sqlite3_busy_timeout(database, connectionStringBuilder.BusyTimeout, true);
	            }

	            if (_statementCompleted != null)
	            {
	                SetProfileCallback(_profileCallback);
	            }

	            database.EndMaintenanceMode();

                SetState(ConnectionState.Open);

	            _firstTimeOpen = false;
	        }
            else
	        {
	            SetState(ConnectionState.Broken);
                Utility.Dispose(ref database);
            }

	        return openSuccess;
	    }

        /// <summary> Opens this object. </summary>
        /// <exception cref="InvalidOperationException"> Thrown when the requested operation is invalid. </exception>
		public void Open()
        {
			VerifyNotDisposed();
			if (State != ConnectionState.Closed)
			{
			    throw new InvalidOperationException("Cannot Open when State is {0}.".FormatInvariant(State));
			}

		    SqliteConnectionStringBuilder csb = null;

		    if (_database == null)
		    {
		        if (_defaultDatabase != null && (ConnectionString == null || ConnectionString == _defaultDatabase.ConnectionString))
		        {
		            lock (staticLocker)
		            {
		                ConnectionString = _defaultDatabase.ConnectionString;
                        _database = _defaultDatabase;
		                _usingDefault = true;
		            }
		        }
		        else
		        {
		            if (ConnectionString == null) { throw new InvalidOperationException("Cannot open a connection to a database with a missing connection string.");}
		            csb = new SqliteConnectionStringBuilder { ConnectionString = ConnectionString };
		            csb.ReadOnly &= AllowOpenReadOnly;

                    //Want to add Create open flag, if database isn't ReadOnly or FailIfMissing
		            _databaseOpenFlags = csb.ReadOnly
		                ? SqliteOpenFlags.ReadOnly
		                : ((csb.FailIfMissing || _databaseOpenFlags != SqliteOpenFlags.ReadWrite) 
                            ? _databaseOpenFlags 
                            : (_databaseOpenFlags | SqliteOpenFlags.Create));

		            if (String.IsNullOrWhiteSpace(csb.DatabaseFilePath))
		            {
		                throw new InvalidOperationException("The file path to the database has not been set.");
		            }
                    _database = new SqliteDatabaseHandle(csb.DatabaseFilePath, _databaseOpenFlags, ConnectionString, csb.StoreDateTimeAsTicks);
		        }
            }

		    SetState(ConnectionState.Connecting);

		    _firstTimeOpen = _firstTimeOpen && ((!_usingDefault) || _defaultDatabaseFirstTimeOpen);

		    // ReSharper disable once RedundantAssignment
		    bool openSuccess = false;
		    if (_firstTimeOpen)
		    {
		        csb = csb ?? new SqliteConnectionStringBuilder {ConnectionString = ConnectionString};
		        csb.ReadOnly &= AllowOpenReadOnly;

		        if (_usingDefault)
		        {
		            lock (staticLocker)
		            {
		                if (_defaultDatabaseFirstTimeOpen)
		                {
		                    // ReSharper disable once RedundantAssignment
		                    openSuccess = FirstTimeOpen(_database, csb);
		                    _defaultDatabaseFirstTimeOpen = false;
		                }
		            }
		        }
		        else
		        {
		            lock (_databaseOpenLocker)
		            {
		                if (_firstTimeOpen)
		                {
		                    // ReSharper disable once RedundantAssignment
		                    openSuccess = FirstTimeOpen(_database, csb);
		                }
		            }
		        }
		    }
		    else
		    {
		        openSuccess = _database.CheckOpenDatabase();
		        SetState(openSuccess ? ConnectionState.Open : ConnectionState.Broken);
            }		    
        }

        /// <summary> Gets or sets the data source. </summary>
        /// <value> The data source. </value>
	    public string DataSource => _database?.DatabaseFilePath;

        /// <summary> Gets or sets the server version. </summary>
        /// <value> The server version. </value>
	    public string ServerVersion => throw new NotSupportedException();

        /// <summary> Creates a command. </summary>
        /// <param name="forMaintenance"> (Optional) True to for maintenance. </param>
        /// <returns> The new command. </returns>
	    public IDbCommand CreateCommand(bool forMaintenance = false) {
			return new SqliteCommand(this, forMaintenance);
		}

        /// <summary> Gets or sets the connection timeout. </summary>
        /// <value> The connection timeout. </value>
		public int ConnectionTimeout => throw new NotSupportedException();

        /// <summary>
        /// Backs up the database, using the specified database connection as the destination.
        /// </summary>
        /// <exception cref="InvalidOperationException"> Thrown when the requested operation is invalid. </exception>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <exception cref="ArgumentException"> Thrown when one or more arguments have unsupported or
        ///     illegal values. </exception>
        /// <exception cref="SqliteException"> Thrown when a Sqlite error condition occurs. </exception>
        /// <param name="destination"> The destination database connection. </param>
        /// <param name="destinationName"> The destination database name (usually <c>"main"</c>). </param>
        /// <param name="sourceName"> The source database name (usually <c>"main"</c>). </param>
        /// <param name="pages"> The number of pages to copy, or negative to copy all remaining pages. </param>
        /// <param name="callback"> The method to invoke between each step of the backup process.  This
        ///     parameter may be <c>null</c> (i.e., no callbacks will be performed). </param>
        /// <param name="retryMilliseconds"> The number of milliseconds to sleep after encountering a
        ///     locking error during the backup process.  A value less than zero means that no sleep
        ///     should be performed. </param>
		public void BackupDatabase(SqliteConnection destination, string destinationName, string sourceName, int pages, SqliteBackupCallback callback, int retryMilliseconds) {
			VerifyNotDisposed();
			if (_connectionState != ConnectionState.Open)
			{
			    throw new InvalidOperationException("Source database is not open.");
			}
		    if (destination == null)
		    {
		        throw new ArgumentNullException(nameof(destination));
		    }
		    if (destination._connectionState != ConnectionState.Open)
		    {
		        throw new ArgumentException("Destination database is not open.", nameof(destination));
		    }
		    if (destinationName == null)
		    {
		        throw new ArgumentNullException(nameof(destinationName));
		    }
		    if (sourceName == null)
		    {
		        throw new ArgumentNullException(nameof(sourceName));
		    }
		    if (pages == 0)
		    {
		        throw new ArgumentException("pages must not be 0.", nameof(pages));
		    }

	        _database.BeginMaintenanceMode();
	        destination.Database.BeginMaintenanceMode();

		    using (SqliteBackupHandle backup = _database.Context.sqlite3_backup_init(destination.Database, destinationName, _database, sourceName))
			{
				if (backup == null)
				{
                    throw new SqliteException(_database.Context.sqlite3_errcode(_database, true), _database);
				}

			    while (true)
				{
					SqliteResultCode resultCode = _database.Context.sqlite3_backup_step(backup, pages);

					if (resultCode == SqliteResultCode.Done)
					{
						break;
					}
					else if (resultCode == SqliteResultCode.Ok || resultCode == SqliteResultCode.Busy || resultCode == SqliteResultCode.Locked)
					{
						bool retry = resultCode != SqliteResultCode.Ok;
						if (callback != null && !callback(this, sourceName, destination, destinationName, pages, _database.Context.sqlite3_backup_remaining(backup), _database.Context.sqlite3_backup_pagecount(backup), retry))
						{
						    break;
						}

					    if (retry && retryMilliseconds > 0)
					    {
					        Thread.Sleep(retryMilliseconds);
					    }
					}
					else
					{
						throw new SqliteException(resultCode, _database);
					}
				}
			}

	        destination.Database.EndMaintenanceMode();
	        _database.EndMaintenanceMode();
	    }

        /// <summary> Event queue for all listeners interested in StatementCompleted events. </summary>
		public event StatementCompletedEventHandler StatementCompleted {
			add
			{
				if (value == null)
				{
				    throw new ArgumentNullException(nameof(value));
				}

			    if (_statementCompleted == null && _database != null)
			    {
			        SetProfileCallback(_profileCallback);
			    }

			    _statementCompleted += value;
			}
			remove
			{
			    // ReSharper disable once DelegateSubtraction
			    _statementCompleted -= value ?? throw new ArgumentNullException(nameof(value));
				if (_statementCompleted == null && _database != null)
				{
				    SetProfileCallback(null);
				}
			}
		}

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
        /// resources.
        /// </summary>
        /// <param name="disposing"> True to release both managed and unmanaged resources; false to
        ///     release only unmanaged resources. </param>
		private void Dispose(bool disposing) {
			try
			{
				if (disposing)
				{
					if (_database != null)
					{
						while (_transactions.Count > 0)
						{
						    _transactions.Pop().Dispose();
						}
					    if (_statementCompleted != null)
					    {
					        SetProfileCallback(null);
					    }

					    if (!_usingDefault)
					    {
					        if (_database.IsDatabaseInMaintenanceMode && State == ConnectionState.Open)
					        {
					            _database.EndMaintenanceMode();
					        }
                            else if (State == ConnectionState.Open)
					        {
					            _database.CloseDatabase();
					        }
					        Utility.Dispose(ref _database);
                        }
                        SetState(ConnectionState.Closed);
					    _database = null;
					}
				}
				_isDisposed = true;
			}
			// ReSharper disable once RedundantEmptyFinallyBlock
			finally
			{
				//not needed
			}
		}

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
        /// resources.
        /// </summary>
        public void Dispose() {
            Dispose(true);
            // ReSharper disable once GCSuppressFinalizeForTypeWithoutDestructor
            GC.SuppressFinalize(this);
        }

        /// <summary> Gets or sets the current transaction. </summary>
        /// <value> The current transaction. </value>
        internal SqliteTransaction CurrentTransaction => _transactions.FirstOrDefault();

        /// <summary> Query if 'transaction' is only transaction. </summary>
        /// <param name="transaction"> The transaction. </param>
        /// <returns> True if only transaction, false if not. </returns>
	    internal bool IsOnlyTransaction(SqliteTransaction transaction) {
			return _transactions.Count == 1 && _transactions.Peek() == transaction;
		}

        /// <summary> Pops the transaction. </summary>
		internal void PopTransaction() {
			_transactions.Pop();
		}

        /// <summary> Callback, called when the set profile. </summary>
        /// <param name="callback"> The method to invoke between each step of the backup process.  This
        ///     parameter may be <c>null</c> (i.e., no callbacks will be performed). </param>
		private void SetProfileCallback(DbProvider.delegate_profile callback) {
			if (callback != null && !_handle.IsAllocated)
			{
			    _handle = GCHandle.Alloc(this);
			}
			else if (callback == null && _handle.IsAllocated)
			{
			    _handle.Free();
			}

            _database.Context.sqlite3_profile(_database, callback, _handle, _database.IsDatabaseInMaintenanceMode);
		}

        /// <summary> Sets a state. </summary>
        /// <param name="newState"> State of the new. </param>
		private void SetState(ConnectionState newState) {
			if (_connectionState != newState)
			{
				var previousState = _connectionState;
				_connectionState = newState;
			    if (State != previousState)
			    {
			        OnStateChange(new StateChangeEventArgs(previousState, State));
                }
            }
		}

        /// <summary> Raises the state change event. </summary>
        /// <param name="e"> Event information to send to registered event handlers. </param>
        private void OnStateChange(StateChangeEventArgs e) {
            var handler = StateChange;
            handler?.Invoke(this, e);
        }

        /// <summary> Callback, called when the profile. </summary>
        /// <param name="data"> The data. </param>
        /// <param name="statement"> The statement. </param>
        /// <param name="ns"> The ns. </param>
        private static void ProfileCallback(object data, string statement, long ns)
	    {
	        var connection = (data as SqliteConnection);
	        StatementCompletedEventHandler handler = connection?._statementCompleted;
	        handler?.Invoke(connection, new StatementCompletedEventArgs(statement, TimeSpan.FromMilliseconds(ns / 1000000.0)));
        }

        /// <summary> Check table name. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <exception cref="ArgumentOutOfRangeException"> Thrown when one or more arguments are outside
        ///     the required range. </exception>
        /// <exception cref="ArgumentException"> Thrown when one or more arguments have unsupported or
        ///     illegal values. </exception>
        /// <param name="tableName"> Name of the table. </param>
        /// <returns> A string. </returns>
	    private string CheckTableName(string tableName)
	    {
	        string invalidChars = @"';/\[]";

            string result = tableName?.Trim() ?? throw new ArgumentNullException(nameof(tableName));
	        if (result == "")
	        {
	            throw new ArgumentOutOfRangeException(nameof(tableName));
	        }

	        if (result.StartsWith("[") || result.EndsWith("]"))
	        {
	            if (result.StartsWith("[") && result.EndsWith("]") && result.Length > 2)
	            {
	                result = result.Substring(1, result.Length - 2);
	            }
	            else
	            {
	                throw new ArgumentException("The specified table name does not appear to be valid.");
                }
	        }

	        if (invalidChars.Cast<char>().Any(invalid => result.IndexOf(invalid) > -1))
	        {
	            throw new ArgumentException("The specified table name contains invalid characters.");
	        }

            return result;
	    } 

        /// <summary> Verify not disposed. </summary>
        /// <exception cref="ObjectDisposedException"> Thrown when a supplied object has been disposed. </exception>
        private void VerifyNotDisposed() {
			if (_isDisposed)
			{
			    throw new ObjectDisposedException(GetType().Name);
			}
		}

        /// <summary> Shutdown sqlite 3 provider. </summary>
        /// <returns> A SqliteResultCode. </returns>
	    public static SqliteResultCode ShutdownSqlite3Provider()
	    {
	        lock (staticLocker)
	        {
	            if ((!_defaultDatabaseFirstTimeOpen) && (_defaultDatabase != null))
	            {
                    //Need to do this backwards, because SqliteDatabaseHandle.Dispose() checks the 
                    // default database; so, in this case, we need it to be null before calling Dispose().
                    SqliteDatabaseHandle toBeDisposed = _defaultDatabase;
                    _defaultDatabase = null;
	                toBeDisposed.Dispose();
                    _defaultDatabaseFirstTimeOpen = true;
                }
	            return SqliteDatabaseHandle.ShutdownSqlite3Provider();
	        }
	    }

        /// <summary>
        /// Sets the database for Maintenance Mode - note that this also opens the database.
        /// </summary>
        /// <exception cref="InvalidOperationException"> Thrown when the requested operation is invalid. </exception>
        /// <returns> True if it succeeds, false if it fails. </returns>
	    public bool BeginDatabaseMaintenanceMode()
	    {
            if (_database == null) { Open(); }
            if (_database == null) { throw new InvalidOperationException("The database for maintenance has not be properly specified.");}
	        bool result = _database.IsDatabaseInMaintenanceMode || _database.BeginMaintenanceMode();
	        if (result && State != ConnectionState.Open)
	        {
	            Open();
	        }
	        return result;
	    }

        /// <summary>
        /// Ends database Maintenance Mode and sets the database for normal operations.
        /// </summary>
        /// <returns> True if it succeeds, false if it fails. </returns>
	    public bool EndDatabaseMaintenanceMode()
	    {
	        if (_database == null || !_database.IsDatabaseInMaintenanceMode) { return true; }
	        bool result = _database.EndMaintenanceMode();
	        if (result && State != ConnectionState.Closed)
	        {
	            Close();
	        }
	        return result;
	    }
	}

    /// <summary> Raised between each backup step. </summary>
    /// <param name="source"> The source database connection. </param>
    /// <param name="sourceName"> The source database name. </param>
    /// <param name="destination"> The destination database connection. </param>
    /// <param name="destinationName"> The destination database name. </param>
    /// <param name="pages"> The number of pages copied with each step. </param>
    /// <param name="remainingPages"> The number of pages remaining to be copied. </param>
    /// <param name="totalPages"> The total number of pages in the source database. </param>
    /// <param name="retry"> Set to true if the operation needs to be retried due to database locking
    ///     issues; otherwise, set to false. </param>
    /// <returns>
    /// <c>true</c> to continue with the backup process; otherwise  <c>false</c> to halt the backup
    /// process, rolling back any changes that have been made so far.
    /// </returns>
    public delegate bool SqliteBackupCallback(SqliteConnection source, string sourceName, SqliteConnection destination, string destinationName, int pages, int remainingPages, int totalPages, bool retry);
}
