using ShadowsocksUriGenerator.Protocols.Shadowsocks;
using System.Text.Json.Serialization;

namespace ShadowsocksUriGenerator.OnlineConfig;

public class SIP008Server
{
    /// <inheritdoc cref="ShadowsocksServerConfig.Id"/>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <inheritdoc cref="ShadowsocksServerConfig.Name"/>
    [JsonPropertyName("remarks")]
    public string Name { get; set; } = "";

    /// <inheritdoc cref="ShadowsocksServerConfig.Host"/>
    [JsonPropertyName("server")]
    public string Host { get; set; } = "";

    /// <inheritdoc cref="ShadowsocksServerConfig.Port"/>
    [JsonPropertyName("server_port")]
    public int Port { get; set; }

    /// <inheritdoc cref="ShadowsocksServerConfig.Method"/>
    public string Method { get; set; } = "2022-blake3-aes-256-gcm";

    /// <inheritdoc cref="OOCv1ShadowsocksServer.Password"/>
    public string Password { get; set; } = "";

    /// <inheritdoc cref="ShadowsocksServerConfig.PluginName"/>
    [JsonPropertyName("plugin")]
    public string? PluginName { get; set; }

    /// <inheritdoc cref="ShadowsocksServerConfig.PluginVersion"/>
    public string? PluginVersion { get; set; }

    /// <inheritdoc cref="ShadowsocksServerConfig.PluginOptions"/>
    [JsonPropertyName("plugin_opts")]
    public string? PluginOptions { get; set; }

    /// <inheritdoc cref="ShadowsocksServerConfig.PluginArguments"/>
    [JsonPropertyName("plugin_args")]
    public string? PluginArguments { get; set; }

    /// <inheritdoc cref="ShadowsocksServerConfig.Group"/>
    public string? Group { get; set; }

    /// <inheritdoc cref="ShadowsocksServerConfig.Owner"/>
    public string? Owner { get; set; }

    /// <inheritdoc cref="ShadowsocksServerConfig.Tags"/>
    public IEnumerable<string>? Tags { get; set; }

    public SIP008Server()
    {
    }

    public SIP008Server(ShadowsocksServerConfig server)
    {
        Id = server.Id;
        Name = server.Name;
        Host = server.Host;
        Port = server.Port;
        Method = server.Method;
        Password = server.GetPassword();
        PluginName = server.PluginName;
        PluginVersion = server.PluginVersion;
        PluginOptions = server.PluginOptions;
        PluginArguments = server.PluginArguments;
        Group = server.Group;
        Owner = server.Owner;
        Tags = server.Tags.Any() ? server.Tags : null;
    }
}
