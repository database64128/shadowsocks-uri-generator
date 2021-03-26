using System;
using System.Linq;
using System.Threading.Tasks;

namespace ShadowsocksUriGenerator.CLI
{
    public static class UserCommand
    {
        public static async Task<int> Add(string[] usernames)
        {
            var commandResult = 0;
            var users = await Users.LoadUsersAsync();

            foreach (var username in usernames)
            {
                var result = users.AddUser(username);
                commandResult += result;
                if (result == 0)
                    Console.WriteLine($"Added {username}.");
                else
                    Console.WriteLine($"{username} already exists.");
            }

            await Users.SaveUsersAsync(users);
            return commandResult;
        }

        public static async Task<int> Rename(string oldName, string newName)
        {
            var users = await Users.LoadUsersAsync();
            using var nodes = await Nodes.LoadNodesAsync();

            var result = await users.RenameUser(oldName, newName, nodes);
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

            await Users.SaveUsersAsync(users);
            return result;
        }

        public static async Task<int> Remove(string[] usernames)
        {
            var commandResult = 0;
            var users = await Users.LoadUsersAsync();
            var settings = await Settings.LoadSettingsAsync();

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

            await Users.SaveUsersAsync(users);
            return commandResult;
        }

        public static async Task List(bool namesOnly, bool onePerLine)
        {
            var users = await Users.LoadUsersAsync();

            if (namesOnly)
            {
                var usernames = users.UserDict.Keys.ToList();
                Utilities.PrintNameList(usernames, onePerLine);
                return;
            }

            var maxNameLength = users.UserDict.Select(x => x.Key.Length)
                                              .DefaultIfEmpty()
                                              .Max();
            var nameFieldWidth = maxNameLength > 4 ? maxNameLength + 2 : 6;

            Utilities.PrintTableBorder(nameFieldWidth, 36, 18);
            Console.WriteLine($"|{"User".PadRight(nameFieldWidth)}|{"UUID",36}|{"Associated Groups",18}|");
            Utilities.PrintTableBorder(nameFieldWidth, 36, 18);

            foreach (var user in users.UserDict)
                Console.WriteLine($"|{user.Key.PadRight(nameFieldWidth)}|{user.Value.Uuid,36}|{user.Value.Credentials.Count,18}|");

            Utilities.PrintTableBorder(nameFieldWidth, 36, 18);
        }

        public static async Task<int> JoinGroups(string username, string[] groups, bool allGroups)
        {
            var commandResult = 0;
            var users = await Users.LoadUsersAsync();
            using var nodes = await Nodes.LoadNodesAsync();

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

            await Users.SaveUsersAsync(users);
            return commandResult;
        }

        public static async Task<int> LeaveGroups(string username, string[] groups, bool allGroups)
        {
            var commandResult = 0;
            var users = await Users.LoadUsersAsync();

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

            await Users.SaveUsersAsync(users);
            return commandResult;
        }

        public static async Task<int> AddCredential(string username, string group, string? method, string? password, string? userinfoBase64url)
        {
            var users = await Users.LoadUsersAsync();
            using var nodes = await Nodes.LoadNodesAsync();

            if (!nodes.Groups.ContainsKey(group))
            {
                Console.WriteLine($"Group not found: {group}");
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
                    Console.WriteLine($"Successfully added {username} to {group}.");
                    break;
                case 1:
                    Console.WriteLine($"The user is already in the group.");
                    break;
                case 2:
                    Console.WriteLine("The user already has a credential for the group.");
                    break;
                case -1:
                    Console.WriteLine($"User not found: {username}");
                    break;
                case -2:
                    Console.WriteLine("The provided credential is invalid.");
                    break;
                default:
                    Console.WriteLine($"Unknown error: {result}.");
                    break;
            }

            await Users.SaveUsersAsync(users);
            return result;
        }

        public static async Task<int> RemoveCredentials(string username, string[] groups, bool allGroups)
        {
            var commandResult = 0;
            var users = await Users.LoadUsersAsync();

            if (allGroups)
            {
                var result = users.RemoveAllCredentialsFromUser(username);
                if (result == -2)
                {
                    Console.WriteLine($"User not found: {username}");
                    commandResult += result;
                }
            }

            foreach (var group in groups)
            {
                var result = users.RemoveCredentialFromUser(username, group);
                if (result == -1)
                    Console.WriteLine($"User {username} is not in group {group}.");
                else if (result == -2)
                    Console.WriteLine($"User not found: {username}");
                commandResult += result;
            }

            await Users.SaveUsersAsync(users);
            return commandResult;
        }

        public static async Task ListCredentials(string[] usernames, string[] groups)
        {
            var users = await Users.LoadUsersAsync();

            var maxUsernameLength = users.UserDict.Select(x => x.Key.Length)
                                                  .DefaultIfEmpty()
                                                  .Max();
            var maxGroupNameLength = users.UserDict.SelectMany(x => x.Value.Credentials.Keys)
                                                   .Select(x => x.Length)
                                                   .DefaultIfEmpty()
                                                   .Max();
            var maxPasswordLength = users.UserDict.SelectMany(x => x.Value.Credentials.Values)
                                                  .Select(x => x?.Password.Length ?? 0)
                                                  .DefaultIfEmpty()
                                                  .Max();
            var usernameFieldWidth = maxUsernameLength > 4 ? maxUsernameLength + 2 : 6;
            var groupNameFieldWidth = maxGroupNameLength > 5 ? maxGroupNameLength + 2 : 7;
            var passwordFieldWidth = maxPasswordLength > 8 ? maxPasswordLength + 2 : 10;

            Utilities.PrintTableBorder(usernameFieldWidth, groupNameFieldWidth, 24, passwordFieldWidth);
            Console.WriteLine($"|{"User".PadRight(usernameFieldWidth)}|{"Group".PadRight(groupNameFieldWidth)}|{"Method",-24}|{"Password".PadRight(passwordFieldWidth)}|");
            Utilities.PrintTableBorder(usernameFieldWidth, groupNameFieldWidth, 24, passwordFieldWidth);

            foreach (var user in users.UserDict)
            {
                if (usernames.Length > 0 && !usernames.Contains(user.Key))
                    continue;

                foreach (var credEntry in user.Value.Credentials)
                {
                    if (groups.Length > 0 && !groups.Contains(credEntry.Key))
                        continue;

                    if (credEntry.Value == null)
                        Console.WriteLine($"|{user.Key.PadRight(usernameFieldWidth)}|{credEntry.Key.PadRight(groupNameFieldWidth)}|{string.Empty,-24}|{string.Empty.PadRight(passwordFieldWidth)}|");
                    else
                        Console.WriteLine($"|{user.Key.PadRight(usernameFieldWidth)}|{credEntry.Key.PadRight(groupNameFieldWidth)}|{credEntry.Value.Method,-24}|{credEntry.Value.Password.PadRight(passwordFieldWidth)}|");
                }
            }

            Utilities.PrintTableBorder(usernameFieldWidth, groupNameFieldWidth, 24, passwordFieldWidth);
        }

        public static async Task<int> GetSSLinks(string username, string[] groups)
        {
            var commandResult = 0;
            var users = await Users.LoadUsersAsync();
            using var nodes = await Nodes.LoadNodesAsync();

            var uris = users.GetUserSSUris(username, nodes, groups);
            if (uris != null)
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

        public static async Task<int> GetDataUsage(string username, SortBy? sortBy)
        {
            var users = await Users.LoadUsersAsync();
            using var nodes = await Nodes.LoadNodesAsync();
            var settings = await Settings.LoadSettingsAsync();

            var records = users.GetUserDataUsage(username, nodes);
            if (records == null)
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

            Utilities.PrintTableBorder(nameFieldWidth, 11, 16);

            Console.WriteLine($"|{"Group".PadRight(nameFieldWidth)}|{"Data Used",11}|{"Data Remaining",16}|");

            Utilities.PrintTableBorder(nameFieldWidth, 11, 16);

            foreach (var (group, bytesUsed, bytesRemaining) in records)
            {
                Console.Write($"|{group.PadRight(nameFieldWidth)}|{Utilities.HumanReadableDataString(bytesUsed),11}|");
                if (bytesRemaining != 0UL)
                    Console.WriteLine($"{Utilities.HumanReadableDataString(bytesRemaining),16}|");
                else
                    Console.WriteLine($"{string.Empty,16}|");
            }

            Utilities.PrintTableBorder(nameFieldWidth, 11, 16);

            return 0;
        }

        public static async Task<int> SetDataLimit(string dataLimit, string[] usernames, string[]? groups)
        {
            var commandResult = 0;
            var users = await Users.LoadUsersAsync();

            if (Utilities.TryParseDataLimitString(dataLimit, out var dataLimitInBytes))
            {
                foreach (var username in usernames)
                {
                    var result = users.SetDataLimitToUser(dataLimitInBytes, username, groups);
                    switch (result)
                    {
                        case 0:
                            Console.WriteLine($"Data limit set for {username}.");
                            break;
                        case -1:
                            Console.WriteLine($"Error: user {username} doesn't exist.");
                            break;
                        case -2:
                            Console.WriteLine($"An error occurred while setting for {username}: some groups were not found.");
                            break;
                        default:
                            Console.WriteLine($"Unknown error.");
                            break;
                    }
                    commandResult += result;
                }
            }
            else
            {
                Console.WriteLine($"An error occurred while parsing the data limit: {dataLimit}");
                commandResult = -3;
            }

            await Users.SaveUsersAsync(users);
            return commandResult;
        }
    }
}
