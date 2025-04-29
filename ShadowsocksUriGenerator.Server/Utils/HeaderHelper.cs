using Microsoft.Extensions.Primitives;
using System.Net;

namespace ShadowsocksUriGenerator.Server.Utils
{
    public static class HeaderHelper
    {
        public static IPAddress? GetRealIP(HttpContext ctx)
        {
            var xRealIpHeader = ctx.Request.Headers["X-Real-IP"];

            if (!StringValues.IsNullOrEmpty(xRealIpHeader) && IPAddress.TryParse(xRealIpHeader, out var ip))
                return ip;
            else
                return ctx.Connection.RemoteIpAddress;
        }
    }
}
