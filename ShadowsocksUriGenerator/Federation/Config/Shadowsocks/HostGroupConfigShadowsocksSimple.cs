using ShadowsocksUriGenerator.Protocols.Shadowsocks;
using System.Collections.Generic;

namespace ShadowsocksUriGenerator.Federation.Config.Shadowsocks;

public class HostGroupConfigShadowsocksSimple : HostGroupConfigShadowsocks
{
    /// <summary>
    /// Stores server credentials for all users.
    /// Key is user ID.
    /// Value is credential.
    /// </summary>
    public Dictionary<ulong, ShadowsocksServerCredential> Credentials { get; set; } = new();
}
