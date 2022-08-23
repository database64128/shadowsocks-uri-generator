using ShadowsocksUriGenerator.Protocols.Shadowsocks;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ShadowsocksUriGenerator.OnlineConfig;

public class OOCv1ShadowsocksServer : IShadowsocksServerConfig
{
    /// <inheritdoc/>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <inheritdoc/>
    public string Name { get; set; } = "";

    /// <inheritdoc/>
    [JsonPropertyName("address")]
    public string Host { get; set; } = "";

    /// <inheritdoc/>
    public int Port { get; set; }

    /// <inheritdoc/>
    public string Method { get; set; } = "2022-blake3-aes-256-gcm";

    /// <inheritdoc/>
    public string Password { get; set; } = "";

    /// <inheritdoc/>
    public string? PluginName { get; set; }

    /// <inheritdoc/>
    public string? PluginVersion { get; set; }

    /// <inheritdoc/>
    public string? PluginOptions { get; set; }

    /// <inheritdoc/>
    public string? PluginArguments { get; set; }

    /// <inheritdoc/>
    public string? Group { get; set; }

    /// <inheritdoc/>
    public string? Owner { get; set; }

    /// <inheritdoc/>
    public List<string>? Tags { get; set; }

    public OOCv1ShadowsocksServer()
    {
    }

    public OOCv1ShadowsocksServer(IShadowsocksServerConfig server)
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
