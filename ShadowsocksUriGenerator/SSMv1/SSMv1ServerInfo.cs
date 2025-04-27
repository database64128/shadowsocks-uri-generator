namespace ShadowsocksUriGenerator.SSMv1;

/// <summary>
/// Server information response.
/// </summary>
public class SSMv1ServerInfo
{
    /// <summary>
    /// Gets or sets the server name.
    /// </summary>
    public required string Server { get; set; }

    /// <summary>
    /// Gets or sets the SSM API version.
    /// </summary>
    public required string ApiVersion { get; set; }
}
