using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Linq;

namespace ShadowsocksUriGenerator.CLI.Utils;

public static class SystemCommandLineExtensions
{
    public static bool ContainsAlias(this IReadOnlyList<SymbolResult> symbolResults, string alias) =>
        symbolResults.Any(x => x.Symbol is IdentifierSymbol y && y.HasAlias(alias));
}
