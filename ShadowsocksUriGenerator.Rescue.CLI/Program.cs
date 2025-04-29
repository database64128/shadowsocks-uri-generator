using System.CommandLine;
using System.Text;

namespace ShadowsocksUriGenerator.Rescue.CLI;

internal class Program
{
    private static Task<int> Main(string[] args)
    {
        var onlineConfigDirOption = new Option<string>("--online-config-dir")
        {
            Description = "Directory of generated online config.",
            Required = true,
        };
        var outputDirOption = new Option<string>("--output-dir")
        {
            Description = "Output directory.",
            Required = true,
        };

        var rootCommand = new RootCommand("A rescue tool CLI for restoring ss-uri-gen config from generated online config directory.")
        {
            onlineConfigDirOption,
            outputDirOption,
        };

        rootCommand.SetAction((parseResult, cancellationToken) =>
        {
            var onlineConfigDir = parseResult.GetValue(onlineConfigDirOption)!;
            var outputDir = parseResult.GetValue(onlineConfigDirOption)!;
            return HandleRootCommand(onlineConfigDir, outputDir, cancellationToken);
        });

        Console.OutputEncoding = Encoding.UTF8;
        return rootCommand.Parse(args).InvokeAsync();
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
