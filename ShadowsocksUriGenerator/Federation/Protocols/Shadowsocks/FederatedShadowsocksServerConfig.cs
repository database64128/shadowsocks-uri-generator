using System.Collections.Generic;

namespace ShadowsocksUriGenerator.Federation.Protocols.Shadowsocks;

public class FederatedShadowsocksServerConfig
{
    public string Name { get; set; } = "";

    public string Host { get; set; } = "";

    public int Port { get; set; }

    public string? Method { get; set; }

    public string? Password { get; set; }

    public string? PluginName { get; set; }

    public string? PluginVersion { get; set; }

    public string? PluginOptions { get; set; }

    public string? PluginArguments { get; set; }

    /// <summary>
    /// Gets or sets the owner of the server.
    /// </summary>
    public string? Owner { get; set; }

    /// <summary>
    /// Gets or sets the list of annotated tags.
    /// </summary>
    public List<string> Tags { get; set; } = [];
}
