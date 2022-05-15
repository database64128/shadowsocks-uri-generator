namespace ShadowsocksUriGenerator.Federation.Config;

public class HostUserConfig
{
    public ulong Id { get; set; }
    public string Name { get; set; } = "";
    public string Password { get; set; } = "";

    /// <summary>
    /// Gets or sets the global data limit of the user in bytes.
    /// 0UL means no data limit.
    /// </summary>
    public ulong DataLimitInBytes { get; set; }

    /// <summary>
    /// Gets or sets the per-group data limit of the user in bytes.
    /// 0UL means no data limit.
    /// </summary>
    public ulong PerGroupDataLimitInBytes { get; set; }
}
