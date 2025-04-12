using ShadowsocksUriGenerator.Data;
using ShadowsocksUriGenerator.OnlineConfig;
using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading;
using System.Threading.Tasks;

namespace ShadowsocksUriGenerator.CLI
{
    public static class ServiceCommand
    {
        public static Action<CommandResult> ValidateRun(
            Option<int> serviceIntervalSecsOption,
            Option<bool> serviceGenerateOnlineConfigOption,
            Option<bool> serviceRegenerateOnlineConfigOption) =>
            commandResult =>
        {
            const int minIntervalSecs = 5;
            const int maxIntervalSecs = int.MaxValue / 1000;
            int intervalSecs = commandResult.GetValue(serviceIntervalSecsOption);
            if (intervalSecs < minIntervalSecs || intervalSecs > maxIntervalSecs)
            {
                commandResult.AddError($"Interval can't be shorter than {minIntervalSecs} seconds or longer than {maxIntervalSecs} seconds.");
            }

            if (commandResult.GetValue(serviceGenerateOnlineConfigOption) &&
                commandResult.GetValue(serviceRegenerateOnlineConfigOption))
            {
                commandResult.AddError("You don't need to generate online config twice.");
            }
        };

        public static async Task<int> Run(
            int intervalSecs,
            bool pullFromServers,
            bool deployToServers,
            bool generateOnlineConfig,
            bool regenerateOnlineConfig,
            CancellationToken cancellationToken = default)
        {
            try
            {
                while (true)
                {
                    var (users, loadUsersErrMsg) = await Users.LoadUsersAsync(cancellationToken);
                    if (loadUsersErrMsg is not null)
                    {
                        Console.WriteLine(loadUsersErrMsg);
                        return 1;
                    }

                    var (loadedNodes, loadNodesErrMsg) = await Nodes.LoadNodesAsync(cancellationToken);
                    if (loadNodesErrMsg is not null)
                    {
                        Console.WriteLine(loadNodesErrMsg);
                        return 1;
                    }
                    using var nodes = loadedNodes;

                    var (settings, loadSettingsErrMsg) = await Settings.LoadSettingsAsync(cancellationToken);
                    if (loadSettingsErrMsg is not null)
                    {
                        Console.WriteLine(loadSettingsErrMsg);
                        return 1;
                    }

                    if (pullFromServers)
                    {
                        try
                        {
                            await nodes.PullGroupsAsync(ReadOnlyMemory<string>.Empty, users, settings, cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }

                        Console.WriteLine("Pulled from servers.");
                    }
                    if (deployToServers)
                    {
                        var errMsgs = nodes.DeployAllOutlineServers(users, cancellationToken);

                        await foreach (var errMsg in errMsgs)
                        {
                            Console.WriteLine(errMsg);
                        }

                        Console.WriteLine("Deployed to servers.");
                    }
                    if (generateOnlineConfig)
                    {
                        var errMsg = await SIP008StaticGen.GenerateAndSave(users, nodes, settings, cancellationToken);
                        if (errMsg is not null)
                            Console.Write(errMsg);

                        Console.WriteLine("Generated online config.");
                    }
                    if (regenerateOnlineConfig)
                    {
                        SIP008StaticGen.Remove(users, settings);

                        Console.WriteLine("Cleaned online config.");

                        var errMsg = await SIP008StaticGen.GenerateAndSave(users, nodes, settings, cancellationToken);
                        if (errMsg is not null)
                            Console.Write(errMsg);

                        Console.WriteLine("Generated online config.");
                    }

                    var saveUsersErrMsg = await Users.SaveUsersAsync(users, cancellationToken);
                    if (saveUsersErrMsg is not null)
                    {
                        Console.WriteLine(saveUsersErrMsg);
                        return 1;
                    }

                    var saveNodesErrMsg = await Nodes.SaveNodesAsync(nodes, cancellationToken);
                    if (saveNodesErrMsg is not null)
                    {
                        Console.WriteLine(saveNodesErrMsg);
                        return 1;
                    }

                    await Task.Delay(intervalSecs * 1000, cancellationToken);
                }
            }
            catch (OperationCanceledException) // Task.Delay() canceled
            {
            }
            catch (Exception ex) // other unhandled
            {
                Console.WriteLine($"An error occurred while executing one of the scheduled tasks: {ex.Message}");
                return -1;
            }

            return 0;
        }
    }
}
