using System.Text.Json.Serialization;

namespace ShadowsocksUriGenerator.OnlineConfig;

[JsonSerializable(typeof(OOCv1ShadowsocksConfig))]
[JsonSerializable(typeof(ShadowsocksGoConfig))]
[JsonSourceGenerationOptions(
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
    IgnoreReadOnlyProperties = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
public partial class OnlineConfigCamelCaseJsonSerializerContext : JsonSerializerContext
{
}

[JsonSerializable(typeof(SingBoxConfig))]
[JsonSerializable(typeof(SIP008Config))]
[JsonSourceGenerationOptions(
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
    IgnoreReadOnlyProperties = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
public partial class OnlineConfigSnakeCaseJsonSerializerContext : JsonSerializerContext
{
}
