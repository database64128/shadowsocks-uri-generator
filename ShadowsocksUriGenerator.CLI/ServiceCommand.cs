using ShadowsocksUriGenerator.OnlineConfig;
using System;
using System.CommandLine.Parsing;
using System.Threading;
using System.Threading.Tasks;

namespace ShadowsocksUriGenerator.CLI
{
    public static class ServiceCommand
    {
        public static string? ValidateRun(CommandResult commandResult)
        {
            if (commandResult.Children.Contains("--generate-online-config") &&
                commandResult.Children.Contains("--regenerate-online-config"))
                return "You don't need to generate online config twice.";
            else
                return null;
        }

        public static async Task<int> Run(
            int interval,
            bool pullOutlineServer,
            bool deployOutlineServer,
            bool generateOnlineConfig,
            bool regenerateOnlineConfig,
            CancellationToken cancellationToken = default)
        {
            if (interval < 60 || interval > int.MaxValue / 1000)
            {
                Console.WriteLine($"Interval can't be shorter than 60 seconds or longer than {int.MaxValue / 1000} seconds.");
                return 1;
            }

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

                    if (pullOutlineServer)
                    {
                        var errMsgs = nodes.PullFromOutlineServerForAllGroups(users, true, cancellationToken);

                        await foreach (var errMsg in errMsgs)
                        {
                            Console.WriteLine(errMsg);
                        }

                        users.CalculateDataUsageForAllUsers(nodes);

                        Console.WriteLine("Pulled from Outline servers.");
                    }
                    if (deployOutlineServer)
                    {
                        var errMsgs = nodes.DeployAllOutlineServers(users, cancellationToken);

                        await foreach (var errMsg in errMsgs)
                        {
                            Console.WriteLine(errMsg);
                        }

                        Console.WriteLine("Deployed to Outline servers.");
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

                    await Task.Delay(interval * 1000, cancellationToken);
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
