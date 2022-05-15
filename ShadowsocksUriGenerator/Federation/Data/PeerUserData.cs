﻿using ShadowsocksUriGenerator.Protocols.Shadowsocks;
using ShadowsocksUriGenerator.Stats;
using System.Collections.Generic;

namespace ShadowsocksUriGenerator.Federation.Data;

public class PeerUserData
{
    public ulong Id { get; set; }
    public string Name { get; set; } = "";

    /// <summary>
    /// Stores user credentials for all host groups.
    /// Key is host group ID.
    /// Value is credential.
    /// </summary>
    public Dictionary<ulong, ShadowsocksServerCredential> Credentials { get; set; } = new();

    /// <summary>
    /// Stores user data usage stats for all host groups.
    /// Key is host group ID.
    /// Value is data usage.
    /// </summary>
    public Dictionary<ulong, DataUsage> DataUsageStats { get; set; } = new();
}