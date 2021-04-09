using ShadowsocksUriGenerator.CLI.Utils;
using System;
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
            var users = await JsonHelper.LoadUsersAsync(cancellationToken);

            foreach (var username in usernames)
            {
                var result = users.AddUser(username);
                commandResult += result;
                if (result == 0)
                    Console.WriteLine($"Added {username}.");
                else
                    Console.WriteLine($"{username} already exists.");
            }

            await JsonHelper.SaveUsersAsync(users, cancellationToken);
            return commandResult;
        }

        public static async Task<int> Rename(string oldName, string newName, CancellationToken cancellationToken = default)
        {
            var users = await JsonHelper.LoadUsersAsync(cancellationToken);
            using var nodes = await JsonHelper.LoadNodesAsync(cancellationToken);

            var result = await users.RenameUser(oldName, newName, nodes, cancellationToken);
            switch (result)
            {
                case -1:
                    Console.WriteLine($"User not found: {oldName}");
                    break;
                case -2:
                    Console.WriteLine($"A user with the same name already exists: {newName}");
                    break;
                case -3:
                    Console.WriteLine("An error occurred while sending renaming requests to Outline server.");
                    break;
                default:
                    Console.WriteLine($"Unknown error.");
                    break;
            }

            await JsonHelper.SaveUsersAsync(users, cancellationToken);
            return result;
        }

        public static async Task<int> Remove(string[] usernames, CancellationToken cancellationToken = default)
        {
            var commandResult = 0;
            var users = await JsonHelper.LoadUsersAsync(cancellationToken);
            var settings = await JsonHelper.LoadSettingsAsync(cancellationToken);

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

            await JsonHelper.SaveUsersAsync(users, cancellationToken);
            return commandResult;
        }

        public static async Task List(bool namesOnly, bool onePerLine, CancellationToken cancellationToken = default)
        {
            var users = await JsonHelper.LoadUsersAsync(cancellationToken);

            if (namesOnly)
            {
                var usernames = users.UserDict.Keys.ToList();
                ConsoleHelper.PrintNameList(usernames, onePerLine);
                return;
            }

            var maxNameLength = users.UserDict.Select(x => x.Key.Length)
                                              .DefaultIfEmpty()
                                              .Max();
            var nameFieldWidth = maxNameLength > 4 ? maxNameLength + 2 : 6;

            ConsoleHelper.PrintTableBorder(nameFieldWidth, 36, 18);
            Console.WriteLine($"|{"User".PadRight(nameFieldWidth)}|{"UUID",36}|{"Associated Groups",18}|");
            ConsoleHelper.PrintTableBorder(nameFieldWidth, 36, 18);

            foreach (var user in users.UserDict)
                Console.WriteLine($"|{user.Key.PadRight(nameFieldWidth)}|{user.Value.Uuid,36}|{user.Value.Memberships.Count,18}|");

            ConsoleHelper.PrintTableBorder(nameFieldWidth, 36, 18);
        }

        public static async Task<int> JoinGroups(string username, string[] groups, bool allGroups, CancellationToken cancellationToken = default)
        {
            var commandResult = 0;
            var users = await JsonHelper.LoadUsersAsync(cancellationToken);
            using var nodes = await JsonHelper.LoadNodesAsync(cancellationToken);

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

            await JsonHelper.SaveUsersAsync(users, cancellationToken);
            return commandResult;
        }

        public static async Task<int> LeaveGroups(string username, string[] groups, bool allGroups, CancellationToken cancellationToken = default)
        {
            var commandResult = 0;
            var users = await JsonHelper.LoadUsersAsync(cancellationToken);

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

            await JsonHelper.SaveUsersAsync(users, cancellationToken);
            return commandResult;
        }

        public static async Task<int> AddCredential(
            string username,
            string[] groups,
            string? method,
            string? password,
            string? userinfoBase64url,
            bool allGroups,
            CancellationToken cancellationToken = default)
        {
            var commandResult = 0;
            var users = await JsonHelper.LoadUsersAsync(cancellationToken);
            using var nodes = await JsonHelper.LoadNodesAsync(cancellationToken);

            // Workaround for https://github.com/dotnet/command-line-api/issues/1233
            groups ??= Array.Empty<string>();

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

            await JsonHelper.SaveUsersAsync(users, cancellationToken);
            return commandResult;
        }

        public static async Task<int> RemoveCredentials(string username, string[] groups, bool allGroups, CancellationToken cancellationToken = default)
        {
            var commandResult = 0;
            var users = await JsonHelper.LoadUsersAsync(cancellationToken);

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

            await JsonHelper.SaveUsersAsync(users, cancellationToken);
            return commandResult;
        }

        public static async Task ListCredentials(string[] usernames, string[] groups, CancellationToken cancellationToken = default)
        {
            var users = await JsonHelper.LoadUsersAsync(cancellationToken);

            var maxUsernameLength = users.UserDict.Select(x => x.Key.Length)
                                                  .DefaultIfEmpty()
                                                  .Max();
            var maxGroupNameLength = users.UserDict.SelectMany(x => x.Value.Memberships.Keys)
                                                   .Select(x => x.Length)
                                                   .DefaultIfEmpty()
                                                   .Max();
            var maxPasswordLength = users.UserDict.SelectMany(x => x.Value.Memberships.Values)
                                                  .Select(x => x.Password.Length)
                                                  .DefaultIfEmpty()
                                                  .Max();
            var usernameFieldWidth = maxUsernameLength > 4 ? maxUsernameLength + 2 : 6;
            var groupNameFieldWidth = maxGroupNameLength > 5 ? maxGroupNameLength + 2 : 7;
            var passwordFieldWidth = maxPasswordLength > 8 ? maxPasswordLength + 2 : 10;

            ConsoleHelper.PrintTableBorder(usernameFieldWidth, groupNameFieldWidth, 24, passwordFieldWidth);
            Console.WriteLine($"|{"User".PadRight(usernameFieldWidth)}|{"Group".PadRight(groupNameFieldWidth)}|{"Method",-24}|{"Password".PadRight(passwordFieldWidth)}|");
            ConsoleHelper.PrintTableBorder(usernameFieldWidth, groupNameFieldWidth, 24, passwordFieldWidth);

            foreach (var user in users.UserDict)
            {
                if (usernames.Length > 0 && !usernames.Contains(user.Key))
                    continue;

                foreach (var membership in user.Value.Memberships)
                {
                    if (groups.Length > 0 && !groups.Contains(membership.Key))
                        continue;

                    Console.WriteLine($"|{user.Key.PadRight(usernameFieldWidth)}|{membership.Key.PadRight(groupNameFieldWidth)}|{membership.Value.Method,-24}|{membership.Value.Password.PadRight(passwordFieldWidth)}|");
                }
            }

            ConsoleHelper.PrintTableBorder(usernameFieldWidth, groupNameFieldWidth, 24, passwordFieldWidth);
        }

        public static async Task<int> GetSSLinks(string username, string[] groups, CancellationToken cancellationToken = default)
        {
            var commandResult = 0;
            var users = await JsonHelper.LoadUsersAsync(cancellationToken);
            using var nodes = await JsonHelper.LoadNodesAsync(cancellationToken);

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
            var users = await JsonHelper.LoadUsersAsync(cancellationToken);
            using var nodes = await JsonHelper.LoadNodesAsync(cancellationToken);
            var settings = await JsonHelper.LoadSettingsAsync(cancellationToken);

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
                Console.WriteLine($"{"Data used",-16}{Utilities.HumanReadableDataString(user.BytesUsed),-32}");
                if (user.BytesRemaining != 0UL)
                    Console.WriteLine($"{"Data remaining",-16}{Utilities.HumanReadableDataString(user.BytesRemaining),-32}");
                if (user.DataLimitInBytes != 0UL)
                    Console.WriteLine($"{"Data limit",-16}{Utilities.HumanReadableDataString(user.DataLimitInBytes),-32}");
            }

            Console.WriteLine();

            ConsoleHelper.PrintTableBorder(nameFieldWidth, 11, 16);

            Console.WriteLine($"|{"Group".PadRight(nameFieldWidth)}|{"Data Used",11}|{"Data Remaining",16}|");

            ConsoleHelper.PrintTableBorder(nameFieldWidth, 11, 16);

            foreach (var (group, bytesUsed, bytesRemaining) in records)
            {
                Console.Write($"|{group.PadRight(nameFieldWidth)}|{Utilities.HumanReadableDataString(bytesUsed),11}|");
                if (bytesRemaining != 0UL)
                    Console.WriteLine($"{Utilities.HumanReadableDataString(bytesRemaining),16}|");
                else
                    Console.WriteLine($"{string.Empty,16}|");
            }

            ConsoleHelper.PrintTableBorder(nameFieldWidth, 11, 16);

            return 0;
        }

        public static string? ValidatePerGroupDataLimit(CommandResult commandResult)
        {
            var hasPerGroup = commandResult.Children.Contains("--per-group");
            var hasGroups = commandResult.Children.Contains("--groups");

            if (!hasPerGroup && hasGroups)
                return "Custom group targets must be used with per-group limits.";

            return null;
        }

        public static async Task<int> SetDataLimit(string[] usernames, ulong? global, ulong? perGroup, string[] groups, CancellationToken cancellationToken = default)
        {
            var commandResult = 0;
            var users = await JsonHelper.LoadUsersAsync(cancellationToken);

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

            await JsonHelper.SaveUsersAsync(users, cancellationToken);
            return commandResult;
        }
    }
}
