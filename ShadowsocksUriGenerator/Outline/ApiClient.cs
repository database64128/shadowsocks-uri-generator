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

        private bool ValidateServerCertificate(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
        {
            var sha256Fingerprint = certificate?.GetCertHashString(HashAlgorithmName.SHA256);
            return string.Equals(_apiKey.CertSha256, sha256Fingerprint, StringComparison.OrdinalIgnoreCase);
        }

        public void Dispose()
        {
            ((IDisposable)_httpClient).Dispose();
        }

        public Task<ServerInfo?> GetServerInfoAsync()
            => _httpClient.GetFromJsonAsync<ServerInfo>($"{_apiKey.ApiUrl}/server", Utilities.commonJsonDeserializerOptions);

        public Task<HttpResponseMessage> SetServerNameAsync(string name)
            => _httpClient.PutAsJsonAsync($"{_apiKey.ApiUrl}/name", new Name(name), Utilities.commonJsonSerializerOptions);

        public Task<HttpResponseMessage> SetServerHostnameAsync(string hostname)
            => _httpClient.PutAsJsonAsync($"{_apiKey.ApiUrl}/server/hostname-for-access-keys", new Hostname(hostname), Utilities.commonJsonSerializerOptions);

        public Task<HttpResponseMessage> SetServerMetricsAsync(bool enabled)
            => _httpClient.PutAsJsonAsync($"{_apiKey.ApiUrl}/metrics/enabled", new Metrics(enabled), Utilities.commonJsonSerializerOptions);

        public Task<List<AccessKey>?> GetAccessKeysAsync()
            => _httpClient.GetFromJsonAsync<List<AccessKey>>($"{_apiKey.ApiUrl}/access-keys", Utilities.commonJsonDeserializerOptions);

        public Task<HttpResponseMessage> CreateAccessKeyAsync()
            => _httpClient.PostAsync($"{_apiKey.ApiUrl}/access-keys", new StringContent(string.Empty));

        public Task<HttpResponseMessage> SetAccessKeysPortAsync(int port)
            => _httpClient.PutAsJsonAsync($"{_apiKey.ApiUrl}/server/port-for-new-access-keys", new Port(port), Utilities.commonJsonSerializerOptions);

        public Task<HttpResponseMessage> DeleteAccessKeyAsync(string id)
            => _httpClient.DeleteAsync($"{_apiKey.ApiUrl}/access-keys/{id}");

        public Task<HttpResponseMessage> SetAccessKeyNameAsync(string id, string name)
            => _httpClient.PutAsJsonAsync($"{_apiKey.ApiUrl}/access-keys/{id}/name", new Name(name), Utilities.commonJsonSerializerOptions);

        public Task<HttpResponseMessage> SetAccessKeyDataLimitAsync(string id, ulong dataLimit)
            => _httpClient.PutAsJsonAsync($"{_apiKey.ApiUrl}/access-keys/{id}/data-limit", new DataLimit(dataLimit), Utilities.commonJsonSerializerOptions);

        public Task<HttpResponseMessage> DeleteAccessKeyDataLimitAsync(string id)
            => _httpClient.DeleteAsync($"{_apiKey.ApiUrl}/access-keys/{id}/data-limit");

        public Task<HttpResponseMessage> GetDataUsageAsync()
            => _httpClient.GetAsync($"{_apiKey.ApiUrl}/metrics/transfer");

        public Task<HttpResponseMessage> SetDataLimitAsync(ulong dataLimit)
            => _httpClient.PutAsJsonAsync($"{_apiKey.ApiUrl}/experimental/access-key-data-limit", new DataLimit(dataLimit), Utilities.commonJsonSerializerOptions);

        public Task<HttpResponseMessage> DeleteDataLimitAsync()
            => _httpClient.DeleteAsync($"{_apiKey.ApiUrl}/experimental/access-key-data-limit");
    }
}
