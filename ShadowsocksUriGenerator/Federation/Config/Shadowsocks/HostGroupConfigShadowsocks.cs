using ShadowsocksUriGenerator.Federation.Protocols.Shadowsocks;
using System.Collections.Generic;

namespace ShadowsocksUriGenerator.Federation.Config.Shadowsocks;

public abstract class HostGroupConfigShadowsocks
{
    public ulong Id { get; set; }
    public string Name { get; set; } = "";

    /// <summary>
    /// Gets or sets the group owner.
    /// </summary>
    public ulong OwnerId { get; set; }

    /// <summary>
    /// Stores all servers in this group.
    /// Key is server ID. Starts from 0.
    /// Value is server config.
    /// </summary>
    public Dictionary<ulong, FederatedShadowsocksServerConfig> Servers { get; set; } = [];
}
