using ShadowsocksUriGenerator.CLI.Utils;
using System;
using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ShadowsocksUriGenerator.CLI
{
    public static class UserCommand
    {
        public static async Task<int> Add(string[] usernames, CancellationToken cancellationToken = default)
        {
            var commandResult = 0;

            var (users, loadUsersErrMsg) = await Users.LoadUsersAsync(cancellationToken);
            if (loadUsersErrMsg is not null)
            {
                Console.WriteLine(loadUsersErrMsg);
                return 1;
            }

            foreach (var username in usernames)
            {
                var result = users.AddUser(username);
                commandResult += result;
                if (result == 0)
                    Console.WriteLine($"Added {username}.");
                else
                    Console.WriteLine($"{username} already exists.");
            }

            var saveUsersErrMsg = await Users.SaveUsersAsync(users, cancellationToken);
            if (saveUsersErrMsg is not null)
            {
                Console.WriteLine(saveUsersErrMsg);
                return 1;
            }

            return commandResult;
        }

        public static async Task<int> Rename(string oldName, string newName, CancellationToken cancellationToken = default)
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

            var result = await users.RenameUser(oldName, newName, nodes, cancellationToken);
            if (result is null)
            {
                Console.WriteLine($"Successfully renamed {oldName} to {newName}.");
            }
            else
            {
                Console.WriteLine(result);
                commandResult--;
            }

            var saveUsersErrMsg = await Users.SaveUsersAsync(users, cancellationToken);
            if (saveUsersErrMsg is not null)
            {
                Console.WriteLine(saveUsersErrMsg);
                return 1;
            }

            return commandResult;
        }

        public static async Task<int> Remove(string[] usernames, CancellationToken cancellationToken = default)
        {
            var commandResult = 0;

            var (users, loadUsersErrMsg) = await Users.LoadUsersAsync(cancellationToken);
            if (loadUsersErrMsg is not null)
            {
                Console.WriteLine(loadUsersErrMsg);
                return 1;
            }

            var (settings, loadSettingsErrMsg) = await Settings.LoadSettingsAsync(cancellationToken);
            if (loadSettingsErrMsg is not null)
            {
                Console.WriteLine(loadSettingsErrMsg);
                return 1;
            }

            // Removing online config requires reading user entry.
            if (settings.OnlineConfigCleanOnUserRemoval)
                OnlineConfig.Remove(users, settings, usernames);
            // Remove user entry.
            foreach (var username in usernames)
            {
                var result = users.RemoveUser(username);
                if (result)
                    Console.WriteLine($"Removed {username}.");
                else
                {
                    commandResult--;
                    Console.WriteLine($"Error: {username} doesn't exist.");
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

        public static async Task<int> List(bool namesOnly, bool onePerLine, CancellationToken cancellationToken = default)
        {
            var (users, loadUsersErrMsg) = await Users.LoadUsersAsync(cancellationToken);
            if (loadUsersErrMsg is not null)
            {
                Console.WriteLine(loadUsersErrMsg);
                return 1;
            }

            Console.WriteLine($"Users: {users.UserDict.Count}");

            if (users.UserDict.Count == 0)
            {
                return 0;
            }

            if (namesOnly)
            {
                ConsoleHelper.PrintNameList(users.UserDict.Keys, onePerLine);
                return 0;
            }

            var maxNameLength = users.UserDict.Max(x => x.Key.Length);
            var nameFieldWidth = maxNameLength > 4 ? maxNameLength + 2 : 6;

            ConsoleHelper.PrintTableBorder(nameFieldWidth, 36, 18);
            Console.WriteLine($"|{"User".PadRight(nameFieldWidth)}|{"UUID",36}|{"Associated Groups",18}|");
            ConsoleHelper.PrintTableBorder(nameFieldWidth, 36, 18);

            foreach (var user in users.UserDict)
            {
                Console.WriteLine($"|{user.Key.PadRight(nameFieldWidth)}|{user.Value.Uuid,36}|{user.Value.Memberships.Count,18}|");
            }

            ConsoleHelper.PrintTableBorder(nameFieldWidth, 36, 18);

            return 0;
        }

        public static async Task<int> JoinGroups(string username, string[] groups, bool allGroups, CancellationToken cancellationToken = default)
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

            if (allGroups)
                groups = nodes.Groups.Keys.ToArray();

            foreach (var group in groups)
            {
                if (!allGroups && !nodes.Groups.ContainsKey(group)) // check group existence when group is specified by user.
                {
                    Console.WriteLine($"Group not found: {group}");
                    commandResult -= 2;
                    continue;
                }

                var result = users.AddUserToGroup(username, group);

                switch (result)
                {
                    case 0:
                        Console.WriteLine($"Successfully added {username} to {group}.");
                        break;
                    case 1:
                        Console.WriteLine($"The user is already in group {group}.");
                        break;
                    case -1:
                        Console.WriteLine("User not found.");
                        commandResult--;
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

        public static async Task<int> LeaveGroups(string username, string[] groups, bool allGroups, CancellationToken cancellationToken = default)
        {
            var commandResult = 0;

            var (users, loadUsersErrMsg) = await Users.LoadUsersAsync(cancellationToken);
            if (loadUsersErrMsg is not null)
            {
                Console.WriteLine(loadUsersErrMsg);
                return 1;
            }

            if (allGroups)
            {
                var result = users.RemoveUserFromAllGroups(username);

                if (result == -2)
                {
                    Console.WriteLine($"User not found: {username}");
                    commandResult += result;
                }
            }

            foreach (var group in groups)
            {
                var result = users.RemoveUserFromGroup(username, group);

                if (result == 1)
                {
                    Console.WriteLine($"User {username} is not in group {group}.");
                    commandResult--;
                }
                else if (result == -2)
                {
                    Console.WriteLine($"User not found: {username}");
                    commandResult -= 2;
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

        public static async Task<int> AddCredential(
            string username,
            string[] groups,
            string method,
            string password,
            string userinfoBase64url,
            bool allGroups,
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

            if (allGroups)
                groups = nodes.Groups.Keys.ToArray();

            foreach (var group in groups)
            {
                if (!nodes.Groups.ContainsKey(group))
                {
                    Console.WriteLine($"Error: Group {group} doesn't exist.");
                    return -1;
                }

                int result;

                if (!string.IsNullOrEmpty(method) && !string.IsNullOrEmpty(password))
                    result = users.AddCredentialToUser(username, group, method, password);
                else if (!string.IsNullOrEmpty(userinfoBase64url))
                    result = users.AddCredentialToUser(username, group, userinfoBase64url);
                else
                    result = users.AddUserToGroup(username, group);

                switch (result)
                {
                    case 0:
                        Console.WriteLine($"Added {username} => {group}");
                        break;
                    case 1:
                        Console.WriteLine($"The user is already in the group.");
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

        public static async Task<int> RemoveCredentials(string username, string[] groups, bool allGroups, CancellationToken cancellationToken = default)
        {
            var commandResult = 0;

            var (users, loadUsersErrMsg) = await Users.LoadUsersAsync(cancellationToken);
            if (loadUsersErrMsg is not null)
            {
                Console.WriteLine(loadUsersErrMsg);
                return 1;
            }

            if (allGroups)
            {
                var result = users.RemoveAllCredentialsFromUser(username);
                if (result == -2)
                {
                    Console.WriteLine($"Error: user {username} doesn't exist.");
                    commandResult += result;
                }
            }

            foreach (var group in groups)
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

        public static async Task<int> ListCredentials(string[] usernames, string[] groups, CancellationToken cancellationToken = default)
        {
            var (users, loadUsersErrMsg) = await Users.LoadUsersAsync(cancellationToken);
            if (loadUsersErrMsg is not null)
            {
                Console.WriteLine(loadUsersErrMsg);
                return 1;
            }

            List<(string username, string group, string method, string password)> filteredCreds = new();

            foreach (var user in users.UserDict)
            {
                if (usernames.Length > 0 && !usernames.Contains(user.Key))
                    continue;

                foreach (var membership in user.Value.Memberships)
                {
                    if (groups.Length > 0 && !groups.Contains(membership.Key))
                        continue;

                    filteredCreds.Add((user.Key, membership.Key, membership.Value.Method, membership.Value.Password));
                }
            }

            Console.WriteLine($"{"Credentials",-16}{filteredCreds.Count}");

            if (filteredCreds.Count == 0)
            {
                return 0;
            }

            var maxUsernameLength = filteredCreds.Max(x => x.username.Length);
            var maxGroupNameLength = filteredCreds.Max(x => x.group.Length);
            var maxMethodLength = filteredCreds.Max(x => x.method.Length);
            var maxPasswordLength = filteredCreds.Max(x => x.password.Length);

            var usernameFieldWidth = maxUsernameLength > 4 ? maxUsernameLength + 2 : 6;
            var groupNameFieldWidth = maxGroupNameLength > 5 ? maxGroupNameLength + 2 : 7;
            var methodFieldWidth = maxMethodLength > 6 ? maxMethodLength + 2 : 8;
            var passwordFieldWidth = maxPasswordLength > 8 ? maxPasswordLength + 2 : 10;

            Console.WriteLine();
            ConsoleHelper.PrintTableBorder(usernameFieldWidth, groupNameFieldWidth, methodFieldWidth, passwordFieldWidth);
            Console.WriteLine($"|{"User".PadRight(usernameFieldWidth)}|{"Group".PadRight(groupNameFieldWidth)}|{"Method".PadRight(methodFieldWidth)}|{"Password".PadRight(passwordFieldWidth)}|");
            ConsoleHelper.PrintTableBorder(usernameFieldWidth, groupNameFieldWidth, methodFieldWidth, passwordFieldWidth);

            foreach (var (username, group, method, password) in filteredCreds)
            {
                Console.WriteLine($"|{username.PadRight(usernameFieldWidth)}|{group.PadRight(groupNameFieldWidth)}|{method.PadRight(methodFieldWidth)}|{password.PadRight(passwordFieldWidth)}|");
            }

            ConsoleHelper.PrintTableBorder(usernameFieldWidth, groupNameFieldWidth, methodFieldWidth, passwordFieldWidth);

            return 0;
        }

        public static async Task<int> GetSSLinks(string username, string[] groups, CancellationToken cancellationToken = default)
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

            var uris = users.GetUserSSUris(username, nodes, groups);
            if (uris is not null)
            {
                foreach (var uri in uris)
                    Console.WriteLine($"{uri.AbsoluteUri}");
            }
            else
            {
                Console.WriteLine($"Error: user {username} doesn't exist.");
                commandResult = -2;
            }

            return commandResult;
        }

        public static async Task<int> GetDataUsage(string username, SortBy? sortBy, CancellationToken cancellationToken = default)
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

            var records = users.GetUserDataUsage(username, nodes);
            if (records is null)
            {
                Console.WriteLine($"Error: user {username} doesn't exist.");
                return -2;
            }

            var maxNameLength = records.Select(x => x.group.Length)
                                       .DefaultIfEmpty()
                                       .Max();
            var nameFieldWidth = maxNameLength > 5 ? maxNameLength + 2 : 7;

            var sortByInEffect = settings.UserDataUsageDefaultSortBy;
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
                    records = records.OrderBy(x => x.group).ToList();
                    break;
                case SortBy.NameDescending:
                    records = records.OrderByDescending(x => x.group).ToList();
                    break;
                case SortBy.DataUsedAscending:
                    records = records.OrderBy(x => x.bytesUsed).ToList();
                    break;
                case SortBy.DataUsedDescending:
                    records = records.OrderByDescending(x => x.bytesUsed).ToList();
                    break;
                case SortBy.DataRemainingAscending:
                    records = records.OrderBy(x => x.bytesRemaining).ToList();
                    break;
                case SortBy.DataRemainingDescending:
                    records = records.OrderByDescending(x => x.bytesRemaining).ToList();
                    break;
            }

            Console.WriteLine($"{"User",-16}{username,-32}");

            if (users.UserDict.TryGetValue(username, out var user))
            {
                Console.WriteLine($"{"Data used",-16}{Utilities.HumanReadableDataString1024(user.BytesUsed),-32}");
                if (user.BytesRemaining != 0UL)
                    Console.WriteLine($"{"Data remaining",-16}{Utilities.HumanReadableDataString1024(user.BytesRemaining),-32}");
                if (user.DataLimitInBytes != 0UL)
                    Console.WriteLine($"{"Data limit",-16}{Utilities.HumanReadableDataString1024(user.DataLimitInBytes),-32}");
            }

            Console.WriteLine();

            if (records.All(x => x.bytesRemaining == 0UL)) // Omit data remaining column if no data.
            {
                ConsoleHelper.PrintTableBorder(nameFieldWidth, 11);
                Console.WriteLine($"|{"Group".PadRight(nameFieldWidth)}|{"Data Used",11}|");
                ConsoleHelper.PrintTableBorder(nameFieldWidth, 11);

                foreach (var (group, bytesUsed, _) in records)
                {
                    Console.WriteLine($"|{group.PadRight(nameFieldWidth)}|{Utilities.HumanReadableDataString1024(bytesUsed),11}|");
                }

                ConsoleHelper.PrintTableBorder(nameFieldWidth, 11);
            }
            else
            {
                ConsoleHelper.PrintTableBorder(nameFieldWidth, 11, 16);
                Console.WriteLine($"|{"Group".PadRight(nameFieldWidth)}|{"Data Used",11}|{"Data Remaining",16}|");
                ConsoleHelper.PrintTableBorder(nameFieldWidth, 11, 16);

                foreach (var (group, bytesUsed, bytesRemaining) in records)
                {
                    Console.Write($"|{group.PadRight(nameFieldWidth)}|{Utilities.HumanReadableDataString1024(bytesUsed),11}|");

                    if (bytesRemaining != 0UL)
                        Console.WriteLine($"{Utilities.HumanReadableDataString1024(bytesRemaining),16}|");
                    else
                        Console.WriteLine($"{string.Empty,16}|");
                }

                ConsoleHelper.PrintTableBorder(nameFieldWidth, 11, 16);
            }

            return 0;
        }

        public static async Task<int> GetDataLimit(string username, CancellationToken cancellationToken = default)
        {
            var (users, loadUsersErrMsg) = await Users.LoadUsersAsync(cancellationToken);
            if (loadUsersErrMsg is not null)
            {
                Console.WriteLine(loadUsersErrMsg);
                return 1;
            }

            if (users.UserDict.TryGetValue(username, out var user))
            {
                Console.WriteLine($"{"User",-24}{username,-32}");
                if (user.DataLimitInBytes != 0UL)
                    Console.WriteLine($"{"Global data limit",-24}{Utilities.HumanReadableDataString1024(user.DataLimitInBytes),-32}");
                if (user.PerGroupDataLimitInBytes != 0UL)
                    Console.WriteLine($"{"Per-group data limit",-24}{Utilities.HumanReadableDataString1024(user.PerGroupDataLimitInBytes),-32}");

                var customLimits = user.Memberships.Where(x => x.Value.DataLimitInBytes > 0UL).Select(x => (x.Key, x.Value.DataLimitInBytes));

                if (!customLimits.Any())
                {
                    return 0;
                }

                var maxNameLength = customLimits.Select(x => x.Key.Length)
                                                .DefaultIfEmpty()
                                                .Max();
                var nameFieldWidth = maxNameLength > 5 ? maxNameLength + 2 : 7;

                Console.WriteLine();

                ConsoleHelper.PrintTableBorder(nameFieldWidth, 19);

                Console.WriteLine($"|{"Group".PadRight(nameFieldWidth)}|{"Custom Data Limit",19}|");

                ConsoleHelper.PrintTableBorder(nameFieldWidth, 19);

                foreach ((var group, var dataLimitInBytes) in customLimits)
                {
                    Console.WriteLine($"|{group.PadRight(nameFieldWidth)}|{Utilities.HumanReadableDataString1024(dataLimitInBytes),19}|");
                }

                ConsoleHelper.PrintTableBorder(nameFieldWidth, 19);

                return 0;
            }
            else
            {
                Console.WriteLine($"Error: user {username} doesn't exist.");
                return -2;
            }
        }

        public static string? ValidateSetDataLimit(CommandResult commandResult)
        {
            var hasGlobal = commandResult.Children.Contains("--global");
            var hasPerGroup = commandResult.Children.Contains("--per-group");
            var hasGroups = commandResult.Children.Contains("--groups");

            if (!hasGlobal && !hasPerGroup)
                return "Please specify either a global data limit with `--global`, or a per-group data limit with `--per-group`.";

            if (!hasPerGroup && hasGroups)
                return "Custom group targets must be used with per-group limits.";

            return null;
        }

        public static async Task<int> SetDataLimit(string[] usernames, ulong? global, ulong? perGroup, string[] groups, CancellationToken cancellationToken = default)
        {
            var commandResult = 0;

            var (users, loadUsersErrMsg) = await Users.LoadUsersAsync(cancellationToken);
            if (loadUsersErrMsg is not null)
            {
                Console.WriteLine(loadUsersErrMsg);
                return 1;
            }

            if (global is ulong globalDataLimit)
            {
                foreach (var username in usernames)
                {
                    var result = users.SetUserGlobalDataLimit(username, globalDataLimit);
                    switch (result)
                    {
                        case 0:
                            Console.WriteLine($"Global data limit set on {username}.");
                            break;
                        case -1:
                            Console.WriteLine($"Error: user {username} doesn't exist.");
                            break;
                        default:
                            Console.WriteLine($"Unknown error.");
                            break;
                    }
                    commandResult += result;
                }
            }

            if (perGroup is ulong perGroupDataLimit)
            {
                foreach (var username in usernames)
                {
                    if (groups.Length == 0)
                    {
                        var result = users.SetUserPerGroupDataLimit(username, perGroupDataLimit);
                        switch (result)
                        {
                            case 0:
                                Console.WriteLine($"Per-group data limit set on {username}.");
                                break;
                            case -1:
                                Console.WriteLine($"Error: user {username} doesn't exist.");
                                break;
                            default:
                                Console.WriteLine($"Unknown error.");
                                break;
                        }
                        commandResult += result;
                    }
                    else
                    {
                        foreach (var group in groups)
                        {
                            var result = users.SetUserDataLimitInGroup(username, group, perGroupDataLimit);
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

            return commandResult;
        }

        public static Task<int> OwnGroups(string username, string[] groups, bool allGroups, bool force, CancellationToken cancellationToken = default)
            => ManageGroupOwnership(username, true, groups, allGroups, force, cancellationToken);

        public static Task<int> DisownGroups(string username, string[] groups, bool allGroups, CancellationToken cancellationToken = default)
            => ManageGroupOwnership(username, false, groups, allGroups, false, cancellationToken);

        public static async Task<int> ManageGroupOwnership(string username, bool own, string[] groups, bool allGroups, bool force = false, CancellationToken cancellationToken = default)
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

            if (users.UserDict.TryGetValue(username, out var user))
            {
                foreach (var groupEntry in nodes.Groups)
                {
                    if (!allGroups && !groups.Contains(groupEntry.Key))
                        continue;

                    if (own)
                    {
                        if (force || groupEntry.Value.OwnerUuid is null)
                        {
                            groupEntry.Value.OwnerUuid = user.Uuid;
                            Console.WriteLine($"Set user {username} as owner of group {groupEntry.Key}.");
                        }
                        else if (groupEntry.Value.OwnerUuid == user.Uuid)
                        {
                            Console.WriteLine($"User {username} already is the owner of group {groupEntry.Key}.");
                        }
                        else
                        {
                            var owner = users.UserDict.Where(x => x.Value.Uuid == groupEntry.Value.OwnerUuid)
                                                      .Select(x => x.Key)
                                                      .FirstOrDefault();
                            Console.WriteLine($"Group {groupEntry.Key} already has owner {owner}. Disown it first or use `--force` to overwrite.");
                        }
                    }
                    else
                    {
                        if (groupEntry.Value.OwnerUuid == user.Uuid)
                        {
                            groupEntry.Value.OwnerUuid = null;
                            Console.WriteLine($"Disowned group {groupEntry.Key}.");
                        }
                        else
                        {
                            Console.WriteLine($"Warning: Group {groupEntry.Key} is not owned by user {username}. Skipping.");
                        }
                    }
                }

                var saveNodesErrMsg = await Nodes.SaveNodesAsync(nodes, cancellationToken);
                if (saveNodesErrMsg is not null)
                {
                    Console.WriteLine(saveNodesErrMsg);
                    return 1;
                }
            }
            else
            {
                Console.WriteLine($"Error: user {username} doesn't exist.");
                return -2;
            }

            return 0;
        }

        public static Task<int> OwnNodes(string username, string[] groups, bool allGroups, string[] nodenames, bool allNodes, bool force, CancellationToken cancellationToken = default)
            => ManageNodeOwnership(username, true, groups, allGroups, nodenames, allNodes, force, cancellationToken);

        public static Task<int> DisownNodes(string username, string[] groups, bool allGroups, string[] nodenames, bool allNodes, CancellationToken cancellationToken = default)
            => ManageNodeOwnership(username, false, groups, allGroups, nodenames, allNodes, false, cancellationToken);

        public static async Task<int> ManageNodeOwnership(string username, bool own, string[] groups, bool allGroups, string[] nodenames, bool allNodes, bool force = false, CancellationToken cancellationToken = default)
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

            if (users.UserDict.TryGetValue(username, out var user))
            {
                foreach (var groupEntry in nodes.Groups)
                {
                    if (!allGroups && !groups.Contains(groupEntry.Key))
                        continue;

                    foreach (var nodeEntry in groupEntry.Value.NodeDict)
                    {
                        if (!allNodes && !nodenames.Contains(nodeEntry.Key))
                            continue;

                        var nodename = nodeEntry.Key;
                        var node = nodeEntry.Value;

                        if (own)
                        {
                            if (force || node.OwnerUuid is null)
                            {
                                node.OwnerUuid = user.Uuid;
                                Console.WriteLine($"Set user {username} as owner of node {nodename} in group {groupEntry.Key}.");
                            }
                            else if (node.OwnerUuid == user.Uuid)
                            {
                                Console.WriteLine($"User {username} already is the owner of node {nodename} in group {groupEntry.Key}.");
                            }
                            else
                            {
                                var owner = users.UserDict.Where(x => x.Value.Uuid == groupEntry.Value.OwnerUuid)
                                                          .Select(x => x.Key)
                                                          .FirstOrDefault();
                                Console.WriteLine($"Node {nodename} in group {groupEntry.Key} already has owner {owner}. Disown it first or use `--force` to overwrite.");
                            }
                        }
                        else
                        {
                            if (node.OwnerUuid == user.Uuid)
                            {
                                node.OwnerUuid = null;
                                Console.WriteLine($"Disowned node {nodename} in group {groupEntry.Key}.");
                            }
                            else
                            {
                                Console.WriteLine($"Warning: Node {nodename} in group {groupEntry.Key} is not owned by user {username}. Skipping.");
                            }
                        }
                    }
                }

                var saveNodesErrMsg = await Nodes.SaveNodesAsync(nodes, cancellationToken);
                if (saveNodesErrMsg is not null)
                {
                    Console.WriteLine(saveNodesErrMsg);
                    return 1;
                }
            }
            else
            {
                Console.WriteLine($"Error: user {username} doesn't exist.");
                return -2;
            }

            return 0;
        }

        public static async Task<int> ListOwnedGroups(string username, CancellationToken cancellationToken = default)
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

            if (users.UserDict.TryGetValue(username, out var user))
            {
                var ownedGroupEntries = nodes.Groups.Where(x => x.Value.OwnerUuid == user.Uuid);

                Console.WriteLine($"Owned Groups: {ownedGroupEntries.Count()}");

                foreach (var groupEntry in ownedGroupEntries)
                {
                    Console.WriteLine(groupEntry.Key);
                }
            }
            else
            {
                Console.WriteLine($"Error: user {username} doesn't exist.");
                return -2;
            }

            return 0;
        }

        public static async Task<int> ListOwnedNodes(string username, string[] groups, CancellationToken cancellationToken = default)
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

            if (users.UserDict.TryGetValue(username, out var user))
            {
                var totalCount = 0;

                foreach (var groupEntry in nodes.Groups)
                {
                    if (groups.Length > 0 && !groups.Contains(groupEntry.Key))
                        continue;

                    var ownedNodeEntries = groupEntry.Value.NodeDict.Where(x => x.Value.OwnerUuid == user.Uuid);

                    if (ownedNodeEntries.Any())
                    {
                        totalCount += ownedNodeEntries.Count();

                        Console.WriteLine($"From group {groupEntry.Key}: {ownedNodeEntries.Count()}");

                        foreach (var nodeEntry in ownedNodeEntries)
                        {
                            Console.WriteLine(nodeEntry.Key);
                        }

                        Console.WriteLine();
                    }
                }

                Console.WriteLine($"Total owned nodes: {totalCount}");
            }
            else
            {
                Console.WriteLine($"Error: user {username} doesn't exist.");
                return -2;
            }

            return 0;
        }
    }
}
