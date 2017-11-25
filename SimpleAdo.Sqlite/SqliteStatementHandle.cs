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
using System.Runtime.InteropServices;

using DbProvider = SQLitePCL;
using DbProviderOperations = SQLitePCL.raw;

namespace SimpleAdo.Sqlite
{
    /// <summary> A sqlite statement handle. This class cannot be inherited. </summary>
	internal sealed class SqliteStatementHandle : CriticalHandle
	{
        /// <summary> The SQL. </summary>
	    private string _sql;
	    // ReSharper disable once RedundantDefaultMemberInitializer
        /// <summary> True if statement finalized. </summary>
	    private bool _statementFinalized = false;

        /// <summary> True to for maintenance. </summary>
	    private readonly bool _forMaintenance;

        /// <summary> Gets or sets a value indicating whether for maintenance. </summary>
        /// <value> True if for maintenance, false if not. </value>
        internal bool ForMaintenance => _forMaintenance;

        /// <summary> The database. </summary>
        private SqliteDatabaseHandle _database;

        /// <summary> Gets or sets the database. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <value> The database. </value>
        internal SqliteDatabaseHandle Database
        {
            get => _database;
            set => _database = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary> The statement. </summary>
	    private DbProvider.sqlite3_stmt _statement;

        /// <summary> Gets the statement. </summary>
        /// <exception cref="SqliteException"> Thrown when a Sqlite error condition occurs. </exception>
        /// <value> The statement. </value>
        public DbProvider.sqlite3_stmt Statement
	    {
	        get
	        {
	            if (_statement == null)
	            {
	                SqliteResultCode resultCode = PrepareStatement(_sql);
                    if (!resultCode.IsSuccessCode())
                    {
                        throw new SqliteException("An error was encountered with the following SQL: " + _sql, resultCode);
                    }
                }
	            return _statement;
	        }
	    }

        /// <summary> Gets or sets a value indicating whether this object is invalid. </summary>
        /// <value> True if this object is invalid, false if not. </value>
	    public override bool IsInvalid => handle == new IntPtr(-1) || handle == (IntPtr)0;

        /// <summary> Constructor. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <exception cref="ArgumentOutOfRangeException"> Thrown when one or more arguments are outside
        ///     the required range. </exception>
        /// <param name="sql"> The SQL. </param>
        /// <param name="forMaintenance"> (Optional) True to for maintenance. </param>
        public SqliteStatementHandle(string sql, bool forMaintenance = false)
			: base((IntPtr) 0)
        {
            _sql = sql?.Trim() ?? throw new ArgumentNullException(nameof(sql));
            if (_sql == "")
            {
                throw new ArgumentOutOfRangeException(nameof(sql), "The statement SQL cannot be blank.");
            }
            _forMaintenance = forMaintenance;
        }

        /// <summary> Constructor. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <param name="sql"> The SQL. </param>
        /// <param name="database"> The database. </param>
        /// <param name="forMaintenance"> (Optional) True to for maintenance. </param>
        public SqliteStatementHandle(string sql, SqliteDatabaseHandle database, bool forMaintenance = false)
			: this(sql, forMaintenance)
		{
		    _database = database ?? throw new ArgumentNullException(nameof(database));
        }

        /// <summary> Constructor. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <param name="database"> The database. </param>
        /// <param name="forMaintenance"> (Optional) True to for maintenance. </param>
	    internal SqliteStatementHandle(SqliteDatabaseHandle database, bool forMaintenance = false)
	        : base((IntPtr)0)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
            _forMaintenance = forMaintenance;
        }

        /// <summary> Check maintenance mode. </summary>
        /// <exception cref="InvalidOperationException"> Thrown when the requested operation is invalid. </exception>
	    public void CheckMaintenanceMode()
	    {
	        if (_database != null && _forMaintenance != _database.IsDatabaseInMaintenanceMode)
	        {
	            throw new InvalidOperationException(_forMaintenance 
                    ? "The database must be placed in Maintenance Mode before accessing it for maintenance operations."
                    : "The database cannot be accessed for normal operations while it is in Maintenance Mode.");
            }
	    }

        /// <summary> Prepare statement. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <exception cref="ArgumentOutOfRangeException"> Thrown when one or more arguments are outside
        ///     the required range. </exception>
        /// <exception cref="InvalidOperationException"> Thrown when the requested operation is invalid. </exception>
        /// <param name="sql"> The SQL. </param>
        /// <returns> A SqliteResultCode. </returns>
	    internal SqliteResultCode PrepareStatement(string sql)
	    {
	        sql = sql?.Trim() ?? throw new ArgumentNullException(nameof(sql));
	        if (sql == "")
	        {
	            throw new ArgumentOutOfRangeException(nameof(sql), "The statement SQL cannot be blank.");
	        }
	        if (_database == null)
	        {
	            throw new InvalidOperationException("The database for the statement has not been specified.");
	        }
	        if (_statement != null)
	        {
	            throw new InvalidOperationException("Cannot re-initialize the command text for this SQLite statement.");
            }
	        _sql = sql;
	        DbProvider.sqlite3 database = (_forMaintenance) ? _database.MaintenanceDb : _database.Db;
            CheckMaintenanceMode();
	        return (SqliteResultCode)DbProviderOperations.sqlite3_prepare_v2(database, _sql, out _statement);
        }

        /// <summary> Finalize statement. </summary>
        /// <exception cref="InvalidOperationException"> Thrown when the requested operation is invalid. </exception>
        /// <returns> A SqliteResultCode. </returns>
	    public SqliteResultCode FinalizeStatement()
	    {
	        if (_database == null)
	        {
	            throw new InvalidOperationException("The database for the statement has not been specified.");
	        }
	        SqliteResultCode resultCode = _statementFinalized ? SqliteResultCode.Ok : SqliteResultCode.NotFound;
	        if ((!_statementFinalized) && _statement != null)
	        {
	            CheckMaintenanceMode();
                resultCode = (SqliteResultCode) DbProviderOperations.sqlite3_finalize(_statement);
	            if (resultCode.IsSuccessCode())
	            {
	                _statementFinalized = true;
	            }
	        }
	        return resultCode;
	    }

        /// <summary> Releases the handle. </summary>
        /// <returns> True if it succeeds, false if it fails. </returns>
	    protected override bool ReleaseHandle()
	    {
	        return (_statement == null) || FinalizeStatement().IsSuccessCode();
		}
	}
}
