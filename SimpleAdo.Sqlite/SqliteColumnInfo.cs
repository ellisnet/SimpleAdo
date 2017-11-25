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
    /// <summary> Information about the sqlite column. </summary>
    public class SqliteColumnInfo : IColumnInfo
    {
        /// <summary> Identifier for the column. </summary>
        private int _columnId;

        /// <summary> Gets or sets the identifier of the column. </summary>
        /// <exception cref="ArgumentOutOfRangeException"> Thrown when one or more arguments are outside
        ///     the required range. </exception>
        /// <value> The identifier of the column. </value>
        public int ColumnId
        {
            get => _columnId;
            set => _columnId = (value > -1) 
                ? value 
                : throw new ArgumentOutOfRangeException($"The value of {nameof(ColumnId)} cannot be less than zero.");
        }

        /// <summary> The name. </summary>
        private string _name;

        /// <summary> Gets or sets the name. </summary>
        /// <value> The name. </value>
        public string Name
        {
            get => _name;
            set => _name = (String.IsNullOrWhiteSpace(value)) ? null : value;
        }

        /// <summary> Type of the data. </summary>
        private DbType _dataType = DbType.Unknown;

        /// <summary> Gets or sets the type of the data. </summary>
        /// <value> The type of the data. </value>
        public DbType DataType
        {
            get => (_dataType == DbType.Unknown && _declaredTypeName != null && SqliteDataReader.SqliteDeclaredTypeToDbType.ContainsKey(_declaredTypeName)) 
                ? (_dataType = SqliteDataReader.SqliteDeclaredTypeToDbType[_declaredTypeName])
                : _dataType;
            set => _dataType = value;
        }

        /// <summary> Gets or sets the type of the colour. </summary>
        /// <value> The type of the colour. </value>
        public Type ClrType => SqliteConversion.DbTypeToClrType(DataType);

        /// <summary> Name of the declared type. </summary>
        private string _declaredTypeName;

        /// <summary> Gets or sets the name of the declared type. </summary>
        /// <value> The name of the declared type. </value>
        public string DeclaredTypeName
        {
            get => _declaredTypeName;
            set => _declaredTypeName = String.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        /// <summary> The default value. </summary>
        private object _defaultValue;

        /// <summary> Gets or sets the default value. </summary>
        /// <value> The default value. </value>
        public object DefaultValue
        {
            get => _defaultValue;
            set => _defaultValue = (value == null || value == DBNull.Value) ? null : value;
        }

        /// <summary> True if this object is not null. </summary>
        private bool _isNotNull;

        /// <summary> Gets or sets a value indicating whether this object is not null. </summary>
        /// <value> True if this object is not null, false if not. </value>
        public bool IsNotNull
        {
            get => _isNotNull;
            set => _isNotNull = value;
        }

        /// <summary> True if this object is primary key. </summary>
        private bool _isPrimaryKey;

        /// <summary> Gets or sets a value indicating whether this object is primary key. </summary>
        /// <value> True if this object is primary key, false if not. </value>
        public bool IsPrimaryKey
        {
            get => _isPrimaryKey;
            set => _isPrimaryKey = value;
        }
    }
}
