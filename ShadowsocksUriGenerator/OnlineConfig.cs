using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
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
        /// <param name="cancellationToken">A token that may be used to cancel the write operation.</param>
        /// <param name="usernames">The specified users to generate for. Pass nothing to generate for all users.</param>
        /// <returns>An error message. Null if no errors occurred.</returns>
        public static async Task<string?> GenerateAndSave(Users users, Nodes nodes, Settings settings, CancellationToken cancellationToken = default, params string[] usernames)
        {
            var errMsgSB = new StringBuilder();
            if (usernames.Length == 0) // generate for all users
            {
                foreach (var userEntry in users.UserDict)
                {
                    var onlineConfigDict = GenerateForUser(userEntry, nodes, settings);
                    var errMsg = await SaveOutputAsync(onlineConfigDict, settings, cancellationToken);
                    if (errMsg is not null)
                        errMsgSB.AppendLine(errMsg);
                }
            }
            else // generate only for the specified user
            {
                foreach (var username in usernames)
                {
                    if (users.UserDict.TryGetValue(username, out User? user))
                    {
                        var onlineConfigDict = GenerateForUser(new(username, user), nodes, settings);
                        var errMsg = await SaveOutputAsync(onlineConfigDict, settings, cancellationToken);
                        if (errMsg is not null)
                            errMsgSB.AppendLine(errMsg);
                    }
                    else
                    {
                        errMsgSB.AppendLine($"Error: user {username} doesn't exist.");
                    }
                }
            }
            if (errMsgSB.Length > 0)
                return errMsgSB.ToString();
            else
                return null;
        }

        /// <summary>
        /// Generates SIP008 user configuration JSON for the specified user.
        /// </summary>
        /// <param name="userEntry">The specified user entry.</param>
        /// <param name="nodes">The object storing all nodes.</param>
        /// <returns>The object of the user's SIP008 configuration.</returns>
        public static Dictionary<string, OnlineConfig> GenerateForUser(KeyValuePair<string, User> userEntry, Nodes nodes, Settings settings)
        {
            var username = userEntry.Key;
            var user = userEntry.Value;
            var dataUsageRecords = user.GetDataUsage(username, nodes);
            var OnlineConfigDict = new Dictionary<string, OnlineConfig>();
            var userOnlineConfig = new OnlineConfig(username, user.Uuid, user.BytesUsed, user.BytesRemaining);
            foreach (var membership in user.Memberships)
            {
                if (!membership.Value.HasCredential)
                    continue;

                if (nodes.Groups.TryGetValue(membership.Key, out var group)) // find credEntry's group
                {
                    // per-group delivery
                    var filteredDataUsageRecords = dataUsageRecords.Where(x => x.group == membership.Key);
                    var dataUsageRecord = filteredDataUsageRecords.Any() ? filteredDataUsageRecords.First() : new();
                    var perGroupOnlineConfig = new OnlineConfig(username, user.Uuid, dataUsageRecord.bytesUsed, dataUsageRecord.bytesRemaining);

                    // add each node to the Servers list.
                    foreach (var node in group.NodeDict)
                    {
                        if (node.Value.Deactivated)
                            continue;

                        var server = new Server(
                            node.Key,
                            node.Value.Uuid,
                            node.Value.Host,
                            node.Value.Port,
                            membership.Value.Password,
                            membership.Value.Method,
                            node.Value.Plugin,
                            node.Value.PluginOpts);

                        userOnlineConfig.Servers.Add(server);

                        if (settings.OnlineConfigDeliverByGroup)
                            perGroupOnlineConfig.Servers.Add(server);
                    }
                    // sort and add per-group online config to dictionary
                    if (settings.OnlineConfigDeliverByGroup)
                    {
                        if (settings.OnlineConfigSortByName)
                            perGroupOnlineConfig.Servers = perGroupOnlineConfig.Servers.OrderBy(server => server.Name).ToList();

                        OnlineConfigDict.Add($"{user.Uuid}/{membership.Key}", perGroupOnlineConfig);
                    }
                }
                else
                    continue; // ignoring is intentional, as groups may get removed.
            }
            // sort and add
            if (settings.OnlineConfigSortByName)
                userOnlineConfig.Servers = userOnlineConfig.Servers.OrderBy(server => server.Name).ToList();

            OnlineConfigDict.Add(user.Uuid, userOnlineConfig);

            return OnlineConfigDict;
        }

        /// <summary>
        /// Saves the generated user configuration to a JSON file.
        /// </summary>
        /// <param name="onlineConfigDict">Username-OnlineConfig pairs.</param>
        /// <param name="settings">The object storing all settings.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the write operation.</param>
        /// <returns>An error message. Null if no errors occurred.</returns>
        public static async Task<string?> SaveOutputAsync(Dictionary<string, OnlineConfig> onlineConfigDict, Settings settings, CancellationToken cancellationToken = default)
        {
            var errMsgSB = new StringBuilder();
            foreach (var x in onlineConfigDict)
            {
                var errMsg = await Utilities.SaveJsonAsync(
                    $"{settings.OnlineConfigOutputDirectory}/{x.Key}.json",
                    x.Value,
                    Utilities.snakeCaseJsonSerializerOptions,
                    false,
                    true,
                    cancellationToken);
                if (errMsg is not null)
                    errMsgSB.AppendLine(errMsg);
            }
            if (errMsgSB.Length > 0)
                return errMsgSB.ToString();
            else
                return null;
        }

        /// <summary>
        /// Removes the generated JSON files for all users or the specified users.
        /// </summary>
        /// <param name="users">The object storing all users.</param>
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
            directory = Utilities.GetAbsolutePath(directory);
            if (Directory.Exists(directory))
                foreach (var uuid in userUuids)
                {
                    var path = $"{directory}/{uuid}";
                    if (Directory.Exists(path))
                        Directory.Delete(path, true);
                    File.Delete($"{path}.json");
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
