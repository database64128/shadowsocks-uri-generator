﻿using ShadowsocksUriGenerator.CLI.Utils;
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

        public static async Task Run(
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
                return;
            }

            try
            {
                while (true)
                {
                    var users = await JsonHelper.LoadUsersAsync(cancellationToken);
                    using var nodes = await JsonHelper.LoadNodesAsync(cancellationToken);
                    var settings = await JsonHelper.LoadSettingsAsync(cancellationToken);

                    if (pullOutlineServer)
                    {
                        try
                        {
                            await nodes.PullFromOutlineServerForAllGroups(users, true, cancellationToken);
                        }
                        catch (OperationCanceledException ex) when (ex.InnerException is not TimeoutException) // canceled
                        {
                            Console.WriteLine(ex.Message);
                            throw;
                        }
                        catch (Exception ex) // timeout and other errors
                        {
                            Console.WriteLine($"An error occurred while pulling from Outline servers.");
                            Console.WriteLine(ex.Message);
                        }
                        Console.WriteLine("Pulled from Outline servers.");
                    }
                    if (deployOutlineServer)
                    {
                        try
                        {
                            await nodes.DeployAllOutlineServers(users, cancellationToken);
                        }
                        catch (OperationCanceledException ex) when (ex.InnerException is not TimeoutException) // canceled
                        {
                            Console.WriteLine(ex.Message);
                            throw;
                        }
                        catch (Exception ex) // timeout and other errors
                        {
                            Console.WriteLine($"An error occurred while deploying Outline servers.");
                            Console.WriteLine(ex.Message);
                        }
                        Console.WriteLine("Deployed to Outline servers.");
                    }
                    if (generateOnlineConfig)
                    {
                        var errMsg = await OnlineConfig.GenerateAndSave(users, nodes, settings, cancellationToken);
                        if (errMsg is not null)
                            Console.Write(errMsg);
                        Console.WriteLine("Generated online config.");
                    }
                    if (regenerateOnlineConfig)
                    {
                        OnlineConfig.Remove(users, settings);
                        Console.WriteLine("Cleaned online config.");
                        var errMsg = await OnlineConfig.GenerateAndSave(users, nodes, settings, cancellationToken);
                        if (errMsg is not null)
                            Console.Write(errMsg);
                        Console.WriteLine("Generated online config.");
                    }

                    await JsonHelper.SaveUsersAsync(users, cancellationToken);
                    await JsonHelper.SaveNodesAsync(nodes, cancellationToken);
                    await Task.Delay(interval * 1000, cancellationToken);
                }
            }
            catch (OperationCanceledException) // Task.Delay() or HttpClient canceled
            {
            }
            catch (Exception ex) // other unhandled
            {
                Console.WriteLine($"An error occurred while executing one of the scheduled tasks.");
                Console.WriteLine(ex.Message);
            }
        }
    }
}
