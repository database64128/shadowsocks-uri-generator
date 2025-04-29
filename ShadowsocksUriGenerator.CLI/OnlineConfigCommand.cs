using ShadowsocksUriGenerator.Data;
using ShadowsocksUriGenerator.OnlineConfig;
using System.Text.Json;

namespace ShadowsocksUriGenerator.CLI
{
    public static class OnlineConfigCommand
    {
        public static async Task<int> Generate(string[] usernames, CancellationToken cancellationToken = default)
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

            var errMsg = await SIP008StaticGen.GenerateAndSave(users, nodes, settings, cancellationToken, usernames);
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

            var printApiLinks = !string.IsNullOrEmpty(settings.ApiServerBaseUrl) && !string.IsNullOrEmpty(settings.ApiServerSecretPath);
            var printStaticLinks = !string.IsNullOrEmpty(settings.OnlineConfigDeliveryRootUri);

            if (printApiLinks)
            {
                Console.WriteLine("=== Online Config API URLs and Tokens ===");
                Console.WriteLine();

                if (usernames.Length == 0)
                {
                    foreach (var userEntry in users.UserDict)
                    {
                        PrintUserApiLinks(userEntry.Key, userEntry.Value, settings);
                    }
                }
                else
                {
                    foreach (var username in usernames)
                    {
                        if (users.UserDict.TryGetValue(username, out User? user))
                        {
                            PrintUserApiLinks(username, user, settings);
                        }
                        else
                        {
                            Console.WriteLine($"Error: user {username} doesn't exist.");
                            commandResult -= 2;
                        }
                    }
                }
            }

            if (printStaticLinks)
            {
                Console.WriteLine("=== Online Config Static URLs ===");
                Console.WriteLine();

                if (usernames.Length == 0)
                {
                    foreach (var userEntry in users.UserDict)
                    {
                        PrintUserStaticLinks(userEntry.Key, userEntry.Value, settings);
                    }
                }
                else
                {
                    foreach (var username in usernames)
                    {
                        if (users.UserDict.TryGetValue(username, out User? user))
                        {
                            PrintUserStaticLinks(username, user, settings);
                        }
                        else
                        {
                            Console.WriteLine($"Error: user {username} doesn't exist.");
                            commandResult -= 2;
                        }
                    }
                }
            }

            Console.WriteLine("=== API Docs ===");
            Console.WriteLine();
            Console.WriteLine($"Swagger UI URL: {settings.ApiServerBaseUrl}/{settings.ApiServerSecretPath}/swagger/");
            Console.WriteLine($"ReDoc UI URL: {settings.ApiServerBaseUrl}/{settings.ApiServerSecretPath}/api-docs/");

            return commandResult;

            static void PrintUserStaticLinks(string username, User user, Settings settings)
            {
                Console.WriteLine($"User: {username}");

                Console.WriteLine($"{settings.OnlineConfigDeliveryRootUri}/{user.Uuid}.json");

                if (settings.OnlineConfigDeliverByGroup)
                {
                    foreach (var group in user.Memberships.Keys)
                    {
                        Console.WriteLine($"{settings.OnlineConfigDeliveryRootUri}/{user.Uuid}/{Uri.EscapeDataString(group)}.json");
                    }
                }

                Console.WriteLine();
            }

            static void PrintUserApiLinks(string username, User user, Settings settings)
            {
                Console.WriteLine($"User:                             {username}");

                OOCv1ApiToken oocv1ApiToken = new(1, settings.ApiServerBaseUrl, settings.ApiServerSecretPath, user.Uuid, null);
                string oocv1ApiTokenString = JsonSerializer.Serialize(oocv1ApiToken, OnlineConfigCamelCaseJsonSerializerContext.Default.OOCv1ApiToken);

                Console.WriteLine($"OOCv1 API Token:                  {oocv1ApiTokenString}");
                Console.WriteLine($"OOCv1 API URL:                    {settings.ApiServerBaseUrl}/{settings.ApiServerSecretPath}/ooc/v1/{user.Uuid}");
                Console.WriteLine($"Shadowsocks Go Client Config URL: {settings.ApiServerBaseUrl}/{settings.ApiServerSecretPath}/shadowsocks-go/clients/{user.Uuid}");
                Console.WriteLine($"Sing Box Outbound Config URL:     {settings.ApiServerBaseUrl}/{settings.ApiServerSecretPath}/sing-box/outbounds/{user.Uuid}");
                Console.WriteLine($"SIP008 Delivery URL:              {settings.ApiServerBaseUrl}/{settings.ApiServerSecretPath}/sip008/{user.Uuid}");

                Console.WriteLine();
            }
        }

        public static async Task<int> Clean(string[] usernames, bool _, CancellationToken cancellationToken = default)
        {
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

            SIP008StaticGen.Remove(users, settings, usernames);

            return 0;
        }
    }
}
