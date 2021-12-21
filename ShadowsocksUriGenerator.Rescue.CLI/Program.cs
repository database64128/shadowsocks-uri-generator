using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ShadowsocksUriGenerator.Rescue.CLI;

internal class Program
{
    private static Task<int> Main(string[] args)
    {
        var onlineConfigDirOption = new Option<string>("--online-config-dir", "Directory of generated online config.")
        {
            IsRequired = true,
        };
        var outputDirOption = new Option<string>("--output-dir", "Output directory.")
        {
            IsRequired = true,
        };

        var rootCommand = new RootCommand("A rescue tool CLI for restoring ss-uri-gen config from generated online config directory.")
        {
            onlineConfigDirOption,
            outputDirOption,
        };

        rootCommand.SetHandler<string, string, CancellationToken>(HandleRootCommand, onlineConfigDirOption, outputDirOption);

        Console.OutputEncoding = Encoding.UTF8;
        return rootCommand.InvokeAsync(args);
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
