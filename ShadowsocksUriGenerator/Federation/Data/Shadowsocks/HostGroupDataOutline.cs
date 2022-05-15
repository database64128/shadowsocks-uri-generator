using ShadowsocksUriGenerator.Outline;
using System.Collections.Generic;

namespace ShadowsocksUriGenerator.Federation.Data.Shadowsocks;

public class HostGroupDataOutline : HostGroupDataShadowsocks
{
    /// <summary>
    /// Gets or sets the Outline server information object.
    /// </summary>
    public ServerInfo OutlineServerInfo { get; set; } = new();

    /// <summary>
    /// Gets or sets the dictionary
    /// that stores user ID to access key ID mappings.
    /// Key is user ID.
    /// Value is access key ID.
    /// </summary>
    public Dictionary<ulong, int> UserAccessKeyDict { get; set; } = new();

    /// <summary>
    /// Gets or sets the per-user data limit of the group in bytes.
    /// 0UL means no data limit.
    /// </summary>
    public ulong PerUserDataLimitInBytes { get; set; }

    /// <summary>
    /// Gets or sets the dictionary
    /// that stores all users' data limit.
    /// Key is user ID.
    /// Value is data limit.
    /// Value 0UL means no data limit.
    /// </summary>
    public Dictionary<ulong, ulong> UserDataLimitDict { get; set; } = new();
}
