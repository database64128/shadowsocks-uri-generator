using ShadowsocksUriGenerator.CLI.Utils;
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
        public static async Task<int> Add(string[] groups, CancellationToken cancellationToken = default)
        {
            var commandResult = 0;
            using var nodes = await JsonHelper.LoadNodesAsync(cancellationToken);

            foreach (var group in groups)
            {
                var result = nodes.AddGroup(group);
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

            await JsonHelper.SaveNodesAsync(nodes, cancellationToken);
            return commandResult;
        }

        public static async Task<int> Rename(string oldName, string newName, CancellationToken cancellationToken = default)
        {
            var users = await JsonHelper.LoadUsersAsync(cancellationToken);
            using var nodes = await JsonHelper.LoadNodesAsync(cancellationToken);

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

            await JsonHelper.SaveUsersAsync(users, cancellationToken);
            await JsonHelper.SaveNodesAsync(nodes, cancellationToken);
            return result;
        }

        public static async Task<int> Remove(string[] groups, CancellationToken cancellationToken = default)
        {
            var commandResult = 0;
            var users = await JsonHelper.LoadUsersAsync(cancellationToken);
            using var nodes = await JsonHelper.LoadNodesAsync(cancellationToken);

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

            await JsonHelper.SaveUsersAsync(users, cancellationToken);
            await JsonHelper.SaveNodesAsync(nodes, cancellationToken);
            return commandResult;
        }

        public static async Task List(bool namesOnly, bool onePerLine, CancellationToken cancellationToken = default)
        {
            using var nodes = await JsonHelper.LoadNodesAsync(cancellationToken);

            if (namesOnly)
            {
                var names = nodes.Groups.Keys.ToList();
                ConsoleHelper.PrintNameList(names, onePerLine);
                return;
            }

            var maxGroupNameLength = nodes.Groups.Select(x => x.Key.Length)
                                                 .DefaultIfEmpty()
                                                 .Max();
            var maxOutlineServerNameLength = nodes.Groups.Select(x => x.Value.OutlineServerInfo?.Name.Length ?? 0)
                                                         .DefaultIfEmpty()
                                                         .Max();
            var groupNameFieldWidth = maxGroupNameLength > 5 ? maxGroupNameLength + 2 : 7;
            var outlineServerNameFieldWidth = maxOutlineServerNameLength > 14 ? maxOutlineServerNameLength + 2 : 16;

            ConsoleHelper.PrintTableBorder(groupNameFieldWidth, 16, outlineServerNameFieldWidth);
            Console.WriteLine($"|{"Group".PadRight(groupNameFieldWidth)}|{"Number of Nodes",16}|{"Outline Server".PadLeft(outlineServerNameFieldWidth)}|");
            ConsoleHelper.PrintTableBorder(groupNameFieldWidth, 16, outlineServerNameFieldWidth);

            foreach (var group in nodes.Groups)
            {
                Console.WriteLine($"|{group.Key.PadRight(groupNameFieldWidth)}|{group.Value.NodeDict.Count,16}|{(group.Value.OutlineServerInfo?.Name ?? "No").PadLeft(outlineServerNameFieldWidth)}|");
            }

            ConsoleHelper.PrintTableBorder(groupNameFieldWidth, 16, outlineServerNameFieldWidth);
        }

        public static async Task<int> AddUsers(string group, string[] usernames, bool allUsers, CancellationToken cancellationToken = default)
        {
            var commandResult = 0;
            var users = await JsonHelper.LoadUsersAsync(cancellationToken);
            using var nodes = await JsonHelper.LoadNodesAsync(cancellationToken);

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

            await JsonHelper.SaveUsersAsync(users, cancellationToken);
            return commandResult;
        }

        public static async Task<int> RemoveUsers(string group, string[] usernames, bool allUsers, CancellationToken cancellationToken = default)
        {
            var commandResult = 0;
            var users = await JsonHelper.LoadUsersAsync(cancellationToken);
            using var nodes = await JsonHelper.LoadNodesAsync(cancellationToken);

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

            await JsonHelper.SaveUsersAsync(users, cancellationToken);
            return commandResult;
        }

        public static async Task ListUsers(string group, CancellationToken cancellationToken = default)
        {
            var users = await JsonHelper.LoadUsersAsync(cancellationToken);

            var maxUsernameLength = users.UserDict.Select(x => x.Key.Length)
                                                  .DefaultIfEmpty()
                                                  .Max();
            var maxPasswordLength = users.UserDict.SelectMany(x => x.Value.Memberships.Values)
                                                  .Select(x => x.Password.Length)
                                                  .DefaultIfEmpty()
                                                  .Max();
            var usernameFieldWidth = maxUsernameLength > 4 ? maxUsernameLength + 2 : 6;
            var passwordFieldWidth = maxPasswordLength > 8 ? maxPasswordLength + 2 : 10;

            Console.WriteLine($"{"Group",-16}{group,-32}");
            Console.WriteLine();

            ConsoleHelper.PrintTableBorder(usernameFieldWidth, 24, passwordFieldWidth);
            Console.WriteLine($"|{"User".PadRight(usernameFieldWidth)}|{"Method",-24}|{"Password".PadRight(passwordFieldWidth)}|");
            ConsoleHelper.PrintTableBorder(usernameFieldWidth, 24, passwordFieldWidth);

            foreach (var user in users.UserDict)
            {
                if (user.Value.Memberships.TryGetValue(group, out var memberinfo))
                {
                    Console.WriteLine($"|{user.Key.PadRight(usernameFieldWidth)}|{memberinfo.Method,-24}|{memberinfo.Password.PadRight(passwordFieldWidth)}|");
                }
            }

            ConsoleHelper.PrintTableBorder(usernameFieldWidth, 24, passwordFieldWidth);
        }

        public static async Task<int> AddCredential(
            string group,
            string[] usernames,
            string? method,
            string? password,
            string? userinfoBase64url,
            bool allUsers,
            CancellationToken cancellationToken = default)
        {
            var commandResult = 0;
            var users = await JsonHelper.LoadUsersAsync(cancellationToken);
            using var nodes = await JsonHelper.LoadNodesAsync(cancellationToken);

            if (!nodes.Groups.ContainsKey(group))
            {
                Console.WriteLine($"Error: Group {group} doesn't exist.");
                return -2;
            }

            if (allUsers)
            {
                foreach (var userEntry in users.UserDict)
                {
                    int result;
                    if (!string.IsNullOrEmpty(method) && !string.IsNullOrEmpty(password))
                        result = userEntry.Value.AddCredential(group, method, password);
                    else if (!string.IsNullOrEmpty(userinfoBase64url))
                        result = userEntry.Value.AddCredential(group, userinfoBase64url);
                    else
                    {
                        Console.WriteLine("You must specify either `--method <method> --password <password>` or `--userinfo-base64url <base64url>`.");
                        return -1;
                    }
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
                int result;
                if (!string.IsNullOrEmpty(method) && !string.IsNullOrEmpty(password))
                    result = users.AddCredentialToUser(username, group, method, password);
                else if (!string.IsNullOrEmpty(userinfoBase64url))
                    result = users.AddCredentialToUser(username, group, userinfoBase64url);
                else
                {
                    Console.WriteLine("You must specify either `--method <method> --password <password>` or `--userinfo-base64url <base64url>`.");
                    return -1;
                }
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

            await JsonHelper.SaveUsersAsync(users, cancellationToken);
            return commandResult;
        }

        public static async Task<int> RemoveCredentials(string group, string[] usernames, bool allUsers, CancellationToken cancellationToken = default)
        {
            var commandResult = 0;
            var users = await JsonHelper.LoadUsersAsync(cancellationToken);
            using var nodes = await JsonHelper.LoadNodesAsync(cancellationToken);

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

            await JsonHelper.SaveUsersAsync(users, cancellationToken);
            return commandResult;
        }

        public static async Task<int> ListCredentials(string group, CancellationToken cancellationToken = default)
        {
            var users = await JsonHelper.LoadUsersAsync(cancellationToken);
            using var nodes = await JsonHelper.LoadNodesAsync(cancellationToken);

            if (!nodes.Groups.ContainsKey(group))
            {
                Console.WriteLine($"Error: Group {group} doesn't exist.");
                return -2;
            }

            var groupCreds = new List<(string username, string method, string password)>();
            foreach (var userEntry in users.UserDict)
            {
                var groupCred = userEntry.Value.Memberships.Where(x => x.Key == group)
                                                           .Select(x => (userEntry.Key, x.Value.Method, x.Value.Password));
                groupCreds.AddRange(groupCred);
            }

            Console.WriteLine($"{"Group",-16}{group}");
            Console.WriteLine($"{"Credentials",-16}{groupCreds.Count}");
            Console.WriteLine();

            if (groupCreds.Count == 0)
            {
                return 0;
            }

            var maxUsernameLength = groupCreds.Select(x => x.username.Length)
                                              .DefaultIfEmpty()
                                              .Max();
            var maxMethodLength = groupCreds.Select(x => x.method.Length)
                                            .DefaultIfEmpty()
                                            .Max();
            var maxPasswordLength = groupCreds.Select(x => x.password.Length)
                                              .DefaultIfEmpty()
                                              .Max();
            var usernameFieldWidth = maxUsernameLength > 4 ? maxUsernameLength + 2 : 6;
            var methodFieldWidth = maxMethodLength > 6 ? maxMethodLength + 2 : 8;
            var passwordFieldWidth = maxPasswordLength > 8 ? maxPasswordLength + 2 : 10;

            ConsoleHelper.PrintTableBorder(usernameFieldWidth, methodFieldWidth, passwordFieldWidth);
            Console.WriteLine($"|{"User".PadRight(usernameFieldWidth)}|{"Method".PadRight(methodFieldWidth)}|{"Password".PadRight(passwordFieldWidth)}|");
            ConsoleHelper.PrintTableBorder(usernameFieldWidth, methodFieldWidth, passwordFieldWidth);

            foreach (var (username, method, password) in groupCreds)
            {
                Console.WriteLine($"|{username.PadRight(usernameFieldWidth)}|{method.PadRight(methodFieldWidth)}|{password.PadRight(passwordFieldWidth)}|");
            }

            ConsoleHelper.PrintTableBorder(usernameFieldWidth, methodFieldWidth, passwordFieldWidth);

            return 0;
        }

        public static async Task<int> GetDataUsage(string group, SortBy? sortBy, CancellationToken cancellationToken = default)
        {
            var users = await JsonHelper.LoadUsersAsync(cancellationToken);
            using var nodes = await JsonHelper.LoadNodesAsync(cancellationToken);
            var settings = await JsonHelper.LoadSettingsAsync(cancellationToken);

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
                    records = records.OrderBy(x => x.username).ToList();
                    break;
                case SortBy.NameDescending:
                    records = records.OrderByDescending(x => x.username).ToList();
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

            Console.WriteLine($"{"Group",-16}{group,-32}");
            if (nodes.Groups.TryGetValue(group, out var targetGroup))
            {
                Console.WriteLine($"{"Data used",-16}{Utilities.HumanReadableDataString(targetGroup.BytesUsed),-32}");
                if (targetGroup.BytesRemaining != 0UL)
                    Console.WriteLine($"{"Data remaining",-16}{Utilities.HumanReadableDataString(targetGroup.BytesRemaining),-32}");
                if (targetGroup.DataLimitInBytes != 0UL)
                    Console.WriteLine($"{"Data limit",-16}{Utilities.HumanReadableDataString(targetGroup.DataLimitInBytes),-32}");
            }

            Console.WriteLine();

            ConsoleHelper.PrintTableBorder(nameFieldWidth, 11, 16);

            Console.WriteLine($"|{"User".PadRight(nameFieldWidth)}|{"Data Used",11}|{"Data Remaining",16}|");

            ConsoleHelper.PrintTableBorder(nameFieldWidth, 11, 16);

            foreach (var (username, bytesUsed, bytesRemaining) in records)
            {
                Console.Write($"|{username.PadRight(nameFieldWidth)}|{Utilities.HumanReadableDataString(bytesUsed),11}|");
                if (bytesRemaining != 0UL)
                    Console.WriteLine($"{Utilities.HumanReadableDataString(bytesRemaining),16}|");
                else
                    Console.WriteLine($"{string.Empty,16}|");
            }

            ConsoleHelper.PrintTableBorder(nameFieldWidth, 11, 16);
            return 0;
        }

        public static string? ValidatePerUserDataLimit(CommandResult commandResult)
        {
            var hasPerUser = commandResult.Children.Contains("--per-user");
            var hasUsernames = commandResult.Children.Contains("--usernames");

            if (!hasPerUser && hasUsernames)
                return "Custom user targets must be used with per-user limits.";

            return null;
        }

        public static async Task<int> SetDataLimit(
            string[] groups,
            ulong? global,
            ulong? perUser,
            string[] usernames,
            CancellationToken cancellationToken = default)
        {
            var commandResult = 0;
            var users = await JsonHelper.LoadUsersAsync(cancellationToken);
            using var nodes = await JsonHelper.LoadNodesAsync(cancellationToken);

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

            await JsonHelper.SaveUsersAsync(users, cancellationToken);
            await JsonHelper.SaveNodesAsync(nodes, cancellationToken);
            return commandResult;
        }
    }
}
