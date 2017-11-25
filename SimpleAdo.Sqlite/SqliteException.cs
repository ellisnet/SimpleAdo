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

using SimpleAdo.Common;

namespace SimpleAdo.Sqlite
{
    /// <summary>
    /// Exception for signalling sqlite errors. This class cannot be inherited.
    /// </summary>
	public sealed class SqliteException : DbException
	{
        /// <summary> Constructor. </summary>
        /// <param name="errorCode"> The error code. </param>
		public SqliteException(SqliteResultCode errorCode)
			: this(errorCode, null) { }

        /// <summary> Constructor. </summary>
        /// <param name="message"> The message. </param>
        /// <param name="errorCode"> The error code. </param>
	    public SqliteException(string message, SqliteResultCode errorCode)
            : base(message, (int)errorCode) { }

        /// <summary> Constructor. </summary>
        /// <param name="errorCode"> The error code. </param>
        /// <param name="database"> The database. </param>
		internal SqliteException(SqliteResultCode errorCode, SqliteDatabaseHandle database)
			: base(GetErrorString(errorCode, database), (int) errorCode) { }

        /// <summary> Gets error string. </summary>
        /// <param name="errorCode"> The error code. </param>
        /// <param name="database"> The database. </param>
        /// <returns> The error string. </returns>
		private static string GetErrorString(SqliteResultCode errorCode, SqliteDatabaseHandle database)
		{
			string errorString = errorCode.ToString();
            return database != null ? "{0}: {1}".FormatInvariant(errorString, database.Context.sqlite3_errmsg(database, database.IsDatabaseInMaintenanceMode))
				: errorString;
		}
	}
}
