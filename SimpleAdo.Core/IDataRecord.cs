using System;

namespace SimpleAdo
{
    /// <summary> Interface for data record. </summary>
    public interface IDataRecord
    {
        /// <summary> Gets the number of fields. </summary>
        /// <value> The number of fields. </value>
        int FieldCount { get; }

        /// <summary> Indexer to get items within this collection using array index syntax. </summary>
        /// <param name="columnName"> Name of the column. </param>
        /// <returns> The indexed item. </returns>
        object this[string columnName] { get; }

        /// <summary> Indexer to get items within this collection using array index syntax. </summary>
        /// <param name="columnIndex"> Zero-based index of the column. </param>
        /// <returns> The indexed item. </returns>
        object this[int columnIndex] { get; }

        /// <summary> Gets a boolean. </summary>
        /// <param name="columnIndex"> Zero-based index of the column. </param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        bool GetBoolean(int columnIndex);

        /// <summary> Gets a boolean. </summary>
        /// <param name="columnName"> Name of the column. </param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        bool GetBoolean(string columnName);

        /// <summary> Gets a byte. </summary>
        /// <param name="columnIndex"> Zero-based index of the column. </param>
        /// <returns> The byte. </returns>
        byte GetByte(int columnIndex);

        /// <summary> Gets a byte. </summary>
        /// <param name="columnName"> Name of the column. </param>
        /// <returns> The byte. </returns>
        byte GetByte(string columnName);

        /// <summary> Gets the byte array. </summary>
        /// <param name="columnIndex"> Zero-based index of the column. </param>
        /// <returns> An array of byte. </returns>
        byte[] GetBytes(int columnIndex);

        /// <summary> Gets the byte array. </summary>
        /// <param name="columnName"> Name of the column. </param>
        /// <returns> An array of byte. </returns>
        byte[] GetBytes(string columnName);

        /// <summary> Gets a character. </summary>
        /// <param name="columnIndex"> Zero-based index of the column. </param>
        /// <returns> The character. </returns>
        char GetChar(int columnIndex);

        /// <summary> Gets a character. </summary>
        /// <param name="columnName"> Name of the column. </param>
        /// <returns> The character. </returns>
        char GetChar(string columnName);

        /// <summary> Gets a data. </summary>
        /// <param name="columnIndex"> Zero-based index of the column. </param>
        /// <returns> The data. </returns>
        IDataReader GetData(int columnIndex);

        /// <summary> Gets a data. </summary>
        /// <param name="columnName"> Name of the column. </param>
        /// <returns> The data. </returns>
        IDataReader GetData(string columnName);

        /// <summary> Gets data type name. </summary>
        /// <param name="columnIndex"> Zero-based index of the column. </param>
        /// <returns> The data type name. </returns>
        string GetDataTypeName(int columnIndex);

        /// <summary> Gets date time. </summary>
        /// <param name="columnIndex"> Zero-based index of the column. </param>
        /// <returns> The date time. </returns>
        DateTime GetDateTime(int columnIndex);

        /// <summary> Gets date time. </summary>
        /// <param name="columnName"> Name of the column. </param>
        /// <returns> The date time. </returns>
        DateTime GetDateTime(string columnName);

        /// <summary> Gets date time offset. </summary>
        /// <param name="columnIndex"> Zero-based index of the column. </param>
        /// <returns> The date time offset. </returns>
        DateTimeOffset GetDateTimeOffset(int columnIndex);

        /// <summary> Gets date time offset. </summary>
        /// <param name="columnName"> Name of the column. </param>
        /// <returns> The date time offset. </returns>
        DateTimeOffset GetDateTimeOffset(string columnName);

        /// <summary> Gets a decimal. </summary>
        /// <param name="columnIndex"> Zero-based index of the column. </param>
        /// <returns> The decimal. </returns>
        decimal GetDecimal(int columnIndex);

        /// <summary> Gets a decimal. </summary>
        /// <param name="columnName"> Name of the column. </param>
        /// <returns> The decimal. </returns>
        decimal GetDecimal(string columnName);

        /// <summary> Gets a double. </summary>
        /// <param name="columnIndex"> Zero-based index of the column. </param>
        /// <returns> The double. </returns>
        double GetDouble(int columnIndex);

        /// <summary> Gets a double. </summary>
        /// <param name="columnName"> Name of the column. </param>
        /// <returns> The double. </returns>
        double GetDouble(string columnName);

        /// <summary> Gets field type. </summary>
        /// <param name="columnIndex"> Zero-based index of the column. </param>
        /// <returns> The field type. </returns>
        Type GetFieldType(int columnIndex);

        /// <summary> Gets a float. </summary>
        /// <param name="columnIndex"> Zero-based index of the column. </param>
        /// <returns> The float. </returns>
        float GetFloat(int columnIndex);

        /// <summary> Gets a float. </summary>
        /// <param name="columnName"> Name of the column. </param>
        /// <returns> The float. </returns>
        float GetFloat(string columnName);

        /// <summary> Gets a unique identifier. </summary>
        /// <param name="columnIndex"> Zero-based index of the column. </param>
        /// <returns> The unique identifier. </returns>
        Guid GetGuid(int columnIndex);

        /// <summary> Gets a unique identifier. </summary>
        /// <param name="columnName"> Name of the column. </param>
        /// <returns> The unique identifier. </returns>
        Guid GetGuid(string columnName);

        /// <summary> Gets int 16. </summary>
        /// <param name="columnIndex"> Zero-based index of the column. </param>
        /// <returns> The int 16. </returns>
        short GetInt16(int columnIndex);

        /// <summary> Gets int 16. </summary>
        /// <param name="columnName"> Name of the column. </param>
        /// <returns> The int 16. </returns>
        short GetInt16(string columnName);

        /// <summary> Gets int 32. </summary>
        /// <param name="columnIndex"> Zero-based index of the column. </param>
        /// <returns> The int 32. </returns>
        int GetInt32(int columnIndex);

        /// <summary> Gets int 32. </summary>
        /// <param name="columnName"> Name of the column. </param>
        /// <returns> The int 32. </returns>
        int GetInt32(string columnName);

        /// <summary> Gets int 64. </summary>
        /// <param name="columnIndex"> Zero-based index of the column. </param>
        /// <returns> The int 64. </returns>
        long GetInt64(int columnIndex);

        /// <summary> Gets int 64. </summary>
        /// <param name="columnName"> Name of the column. </param>
        /// <returns> The int 64. </returns>
        long GetInt64(string columnName);

        /// <summary> Gets column name. </summary>
        /// <param name="columnIndex"> Zero-based index of the column. </param>
        /// <returns> The column name. </returns>
        string GetColumnName(int columnIndex);

        /// <summary> Gets column index. </summary>
        /// <param name="columnName"> Name of the column. </param>
        /// <returns> The column index. </returns>
        int GetColumnIndex(string columnName);

        /// <summary> Gets a string. </summary>
        /// <param name="columnIndex"> Zero-based index of the column. </param>
        /// <returns> The string. </returns>
        string GetString(int columnIndex);

        /// <summary> Gets a string. </summary>
        /// <param name="columnName"> Name of the column. </param>
        /// <returns> The string. </returns>
        string GetString(string columnName);

        /// <summary> Gets a value. </summary>
        /// <param name="columnIndex"> Zero-based index of the column. </param>
        /// <returns> The value. </returns>
        object GetValue(int columnIndex);

        /// <summary> Gets a value. </summary>
        /// <param name="columnName"> Name of the column. </param>
        /// <returns> The value. </returns>
        object GetValue(string columnName);
        // ReSharper disable InconsistentNaming

        /// <summary> Query if 'columnName' is database null. </summary>
        /// <param name="columnIndex"> Zero-based index of the column. </param>
        /// <returns> True if database null, false if not. </returns>
        bool IsDBNull(int columnIndex);

        /// <summary> Query if 'columnName' is database null. </summary>
        /// <param name="columnName"> Name of the column. </param>
        /// <returns> True if database null, false if not. </returns>
        bool IsDBNull(string columnName);
        // ReSharper restore InconsistentNaming

        /// <summary> Gets the values. </summary>
        /// <param name="values"> The values. </param>
        /// <returns> The values. </returns>
        int GetValues(object[] values);

        /// <summary> Gets the identifier of the last insert row. </summary>
        /// <value> The identifier of the last insert row. </value>
        long LastInsertRowId { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the row identifier should be set.
        /// </summary>
        /// <value> True if set row identifier, false if not. </value>
        bool SetRowId { get; set; }

    }
}
