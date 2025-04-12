namespace ShadowsocksUriGenerator.SSMv1;

/// <summary>
/// Traffic stats.
/// </summary>
public class SSMv1StatsBase
{
    /// <summary>
    /// Gets or sets the number of UDP packets transferred from server to client.
    /// </summary>
    public ulong DownlinkPackets { get; set; }

    /// <summary>
    /// Gets or sets the number of TCP and UDP payload bytes transferred from server to client.
    /// </summary>
    public ulong DownlinkBytes { get; set; }

    /// <summary>
    /// Gets or sets the number of UDP packets transferred from client to server.
    /// </summary>
    public ulong UplinkPackets { get; set; }

    /// <summary>
    /// Gets or sets the number of TCP and UDP payload bytes transferred from client to server.
    /// </summary>
    public ulong UplinkBytes { get; set; }

    /// <summary>
    /// Gets or sets the number of TCP sessions.
    /// </summary>
    public ulong TcpSessions { get; set; }

    /// <summary>
    /// Gets or sets the number of UDP sessions.
    /// </summary>
    public ulong UdpSessions { get; set; }

    /// <summary>
    /// Resets the stats to zero.
    /// </summary>
    public void Clear()
    {
        DownlinkPackets = 0UL;
        DownlinkBytes = 0UL;
        UplinkPackets = 0UL;
        UplinkBytes = 0UL;
        TcpSessions = 0UL;
        UdpSessions = 0UL;
    }
}

/// <summary>
/// Traffic stats response.
/// </summary>
public class SSMv1Stats : SSMv1StatsBase
{
    /// <summary>
    /// Per-user traffic stats.
    /// </summary>
    public SSMv1UserStats[] Users { get; set; } = [];
}
