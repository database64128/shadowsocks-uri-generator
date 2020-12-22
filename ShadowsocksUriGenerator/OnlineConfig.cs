﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ShadowsocksUriGenerator
{
    /// <summary>
    /// The class for online configuration delivery (SIP008).
    /// </summary>
    public class OnlineConfig
    {
        public int Version { get; set; }
        public string Username { get; set; }
        public string UserUuid { get; set; }
        public ulong BytesUsed { get; set; }
        public ulong BytesRemaining { get; set; }
        public List<Server> Servers { get; set; }

        public OnlineConfig()
        {
            Version = 1;
            Username = "";
            UserUuid = Guid.NewGuid().ToString();
            Servers = new();
        }

        public OnlineConfig(string username, string userUuid, ulong bytesUsed = 0, ulong bytesRemaining = 0)
        {
            Version = 1;
            Username = username;
            UserUuid = userUuid;
            BytesUsed = bytesUsed;
            BytesRemaining = bytesRemaining;
            Servers = new();
        }

        /// <summary>
        /// Generates and saves SIP008 online configuration delivery files.
        /// </summary>
        /// <param name="users">The object storing all users.</param>
        /// <param name="nodes">The object storing all nodes.</param>
        /// <param name="settings">The object storing all settings.</param>
        /// <param name="usernames">The specified users to generate for. Pass nothing to generate for all users.</param>
        /// <returns>0 for success. 404 for user not found.</returns>
        public static async Task<int> GenerateAndSave(Users users, Nodes nodes, Settings settings, params string[] usernames)
        {
            if (usernames.Length == 0) // generate for all users
                foreach (var userEntry in users.UserDict)
                {
                    var onlineConfig = Generate(userEntry, nodes, settings);
                    await SaveOutputAsync(onlineConfig, settings);
                }
            else // generate only for the specified user
                foreach (var username in usernames)
                    if (users.UserDict.TryGetValue(username, out User? user))
                    {
                        var onlineConfig = Generate(new(username, user), nodes, settings);
                        await SaveOutputAsync(onlineConfig, settings);
                    }
                    else
                        return 404;
            return 0;
        }

        /// <summary>
        /// Generates SIP008 user configuration JSON for the specified user.
        /// </summary>
        /// <param name="userEntry">The specified user entry.</param>
        /// <param name="nodes">The object storing all nodes.</param>
        /// <returns>The object of the user's SIP008 configuration.</returns>
        public static OnlineConfig Generate(KeyValuePair<string, User> userEntry, Nodes nodes, Settings settings)
        {
            var username = userEntry.Key;
            var user = userEntry.Value;
            user.CalculateTotalDataUsage(username, nodes);
            var onlineConfig = new OnlineConfig(username, user.Uuid, user.BytesUsed, user.BytesRemaining);
            foreach (var credEntry in user.Credentials)
            {
                if (credEntry.Value == null)
                    continue;
                
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
                            credEntry.Value.Method,
                            node.Value.Plugin,
                            node.Value.PluginOpts);
                        onlineConfig.Servers.Add(server);
                    }
                }
                else
                    continue; // ignoring is intentional, as groups may get removed.
            }
            // Sort by server name if `OnlineConfigSortByName` is true.
            if (settings.OnlineConfigSortByName)
                onlineConfig.Servers = onlineConfig.Servers.OrderBy(server => server.Name).ToList();
            return onlineConfig;
        }

        /// <summary>
        /// Saves the generated user configuration to a JSON file.
        /// </summary>
        /// <param name="onlineConfig">The generated user configuration object.</param>
        /// <param name="settings">The object storing all settings.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        public static async Task SaveOutputAsync(OnlineConfig onlineConfig, Settings settings)
        {
            try
            {
                Directory.CreateDirectory(settings.OnlineConfigOutputDirectory);
                await Utilities.SaveJsonAsync(
                    $"{settings.OnlineConfigOutputDirectory}/{onlineConfig.UserUuid}.json",
                    onlineConfig,
                    Utilities.snakeCaseJsonSerializerOptions);
            }
            catch
            {
                Console.WriteLine($"Error: failed to create {settings.OnlineConfigOutputDirectory}.");
            }
        }

        /// <summary>
        /// Removes the generated JSON files for all users or the specified users.
        /// </summary>
        /// <param name="users">The object storing all users.</param>
        /// <param name="nodes">The object storing all nodes.</param>
        /// <param name="settings">The object storing all settings.</param>
        /// <param name="usernames">The specified users. Pass nothing to remove for all users.</param>
        public static void Remove(Users users, Settings settings, params string[] usernames)
        {
            var directory = settings.OnlineConfigOutputDirectory;
            if (usernames.Length == 0)
            {
                var userUuids = users.UserDict.Select(x => x.Value.Uuid).ToArray();
                Remove(directory, userUuids);
            }
            else
                foreach (var username in usernames)
                    if (users.UserDict.TryGetValue(username, out var user))
                        Remove(directory, user.Uuid);
        }

        /// <summary>
        /// Removes online config files of the users in the list.
        /// </summary>
        /// <param name="directory">The online config directory.</param>
        /// <param name="userUuids">The list of users whose online config file will be removed.</param>
        public static void Remove(string directory, params string[] userUuids)
        {
            if (Directory.Exists(directory))
                foreach (var uuid in userUuids)
                    File.Delete($"{directory}/{uuid}.json");
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
        public string? Plugin { get; set; }
        public string? PluginOpts { get; set; }
        [JsonPropertyName("remarks")]
        public string Name { get; set; }
        [JsonPropertyName("id")]
        public string Uuid { get; set; }

        public Server()
        {
            Host = "";
            Port = 0;
            Password = "";
            Method = "";
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
            string? plugin = null,
            string? pluginOpts = null)
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
