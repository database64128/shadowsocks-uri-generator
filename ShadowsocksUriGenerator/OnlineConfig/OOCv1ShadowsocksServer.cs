using ShadowsocksUriGenerator.Protocols.Shadowsocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace ShadowsocksUriGenerator.OnlineConfig;

public class OOCv1ShadowsocksServer
{
    /// <inheritdoc cref="ShadowsocksServerConfig.Id"/>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <inheritdoc cref="ShadowsocksServerConfig.Name"/>
    public string Name { get; set; } = "";

    /// <inheritdoc cref="ShadowsocksServerConfig.Host"/>
    [JsonPropertyName("address")]
    public string Host { get; set; } = "";

    /// <inheritdoc cref="ShadowsocksServerConfig.Port"/>
    public int Port { get; set; }

    /// <inheritdoc cref="ShadowsocksServerConfig.Method"/>
    public string Method { get; set; } = "2022-blake3-aes-256-gcm";

    /// <summary>
    /// Gets or sets the password for the server.
    /// </summary>
    public string Password { get; set; } = "";

    /// <inheritdoc cref="ShadowsocksServerConfig.PluginName"/>
    public string? PluginName { get; set; }

    /// <inheritdoc cref="ShadowsocksServerConfig.PluginVersion"/>
    public string? PluginVersion { get; set; }

    /// <inheritdoc cref="ShadowsocksServerConfig.PluginOptions"/>
    public string? PluginOptions { get; set; }

    /// <inheritdoc cref="ShadowsocksServerConfig.PluginArguments"/>
    public string? PluginArguments { get; set; }

    /// <inheritdoc cref="ShadowsocksServerConfig.Group"/>
    public string? Group { get; set; }

    /// <inheritdoc cref="ShadowsocksServerConfig.Owner"/>
    public string? Owner { get; set; }

    /// <inheritdoc cref="ShadowsocksServerConfig.Tags"/>
    public IEnumerable<string>? Tags { get; set; }

    public OOCv1ShadowsocksServer()
    {
    }

    public OOCv1ShadowsocksServer(ShadowsocksServerConfig server)
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
