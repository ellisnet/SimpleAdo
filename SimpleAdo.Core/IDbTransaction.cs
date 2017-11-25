using System;

namespace SimpleAdo
{
    /// <summary> Interface for database transaction. </summary>
    public interface IDbTransaction : IDisposable
    {
        /// <summary> Gets the connection. </summary>
        /// <value> The connection. </value>
        IDbConnection Connection { get; }

        /// <summary> Gets the isolation level. </summary>
        /// <value> The isolation level. </value>
        IsolationLevel IsolationLevel { get; }

        /// <summary> Commits this object. </summary>
        void Commit();
        /// <summary> Rollbacks this object. </summary>
        void Rollback();
    }
}
