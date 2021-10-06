using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ShadowsocksUriGenerator.Rescue.CLI
{
    internal class Program
    {
        private static Task<int> Main(string[] args)
        {
            var rootCommand = new RootCommand("A rescue tool CLI for restoring ss-uri-gen config from generated online config directory.")
            {
                new Option<string>("--online-config-dir", "Directory of generated online config."),
                new Option<string>("--output-dir", "Output directory."),
            };
            rootCommand.AddValidator(ValidateRootCommand);
            rootCommand.Handler = CommandHandler.Create<string, string, CancellationToken>(HandleRootCommand);

            Console.OutputEncoding = Encoding.UTF8;
            return rootCommand.InvokeAsync(args);
        }

        private static string? ValidateRootCommand(CommandResult commandResult)
        {
            var hasOnlineConfigDir = commandResult.Children.ContainsAlias("--online-config-dir");

            if (!hasOnlineConfigDir)
                return "Specify path to online config directory with `--online-config-dir`.";
            else
                return null;
        }

        private static async Task<int> HandleRootCommand(string onlineConfigDir, string outputDir, CancellationToken cancellationToken = default)
        {
            var (users, nodes, errMsgGetFromOC) = await Rescuers.FromOnlineConfig(onlineConfigDir, cancellationToken);
            if (errMsgGetFromOC is not null || users is null || nodes is null)
            {
                Console.WriteLine(errMsgGetFromOC);
                return -1;
            }

            var errMsgSaveJson = await Restorers.ToJsonFiles(outputDir, users, nodes, cancellationToken);
            if (errMsgSaveJson is not null)
            {
                Console.WriteLine(errMsgSaveJson);
                return -2;
            }

            return 0;
        }
    }
}
