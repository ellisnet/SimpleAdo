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
    /// <summary> A sqlite parameter. This class cannot be inherited. </summary>
	public sealed class SqliteParameter : IDbDataParameter {

        /// <summary> Gets or sets the type of the database. </summary>
        /// <value> The type of the database. </value>
		public DbType DbType { get; set; }

        /// <summary> Gets or sets the direction. </summary>
        /// <value> The direction. </value>
		public ParameterDirection Direction { get; set; } = ParameterDirection.Input; //Defaulting to input

        /// <summary> Gets or sets a value indicating whether this object is nullable. </summary>
        /// <value> True if this object is nullable, false if not. </value>
		public bool IsNullable { get; set; }

        /// <summary> Gets or sets the name of the parameter. </summary>
        /// <value> The name of the parameter. </value>
		public string ParameterName { get; set; }

        /// <summary> Gets or sets the size. </summary>
        /// <value> The size. </value>
		public int Size { get; set; }

        /// <summary> Gets or sets source column. </summary>
        /// <exception cref="NotSupportedException"> Thrown when the requested operation is not supported. </exception>
        /// <value> The source column. </value>
		public string SourceColumn
		{
			get => throw new NotSupportedException();
		    set => throw new NotSupportedException();
		}

        /// <summary>
        /// Gets or sets a value indicating whether the source column null mapping.
        /// </summary>
        /// <exception cref="NotSupportedException"> Thrown when the requested operation is not supported. </exception>
        /// <value> True if source column null mapping, false if not. </value>
		public bool SourceColumnNullMapping
		{
			get => throw new NotSupportedException();
		    set => throw new NotSupportedException();
		}

        /// <summary> Gets or sets source version. </summary>
        /// <exception cref="NotSupportedException"> Thrown when the requested operation is not supported. </exception>
        /// <value> The source version. </value>
		public DataRowVersion SourceVersion
		{
			get => throw new NotSupportedException();
		    set => throw new NotSupportedException();
		}

        /// <summary> Gets or sets the value. </summary>
        /// <value> The value. </value>
		public object Value { get; set; }

        /// <summary> Resets the database type. </summary>
		public void ResetDbType()
		{
			DbType = default(DbType);
		}

        /// <summary> Default constructor. </summary>
	    public SqliteParameter() { }

        /// <summary> Constructor. </summary>
        /// <param name="parameterName"> Name of the parameter. </param>
        /// <param name="value"> The value. </param>
	    public SqliteParameter(string parameterName, object value) {
	        ParameterName = parameterName;
	        Value = value;
	    }

        /// <summary> Gets or sets the precision. </summary>
        /// <value> The precision. </value>
        public byte Precision {
            get { return 0; }
            set { }
        }

        /// <summary> Gets or sets the scale. </summary>
        /// <value> The scale. </value>
        public byte Scale {
            get { return 0; }
            set { }
        }
    }
}
