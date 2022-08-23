using System.Collections.Generic;

namespace ShadowsocksUriGenerator.OnlineConfig;

public class SIP008Config
{
    /// <summary>
    /// Gets or sets the SIP008 document version.
    /// </summary>
    public int Version { get; set; } = 1;

    /// <summary>
    /// Gets or sets the username.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Gets or sets the user ID.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the data usage in bytes.
    /// </summary>
    public ulong? BytesUsed { get; set; }

    /// <summary>
    /// Gets or sets the data remaining to be used in bytes.
    /// </summary>
    public ulong? BytesRemaining { get; set; }

    /// <summary>
    /// Gets or sets the list of servers.
    /// </summary>
    public List<SIP008Server> Servers { get; set; } = new();
}
