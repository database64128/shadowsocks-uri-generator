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
    public string? Plugin { get; set; }
    public string? PluginOpts { get; set; }
    public string? Network { get; set; }
    public bool UdpOverTcp { get; set; }
    public SingBoxMultiplexConfig? Multiplex { get; set; }
    #endregion

    #region DialerOptions
    public string? Detour { get; set; }
    public string? BindInterface { get; set; }
    public string? Inet4BindAddress { get; set; }
    public string? Inet6BindAddress { get; set; }
    public int RoutingMark { get; set; }
    public bool ReuseAddr { get; set; }
    public string? ConnectTimeout { get; set; }
    public bool TcpFastOpen { get; set; }
    public bool UdpFragment { get; set; }
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
        string? inet4BindAddress,
        string? inet6BindAddress,
        int routingMark,
        bool reuseAddr,
        string? connectTimeout,
        bool tcpFastOpen,
        bool udpFragment,
        string? domainStrategy,
        string? fallbackDelay)
    {
        Type = "shadowsocks";
        Tag = server.Name;

        Server = server.Host;
        ServerPort = server.Port;
        Method = server.Method;
        Password = server.GetPassword();
        Plugin = server.PluginName;
        PluginOpts = server.PluginOptions;
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
        Inet4BindAddress = inet4BindAddress;
        Inet6BindAddress = inet6BindAddress;
        RoutingMark = routingMark;
        ReuseAddr = reuseAddr;
        ConnectTimeout = connectTimeout;
        TcpFastOpen = tcpFastOpen;
        UdpFragment = udpFragment;
        DomainStrategy = domainStrategy;
        FallbackDelay = fallbackDelay;
    }
}
