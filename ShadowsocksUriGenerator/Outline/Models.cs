using System.Collections.Generic;

namespace ShadowsocksUriGenerator.Outline
{
    public record ServerName(string Name);
    public record ServerHostname(string Hostname);
    public record DataLimit(ulong Bytes);
    public record Metrics(bool MetricsEnabled);
    public record AccessKeysPort(int Port);
    public record DataUsage(Dictionary<int, ulong> BytesTransferredByUserId);
}
