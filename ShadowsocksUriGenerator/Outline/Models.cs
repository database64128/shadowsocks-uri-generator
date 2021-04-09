using System.Collections.Generic;

namespace ShadowsocksUriGenerator.Outline
{
    public record ServerName(string Name);
    public record ServerHostname(string Hostname);
    public record DataLimit(ulong Bytes);
    public record DataLimitContainer(DataLimit Limit);
    public record Metrics(bool MetricsEnabled);
    public record AccessKeysPort(int Port);
    public record AccessKeysResponse(List<AccessKey> AccessKeys);
    public record DataUsage(Dictionary<int, ulong> BytesTransferredByUserId);
}
