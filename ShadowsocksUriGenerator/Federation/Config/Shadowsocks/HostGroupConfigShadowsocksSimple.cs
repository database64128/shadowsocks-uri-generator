using ShadowsocksUriGenerator.Federation.Protocols.Shadowsocks;

namespace ShadowsocksUriGenerator.Federation.Config.Shadowsocks;

public class HostGroupConfigShadowsocksSimple : HostGroupConfigShadowsocks
{
    /// <summary>
    /// Stores server credentials for all users.
    /// Key is user ID.
    /// Value is credential.
    /// </summary>
    public Dictionary<ulong, FederatedShadowsocksServerCredential> Credentials { get; set; } = [];
}
