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
    /// <summary> A sqlite log. </summary>
	public static class SqliteLog
    {

        /// <summary> The static locker. </summary>
        private static readonly object staticLocker = new object();
	    // ReSharper disable once InconsistentNaming
        /// <summary> The context. </summary>
	    private static SqliteContext _context;

        /// <summary>
        /// This event is raised whenever SQLite raises a logging event. Note that this should be set as
        /// one of the first things in the application.
        /// </summary>
        public static event SqliteLogEventHandler Log
		{
			add
			{
				if (s_loggingDisabled)
				{
				    throw new InvalidOperationException("SQLite logging is disabled.");
				}

			    lock (s_lock)
			    {
			        Handlers += value;
			    }
			}
			remove
			{
				if (s_loggingDisabled)
				{
				    throw new InvalidOperationException("SQLite logging is disabled.");
				}

			    lock (s_lock)
			    {
			        Handlers -= value;
			    }
			}
		}

        /// <summary> Initializes this object. </summary>
        /// <param name="context"> The context. </param>
		internal static void Initialize(SqliteContext context) {
            // reference a static field to force the static constructor to run
		    GC.KeepAlive(s_lock);
            lock (staticLocker)
		    {
		        _context = context ?? _context;
		        _context?.sqlite3_config_log(s_callback, null);
		    }
		}

	    // ReSharper disable once InconsistentNaming

        /// <summary> Callback, called when the log. </summary>
        /// <param name="user_data"> Information describing the user. </param>
        /// <param name="errorCode"> The error code. </param>
        /// <param name="msg"> The message. </param>
	    static void LogCallback(object user_data, int errorCode, string msg)
	    {
	        lock (s_lock)
	        {
	            Handlers?.Invoke(null, new LogEventArgs(user_data, (SqliteResultCode)errorCode, msg, null));
	        }
        }

        // ReSharper disable InconsistentNaming
        /// <summary> The lock. </summary>
        static readonly object s_lock = new object();
        /// <summary> The callback. </summary>
		static readonly DbProvider.delegate_log s_callback = LogCallback;
        /// <summary> True to disable, false to enable the logging. </summary>
		static readonly bool s_loggingDisabled = false;
        /// <summary> Event queue for all listeners interested in Handlers events. </summary>
		static event SqliteLogEventHandler Handlers;
	    // ReSharper restore InconsistentNaming
    }

    /// <summary> Delegate for handling SqliteLog events. </summary>
    /// <param name="sender"> Source of the event. </param>
    /// <param name="e"> Log event information. </param>
    public delegate void SqliteLogEventHandler(object sender, LogEventArgs e);
}
