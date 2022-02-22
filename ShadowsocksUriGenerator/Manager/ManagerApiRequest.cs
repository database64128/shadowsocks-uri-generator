using System.Buffers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShadowsocksUriGenerator.Manager;

public class ManagerApiAddRequest
{
    [JsonPropertyName("server_port")]
    public int Port { get; set; }

    public string Password { get; set; } = "";

    public string? Method { get; set; }

    [JsonPropertyName("plugin")]
    public string? PluginName { get; set; }

    [JsonPropertyName("plugin_version")]
    public string? PluginVersion { get; set; }

    [JsonPropertyName("plugin_opts")]
    public string? PluginOptions { get; set; }

    [JsonPropertyName("plugin_args")]
    public string? PluginArguments { get; set; }

    public byte[] ToBytes()
    {
        var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(this, ManagerAPIJsonSerializerContext.Default.ManagerApiAddRequest);
        var requestBytes = ArrayPool<byte>.Shared.Rent(5 + jsonBytes.Length);
        _ = Encoding.UTF8.GetBytes("add: ", requestBytes);
        jsonBytes.CopyTo(requestBytes, jsonBytes.Length);
        return requestBytes;
    }
}

public class ManagerApiRemoveRequest
{
    [JsonPropertyName("server_port")]
    public int Port { get; set; }

    public byte[] ToBytes()
    {
        var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(this, ManagerAPIJsonSerializerContext.Default.ManagerApiRemoveRequest);
        var requestBytes = ArrayPool<byte>.Shared.Rent(8 + jsonBytes.Length);
        _ = Encoding.UTF8.GetBytes("remove: ", requestBytes);
        jsonBytes.CopyTo(requestBytes, jsonBytes.Length);
        return requestBytes;
    }
}
