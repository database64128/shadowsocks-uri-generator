using ShadowsocksUriGenerator.Protocols.Shadowsocks;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace ShadowsocksUriGenerator.OnlineConfig;

public class ShadowsocksGoClientConfig
{
    public string Name { get; set; } = "";
    public string? Endpoint { get; set; }
    public string Protocol { get; set; } = "";
    public int DialerFwmark { get; set; }

    public bool EnableTCP { get; set; } = true;
    public bool DialerTFO { get; set; } = true;

    public bool EnableUDP { get; set; } = true;
    public int MTU { get; set; } = 1500;

    public string? PSK { get; set; }
    [JsonPropertyName("iPSKs")]
    public IEnumerable<string>? IdentityPSKs { get; set; }
    public string? PaddingPolicy { get; set; }

    /// <summary>
    /// Creates an empty client config.
    /// </summary>
    public ShadowsocksGoClientConfig()
    {
    }

    /// <summary>
    /// Creates a direct client config.
    /// </summary>
    /// <param name="disableTCP">Whether to disable TCP.</param>
    /// <param name="disableTFO">Whether to disable TCP Fast Open.</param>
    /// <param name="disableUDP">Whether to disable UDP.</param>
    /// <param name="dialerFwmark">Set a fwmark for sockets.</param>
    /// <param name="mtu">The path MTU between client and server.</param>
    public ShadowsocksGoClientConfig(
        bool disableTCP,
        bool disableTFO,
        bool disableUDP,
        int dialerFwmark,
        int mtu)
    {
        Name = "direct";
        Protocol = "direct";
        DialerFwmark = dialerFwmark;
        EnableTCP = !disableTCP;
        DialerTFO = !disableTFO;
        EnableUDP = !disableUDP;
        MTU = mtu;
    }

    /// <summary>
    /// Creates a Shadowsocks client config.
    /// </summary>
    /// <param name="server">The Shadowsocks server config.</param>
    /// <param name="paddingPolicy">The padding policy to use for outgoing Shadowsocks traffic.</param>
    /// <param name="disableTCP">Whether to disable TCP.</param>
    /// <param name="disableTFO">Whether to disable TCP Fast Open.</param>
    /// <param name="disableUDP">Whether to disable UDP.</param>
    /// <param name="dialerFwmark">Set a fwmark for sockets.</param>
    /// <param name="mtu">The path MTU between client and server.</param>
    public ShadowsocksGoClientConfig(
        ShadowsocksServerConfig server,
        string? paddingPolicy,
        bool disableTCP,
        bool disableTFO,
        bool disableUDP,
        int dialerFwmark,
        int mtu)
    {
        Name = server.Name;
        Endpoint = server.GetHostPort();
        Protocol = server.Method;
        DialerFwmark = dialerFwmark;
        EnableTCP = !disableTCP;
        DialerTFO = !disableTFO;
        EnableUDP = !disableUDP;
        MTU = mtu;
        PSK = server.UserPSK;
        IdentityPSKs = server.IdentityPSKs.Any() ? server.IdentityPSKs : null;
        PaddingPolicy = paddingPolicy;
    }
}
