using ShadowsocksUriGenerator.Outline;

namespace ShadowsocksUriGenerator.Federation.Config.Shadowsocks;

public class HostGroupConfigOutline : HostGroupConfigShadowsocks
{
    /// <summary>
    /// Gets or sets the Outline API key record.
    /// </summary>
    public ApiKey OutlineApiKey { get; set; } = new("", null);
}
