using System.Text.Json.Serialization;

namespace ShadowsocksUriGenerator.Outline
{
    /// <summary>
    /// The mutable record type that stores information about an Outline server.
    /// It's mutable so it can be atomically updated.
    /// </summary>
    public record OutlineServerInfo
    {
        public string Name { get; set; } = "";
        public string ServerId { get; set; } = Guid.NewGuid().ToString();
        public bool MetricsEnabled { get; set; }
        [JsonConverter(typeof(DateTimeOffsetUnixTimeMillisecondsConverter))]
        public DateTimeOffset CreatedTimestampMs { get; set; } = DateTimeOffset.UtcNow;
        public string Version { get; set; } = "";
        public OutlineDataLimit? AccessKeyDataLimit { get; set; }
        public int PortForNewAccessKeys { get; set; }
        public string HostnameForAccessKeys { get; set; } = "";
    }
}
