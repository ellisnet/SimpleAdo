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
using System.Threading;
using System.Threading.Tasks;

namespace SimpleAdo.Sqlite {

    /// <summary> A sqlite command. This class cannot be inherited. </summary>
	public sealed class SqliteCommand : IDbCommand
	{
        /// <summary> The database. </summary>
	    private SqliteDatabaseHandle _database;
        /// <summary> True to for maintenance. </summary>
	    private readonly bool _forMaintenance;

        /// <summary> Gets or sets a value indicating whether for maintenance. </summary>
        /// <value> True if for maintenance, false if not. </value>
	    internal bool ForMaintenance => _forMaintenance;

        /// <summary>
        /// ReSharper disable once InconsistentNaming ReSharper disable once
        /// RedundantDefaultMemberInitializer.
        /// </summary>
	    internal IObjectCryptEngine _cryptEngine = null;
        /// <summary> The no crypt engine. </summary>
        private static readonly string NO_CRYPT_ENGINE = "Cryptography has not been enabled on this SQLite command.";

	    // ReSharper disable RedundantArgumentDefaultValue

        /// <summary> Default constructor. </summary>
        public SqliteCommand()
			: this(null, null, null, null, false)
		{
		}

        /// <summary> Constructor. </summary>
        /// <param name="cryptEngine"> The crypt engine. </param>
        public SqliteCommand(IObjectCryptEngine cryptEngine)
            : this(null, null, null, cryptEngine, false) 
        {
        }

        /// <summary> Constructor. </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="forMaintenance"> (Optional) True to for maintenance. </param>
	    public SqliteCommand(SqliteConnection connection, bool forMaintenance = false)
	        : this(null, connection, null, null, forMaintenance)
	    {
	    }

        /// <summary> Constructor. </summary>
        /// <param name="commandText"> The command text. </param>
        /// <param name="cryptEngine"> (Optional) The crypt engine. </param>
        public SqliteCommand(string commandText, IObjectCryptEngine cryptEngine = null)
			: this(commandText, null, null, cryptEngine, false)
		{
		}

        /// <summary> Constructor. </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="cryptEngine"> The crypt engine. </param>
        /// <param name="forMaintenance"> (Optional) True to for maintenance. </param>
		public SqliteCommand(SqliteConnection connection, IObjectCryptEngine cryptEngine, bool forMaintenance = false)
			: this(null, connection, null, cryptEngine, forMaintenance)
		{
		}

        /// <summary> Constructor. </summary>
        /// <param name="commandText"> The command text. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="cryptEngine"> The crypt engine. </param>
        /// <param name="forMaintenance"> (Optional) True to for maintenance. </param>
        public SqliteCommand(string commandText, SqliteConnection connection, IObjectCryptEngine cryptEngine, bool forMaintenance = false)
			: this(commandText, connection, null, cryptEngine, forMaintenance)
		{
		}

        /// <summary> Constructor. </summary>
        /// <param name="commandText"> The command text. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="forMaintenance"> (Optional) True to for maintenance. </param>
	    public SqliteCommand(string commandText, SqliteConnection connection, bool forMaintenance = false)
	        : this(commandText, connection, null, null, forMaintenance)
	    {
	    }

	    // ReSharper restore RedundantArgumentDefaultValue

        /// <summary> Constructor. </summary>
        /// <param name="commandText"> The command text. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="transaction"> The transaction. </param>
        /// <param name="cryptEngine"> (Optional) The crypt engine. </param>
        /// <param name="forMaintenance"> (Optional) True to for maintenance. </param>
        public SqliteCommand(string commandText, SqliteConnection connection, IDbTransaction transaction, IObjectCryptEngine cryptEngine = null, bool forMaintenance = false) {
			_commandText = String.IsNullOrWhiteSpace(commandText) ? null : commandText.Trim();
            _dbConnection = connection;
		    _forMaintenance = forMaintenance;
		    _database = connection?.Database;
		    _cryptEngine = cryptEngine ?? connection?._cryptEngine;
		    Transaction = transaction;
			m_parameterCollection = new SqliteParameterCollection();
		}

        /// <summary> Prepares this object. </summary>
        /// <exception cref="InvalidOperationException"> Thrown when the requested operation is invalid. </exception>
		public void Prepare() {
			if (m_statementPreparer == null)
			{
			    if (_database == null)
			    {
			        throw new InvalidOperationException("The database connection has not been properly set.");
			    }
			    m_statementPreparer = new SqliteStatementPreparer(_database, _commandText, _forMaintenance);
			}
		}

        /// <summary> The command text. </summary>
	    private string _commandText;

        /// <summary> Gets or sets the command text. </summary>
        /// <value> The command text. </value>
	    public string CommandText {
	        get => _commandText;
	        set {
	            _commandText = String.IsNullOrWhiteSpace(value) ? null : value.Trim();
                //need to reset the statementpreparer, since the statement to be executed has changed
	            m_statementPreparer = null;
	        }
	    }

        /// <summary> Gets or sets the command timeout. </summary>
        /// <value> The command timeout. </value>
	    public int CommandTimeout { get; set; }

        /// <summary> Gets or sets the type of the command. </summary>
        /// <exception cref="ArgumentException"> Thrown when one or more arguments have unsupported or
        ///     illegal values. </exception>
        /// <value> The type of the command. </value>
		public CommandType CommandType {
			get => CommandType.Text;
		    set {
				if (value != CommandType.Text)
				{
				    throw new ArgumentException("CommandType must be Text.", nameof(value));
				}
		    }
		}

        /// <summary> Gets or sets the updated row source. </summary>
        /// <exception cref="NotSupportedException"> Thrown when the requested operation is not supported. </exception>
        /// <value> The updated row source. </value>
		public UpdateRowSource UpdatedRowSource {
			get => throw new NotSupportedException();
		    set => throw new NotSupportedException();
		}

        /// <summary> Gets or sets the connection. </summary>
        /// <value> The connection. </value>
		public IDbConnection Connection {
		    get => _dbConnection;
		    set => this.DbConnection = value as SqliteConnection;
		}

        /// <summary> The database connection. </summary>
	    private SqliteConnection _dbConnection;

        /// <summary> Gets or sets the database connection. </summary>
        /// <value> The database connection. </value>
        public SqliteConnection DbConnection {
            get => _dbConnection;
            set {
                _dbConnection = value;
                _database = value?.Database;
            }
        }

        /// <summary> Gets options for controlling the operation. </summary>
        /// <value> The parameters. </value>
		public IDataParameterCollection Parameters {
			get {
				VerifyCommandNotDisposed();
				return m_parameterCollection;
			}
		}

        /// <summary> Gets or sets the transaction. </summary>
        /// <value> The transaction. </value>
		public IDbTransaction Transaction { get; set; }

        /// <summary> Cancels this object. </summary>
		public void Cancel() {
			throw new NotImplementedException();
		}

        /// <summary> Creates the parameter. </summary>
        /// <returns> The new parameter. </returns>
		public IDbDataParameter CreateParameter() {
			VerifyCommandNotDisposed();
			return new SqliteParameter();
		}

        /// <summary> Executes the reader operation. </summary>
        /// <param name="behavior"> The behavior. </param>
        /// <returns> An IDataReader. </returns>
		public IDataReader ExecuteReader(CommandBehavior behavior) {
			VerifyCommandIsValid();
			Prepare();
			return SqliteDataReader.Create(this, behavior);
		}

        /// <summary> Executes the reader operation. </summary>
        /// <returns> An IDataReader. </returns>
	    public IDataReader ExecuteReader() {
            return ExecuteReader(CommandBehavior.Default);
        }

        /// <summary>
        /// Execute the command and return the number of rows inserted/updated affected by it.
        /// </summary>
        /// <returns> Number of rows. </returns>
        public int ExecuteNonQuery() {
			using (var reader = ExecuteReader())
			{
			    bool readSuccess = reader.Read();
			    while (readSuccess)
			    {
			        readSuccess &= reader.NextResult() && reader.Read();
			    }
				return reader.RecordsAffected;
			}
		}

        /// <summary> Executes the command and return the Id of last inserted row. </summary>
        /// <returns> Id of last inserted row. </returns>
        public long ExecuteReturnRowId() {
            using (var reader = ExecuteReader()) {
                reader.SetRowId = true;
                bool readSuccess = reader.Read();
                while (readSuccess)
                {
                    readSuccess &= reader.NextResult() && reader.Read();
                }
                return reader.LastInsertRowId;
            }
        }

        /// <summary>
        /// Execute the command and return the first column of the first row of the resultset (if
        /// present), or null if no resultset was returned.
        /// </summary>
        /// <returns> The first column of the first row of the first resultset from the query. </returns>
        public object ExecuteScalar() {
			using (var reader = ExecuteReader(CommandBehavior.SingleResult | CommandBehavior.SingleRow)) {
				do {
					if (reader.Read())
					{
					    return reader.GetValue(0);
					}
				} while (reader.NextResult());
			}
			return null;
		}

        /// <summary>
        /// A variation of DbCommand.ExecuteScalar() that allows you to specify the Type of the returned
        /// value.
        /// </summary>
        /// <exception cref="DbNullException"> Thrown when a Database Null error condition occurs. </exception>
        /// <typeparam name="T"> The type of value to return. </typeparam>
        /// <param name="dbNullHandling"> (Optional) Determines how table column values of NULL are
        ///     handled. </param>
        /// <returns> A value of the specified type. </returns>
        public T ExecuteScalar<T>(DbNullHandling dbNullHandling = DbNullHandling.ThrowDbNullException) {
            T result = default(T);
            using (var reader = ExecuteReader(CommandBehavior.SingleResult | CommandBehavior.SingleRow)) {
                if (reader.Read()) {
                    if (reader.IsDBNull(0)) {
                        if (dbNullHandling == DbNullHandling.ThrowDbNullException)
                        {
                            throw new DbNullException();
                        }
                    }
                    else {
                        result = (T)Convert.ChangeType(reader[0], typeof(T));
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// A variation of DbCommand.ExecuteScalar() that allows you to specify that a decrypted value of
        /// the specified type will be returned.
        /// </summary>
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// <exception cref="DbNullException"> Thrown when a Database Null error condition occurs. </exception>
        /// <typeparam name="T"> The type of value to return after decryption. </typeparam>
        /// <param name="dbNullHandling"> (Optional) Determines how table column values of NULL are
        ///     handled. </param>
        /// <returns> A decrypted value of the specified type. </returns>
        public T ExecuteDecrypt<T>(DbNullHandling dbNullHandling = DbNullHandling.ThrowDbNullException) {
            T result = default(T);
            if (_cryptEngine == null)
            {
                throw new Exception(NO_CRYPT_ENGINE);
            }
            using (var reader = ExecuteReader(CommandBehavior.SingleResult | CommandBehavior.SingleRow)) {
                if (reader.Read()) {
                    if (reader.IsDBNull(0)) {
                        if (dbNullHandling == DbNullHandling.ThrowDbNullException)
                        {
                            throw new DbNullException();
                        }
                    }
                    else {
                        result = _cryptEngine.DecryptObject<T>(reader[0].ToString());
                    }
                }
            }
            return result;
        }

        /// <summary> Executes the reader asynchronous operation. </summary>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> The asynchronous result that yields an IDataReader. </returns>
        public Task<IDataReader> ExecuteReaderAsync(CancellationToken cancellationToken) {
	        return ExecuteReaderAsync(CommandBehavior.Default, cancellationToken);
	    }

        /// <summary> Executes the non query asynchronous operation. </summary>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> The asynchronous result that yields an int. </returns>
	    public async Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken) {
            using (var reader = await ExecuteReaderAsync(cancellationToken).ConfigureAwait(false)) {
                do {
                    while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false)) {
                    }
                } while (await reader.NextResultAsync(cancellationToken).ConfigureAwait(false));
                return reader.RecordsAffected;
            }
        }

        /// <summary> Executes the reader asynchronous operation. </summary>
        /// <param name="behavior"> The behavior. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> The asynchronous result that yields an IDataReader. </returns>
        public Task<IDataReader> ExecuteReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken) {
            VerifyCommandIsValid();
            Prepare();
            return SqliteDataReader.CreateAsync(this, behavior, cancellationToken);
        }

        /// <summary> Executes the scalar asynchronous operation. </summary>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> The asynchronous result that yields an object. </returns>
        public async Task<object> ExecuteScalarAsync(CancellationToken cancellationToken) {
            using (var reader = await ExecuteReaderAsync(CommandBehavior.SingleResult | CommandBehavior.SingleRow, cancellationToken).ConfigureAwait(false)) {
                do {
                    if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                    {
                        return reader.GetValue(0);
                    }
                } while (await reader.NextResultAsync(cancellationToken).ConfigureAwait(false));
            }
            return null;
        }

        /// <summary>
        /// Adds a Sqlite parameter to the command with properties matching the specified parameter, but
        /// with the value encrypted.
        /// </summary>
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// <exception cref="ArgumentException"> Thrown when one or more arguments have unsupported or
        ///     illegal values. </exception>
        /// <param name="parameter"> The parameter to be added after the value has been encrypted. </param>
        public void AddEncryptedParameter(SqliteParameter parameter) {
            if (_cryptEngine == null)
            {
                throw new Exception(NO_CRYPT_ENGINE);
            }
            if (!String.IsNullOrWhiteSpace(parameter?.ParameterName)) { 
                if (parameter.Direction != ParameterDirection.Input)
                {
                    throw new ArgumentException("Only Input parameters can be encrypted.", nameof(parameter));
                }
                this.Parameters.Add(new SqliteParameter(parameter.ParameterName, DbType.String) {
                    Value = _cryptEngine.EncryptObject(parameter.Value),
                    Direction = ParameterDirection.Input
                });
            }
        }

	    // ReSharper disable once UnusedParameter.Local

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
        /// resources.
        /// </summary>
        /// <param name="disposing"> True to release both managed and unmanaged resources; false to
        ///     release only unmanaged resources. </param>
        private void Dispose(bool disposing) {
			try
			{
				m_parameterCollection = null;
			    _database = null;
				Utility.Dispose(ref m_statementPreparer);
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

        /// <summary> Gets statement preparer. </summary>
        /// <returns> The statement preparer. </returns>
        internal SqliteStatementPreparer GetStatementPreparer(bool skipDatabaseOpenCheck = false) {
            if (m_statementPreparer == null) {
                VerifyCommandIsValid(skipDatabaseOpenCheck);
                Prepare();
            }
            m_statementPreparer.AddRef();
			return m_statementPreparer;
		}

        /// <summary> Verify command not disposed. </summary>
        /// <exception cref="ObjectDisposedException"> Thrown when a supplied object has been disposed. </exception>
	    private void VerifyCommandNotDisposed() {
			if (m_parameterCollection == null)
			{
			    throw new ObjectDisposedException(GetType().Name);
			}
		}

        /// <summary> Verify command is valid. </summary>
        /// <exception cref="InvalidOperationException"> Thrown when the requested operation is invalid. </exception>
        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
		private void VerifyCommandIsValid(bool skipDatabaseOpenCheck = false) {
			VerifyCommandNotDisposed();
			if (Connection == null)
			{
			    throw new InvalidOperationException("Connection property must be non-null.");
			}
            if (skipDatabaseOpenCheck)
            {
                Connection.SafeOpen();
            }
            if (Connection.State != ConnectionState.Open && Connection.State != ConnectionState.Connecting)
            {
                throw new InvalidOperationException("Connection must be Open; current state is {0}.".FormatInvariant(Connection.State));
            }

            if (Transaction != ((SqliteConnection) Connection).CurrentTransaction)
		    {
		        throw new InvalidOperationException("The transaction associated with this command is not the connection's active transaction.");
		    }
		    if (string.IsNullOrWhiteSpace(_commandText))
		    {
		        throw new InvalidOperationException("CommandText must be specified");
		    }
		}

	    // ReSharper disable InconsistentNaming
        /// <summary> Collection of parameters. </summary>
		SqliteParameterCollection m_parameterCollection;
        /// <summary> The statement preparer. </summary>
		SqliteStatementPreparer m_statementPreparer;
	    // ReSharper restore InconsistentNaming
	}
}
