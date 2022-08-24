namespace ShadowsocksUriGenerator.OnlineConfig;

public class SingBoxMultiplexConfig
{
    public bool Enabled { get; set; }
    public string? Protocol { get; set; }
    public int MaxConnections { get; set; }
    public int MinStreams { get; set; }
    public int MaxStreams { get; set; }
}
