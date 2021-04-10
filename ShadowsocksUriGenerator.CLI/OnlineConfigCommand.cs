using ShadowsocksUriGenerator.CLI.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ShadowsocksUriGenerator.CLI
{
    public static class OnlineConfigCommand
    {
        public static async Task<int> Generate(string[] usernames, CancellationToken cancellationToken = default)
        {
            var users = await JsonHelper.LoadUsersAsync(cancellationToken);
            using var nodes = await JsonHelper.LoadNodesAsync(cancellationToken);
            var settings = await JsonHelper.LoadSettingsAsync(cancellationToken);

            var errMsg = await OnlineConfig.GenerateAndSave(users, nodes, settings, cancellationToken, usernames);
            if (errMsg is not null)
            {
                Console.WriteLine(errMsg);
                return 1;
            }

            return 0;
        }

        public static async Task<int> GetLinks(string[] usernames, CancellationToken cancellationToken = default)
        {
            var commandResult = 0;
            var users = await JsonHelper.LoadUsersAsync(cancellationToken);
            var settings = await JsonHelper.LoadSettingsAsync(cancellationToken);

            if (usernames.Length == 0)
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
                    foreach (var group in user.Memberships.Keys)
                        Console.WriteLine($"{settings.OnlineConfigDeliveryRootUri}/{user.Uuid}/{Uri.EscapeDataString(group)}.json");
                Console.WriteLine();
            }
        }

        public static async Task<int> Clean(string[] usernames, CancellationToken cancellationToken = default)
        {
            var users = await JsonHelper.LoadUsersAsync(cancellationToken);
            var settings = await JsonHelper.LoadSettingsAsync(cancellationToken);

            OnlineConfig.Remove(users, settings, usernames);

            return 0;
        }
    }
}
