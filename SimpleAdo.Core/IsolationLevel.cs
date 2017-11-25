namespace SimpleAdo
{
    /// <summary> Values that represent isolation levels. </summary>
    public enum IsolationLevel
    {
        /// <summary> An enum constant representing the chaos option. </summary>
        Chaos = 16,
        /// <summary> An enum constant representing the read uncommitted option. </summary>
        ReadUncommitted = 256,
        /// <summary> An enum constant representing the read committed option. </summary>
        ReadCommitted = 4096,
        /// <summary> An enum constant representing the repeatable read option. </summary>
        RepeatableRead = 65536,
        /// <summary> An enum constant representing the serializable option. </summary>
        Serializable = 1048576,
        /// <summary> An enum constant representing the snapshot option. </summary>
        Snapshot = 16777216,
        /// <summary> An enum constant representing the unspecified option. </summary>
        Unspecified = -1
    }
}
