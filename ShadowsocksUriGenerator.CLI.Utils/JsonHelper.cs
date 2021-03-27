using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ShadowsocksUriGenerator.CLI.Utils
{
    public static class JsonHelper
    {
        public static async Task<T> LoadJsonAsync<T>(string filename, JsonSerializerOptions? jsonSerializerOptions = null, CancellationToken cancellationToken = default) where T : class, new()
        {
            var (jsonData, errMsg) = await Utilities.LoadJsonAsync<T>(filename, jsonSerializerOptions, cancellationToken);
            if (errMsg is not null)
            {
                Console.WriteLine(errMsg);
                Environment.Exit(1);
            }
            return jsonData;
        }

        public static async Task<Users> LoadUsersAsync(CancellationToken cancellationToken = default)
        {
            var (users, errMsg) = await Users.LoadUsersAsync(cancellationToken);
            if (errMsg is not null)
            {
                Console.WriteLine(errMsg);
                Environment.Exit(1);
            }
            return users;
        }

        public static async Task<Nodes> LoadNodesAsync(CancellationToken cancellationToken = default)
        {
            var (nodes, errMsg) = await Nodes.LoadNodesAsync(cancellationToken);
            if (errMsg is not null)
            {
                Console.WriteLine(errMsg);
                Environment.Exit(1);
            }
            return nodes;
        }

        public static async Task<Settings> LoadSettingsAsync(CancellationToken cancellationToken = default)
        {
            var (settings, errMsg) = await Settings.LoadSettingsAsync(cancellationToken);
            if (errMsg is not null)
            {
                Console.WriteLine(errMsg);
                Environment.Exit(1);
            }
            return settings;
        }

        public static async Task SaveJsonAsync<T>(string filename, T jsonData, JsonSerializerOptions? jsonSerializerOptions = null, CancellationToken cancellationToken = default)
        {
            var errMsg = await Utilities.SaveJsonAsync(filename, jsonData, jsonSerializerOptions, cancellationToken);
            if (errMsg is not null)
            {
                Console.WriteLine(errMsg);
            }
        }

        public static async Task SaveUsersAsync(Users users, CancellationToken cancellationToken = default)
        {
            var errMsg = await Users.SaveUsersAsync(users, cancellationToken);
            if (errMsg is not null)
            {
                Console.WriteLine(errMsg);
            }
        }

        public static async Task SaveNodesAsync(Nodes nodes, CancellationToken cancellationToken = default)
        {
            var errMsg = await Nodes.SaveNodesAsync(nodes, cancellationToken);
            if (errMsg is not null)
            {
                Console.WriteLine(errMsg);
            }
        }

        public static async Task SaveSettingsAsync(Settings settings, CancellationToken cancellationToken = default)
        {
            var errMsg = await Settings.SaveSettingsAsync(settings, cancellationToken);
            if (errMsg is not null)
            {
                Console.WriteLine(errMsg);
            }
        }
    }
}
