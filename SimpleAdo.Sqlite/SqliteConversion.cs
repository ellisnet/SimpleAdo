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
using System.Linq;
using System.Reflection;

namespace SimpleAdo.Sqlite {

    /// <summary>
    /// Intermediary types for converting between Sqlite data types and CLR data types.
    /// </summary>
    public enum NetType {
        /// <summary>
        /// Empty data type
        /// </summary>
        Empty = 0,
        /// <summary>
        /// Object data type
        /// </summary>
        Object = 1,
        /// <summary> An enum constant representing the Database null option. </summary>
        /// <summary>
        /// SQL NULL value
        /// </summary>
        // ReSharper disable once InconsistentNaming
        DBNull = 2,
        /// <summary>
        /// Boolean data type
        /// </summary>
        Boolean = 3,
        /// <summary>
        /// Character data type
        /// </summary>
        Char = 4,
        /// <summary>
        /// Signed byte data type
        /// </summary>
        SByte = 5,
        /// <summary>
        /// Byte data type
        /// </summary>
        Byte = 6,
        /// <summary>
        /// Short/Int16 data type
        /// </summary>
        Int16 = 7,
        /// <summary>
        /// Unsigned short data type
        /// </summary>
        UInt16 = 8,
        /// <summary>
        /// Integer data type
        /// </summary>
        Int32 = 9,
        /// <summary>
        /// Unsigned integer data type
        /// </summary>
        UInt32 = 10,
        /// <summary>
        /// Long/Int64 data type
        /// </summary>
        Int64 = 11,
        /// <summary>
        /// Unsigned long data type
        /// </summary>
        UInt64 = 12,
        /// <summary>
        /// Single precision float data type
        /// </summary>
        Single = 13,
        /// <summary>
        /// Double precision float data type
        /// </summary>
        Double = 14,
        /// <summary>
        /// Decimal data type
        /// </summary>
        Decimal = 15,
        /// <summary>
        /// DateTime data type
        /// </summary>
        DateTime = 16,
        /// <summary>
        /// String data type
        /// </summary>
        String = 18,
        /// <summary>
        /// DateTimeOffset data type
        /// </summary>
        DateTimeOffset = 19,
    }

    /// <summary> A sqlite conversion. </summary>
    internal static class SqliteConversion {

        /// <summary> Get the custom attributes of the specified type. </summary>
        /// <param name="type"> The type to check for attributes. </param>
        /// <param name="attributeType"> The type of attribute to look for. </param>
        /// <param name="inherit"> Get attributes that are inherited? </param>
        /// <returns> Array of attributes. </returns>
        public static Attribute[] GetCustomAttributes(this Type type, Type attributeType, bool inherit) {
            return type.GetTypeInfo().GetCustomAttributes(attributeType, inherit).ToArray();
        }

        /// <summary> Get the properties of the specified type. </summary>
        /// <param name="type"> The type to check for properties. </param>
        /// <returns> Array of properties. </returns>
        public static PropertyInfo[] GetProperties(this Type type) {
            return type.GetTypeInfo().DeclaredProperties.ToArray();
        }

        /// <summary> Determines if specified type is an Enum. </summary>
        /// <param name="type"> The type to check. </param>
        /// <returns> If true, specified type is an Enum. </returns>
        public static bool IsEnum(this Type type) {
            return type.GetTypeInfo().IsEnum;
        }

        /// <summary> Determines the underlying CLR type of the specified type. </summary>
        /// <param name="type"> The type to inspect. </param>
        /// <returns> The underlying CLR type. </returns>
        public static Type GetUnderlyingSystemType(this Type type) {
            return type;
        }

        /// <summary> Database type to colour type. </summary>
        /// <param name="dbType"> Type of the database. </param>
        /// <returns> A Type. </returns>
        public static Type DbTypeToClrType(DbType dbType)
        {
            switch (dbType)
            {
                case DbType.AnsiStringFixedLength:
                case DbType.AnsiString:
                case DbType.StringFixedLength:
                case DbType.String:
                case DbType.Xml:
                case DbType.Encrypted:
                    return typeof(string);
                    //break;
                case DbType.Binary:
                    return typeof(byte[]);
                    //break;
                case DbType.Boolean:
                    return typeof(bool);
                    //break;
                case DbType.Byte:
                    return typeof(byte);
                    //break;
                case DbType.Currency:
                case DbType.Decimal:
                    return typeof(Decimal);
                    //break;
                case DbType.Date:
                case DbType.Time:
                case DbType.DateTime:
                case DbType.DateTime2:
                    return typeof(DateTime);
                    //break;
                case DbType.Double:
                case DbType.VarNumeric:
                    return typeof(double);
                    //break;
                case DbType.Guid:
                    return typeof(Guid);
                    //break;
                case DbType.Int16:
                    return typeof(Int16);
                    //break;
                case DbType.Int32:
                    return typeof(Int32);
                    //break;
                case DbType.Int64:
                    return typeof(Int64);
                    //break;
                case DbType.SByte:
                    return typeof(SByte);
                    //break;
                case DbType.Single:
                    return typeof(Single);
                    //break;
                case DbType.UInt16:
                    return typeof(UInt16);
                    //break;
                case DbType.UInt32:
                    return typeof(UInt32);
                    //break;
                case DbType.UInt64:
                    return typeof(UInt64);
                    //break;
                case DbType.DateTimeOffset:
                    return typeof(DateTimeOffset);
                    //break;
                default:
                    break;
            }
            return typeof(object);
        }

        /// <summary>
        /// For a given type, return the closest-match SQLite column type, which only understands a very
        /// limited subset of types.
        /// </summary>
        /// <param name="typ"> The type to evaluate. </param>
        /// <returns> The SQLite type column type for that type. </returns>
        internal static SqliteColumnType TypeToColumnType(Type typ) {
            var tc = NetTypes.GetNetType(typ);
            if (tc == NetType.Object) {
                if (typ == typeof(byte[]) || typ == typeof(Guid))
                {
                    return SqliteColumnType.Blob;
                }
                else
                {
                    return SqliteColumnType.Text;
                }
            }
            return typecodeColumnTypes[(int)tc];
        }

        /// <summary> List of types of the typecode columns. </summary>
        private static readonly SqliteColumnType[] typecodeColumnTypes = {
            SqliteColumnType.Null, //0 - should match NetType.Empty
            SqliteColumnType.Blob, //1 - should match NetType.Object
            SqliteColumnType.Null, //2 - should match NetType.DBNull
            SqliteColumnType.Integer, //3 - should match NetType.Boolean
            SqliteColumnType.Integer, //4 - should match NetType.Char
            SqliteColumnType.Integer, //5 - should match NetType.SByte
            SqliteColumnType.Integer, //6 - should match NetType.Byte
            SqliteColumnType.Integer, //7 - should match NetType.Int16
            SqliteColumnType.Integer, //8 - should match NetType.UInt16
            SqliteColumnType.Integer, //9 - should match NetType.Int32
            SqliteColumnType.Integer, //10 - should match NetType.UInt32
            SqliteColumnType.Integer, //11 - should match NetType.Int64
            SqliteColumnType.Integer, //12 - should match NetType.UInt64
            SqliteColumnType.Double, //13 - should match NetType.Single
            SqliteColumnType.Double, //14 - should match NetType.Double
            SqliteColumnType.Double, //15 - should match NetType.Decimal
            SqliteColumnType.ConvDateTime, //16 - should match NetType.DateTime
            SqliteColumnType.Null, //17 - not matching NetType
            SqliteColumnType.Text, //18 - should match NetType.String
            SqliteColumnType.ConvDateTimeOffset, //19 - should match NetType.DateTimeOffset
        };

        /// <summary> A net types. </summary>
        internal static class NetTypes
        {
            /// <summary> The types. </summary>
            private static readonly Dictionary<Type, NetType> types = new Dictionary<Type, NetType> {
                {typeof(bool), NetType.Boolean},
                {typeof(char), NetType.Char},
                {typeof(sbyte), NetType.SByte},
                {typeof(byte), NetType.Byte},
                {typeof(short), NetType.Int16},
                {typeof(ushort), NetType.UInt16},
                {typeof(int), NetType.Int32},
                {typeof(uint), NetType.UInt32},
                {typeof(long), NetType.Int64},
                {typeof(ulong), NetType.UInt64},
                {typeof(float), NetType.Single},
                {typeof(double), NetType.Double},
                {typeof(decimal), NetType.Decimal},
                {typeof(DateTime), NetType.DateTime},
                {typeof(string), NetType.String},
                {typeof(DateTimeOffset), NetType.DateTimeOffset },
            };

            /// <summary> Gets net type. </summary>
            /// <param name="type"> The type. </param>
            /// <returns> The net type. </returns>
            public static NetType GetNetType(Type type) {
                if (type == null)
                {
                    return NetType.Empty;
                }
                else if (type != type.GetUnderlyingSystemType() && type.GetUnderlyingSystemType() != null)
                {
                    return GetNetType(type.GetUnderlyingSystemType());
                }
                else
                {
                    return GetNetTypeImpl(type);
                }
            }

            /// <summary> Gets net type implementation. </summary>
            /// <param name="type"> The type. </param>
            /// <returns> The net type implementation. </returns>
            private static NetType GetNetTypeImpl(Type type) {
                if (types.ContainsKey(type)) {
                    return types[type];
                }

                if (type.IsEnum()) {
                    return types[Enum.GetUnderlyingType(type)];
                }

                return NetType.Object;
            }
        }

    }
}
