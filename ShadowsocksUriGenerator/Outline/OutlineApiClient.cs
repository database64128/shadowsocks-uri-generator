using System.Net.Http.Json;
using System.Net.Security;
using System.Security.Cryptography;

namespace ShadowsocksUriGenerator.Outline;

public class OutlineApiClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly Uri _serverInfoUri;
    private readonly Uri _serverNameUri;
    private readonly Uri _serverHostnameUri;
    private readonly Uri _serverMetricsUri;
    private readonly Uri _accessKeysUri;
    private readonly Uri _accessKeysPortUri;
    private readonly Uri _accessKeysSlashUri;
    private readonly Uri _dataUsageUri;
    private readonly Uri _dataLimitUri;
    private readonly bool _disposeHttpClient;
    private bool _disposedValue;

    /// <summary>
    /// Creates an Outline API client for the specified API key.
    /// </summary>
    /// <param name="httpClient">
    /// Generic HTTP client to use when the API key does not pin a certificate fingerprint.
    /// This <see cref="HttpClient"/> instance is ignored when the API key pins a certificate fingerprint with <see cref="OutlineApiKey.CertSha256"/>.
    /// Remember to set a timeout on the instance before passing it to this method. A 30s timeout is recommended.
    /// </param>
    /// <param name="apiKey">Outline API key.</param>
    public OutlineApiClient(HttpClient httpClient, OutlineApiKey apiKey)
    {
        if (string.IsNullOrEmpty(apiKey.CertSha256))
        {
            _httpClient = httpClient;
        }
        else
        {
            SslClientAuthenticationOptions sslClientAuthenticationOptions = new()
            {
                RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
                {
                    string? sha256Fingerprint = certificate?.GetCertHashString(HashAlgorithmName.SHA256);
                    return string.Equals(apiKey.CertSha256, sha256Fingerprint, StringComparison.OrdinalIgnoreCase);
                },
            };
            SocketsHttpHandler socketsHttpHandler = new()
            {
                SslOptions = sslClientAuthenticationOptions,
            };
            _httpClient = new(socketsHttpHandler);
            _disposeHttpClient = true;
        }

        Uri baseUri = apiKey.ApiUrl.AbsolutePath.EndsWith('/') ? apiKey.ApiUrl : new(apiKey.ApiUrl.AbsoluteUri + "/");
        _serverInfoUri = new(baseUri, "server");
        _serverNameUri = new(baseUri, "name");
        _serverHostnameUri = new(baseUri, "server/hostname-for-access-keys");
        _serverMetricsUri = new(baseUri, "metrics/enabled");
        _accessKeysUri = new(baseUri, "access-keys");
        _accessKeysPortUri = new(baseUri, "server/port-for-new-access-keys");
        _accessKeysSlashUri = new(baseUri, "access-keys/");
        _dataUsageUri = new(baseUri, "metrics/transfer");
        _dataLimitUri = new(baseUri, "server/access-key-data-limit");
    }

    /// <inheritdoc cref="HttpClient.Timeout"/>
    public TimeSpan Timeout
    {
        get => _httpClient.Timeout;
        set => _httpClient.Timeout = value;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposeHttpClient && !_disposedValue)
        {
            if (disposing)
            {
                _httpClient.Dispose();
            }
            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public Task<OutlineServerInfo?> GetServerInfoAsync(CancellationToken cancellationToken = default)
        => _httpClient.GetFromJsonAsync(_serverInfoUri, OutlineJsonSerializerContext.Default.OutlineServerInfo, cancellationToken);

    public Task<HttpResponseMessage> SetServerNameAsync(string name, CancellationToken cancellationToken = default)
        => _httpClient.PutAsJsonAsync(_serverNameUri, new OutlineServerName(name), OutlineJsonSerializerContext.Default.OutlineServerName, cancellationToken);

    public Task<HttpResponseMessage> SetServerHostnameAsync(string hostname, CancellationToken cancellationToken = default)
        => _httpClient.PutAsJsonAsync(_serverHostnameUri, new OutlineServerHostname(hostname), OutlineJsonSerializerContext.Default.OutlineServerHostname, cancellationToken);

    public Task<HttpResponseMessage> SetServerMetricsAsync(bool enabled, CancellationToken cancellationToken = default)
        => _httpClient.PutAsJsonAsync(_serverMetricsUri, new OutlineMetrics(enabled), OutlineJsonSerializerContext.Default.OutlineMetrics, cancellationToken);

    public Task<OutlineAccessKeysResponse?> GetAccessKeysAsync(CancellationToken cancellationToken = default)
        => _httpClient.GetFromJsonAsync(_accessKeysUri, OutlineJsonSerializerContext.Default.OutlineAccessKeysResponse, cancellationToken);

    public Task<HttpResponseMessage> CreateAccessKeyAsync(CancellationToken cancellationToken = default)
        => _httpClient.PostAsync(_accessKeysUri, new StringContent(string.Empty), cancellationToken);

    public Task<HttpResponseMessage> SetAccessKeysPortAsync(int port, CancellationToken cancellationToken = default)
        => _httpClient.PutAsJsonAsync(_accessKeysPortUri, new OutlineAccessKeysPort(port), OutlineJsonSerializerContext.Default.OutlineAccessKeysPort, cancellationToken);

    public Task<HttpResponseMessage> DeleteAccessKeyAsync(string id, CancellationToken cancellationToken = default)
        => _httpClient.DeleteAsync(new Uri(_accessKeysSlashUri, id), cancellationToken);

    public Task<HttpResponseMessage> SetAccessKeyNameAsync(string id, string name, CancellationToken cancellationToken = default)
        => _httpClient.PutAsJsonAsync(new Uri(_accessKeysSlashUri, $"{id}/name"), new OutlineServerName(name), OutlineJsonSerializerContext.Default.OutlineServerName, cancellationToken);

    public Task<HttpResponseMessage> SetAccessKeyDataLimitAsync(string id, ulong dataLimit, CancellationToken cancellationToken = default)
        => _httpClient.PutAsJsonAsync(new Uri(_accessKeysSlashUri, $"{id}/data-limit"), new OutlineDataLimitRequest(new(dataLimit)), OutlineJsonSerializerContext.Default.OutlineDataLimitRequest, cancellationToken);

    public Task<HttpResponseMessage> DeleteAccessKeyDataLimitAsync(string id, CancellationToken cancellationToken = default)
        => _httpClient.DeleteAsync(new Uri(_accessKeysSlashUri, $"{id}/data-limit"), cancellationToken);

    public Task<OutlineDataUsage?> GetDataUsageAsync(CancellationToken cancellationToken = default)
        => _httpClient.GetFromJsonAsync(_dataUsageUri, OutlineJsonSerializerContext.Default.OutlineDataUsage, cancellationToken);

    public Task<HttpResponseMessage> SetDataLimitAsync(ulong dataLimit, CancellationToken cancellationToken = default)
        => _httpClient.PutAsJsonAsync(_dataLimitUri, new OutlineDataLimitRequest(new(dataLimit)), OutlineJsonSerializerContext.Default.OutlineDataLimitRequest, cancellationToken);

    public Task<HttpResponseMessage> DeleteDataLimitAsync(CancellationToken cancellationToken = default)
        => _httpClient.DeleteAsync(_dataLimitUri, cancellationToken);
}
