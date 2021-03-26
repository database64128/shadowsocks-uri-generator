using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ShadowsocksUriGenerator.CLI
{
    public static class OutlineServerCommand
    {
        public static async Task<int> Add(string group, string apiKey, CancellationToken cancellationToken = default)
        {
            using var nodes = await Nodes.LoadNodesAsync();
            var settings = await Settings.LoadSettingsAsync();

            if (string.IsNullOrEmpty(apiKey))
                Console.WriteLine("You must specify an API key.");

            int result;

            try
            {
                if (settings.OutlineServerApplyDefaultUserOnAssociation)
                    result = await nodes.AssociateOutlineServerWithGroup(group, apiKey, settings.OutlineServerGlobalDefaultUser, cancellationToken);
                else
                    result = await nodes.AssociateOutlineServerWithGroup(group, apiKey, null, cancellationToken);
            }
            catch (OperationCanceledException ex) when (ex is not TaskCanceledException)
            {
                Console.WriteLine(ex.Message);
                result = -4;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                result = -3;
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
                    Console.WriteLine($"An error occurred while applying the global default user setting.");
                    break;
            }

            await Nodes.SaveNodesAsync(nodes);
            return result;
        }

        public static async Task<int> Get(string group)
        {
            using var nodes = await Nodes.LoadNodesAsync();

            var outlineApiKeyString = nodes.GetOutlineApiKeyStringFromGroup(group);
            var outlineServerInfo = nodes.GetOutlineServerInfoFromGroup(group);
            if (outlineApiKeyString != null && outlineServerInfo != null)
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
            string? name,
            string? hostname,
            int? port,
            bool? metrics,
            string? defaultUser,
            CancellationToken cancellationToken = default)
        {
            var commandResult = 0;
            using var nodes = await Nodes.LoadNodesAsync();

            try
            {
                var statusCodes = await nodes.SetOutlineServerInGroup(group, name, hostname, port, metrics, defaultUser, cancellationToken);
                if (statusCodes != null)
                {
                    foreach (var statusCode in statusCodes)
                    {
                        if (statusCode != System.Net.HttpStatusCode.NoContent)
                        {
                            Console.WriteLine($"{statusCode:D} {statusCode:G}");
                            commandResult += (int)statusCode;
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Error: Group not found or no associated Outline server.");
                    commandResult = -2;
                }
            }
            catch (OperationCanceledException ex) when (ex is not TaskCanceledException)
            {
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while applying settings to Outline servers.");
                Console.WriteLine(ex.Message);
                commandResult -= 3;
            }

            await Nodes.SaveNodesAsync(nodes);
            return commandResult;
        }

        public static async Task<int> Remove(string[] groups, bool removeCreds)
        {
            var commandResult = 0;
            var users = await Users.LoadUsersAsync();
            using var nodes = await Nodes.LoadNodesAsync();

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

            await Users.SaveUsersAsync(users);
            await Nodes.SaveNodesAsync(nodes);
            return commandResult;
        }

        public static async Task<int> Pull(string[]? groups, bool noSync, CancellationToken cancellationToken = default)
        {
            var commandResult = 0;
            using var nodes = await Nodes.LoadNodesAsync();
            var users = await Users.LoadUsersAsync();

            try
            {
                if (groups == null)
                    await nodes.UpdateOutlineServerForAllGroups(users, !noSync, cancellationToken);
                else
                    foreach (var group in groups)
                    {
                        var result = await nodes.UpdateGroupOutlineServer(group, users, !noSync, cancellationToken);
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
            catch (OperationCanceledException ex) when (ex is not TaskCanceledException)
            {
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while updating from Outline servers.");
                Console.WriteLine(ex.Message);
                commandResult -= 3;
            }

            await Users.SaveUsersAsync(users);
            await Nodes.SaveNodesAsync(nodes);
            return commandResult;
        }

        public static async Task<int> Deploy(string[]? groups, CancellationToken cancellationToken = default)
        {
            var commandResult = 0;
            var users = await Users.LoadUsersAsync();
            using var nodes = await Nodes.LoadNodesAsync();

            try
            {
                if (groups == null)
                    await nodes.DeployAllOutlineServers(users, cancellationToken);
                else
                {
                    var tasks = groups.Select(async x => await nodes.DeployGroupOutlineServer(x, users, cancellationToken));
                    var results = await Task.WhenAll(tasks);
                    foreach (var result in results)
                    {
                        switch (result)
                        {
                            case 0:
                                Console.WriteLine("Success.");
                                break;
                            case -1:
                                Console.WriteLine("Target group doesn't exist.");
                                break;
                            case -2:
                                Console.WriteLine("No associated Outline server.");
                                break;
                            default:
                                Console.WriteLine($"Unknown error.");
                                break;
                        }
                        commandResult += result;
                    }
                }
            }
            catch (OperationCanceledException ex) when (ex is not TaskCanceledException)
            {
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while deploying Outline servers.");
                Console.WriteLine(ex.Message);
                commandResult -= 3;
            }

            await Users.SaveUsersAsync(users);
            await Nodes.SaveNodesAsync(nodes);
            return commandResult;
        }

        public static async Task<int> RotatePassword(string[]? usernames, string[]? groups, CancellationToken cancellationToken = default)
        {
            var commandResult = 0;
            var users = await Users.LoadUsersAsync();
            using var nodes = await Nodes.LoadNodesAsync();

            try
            {
                if (groups != null)
                {
                    var tasks = groups.Select(async x => await nodes.RotateGroupPassword(x, users, cancellationToken, usernames));
                    await Task.WhenAll(tasks);
                }
                else if (usernames != null)
                {
                    var targetGroups = usernames.Where(x => users.UserDict.ContainsKey(x))
                                                .SelectMany(x => users.UserDict[x].Credentials.Keys)
                                                .Distinct();
                    var tasks = targetGroups.Select(async x => await nodes.RotateGroupPassword(x, users, cancellationToken, usernames));
                    await Task.WhenAll(tasks);
                }
                else
                {
                    Console.WriteLine("Please provide either a username or a group, or both.");
                    commandResult = -3;
                }
            }
            catch (OperationCanceledException ex) when (ex is not TaskCanceledException)
            {
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while connecting to Outline servers.");
                Console.WriteLine(ex.Message);
                commandResult -= 3;
            }

            await Users.SaveUsersAsync(users);
            await Nodes.SaveNodesAsync(nodes);
            return commandResult;
        }
    }
}
