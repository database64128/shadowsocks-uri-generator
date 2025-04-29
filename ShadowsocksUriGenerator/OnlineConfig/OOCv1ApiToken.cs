namespace ShadowsocksUriGenerator.OnlineConfig;

/// <summary>
/// OOCv1 API access information.
/// Serialize and deserialize in camelCase.
/// </summary>
public record OOCv1ApiToken(int Version, string BaseUrl, string Secret, string UserId, string? CertSha256);
