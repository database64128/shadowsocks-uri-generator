using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace ShadowsocksUriGenerator.Outline
{
    public class ApiClient : IDisposable
    {
        public ApiClient(ApiKey apiKey)
        {
            _apiKey = apiKey;

            SslClientAuthenticationOptions sslClientAuthenticationOptions = new()
            {
                RemoteCertificateValidationCallback = ValidateServerCertificate,
            };
            SocketsHttpHandler socketsHttpHandler = new()
            {
                SslOptions = sslClientAuthenticationOptions,
            };
            _httpClient = new(socketsHttpHandler);
        }

        private readonly ApiKey _apiKey;
        private readonly HttpClient _httpClient;
        private bool disposedValue;

        private bool ValidateServerCertificate(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
        {
            var sha256Fingerprint = certificate?.GetCertHashString(HashAlgorithmName.SHA256);
            return string.Equals(_apiKey.CertSha256, sha256Fingerprint, StringComparison.OrdinalIgnoreCase);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _httpClient.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public Task<ServerInfo?> GetServerInfoAsync()
            => _httpClient.GetFromJsonAsync<ServerInfo>($"{_apiKey.ApiUrl}/server", Utilities.commonJsonDeserializerOptions);

        public Task<HttpResponseMessage> SetServerNameAsync(string name)
            => _httpClient.PutAsJsonAsync($"{_apiKey.ApiUrl}/name", new ServerName(name), Utilities.commonJsonSerializerOptions);

        public Task<HttpResponseMessage> SetServerHostnameAsync(string hostname)
            => _httpClient.PutAsJsonAsync($"{_apiKey.ApiUrl}/server/hostname-for-access-keys", new ServerHostname(hostname), Utilities.commonJsonSerializerOptions);

        public Task<HttpResponseMessage> SetServerMetricsAsync(bool enabled)
            => _httpClient.PutAsJsonAsync($"{_apiKey.ApiUrl}/metrics/enabled", new Metrics(enabled), Utilities.commonJsonSerializerOptions);

        public Task<AccessKeysResponse?> GetAccessKeysAsync()
            => _httpClient.GetFromJsonAsync<AccessKeysResponse>($"{_apiKey.ApiUrl}/access-keys", Utilities.commonJsonDeserializerOptions);

        public Task<HttpResponseMessage> CreateAccessKeyAsync()
            => _httpClient.PostAsync($"{_apiKey.ApiUrl}/access-keys", new StringContent(string.Empty));

        public Task<HttpResponseMessage> SetAccessKeysPortAsync(int port)
            => _httpClient.PutAsJsonAsync($"{_apiKey.ApiUrl}/server/port-for-new-access-keys", new AccessKeysPort(port), Utilities.commonJsonSerializerOptions);

        public Task<HttpResponseMessage> DeleteAccessKeyAsync(string id)
            => _httpClient.DeleteAsync($"{_apiKey.ApiUrl}/access-keys/{id}");

        public Task<HttpResponseMessage> SetAccessKeyNameAsync(string id, string name)
            => _httpClient.PutAsJsonAsync($"{_apiKey.ApiUrl}/access-keys/{id}/name", new ServerName(name), Utilities.commonJsonSerializerOptions);

        public Task<HttpResponseMessage> SetAccessKeyDataLimitAsync(string id, ulong dataLimit)
            => _httpClient.PutAsJsonAsync($"{_apiKey.ApiUrl}/access-keys/{id}/data-limit", new DataLimit(dataLimit), Utilities.commonJsonSerializerOptions);

        public Task<HttpResponseMessage> DeleteAccessKeyDataLimitAsync(string id)
            => _httpClient.DeleteAsync($"{_apiKey.ApiUrl}/access-keys/{id}/data-limit");

        public Task<DataUsage?> GetDataUsageAsync()
            => _httpClient.GetFromJsonAsync<DataUsage>($"{_apiKey.ApiUrl}/metrics/transfer", Utilities.commonJsonDeserializerOptions);

        public Task<HttpResponseMessage> SetDataLimitAsync(ulong dataLimit)
            => _httpClient.PutAsJsonAsync($"{_apiKey.ApiUrl}/experimental/access-key-data-limit", new DataLimit(dataLimit), Utilities.commonJsonSerializerOptions);

        public Task<HttpResponseMessage> DeleteDataLimitAsync()
            => _httpClient.DeleteAsync($"{_apiKey.ApiUrl}/experimental/access-key-data-limit");
    }
}
