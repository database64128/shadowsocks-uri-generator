using System;
using System.Threading.Tasks;

namespace ShadowsocksUriGenerator.CLI
{
    public static class OnlineConfigCommand
    {
        public static async Task<int> Generate(string[]? usernames)
        {
            var users = await Users.LoadUsersAsync();
            using var nodes = await Nodes.LoadNodesAsync();
            var settings = await Settings.LoadSettingsAsync();

            int result;
            if (usernames == null)
                result = await OnlineConfig.GenerateAndSave(users, nodes, settings);
            else
                result = await OnlineConfig.GenerateAndSave(users, nodes, settings, usernames);
            if (result == 404)
                Console.WriteLine($"One or more specified users are not found.");

            return result;
        }

        public static async Task<int> GetLinks(string[]? usernames)
        {
            var commandResult = 0;
            var users = await Users.LoadUsersAsync();
            var settings = await Settings.LoadSettingsAsync();

            if (usernames == null)
            {
                foreach (var userEntry in users.UserDict)
                {
                    var username = userEntry.Key;
                    var user = userEntry.Value;
                    PrintUserLinks(username, user, settings);
                }
            }
            else
            {
                foreach (var username in usernames)
                {
                    if (users.UserDict.TryGetValue(username, out User? user))
                        PrintUserLinks(username, user, settings);
                    else
                    {
                        Console.WriteLine($"Error: user {username} doesn't exist.");
                        commandResult -= 2;
                    }
                }
            }

            return commandResult;

            static void PrintUserLinks(string username, User user, Settings settings)
            {
                Console.WriteLine($"{"User",-8}{username,-32}");
                Console.WriteLine();
                Console.WriteLine($"{settings.OnlineConfigDeliveryRootUri}/{user.Uuid}.json");
                if (settings.OnlineConfigDeliverByGroup)
                    foreach (var group in user.Credentials.Keys)
                        Console.WriteLine($"{settings.OnlineConfigDeliveryRootUri}/{user.Uuid}/{Uri.EscapeDataString(group)}.json");
                Console.WriteLine();
            }
        }

        public static async Task<int> Clean(string[]? usernames, bool all)
        {
            var users = await Users.LoadUsersAsync();
            var settings = await Settings.LoadSettingsAsync();

            if (usernames != null && !all)
                OnlineConfig.Remove(users, settings, usernames);
            else if (usernames == null && all)
                OnlineConfig.Remove(users, settings);
            else
            {
                Console.WriteLine("Invalid arguments or options. Either specify usernames, or use '--all' to target all users.");
                return -3;
            }

            return 0;
        }
    }
}
