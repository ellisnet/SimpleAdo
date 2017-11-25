namespace SimpleAdo
{
    /// <summary> Interface for database data parameter. </summary>
    public interface IDbDataParameter : IDataParameter
    {
        /// <summary> Gets or sets the precision. </summary>
        /// <value> The precision. </value>
        byte Precision { get; set; }

        /// <summary> Gets or sets the scale. </summary>
        /// <value> The scale. </value>
        byte Scale { get; set; }

        /// <summary> Gets or sets the size. </summary>
        /// <value> The size. </value>
        int Size { get; set; }
    }
}
