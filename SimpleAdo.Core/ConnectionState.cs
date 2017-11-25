using System;

namespace SimpleAdo
{
    /// <summary> A bit-field of flags for specifying connection states. </summary>
    [Flags]
    public enum ConnectionState
    {
        /// <summary> A binary constant representing the closed flag. </summary>
        Closed = 0,
        /// <summary> A binary constant representing the open flag. </summary>
        Open = 1,
        /// <summary> A binary constant representing the connecting flag. </summary>
        Connecting = 2,
        /// <summary> A binary constant representing the executing flag. </summary>
        Executing = 4,
        /// <summary> A binary constant representing the fetching flag. </summary>
        Fetching = 8,
        /// <summary> A binary constant representing the broken flag. </summary>
        Broken = 16,        
    }
}
