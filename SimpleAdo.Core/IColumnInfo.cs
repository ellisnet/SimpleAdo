namespace SimpleAdo
{
    /// <summary> Interface for column information. </summary>
    public interface IColumnInfo
    {
        /// <summary> Gets or sets the identifier of the column. </summary>
        /// <value> The identifier of the column. </value>
        int ColumnId { get; set; }

        /// <summary> Gets or sets the name. </summary>
        /// <value> The name. </value>
        string Name { get; set; }

        /// <summary> Gets or sets the type of the data. </summary>
        /// <value> The type of the data. </value>
        DbType DataType { get; set; }

        /// <summary> Gets or sets the name of the declared type. </summary>
        /// <value> The name of the declared type. </value>
        string DeclaredTypeName { get; set; }

        /// <summary> Gets or sets the default value. </summary>
        /// <value> The default value. </value>
        object DefaultValue { get; set; }

        /// <summary> Gets or sets a value indicating whether this object is not null. </summary>
        /// <value> True if this object is not null, false if not. </value>
        bool IsNotNull { get; set; }

        /// <summary> Gets or sets a value indicating whether this object is primary key. </summary>
        /// <value> True if this object is primary key, false if not. </value>
        bool IsPrimaryKey { get; set; }
    }
}
