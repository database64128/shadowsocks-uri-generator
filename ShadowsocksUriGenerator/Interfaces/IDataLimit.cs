namespace ShadowsocksUriGenerator
{
    public interface IDataLimit
    {
        /// <summary>
        /// Gets or sets the global data limit in bytes.
        /// </summary>
        public ulong DataLimitInBytes { get; set; }
    }
}
