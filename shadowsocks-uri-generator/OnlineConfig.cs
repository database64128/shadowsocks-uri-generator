using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace shadowsocks_uri_generator
{
    /// <summary>
    /// The class for online configuration delivery (SIP008).
    /// </summary>
    public class OnlineConfig
    {
        public int Version { get; set; }
        public string Username { get; set; }
        public string UserUuid { get; set; }
        public List<Server> Servers { get; set; }

        public OnlineConfig()
        {
            Version = 1;
            Username = "";
            UserUuid = Guid.NewGuid().ToString();
            Servers = new List<Server>();
        }

        public OnlineConfig(string username, string userUuid)
        {
            Version = 1;
            Username = username;
            UserUuid = userUuid;
            Servers = new List<Server>();
        }

        /// <summary>
        /// Generate and save SIP008 online configuration delivery files.
        /// </summary>
        /// <param name="users">The object storing all users.</param>
        /// <param name="nodes">The object storing all nodes.</param>
        /// <param name="settings">The object storing all settings.</param>
        /// <param name="username">Defaults to null for all users. Specify a user name to only generate for the user.</param>
        /// <returns>0 for success. 404 for user not found.</returns>
        public static async Task<int> GenerateAndSave(Users users, Nodes nodes, Settings settings, string? username = null)
        {
            if (string.IsNullOrEmpty(username)) // generate for all users
            {
                foreach (var userEntry in users.UserDict)
                {
                    var onlineConfig = Generate(userEntry, nodes);
                    await SaveOutputAsync(onlineConfig, settings);
                }
            }
            else // generate only for the specified user
            {
                if (users.UserDict.TryGetValue(username, out User? user))
                {
                    var onlineConfig = Generate(new KeyValuePair<string, User>(username, user), nodes);
                    await SaveOutputAsync(onlineConfig, settings);
                }
                else
                    return 404;
            }
            return 0;
        }

        /// <summary>
        /// Generate SIP008 user configuration JSON for the specified user.
        /// </summary>
        /// <param name="userEntry">The specified user entry.</param>
        /// <param name="nodes">The object storing all nodes.</param>
        /// <returns>The object of the user's SIP008 configuration.</returns>
        public static OnlineConfig Generate(KeyValuePair<string, User> userEntry, Nodes nodes)
        {
            var user = userEntry.Value;
            var onlineConfig = new OnlineConfig(userEntry.Key, user.Uuid);
            foreach (var credEntry in user.Credentials)
            {
                if (nodes.Groups.TryGetValue(credEntry.Key, out Group? group)) // find credEntry's group
                {
                    // add each node to the Servers list.
                    foreach (var node in group.NodeDict)
                    {
                        var server = new Server(
                            node.Key,
                            node.Value.Uuid,
                            node.Value.Host,
                            node.Value.Port,
                            credEntry.Value.Password,
                            credEntry.Value.Method);
                        onlineConfig.Servers.Add(server);
                    }
                }
                else
                    continue; // ignoring is intentional, as groups may get removed.
            }
            return onlineConfig;
        }

        /// <summary>
        /// Save the generated user configuration to a JSON file.
        /// </summary>
        /// <param name="onlineConfig">The generated user configuration object.</param>
        /// <param name="settings">The object storing all settings.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        public static async Task SaveOutputAsync(OnlineConfig onlineConfig, Settings settings)
        {
            var jsonSerializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = new JsonSnakeCaseNamingPolicy()
            };
            try
            {
                Directory.CreateDirectory(settings.OnlineConfigOutputDirectory);
                await Utilities.SaveJsonAsync(
                    $"{settings.OnlineConfigOutputDirectory}/{onlineConfig.UserUuid}.json",
                    onlineConfig,
                    jsonSerializerOptions);
            }
            catch
            {
                Console.WriteLine($"Error: failed to create {settings.OnlineConfigOutputDirectory}.");
            }
        }
    }

    /// <summary>
    /// The class for a server in the Servers list.
    /// </summary>
    public class Server
    {
        [JsonPropertyName("server")]
        public string Host { get; set; }
        [JsonPropertyName("server_port")]
        public int Port { get; set; }
        public string Password { get; set; }
        public string Method { get; set; }
        public string Plugin { get; set; }
        public string PluginOpts { get; set; }
        [JsonPropertyName("remarks")]
        public string Name { get; set; }
        [JsonPropertyName("id")]
        public string Uuid { get; set; }

        public Server()
        {
            Host = "";
            Password = "";
            Method = "";
            Plugin = "";
            PluginOpts = "";
            Name = "";
            Uuid = "";
        }

        public Server(
            string name,
            string uuid,
            string host,
            int port,
            string password,
            string method,
            string plugin = "",
            string pluginOpts = "")
        {
            Host = host;
            Port = port;
            Password = password;
            Method = method;
            Plugin = plugin;
            PluginOpts = pluginOpts;
            Name = name;
            Uuid = uuid;
        }
    }
}
