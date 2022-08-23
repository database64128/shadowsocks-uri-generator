namespace ShadowsocksUriGenerator.Federation.Protocols.Shadowsocks;

public class FederatedShadowsocksServerCredential
{
    public int Port { get; set; }

    public string? Method { get; set; }

    public string? Password { get; set; }

    public string? PluginOptions { get; set; }

    public string? PluginArguments { get; set; }
}
