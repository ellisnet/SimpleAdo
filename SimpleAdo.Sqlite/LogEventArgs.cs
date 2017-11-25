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
    /// <summary> Event data for logging event handlers. </summary>
	public sealed class LogEventArgs : EventArgs
	{
        /// <summary>
        /// The error code.  The type of this object value should be
        /// <see cref="Int32" /> or <see cref="SqliteResultCode" />.
        /// </summary>
		public readonly SqliteResultCode ErrorCode;

        /// <summary> SQL statement text as the statement first begins executing. </summary>
		public readonly string Message;

        /// <summary> Extra data associated with this event, if any. </summary>
		public readonly object Data;

        /// <summary> Constructs the object. </summary>
        /// <param name="pUserData"> Should be null. </param>
        /// <param name="errorCode"> The error code.  The type of this object value should be
        ///     <see cref="Int32" /> or <see cref="SqliteResultCode" />. </param>
        /// <param name="message"> The error message, if any. </param>
        /// <param name="data"> The extra data, if any. </param>
		internal LogEventArgs(object pUserData, SqliteResultCode errorCode, string message, object data) {
			ErrorCode = errorCode;
			Message = message;
			Data = data;
		}
	}
}
