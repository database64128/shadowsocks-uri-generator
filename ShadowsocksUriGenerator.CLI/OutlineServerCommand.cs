using ShadowsocksUriGenerator.CLI.Utils;
using System;
using System.Collections.Generic;
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

            var errMsg = await nodes.AssociateOutlineServerWithGroup(
                group,
                apiKey,
                users,
                settings.OutlineServerApplyDefaultUserOnAssociation ? settings.OutlineServerGlobalDefaultUser : null,
                settings.OutlineServerApplyDataLimitOnAssociation,
                cancellationToken);

            if (errMsg is not null)
            {
                Console.WriteLine(errMsg);
                return -1;
            }

            Console.WriteLine($"Successfully associated the Outline server with {group}");

            users.CalculateDataUsageForAllUsers(nodes);

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

            return 0;
        }

        public static async Task<int> Get(string group, CancellationToken cancellationToken = default)
        {
            var (loadedNodes, loadNodesErrMsg) = await Nodes.LoadNodesAsync(cancellationToken);
            if (loadNodesErrMsg is not null)
            {
                Console.WriteLine(loadNodesErrMsg);
                return 1;
            }
            using var nodes = loadedNodes;

            var outlineApiKeyString = nodes.GetOutlineApiKeyStringFromGroup(group);
            var outlineServerInfo = nodes.GetOutlineServerInfoFromGroup(group);
            var outlineDefaultUser = nodes.GetOutlineDefaultUserFromGroup(group);

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

                if (outlineDefaultUser is not null)
                    Console.WriteLine($"{"Admin key username",-32}{outlineDefaultUser,-36}");

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

            var (loadedNodes, loadNodesErrMsg) = await Nodes.LoadNodesAsync(cancellationToken);
            if (loadNodesErrMsg is not null)
            {
                Console.WriteLine(loadNodesErrMsg);
                return 1;
            }
            using var nodes = loadedNodes;

            var errMsg = await nodes.SetOutlineServerInGroup(group, name, hostname, port, metrics, defaultUser, cancellationToken);
            if (errMsg is not null)
            {
                Console.WriteLine(errMsg);
                commandResult = -2;
            }
            else
            {
                Console.WriteLine($"Successfully applied new settings to {group}.");
            }

            var saveNodesErrMsg = await Nodes.SaveNodesAsync(nodes, cancellationToken);
            if (saveNodesErrMsg is not null)
            {
                Console.WriteLine(saveNodesErrMsg);
                return 1;
            }

            return commandResult;
        }

        public static async Task<int> Remove(string[] groups, bool removeCreds, CancellationToken cancellationToken = default)
        {
            var commandResult = 0;

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

            users.CalculateDataUsageForAllUsers(nodes);

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

            return commandResult;
        }

        public static async Task<int> Pull(string[] groups, bool noSync, CancellationToken cancellationToken = default)
        {
            var commandResult = 0;

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

            IAsyncEnumerable<string> errMsgs;

            if (groups.Length == 0)
            {
                errMsgs = nodes.PullFromOutlineServerForAllGroups(users, !noSync, cancellationToken);
            }
            else
            {
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
                errMsgs = groups.Select(group => nodes.PullFromGroupOutlineServer(group, users, !noSync, cancellationToken).ToAsyncEnumerable())
                                .ConcurrentMerge()
                                .Where(errMsg => errMsg is not null);
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
            }

            await foreach (var errMsg in errMsgs)
            {
                Console.WriteLine(errMsg);
                commandResult--;
            }

            users.CalculateDataUsageForAllUsers(nodes);

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

            return commandResult;
        }

        public static async Task<int> Deploy(string[] groups, CancellationToken cancellationToken = default)
        {
            var commandResult = 0;

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

            IAsyncEnumerable<string> errMsgs;

            if (groups.Length == 0)
            {
                errMsgs = nodes.DeployAllOutlineServers(users, cancellationToken);
            }
            else
            {
                errMsgs = groups.Select(group => nodes.DeployGroupOutlineServer(group, users, cancellationToken)).ConcurrentMerge();
            }

            await foreach (var errMsg in errMsgs)
            {
                Console.WriteLine(errMsg);
                commandResult--;
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

            return commandResult;
        }

        public static void ValidateRotatePassword(CommandResult commandResult)
        {
            var hasUsernames = commandResult.ContainsSymbolWithName("usernames");
            var hasGroups = commandResult.ContainsSymbolWithName("groups");
            var hasAll = commandResult.ContainsSymbolWithName("all");

            if (hasAll && (hasUsernames || hasGroups))
                commandResult.ErrorMessage = "You are already targeting all groups and users with '--all'. Drop '--all' if you want to target specific users or groups.";

            if (!hasUsernames && !hasGroups && !hasAll)
                commandResult.ErrorMessage = "Target specific users with '--usernames', groups with '--groups'. You can also combine both, or target all groups and users with '--all'.";
        }

        public static async Task<int> RotatePassword(string[] usernames, string[] groups, bool allGroups, CancellationToken cancellationToken = default)
        {
            var commandResult = 0;

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

            IAsyncEnumerable<string> errMsgs;

            if (allGroups)
            {
                errMsgs = nodes.RotatePasswordForAllGroups(users, cancellationToken);
            }
            else if (groups.Length > 0)
            {
                // Rotate for specified or all users in these groups.
                errMsgs = groups.Select(x => nodes.RotateGroupPassword(x, users, cancellationToken, usernames)).ConcurrentMerge();
            }
            else if (usernames.Length > 0)
            {
                // Find the groups these users are in and rotate for them in these groups.
                var targetGroups = usernames.Where(x => users.UserDict.ContainsKey(x))
                                            .SelectMany(x => users.UserDict[x].Memberships.Keys)
                                            .Distinct();
                errMsgs = targetGroups.Select(x => nodes.RotateGroupPassword(x, users, cancellationToken, usernames)).ConcurrentMerge();
            }
            else
            {
                Console.WriteLine("Please provide either a username or a group, or both.");
                return -3;
            }

            await foreach (var errMsg in errMsgs)
            {
                Console.WriteLine(errMsg);
                commandResult--;
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

            return commandResult;
        }
    }
}
