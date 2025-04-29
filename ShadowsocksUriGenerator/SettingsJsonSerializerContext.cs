using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShadowsocksUriGenerator;

[JsonSerializable(typeof(Settings))]
[JsonSourceGenerationOptions(
    AllowTrailingCommas = true,
    IgnoreReadOnlyProperties = true,
    ReadCommentHandling = JsonCommentHandling.Skip,
    WriteIndented = true)]
public partial class SettingsJsonSerializerContext : JsonSerializerContext
{
}
