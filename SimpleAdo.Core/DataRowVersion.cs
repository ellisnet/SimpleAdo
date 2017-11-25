namespace SimpleAdo
{
    /// <summary> Values that represent data row versions. </summary>
    public enum DataRowVersion
    {
        /// <summary> An enum constant representing the original option. </summary>
        Original = 256,
        /// <summary> An enum constant representing the current option. </summary>
        Current = 512,
        /// <summary> An enum constant representing the proposed option. </summary>
        Proposed = 1024,
        /// <summary> An enum constant representing the default option. </summary>
        Default = 1536,
    }
}
