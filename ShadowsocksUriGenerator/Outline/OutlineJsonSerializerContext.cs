using System.Text.Json.Serialization;

namespace ShadowsocksUriGenerator.Outline;

[JsonSerializable(typeof(OutlineApiKey))]
[JsonSerializable(typeof(OutlineServerName))]
[JsonSerializable(typeof(OutlineServerHostname))]
[JsonSerializable(typeof(OutlineDataLimit))]
[JsonSerializable(typeof(OutlineDataLimitRequest))]
[JsonSerializable(typeof(OutlineMetrics))]
[JsonSerializable(typeof(OutlineAccessKeysPort))]
[JsonSerializable(typeof(OutlineAccessKeysResponse))]
[JsonSerializable(typeof(OutlineDataUsage))]
[JsonSerializable(typeof(OutlineAccessKey))]
[JsonSerializable(typeof(OutlineServerInfo))]
[JsonSourceGenerationOptions(
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
    NumberHandling = JsonNumberHandling.AllowReadingFromString,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
public partial class OutlineJsonSerializerContext : JsonSerializerContext
{
}
