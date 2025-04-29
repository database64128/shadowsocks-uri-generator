using ShadowsocksUriGenerator.OnlineConfig;
using System.Text.Json.Serialization;

namespace ShadowsocksUriGenerator.Manager;

[JsonSerializable(typeof(ManagerApiAddRequest))]
[JsonSerializable(typeof(ManagerApiRemoveRequest))]
[JsonSerializable(typeof(Dictionary<int, ulong>))]
[JsonSerializable(typeof(SIP008Config))]
[JsonSourceGenerationOptions(
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
    IgnoreReadOnlyProperties = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
public partial class ManagerApiJsonSerializerContext : JsonSerializerContext
{
}
