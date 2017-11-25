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

namespace SimpleAdo.Sqlite
{
    /// <summary> A sqlite transaction. This class cannot be inherited. </summary>
	public sealed class SqliteTransaction : IDbTransaction
    {
        /// <summary> True if this object is finished. </summary>
        private bool _isFinished;

        /// <summary> The connection. </summary>
        private SqliteConnection _connection;

        /// <summary> Gets the connection. </summary>
        /// <value> The connection. </value>
        public IDbConnection Connection
        {
            get
            {
                VerifyNotDisposed();
                return _connection;
            }
        }

        /// <summary> The isolation level. </summary>
        private readonly IsolationLevel _isolationLevel;

        /// <summary> Gets the isolation level. </summary>
        /// <value> The isolation level. </value>
        public IsolationLevel IsolationLevel
        {
            get
            {
                VerifyNotDisposed();
                return _isolationLevel;
            }
        }

        /// <summary> Constructor. </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="isolationLevel"> The isolation level. </param>
        internal SqliteTransaction(SqliteConnection connection, IsolationLevel isolationLevel)
		{
			_connection = connection;
			_isolationLevel = isolationLevel;
		}

        /// <summary> Commits this object. </summary>
        /// <exception cref="InvalidOperationException"> Thrown when the requested operation is invalid. </exception>
		public void Commit()
		{
			VerifyNotDisposed();
			if (_isFinished)
			{
			    throw new InvalidOperationException("Already committed or rolled back.");
			}

		    if (_connection.CurrentTransaction == this)
			{
				if (_connection.IsOnlyTransaction(this))
				{
				    _connection.ExecuteNonQuery(this, "COMMIT");
				}
			    _connection.PopTransaction();
				_isFinished = true;
			}
			else if (_connection.CurrentTransaction != null)
			{
				throw new InvalidOperationException("This is not the active transaction.");
			}
			else if (_connection.CurrentTransaction == null)
			{
				throw new InvalidOperationException("There is no active transaction.");
			}
		}

        /// <summary> Rollbacks this object. </summary>
        /// <exception cref="InvalidOperationException"> Thrown when the requested operation is invalid. </exception>
		public void Rollback()
		{
			VerifyNotDisposed();
			if (_isFinished)
			{
			    throw new InvalidOperationException("Already committed or rolled back.");
			}

		    if (_connection.CurrentTransaction == this)
			{
				if (_connection.IsOnlyTransaction(this))
				{
					_connection.ExecuteNonQuery(this, "ROLLBACK");
					_connection.PopTransaction();
					_isFinished = true;
				}
				else
				{
					throw new InvalidOperationException("Can't roll back nested transaction.");
				}
			}
			else if (_connection.CurrentTransaction != null)
			{
				throw new InvalidOperationException("This is not the active transaction.");
			}
			else if (_connection.CurrentTransaction == null)
			{
				throw new InvalidOperationException("There is no active transaction.");
			}
		}

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
        /// resources.
        /// </summary>
        /// <param name="disposing"> True to release both managed and unmanaged resources; false to
        ///     release only unmanaged resources. </param>
		private void Dispose(bool disposing)
		{
			try
			{
				if (disposing)
				{
					if (!_isFinished && _connection != null && _connection.CurrentTransaction == this && _connection.IsOnlyTransaction(this))
					{
						_connection.ExecuteNonQuery(this, "ROLLBACK");
						_connection.PopTransaction();
					}
					_connection = null;
				}
			}
			// ReSharper disable once RedundantEmptyFinallyBlock
			finally
			{
				//nothing
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

        /// <summary> Verify not disposed. </summary>
        /// <exception cref="ObjectDisposedException"> Thrown when a supplied object has been disposed. </exception>
        private void VerifyNotDisposed()
		{
			if (_connection == null)
			{
			    throw new ObjectDisposedException(GetType().Name);
			}
		}
	}
}
