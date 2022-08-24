using ShadowsocksUriGenerator.Protocols.Shadowsocks;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace ShadowsocksUriGenerator.OnlineConfig;

public class ShadowsocksGoClientConfig
{
    public string Name { get; set; } = "";
    public string Endpoint { get; set; } = "";
    public string Protocol { get; set; } = "";
    public int DialerFwmark { get; set; }

    public bool EnableTCP { get; set; } = true;
    public bool DialerTFO { get; set; } = true;

    public bool EnableUDP { get; set; } = true;
    public int MTU { get; set; } = 1500;

    public string PSK { get; set; } = "";
    [JsonPropertyName("iPSKs")]
    public IEnumerable<string>? IdentityPSKs { get; set; }
    public string? PaddingPolicy { get; set; }

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
