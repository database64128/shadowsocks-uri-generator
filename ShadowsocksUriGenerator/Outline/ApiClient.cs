using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace ShadowsocksUriGenerator.Outline
{
    public class ApiClient : IDisposable
    {
        private readonly ApiKey _apiKey;
        private readonly HttpClient _httpClient;
        private readonly bool _disposeHttpClient;
        private bool _disposedValue;

        /// <summary>
        /// Creates an Outline API client for the specified API key.
        /// </summary>
        /// <param name="apiKey">Outline API key.</param>
        /// <param name="httpClient">
        /// Generic HTTP client to use when the API key does not pin a certificate fingerprint.
        /// This <see cref="HttpClient"/> instance is ignored when the API key pins a certificate fingerprint with <see cref="ApiKey.CertSha256"/>.
        /// Remember to set a timeout on the instance before passing it to this method. A 30s timeout is recommended.
        /// </param>
        public ApiClient(ApiKey apiKey, HttpClient httpClient)
        {
            _apiKey = apiKey;

            if (string.IsNullOrEmpty(_apiKey.CertSha256))
            {
                _httpClient = httpClient;
            }
            else
            {
                SslClientAuthenticationOptions sslClientAuthenticationOptions = new()
                {
                    RemoteCertificateValidationCallback = ValidateServerCertificate,
                };
                SocketsHttpHandler socketsHttpHandler = new()
                {
                    SslOptions = sslClientAuthenticationOptions,
                };
                _httpClient = new(socketsHttpHandler);
                _disposeHttpClient = true;
            }
        }

        /// <inheritdoc cref="HttpClient.Timeout"/>
        public TimeSpan Timeout
        {
            get => _httpClient.Timeout;
            set => _httpClient.Timeout = value;
        }

        private bool ValidateServerCertificate(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
        {
            var sha256Fingerprint = certificate?.GetCertHashString(HashAlgorithmName.SHA256);
            return string.Equals(_apiKey.CertSha256, sha256Fingerprint, StringComparison.OrdinalIgnoreCase);
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

        public Task<ServerInfo?> GetServerInfoAsync(CancellationToken cancellationToken = default)
            => _httpClient.GetFromJsonAsync<ServerInfo>($"{_apiKey.ApiUrl}/server", Utilities.commonJsonDeserializerOptions, cancellationToken);

        public Task<HttpResponseMessage> SetServerNameAsync(string name, CancellationToken cancellationToken = default)
            => _httpClient.PutAsJsonAsync($"{_apiKey.ApiUrl}/name", new ServerName(name), Utilities.commonJsonSerializerOptions, cancellationToken);

        public Task<HttpResponseMessage> SetServerHostnameAsync(string hostname, CancellationToken cancellationToken = default)
            => _httpClient.PutAsJsonAsync($"{_apiKey.ApiUrl}/server/hostname-for-access-keys", new ServerHostname(hostname), Utilities.commonJsonSerializerOptions, cancellationToken);

        public Task<HttpResponseMessage> SetServerMetricsAsync(bool enabled, CancellationToken cancellationToken = default)
            => _httpClient.PutAsJsonAsync($"{_apiKey.ApiUrl}/metrics/enabled", new Metrics(enabled), Utilities.commonJsonSerializerOptions, cancellationToken);

        public Task<AccessKeysResponse?> GetAccessKeysAsync(CancellationToken cancellationToken = default)
            => _httpClient.GetFromJsonAsync<AccessKeysResponse>($"{_apiKey.ApiUrl}/access-keys", Utilities.commonJsonDeserializerOptions, cancellationToken);

        public Task<HttpResponseMessage> CreateAccessKeyAsync(CancellationToken cancellationToken = default)
            => _httpClient.PostAsync($"{_apiKey.ApiUrl}/access-keys", new StringContent(string.Empty), cancellationToken);

        public Task<HttpResponseMessage> SetAccessKeysPortAsync(int port, CancellationToken cancellationToken = default)
            => _httpClient.PutAsJsonAsync($"{_apiKey.ApiUrl}/server/port-for-new-access-keys", new AccessKeysPort(port), Utilities.commonJsonSerializerOptions, cancellationToken);

        public Task<HttpResponseMessage> DeleteAccessKeyAsync(string id, CancellationToken cancellationToken = default)
            => _httpClient.DeleteAsync($"{_apiKey.ApiUrl}/access-keys/{id}", cancellationToken);

        public Task<HttpResponseMessage> SetAccessKeyNameAsync(string id, string name, CancellationToken cancellationToken = default)
            => _httpClient.PutAsJsonAsync($"{_apiKey.ApiUrl}/access-keys/{id}/name", new ServerName(name), Utilities.commonJsonSerializerOptions, cancellationToken);

        public Task<HttpResponseMessage> SetAccessKeyDataLimitAsync(string id, ulong dataLimit, CancellationToken cancellationToken = default)
            => _httpClient.PutAsJsonAsync($"{_apiKey.ApiUrl}/access-keys/{id}/data-limit", new DataLimitContainer(new(dataLimit)), Utilities.commonJsonSerializerOptions, cancellationToken);

        public Task<HttpResponseMessage> DeleteAccessKeyDataLimitAsync(string id, CancellationToken cancellationToken = default)
            => _httpClient.DeleteAsync($"{_apiKey.ApiUrl}/access-keys/{id}/data-limit", cancellationToken);

        public Task<DataUsage?> GetDataUsageAsync(CancellationToken cancellationToken = default)
            => _httpClient.GetFromJsonAsync<DataUsage>($"{_apiKey.ApiUrl}/metrics/transfer", Utilities.commonJsonDeserializerOptions, cancellationToken);

        public Task<HttpResponseMessage> SetDataLimitAsync(ulong dataLimit, CancellationToken cancellationToken = default)
            => _httpClient.PutAsJsonAsync($"{_apiKey.ApiUrl}/server/access-key-data-limit", new DataLimitContainer(new(dataLimit)), Utilities.commonJsonSerializerOptions, cancellationToken);

        public Task<HttpResponseMessage> DeleteDataLimitAsync(CancellationToken cancellationToken = default)
            => _httpClient.DeleteAsync($"{_apiKey.ApiUrl}/server/access-key-data-limit", cancellationToken);
    }
}
