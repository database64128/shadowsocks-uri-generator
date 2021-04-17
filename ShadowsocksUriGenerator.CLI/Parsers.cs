using System.CommandLine.Parsing;
using System.Linq;

namespace ShadowsocksUriGenerator.CLI
{
    /// <summary>
    /// Class for common utility parsers.
    /// Do not put command-specific parsers here.
    /// </summary>
    public static class Parsers
    {
        public static string ParseShadowsocksAEADMethod(ArgumentResult argumentResult)
        {
            var method = argumentResult.Tokens.Single().Value;

            switch (method)
            {
                case "AEAD_AES_128_GCM" or "aes-128-gcm":
                    return "aes-128-gcm";
                case "AEAD_AES_256_GCM" or "aes-256-gcm":
                    return "aes-256-gcm";
                case "AEAD_CHACHA20_POLY1305" or "chacha20-poly1305" or "chacha20-ietf-poly1305":
                    return "chacha20-ietf-poly1305";
                default:
                    argumentResult.ErrorMessage = $"Invalid Shadowsocks AEAD method: {method}";
                    return string.Empty;
            }
        }

        public static ulong? ParseDataString(ArgumentResult argumentResult)
        {
            var dataString = argumentResult.Tokens.Single().Value;

            // Not specified
            if (string.IsNullOrEmpty(dataString))
            {
                return null;
            }

            if (Utilities.TryParseDataLimitString(dataString, out var dataLimitInBytes))
            {
                return dataLimitInBytes;
            }
            else
            {
                argumentResult.ErrorMessage = $"Invalid data string representation: {dataString}";
                return default;
            }
        }
    }
}
