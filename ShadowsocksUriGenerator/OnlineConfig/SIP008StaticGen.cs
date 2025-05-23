﻿using ShadowsocksUriGenerator.Data;
using ShadowsocksUriGenerator.Utils;
using System.Text;

namespace ShadowsocksUriGenerator.OnlineConfig
{
    /// <summary>
    /// Legacy SIP008 online config static file generator.
    /// </summary>
    public static class SIP008StaticGen
    {
        /// <summary>
        /// Generates and saves SIP008 delivery files.
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
                    var onlineConfigDict = GenerateForUser(userEntry, users, nodes, settings);
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
                        var onlineConfigDict = GenerateForUser(new(username, user), users, nodes, settings);
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
        /// Generates SIP008 delivery JSON for the specified user.
        /// </summary>
        /// <param name="userEntry">The specified user entry.</param>
        /// <param name="users">The object storing all users.</param>
        /// <param name="nodes">The object storing all nodes.</param>
        /// <param name="settings">The object storing all settings.</param>
        /// <returns>The user's SIP008 configuration object.</returns>
        public static Dictionary<string, SIP008Config> GenerateForUser(KeyValuePair<string, User> userEntry, Users users, Nodes nodes, Settings settings)
        {
            var username = userEntry.Key;
            var user = userEntry.Value;

            var dataUsageRecords = user.GetDataUsage(username, nodes);

            var OnlineConfigDict = new Dictionary<string, SIP008Config>();

            var userOnlineConfig = new SIP008Config()
            {
                Username = username,
                Id = user.Uuid,
                BytesUsed = user.BytesUsed > 0UL ? user.BytesUsed : null,
                BytesRemaining = user.BytesRemaining > 0UL ? user.BytesRemaining : null,
            };

            var servers = new List<SIP008Server>();

            foreach (var membership in user.Memberships)
            {
                if (membership.Value.HasCredential && nodes.Groups.TryGetValue(membership.Key, out var group))
                {
                    // per-group delivery
                    var filteredDataUsageRecords = dataUsageRecords.Where(x => x.group == membership.Key);
                    var dataUsageRecord = filteredDataUsageRecords.Any() ? filteredDataUsageRecords.First() : new();

                    var perGroupOnlineConfig = new SIP008Config()
                    {
                        Username = username,
                        Id = user.Uuid,
                        BytesUsed = dataUsageRecord.bytesUsed > 0UL ? dataUsageRecord.bytesUsed : null,
                        BytesRemaining = dataUsageRecord.bytesRemaining > 0UL ? dataUsageRecord.bytesRemaining : null,
                    };

                    var perGroupServers = new List<SIP008Server>();

                    // add each node to the Servers list.
                    foreach (var nodeEntry in group.NodeDict)
                    {
                        if (nodeEntry.Value.Deactivated)
                            continue;

                        var ownerUuid = nodeEntry.Value.OwnerUuid;
                        var owner = ownerUuid is null
                            ? null
                            : users.TryGetUserById(ownerUuid, out var ownerEntry)
                            ? ownerEntry.Key
                            : null;

                        var tags = nodeEntry.Value.Tags.Count > 0
                            ? nodeEntry.Value.Tags
                            : null;

                        var server = new SIP008Server()
                        {
                            Id = nodeEntry.Value.Uuid,
                            Name = nodeEntry.Key,
                            Host = nodeEntry.Value.Host,
                            Port = nodeEntry.Value.Port,
                            Method = membership.Value.Method,
                            Password = membership.Value.PasswordForNode(nodeEntry.Value.IdentityPSKs),
                            PluginName = nodeEntry.Value.Plugin,
                            PluginVersion = nodeEntry.Value.PluginVersion,
                            PluginOptions = nodeEntry.Value.PluginOpts,
                            PluginArguments = nodeEntry.Value.PluginArguments,
                            Group = membership.Key,
                            Owner = owner,
                            Tags = tags,
                        };

                        servers.Add(server);

                        if (settings.OnlineConfigDeliverByGroup)
                            perGroupServers.Add(server);
                    }

                    // sort and add per-group online config to dictionary
                    if (settings.OnlineConfigDeliverByGroup)
                    {
                        perGroupOnlineConfig.Servers = perGroupServers;

                        if (settings.OnlineConfigSortByName)
                            perGroupOnlineConfig.Servers = perGroupOnlineConfig.Servers.OrderBy(server => server.Name);

                        OnlineConfigDict.Add($"{user.Uuid}/{membership.Key}", perGroupOnlineConfig);
                    }
                }
            }

            userOnlineConfig.Servers = servers;

            if (settings.OnlineConfigSortByName)
                userOnlineConfig.Servers = userOnlineConfig.Servers.OrderBy(server => server.Name);

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
        public static async Task<string?> SaveOutputAsync(Dictionary<string, SIP008Config> onlineConfigDict, Settings settings, CancellationToken cancellationToken = default)
        {
            var errMsgSB = new StringBuilder();
            foreach (var x in onlineConfigDict)
            {
                var errMsg = await FileHelper.SaveJsonAsync(
                    $"{settings.OnlineConfigOutputDirectory}/{x.Key}.json",
                    x.Value,
                    OnlineConfigSnakeCaseJsonSerializerContext.Default.SIP008Config,
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
            if (!Directory.Exists(directory))
                return;

            foreach (var uuid in userUuids)
            {
                var path = $"{directory}/{uuid}";
                if (Directory.Exists(path))
                    Directory.Delete(path, true);
                File.Delete($"{path}.json");
            }
        }
    }
}
