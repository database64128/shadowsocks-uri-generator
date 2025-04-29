using ShadowsocksUriGenerator.Outline;

namespace ShadowsocksUriGenerator.Federation.Config.Shadowsocks;

public class HostGroupConfigOutline : HostGroupConfigShadowsocks
{
    /// <summary>
    /// Gets or sets the Outline API key record.
    /// </summary>
    public required OutlineApiKey OutlineApiKey { get; set; }
}
