using System.Collections.Generic;

namespace ShadowsocksUriGenerator.Federation.Config.Shadowsocks;

public class HostGroupConfigShadowsocksManager : HostGroupConfigShadowsocks
{
    public string? UnixDomainSocketPath { get; set; }
    public string? UDPHost { get; set; }
    public int UDPPort { get; set; }

    /// <summary>
    /// Gets or sets the minimum server port number.
    /// Use together with <see cref="MaxServerPort"/>.
    /// Do not use with <see cref="ServerPortAllocationRange"/>.
    /// </summary>
    public int MinServerPort { get; set; }

    /// <summary>
    /// Gets or sets the maximum server port number.
    /// Use together with <see cref="MinServerPort"/>.
    /// Do not use with <see cref="ServerPortAllocationRange"/>.
    /// </summary>
    public int MaxServerPort { get; set; }

    /// <summary>
    /// Gets or sets a list of ports to be allocated for Shadowsocks server.
    /// Do not use with <see cref="MinServerPort"/> or <see cref="MaxServerPort"/>.
    /// </summary>
    public List<int>? ServerPortAllocationRange { get; set; }
}
