using ShadowsocksUriGenerator.CLI.Utils;
using ShadowsocksUriGenerator.Data;
using ShadowsocksUriGenerator.Utils;
using System;
using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ShadowsocksUriGenerator.CLI
{
    public static class GroupCommand
    {
        public static async Task<int> Add(string[] groups, string? owner, CancellationToken cancellationToken = default)
        {
            var commandResult = 0;

            var (loadedNodes, loadNodesErrMsg) = await Nodes.LoadNodesAsync(cancellationToken);
            if (loadNodesErrMsg is not null)
            {
                Console.WriteLine(loadNodesErrMsg);
                return 1;
            }
            using var nodes = loadedNodes;

            // Retrieve owner user UUID.
            string? ownerUuid = null;
            if (!string.IsNullOrEmpty(owner))
            {
                var (users, loadUsersErrMsg) = await Users.LoadUsersAsync(cancellationToken);
                if (loadUsersErrMsg is not null)
                {
                    Console.WriteLine(loadUsersErrMsg);
                    return 1;
                }

                if (users.UserDict.TryGetValue(owner, out var targetUser))
                {
                    ownerUuid = targetUser.Uuid;
                }
                else
                {
                    Console.WriteLine($"Warning: The specified owner {owner} is not a user. Skipping.");
                }
            }

            foreach (var group in groups)
            {
                var result = nodes.AddGroup(group, ownerUuid);
                switch (result)
                {
                    case 0:
                        Console.WriteLine($"Added {group}.");
                        break;
                    case 1:
                        Console.WriteLine($"{group} already exists.");
                        break;
                    default:
                        Console.WriteLine($"Unknown error.");
                        break;
                }
                commandResult += result;
            }

            var saveNodesErrMsg = await Nodes.SaveNodesAsync(nodes, cancellationToken);
            if (saveNodesErrMsg is not null)
            {
                Console.WriteLine(saveNodesErrMsg);
                return 1;
            }

            return commandResult;
        }

        public static async Task<int> Edit(string[] groups, string? owner, bool unsetOwner, CancellationToken cancellationToken = default)
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

            // Retrieve owner user UUID.
            string? ownerUuid = null;
            if (!string.IsNullOrEmpty(owner))
            {
                if (users.UserDict.TryGetValue(owner, out var targetUser))
                {
                    ownerUuid = targetUser.Uuid;
                }
                else
                {
                    Console.WriteLine($"Error: The specified owner {owner} is not a user.");
                    return -1;
                }
            }

            var commandResult = 0;

            foreach (var group in groups)
            {
                if (nodes.Groups.TryGetValue(group, out var targetGroup))
                {
                    if (ownerUuid is not null)
                    {
                        targetGroup.OwnerUuid = ownerUuid;
                        Console.WriteLine($"Set user {owner} as owner of group {group}.");
                    }

                    if (unsetOwner)
                    {
                        targetGroup.OwnerUuid = null;
                        Console.WriteLine($"Unset owner of group {group}.");
                    }
                }
                else
                {
                    Console.WriteLine($"Error: Group {group} doesn't exist.");
                    commandResult -= 2;
                }
            }

            var saveNodesErrMsg = await Nodes.SaveNodesAsync(nodes, cancellationToken);
            if (saveNodesErrMsg is not null)
            {
                Console.WriteLine(saveNodesErrMsg);
                return 1;
            }

            return commandResult;
        }

        public static async Task<int> Rename(string oldName, string newName, CancellationToken cancellationToken = default)
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

            var result = nodes.RenameGroup(oldName, newName);
            switch (result)
            {
                case 0:
                    users.UpdateCredentialGroupsForAllUsers(oldName, newName);
                    break;
                case -1:
                    Console.WriteLine($"Error: Group {oldName} doesn't exist.");
                    break;
                case -2:
                    Console.WriteLine($"A group with the new name already exists: {newName}");
                    break;
                default:
                    Console.WriteLine($"Unknown error.");
                    break;
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

            return result;
        }

        public static async Task<int> Remove(string[] groups, CancellationToken cancellationToken = default)
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

            users.RemoveAllUsersFromGroups(groups);

            foreach (var group in groups)
            {
                if (nodes.RemoveGroup(group))
                    Console.WriteLine($"Removed {group}.");
                else
                {
                    Console.WriteLine($"Error: {group} doesn't exist.");
                    commandResult -= 1;
                }
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

        public static async Task<int> List(bool namesOnly, bool onePerLine, CancellationToken cancellationToken = default)
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

            List<(string group, int nodesCount, string owner, string outlineServerName)> groups = [];

            foreach (var groupEntry in nodes.Groups)
            {
                var group = groupEntry.Key;
                var nodesCount = groupEntry.Value.NodeDict.Count;
                var ownerUuid = groupEntry.Value.OwnerUuid;
                var owner = ownerUuid is null
                    ? "N/A"
                    : users.TryGetUserById(ownerUuid, out var userEntry)
                    ? userEntry.Key
                    : "N/A";
                var outlineServerName = groupEntry.Value.OutlineServerInfo?.Name ?? "N/A";

                groups.Add((group, nodesCount, owner, outlineServerName));
            }

            Console.WriteLine($"Groups: {groups.Count}");

            if (groups.Count == 0)
            {
                return 0;
            }

            if (namesOnly)
            {
                var groupNames = groups.Select(x => x.group);
                ConsoleHelper.PrintNameList(groupNames, onePerLine);
                return 0;
            }

            var maxGroupNameLength = groups.Max(x => x.group.Length);
            var maxOwnerNameLength = groups.Max(x => x.owner?.Length ?? 0);
            var maxOutlineServerNameLength = groups.Max(x => x.outlineServerName?.Length ?? 0);

            var groupNameFieldWidth = maxGroupNameLength > 5 ? maxGroupNameLength + 2 : 7;
            var ownerNameFieldWidth = maxOwnerNameLength > 5 ? maxOwnerNameLength + 2 : 7;
            var outlineServerNameFieldWidth = maxOutlineServerNameLength > 14 ? maxOutlineServerNameLength + 2 : 16;

            ConsoleHelper.PrintTableBorder(groupNameFieldWidth, 16, ownerNameFieldWidth, outlineServerNameFieldWidth);
            Console.WriteLine($"|{"Group".PadRight(groupNameFieldWidth)}|{"Number of Nodes",16}|{"Owner".PadLeft(ownerNameFieldWidth)}|{"Outline Server".PadLeft(outlineServerNameFieldWidth)}|");
            ConsoleHelper.PrintTableBorder(groupNameFieldWidth, 16, ownerNameFieldWidth, outlineServerNameFieldWidth);

            foreach (var (group, nodesCount, owner, outlineServerName) in groups)
            {
                Console.WriteLine($"|{group.PadRight(groupNameFieldWidth)}|{nodesCount,16}|{owner.PadLeft(ownerNameFieldWidth)}|{outlineServerName.PadLeft(outlineServerNameFieldWidth)}|");
            }

            ConsoleHelper.PrintTableBorder(groupNameFieldWidth, 16, ownerNameFieldWidth, outlineServerNameFieldWidth);

            return 0;
        }

        public static async Task<int> AddUsers(string group, string[] usernames, bool allUsers, CancellationToken cancellationToken = default)
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

            if (!nodes.Groups.ContainsKey(group))
            {
                Console.WriteLine($"Error: Group {group} doesn't exist.");
                return -2;
            }

            if (allUsers)
            {
                foreach (var userEntry in users.UserDict)
                {
                    var username = userEntry.Key;
                    var user = userEntry.Value;

                    var result = user.AddToGroup(group);
                    if (result)
                        Console.WriteLine($"Successfully added {username} to {group}.");
                    else
                        Console.WriteLine($"User {username} is already in group {group}.");
                }
            }

            foreach (var username in usernames)
            {
                var result = users.AddUserToGroup(username, group);

                switch (result)
                {
                    case 0:
                        Console.WriteLine($"Successfully added {username} to {group}.");
                        break;
                    case 1:
                        Console.WriteLine($"User {username} is already in group {group}.");
                        break;
                    case -1:
                        Console.WriteLine($"Error: user {username} doesn't exist.");
                        commandResult += result;
                        break;
                    default:
                        Console.WriteLine($"Unknown error: {result}.");
                        commandResult += result;
                        break;
                }
            }

            var saveUsersErrMsg = await Users.SaveUsersAsync(users, cancellationToken);
            if (saveUsersErrMsg is not null)
            {
                Console.WriteLine(saveUsersErrMsg);
                return 1;
            }

            return commandResult;
        }

        public static async Task<int> RemoveUsers(string group, string[] usernames, bool allUsers, CancellationToken cancellationToken = default)
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

            if (!nodes.Groups.ContainsKey(group))
            {
                Console.WriteLine($"Error: Group {group} doesn't exist.");
                return -2;
            }

            if (allUsers)
            {
                foreach (var userEntry in users.UserDict)
                    _ = userEntry.Value.RemoveFromGroup(group);
            }

            foreach (var username in usernames)
            {
                var result = users.RemoveUserFromGroup(username, group);
                switch (result)
                {
                    case 1:
                        Console.WriteLine($"User {username} is not in group {group}.");
                        break;
                    case -2:
                        Console.WriteLine($"Error: user {username} doesn't exist.");
                        commandResult += result;
                        break;
                }
            }

            var saveUsersErrMsg = await Users.SaveUsersAsync(users, cancellationToken);
            if (saveUsersErrMsg is not null)
            {
                Console.WriteLine(saveUsersErrMsg);
                return 1;
            }

            return commandResult;
        }

        public static async Task<int> ListUsers(string group, CancellationToken cancellationToken = default)
        {
            var (users, loadUsersErrMsg) = await Users.LoadUsersAsync(cancellationToken);
            if (loadUsersErrMsg is not null)
            {
                Console.WriteLine(loadUsersErrMsg);
                return 1;
            }

            List<(string username, string method, string password)> members = [];

            foreach (var user in users.UserDict)
            {
                if (user.Value.Memberships.TryGetValue(group, out var memberinfo))
                {
                    members.Add((user.Key, memberinfo.Method, memberinfo.Password));
                }
            }

            Console.WriteLine($"Group: {group}");
            Console.WriteLine($"Members: {members.Count}");

            if (members.Count == 0)
            {
                return 0;
            }

            var maxUsernameLength = members.Max(x => x.username.Length);
            var maxMethodLength = members.Max(x => x.method.Length);
            var maxPasswordLength = members.Max(x => x.password.Length);

            var usernameFieldWidth = maxUsernameLength > 4 ? maxUsernameLength + 2 : 6;
            var methodFieldWidth = maxMethodLength > 6 ? maxMethodLength + 2 : 8;
            var passwordFieldWidth = maxPasswordLength > 8 ? maxPasswordLength + 2 : 10;

            ConsoleHelper.PrintTableBorder(usernameFieldWidth, methodFieldWidth, passwordFieldWidth);
            Console.WriteLine($"|{"User".PadRight(usernameFieldWidth)}|{"Method".PadLeft(methodFieldWidth)}|{"Password".PadLeft(passwordFieldWidth)}|");
            ConsoleHelper.PrintTableBorder(usernameFieldWidth, methodFieldWidth, passwordFieldWidth);

            foreach (var (username, method, password) in members)
            {
                Console.WriteLine($"|{username.PadRight(usernameFieldWidth)}|{method.PadLeft(methodFieldWidth)}|{password.PadLeft(passwordFieldWidth)}|");
            }

            ConsoleHelper.PrintTableBorder(usernameFieldWidth, methodFieldWidth, passwordFieldWidth);

            return 0;
        }

        public static async Task<int> AddCredential(
            string group,
            string method,
            string password,
            string[] usernames,
            bool allUsers,
            CancellationToken cancellationToken = default)
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

            if (!nodes.Groups.ContainsKey(group))
            {
                Console.WriteLine($"Error: Group {group} doesn't exist.");
                return -2;
            }

            if (allUsers)
            {
                foreach (var userEntry in users.UserDict)
                {
                    var result = userEntry.Value.AddCredential(group, method, password);
                    switch (result)
                    {
                        case 0:
                            Console.WriteLine($"Added group credential to {userEntry.Key}.");
                            break;
                        case 2:
                            Console.WriteLine("The user already has a credential for the group.");
                            break;
                        case -2:
                            Console.WriteLine("Error: The provided credential is invalid.");
                            commandResult += result;
                            break;
                        default:
                            Console.WriteLine($"Unknown error: {result}.");
                            commandResult += result;
                            break;
                    }
                }
            }

            foreach (var username in usernames)
            {
                var result = users.AddCredentialToUser(username, group, method, password);
                switch (result)
                {
                    case 0:
                        Console.WriteLine($"Added group credential to {username}.");
                        break;
                    case 2:
                        Console.WriteLine("The user already has a credential for the group.");
                        break;
                    case -1:
                        Console.WriteLine($"Error: user {username} doesn't exist.");
                        commandResult += result;
                        break;
                    case -2:
                        Console.WriteLine("Error: The provided credential is invalid.");
                        commandResult += result;
                        break;
                    default:
                        Console.WriteLine($"Unknown error: {result}.");
                        commandResult += result;
                        break;
                }
            }

            var saveUsersErrMsg = await Users.SaveUsersAsync(users, cancellationToken);
            if (saveUsersErrMsg is not null)
            {
                Console.WriteLine(saveUsersErrMsg);
                return 1;
            }

            return commandResult;
        }

        public static async Task<int> RemoveCredentials(string group, string[] usernames, bool allUsers, CancellationToken cancellationToken = default)
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

            if (!nodes.Groups.ContainsKey(group))
            {
                Console.WriteLine($"Error: Group {group} doesn't exist.");
                return -2;
            }

            if (allUsers)
            {
                users.RemoveCredentialsFromAllUsers(group);
            }

            foreach (var username in usernames)
            {
                var result = users.RemoveCredentialFromUser(username, group);
                switch (result)
                {
                    case 0:
                        Console.WriteLine($"Removed credential of {group} from {username}.");
                        break;
                    case 1:
                        Console.WriteLine($"Warning: user {username} is in group {group} but has no associated credential.");
                        break;
                    case -1:
                        Console.WriteLine($"Error: User {username} is not in group {group}.");
                        commandResult += result;
                        break;
                    case -2:
                        Console.WriteLine($"Error: user {username} doesn't exist.");
                        commandResult += result;
                        break;
                }
            }

            var saveUsersErrMsg = await Users.SaveUsersAsync(users, cancellationToken);
            if (saveUsersErrMsg is not null)
            {
                Console.WriteLine(saveUsersErrMsg);
                return 1;
            }

            return commandResult;
        }

        public static async Task<int> GetDataUsage(string group, SortBy? sortBy, CancellationToken cancellationToken = default)
        {
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

            var records = nodes.GetGroupDataUsage(group);

            if (records is null)
            {
                Console.WriteLine($"Error: group {group} doesn't exist.");
                return -2;
            }

            var maxNameLength = records.Select(x => x.username.Length)
                                       .DefaultIfEmpty()
                                       .Max();
            var nameFieldWidth = maxNameLength > 4 ? maxNameLength + 2 : 6;

            var sortByInEffect = settings.GroupDataUsageDefaultSortBy;
            if (sortBy is SortBy currentRunSortBy)
                sortByInEffect = currentRunSortBy;
            switch (sortByInEffect)
            {
                case SortBy.DefaultAscending:
                    break;
                case SortBy.DefaultDescending:
                    records.Reverse();
                    break;
                case SortBy.NameAscending:
                    records = [.. records.OrderBy(x => x.username)];
                    break;
                case SortBy.NameDescending:
                    records = [.. records.OrderByDescending(x => x.username)];
                    break;
                case SortBy.DataUsedAscending:
                    records = [.. records.OrderBy(x => x.bytesUsed)];
                    break;
                case SortBy.DataUsedDescending:
                    records = [.. records.OrderByDescending(x => x.bytesUsed)];
                    break;
                case SortBy.DataRemainingAscending:
                    records = [.. records.OrderBy(x => x.bytesRemaining)];
                    break;
                case SortBy.DataRemainingDescending:
                    records = [.. records.OrderByDescending(x => x.bytesRemaining)];
                    break;
            }

            Console.WriteLine($"{"Group",-16}{group,-32}");

            if (nodes.Groups.TryGetValue(group, out var targetGroup))
            {
                Console.WriteLine($"{"Data used",-16}{InteractionHelper.HumanReadableDataString1024(targetGroup.BytesUsed),-32}");
                if (targetGroup.BytesRemaining != 0UL)
                    Console.WriteLine($"{"Data remaining",-16}{InteractionHelper.HumanReadableDataString1024(targetGroup.BytesRemaining),-32}");
                if (targetGroup.DataLimitInBytes != 0UL)
                    Console.WriteLine($"{"Data limit",-16}{InteractionHelper.HumanReadableDataString1024(targetGroup.DataLimitInBytes),-32}");
            }

            if (records.All(x => x.bytesRemaining == 0UL)) // Omit data remaining column if no data.
            {
                ConsoleHelper.PrintTableBorder(nameFieldWidth, 11);
                Console.WriteLine($"|{"User".PadRight(nameFieldWidth)}|{"Data Used",11}|");
                ConsoleHelper.PrintTableBorder(nameFieldWidth, 11);

                foreach (var (username, bytesUsed, _) in records)
                {
                    Console.WriteLine($"|{username.PadRight(nameFieldWidth)}|{InteractionHelper.HumanReadableDataString1024(bytesUsed),11}|");
                }

                ConsoleHelper.PrintTableBorder(nameFieldWidth, 11);
            }
            else
            {
                ConsoleHelper.PrintTableBorder(nameFieldWidth, 11, 16);
                Console.WriteLine($"|{"User".PadRight(nameFieldWidth)}|{"Data Used",11}|{"Data Remaining",16}|");
                ConsoleHelper.PrintTableBorder(nameFieldWidth, 11, 16);

                foreach (var (username, bytesUsed, bytesRemaining) in records)
                {
                    Console.Write($"|{username.PadRight(nameFieldWidth)}|{InteractionHelper.HumanReadableDataString1024(bytesUsed),11}|");

                    if (bytesRemaining != 0UL)
                        Console.WriteLine($"{InteractionHelper.HumanReadableDataString1024(bytesRemaining),16}|");
                    else
                        Console.WriteLine($"{string.Empty,16}|");
                }

                ConsoleHelper.PrintTableBorder(nameFieldWidth, 11, 16);
            }

            return 0;
        }

        public static async Task<int> GetDataLimit(string group, CancellationToken cancellationToken = default)
        {
            var (loadedNodes, loadNodesErrMsg) = await Nodes.LoadNodesAsync(cancellationToken);
            if (loadNodesErrMsg is not null)
            {
                Console.WriteLine(loadNodesErrMsg);
                return 1;
            }
            using var nodes = loadedNodes;

            if (nodes.Groups.TryGetValue(group, out var targetGroup))
            {
                Console.WriteLine($"{"Group",-24}{group,-32}");
                if (targetGroup.DataLimitInBytes != 0UL)
                    Console.WriteLine($"{"Global data limit",-24}{InteractionHelper.HumanReadableDataString1024(targetGroup.DataLimitInBytes),-32}");
                if (targetGroup.PerUserDataLimitInBytes != 0UL)
                    Console.WriteLine($"{"Per-user data limit",-24}{InteractionHelper.HumanReadableDataString1024(targetGroup.PerUserDataLimitInBytes),-32}");

                var outlineAccessKeyCustomLimits = targetGroup.OutlineAccessKeys?.Where(x => x.DataLimit is not null).Select(x => (x.Name, x.DataLimit!.Bytes));

                if (outlineAccessKeyCustomLimits is null || !outlineAccessKeyCustomLimits.Any())
                {
                    return 0;
                }

                var maxNameLength = outlineAccessKeyCustomLimits.Select(x => x.Name.Length)
                                                                .DefaultIfEmpty()
                                                                .Max();
                var nameFieldWidth = maxNameLength > 4 ? maxNameLength + 2 : 6;

                ConsoleHelper.PrintTableBorder(nameFieldWidth, 19);
                Console.WriteLine($"|{"User".PadRight(nameFieldWidth)}|{"Custom Data Limit",19}|");
                ConsoleHelper.PrintTableBorder(nameFieldWidth, 19);

                foreach ((var username, var dataLimitInBytes) in outlineAccessKeyCustomLimits)
                {
                    Console.WriteLine($"|{username.PadRight(nameFieldWidth)}|{InteractionHelper.HumanReadableDataString1024(dataLimitInBytes),19}|");
                }

                ConsoleHelper.PrintTableBorder(nameFieldWidth, 19);

                return 0;
            }
            else
            {
                Console.WriteLine($"Error: group {group} doesn't exist.");
                return -2;
            }
        }

        public static void ValidateSetDataLimit(CommandResult commandResult)
        {
            var hasGlobal = commandResult.ContainsOptionWithName("--global");
            var hasPerUser = commandResult.ContainsOptionWithName("--per-user");
            var hasUsernames = commandResult.ContainsOptionWithName("--usernames");

            if (!hasGlobal && !hasPerUser)
                commandResult.AddError("Please specify either a global data limit with `--global`, or a per-user data limit with `--per-user`.");

            if (!hasPerUser && hasUsernames)
                commandResult.AddError("Custom user targets must be used with per-user limits.");
        }

        public static async Task<int> SetDataLimit(
            string[] groups,
            ulong? global,
            ulong? perUser,
            string[] usernames,
            CancellationToken cancellationToken = default)
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

            if (global is ulong globalDataLimit)
            {
                foreach (var group in groups)
                {
                    var result = nodes.SetGroupGlobalDataLimit(group, globalDataLimit);
                    switch (result)
                    {
                        case 0:
                            Console.WriteLine($"Global data limit set on {group}.");
                            break;
                        case -2:
                            Console.WriteLine($"Error: Group {group} doesn't exist.");
                            break;
                        default:
                            Console.WriteLine($"Unknown error.");
                            break;
                    }
                    commandResult += result;
                }
            }

            if (perUser is ulong perUserDataLimit)
            {
                foreach (var group in groups)
                {
                    if (usernames.Length == 0)
                    {
                        var result = nodes.SetGroupPerUserDataLimit(group, perUserDataLimit);
                        switch (result)
                        {
                            case 0:
                                Console.WriteLine($"Per-user data limit set on {group}.");
                                break;
                            case -2:
                                Console.WriteLine($"Error: Group {group} doesn't exist.");
                                break;
                            default:
                                Console.WriteLine($"Unknown error.");
                                break;
                        }
                        commandResult += result;
                    }
                    else
                    {
                        foreach (var username in usernames)
                        {
                            var result = users.SetUserDataLimitInGroup(username, group, perUserDataLimit);
                            switch (result)
                            {
                                case 0:
                                    Console.WriteLine($"Custom data limit set on {username} in {group}.");
                                    break;
                                case -1:
                                    Console.WriteLine($"Error: user {username} doesn't exist.");
                                    break;
                                case -2:
                                    Console.WriteLine($"Error: user {username} is not in group {group}.");
                                    break;
                                default:
                                    Console.WriteLine($"Unknown error.");
                                    break;
                            }
                            commandResult += result;
                        }
                    }
                }
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
