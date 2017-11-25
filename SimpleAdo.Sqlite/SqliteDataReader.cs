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
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleAdo.Sqlite {
    /// <summary> A sqlite data reader. This class cannot be inherited. </summary>
	public sealed class SqliteDataReader : IDataReader
    {
        #region Fields

        /// <summary> The database. </summary>
        private SqliteDatabaseHandle _database;

        /// <summary> Gets or sets the database. </summary>
        /// <value> The database. </value>
        internal SqliteDatabaseHandle Database => _database;
        /// <summary> The command. </summary>
	    private SqliteCommand _command;
        /// <summary> The behavior. </summary>
	    private readonly CommandBehavior _behavior;
        /// <summary> The starting changes. </summary>
	    private readonly int _startingChanges;
        /// <summary> The statement preparer. </summary>
	    private SqliteStatementPreparer _statementPreparer;
        /// <summary> The current statement index. </summary>
	    private int _currentStatementIndex;
        /// <summary> The current statement. </summary>
	    private SqliteStatementHandle _currentStatement;

        /// <summary>
        /// ReSharper disable once FieldCanBeMadeReadOnly.Local ReSharper disable once
        /// RedundantDefaultMemberInitializer.
        /// </summary>
        private IObjectCryptEngine _cryptEngine = null;
        /// <summary> The no crypt engine. </summary>
        private static readonly string NO_CRYPT_ENGINE = "Cryptography has not been enabled on this SQLite data reader.";

        /// <summary> The columns. </summary>
        private List<string> _columns = new List<string>();

        /// <summary> The canceled task. </summary>
        private static readonly Task<bool> canceledTask = CreateCanceledTask();
        /// <summary> The false task. </summary>
        private static readonly Task<bool> falseTask = Task.FromResult(false);
        /// <summary> The true task. </summary>
        private static readonly Task<bool> trueTask = Task.FromResult(true);

        /// <summary> True if this object has read. </summary>
        private bool _hasRead;
        /// <summary> Type of the column. </summary>
        private DbType?[] _columnType;
        /// <summary> List of names of the columns. </summary>
        private Dictionary<string, int> _columnNames;

        #region Public static dictionaries and lists

        /// <summary> Type of the sqlite declared type to database. </summary>
        public static readonly Dictionary<string, DbType> SqliteDeclaredTypeToDbType = new Dictionary<string, DbType>(StringComparer.OrdinalIgnoreCase)
        {
            { "bigint", DbType.Int64 },
            { "bit", DbType.Boolean },
            { "blob", DbType.Binary },
            { "bool", DbType.Boolean },
            { "boolean", DbType.Boolean },
            { "datetime", DbType.DateTime },
            { "double", DbType.Double },
            { "float", DbType.Double },
            { "guid", DbType.Guid },
            { "int", DbType.Int32 },
            { "integer", DbType.Int64 },
            { "long", DbType.Int64 },
            { "real", DbType.Double },
            { "single", DbType.Single},
            { "string", DbType.String },
            { "text", DbType.String },
            { "counter", DbType.Int64 },
            { "autoincrement", DbType.Int64 },
            { "identity", DbType.Int64 },
            { "longtext", DbType.String },
            { "longchar", DbType.String },
            { "longvarchar", DbType.String },
            { "tinyint", DbType.Byte },
            { "varchar", DbType.String },
            { "nvarchar", DbType.String },
            { "char", DbType.String },
            { "nchar", DbType.String },
            { "ntext", DbType.String },
            { "yesno", DbType.Boolean },
            { "logical", DbType.Boolean },
            { "numeric", DbType.Decimal },
            { "decimal", DbType.Decimal },
            { "money", DbType.Decimal },
            { "currency", DbType.Decimal },
            { "time", DbType.DateTime },
            { "date", DbType.DateTime },
            { "smalldate", DbType.DateTime },
            { "binary", DbType.Binary },
            { "varbinary", DbType.Binary },
            { "image", DbType.Binary },
            { "general", DbType.Binary },
            { "oleobject", DbType.Binary },
            { "guidblob", DbType.Guid },
            { "uniqueidentifier", DbType.Guid },
            { "memo", DbType.String },
            { "note", DbType.String },
            { "smallint", DbType.Int16 },
            { "timestamp", DbType.DateTime },
            { "encrypted", DbType.Encrypted },
            { "datetimeoffset", DbType.DateTimeOffset },
        };

        /// <summary> Type of the sqlite column type to database. </summary>
        public static readonly Dictionary<SqliteColumnType, DbType> SqliteColumnTypeToDbType = new Dictionary<SqliteColumnType, DbType>()
        {
            { SqliteColumnType.Integer, DbType.Int64 },
            { SqliteColumnType.Blob, DbType.Binary },
            { SqliteColumnType.Text, DbType.String },
            { SqliteColumnType.Double, DbType.Double },
            { SqliteColumnType.Null, DbType.Object }
        };

        /// <summary> The date time formats. </summary>
        public static readonly string[] DateTimeFormats =
        {
            "THHmmssK",
            "THHmmK",
            "HH:mm:ss.FFFFFFFK",
            "HH:mm:ssK",
            "HH:mmK",
            "yyyy-MM-dd HH:mm:ss.FFFFFFFK",
            "yyyy-MM-dd HH:mm:ssK",
            "yyyy-MM-dd HH:mmK",
            "yyyy-MM-ddTHH:mm:ss.FFFFFFFK",
            "yyyy-MM-ddTHH:mmK",
            "yyyy-MM-ddTHH:mm:ssK",
            "yyyyMMddHHmmssK",
            "yyyyMMddHHmmK",
            "yyyyMMddTHHmmssFFFFFFFK",
            "THHmmss",
            "THHmm",
            "HH:mm:ss.FFFFFFF",
            "HH:mm:ss",
            "HH:mm",
            "yyyy-MM-dd HH:mm:ss.FFFFFFF",
            "yyyy-MM-dd HH:mm:ss",
            "yyyy-MM-dd HH:mm",
            "yyyy-MM-ddTHH:mm:ss.FFFFFFF",
            "yyyy-MM-ddTHH:mm",
            "yyyy-MM-ddTHH:mm:ss",
            "yyyyMMddHHmmss",
            "yyyyMMddHHmm",
            "yyyyMMddTHHmmssFFFFFFF",
            "yyyy-MM-dd",
            "yyyyMMdd",
            "yy-MM-dd"
        };

        #endregion

        #endregion

        #region Indexers

        /// <summary>
        /// Indexer to get or set items within this collection using array index syntax.
        /// </summary>
        /// <param name="columnIndex"> Zero-based index of the column. </param>
        /// <returns> The indexed item. </returns>
        public object this[int columnIndex] => GetValue(columnIndex);

        /// <summary>
        /// Indexer to get or set items within this collection using array index syntax.
        /// </summary>
        /// <param name="columnName"> Name of the column. </param>
        /// <returns> The indexed item. </returns>
        public object this[string columnName] => GetValue(GetColumnIndex(columnName));

        #endregion

        #region Properties

        /// <summary>
        /// A list of column names of the recordset referenced by the DataReader - note that the list
        /// will be empty before the first read.
        /// </summary>
        /// <value> The columns. </value>
        public List<string> Columns => _columns;

        /// <summary> Identifier for the last insert row. </summary>
        private long _lastInsertRowId = -1;

        /// <summary> Gets or sets the identifier of the last insert row. </summary>
        /// <value> The identifier of the last insert row. </value>
        public long LastInsertRowId => _lastInsertRowId;

        /// <summary> True to set row identifier. </summary>
        private bool _setRowId;

        /// <summary>
        /// Gets or sets a value indicating whether the row identifier should be set.
        /// </summary>
        /// <value> True if set row identifier, false if not. </value>
        public bool SetRowId
        {
            get => _setRowId;
            set => _setRowId = value;
        }

        /// <summary> Gets or sets a value indicating whether this object is closed. </summary>
        /// <value> True if this object is closed, false if not. </value>
        public bool IsClosed => _command == null;

        /// <summary> Gets or sets the records affected. </summary>
        /// <value> The records affected. </value>
        public int RecordsAffected => _database.Context.sqlite3_total_changes(_database, _command.ForMaintenance) - _startingChanges;

        /// <summary> Gets the number of fields. </summary>
        /// <value> The number of fields. </value>
        public int FieldCount
        {
            get
            {
                VerifyNotDisposed();
                return _hasRead ? _columnType.Length : _database.Context.sqlite3_column_count(_currentStatement);
            }
        }

        /// <summary> Gets a value indicating whether this object has rows. </summary>
        /// <value> True if this object has rows, false if not. </value>
        public bool HasRows
        {
            get
            {
                VerifyNotDisposed();
                return _hasRead;
            }
        }

        /// <summary> Gets or sets the depth. </summary>
        /// <value> The depth. </value>
        public int Depth => throw new NotSupportedException();

        /// <summary> Gets or sets the number of visible fields. </summary>
        /// <value> The number of visible fields. </value>
        public int VisibleFieldCount => FieldCount;

        #endregion

        #region Constructors

        /// <summary> Constructor. </summary>
        /// <param name="command"> The command. </param>
        /// <param name="behavior"> (Optional) The behavior. </param>
        public SqliteDataReader(SqliteCommand command, CommandBehavior behavior = CommandBehavior.Default)
            : this(command, behavior, false)
        { }

        /// <summary> Constructor. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <exception cref="ArgumentException"> Thrown when one or more arguments have unsupported or
        ///     illegal values. </exception>
        /// <param name="command"> The command. </param>
        /// <param name="behavior"> The behavior. </param>
        /// <param name="internallyCreatedReader"> True to internally created reader. </param>
        internal SqliteDataReader(SqliteCommand command, CommandBehavior behavior, bool internallyCreatedReader)
        {
            _command = command ?? throw new ArgumentNullException(nameof(command));
            _database = (command.Connection as SqliteConnection)?.Database ?? throw new ArgumentException("The database cannot be null.", nameof(command));
            _cryptEngine = command._cryptEngine;
            _behavior = behavior;
            _statementPreparer = command.GetStatementPreparer(true);

            _startingChanges = _database.Context.sqlite3_total_changes(_database, command.ForMaintenance);
            _currentStatementIndex = -1;
            if (!internallyCreatedReader)
            {
                NextResult();
            }
        }

        #endregion

        #region Methods

        /// <summary> Creates a new IDataReader. </summary>
        /// <param name="command"> The command. </param>
        /// <param name="behavior"> The behavior. </param>
        /// <returns> An IDataReader. </returns>
        internal static IDataReader Create(SqliteCommand command, CommandBehavior behavior)
        {
            var dataReader = new SqliteDataReader(command, behavior, true);
            dataReader.NextResult();
            return dataReader;
        }

        /// <summary> Creates the asynchronous. </summary>
        /// <param name="command"> The command. </param>
        /// <param name="behavior"> The behavior. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> The asynchronous result that yields the new asynchronous. </returns>
        internal static async Task<IDataReader> CreateAsync(SqliteCommand command, CommandBehavior behavior, CancellationToken cancellationToken)
        {
            var dataReader = new SqliteDataReader(command, behavior, true);
            await dataReader.NextResultAsync(cancellationToken);
            return dataReader;
        }

        /// <summary> Reads this object. </summary>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool Read()
        {
            VerifyNotDisposed();
            return ReadAsyncCore(CancellationToken.None).Result;
        }

        /// <summary> Reads the asynchronous. </summary>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns>
        /// The asynchronous result that yields true if it succeeds, false if it fails.
        /// </returns>
        public Task<bool> ReadAsync(CancellationToken cancellationToken)
        {
            VerifyNotDisposed();

            return ReadAsyncCore(cancellationToken);
        }

        /// <summary> Reads asynchronous core. </summary>
        /// <exception cref="SqliteException"> Thrown when a Sqlite error condition occurs. </exception>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns>
        /// The asynchronous result that yields true if it succeeds, false if it fails.
        /// </returns>
        private Task<bool> ReadAsyncCore(CancellationToken cancellationToken)
        {
            Random random = null;
            while (!cancellationToken.IsCancellationRequested)
            {
                SqliteResultCode resultCode;

                if (!_hasRead)
                {
                    _command?.Connection?.SafeOpen();
                }

                if (_setRowId)
                {
                    _lastInsertRowId = _database.Context.sqlite3_step_return_rowid(_database, _currentStatement, out resultCode);
                }
                else
                {
                    resultCode = _database.Context.sqlite3_step(_currentStatement);
                }

                switch (resultCode)
                {
                    case SqliteResultCode.Done:
                        Reset();
                        return falseTask;

                    case SqliteResultCode.Row:
                        _hasRead = true;
                        if (_columnType == null)
                        {
                            int numColumns = _database.Context.sqlite3_column_count(_currentStatement);
                            if (_columns.Count == 0 && numColumns > 0)
                            {
                                for (int i = 0; i < numColumns; i++)
                                {
                                    _columns.Add(_database.Context.sqlite3_column_name(_currentStatement, i));
                                }
                            }
                            _columnType = new DbType?[numColumns];
                        }
                        return trueTask;

                    case SqliteResultCode.Busy:
                    case SqliteResultCode.Locked:
                    case SqliteResultCode.CantOpen:
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return canceledTask;
                        }
                        if (random == null)
                        {
                            random = new Random();
                        }
                        Thread.Sleep(random.Next(1, 150));
                        break;

                    case SqliteResultCode.Interrupt:
                        return canceledTask;

                    default:
                        throw new SqliteException(resultCode);
                }
            }

            return cancellationToken.IsCancellationRequested ? canceledTask : trueTask;
        }

        /// <summary> Determines if we can next result. </summary>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool NextResult()
        {
            return NextResultAsyncCore(CancellationToken.None).Result;
        }

        /// <summary> Next result asynchronous. </summary>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns>
        /// The asynchronous result that yields true if it succeeds, false if it fails.
        /// </returns>
        public Task<bool> NextResultAsync(CancellationToken cancellationToken)
        {
            return NextResultAsyncCore(cancellationToken);
        }

        /// <summary> Next result asynchronous core. </summary>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns>
        /// The asynchronous result that yields true if it succeeds, false if it fails.
        /// </returns>
        private Task<bool> NextResultAsyncCore(CancellationToken cancellationToken)
        {
            VerifyNotDisposed();

            Reset();
            _currentStatementIndex++;
            _currentStatement = _statementPreparer.Get(_currentStatementIndex, cancellationToken);
            if (_currentStatement == null)
            {
                return falseTask;
            }

            bool success = false;
            try
            {
                for (int i = 0; i < _command.Parameters.Count; i++)
                {
                    SqliteParameter parameter = (SqliteParameter)_command.Parameters[i];
                    string parameterName = parameter.ParameterName;
                    int index;
                    if (parameterName != null)
                    {
                        if (parameterName[0] != '@')
                        {
                            parameterName = "@" + parameterName;
                        }
                        index = _database.Context.sqlite3_bind_parameter_index(_currentStatement, parameterName);
                    }
                    else
                    {
                        index = i + 1;
                    }
                    if (index > 0)
                    {
                        object value = parameter.Value;
                        if (value == null || value.Equals(DBNull.Value))
                        {
                            _database.Context.sqlite3_bind_null(_currentStatement, index).ThrowOnError();
                        }
                        else if (value is int || (value is Enum && Enum.GetUnderlyingType(value.GetType()) == typeof(int)))
                        {
                            _database.Context.sqlite3_bind_int(_currentStatement, index, (int)value).ThrowOnError();
                        }
                        else if (value is bool)
                        {
                            _database.Context.sqlite3_bind_int(_currentStatement, index, ((bool)value) ? 1 : 0).ThrowOnError();
                        }
                        else if (value is string)
                        {
                            BindText(index, (string)value);
                        }
                        else if (value is byte[])
                        {
                            BindBlob(index, (byte[])value);
                        }
                        else if (value is long)
                        {
                            _database.Context.sqlite3_bind_int64(_currentStatement, index, (long)value).ThrowOnError();
                        }
                        else if (value is float)
                        {
                            _database.Context.sqlite3_bind_double(_currentStatement, index, (float)value).ThrowOnError();
                        }
                        else if (value is double)
                        {
                            _database.Context.sqlite3_bind_double(_currentStatement, index, (double)value).ThrowOnError();
                        }
                        else if (value is DateTime)
                        {
                            if (Database.StoreDateTimeAsTicks)
                            {
                                _database.Context.sqlite3_bind_int64(_currentStatement, index, ((DateTime)value).Ticks).ThrowOnError();
                            }
                            else
                            {
                                BindText(index, ToString((DateTime)value));
                            }
                        }
                        else if (value is DateTimeOffset)
                        {
                            BindText(index, ToString((DateTimeOffset)value));
                        }
                        else if (value is Guid)
                        {
                            BindBlob(index, ((Guid)value).ToByteArray());
                        }
                        else if (value is byte)
                        {
                            _database.Context.sqlite3_bind_int(_currentStatement, index, (byte)value).ThrowOnError();
                        }
                        else if (value is short)
                        {
                            _database.Context.sqlite3_bind_int(_currentStatement, index, (short)value).ThrowOnError();
                        }
                        else
                        {
                            BindText(index, Convert.ToString(value, CultureInfo.InvariantCulture));
                        }
                    }
                }

                success = true;
            }
            finally
            {
                if (!success)
                {
                    _database.Context.sqlite3_reset(_currentStatement).ThrowOnError();
                }
            }

            return trueTask;
        }

        /// <summary> Gets schema table. </summary>
        /// <exception cref="NotSupportedException"> Thrown when the requested operation is not supported. </exception>
        /// <returns> The schema table. </returns>
        public DataTable GetSchemaTable()
        {
            throw new NotSupportedException();
        }

        /// <summary> Gets column name. </summary>
        /// <exception cref="ArgumentOutOfRangeException"> Thrown when one or more arguments are outside
        ///     the required range. </exception>
        /// <param name="columnIndex"> The index of the column to retrieve. </param>
        /// <returns> The column name. </returns>
        public string GetColumnName(int columnIndex)
        {
            VerifyHasResult();
            if (columnIndex < 0 || columnIndex > FieldCount)
            {
                throw new ArgumentOutOfRangeException(nameof(columnIndex), "value must be between 0 and {0}.".FormatInvariant(FieldCount - 1));
            }

            return _database.Context.sqlite3_column_name(_currentStatement, columnIndex);
        }

        /// <summary> Gets the values. </summary>
        /// <param name="values"> The values. </param>
        /// <returns> The values. </returns>
        public int GetValues(object[] values)
        {
            VerifyRead();
            int count = Math.Min(values.Length, FieldCount);
            for (int i = 0; i < count; i++)
            {
                values[i] = GetValue(i);
            }
            return count;
        }

        /// <summary> Query if 'columnName' is database null. </summary>
        /// <param name="columnIndex"> The index of the column to retrieve. </param>
        /// <returns> True if database null, false if not. </returns>
        public bool IsDBNull(int columnIndex)
        {
            VerifyRead();
            return _database.Context.sqlite3_column_type(_currentStatement, columnIndex) == SqliteColumnType.Null;
        }

        /// <summary> Query if 'columnName' is database null. </summary>
        /// <param name="columnName"> The name of the column to retrieve. </param>
        /// <returns> True if database null, false if not. </returns>
        public bool IsDBNull(string columnName)
        {
            return IsDBNull(GetColumnIndex(columnName));
        }

        /// <summary> Closes this object. </summary>
        public void Close()
        {
            // NOTE: Dispose calls Close, so we can't put our logic in Dispose(bool) and call Dispose() from this method.
            Reset();
            Utility.Dispose(ref _statementPreparer);

            if (_behavior.HasFlag(CommandBehavior.CloseConnection))
            {
                var dbConnection = _command?.Connection;
                _command?.Dispose();
                dbConnection?.Close();
            }

            _command = null;
        }

        /// <summary> Gets provider specific field type. </summary>
        /// <exception cref="NotSupportedException"> Thrown when the requested operation is not supported. </exception>
        /// <param name="columnIndex"> The index of the column to retrieve. </param>
        /// <returns> The provider specific field type. </returns>
        public Type GetProviderSpecificFieldType(int columnIndex)
        {
            throw new NotSupportedException();
        }

        /// <summary> Gets provider specific value. </summary>
        /// <exception cref="NotSupportedException"> Thrown when the requested operation is not supported. </exception>
        /// <param name="columnIndex"> The index of the column to retrieve. </param>
        /// <returns> The provider specific value. </returns>
        public object GetProviderSpecificValue(int columnIndex)
        {
            throw new NotSupportedException();
        }

        /// <summary> Gets provider specific values. </summary>
        /// <exception cref="NotSupportedException"> Thrown when the requested operation is not supported. </exception>
        /// <param name="values"> The values. </param>
        /// <returns> The provider specific values. </returns>
        public int GetProviderSpecificValues(object[] values)
        {
            throw new NotSupportedException();
        }

        /// <summary> Gets a data. </summary>
        /// <param name="columnIndex"> The index of the column to retrieve. </param>
        /// <returns> The data. </returns>
        public IDataReader GetData(int columnIndex)
        {
            return (IDataReader)GetValue(columnIndex);
        }

        /// <summary> Gets a data. </summary>
        /// <param name="columnName"> The name of the column to retrieve. </param>
        /// <returns> The data. </returns>
        public IDataReader GetData(string columnName)
        {
            return GetData(GetColumnIndex(columnName));
        }

        /// <summary> Resets this object. </summary>
        private void Reset()
        {
            if (_database?.Context != null && _currentStatement != null)
            {
                _database.Context.sqlite3_reset(_currentStatement);
            }
            _currentStatement = null;
            _columnNames = null;
            _columnType = null;
            _hasRead = false;
        }

        /// <summary> Verify has result. </summary>
        /// <exception cref="InvalidOperationException"> Thrown when the requested operation is invalid. </exception>
        private void VerifyHasResult()
        {
            VerifyNotDisposed();
            if (_currentStatement == null)
            {
                throw new InvalidOperationException("There is no current result set.");
            }
        }

        /// <summary> Verify read. </summary>
        /// <exception cref="InvalidOperationException"> Thrown when the requested operation is invalid. </exception>
        private void VerifyRead()
        {
            VerifyHasResult();
            if (!_hasRead)
            {
                throw new InvalidOperationException("Read must be called first.");
            }
        }

        /// <summary> Verify not disposed. </summary>
        /// <exception cref="ObjectDisposedException"> Thrown when a supplied object has been disposed. </exception>
        private void VerifyNotDisposed()
        {
            if (_command == null)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        /// <summary> Bind BLOB. </summary>
        /// <param name="columnIndex"> The index of the column to retrieve. </param>
        /// <param name="blob"> The BLOB. </param>
        private void BindBlob(int columnIndex, byte[] blob)
        {
            _database.Context.sqlite3_bind_blob(_currentStatement, columnIndex, blob).ThrowOnError();
        }

        /// <summary> Bind text. </summary>
        /// <param name="columnIndex"> The index of the column to retrieve. </param>
        /// <param name="text"> The text. </param>
        private void BindText(int columnIndex, string text)
        {
            _database.Context.sqlite3_bind_text(_currentStatement, columnIndex, text).ThrowOnError();
        }

        /// <summary> Convert this object into a string representation. </summary>
        /// <param name="dateTime"> The date time. </param>
        /// <returns> A string that represents this object. </returns>
        private static string ToString(DateTime dateTime)
        {
            // these are the SimpleAdo.Sqlite default format strings (from SqliteConvert.cs)
            string formatString = dateTime.Kind == DateTimeKind.Utc ? "yyyy-MM-dd HH:mm:ss.FFFFFFFK" : "yyyy-MM-dd HH:mm:ss.FFFFFFF";
            return dateTime.ToString(formatString, CultureInfo.InvariantCulture);
        }

        /// <summary> Convert this object into a string representation. </summary>
        /// <param name="dateTime"> The date time. </param>
        /// <returns> A string that represents this object. </returns>
        private static string ToString(DateTimeOffset dateTime)
        {
            // these are the SimpleAdo.Sqlite default format strings (from SqliteConvert.cs)
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss.FFFFFFFK", CultureInfo.InvariantCulture);
        }

        /// <summary> Creates canceled task. </summary>
        /// <returns>
        /// The asynchronous result that yields true if it succeeds, false if it fails.
        /// </returns>
        private static Task<bool> CreateCanceledTask()
        {
            var source = new TaskCompletionSource<bool>();
            source.SetCanceled();
            return source.Task;
        }

        /// <summary> Gets column index. </summary>
        /// <exception cref="IndexOutOfRangeException"> Thrown when the index is outside the required
        ///     range. </exception>
        /// <param name="columnName"> The name of the column to retrieve. </param>
        /// <returns> The column index. </returns>
        public int GetColumnIndex(string columnName)
        {
            VerifyHasResult();

            if (_columnNames == null)
            {
                var columnNames = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < FieldCount; i++)
                {
                    string name = _database.Context.sqlite3_column_name(_currentStatement, i);
                    columnNames[name] = i;
                }
                _columnNames = columnNames;
            }

            if (!_columnNames.TryGetValue(columnName, out int index))
            {
                throw new IndexOutOfRangeException("The column name '{0}' does not exist in the result set.".FormatInvariant(columnName));
            }
            return index;
        }

        /// <summary> Gets data type name. </summary>
        /// <exception cref="NotSupportedException"> Thrown when the requested operation is not supported. </exception>
        /// <param name="columnIndex"> The index of the column to retrieve. </param>
        /// <returns> The data type name. </returns>
        public string GetDataTypeName(int columnIndex)
        {
            throw new NotSupportedException();
        }

        /// <summary> Gets field type. </summary>
        /// <exception cref="NotSupportedException"> Thrown when the requested operation is not supported. </exception>
        /// <param name="columnIndex"> The index of the column to retrieve. </param>
        /// <returns> The field type. </returns>
        public Type GetFieldType(int columnIndex)
        {
            throw new NotSupportedException();
        }

        /// <summary> Gets column type. </summary>
        /// <param name="columnIndex"> The index of the column to retrieve. </param>
        /// <returns> The column type. </returns>
        public SqliteColumnType GetColumnType(int columnIndex)
        {
            return _database.Context.sqlite3_column_type(_currentStatement, columnIndex);
        }

        /// <summary> Gets column type. </summary>
        /// <param name="columnName"> The name of the column to retrieve. </param>
        /// <returns> The column type. </returns>
        public SqliteColumnType GetColumnType(string columnName)
        {
            return GetColumnType(GetColumnIndex(columnName));
        }

        /// <summary> Gets column declared data type. </summary>
        /// <param name="columnIndex"> The index of the column to retrieve. </param>
        /// <param name="toUpper"> (Optional) True to to upper. </param>
        /// <returns> The column declared data type. </returns>
        public string GetColumnDeclaredDataType(int columnIndex, bool toUpper = false)
        {
            string result = _database.Context.sqlite3_column_decltype(_currentStatement, columnIndex);

            result = (String.IsNullOrWhiteSpace(result))
                ? null
                : ((toUpper) ? result.ToUpper() : result.ToLower()).Trim();

            return result;
        }

        /// <summary> Gets column declared data type. </summary>
        /// <param name="columnName"> The name of the column to retrieve. </param>
        /// <param name="toUpper"> (Optional) True to to upper. </param>
        /// <returns> The column declared data type. </returns>
        public string GetColumnDeclaredDataType(string columnName, bool toUpper = false)
        {
            return GetColumnDeclaredDataType(GetColumnIndex(columnName), toUpper);
        }

        /// <summary> Gets column database type. </summary>
        /// <exception cref="NotSupportedException"> Thrown when the requested operation is not supported. </exception>
        /// <param name="columnIndex"> The index of the column to retrieve. </param>
        /// <returns> The column database type. </returns>
        public DbType GetColumnDbType(int columnIndex)
        {
            // determine (and cache) the declared type of the column (e.g., from the SQL schema)
            DbType result;
            if (_columnType[columnIndex].HasValue)
            {
                result = _columnType[columnIndex].Value;
            }
            else
            {
                string declType = _database.Context.sqlite3_column_decltype(_currentStatement, columnIndex);
                if (!String.IsNullOrWhiteSpace(declType))
                {
                    if (!SqliteDeclaredTypeToDbType.TryGetValue(declType.ToLower().Trim(), out result))
                    {
                        throw new NotSupportedException("The declared data type name '{0}' is not supported.".FormatInvariant(declType));
                    }
                }
                else
                {
                    result = DbType.Object;
                }
                _columnType[columnIndex] = result;
            }
            if (result == DbType.Object)
            {
                result = SqliteColumnTypeToDbType[GetColumnType(columnIndex)];
            }
            return result;
        }

        /// <summary> Gets column database type. </summary>
        /// <param name="columnName"> The name of the column to retrieve. </param>
        /// <returns> The column database type. </returns>
        public DbType GetColumnDbType(string columnName)
        {
            return GetColumnDbType(GetColumnIndex(columnName));
        }

        /// <summary> Gets a value. </summary>
        /// <exception cref="ArgumentOutOfRangeException"> Thrown when one or more arguments are outside
        ///     the required range. </exception>
        /// <exception cref="InvalidOperationException"> Thrown when the requested operation is invalid. </exception>
        /// <param name="columnIndex"> The index of the column to retrieve. </param>
        /// <returns> The value. </returns>
        public object GetValue(int columnIndex)
        {
            VerifyRead();
            if (columnIndex < 0 || columnIndex > FieldCount)
            {
                throw new ArgumentOutOfRangeException(nameof(columnIndex), "value must be between 0 and {0}.".FormatInvariant(FieldCount - 1));
            }

            DbType dbType = GetColumnDbType(columnIndex);
            var sqliteType = GetColumnType(columnIndex);

            switch (sqliteType)
            {
                case SqliteColumnType.Null:
                    return DBNull.Value;

                case SqliteColumnType.Blob:
                    int byteCount = _database.Context.sqlite3_column_bytes(_currentStatement, columnIndex);
                    byte[] bytes = (byteCount > 0) ? _database.Context.sqlite3_column_blob(_currentStatement, columnIndex) : new byte[0];
                    return dbType == DbType.Guid && byteCount == 16 ? new Guid(bytes) : (object)bytes;

                case SqliteColumnType.Double:
                    double doubleValue = _database.Context.sqlite3_column_double(_currentStatement, columnIndex);
                    return dbType == DbType.Single ? (float)doubleValue : (object)doubleValue;

                case SqliteColumnType.Integer:
                    long integerValue = _database.Context.sqlite3_column_int64(_currentStatement, columnIndex);
                    return dbType == DbType.Int32 ? (int)integerValue :
                        dbType == DbType.Boolean ? integerValue != 0 :
                        dbType == DbType.Int16 ? (short)integerValue :
                        dbType == DbType.Byte ? (byte)integerValue :
                        dbType == DbType.Single ? (float)integerValue :
                        dbType == DbType.Double ? (double)integerValue :
                        (Database.StoreDateTimeAsTicks && dbType == DbType.DateTime) ? new DateTime(integerValue) :
                        (object)integerValue;

                case SqliteColumnType.Text:
                    string stringValue = _database.Context.sqlite3_column_text(_currentStatement, columnIndex);
                    return dbType == DbType.DateTime
                        ? (Database.StoreDateTimeAsTicks
                            ? (Int64.TryParse(stringValue, out long dtTicks) && (dtTicks > 0) ? new DateTime(dtTicks) : DateTime.MinValue)
                            : DateTime.ParseExact(stringValue, DateTimeFormats, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None))
                        : dbType == DbType.DateTimeOffset
                            ? DateTimeOffset.ParseExact(stringValue, DateTimeFormats, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None)
                            : (object)stringValue;

                default:
                    throw new InvalidOperationException();
            }
        }

        /// <summary> Gets a value. </summary>
        /// <param name="columnName"> The name of the column to retrieve. </param>
        /// <returns> The value. </returns>
        public object GetValue(string columnName)
        {
            return GetValue(GetColumnIndex(columnName));
        }

        #region Get Decrytped methods

        /// <summary> Retrieves the decrypted value of a column as the specified type. </summary>
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        /// <exception cref="DbNullException"> Thrown when a Database Null error condition occurs. </exception>
        /// <typeparam name="T"> The type of the object or value to be retrieved. </typeparam>
        /// <param name="columnIndex"> The index of the column to retrieve. </param>
        /// <param name="dbNullHandling"> (Optional) Determines how table column values of NULL are
        ///     handled. </param>
        /// <returns> Specified type. </returns>
        public T GetDecrypted<T>(int columnIndex, DbNullHandling dbNullHandling = DbNullHandling.ThrowDbNullException)
        {
            T result = default(T);

            if (_cryptEngine == null)
            {
                throw new Exception(NO_CRYPT_ENGINE);
            }
            if (IsDBNull(columnIndex))
            {
                if (dbNullHandling == DbNullHandling.ThrowDbNullException)
                {
                    throw new DbNullException();
                }
            }
            else
            {
                if (GetColumnType(columnIndex) != SqliteColumnType.Text)
                {
                    throw new Exception("The column value is not of the correct data type.");
                }
                var encrypted = GetString(columnIndex);
                if (String.IsNullOrWhiteSpace(encrypted))
                {
                    throw new Exception("The column value to be decrypted is empty.");
                }
                else
                {
                    result = _cryptEngine.DecryptObject<T>(encrypted);
                }
            }

            return result;
        }

        /// <summary> Retrieves the decrypted value of a column as the specified type. </summary>
        /// <typeparam name="T"> The type of the object or value to be retrieved. </typeparam>
        /// <param name="columnName"> The name of the column to retrieve. </param>
        /// <param name="dbNullHandling"> (Optional) Determines how table column values of NULL are
        ///     handled. </param>
        /// <returns> Specified type. </returns>
        public T GetDecrypted<T>(string columnName, DbNullHandling dbNullHandling = DbNullHandling.ThrowDbNullException)
        {
            return GetDecrypted<T>(GetColumnIndex(columnName), dbNullHandling);
        }

        /// <summary> Tries to decrypt the column as a value of the specified type. </summary>
        /// <typeparam name="T"> The type of the object or value to be retrieved. </typeparam>
        /// <param name="columnIndex"> The index of the column to retrieve. </param>
        /// <param name="value"> [out] The decrypted value. </param>
        /// <param name="failOnDbNull"> Fail when column is NULL? (Else returns default value) </param>
        /// <returns> bool. </returns>
        public bool TryDecrypt<T>(int columnIndex, out T value, bool failOnDbNull)
        {
            // ReSharper disable once RedundantAssignment
            var result = false;
            value = default(T);

            if (IsDBNull(columnIndex))
            {
                result = !failOnDbNull;
            }
            else
            {
                if ((_cryptEngine == null) || (GetColumnType(columnIndex) != SqliteColumnType.Text))
                {
                    // ReSharper disable once RedundantAssignment
                    result = false;
                }
                else
                {
                    var encrypted = GetString(columnIndex);
                    if (String.IsNullOrWhiteSpace(encrypted))
                    {
                        result = !failOnDbNull;
                    }
                    else
                    {
                        try
                        {
                            value = _cryptEngine.DecryptObject<T>(encrypted);
                            result = true;
                        }
                        catch (Exception)
                        {
                            result = false;
                        }
                    }
                }
            }

            return result;
        }

        /// <summary> Tries to decrypt the column as a value of the specified type. </summary>
        /// <typeparam name="T"> The type of the object or value to be retrieved. </typeparam>
        /// <param name="columnName"> The name of the column to retrieve. </param>
        /// <param name="value"> [out] The decrypted value. </param>
        /// <param name="failOnDbNull"> Fail when column is NULL? (Else returns default value) </param>
        /// <returns> bool. </returns>
        public bool TryDecrypt<T>(string columnName, out T value, bool failOnDbNull)
        {
            return TryDecrypt(GetColumnIndex(columnName), out value, failOnDbNull);
        }

        #endregion

        #region Get Boolean methods

        /// <summary> Retrieves the column as a boolean value. </summary>
        /// <param name="columnIndex"> The index of the column to retrieve. </param>
        /// <returns> bool. </returns>
        public bool GetBoolean(int columnIndex)
        {
            return Convert.ToBoolean(GetValue(columnIndex), CultureInfo.CurrentCulture);
        }

        /// <summary> Retrieves the column as a boolean value. </summary>
        /// <param name="columnName"> The name of the column to retrieve. </param>
        /// <returns> bool. </returns>
        public bool GetBoolean(string columnName)
        {
            return GetBoolean(GetColumnIndex(columnName));
        }

        /// <summary> Tries to decrypt the column value as a Boolean value. </summary>
        /// <param name="columnIndex"> The index of the column to retrieve. </param>
        /// <param name="value"> [out] The decrypted value. </param>
        /// <param name="failOnDbNull"> (Optional) Fail when column is NULL? (Else returns default value) </param>
        /// <returns> bool. </returns>
        public bool TryDecryptBoolean(int columnIndex, out bool value, bool failOnDbNull = false)
        {
            return TryDecrypt(columnIndex, out value, failOnDbNull);
        }

        /// <summary> Tries to decrypt the column value as a Boolean value. </summary>
        /// <param name="columnName"> The name of the column to retrieve. </param>
        /// <param name="value"> [out] The decrypted value. </param>
        /// <param name="failOnDbNull"> (Optional) Fail when column is NULL? (Else returns default value) </param>
        /// <returns> bool. </returns>
        public bool TryDecryptBoolean(string columnName, out bool value, bool failOnDbNull = false)
        {
            return TryDecrypt(GetColumnIndex(columnName), out value, failOnDbNull);
        }

        #endregion

        #region Get Byte methods

        /// <summary> Retrieves the column as a single byte value. </summary>
        /// <param name="columnIndex"> The index of the column to retrieve. </param>
        /// <returns> byte. </returns>
        public byte GetByte(int columnIndex)
        {
            return (byte)GetValue(columnIndex);
        }

        /// <summary> Retrieves the column as a single byte value. </summary>
        /// <param name="columnName"> The name of the column to retrieve. </param>
        /// <returns> byte. </returns>
        public byte GetByte(string columnName)
        {
            return GetByte(GetColumnIndex(columnName));
        }

        /// <summary> Tries to decrypt the column value as a byte value. </summary>
        /// <param name="columnIndex"> The index of the column to retrieve. </param>
        /// <param name="value"> [out] The decrypted value. </param>
        /// <param name="failOnDbNull"> (Optional) Fail when column is NULL? (Else returns default value) </param>
        /// <returns> bool. </returns>
        public bool TryDecryptByte(int columnIndex, out byte value, bool failOnDbNull = true)
        {
            return TryDecrypt(columnIndex, out value, failOnDbNull);
        }

        /// <summary> Tries to decrypt the column value as a byte value. </summary>
        /// <param name="columnName"> The name of the column to retrieve. </param>
        /// <param name="value"> [out] The decrypted value. </param>
        /// <param name="failOnDbNull"> (Optional) Fail when column is NULL? (Else returns default value) </param>
        /// <returns> bool. </returns>
        public bool TryDecryptByte(string columnName, out byte value, bool failOnDbNull = true)
        {
            return TryDecrypt(GetColumnIndex(columnName), out value, failOnDbNull);
        }

        #endregion

        #region Get Byte array methods

        /// <summary> Retrieves a column as an array of bytes (blob) </summary>
        /// <exception cref="InvalidCastException"> Thrown when an object cannot be cast to a required
        ///     type. </exception>
        /// <param name="columnIndex"> The index of the column to retrieve. </param>
        /// <returns> A byte array containing the contents of the specified column. </returns>
        public byte[] GetBytes(int columnIndex)
        {
            var sqliteType = _database.Context.sqlite3_column_type(_currentStatement, columnIndex);
            if (sqliteType == SqliteColumnType.Null)
            {
                return null;
            }
            else if (sqliteType != SqliteColumnType.Blob)
            {
                throw new InvalidCastException("Cannot convert '{0}' to bytes.".FormatInvariant(sqliteType));
            }
            return _database.Context.sqlite3_column_blob(_currentStatement, columnIndex);
        }

        /// <summary> Retrieves a column as an array of bytes (blob) </summary>
        /// <param name="columnName"> The name of the column to retrieve. </param>
        /// <returns> A byte array containing the contents of the specified column. </returns>
        public byte[] GetBytes(string columnName)
        {
            return GetBytes(GetColumnIndex(columnName));
        }

        /// <summary> Tries to decrypt the column value as a byte array. </summary>
        /// <param name="columnIndex"> The index of the column to retrieve. </param>
        /// <param name="value"> [out] The decrypted value. </param>
        /// <param name="failOnDbNull"> (Optional) Fail when column is NULL? (Else returns default value) </param>
        /// <returns> bool. </returns>
        public bool TryDecryptBytes(int columnIndex, out byte[] value, bool failOnDbNull = false)
        {
            return TryDecrypt(columnIndex, out value, failOnDbNull);
        }

        /// <summary> Tries to decrypt the column value as a byte array. </summary>
        /// <param name="columnName"> The name of the column to retrieve. </param>
        /// <param name="value"> [out] The decrypted value. </param>
        /// <param name="failOnDbNull"> (Optional) Fail when column is NULL? (Else returns default value) </param>
        /// <returns> bool. </returns>
        public bool TryDecryptBytes(string columnName, out byte[] value, bool failOnDbNull = false)
        {
            return TryDecrypt(GetColumnIndex(columnName), out value, failOnDbNull);
        }

        #endregion

        #region Get Char methods

        /// <summary> Returns the column as a single character. </summary>
        /// <param name="columnIndex"> The index of the column to retrieve. </param>
        /// <returns> char. </returns>
        public char GetChar(int columnIndex)
        {
            return (char)GetValue(columnIndex);
        }

        /// <summary> Returns the column as a single character. </summary>
        /// <param name="columnName"> The name of the column to retrieve. </param>
        /// <returns> char. </returns>
        public char GetChar(string columnName)
        {
            return GetChar(GetColumnIndex(columnName));
        }

        /// <summary> Tries to decrypt the column value as a char value. </summary>
        /// <param name="columnIndex"> The index of the column to retrieve. </param>
        /// <param name="value"> [out] The decrypted value. </param>
        /// <param name="failOnDbNull"> (Optional) Fail when column is NULL? (Else returns default value) </param>
        /// <returns> bool. </returns>
        public bool TryDecryptChar(int columnIndex, out char value, bool failOnDbNull = true)
        {
            return TryDecrypt(columnIndex, out value, failOnDbNull);
        }

        /// <summary> Tries to decrypt the column value as a char value. </summary>
        /// <param name="columnName"> The name of the column to retrieve. </param>
        /// <param name="value"> [out] The decrypted value. </param>
        /// <param name="failOnDbNull"> (Optional) Fail when column is NULL? (Else returns default value) </param>
        /// <returns> bool. </returns>
        public bool TryDecryptChar(string columnName, out char value, bool failOnDbNull = true)
        {
            return TryDecrypt(GetColumnIndex(columnName), out value, failOnDbNull);
        }

        #endregion

        #region Get Guid methods

        /// <summary> Returns the column as a Guid. </summary>
        /// <param name="columnIndex"> The index of the column to retrieve. </param>
        /// <returns> Guid. </returns>
        public Guid GetGuid(int columnIndex)
        {
            return (Guid)GetValue(columnIndex);
        }

        /// <summary> Returns the column as a Guid. </summary>
        /// <param name="columnName"> The name of the column to retrieve. </param>
        /// <returns> Guid. </returns>
        public Guid GetGuid(string columnName)
        {
            return GetGuid(GetColumnIndex(columnName));
        }

        /// <summary> Tries to decrypt the column value as a Guid value. </summary>
        /// <param name="columnIndex"> The index of the column to retrieve. </param>
        /// <param name="value"> [out] The decrypted value. </param>
        /// <param name="failOnDbNull"> (Optional) Fail when column is NULL? (Else returns default value) </param>
        /// <returns> bool. </returns>
        public bool TryDecryptGuid(int columnIndex, out Guid value, bool failOnDbNull = true)
        {
            return TryDecrypt(columnIndex, out value, failOnDbNull);
        }

        /// <summary> Tries to decrypt the column value as a Guid value. </summary>
        /// <param name="columnName"> The name of the column to retrieve. </param>
        /// <param name="value"> [out] The decrypted value. </param>
        /// <param name="failOnDbNull"> (Optional) Fail when column is NULL? (Else returns default value) </param>
        /// <returns> bool. </returns>
        public bool TryDecryptGuid(string columnName, out Guid value, bool failOnDbNull = true)
        {
            return TryDecrypt(GetColumnIndex(columnName), out value, failOnDbNull);
        }

        #endregion

        #region Get Int16 methods

        /// <summary> Returns the column as a short. </summary>
        /// <param name="columnIndex"> The index of the column to retrieve. </param>
        /// <returns> Int16. </returns>
        public short GetInt16(int columnIndex)
        {
            return (short)GetValue(columnIndex);
        }

        /// <summary> Returns the column as a short. </summary>
        /// <param name="columnName"> The name of the column to retrieve. </param>
        /// <returns> Int16. </returns>
        public Int16 GetInt16(string columnName)
        {
            return GetInt16(GetColumnIndex(columnName));
        }

        /// <summary> Tries to decrypt the column value as a Int16 value. </summary>
        /// <param name="columnIndex"> The index of the column to retrieve. </param>
        /// <param name="value"> [out] The decrypted value. </param>
        /// <param name="failOnDbNull"> (Optional) Fail when column is NULL? (Else returns default value) </param>
        /// <returns> bool. </returns>
        public bool TryDecryptInt16(int columnIndex, out Int16 value, bool failOnDbNull = true)
        {
            return TryDecrypt(columnIndex, out value, failOnDbNull);
        }

        /// <summary> Tries to decrypt the column value as a Int16 value. </summary>
        /// <param name="columnName"> The name of the column to retrieve. </param>
        /// <param name="value"> [out] The decrypted value. </param>
        /// <param name="failOnDbNull"> (Optional) Fail when column is NULL? (Else returns default value) </param>
        /// <returns> bool. </returns>
        public bool TryDecryptInt16(string columnName, out Int16 value, bool failOnDbNull = true)
        {
            return TryDecrypt(GetColumnIndex(columnName), out value, failOnDbNull);
        }

        #endregion

        #region Get Int32 methods

        /// <summary> Retrieves the column as an int. </summary>
        /// <param name="columnIndex"> The index of the column to retrieve. </param>
        /// <returns> Int32. </returns>
        public int GetInt32(int columnIndex)
        {
            object value = GetValue(columnIndex);
            switch (value)
            {
                case short _:
                    return (short)value;
                case long _:
                    return checked((int)(long)value);
            }
            return (int)value;
        }

        /// <summary> Retrieves the column as an int. </summary>
        /// <param name="columnName"> The name of the column to retrieve. </param>
        /// <returns> Int32. </returns>
        public Int32 GetInt32(string columnName)
        {
            return GetInt32(GetColumnIndex(columnName));
        }

        /// <summary> Tries to decrypt the column value as a Int32 value. </summary>
        /// <param name="columnIndex"> The index of the column to retrieve. </param>
        /// <param name="value"> [out] The decrypted value. </param>
        /// <param name="failOnDbNull"> (Optional) Fail when column is NULL? (Else returns default value) </param>
        /// <returns> bool. </returns>
        public bool TryDecryptInt32(int columnIndex, out Int32 value, bool failOnDbNull = true)
        {
            return TryDecrypt(columnIndex, out value, failOnDbNull);
        }

        /// <summary> Tries to decrypt the column value as a Int32 value. </summary>
        /// <param name="columnName"> The name of the column to retrieve. </param>
        /// <param name="value"> [out] The decrypted value. </param>
        /// <param name="failOnDbNull"> (Optional) Fail when column is NULL? (Else returns default value) </param>
        /// <returns> bool. </returns>
        public bool TryDecryptInt32(string columnName, out Int32 value, bool failOnDbNull = true)
        {
            return TryDecrypt(GetColumnIndex(columnName), out value, failOnDbNull);
        }

        #endregion

        #region Get Int64 methods

        /// <summary> Retrieves the column as a long. </summary>
        /// <param name="columnIndex"> The index of the column to retrieve. </param>
        /// <returns> Int64. </returns>
        public long GetInt64(int columnIndex)
        {
            object value = GetValue(columnIndex);
            if (value is short s)
            {
                return s;
            }
            if (value is int iTemp)
            {
                return iTemp;
            }
            return (long)value;
        }

        /// <summary> Retrieves the column as a long. </summary>
        /// <param name="columnName"> The name of the column to retrieve. </param>
        /// <returns> Int64. </returns>
        public Int64 GetInt64(string columnName)
        {
            return GetInt64(GetColumnIndex(columnName));
        }

        /// <summary> Tries to decrypt the column value as a Int64 value. </summary>
        /// <param name="columnIndex"> The index of the column to retrieve. </param>
        /// <param name="value"> [out] The decrypted value. </param>
        /// <param name="failOnDbNull"> (Optional) Fail when column is NULL? (Else returns default value) </param>
        /// <returns> bool. </returns>
        public bool TryDecryptInt64(int columnIndex, out Int64 value, bool failOnDbNull = true)
        {
            return TryDecrypt(columnIndex, out value, failOnDbNull);
        }

        /// <summary> Tries to decrypt the column value as a Int64 value. </summary>
        /// <param name="columnName"> The name of the column to retrieve. </param>
        /// <param name="value"> [out] The decrypted value. </param>
        /// <param name="failOnDbNull"> (Optional) Fail when column is NULL? (Else returns default value) </param>
        /// <returns> bool. </returns>
        public bool TryDecryptInt64(string columnName, out Int64 value, bool failOnDbNull = true)
        {
            return TryDecrypt(GetColumnIndex(columnName), out value, failOnDbNull);
        }

        #endregion

        #region Get DateTime methods

        /// <summary> Retrieve the column as a date/time value. </summary>
        /// <param name="columnIndex"> The index of the column to retrieve. </param>
        /// <returns> DateTime. </returns>
        public DateTime GetDateTime(int columnIndex)
        {
            return (DateTime)GetValue(columnIndex);
        }

        /// <summary> Retrieve the column as a date/time value. </summary>
        /// <param name="columnName"> The name of the column to retrieve. </param>
        /// <returns> DateTime. </returns>
        public DateTime GetDateTime(string columnName)
        {
            return GetDateTime(GetColumnIndex(columnName));
        }

        /// <summary> Tries to decrypt the column value as a DateTime value. </summary>
        /// <param name="columnIndex"> The index of the column to retrieve. </param>
        /// <param name="value"> [out] The decrypted value. </param>
        /// <param name="failOnDbNull"> (Optional) Fail when column is NULL? (Else returns default value) </param>
        /// <returns> bool. </returns>
        public bool TryDecryptDateTime(int columnIndex, out DateTime value, bool failOnDbNull = true)
        {
            return TryDecrypt(columnIndex, out value, failOnDbNull);
        }

        /// <summary> Tries to decrypt the column value as a DateTime value. </summary>
        /// <param name="columnName"> The name of the column to retrieve. </param>
        /// <param name="value"> [out] The decrypted value. </param>
        /// <param name="failOnDbNull"> (Optional) Fail when column is NULL? (Else returns default value) </param>
        /// <returns> bool. </returns>
        public bool TryDecryptDateTime(string columnName, out DateTime value, bool failOnDbNull = true)
        {
            return TryDecrypt(GetColumnIndex(columnName), out value, failOnDbNull);
        }

        #endregion

        #region Get DateTimeOffset methods

        /// <summary> Retrieve the column as a DateTimeOffset value. </summary>
        /// <param name="columnIndex"> The index of the column to retrieve. </param>
        /// <returns> DateTimeOffset. </returns>
        public DateTimeOffset GetDateTimeOffset(int columnIndex)
        {
            return (DateTimeOffset)GetValue(columnIndex);
        }

        /// <summary> Retrieve the column as a DateTimeOffset value. </summary>
        /// <param name="columnName"> The name of the column to retrieve. </param>
        /// <returns> DateTimeOffset. </returns>
        public DateTimeOffset GetDateTimeOffset(string columnName)
        {
            return GetDateTimeOffset(GetColumnIndex(columnName));
        }

        /// <summary> Tries to decrypt the column value as a DateTimeOffset value. </summary>
        /// <param name="columnIndex"> The index of the column to retrieve. </param>
        /// <param name="value"> [out] The decrypted value. </param>
        /// <param name="failOnDbNull"> (Optional) Fail when column is NULL? (Else returns default value) </param>
        /// <returns> bool. </returns>
        public bool TryDecryptDateTimeOffset(int columnIndex, out DateTimeOffset value, bool failOnDbNull = true)
        {
            return TryDecrypt(columnIndex, out value, failOnDbNull);
        }

        /// <summary> Tries to decrypt the column value as a DateTimeOffset value. </summary>
        /// <param name="columnName"> The name of the column to retrieve. </param>
        /// <param name="value"> [out] The decrypted value. </param>
        /// <param name="failOnDbNull"> (Optional) Fail when column is NULL? (Else returns default value) </param>
        /// <returns> bool. </returns>
        public bool TryDecryptDateTimeOffset(string columnName, out DateTimeOffset value, bool failOnDbNull = true)
        {
            return TryDecrypt(GetColumnIndex(columnName), out value, failOnDbNull);
        }

        #endregion

        #region Get String methods

        /// <summary> Retrieves the column as a string. </summary>
        /// <param name="columnIndex"> The index of the column to retrieve. </param>
        /// <returns> string. </returns>
        public string GetString(int columnIndex)
        {
            return (string)GetValue(columnIndex);
        }

        /// <summary> Retrieves the column as a string. </summary>
        /// <param name="columnName"> The name of the column to retrieve. </param>
        /// <returns> string. </returns>
        public string GetString(string columnName)
        {
            return GetString(GetColumnIndex(columnName));
        }

        /// <summary> Tries to decrypt the column value as a string value. </summary>
        /// <param name="columnIndex"> The index of the column to retrieve. </param>
        /// <param name="value"> [out] The decrypted value. </param>
        /// <param name="failOnDbNull"> (Optional) Fail when column is NULL? (Else returns default value) </param>
        /// <returns> bool. </returns>
        public bool TryDecryptString(int columnIndex, out string value, bool failOnDbNull = false)
        {
            return TryDecrypt(columnIndex, out value, failOnDbNull);
        }

        /// <summary> Tries to decrypt the column value as a string value. </summary>
        /// <param name="columnName"> The name of the column to retrieve. </param>
        /// <param name="value"> [out] The decrypted value. </param>
        /// <param name="failOnDbNull"> (Optional) Fail when column is NULL? (Else returns default value) </param>
        /// <returns> bool. </returns>
        public bool TryDecryptString(string columnName, out string value, bool failOnDbNull = false)
        {
            return TryDecrypt(GetColumnIndex(columnName), out value, failOnDbNull);
        }

        #endregion

        #region Get Decimal methods

        /// <summary> Retrieve the column as a decimal value. </summary>
        /// <param name="columnIndex"> The index of the column to retrieve. </param>
        /// <returns> decimal. </returns>
        public decimal GetDecimal(int columnIndex)
        {
            return Decimal.Parse(GetValue(columnIndex).ToString(), NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
        }

        /// <summary> Retrieve the column as a decimal value. </summary>
        /// <param name="columnName"> The name of the column to retrieve. </param>
        /// <returns> decimal. </returns>
        public decimal GetDecimal(string columnName)
        {
            return GetDecimal(GetColumnIndex(columnName));
        }

        /// <summary> Tries to decrypt the column value as a Decimal value. </summary>
        /// <param name="columnIndex"> The index of the column to retrieve. </param>
        /// <param name="value"> [out] The decrypted value. </param>
        /// <param name="failOnDbNull"> (Optional) Fail when column is NULL? (Else returns default value) </param>
        /// <returns> bool. </returns>
        public bool TryDecryptDecimal(int columnIndex, out Decimal value, bool failOnDbNull = true)
        {
            return TryDecrypt(columnIndex, out value, failOnDbNull);
        }

        /// <summary> Tries to decrypt the column value as a Decimal value. </summary>
        /// <param name="columnName"> The name of the column to retrieve. </param>
        /// <param name="value"> [out] The decrypted value. </param>
        /// <param name="failOnDbNull"> (Optional) Fail when column is NULL? (Else returns default value) </param>
        /// <returns> bool. </returns>
        public bool TryDecryptDecimal(string columnName, out Decimal value, bool failOnDbNull = true)
        {
            return TryDecrypt(GetColumnIndex(columnName), out value, failOnDbNull);
        }

        #endregion

        #region Get Double methods

        /// <summary> Returns the column as a double. </summary>
        /// <param name="columnIndex"> The index of the column to retrieve. </param>
        /// <returns> double. </returns>
        public double GetDouble(int columnIndex)
        {
            object value = GetValue(columnIndex);
            if (value is float f)
            {
                return f;
            }
            return (double)value;
        }

        /// <summary> Returns the column as a double. </summary>
        /// <param name="columnName"> The name of the column to retrieve. </param>
        /// <returns> double. </returns>
        public double GetDouble(string columnName)
        {
            return GetDouble(GetColumnIndex(columnName));
        }

        /// <summary> Tries to decrypt the column value as a double value. </summary>
        /// <param name="columnIndex"> The index of the column to retrieve. </param>
        /// <param name="value"> [out] The decrypted value. </param>
        /// <param name="failOnDbNull"> (Optional) Fail when column is NULL? (Else returns default value) </param>
        /// <returns> bool. </returns>
        public bool TryDecryptDouble(int columnIndex, out double value, bool failOnDbNull = true)
        {
            return TryDecrypt(columnIndex, out value, failOnDbNull);
        }

        /// <summary> Tries to decrypt the column value as a double value. </summary>
        /// <param name="columnName"> The name of the column to retrieve. </param>
        /// <param name="value"> [out] The decrypted value. </param>
        /// <param name="failOnDbNull"> (Optional) Fail when column is NULL? (Else returns default value) </param>
        /// <returns> bool. </returns>
        public bool TryDecryptDouble(string columnName, out double value, bool failOnDbNull = true)
        {
            return TryDecrypt(GetColumnIndex(columnName), out value, failOnDbNull);
        }

        #endregion

        #region Get Float methods

        /// <summary> Returns a column as a float value. </summary>
        /// <param name="columnIndex"> The index of the column to retrieve. </param>
        /// <returns> float. </returns>
        public float GetFloat(int columnIndex)
        {
            return (float)GetValue(columnIndex);
        }

        /// <summary> Returns a column as a float value. </summary>
        /// <param name="columnName"> The name of the column to retrieve. </param>
        /// <returns> float. </returns>
        public float GetFloat(string columnName)
        {
            return GetFloat(GetColumnIndex(columnName));
        }

        /// <summary> Tries to decrypt the column value as a float value. </summary>
        /// <param name="columnIndex"> The index of the column to retrieve. </param>
        /// <param name="value"> [out] The decrypted value. </param>
        /// <param name="failOnDbNull"> (Optional) Fail when column is NULL? (Else returns default value) </param>
        /// <returns> bool. </returns>
        public bool TryDecryptFloat(int columnIndex, out float value, bool failOnDbNull = true)
        {
            return TryDecrypt(columnIndex, out value, failOnDbNull);
        }

        /// <summary> Tries to decrypt the column value as a float value. </summary>
        /// <param name="columnName"> The name of the column to retrieve. </param>
        /// <param name="value"> [out] The decrypted value. </param>
        /// <param name="failOnDbNull"> (Optional) Fail when column is NULL? (Else returns default value) </param>
        /// <returns> bool. </returns>
        public bool TryDecryptFloat(string columnName, out float value, bool failOnDbNull = true)
        {
            return TryDecrypt(GetColumnIndex(columnName), out value, failOnDbNull);
        }

        #endregion

        /// <summary>
        /// Releases the unmanaged resources used by the SimpleAdo.Sqlite.SqliteDataReader and optionally
        /// releases the managed resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            // ReSharper disable once GCSuppressFinalizeForTypeWithoutDestructor
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the SimpleAdo.Sqlite.SqliteDataReader and optionally
        /// releases the managed resources.
        /// </summary>
        /// <param name="disposing"> True to release both managed and unmanaged resources; false to
        ///     release only unmanaged resources. </param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                Close();
            }
            _database = null;
            _columns = null;
            _cryptEngine = null;
        }

        #endregion
    }
}
