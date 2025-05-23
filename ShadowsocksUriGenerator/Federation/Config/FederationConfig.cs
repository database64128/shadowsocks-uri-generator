﻿using ShadowsocksUriGenerator.Federation.Config.Shadowsocks;

namespace ShadowsocksUriGenerator.Federation.Config;

public class FederationConfig
{
    /// <summary>
    /// Defines the default federation configuration version
    /// used by this version of the app.
    /// </summary>
    public static readonly int _defaultVersion = 1;

    /// <summary>
    /// Gets or sets the configuration version number.
    /// Update if older config is present.
    /// Throw error if config is newer than supported.
    /// </summary>
    public int Version { get; set; } = _defaultVersion;

    public Dictionary<ulong, HostUserConfig> HostUsers { get; set; } = [];

    public Dictionary<ulong, HostGroupConfigShadowsocksSimple> HostGroupsShadowsocksSimple { get; set; } = [];

    public Dictionary<ulong, HostGroupConfigShadowsocksManager> HostGroupsShadowsocksManager { get; set; } = [];

    public Dictionary<ulong, HostGroupConfigOutline> HostGroupsOutline { get; set; } = [];

    public Dictionary<ulong, FederatedPeerConfig> FederatedPeers { get; set; } = [];
}
