using ShadowsocksUriGenerator.CLI.Utils;
using System;
using System.CommandLine.Parsing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ShadowsocksUriGenerator.CLI
{
    public static class OutlineServerCommand
    {
        public static async Task<int> Add(string group, string apiKey, CancellationToken cancellationToken = default)
        {
            using var nodes = await JsonHelper.LoadNodesAsync(cancellationToken);
            var settings = await JsonHelper.LoadSettingsAsync(cancellationToken);

            if (string.IsNullOrEmpty(apiKey))
                Console.WriteLine("You must specify an API key.");

            int result;
            string? errMsg = null;

            try
            {
                (result, errMsg) = await nodes.AssociateOutlineServerWithGroup(
                    group,
                    apiKey,
                    settings.OutlineServerApplyDefaultUserOnAssociation ? settings.OutlineServerGlobalDefaultUser : null,
                    settings.OutlineServerApplyDataLimitOnAssociation,
                    cancellationToken);

                await JsonHelper.SaveNodesAsync(nodes, cancellationToken);
            }
            catch (OperationCanceledException ex) when (ex.InnerException is not TimeoutException) // canceled
            {
                Console.WriteLine(ex.Message);
                return 1;
            }
            catch (Exception ex) // timeout and other errors
            {
                Console.WriteLine($"An error occurred while connecting to the Outline server: {ex.Message}");
                return -3;
            }

            switch (result)
            {
                case 0:
                    Console.WriteLine($"Successfully associated the Outline server with {group}");
                    break;
                case -1:
                    Console.WriteLine($"Group not found: {group}");
                    break;
                case -2:
                    Console.WriteLine($"Invalid API key: {apiKey}");
                    break;
                case -3:
                    Console.WriteLine($"An error occurred while applying the group's per-user data limit: {errMsg}");
                    break;
                default:
                    Console.WriteLine($"Unknown error: {result}");
                    break;
            }

            return result;
        }

        public static async Task<int> Get(string group, CancellationToken cancellationToken = default)
        {
            using var nodes = await JsonHelper.LoadNodesAsync(cancellationToken);

            var outlineApiKeyString = nodes.GetOutlineApiKeyStringFromGroup(group);
            var outlineServerInfo = nodes.GetOutlineServerInfoFromGroup(group);
            if (outlineApiKeyString is not null && outlineServerInfo is not null)
            {
                Console.WriteLine($"{"Name",-32}{outlineServerInfo.Name,-36}");
                Console.WriteLine($"{"ID",-32}{outlineServerInfo.ServerId,-36}");
                Console.WriteLine($"{"API key",-32}{outlineApiKeyString,-36}");
                Console.WriteLine($"{"Date created",-32}{outlineServerInfo.CreatedTimestampMs,-36}");
                Console.WriteLine($"{"Version",-32}{outlineServerInfo.Version,-36}");
                Console.WriteLine($"{"Hostname",-32}{outlineServerInfo.HostnameForAccessKeys,-36}");
                Console.WriteLine($"{"Port for new keys",-32}{outlineServerInfo.PortForNewAccessKeys,-36}");
                Console.WriteLine($"{"Telemetry enabled",-32}{outlineServerInfo.MetricsEnabled,-36}");

                return 0;
            }
            else
            {
                Console.WriteLine("The group is not linked to any Outline server.");
                return 1;
            }
        }

        public static async Task<int> Set(
            string group,
            string name,
            string hostname,
            int? port,
            bool? metrics,
            string defaultUser,
            CancellationToken cancellationToken = default)
        {
            var commandResult = 0;
            using var nodes = await JsonHelper.LoadNodesAsync(cancellationToken);

            try
            {
                var errMsg = await nodes.SetOutlineServerInGroup(group, name, hostname, port, metrics, defaultUser, cancellationToken);
                if (errMsg is null)
                {
                    Console.WriteLine($"Successfully applied new settings to {group}.");
                }
                else
                {
                    Console.WriteLine(errMsg);
                    commandResult = -2;
                }

                await JsonHelper.SaveNodesAsync(nodes, cancellationToken);
            }
            catch (OperationCanceledException ex) when (ex.InnerException is not TimeoutException) // canceled
            {
                Console.WriteLine(ex.Message);
                commandResult = 1;
            }
            catch (Exception ex) // timeout and other errors
            {
                Console.WriteLine($"An error occurred while applying settings to Outline servers: {ex.Message}");
                commandResult = -3;
            }

            return commandResult;
        }

        public static async Task<int> Remove(string[] groups, bool removeCreds, CancellationToken cancellationToken = default)
        {
            var commandResult = 0;
            var users = await JsonHelper.LoadUsersAsync(cancellationToken);
            using var nodes = await JsonHelper.LoadNodesAsync(cancellationToken);

            foreach (var group in groups)
            {
                var result = nodes.RemoveOutlineServerFromGroup(group);
                switch (result)
                {
                    case 0:
                        break;
                    case -1:
                        Console.WriteLine($"Error: Group {group} doesn't exist.");
                        commandResult -= 2;
                        break;
                    default:
                        Console.WriteLine($"Unknown error.");
                        break;
                }
            }

            if (removeCreds)
                users.RemoveCredentialsFromAllUsers(groups);

            await JsonHelper.SaveUsersAsync(users, cancellationToken);
            await JsonHelper.SaveNodesAsync(nodes, cancellationToken);
            return commandResult;
        }

        public static async Task<int> Pull(string[] groups, bool noSync, CancellationToken cancellationToken = default)
        {
            var commandResult = 0;
            using var nodes = await JsonHelper.LoadNodesAsync(cancellationToken);
            var users = await JsonHelper.LoadUsersAsync(cancellationToken);

            try
            {
                if (groups.Length == 0)
                    await nodes.PullFromOutlineServerForAllGroups(users, !noSync, cancellationToken);
                else
                    foreach (var group in groups)
                    {
                        var result = await nodes.PullFromGroupOutlineServer(group, users, !noSync, cancellationToken);
                        switch (result)
                        {
                            case 0:
                                break;
                            case -1:
                                Console.WriteLine($"Error: Group {group} doesn't exist.");
                                break;
                            case -2:
                                Console.WriteLine($"Error: Group {group} is not associated with any Outline server.");
                                break;
                            default:
                                Console.WriteLine($"Unknown error.");
                                break;
                        }
                        commandResult += result;
                    }
            }
            catch (OperationCanceledException ex) when (ex.InnerException is not TimeoutException) // canceled
            {
                Console.WriteLine(ex.Message);
                commandResult = 1;
            }
            catch (Exception ex) // timeout and other errors
            {
                Console.WriteLine($"An error occurred while pulling from Outline servers.");
                Console.WriteLine(ex.Message);
                commandResult -= 3;
            }

            await JsonHelper.SaveUsersAsync(users, default);
            await JsonHelper.SaveNodesAsync(nodes, default);
            return commandResult;
        }

        public static async Task<int> Deploy(string[] groups, CancellationToken cancellationToken = default)
        {
            var commandResult = 0;
            var users = await JsonHelper.LoadUsersAsync(cancellationToken);
            using var nodes = await JsonHelper.LoadNodesAsync(cancellationToken);

            try
            {
                if (groups.Length == 0)
                    await nodes.DeployAllOutlineServers(users, cancellationToken);
                else
                {
                    var tasks = groups.Select(async x => await nodes.DeployGroupOutlineServer(x, users, cancellationToken));
                    var results = await Task.WhenAll(tasks);
                    foreach (var result in results)
                    {
                        switch (result)
                        {
                            case null:
                                Console.WriteLine("Success.");
                                break;
                            default:
                                Console.WriteLine(result);
                                commandResult--;
                                break;
                        }
                    }
                }
            }
            catch (OperationCanceledException ex) when (ex.InnerException is not TimeoutException) // canceled
            {
                Console.WriteLine(ex.Message);
                commandResult = 1;
            }
            catch (Exception ex) // timeout and other errors
            {
                Console.WriteLine($"An error occurred while deploying Outline servers: {ex.Message}");
                commandResult -= 3;
            }

            await JsonHelper.SaveUsersAsync(users, default);
            await JsonHelper.SaveNodesAsync(nodes, default);
            return commandResult;
        }

        public static string? ValidateRotatePassword(CommandResult commandResult)
        {
            if (commandResult.Children.Contains("--usernames") ||
                commandResult.Children.Contains("--groups"))
                return null;
            else
                return "Please provide either a username or a group, or both.";
        }

        public static async Task<int> RotatePassword(string[] usernames, string[] groups, CancellationToken cancellationToken = default)
        {
            var commandResult = 0;
            var users = await JsonHelper.LoadUsersAsync(cancellationToken);
            using var nodes = await JsonHelper.LoadNodesAsync(cancellationToken);

            var errMsgs = Array.Empty<string?>();

            try
            {
                if (groups.Length > 0)
                {
                    var tasks = groups.Select(async x => await nodes.RotateGroupPassword(x, users, cancellationToken, usernames));
                    errMsgs = await Task.WhenAll(tasks);
                }
                else if (usernames.Length > 0)
                {
                    var targetGroups = usernames.Where(x => users.UserDict.ContainsKey(x))
                                                .SelectMany(x => users.UserDict[x].Memberships.Keys)
                                                .Distinct();
                    var tasks = targetGroups.Select(async x => await nodes.RotateGroupPassword(x, users, cancellationToken, usernames));
                    errMsgs = await Task.WhenAll(tasks);
                }
                else
                {
                    Console.WriteLine("Please provide either a username or a group, or both.");
                    commandResult = -3;
                }
            }
            catch (OperationCanceledException ex) when (ex.InnerException is not TimeoutException) // canceled
            {
                Console.WriteLine(ex.Message);
                commandResult = 1;
            }
            catch (Exception ex) // timeout and other errors
            {
                Console.WriteLine($"An error occurred while connecting to Outline servers.");
                Console.WriteLine(ex.Message);
                commandResult -= 3;
            }

            foreach (var errMsg in errMsgs)
            {
                Console.WriteLine(errMsg);
                commandResult--;
            }

            await JsonHelper.SaveUsersAsync(users, default);
            await JsonHelper.SaveNodesAsync(nodes, default);
            return commandResult;
        }
    }
}
