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
    /// <summary>
    /// Provides additional information for the <see cref="SqliteConnection.StatementCompleted"/>
    /// event.
    /// </summary>
	public sealed class StatementCompletedEventArgs : EventArgs
	{
        /// <summary> The SQL. </summary>
	    private readonly string _sql;

        /// <summary> The SQL of the statement that was executed. </summary>
        /// <value> The SQL. </value>
        public string Sql => _sql;

        /// <summary> The time. </summary>
	    private readonly TimeSpan _time;

        /// <summary> The time it took to execute the statement. </summary>
        /// <value> The time. </value>
        public TimeSpan Time => _time;

        /// <summary> Constructor. </summary>
        /// <param name="sql"> The SQL of the statement that was executed. </param>
        /// <param name="time"> The time it took to execute the statement. </param>
	    internal StatementCompletedEventArgs(string sql, TimeSpan time)
		{
			_sql = sql;
			_time = time;
		}
	}

    /// <summary>
    /// The delegate type for the event handlers of the
    /// <see cref="SqliteConnection.StatementCompleted"/> event.
    /// </summary>
    /// <param name="sender"> The source of the event. </param>
    /// <param name="e"> The data for the event. </param>
    public delegate void StatementCompletedEventHandler(object sender, StatementCompletedEventArgs e);
}
