using System;
using System.Text.Json.Serialization;

namespace ShadowsocksUriGenerator.Outline
{
    public class ServerInfo
    {
        public string Name { get; set; }
        public string ServerId { get; set; }
        public bool MetricsEnabled { get; set; }
        [JsonConverter(typeof(DateTimeOffsetUnixTimeMillisecondsConverter))]
        public DateTimeOffset CreatedTimestampMs { get; set; }
        public string Version { get; set; }
        public int PortForNewAccessKeys { get; set; }
        public string HostnameForAccessKeys { get; set; }

        public ServerInfo()
        {
            Name = "";
            ServerId = new Guid().ToString();
            MetricsEnabled = false;
            CreatedTimestampMs = DateTimeOffset.UtcNow;
            Version = "";
            PortForNewAccessKeys = 0;
            HostnameForAccessKeys = "";
        }
    }
}
