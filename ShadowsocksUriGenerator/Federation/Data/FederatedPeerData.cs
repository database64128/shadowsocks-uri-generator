using OpenOnlineConfig.v1;
using ShadowsocksUriGenerator.Protocols.Shadowsocks;
using System.Collections.Generic;

namespace ShadowsocksUriGenerator.Federation.Data;

public class FederatedPeerData
{
    /// <summary>
    /// Gets or sets the peer's API endpoint.
    /// Updated by peer via OOCv1 federation API.
    /// Overrides the one defined in config.
    /// </summary>
    public OOCv1ApiToken? ApiEndpoint { get; set; }

    /// <summary>
    /// Stores all servers from this peer.
    /// Key is server ID. Starts from 0.
    /// Value is server config.
    /// </summary>
    public Dictionary<ulong, ShadowsocksServerConfig> Servers { get; set; } = new();
}
