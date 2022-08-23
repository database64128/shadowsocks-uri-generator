using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace ShadowsocksUriGenerator.Protocols.Shadowsocks;

public class ShadowsocksServerConfig : IShadowsocksServerConfig
{
    /// <inheritdoc/>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <inheritdoc/>
    public string Name { get; set; } = "";

    /// <inheritdoc/>
    public string Host { get; set; } = "";

    /// <inheritdoc/>
    public int Port { get; set; }

    /// <inheritdoc/>
    public string Method { get; set; } = "2022-blake3-aes-256-gcm";

    /// <inheritdoc/>
    public string Password { get; set; } = "";

    /// <inheritdoc/>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? PluginName { get; set; }

    /// <inheritdoc/>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? PluginVersion { get; set; }

    /// <inheritdoc/>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? PluginOptions { get; set; }

    /// <inheritdoc/>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? PluginArguments { get; set; }

    /// <inheritdoc/>
    public string? Group { get; set; }

    /// <inheritdoc/>
    public string? Owner { get; set; }

    /// <inheritdoc/>
    public List<string>? Tags { get; set; }

    public ShadowsocksServerConfig()
    {
    }

    public ShadowsocksServerConfig(IShadowsocksServerConfig server)
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

    public bool Equals(ShadowsocksServerConfig? other) => Id == other?.Id;
    public override bool Equals(object? obj) => Equals(obj as ShadowsocksServerConfig);
    public override int GetHashCode() => Id.GetHashCode();
    public override string ToString() => Name;

    public IShadowsocksServerConfig ToIShadowsocksServerConfig() => this;

    public static bool TryParse(string url, [NotNullWhen(true)] out ShadowsocksServerConfig? serverConfig)
    {
        serverConfig = null;
        return Uri.TryCreate(url, UriKind.Absolute, out var uri) && TryParse(uri, out serverConfig);
    }

    public static bool TryParse(Uri uri, [NotNullWhen(true)] out ShadowsocksServerConfig? serverConfig)
    {
        serverConfig = null;

        // Check scheme.
        if (uri.Scheme != "ss")
            return false;

        // Parse userinfo.
        var userinfoSplitArray = uri.UserInfo.Split(':', 2);
        if (userinfoSplitArray.Length != 2)
            return false;
        var method = userinfoSplitArray[0];
        var password = Uri.UnescapeDataString(userinfoSplitArray[1]);

        // Parse host.
        var host = uri.HostNameType == UriHostNameType.IPv6 ? uri.Host[1..^1] : uri.Host;

        // Parse name.
        var escapedFragment = string.IsNullOrEmpty(uri.Fragment) ? uri.Fragment : uri.Fragment[1..];
        var name = Uri.UnescapeDataString(escapedFragment);

        // Create server config.
        serverConfig = new()
        {
            Name = name,
            Host = host,
            Port = uri.Port,
            Method = method,
            Password = password,
        };

        // Find plugin queries.
        var parsedQueriesArray = uri.Query.Split('?', '&');

        string? pluginQueryContent = null;
        string? pluginVersion = null;
        string? pluginArguments = null;

        foreach (var query in parsedQueriesArray)
        {
            if (query.StartsWith("plugin=") && query.Length > 7)
            {
                pluginQueryContent = Uri.UnescapeDataString(query[7..]); // remove "plugin=" and unescape
            }

            if (query.StartsWith("pluginVersion=") && query.Length > 14)
            {
                pluginVersion = Uri.UnescapeDataString(query[14..]);
            }

            if (query.StartsWith("pluginArguments=") && query.Length > 16)
            {
                pluginArguments = Uri.UnescapeDataString(query[16..]);
            }
        }

        if (string.IsNullOrEmpty(pluginQueryContent)) // no plugin
            return true;

        var parsedPluginQueryArray = pluginQueryContent.Split(';', 2);

        switch (parsedPluginQueryArray.Length)
        {
            case 1:
                serverConfig.PluginName = parsedPluginQueryArray[0];
                break;
            case 2:
                serverConfig.PluginName = parsedPluginQueryArray[0];
                serverConfig.PluginOptions = parsedPluginQueryArray[1];
                break;
        }

        serverConfig.PluginVersion = pluginVersion;
        serverConfig.PluginArguments = pluginArguments;

        return true;
    }
}
