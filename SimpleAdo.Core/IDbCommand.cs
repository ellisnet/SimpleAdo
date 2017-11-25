using System;

namespace SimpleAdo
{
    /// <summary> Interface for database command. </summary>
    public interface IDbCommand : IDisposable
    {
        /// <summary> Gets or sets the command text. </summary>
        /// <value> The command text. </value>
        string CommandText { get; set; }

        /// <summary> Gets or sets the command timeout. </summary>
        /// <value> The command timeout. </value>
        int CommandTimeout { get; set; }

        /// <summary> Gets or sets the type of the command. </summary>
        /// <value> The type of the command. </value>
        CommandType CommandType { get; set; }

        /// <summary> Gets or sets the connection. </summary>
        /// <value> The connection. </value>
        IDbConnection Connection { get; set; }

        /// <summary> Gets options for controlling the operation. </summary>
        /// <value> The parameters. </value>
        IDataParameterCollection Parameters { get; }

        /// <summary> Gets or sets the transaction. </summary>
        /// <value> The transaction. </value>
        IDbTransaction Transaction { get; set; }

        /// <summary> Gets or sets the updated row source. </summary>
        /// <value> The updated row source. </value>
        UpdateRowSource UpdatedRowSource { get; set; }

        /// <summary> Cancels this object. </summary>
        void Cancel();

        /// <summary> Creates the parameter. </summary>
        /// <returns> The new parameter. </returns>
        IDbDataParameter CreateParameter();

        /// <summary> Executes the non query operation. </summary>
        /// <returns> An int. </returns>
        int ExecuteNonQuery();

        /// <summary> Executes the return row identifier operation. </summary>
        /// <returns> A long. </returns>
        long ExecuteReturnRowId();

        /// <summary> Executes the reader operation. </summary>
        /// <returns> An IDataReader. </returns>
        IDataReader ExecuteReader();

        /// <summary> Executes the reader operation. </summary>
        /// <param name="behavior"> The behavior. </param>
        /// <returns> An IDataReader. </returns>
        IDataReader ExecuteReader(CommandBehavior behavior);

        /// <summary> Executes the scalar operation. </summary>
        /// <returns> An object. </returns>
        object ExecuteScalar();
        /// <summary> Prepares this object. </summary>
        void Prepare();
    }
}
