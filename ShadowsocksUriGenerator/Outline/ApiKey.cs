namespace ShadowsocksUriGenerator.Outline
{
    /// <summary>
    /// The type for Outline Access Keys Management API key.
    /// </summary>
    public class ApiKey
    {
        /// <summary>
        /// Gets or sets the API URL.
        /// Must not end with a '/'.
        /// </summary>
        public string ApiUrl { get; set; }

        /// <summary>
        /// Gets or sets the TLS certificate's SHA-256 fingerprint.
        /// </summary>
        public string CertSha256 { get; set; }

        public ApiKey()
        {
            ApiUrl = "";
            CertSha256 = "";
        }

        public ApiKey(string apiUrl, string certSha256)
        {
            ApiUrl = apiUrl;
            CertSha256 = certSha256;
        }
    }
}
