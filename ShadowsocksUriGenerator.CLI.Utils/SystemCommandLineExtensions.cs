using System.CommandLine.Parsing;
using System.Linq;

namespace ShadowsocksUriGenerator.CLI.Utils;

public static class SystemCommandLineExtensions
{
    public static bool ContainsOptionWithName(this CommandResult commandResult, string name) =>
        commandResult.Children.Any(x => x is OptionResult optionResult && optionResult.Option.Name == name);
}
