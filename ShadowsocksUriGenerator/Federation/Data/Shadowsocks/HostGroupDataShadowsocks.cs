using ShadowsocksUriGenerator.Federation.Protocols.Shadowsocks;

namespace ShadowsocksUriGenerator.Federation.Data.Shadowsocks;

public class HostGroupDataShadowsocks
{
    /// <summary>
    /// Stores server credentials for all users.
    /// Key is user ID.
    /// Value is credential.
    /// </summary>
    public Dictionary<ulong, FederatedShadowsocksServerCredential> Credentials { get; set; } = [];

    /// <summary>
    /// Gets or sets the group's total data usage stats.
    /// </summary>
    public DataUsage TotalDataUsage { get; set; } = new();

    /// <summary>
    /// Gets or sets data usage stats of all users.
    /// </summary>
    public Dictionary<ulong, DataUsage> UserDataUsageStats { get; set; } = [];
}
