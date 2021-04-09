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
