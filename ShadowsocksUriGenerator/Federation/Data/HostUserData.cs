using ShadowsocksUriGenerator.Stats;

namespace ShadowsocksUriGenerator.Federation.Data;

public class HostUserData
{
    /// <summary>
    /// Gets or sets the user's total data usage stats.
    /// </summary>
    public DataUsage TotalDataUsage { get; set; } = new();
}
