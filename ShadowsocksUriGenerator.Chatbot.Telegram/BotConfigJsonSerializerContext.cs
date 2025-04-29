using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShadowsocksUriGenerator.Chatbot.Telegram;

[JsonSerializable(typeof(BotConfig))]
[JsonSourceGenerationOptions(
    AllowTrailingCommas = true,
    IgnoreReadOnlyProperties = true,
    ReadCommentHandling = JsonCommentHandling.Skip,
    WriteIndented = true)]
public partial class BotConfigJsonSerializerContext : JsonSerializerContext
{
}
