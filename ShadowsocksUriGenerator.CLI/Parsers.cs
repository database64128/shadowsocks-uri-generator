using ShadowsocksUriGenerator.Utils;
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
        /// <summary>
        /// Parses the string representation of the port number
        /// into an integer representation.
        /// </summary>
        /// <param name="argumentResult">The argument result.</param>
        /// <returns>The integer representation of the port number.</returns>
        public static int ParsePortNumber(ArgumentResult argumentResult)
        {
            var portString = argumentResult.Tokens.Single().Value;
            if (int.TryParse(portString, out var port))
            {
                if (port is > 0 and <= 65535)
                {
                    return port;
                }
                else
                {
                    argumentResult.AddError("Port out of range: (0, 65535]");
                    return default;
                }
            }
            else
            {
                argumentResult.AddError($"Invalid port number: {portString}");
                return default;
            }
        }

        /// <summary>
        /// Parses the Shadowsocks AEAD method string
        /// into the unified form.
        /// </summary>
        /// <param name="argumentResult">The argument result.</param>
        /// <returns>The Shadowsocks AEAD method string in the unified form.</returns>
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
                case "2022-blake3-aes-128-gcm" or "2022-blake3-aes-256-gcm" or "2022-blake3-chacha8-poly1305" or "2022-blake3-chacha12-poly1305" or "2022-blake3-chacha20-poly1305":
                    return method;
                default:
                    argumentResult.AddError($"Invalid Shadowsocks AEAD or 2022 method: {method}");
                    return string.Empty;
            }
        }

        /// <summary>
        /// Parses the string representation of an amount of data
        /// into the number of bytes it represents.
        /// </summary>
        /// <param name="argumentResult">The argument result.</param>
        /// <returns>The number of bytes. Null if not specified.</returns>
        public static ulong? ParseDataString(ArgumentResult argumentResult)
        {
            var dataString = argumentResult.Tokens.Single().Value;

            // Not specified
            if (string.IsNullOrEmpty(dataString))
            {
                return null;
            }

            if (InteractionHelper.TryParseDataLimitString(dataString, out var dataLimitInBytes))
            {
                return dataLimitInBytes;
            }
            else
            {
                argumentResult.AddError($"Invalid data string representation: {dataString}");
                return default;
            }
        }
    }
}
