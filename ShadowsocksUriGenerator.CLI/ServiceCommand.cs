using System;
using System.Threading;
using System.Threading.Tasks;

namespace ShadowsocksUriGenerator.CLI
{
    public static class ServiceCommand
    {
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
                    var users = await Users.LoadUsersAsync();
                    using var nodes = await Nodes.LoadNodesAsync();
                    var settings = await Settings.LoadSettingsAsync();

                    if (pullOutlineServer)
                    {
                        try
                        {
                            await nodes.UpdateOutlineServerForAllGroups(users, true, cancellationToken);
                        }
                        catch (OperationCanceledException ex) when (ex is not TaskCanceledException)
                        {
                            Console.WriteLine(ex.Message);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"An error occurred while updating from Outline servers.");
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
                        catch (OperationCanceledException ex) when (ex is not TaskCanceledException)
                        {
                            Console.WriteLine(ex.Message);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"An error occurred while deploying Outline servers.");
                            Console.WriteLine(ex.Message);
                        }
                        Console.WriteLine("Deployed to Outline servers.");
                    }
                    if (generateOnlineConfig)
                    {
                        await OnlineConfig.GenerateAndSave(users, nodes, settings);
                        Console.WriteLine("Generated online config.");
                    }
                    if (regenerateOnlineConfig)
                    {
                        OnlineConfig.Remove(users, settings);
                        Console.WriteLine("Cleaned online config.");
                        await OnlineConfig.GenerateAndSave(users, nodes, settings);
                        Console.WriteLine("Generated online config.");
                    }

                    await Task.Delay(interval * 1000, cancellationToken);
                }
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while executing one of the scheduled tasks.");
                Console.WriteLine(ex.Message);
            }
        }
    }
}
