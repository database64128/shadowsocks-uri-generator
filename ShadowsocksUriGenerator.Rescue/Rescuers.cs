﻿using ShadowsocksUriGenerator.OnlineConfig;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ShadowsocksUriGenerator.Rescue
{
    /// <summary>
    /// Class for static methods that
    /// extract config information from generated files and
    /// reconstruct config data.
    /// </summary>
    public static class Rescuers
    {
        /// <summary>
        /// Extracts config information from generated online config directory.
        /// </summary>
        /// <param name="onlineConfigDir">Path to online config directory.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the read operation.</param>
        /// <returns>
        /// A ValueTuple containing <see cref="Users"/> and <see cref="Nodes"/> config data
        /// and an error message.
        /// The error message is null if no errors occurred.
        /// </returns>
        public static async Task<(Users? users, Nodes? nodes, string? errMsg)> FromOnlineConfig(string onlineConfigDir, CancellationToken cancellationToken = default)
        {
            onlineConfigDir = Utilities.GetAbsolutePath(onlineConfigDir);

            try
            {
                var onlineConfigDirInfo = new DirectoryInfo(onlineConfigDir);

                if (!onlineConfigDirInfo.Exists)
                    return (null, null, $"Error: online config directory `{onlineConfigDir}` doesn't exist.");

                var userDirs = onlineConfigDirInfo.GetDirectories();
                var userJsons = onlineConfigDirInfo.GetFiles();

                return userDirs.Length switch
                {
                    > 0 => await FromGroupedOnlineConfig(userDirs, cancellationToken),
                    0 when userJsons.Length > 0 => await FromUngroupedOnlineConfig(userJsons, cancellationToken),
                    _ => (null, null, $"Error: online config directory `{onlineConfigDir}` is empty.")
                };
            }
            catch (Exception ex)
            {
                return (null, null, ex.Message);
            }
        }

        /// <summary>
        /// Extracts config information from online config directory
        /// that is generated by group.
        /// </summary>
        /// <param name="userDirs">An array of <see cref="DirectoryInfo"/> objects that corresponds to each user.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the read operation.</param>
        /// <returns>
        /// A ValueTuple containing <see cref="Users"/> and <see cref="Nodes"/> config data
        /// and an error message.
        /// The error message is null if no errors occurred.
        /// </returns>
        private static async Task<(Users users, Nodes nodes, string? errMsg)> FromGroupedOnlineConfig(DirectoryInfo[] userDirs, CancellationToken cancellationToken = default)
        {
            var users = new Users();
            var nodes = new Nodes();

            // iterate through each user
            foreach (var userDir in userDirs)
            {
                // filter out invalid user UUID
                if (!Guid.TryParse(userDir.Name, out var userUuid))
                    continue;

                // create user
                string? username = null;
                var user = new User
                {
                    Uuid = userUuid.ToString(),
                };

                // iterate through user creds
                var userCredJsons = userDir.GetFiles();
                foreach (var userCredJson in userCredJsons)
                {
                    // skip irrelevant files
                    if (userCredJson.Name.Length < 6 || !userCredJson.Name.EndsWith(".json"))
                        continue;

                    // retrieve group credential
                    using var userCredJsonFS = userCredJson.OpenRead();
                    var userCred = await JsonSerializer.DeserializeAsync<SIP008Config>(userCredJsonFS, Utilities.snakeCaseJsonSerializerOptions, cancellationToken);
                    if (userCred is null)
                        continue;

                    // if group doesn't exist, add nodes. Otherwise, do nothing.
                    var groupName = userCredJson.Name[0..^5];
                    if (!nodes.Groups.ContainsKey(groupName))
                    {
                        var group = new Group();
                        foreach (var server in userCred.Servers)
                        {
                            var node = new Node()
                            {
                                Uuid = server.Id,
                                Host = server.Host,
                                Port = server.Port,
                                Plugin = server.PluginName,
                                PluginVersion = server.PluginVersion,
                                PluginOpts = server.PluginOptions,
                                PluginArguments = server.PluginArguments,
                            };
                            group.NodeDict.Add(server.Name, node);
                        }
                        // save group
                        nodes.Groups.Add(groupName, group);
                    }

                    // save user info and creds
                    username ??= userCred.Username;
                    user.BytesUsed += userCred.BytesUsed ?? 0UL;
                    user.BytesRemaining += userCred.BytesRemaining ?? 0UL;
                    var credSource = userCred.Servers.Any() ? userCred.Servers.First() : null;
                    if (credSource is not null)
                        _ = user.AddCredential(groupName, credSource.Method, credSource.Password);
                }

                // save user
                username ??= user.GetHashCode().ToString();
                users.UserDict.Add(username, user);
            }

            return (users, nodes, null);
        }

        /// <summary>
        /// Extracts config information from online config directory
        /// where each user has one corresponding JSON file.
        /// </summary>
        /// <param name="userJsons">An array of <see cref="FileInfo"/> objects that corresponds to each user.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the read operation.</param>
        /// <returns>
        /// A ValueTuple containing <see cref="Users"/> and <see cref="Nodes"/> config data
        /// and an error message.
        /// The error message is null if no errors occurred.
        /// </returns>
        private static async Task<(Users users, Nodes nodes, string? errMsg)> FromUngroupedOnlineConfig(FileInfo[] userJsons, CancellationToken cancellationToken = default)
        {
            var users = new Users();
            var nodes = new Nodes();

            foreach (var userJson in userJsons)
            {
                // skip irrelevant files
                if (userJson.Name.Length < 6 || !userJson.Name.EndsWith(".json"))
                    continue;

                // retrieve user credentials
                using var userJsonFS = userJson.OpenRead();
                var userCreds = await JsonSerializer.DeserializeAsync<SIP008Config>(userJsonFS, Utilities.snakeCaseJsonSerializerOptions, cancellationToken);
                if (userCreds is null)
                    continue;

                // save user info
                var username = userCreds.Username ?? Guid.NewGuid().ToString();
                var user = new User()
                {
                    Uuid = userCreds.Id ?? Guid.NewGuid().ToString(),
                    BytesUsed = userCreds.BytesUsed ?? 0UL,
                    BytesRemaining = userCreds.BytesRemaining ?? 0UL,
                };

                // group servers by credential
                var groupedServers = userCreds.Servers.GroupBy(server => new MemberInfo(server.Method, server.Password));

                // find belonging group by node name
                foreach (var credGroup in groupedServers)
                {
                    // pick the first node to represent the group
                    var nodename = credGroup.First().Name;

                    // search for nodename in existing nodes
                    var matchedGroups = nodes.Groups.Where(x => x.Value.NodeDict.ContainsKey(nodename));
                    string matchedGroup;

                    // if no matched groups, create group and add nodes. Otherwise, do nothing.
                    if (!matchedGroups.Any())
                    {
                        matchedGroup = credGroup.Key.GetHashCode().ToString();
                        var group = new Group();
                        foreach (var server in credGroup)
                        {
                            var node = new Node()
                            {
                                Uuid = server.Id,
                                Host = server.Host,
                                Port = server.Port,
                                Plugin = server.PluginName,
                                PluginVersion = server.PluginVersion,
                                PluginOpts = server.PluginOptions,
                                PluginArguments = server.PluginArguments,
                            };
                            group.NodeDict.Add(server.Name, node);
                        }
                        // save group
                        nodes.Groups.Add(matchedGroup, group);
                    }
                    else
                    {
                        matchedGroup = matchedGroups.First().Key;
                    }

                    // save credential
                    user.Memberships.Add(matchedGroup, credGroup.Key);
                }

                // save user
                users.UserDict.Add(username, user);
            }

            return (users, nodes, null);
        }
    }
}
