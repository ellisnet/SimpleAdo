using System;

namespace SimpleAdo
{
    /// <summary> Interface for database connection. </summary>
    public interface IDbConnection : IDisposable
    {
        /// <summary> Gets or sets the connection string. </summary>
        /// <value> The connection string. </value>
        string ConnectionString { get; set; }

        /// <summary> Gets the connection timeout. </summary>
        /// <value> The connection timeout. </value>
        int ConnectionTimeout { get; }

        /// <summary> Gets the state. </summary>
        /// <value> The state. </value>
        ConnectionState State { get; }

        /// <summary> Begins a transaction. </summary>
        /// <returns> An IDbTransaction. </returns>
        IDbTransaction BeginTransaction();

        /// <summary> Begins a transaction. </summary>
        /// <param name="il"> The il. </param>
        /// <returns> An IDbTransaction. </returns>
        IDbTransaction BeginTransaction(IsolationLevel il);

        /// <summary> Closes this object. </summary>
        void Close();
        /// <summary> Safe close. </summary>
        void SafeClose();

        /// <summary> Creates a command. </summary>
        /// <param name="forMaintenance"> (Optional) True to for maintenance. </param>
        /// <returns> The new command. </returns>
        IDbCommand CreateCommand(bool forMaintenance = false);
        /// <summary> Opens this object. </summary>
        void Open();
        /// <summary> Queries if a given safe open. </summary>
        void SafeOpen();

        /// <summary> Gets database schema version. </summary>
        /// <returns> The database schema version. </returns>
        long GetDatabaseSchemaVersion();

        /// <summary> Sets database schema version. </summary>
        /// <param name="version"> The version. </param>
        void SetDatabaseSchemaVersion(long version);

        /// <summary> Queries if a given table exists. </summary>
        /// <param name="tableName"> Name of the table. </param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        bool TableExists(string tableName);

        /// <summary> Gets table column list. </summary>
        /// <param name="tableName"> Name of the table. </param>
        /// <returns> An array of i column information. </returns>
        IColumnInfo[] GetTableColumnList(string tableName);
    }
}
