using System;

namespace SimpleAdo
{
    /// <summary> A bit-field of flags for specifying command behaviors. </summary>
    [Flags]
    public enum CommandBehavior
    {
        /// <summary> A binary constant representing the default flag. </summary>
        Default = 0,
        /// <summary> A binary constant representing the single result flag. </summary>
        SingleResult = 1,
        /// <summary> A binary constant representing the schema only flag. </summary>
        SchemaOnly = 2,
        /// <summary> A binary constant representing the key Information flag. </summary>
        KeyInfo = 4,
        /// <summary> A binary constant representing the single row flag. </summary>
        SingleRow = 8,
        /// <summary> A binary constant representing the sequential access flag. </summary>
        SequentialAccess = 16,
        /// <summary> A binary constant representing the close connection flag. </summary>
        CloseConnection = 32,
    }
}
