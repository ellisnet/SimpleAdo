namespace SimpleAdo
{
    /// <summary> Interface for data parameter. </summary>
    public interface IDataParameter
    {
        /// <summary> Gets or sets the type of the database. </summary>
        /// <value> The type of the database. </value>
        DbType DbType 
        { 
            get; 
            set; 
        }

        /// <summary> Gets or sets the direction. </summary>
        /// <value> The direction. </value>
        ParameterDirection Direction 
        { 
            get; 
            set; 
        }

        /// <summary> Gets a value indicating whether this object is nullable. </summary>
        /// <value> True if this object is nullable, false if not. </value>
        bool IsNullable 
        { 
            get; 
        }

        /// <summary> Gets or sets the name of the parameter. </summary>
        /// <value> The name of the parameter. </value>
        string ParameterName 
        { 
            get; set; 
        }

        /// <summary> Gets or sets source column. </summary>
        /// <value> The source column. </value>
        string SourceColumn 
        { 
            get; 
            set; 
        }

        /// <summary> Gets or sets source version. </summary>
        /// <value> The source version. </value>
        DataRowVersion SourceVersion 
        { 
            get; 
            set; 
        }

        /// <summary> Gets or sets the value. </summary>
        /// <value> The value. </value>
        object Value 
        { 
            get; 
            set; 
        }
    }
}
