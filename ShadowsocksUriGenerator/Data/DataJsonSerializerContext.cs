using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShadowsocksUriGenerator.Data;

[JsonSerializable(typeof(Users))]
[JsonSerializable(typeof(Nodes))]
[JsonSourceGenerationOptions(
    AllowTrailingCommas = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
    IgnoreReadOnlyProperties = true,
    ReadCommentHandling = JsonCommentHandling.Skip,
    WriteIndented = true)]
public partial class DataJsonSerializerContext : JsonSerializerContext
{
}
