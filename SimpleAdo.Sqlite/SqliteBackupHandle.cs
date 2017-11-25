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
using DbProvider = SQLitePCL;

namespace SimpleAdo.Sqlite
{
    /// <summary> A sqlite backup handle. This class cannot be inherited. </summary>
	internal sealed class SqliteBackupHandle : IDisposable
	{
        /// <summary> Name of the destination. </summary>
	    private string _destinationName;

        /// <summary> Gets or sets the name of the destination. </summary>
        /// <value> The name of the destination. </value>
	    internal string DestinationName => _destinationName;

        /// <summary> Name of the source. </summary>
	    private string _sourceName;

        /// <summary> Gets or sets the name of the source. </summary>
        /// <value> The name of the source. </value>
	    internal string SourceName => _sourceName;

        /// <summary> Handle of the destination. </summary>
        private SqliteDatabaseHandle _destinationHandle;

        /// <summary> Gets or sets the handle of the destination. </summary>
        /// <value> The destination handle. </value>
	    internal SqliteDatabaseHandle DestinationHandle => _destinationHandle;

        /// <summary> Handle of the source. </summary>
	    private SqliteDatabaseHandle _sourceHandle;

        /// <summary> Gets or sets the handle of the source. </summary>
        /// <value> The source handle. </value>
	    internal SqliteDatabaseHandle SourceHandle => _sourceHandle;

        /// <summary> The backup. </summary>
	    private DbProvider.sqlite3_backup _backup;

        /// <summary> Gets or sets the backup. </summary>
        /// <value> The backup. </value>
	    internal DbProvider.sqlite3_backup Backup => _backup;

        /// <summary> Constructor. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <exception cref="ArgumentOutOfRangeException"> Thrown when one or more arguments are outside
        ///     the required range. </exception>
        /// <param name="destinationHandle"> Handle of the destination. </param>
        /// <param name="destinationName"> Name of the destination. </param>
        /// <param name="sourceHandle"> Handle of the source. </param>
        /// <param name="sourceName"> Name of the source. </param>
        /// <param name="backup"> The backup. </param>
	    internal SqliteBackupHandle(
            SqliteDatabaseHandle destinationHandle,
            string destinationName,
            SqliteDatabaseHandle sourceHandle,
            string sourceName,
            DbProvider.sqlite3_backup backup)
	    {
	        _destinationHandle = destinationHandle ?? throw new ArgumentNullException(nameof(destinationHandle));
	        _sourceHandle = sourceHandle ?? throw new ArgumentNullException(nameof(sourceHandle));
	        _backup = backup ?? throw new ArgumentNullException(nameof(backup));
	        _destinationName = (!String.IsNullOrWhiteSpace(destinationName))
	            ? destinationName.Trim()
	            : throw new ArgumentOutOfRangeException(nameof(destinationName));
	        _sourceName = (!String.IsNullOrWhiteSpace(sourceName))
	            ? sourceName.Trim()
	            : throw new ArgumentOutOfRangeException(nameof(sourceName));
	    }

        /// <summary> Check maintenance mode. </summary>
        /// <exception cref="InvalidOperationException"> Thrown when the requested operation is invalid. </exception>
	    internal void CheckMaintenanceMode()
	    {
	        if (!_destinationHandle.IsDatabaseInMaintenanceMode)
	        {
	            throw new InvalidOperationException("The destination database is not currently in maintenance mode.");
	        }

	        if (!_sourceHandle.IsDatabaseInMaintenanceMode)
	        {
	            throw new InvalidOperationException("The source database is not currently in maintenance mode.");
	        }
	    }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
        /// resources.
        /// </summary>
	    public void Dispose()
	    {
	        _destinationName = null;
	        _sourceName = null;
	        _destinationHandle = null;
	        _sourceHandle = null;
	        _backup = null;
	    }
    }
}
