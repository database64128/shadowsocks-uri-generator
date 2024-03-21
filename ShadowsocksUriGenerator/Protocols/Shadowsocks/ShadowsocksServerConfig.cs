using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace ShadowsocksUriGenerator.Protocols.Shadowsocks;

public class ShadowsocksServerConfig
{
    /// <summary>
    /// Gets or sets the server ID.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the server name.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Gets or sets the server address.
    /// </summary>
    public string Host { get; set; } = "";

    /// <summary>
    /// Gets or sets the server port.
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// Gets or sets the method used for the server.
    /// </summary>
    public string Method { get; set; } = "2022-blake3-aes-256-gcm";

    /// <summary>
    /// Gets or sets the user PSK.
    /// For Shadowsocks 2022 without EIH and legacy Shadowsocks,
    /// this is the main PSK.
    /// </summary>
    public string UserPSK { get; set; } = "";

    /// <summary>
    /// Gets or sets the identity PSKs.
    /// </summary>
    public IEnumerable<string> IdentityPSKs { get; set; } = [];

    /// <summary>
    /// Gets or sets the plugin name.
    /// Null when not using a plugin.
    /// </summary>
    public string? PluginName { get; set; }

    /// <summary>
    /// Gets or sets the required plugin version string.
    /// Null when not using a plugin.
    /// </summary>
    public string? PluginVersion { get; set; }

    /// <summary>
    /// Gets or sets the plugin options passed as environment variable SS_PLUGIN_OPTIONS.
    /// Null when not using a plugin.
    /// </summary>
    public string? PluginOptions { get; set; }

    /// <summary>
    /// Gets or sets the plugin startup arguments.
    /// Null when not using a plugin.
    /// </summary>
    public string? PluginArguments { get; set; }

    /// <summary>
    /// Gets or sets the node group
    /// this server belongs to.
    /// </summary>
    public string? Group { get; set; }

    /// <summary>
    /// Gets or sets the owner of the server.
    /// </summary>
    public string? Owner { get; set; }

    /// <summary>
    /// Gets or sets the list of annotated tags.
    /// </summary>
    public IEnumerable<string> Tags { get; set; } = [];

    /// <summary>
    /// Gets the string representation of host:port.
    /// IPv6 addresses are enclosed in square brackets ([]).
    /// </summary>
    public string GetHostPort() => Host.Contains(':') ? $"[{Host}]:{Port}" : $"{Host}:{Port}";

    /// <summary>
    /// Gets the password to the server by combining iPSKs and uPSK.
    /// </summary>
    /// <returns>The password to the server.</returns>
    public string GetPassword()
    {
        if (!IdentityPSKs.Any())
            return UserPSK;

        var length = IdentityPSKs.Count() + IdentityPSKs.Sum(x => x.Length) + UserPSK.Length;
        return string.Create(length, IdentityPSKs, (chars, iPSKs) =>
        {
            foreach (var iPSK in iPSKs)
            {
                iPSK.CopyTo(chars);
                chars[iPSK.Length] = ':';
                chars = chars[(iPSK.Length + 1)..];
            }

            UserPSK.CopyTo(chars);
        });
    }

    /// <summary>
    /// Gets an SIP002 URL representing the server.
    /// </summary>
    /// <returns>An SIP002 URL.</returns>
    public Uri ToUri()
    {
        var uriBuilder = new UriBuilder("ss", Host, Port)
        {
            UserName = Method,
            Password = Uri.EscapeDataString(GetPassword()),
            Fragment = Name,
        };

        if (!string.IsNullOrEmpty(PluginName))
        {
            var querySB = new StringBuilder("plugin=");

            querySB.Append(Uri.EscapeDataString(PluginName));

            if (!string.IsNullOrEmpty(PluginOptions))
            {
                querySB.Append("%3B"); // URI-escaped ';'
                querySB.Append(Uri.EscapeDataString(PluginOptions));
            }

            if (!string.IsNullOrEmpty(PluginVersion))
            {
                querySB.Append("&pluginVersion=");
                querySB.Append(Uri.EscapeDataString(PluginVersion));
            }

            if (!string.IsNullOrEmpty(PluginArguments))
            {
                querySB.Append("&pluginArguments=");
                querySB.Append(Uri.EscapeDataString(PluginArguments));
            }

            uriBuilder.Query = querySB.ToString();
        }

        return uriBuilder.Uri;
    }

    /// <summary>
    /// Parses an SIP002 URL into a Shadowsocks server configuration.
    /// </summary>
    /// <param name="url">The URL to parse.</param>
    /// <param name="serverConfig">The parsed server configuration.</param>
    /// <returns>Whether the parsing is successful.</returns>
    public static bool TryParse(string url, [NotNullWhen(true)] out ShadowsocksServerConfig? serverConfig)
    {
        serverConfig = null;
        return Uri.TryCreate(url, UriKind.Absolute, out var uri) && TryParse(uri, out serverConfig);
    }

    /// <summary>
    /// Parses an SIP002 URI into a Shadowsocks server configuration.
    /// </summary>
    /// <param name="uri">The URI to parse.</param>
    /// <param name="serverConfig">The parsed server configuration.</param>
    /// <returns>Whether the parsing is successful.</returns>
    public static bool TryParse(Uri uri, [NotNullWhen(true)] out ShadowsocksServerConfig? serverConfig)
    {
        serverConfig = null;

        // Check scheme.
        if (uri.Scheme != "ss")
            return false;

        // Parse userinfo.
        var unescapedUserinfo = Uri.UnescapeDataString(uri.UserInfo);
        var userinfoSplitArray = unescapedUserinfo.Split(':');
        if (userinfoSplitArray.Length < 2)
            return false;
        var method = userinfoSplitArray[0];
        var iPSKs = userinfoSplitArray[1..^1];
        var uPSK = userinfoSplitArray[^1];

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
            UserPSK = uPSK,
            IdentityPSKs = iPSKs,
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
