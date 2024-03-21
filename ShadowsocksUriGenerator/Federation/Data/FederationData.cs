using ShadowsocksUriGenerator.Federation.Data.Shadowsocks;
using System.Collections.Generic;

namespace ShadowsocksUriGenerator.Federation.Data;

public class FederationData
{
    /// <summary>
    /// Defines the default federation data format version
    /// used by this version of the app.
    /// </summary>
    public static readonly int _defaultVersion = 1;

    /// <summary>
    /// Gets or sets the data format version number.
    /// Update if older data format is present.
    /// Throw error if data format is newer than supported.
    /// </summary>
    public int Version { get; set; } = _defaultVersion;

    public Dictionary<ulong, HostUserData> HostUsers { get; set; } = [];

    public Dictionary<ulong, HostGroupDataShadowsocksManager> HostGroupsShadowsocksManager { get; set; } = [];

    public Dictionary<ulong, HostGroupDataOutline> HostGroupsOutline { get; set; } = [];

    public Dictionary<ulong, FederatedPeerData> FederatedPeers { get; set; } = [];
}
