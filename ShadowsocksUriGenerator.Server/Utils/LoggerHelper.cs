using System.Net;

namespace ShadowsocksUriGenerator.Server.Utils;

public static partial class LoggerHelper
{
    [LoggerMessage(
        EventId = 0,
        Level = LogLevel.Information,
        Message = "{Username} ({Id}) retrieved online config from {Ip} with query {Query}")]
    public static partial void OnlineConfig(
        ILogger logger,
        string username,
        string id,
        IPAddress? ip,
        IQueryCollection query);
}
