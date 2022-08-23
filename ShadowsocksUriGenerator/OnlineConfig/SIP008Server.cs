using ShadowsocksUriGenerator.Protocols.Shadowsocks;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ShadowsocksUriGenerator.OnlineConfig;

public class SIP008Server : IShadowsocksServerConfig
{
    /// <inheritdoc/>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <inheritdoc/>
    [JsonPropertyName("remarks")]
    public string Name { get; set; } = "";

    /// <inheritdoc/>
    [JsonPropertyName("server")]
    public string Host { get; set; } = "";

    /// <inheritdoc/>
    [JsonPropertyName("server_port")]
    public int Port { get; set; }

    /// <inheritdoc/>
    public string Method { get; set; } = "2022-blake3-aes-256-gcm";

    /// <inheritdoc/>
    public string Password { get; set; } = "";

    /// <inheritdoc/>
    [JsonPropertyName("plugin")]
    public string? PluginName { get; set; }

    /// <inheritdoc/>
    public string? PluginVersion { get; set; }

    /// <inheritdoc/>
    [JsonPropertyName("plugin_opts")]
    public string? PluginOptions { get; set; }

    /// <inheritdoc/>
    [JsonPropertyName("plugin_args")]
    public string? PluginArguments { get; set; }

    /// <inheritdoc/>
    public string? Group { get; set; }

    /// <inheritdoc/>
    public string? Owner { get; set; }

    /// <inheritdoc/>
    public List<string>? Tags { get; set; }

    public SIP008Server()
    {
    }

    public SIP008Server(IShadowsocksServerConfig server)
    {
        Id = server.Id;
        Name = server.Name;
        Host = server.Host;
        Port = server.Port;
        Method = server.Method;
        Password = server.Password;
        PluginName = server.PluginName;
        PluginVersion = server.PluginVersion;
        PluginOptions = server.PluginOptions;
        PluginArguments = server.PluginArguments;
        Group = server.Group;
        Owner = server.Owner;
        Tags = server.Tags;
    }

    public IShadowsocksServerConfig ToIShadowsocksServerConfig() => this;
}
