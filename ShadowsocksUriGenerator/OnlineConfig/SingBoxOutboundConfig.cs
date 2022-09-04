using ShadowsocksUriGenerator.Protocols.Shadowsocks;
using System.Collections.Generic;

namespace ShadowsocksUriGenerator.OnlineConfig;

public class SingBoxOutboundConfig
{
    public string Type { get; set; } = "";
    public string Tag { get; set; } = "";

    #region ServerOptions
    public string? Server { get; set; }
    public int ServerPort { get; set; }
    #endregion

    #region ShadowsocksOutboundOptions
    public string? Method { get; set; }
    public string? Password { get; set; }
    public string? Network { get; set; }
    public bool UdpOverTcp { get; set; }
    public SingBoxMultiplexConfig? Multiplex { get; set; }
    #endregion

    #region DialerOptions
    public string? Detour { get; set; }
    public string? BindInterface { get; set; }
    public string? BindAddress { get; set; }
    public int RoutingMark { get; set; }
    public bool ReuseAddr { get; set; }
    public string? ConnectTimeout { get; set; }
    public bool TcpFastOpen { get; set; }
    public string? DomainStrategy { get; set; }
    public string? FallbackDelay { get; set; }
    #endregion

    #region SelectorOutboundOptions
    public IEnumerable<string>? Outbounds { get; set; }
    public string? Default { get; set; }
    #endregion

    public SingBoxOutboundConfig()
    {
    }

    public SingBoxOutboundConfig(
        ShadowsocksServerConfig server,
        string? network,
        bool uot,
        bool multiplex,
        string? multiplexProtocol,
        int multiplexMaxConnections,
        int multiplexMinStreams,
        int multiplexMaxStreams,
        string? detour,
        string? bindInterface,
        string? bindAddress,
        int routingMark,
        bool reuseAddr,
        string? connectTimeout,
        bool tfo,
        string? domainStrategy,
        string? fallbackDelay)
    {
        Type = "shadowsocks";
        Tag = server.Name;

        Server = server.Host;
        ServerPort = server.Port;
        Method = server.Method;
        Password = server.GetPassword();
        Network = network;
        UdpOverTcp = uot;
        if (multiplex)
            Multiplex = new()
            {
                Enabled = true,
                Protocol = multiplexProtocol,
                MaxConnections = multiplexMaxConnections,
                MinStreams = multiplexMinStreams,
                MaxStreams = multiplexMaxStreams,
            };

        Detour = detour;
        BindInterface = bindInterface;
        BindAddress = bindAddress;
        RoutingMark = routingMark;
        ReuseAddr = reuseAddr;
        ConnectTimeout = connectTimeout;
        TcpFastOpen = tfo;
        DomainStrategy = domainStrategy;
        FallbackDelay = fallbackDelay;
    }
}
