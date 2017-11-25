using System;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleAdo
{
    /// <summary> Interface for data reader. </summary>
    public interface IDataReader : IDisposable, IDataRecord
    {
        /// <summary> Gets the depth. </summary>
        /// <value> The depth. </value>
        int Depth { get; }

        /// <summary> Gets a value indicating whether this object is closed. </summary>
        /// <value> True if this object is closed, false if not. </value>
        bool IsClosed { get; }

        /// <summary> Gets the records affected. </summary>
        /// <value> The records affected. </value>
        int RecordsAffected { get; }

        /// <summary> Closes this object. </summary>
        void Close();

        /// <summary> Gets schema table. </summary>
        /// <returns> The schema table. </returns>
		DataTable GetSchemaTable();

        /// <summary> Determines if we can next result. </summary>
        /// <returns> True if it succeeds, false if it fails. </returns>
        bool NextResult();

        /// <summary> Reads this object. </summary>
        /// <returns> True if it succeeds, false if it fails. </returns>
        bool Read();

        /// <summary> Reads the asynchronous. </summary>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns>
        /// The asynchronous result that yields true if it succeeds, false if it fails.
        /// </returns>
        Task<bool> ReadAsync(CancellationToken cancellationToken);

        /// <summary> Next result asynchronous. </summary>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns>
        /// The asynchronous result that yields true if it succeeds, false if it fails.
        /// </returns>
        Task<bool> NextResultAsync(CancellationToken cancellationToken);
    }
}
