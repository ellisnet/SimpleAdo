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
using DbProvider = SQLitePCL;
using DbProviderOperations = SQLitePCL.raw;

namespace SimpleAdo.Sqlite
{
    /// <summary> A sqlite database handle. This class cannot be inherited. </summary>
	internal sealed class SqliteDatabaseHandle : IDisposable
	{
        /// <summary> The database locker. </summary>
        private readonly object _dbLocker = new object();
        /// <summary> True to path added to active. </summary>
	    private bool _pathAddedToActive;

        /// <summary> True if sqlite provider initialized. </summary>
	    private static bool sqliteProviderInitialized;
        /// <summary> The static locker. </summary>
        private static readonly object staticLocker = new object();

        /// <summary> The active database paths. </summary>
        private static readonly List<string> activeDbPaths = new List<string>();

	    // ReSharper disable RedundantDefaultMemberInitializer

        /// <summary> True if this object is database locked. </summary>
	    private bool _isDbLocked = false;

        /// <summary>
        /// Gets or sets a value indicating whether this object is database locked.
        /// </summary>
        /// <value> True if this object is database locked, false if not. </value>
	    internal bool IsDatabaseLocked => _isDbLocked;

        /// <summary>
        /// True if the database first open operation was a success, false if it failed.
        /// </summary>
	    private bool _dbFirstOpenSuccess = false;

        /// <summary>
        /// Gets or sets a value indicating whether the database first open success.
        /// </summary>
        /// <value> True if database first open success, false if not. </value>
	    internal bool DbFirstOpenSuccess => _dbFirstOpenSuccess;

        /// <summary> True if this object is database open. </summary>
	    private bool _isDbOpen = false;

        /// <summary> Gets or sets a value indicating whether the database is open. </summary>
        /// <value> True if the database is open, false if not. </value>
	    internal bool IsDatabaseOpen => _isDbOpen && (!_isDatabaseInMaintenanceMode);

        /// <summary> Gets or sets a value indicating whether the database is closed. </summary>
        /// <value> True if the database is closed, false if not. </value>
	    internal bool IsDatabaseClosed => _dbFirstOpenSuccess && (!_isDbOpen) && (!_isDatabaseInMaintenanceMode);

        /// <summary> True if this object is database in maintenance mode. </summary>
	    private bool _isDatabaseInMaintenanceMode = false;

        /// <summary>
        /// Gets or sets a value indicating whether this object is database in maintenance mode.
        /// </summary>
        /// <value> True if this object is database in maintenance mode, false if not. </value>
	    internal bool IsDatabaseInMaintenanceMode => _isDatabaseInMaintenanceMode;

        /// <summary> The connection string. </summary>
	    private string _connectionString;

        /// <summary> Gets or sets the connection string. </summary>
        /// <value> The connection string. </value>
	    internal string ConnectionString => _connectionString;

        /// <summary> The context. </summary>
	    private SqliteContext _context = new SqliteContext();

        /// <summary> Gets or sets the context. </summary>
        /// <value> The context. </value>
	    internal SqliteContext Context => _context;

        /// <summary> Full pathname of the database file. </summary>
	    private string _databaseFilePath = null;

        /// <summary> Gets or sets the full pathname of the database file. </summary>
        /// <exception cref="InvalidOperationException"> Thrown when the requested operation is invalid. </exception>
        /// <value> The full pathname of the database file. </value>
	    internal string DatabaseFilePath
	    {
	        // ReSharper disable InconsistentlySynchronizedField
	        get => _databaseFilePath;
	        set => _databaseFilePath = (_databaseFilePath == null)
	            ? (String.IsNullOrWhiteSpace(value) ? null : value.Trim())
	            : throw new InvalidOperationException($"The '{nameof(DatabaseFilePath)}' property can only be set once.");
	        // ReSharper restore InconsistentlySynchronizedField
	    }

        /// <summary> The open flags. </summary>
        private SqliteOpenFlags _openFlags;

        /// <summary> Gets or sets the open flags. </summary>
        /// <exception cref="InvalidOperationException"> Thrown when the requested operation is invalid. </exception>
        /// <value> The open flags. </value>
	    internal SqliteOpenFlags OpenFlags
	    {
	        get => _openFlags;
	        set => _openFlags = (_dbFirstOpenSuccess)
	            ? throw new InvalidOperationException(
	                $"The '{nameof(OpenFlags)}' property can not be set once the database has been opened.")
	            : value;
	    }

        /// <summary> True to store date time as ticks. </summary>
	    private readonly bool _storeDateTimeAsTicks;

        /// <summary> Gets or sets a value indicating whether the store date time as ticks. </summary>
        /// <value> True if store date time as ticks, false if not. </value>
	    internal bool StoreDateTimeAsTicks => _storeDateTimeAsTicks;

        /// <summary> The database. </summary>
	    private DbProvider.sqlite3 _db = null;

        /// <summary> Gets or sets the database. </summary>
        /// <value> The database. </value>
	    internal DbProvider.sqlite3 Db => (!_isDatabaseInMaintenanceMode) 
            ? (_db ?? (_db = GetSqlite3()))
	        : throw new InvalidOperationException("The database cannot be accessed for normal operations while it is in Maintenance Mode.");

        /// <summary> Gets or sets the maintenance database. </summary>
        /// <value> The maintenance database. </value>
	    internal DbProvider.sqlite3 MaintenanceDb => (_isDatabaseInMaintenanceMode) 
            ? (_db ?? (_db = GetSqlite3()))
            : throw new InvalidOperationException("The database must be placed in Maintenance Mode before accessing it for maintenance operations.");

        // ReSharper restore RedundantDefaultMemberInitializer

        /// <summary> Constructor. </summary>
        /// <param name="databaseFilePath"> Full pathname of the database file. </param>
        /// <param name="openFlags"> The open flags. </param>
        /// <param name="connectionString"> (Optional) The connection string. </param>
        /// <param name="storeDateTimeAsTicks"> (Optional) True to store date time as ticks. </param>
	    internal SqliteDatabaseHandle(string databaseFilePath, SqliteOpenFlags openFlags, string connectionString = null, bool storeDateTimeAsTicks = true)
	    {
	        _databaseFilePath = (String.IsNullOrWhiteSpace(databaseFilePath)) ? null : databaseFilePath.Trim();
	        _connectionString = (String.IsNullOrWhiteSpace(connectionString)) ? null : connectionString.Trim();
            _openFlags = openFlags;
	        _storeDateTimeAsTicks = storeDateTimeAsTicks;
	    }

        /// <summary> Gets sqlite 3. </summary>
        /// <exception cref="InvalidOperationException"> Thrown when the requested operation is invalid. </exception>
        /// <exception cref="SqliteException"> Thrown when a Sqlite error condition occurs. </exception>
        /// <returns> The sqlite 3. </returns>
	    private DbProvider.sqlite3 GetSqlite3()
	    {
	        if (_databaseFilePath == null) { throw new InvalidOperationException($"The '{nameof(DatabaseFilePath)}' property has not yet been set.");}

	        if (!sqliteProviderInitialized)
	        {
	            lock (staticLocker)
	            {
	                if (!sqliteProviderInitialized)
	                {
	                    DbProvider.Batteries_V2.Init();
	                    sqliteProviderInitialized = true;
	                }
                }
	        }

	        lock (_dbLocker)
	        {
	            _isDbLocked = true;

	            if (!_dbFirstOpenSuccess)
	            {
	                lock (staticLocker)
	                {
                        if (activeDbPaths.Any(a => a.Equals(_databaseFilePath, StringComparison.CurrentCultureIgnoreCase)))
	                    {
	                        throw new InvalidOperationException($"Cannot have more than one database handle for the SQLite database at: {_databaseFilePath}");
	                    }
	                }
	            }

	            var resultCode = (SqliteResultCode) DbProviderOperations.sqlite3_open_v2(_databaseFilePath, out DbProvider.sqlite3 result, (int) _openFlags, null);
	            if ((!resultCode.IsSuccessCode()) || result == null)
	            {
	                throw new SqliteException(
	                    $"Attempting to open the database at the following location resulted in '{(result == null ? "Null database" : resultCode.ToString())}': {_databaseFilePath}",
	                    resultCode);
	            }
	            else
	            {
	                if (!_dbFirstOpenSuccess)
	                {
	                    lock (staticLocker)
	                    {
	                        if (activeDbPaths.Any(a => a.Equals(_databaseFilePath, StringComparison.CurrentCultureIgnoreCase)))
                            {
	                            throw new InvalidOperationException($"Cannot have more than one database handle for the SQLite database at: {_databaseFilePath}");
	                        }
	                        activeDbPaths.Add(_databaseFilePath);
	                        _pathAddedToActive = true;
	                    }
	                    _dbFirstOpenSuccess = true;
	                }
	                _isDbOpen = true;
	            }

                _isDbLocked = false;

	            return result;
	        }
	    }

        /// <summary> Queries if a given check first time open. </summary>
	    private void CheckFirstTimeOpen()
	    {
	        if (_db == null)
	        {
	            _db = GetSqlite3();
	        }
        }

        /// <summary> Determines if we can check open database. </summary>
        /// <exception cref="SqliteException"> Thrown when a Sqlite error condition occurs. </exception>
        /// <returns> True if it succeeds, false if it fails. </returns>
	    internal bool CheckOpenDatabase()
	    {
            CheckFirstTimeOpen();

	        if ((!_isDbOpen) && (!_isDatabaseInMaintenanceMode))
	        {
	            lock (_dbLocker)
	            {
	                _isDbLocked = true;

                    if ((!_isDbOpen) && (!_isDatabaseInMaintenanceMode))
                    {
	                    var resultCode = (SqliteResultCode)DbProviderOperations.sqlite3_open_v2(_databaseFilePath, out _db, (int)_openFlags, null);

	                    if ((!resultCode.IsSuccessCode()) || _db == null)
	                    {
	                        throw new SqliteException(
	                            $"Attempting to open the database at the following location resulted in '{(_db == null ? "Null database" : resultCode.ToString())}': {_databaseFilePath}",
	                            resultCode);
	                    }
	                    else
	                    {
	                        _isDbOpen = true;
	                    }
                    }

                    _isDbLocked = false;
	            }
	        }

	        return _isDbOpen && (!_isDatabaseInMaintenanceMode);
	    }

        /// <summary> Closes the database. </summary>
        /// <exception cref="InvalidOperationException"> Thrown when the requested operation is invalid. </exception>
        /// <exception cref="SqliteException"> Thrown when a Sqlite error condition occurs. </exception>
        /// <returns> True if it succeeds, false if it fails. </returns>
	    internal bool CloseDatabase()
	    {
	        CheckFirstTimeOpen();

	        if (!_dbFirstOpenSuccess)
	        {
	            throw new InvalidOperationException("The database must be opened once before it can be closed.");
	        }

	        if (_isDbOpen && (!_isDatabaseInMaintenanceMode))
	        {
	            lock (_dbLocker)
	            {
	                if (_isDbOpen && (!_isDatabaseInMaintenanceMode))
	                {
	                    _isDbLocked = true;

	                    var resultCode = (SqliteResultCode)DbProviderOperations.sqlite3_close_v2(_db);
	                    if (!resultCode.IsSuccessCode())
	                    {
	                        throw new SqliteException(
	                            $"Attempting to close the database at the following location resulted in '{resultCode}': {_databaseFilePath}",
	                            resultCode);
	                    }
	                    else
	                    {
	                        _isDbOpen = false;
	                    }

                        _isDbLocked = false;
	                }
	            }
	        }

	        return !_isDbOpen && (!_isDatabaseInMaintenanceMode);
	    }

        /// <summary> Begins maintenance mode. </summary>
        /// <exception cref="SqliteException"> Thrown when a Sqlite error condition occurs. </exception>
        /// <returns> True if it succeeds, false if it fails. </returns>
	    internal bool BeginMaintenanceMode()
	    {
	        CheckFirstTimeOpen();

	        if (!_isDatabaseInMaintenanceMode)
	        {
	            lock (_dbLocker)
	            {
	                _isDbLocked = true;

	                if (!_isDbOpen)
	                {
	                    var resultCode = (SqliteResultCode)DbProviderOperations.sqlite3_open_v2(_databaseFilePath, out _db, (int)_openFlags, null);

	                    if ((!resultCode.IsSuccessCode()) || _db == null)
	                    {
	                        throw new SqliteException(
	                            $"Attempting to open the database for maintenance at the following location resulted in '{(_db == null ? "Null database" : resultCode.ToString())}': {_databaseFilePath}",
	                            resultCode);
	                    }
	                    else
	                    {
	                        _isDbOpen = true;
	                    }
                    }

	                _isDatabaseInMaintenanceMode = true;

                    _isDbLocked = false;
	            }
	        }

	        return _isDatabaseInMaintenanceMode;
	    }

        /// <summary> Ends maintenance mode. </summary>
        /// <exception cref="SqliteException"> Thrown when a Sqlite error condition occurs. </exception>
        /// <returns> True if it succeeds, false if it fails. </returns>
	    internal bool EndMaintenanceMode()
	    {
	        CheckFirstTimeOpen();

	        if (_isDatabaseInMaintenanceMode)
	        {
	            lock (_dbLocker)
	            {
	                _isDbLocked = true;

	                if (_isDbOpen)
	                {
	                    var resultCode = (SqliteResultCode)DbProviderOperations.sqlite3_close_v2(_db);
	                    if (!resultCode.IsSuccessCode())
	                    {
	                        throw new SqliteException(
	                            $"Attempting to close the database at the following location resulted in '{resultCode}': {_databaseFilePath}",
	                            resultCode);
	                    }
	                    else
	                    {
	                        _isDbOpen = false;
	                    }
                    }

	                _isDatabaseInMaintenanceMode = false;

                    _isDbLocked = false;
	            }
            }

            return !_isDatabaseInMaintenanceMode;
	    }

        /// <summary> Shutdown sqlite 3 provider. </summary>
        /// <returns> A SqliteResultCode. </returns>
	    internal static SqliteResultCode ShutdownSqlite3Provider()
	    {
	        return (SqliteResultCode)DbProviderOperations.sqlite3_shutdown();
	    }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
        /// resources.
        /// </summary>
        public void Dispose()
	    {
	        if (_db != null && _isDbOpen)
	        {
	            if (_isDatabaseInMaintenanceMode)
	            {
	                EndMaintenanceMode();
	            }
	            else
	            {
	                CloseDatabase();
	            }
	        }

            lock (_dbLocker)
	        {
	            _isDbLocked = true;

                _db?.Dispose();
	            _db = null;
	            _dbFirstOpenSuccess = false;
	            _isDbOpen = false;
	            _isDatabaseInMaintenanceMode = false;

                if (_databaseFilePath != null)
	            {
	                lock (staticLocker)
	                {
                        if (activeDbPaths.Any(a => a.Equals(_databaseFilePath, StringComparison.CurrentCultureIgnoreCase)) && _pathAddedToActive
                            && !_databaseFilePath.Equals((SqliteConnection.DefaultDatabase?.DatabaseFilePath ?? "none"), StringComparison.CurrentCultureIgnoreCase))
                        {
                            string toBeRemoved = activeDbPaths.First(f =>
                                f.Equals(_databaseFilePath, StringComparison.CurrentCultureIgnoreCase));
                            activeDbPaths.Remove(toBeRemoved);
	                        _pathAddedToActive = false;
	                    }
	                }
	            }
	            _openFlags = SqliteOpenFlags.None;
	            _databaseFilePath = null;
	            _connectionString = null;
	            _context = null;

	            _isDbLocked = false;
	        }
	    }
	}
}
