using OpenOnlineConfig.v1;

namespace ShadowsocksUriGenerator.Federation.Config;

public class FederatedPeerConfig
{
    public ulong Id { get; set; }
    public string Name { get; set; } = "";
    public string Password { get; set; } = "";

    /// <summary>
    /// Gets or sets the peer's API endpoint.
    /// Leave this property null if the peer does not
    /// have a public API endpoint.
    /// </summary>
    public OOCv1ApiToken? ApiEndpoint { get; set; }
}
