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
using System.Threading;

namespace SimpleAdo.Sqlite
{
    /// <summary> A sqlite statement preparer. This class cannot be inherited. </summary>
	internal sealed class SqliteStatementPreparer : IDisposable
	{
        /// <summary> The database. </summary>
	    private readonly SqliteDatabaseHandle _database;
        /// <summary> True to for maintenance. </summary>
	    private readonly bool _forMaintenance;
        /// <summary> The command text. </summary>
	    private readonly string _commandText;
        /// <summary> The statements. </summary>
	    private List<SqliteStatementHandle> _statements;
        /// <summary> Number of references. </summary>
	    private int _refCount;

        /// <summary> Constructor. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <param name="database"> The database. </param>
        /// <param name="commandText"> The command text. </param>
        /// <param name="forMaintenance"> (Optional) True to for maintenance. </param>
        public SqliteStatementPreparer(SqliteDatabaseHandle database, string commandText, bool forMaintenance = false)
		{
		    _database = database ?? throw new ArgumentNullException(nameof(database));
		    _commandText = commandText;
		    _forMaintenance = forMaintenance;
			_statements = new List<SqliteStatementHandle>();
			_refCount = 1;
		}

        /// <summary> Gets. </summary>
        /// <exception cref="ObjectDisposedException"> Thrown when a supplied object has been disposed. </exception>
        /// <exception cref="ArgumentOutOfRangeException"> Thrown when one or more arguments are outside
        ///     the required range. </exception>
        /// <exception cref="SqliteException"> Thrown when a Sqlite error condition occurs. </exception>
        /// <param name="index"> Zero-based index of the. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> A SqliteStatementHandle. </returns>
		public SqliteStatementHandle Get(int index, CancellationToken cancellationToken)
		{
			if (_statements == null)
			{
			    throw new ObjectDisposedException(GetType().Name);
			}
		    if (index < 0 || index > _statements.Count)
		    {
		        throw new ArgumentOutOfRangeException(nameof(index));
		    }
		    if (index < _statements.Count)
		    {
		        return _statements[index];
		    }
		    if (_statements.Count > 0 && index == _statements.Count)
		    {
		        //statements are all done
		        return null;
		    }

		    Random random = null;
			SqliteResultCode resultCode;

		    do
		    {
		        var statement = new SqliteStatementHandle(_database, _forMaintenance);
		        resultCode = statement.PrepareStatement(_commandText);

		        switch (resultCode)
		        {
		            case SqliteResultCode.Ok:
		                _statements.Add(statement);
		                break;

		            case SqliteResultCode.Busy:
		            case SqliteResultCode.Locked:
		            case SqliteResultCode.CantOpen:
		                if (cancellationToken.IsCancellationRequested)
		                {
		                    return null;
		                }
		                if (random == null)
		                {
		                    random = new Random();
		                }
		                Thread.Sleep(random.Next(1, 150));
		                break;

		            default:
		                throw new SqliteException(resultCode, _database);
		        }

            } while (!resultCode.IsSuccessCode());

			return _statements[index];
		}

        /// <summary> Adds reference. </summary>
        /// <exception cref="ObjectDisposedException"> Thrown when a supplied object has been disposed. </exception>
		public void AddRef()
		{
			if (_refCount == 0)
			{
			    throw new ObjectDisposedException(GetType().Name);
			}
		    _refCount++;
		}

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
        /// resources.
        /// </summary>
        /// <exception cref="InvalidOperationException"> Thrown when the requested operation is invalid. </exception>
		public void Dispose()
		{
			_refCount--;
			if (_refCount == 0)
			{
				foreach (var statement in _statements)
				{
				    statement.Dispose();
				}
			    _statements = null;
			}
			else if (_refCount < 0)
			{
				throw new InvalidOperationException("SqliteStatementList ref count decremented below zero.");
			}
		}
	}
}
