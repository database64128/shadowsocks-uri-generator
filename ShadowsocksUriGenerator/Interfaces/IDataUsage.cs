namespace ShadowsocksUriGenerator
{
    public interface IDataUsage
    {
        /// <summary>
        /// Gets or sets the data usage in bytes.
        /// </summary>
        public ulong BytesUsed { get; set; }

        /// <summary>
        /// Gets or sets the data remaining to be used in bytes.
        /// </summary>
        public ulong BytesRemaining { get; set; }
    }
}
