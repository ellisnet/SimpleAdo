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

namespace SimpleAdo.Sqlite
{
    /// <summary> Values that represent sqlite column types. </summary>
	public enum SqliteColumnType
	{
		/// <summary>
		/// Not used
		/// </summary>
		None = 0,

		/// <summary>
		/// All integers in SQLite default to Int64
		/// </summary>
		Integer = 1,

		/// <summary>
		/// All floating point numbers in SQLite default to double
		/// </summary>
		Double = 2,

		/// <summary>
		/// The default data type of SQLite is text
		/// </summary>
		Text = 3,

		/// <summary>
		/// Typically blob types are only seen when returned from a function
		/// </summary>
		Blob = 4,

		/// <summary>
		/// Null types can be returned from functions
		/// </summary>
		Null = 5,

        /// <summary>
        /// DateTime is not really a SQLite column type, but using this enum member for internal processing
        /// </summary>
        ConvDateTime = 6,


	    /// <summary>
	    /// DateTimeOffset is not really a SQLite column type, but using this enum member for internal processing
	    /// </summary>
	    ConvDateTimeOffset = 7,
    }
}