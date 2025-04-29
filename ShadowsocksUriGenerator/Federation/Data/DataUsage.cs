namespace ShadowsocksUriGenerator.Federation.Data;

public class DataUsage
{
    /// <summary>
    /// Gets or sets the data usage in bytes.
    /// </summary>
    public ulong BytesUsed { get; set; }

    /// <summary>
    /// Gets or sets the data remaining to be used in bytes.
    /// 0UL means used up.
    /// null means no data limit.
    /// </summary>
    public ulong? BytesRemaining { get; set; }
}
