using OpenOnlineConfig.v1;
using System.Collections.Generic;

namespace ShadowsocksUriGenerator.OnlineConfig;

public class OOCv1ShadowsocksConfig : OOCv1ConfigBase
{
    /// <summary>
    /// Gets or sets the list of Shadowsocks servers.
    /// </summary>
    public List<OOCv1ShadowsocksServer> Shadowsocks { get; set; } = new();

    /// <summary>
    /// Initializes an OOCv1 Shadowsocks config.
    /// </summary>
    public OOCv1ShadowsocksConfig() => Protocols.Add("shadowsocks");
}
