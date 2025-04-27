using ShadowsocksUriGenerator.Outline;
using ShadowsocksUriGenerator.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ShadowsocksUriGenerator.Data
{
    /// <summary>
    /// The class for storing node information in Nodes.json
    /// </summary>
    public class Nodes : IDisposable
    {
        private bool disposedValue;
        private readonly HttpClient _httpClient = new();

        /// <summary>
        /// Gets the default configuration version
        /// used by this version of the app.
        /// </summary>
        public static int DefaultVersion => 2;

        /// <summary>
        /// Gets or sets the configuration version number.
        /// 0 for the legacy config version
        /// without a version number property.
        /// Newer config versions start from 1.
        /// Update if older config is present.
        /// Throw error if config is newer than supported.
        /// </summary>
        public int Version { get; set; } = DefaultVersion;

        /// <summary>
        /// Gets or sets the group dictionary.
        /// key is group name.
        /// value is group info.
        /// </summary>
        public Dictionary<string, Group> Groups { get; set; } = [];

        /// <summary>
        /// Adds a new node group to the group dictionary.
        /// </summary>
        /// <param name="groupName">The group to add.</param>
        /// <param name="ownerUuid">Optional. User UUID of the group owner.</param>
        /// <param name="ssmv1BaseUri">Optional. SSMv1 API base URI.</param>
        /// <param name="ssmv1ServerMethod">Optional. SSMv1 server method.</param>
        /// <returns>
        /// 0 for success.
        /// 1 when a group with the same name already exists.
        /// </returns>
        public int AddGroup(
            string groupName,
            string? ownerUuid = null,
            Uri? ssmv1BaseUri = null,
            string? ssmv1ServerMethod = null)
        {
            if (!Groups.ContainsKey(groupName))
            {
                Group group = new()
                {
                    OwnerUuid = ownerUuid,
                };

                if (ssmv1BaseUri is not null)
                {
                    group.SSMv1Server = new()
                    {
                        BaseUri = ssmv1BaseUri,
                    };

                    if (ssmv1ServerMethod is not null)
                    {
                        group.SSMv1Server.ServerMethod = ssmv1ServerMethod;
                    }
                }

                Groups.Add(groupName, group);
                return 0;
            }
            else
            {
                return 1;
            }
        }

        /// <summary>
        /// Renames an existing group with a new name.
        /// </summary>
        /// <param name="oldName">The existing group name.</param>
        /// <param name="newName">The new group name.</param>
        /// <returns>
        /// 0 when success.
        /// -1 when the old group is not found.
        /// -2 when a group with the same name already exists.
        /// </returns>
        public int RenameGroup(string oldName, string newName)
        {
            if (Groups.ContainsKey(newName))
                return -2;
            if (!Groups.Remove(oldName, out var group))
                return -1;
            Groups.Add(newName, group);
            return 0;
        }

        /// <summary>
        /// Removes the group from storage.
        /// </summary>
        /// <param name="group">The group to be removed.</param>
        /// <returns>
        /// True if the group is successfully found and removed.
        /// Otherwise, false.
        /// </returns>
        public bool RemoveGroup(string group) => Groups.Remove(group);

        /// <summary>
        /// Adds a node to a node group.
        /// </summary>
        /// <param name="group">Destination group name.</param>
        /// <param name="node">Node name</param>
        /// <param name="host">Node's host</param>
        /// <param name="port">Node's port number.</param>
        /// <param name="plugin">Optional. Plugin binary name.</param>
        /// <param name="pluginVersion">Optional. Required plugin version.</param>
        /// <param name="pluginOpts">Optional. Plugin options.</param>
        /// <param name="pluginArguments">Optional. Plugin startup arguments.</param>
        /// <param name="ownerUuid">Optional. Node owner's user UUID.</param>
        /// <param name="tags">Node's tags.</param>
        /// <param name="iPSKs">Node's identity PSKs.</param>
        /// <returns>
        /// 0 if success.
        /// -1 if a node with the same name already exists.
        /// -2 if the group doesn't exist.
        /// </returns>
        public int AddNodeToGroup(string group, string node, string host, int port, string? plugin, string? pluginVersion, string? pluginOpts, string? pluginArguments, string? ownerUuid, string[] tags, string[] iPSKs)
        {
            if (Groups.TryGetValue(group, out var targetGroup))
            {
                return targetGroup.AddNode(node, host, port, plugin, pluginVersion, pluginOpts, pluginArguments, ownerUuid, tags, iPSKs);
            }
            else
            {
                return -2;
            }
        }

        /// <summary>
        /// Renames an existing node with a new name.
        /// </summary>
        /// <param name="group">The node group which contains the node.</param>
        /// <param name="oldName">The existing node name.</param>
        /// <param name="newName">The new node name.</param>
        /// <returns>
        /// 0 when success.
        /// -1 when old node name is not found.
        /// -2 when new node name already exists.
        /// -3 when the group is not found.
        /// </returns>
        public int RenameNodeInGroup(string group, string oldName, string newName)
        {
            if (Groups.TryGetValue(group, out var targetGroup))
                return targetGroup.RenameNode(oldName, newName);
            else
                return -3;
        }

        /// <summary>
        /// Removes the node from the node group.
        /// </summary>
        /// <param name="group">Group to remove nodes from.</param>
        /// <param name="node">Node name to be removed.</param>
        /// <returns>
        /// 0 if the node is successfully found and removed.
        /// -1 if the node doesn't exist.
        /// -2 if the group doesn't exist.
        /// </returns>
        public int RemoveNodeFromGroup(string group, string node)
        {
            if (Groups.TryGetValue(group, out var targetGroup))
                return targetGroup.RemoveNode(node) ? 0 : -1;
            else
                return -2;
        }

        /// <summary>
        /// Activates the node in the group.
        /// </summary>
        /// <param name="group">Group name.</param>
        /// <param name="node">Node name.</param>
        /// <returns>
        /// 0 if successfully activated the node.
        /// 1 if already activated.
        /// -1 if node not found.
        /// -2 if group not found.
        /// </returns>
        public int ActivateNodeInGroup(string group, string node)
        {
            if (Groups.TryGetValue(group, out var targetGroup))
                return targetGroup.ActivateNode(node);
            else
                return -2;
        }

        /// <summary>
        /// Activates all nodes in the group.
        /// </summary>
        /// <param name="group">Group name.</param>
        /// <returns>
        /// 0 when success.
        /// -2 if group is not found.
        /// </returns>
        public int ActivateAllNodesInGroup(string group)
        {
            if (Groups.TryGetValue(group, out var targetGroup))
            {
                targetGroup.ActivateAllNodes();
                return 0;
            }
            else
                return -2;
        }

        /// <summary>
        /// Deactivates the node in the group.
        /// </summary>
        /// <param name="group">Group name.</param>
        /// <param name="node">Node name.</param>
        /// <returns>
        /// 0 if successfully deactivated the node.
        /// 1 if already deactivated.
        /// -1 if node not found.
        /// -2 if group not found.
        /// </returns>
        public int DeactivateNodeInGroup(string group, string node)
        {
            if (Groups.TryGetValue(group, out var targetGroup))
                return targetGroup.DeactivateNode(node);
            else
                return -2;
        }

        /// <summary>
        /// Deactivates all nodes in the group.
        /// </summary>
        /// <param name="group">Group name.</param>
        /// <returns>
        /// 0 when success.
        /// -2 if group is not found.
        /// </returns>
        public int DeactivateAllNodesInGroup(string group)
        {
            if (Groups.TryGetValue(group, out var targetGroup))
            {
                targetGroup.DeactivateAllNodes();
                return 0;
            }
            else
                return -2;
        }

        /// <summary>
        /// Gets all data usage records of the group.
        /// </summary>
        /// <param name="group">Target group.</param>
        /// <param name="users"> The <see cref="Users"/> object.</param>
        /// <returns>
        /// A list of data usage records as tuples.
        /// Null if the group doesn't exist.
        /// </returns>
        public List<(string username, ulong bytesUsed, ulong bytesRemaining)>? GetGroupDataUsage(string group, Users users)
        {
            if (Groups.TryGetValue(group, out var targetGroup))
                return targetGroup.GetDataUsage(group, users);
            else
                return null;
        }

        /// <summary>
        /// Gets data usage records that contain
        /// each group's total data usage.
        /// </summary>
        /// <returns>A sequence of data usage records as tuples.</returns>
        public IEnumerable<(string group, ulong bytesUsed, ulong bytesRemaining)> GetDataUsageByGroup()
            => Groups.Select(groupEntry => (groupEntry.Key, groupEntry.Value.BytesUsed, groupEntry.Value.BytesRemaining));

        /// <summary>
        /// Sets the global data limit on the group.
        /// </summary>
        /// <param name="group">Target group.</param>
        /// <param name="dataLimit">The data limit in bytes.</param>
        /// <returns>
        /// 0 if the global data limit is successfully set on the group.
        /// -2 if the group doesn't exist.
        /// </returns>
        public int SetGroupGlobalDataLimit(string group, ulong dataLimit)
        {
            if (Groups.TryGetValue(group, out var targetGroup))
            {
                targetGroup.DataLimitInBytes = dataLimit;
                return 0;
            }
            else
                return -2;
        }

        /// <summary>
        /// Sets the per-user data limit on the group.
        /// </summary>
        /// <param name="group">Target group.</param>
        /// <param name="dataLimit">The data limit in bytes.</param>
        /// <returns>
        /// 0 if the per-user data limit is successfully set on the group.
        /// -2 if the group doesn't exist.
        /// </returns>
        public int SetGroupPerUserDataLimit(string group, ulong dataLimit)
        {
            if (Groups.TryGetValue(group, out var targetGroup))
            {
                targetGroup.PerUserDataLimitInBytes = dataLimit;
                return 0;
            }
            else
                return -2;
        }

        /// <summary>
        /// Associates the Outline server with the group.
        /// Saves the API key. Pulls from the Outline server.
        /// Optionally sets the admin key username.
        /// Optionally sets the per-user data limit.
        /// </summary>
        /// <param name="group">Target group.</param>
        /// <param name="apiKey">Outline server API key.</param>
        /// <param name="users">The <see cref="Users"/> object.</param>
        /// <param name="globalDefaultUser">The global default user setting.</param>
        /// <param name="applyDataLimit">Whether to apply the per-user data limit.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <returns>
        /// The task that represents the operation.
        /// An optional error message.
        /// </returns>
        public Task<string?> AssociateOutlineServerWithGroup(string group, string apiKey, Users users, string? globalDefaultUser = null, bool applyDataLimit = true, CancellationToken cancellationToken = default)
        {
            if (Groups.TryGetValue(group, out var targetGroup))
                return targetGroup.AssociateOutlineServer(group, apiKey, users, _httpClient, globalDefaultUser, applyDataLimit, cancellationToken);
            else
                return Task.FromResult<string?>($"Error: Group {group} doesn't exist.");
        }

        /// <summary>
        /// Gets the Outline API key
        /// associated with the group.
        /// </summary>
        /// <param name="group">Target group.</param>
        /// <returns>
        /// The Outline API key object.
        /// Null if the group doesn't exist or has no associated API key.
        /// </returns>
        public ApiKey? GetOutlineApiKeyFromGroup(string group)
            => Groups.TryGetValue(group, out var targetGroup) ? targetGroup.OutlineApiKey : null;

        /// <summary>
        /// Gets the Outline API key
        /// associated with the group
        /// as a string.
        /// </summary>
        /// <param name="group">Target group.</param>
        /// <returns>
        /// The Outline API key string.
        /// Null if the group doesn't exist or has no associated API key.
        /// </returns>
        public string? GetOutlineApiKeyStringFromGroup(string group)
            => GetOutlineApiKeyFromGroup(group) is ApiKey apiKey ? JsonSerializer.Serialize(apiKey, Utilities.apiKeyJsonSerializerOptions) : null;

        /// <summary>
        /// Gets the Outline server information object.
        /// </summary>
        /// <param name="group">Target group.</param>
        /// <returns>
        /// The Outline server information object.
        /// Null if the group doesn't exist or has no associated information.
        /// </returns>
        public ServerInfo? GetOutlineServerInfoFromGroup(string group)
            => Groups.TryGetValue(group, out var targetGroup) ? targetGroup.OutlineServerInfo : null;

        /// <summary>
        /// Gets the group's Outline server's admin key username.
        /// </summary>
        /// <param name="group">Target group.</param>
        /// <returns>
        /// A string if the admin key exists.
        /// The string may be an empty string.
        /// Null if the group or the admin key doesn't exist.
        /// </returns>
        public string? GetOutlineDefaultUserFromGroup(string group)
            => Groups.TryGetValue(group, out var targetGroup) ? targetGroup.GetOutlineDefaultUser() : null;

        /// <summary>
        /// Changes settings for the group's associated Outline server.
        /// </summary>
        /// <param name="group">Target group.</param>
        /// <param name="name">Server name.</param>
        /// <param name="hostname">Server hostname.</param>
        /// <param name="port">Port number for new access keys.</param>
        /// <param name="metrics">Enable telemetry.</param>
        /// <param name="defaultUser">The default username for access key id 0.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <returns>
        /// The task that represents the operation.
        /// An optional error message.
        /// </returns>
        public Task<string?> SetOutlineServerInGroup(string group, string? name, string? hostname, int? port, bool? metrics, string? defaultUser, CancellationToken cancellationToken = default)
        {
            if (Groups.TryGetValue(group, out var targetGroup))
                return targetGroup.SetOutlineServer(group, name, hostname, port, metrics, defaultUser, _httpClient, cancellationToken);
            else
                return Task.FromResult<string?>($"Error: Group {group} doesn't exist.");
        }

        /// <summary>
        /// Removes the association of the Outline server
        /// and all saved data related to it from the target group.
        /// </summary>
        /// <param name="group">Target group.</param>
        /// <param name="users"> The <see cref="Users"/> object.</param>
        /// <returns>0 for success. -1 when target group doesn't exist.</returns>
        public int RemoveOutlineServerFromGroup(string group, Users users)
        {
            if (Groups.TryGetValue(group, out var targetGroup))
            {
                targetGroup.RemoveOutlineServer(group, users);
                return 0;
            }
            else
                return -1;
        }

        /// <summary>
        /// Deploys the group's Outline server.
        /// </summary>
        /// <param name="group">Target group.</param>
        /// <param name="users">The object which contains all users' information.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <returns>
        /// An async-enumerable sequence whose elements are error messages.
        /// </returns>
        public IAsyncEnumerable<Task> DeployGroupOutlineServer(string group, Users users, CancellationToken cancellationToken = default)
        {
            if (Groups.TryGetValue(group, out var targetGroup))
                return targetGroup.DeployToOutlineServer(group, users, _httpClient, cancellationToken);
            else
                return AsyncEnumerableEx.Return(Task.CompletedTask);
        }

        /// <summary>
        /// Deploy to every associated Outline server.
        /// </summary>
        /// <param name="users">The object which contains all users' information.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <returns>
        /// An async-enumerable sequence whose elements are error messages.
        /// </returns>
        public IAsyncEnumerable<Task> DeployAllOutlineServers(Users users, CancellationToken cancellationToken = default)
            => Groups.Select(x => x.Value.DeployToOutlineServer(x.Key, users, _httpClient, cancellationToken)).ConcurrentMerge();

        /// <summary>
        /// Renames the user and syncs with Outline server in the group.
        /// </summary>
        /// <param name="group">Group name.</param>
        /// <param name="oldName">The old username.</param>
        /// <param name="newName">The new username.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <returns>
        /// The task that represents the operation.
        /// An optional error message.
        /// </returns>
        public Task<string?> RenameUserInGroup(string group, string oldName, string newName, CancellationToken cancellationToken = default)
        {
            if (Groups.TryGetValue(group, out var targetGroup))
                return targetGroup.RenameUser(oldName, newName, _httpClient, cancellationToken);
            else
                return Task.FromResult<string?>($"Error: Group {group} doesn't exist.");
        }

        /// <summary>
        /// Rotates the specified group's user password.
        /// </summary>
        /// <param name="group">Target group.</param>
        /// <param name="users">The object which contains all users' information.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <param name="usernames">Optional. Only target these members in group if specified.</param>
        /// <returns>
        /// An async-enumerable sequence whose elements are error messages.
        /// </returns>
        public IAsyncEnumerable<string> RotateGroupPassword(string group, Users users, CancellationToken cancellationToken = default, params string[] usernames)
        {
            if (Groups.TryGetValue(group, out var targetGroup))
                return targetGroup.RotatePassword(group, users, _httpClient, false, cancellationToken, usernames);
            else
                return AsyncEnumerableEx.Return($"Error: Group {group} doesn't exist.");
        }

        /// <summary>
        /// Rotates password in all supported groups for the specified or all users.
        /// </summary>
        /// <param name="users">The <see cref="Users"/> object.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <param name="usernames">Optional. Only target these members in group if specified.</param>
        /// <returns>
        /// An async-enumerable sequence whose elements are error messages.
        /// </returns>
        public IAsyncEnumerable<string> RotatePasswordForAllGroups(Users users, CancellationToken cancellationToken = default, params string[] usernames)
            => Groups.Select(x => x.Value.RotatePassword(x.Key, users, _httpClient, false, cancellationToken, usernames)).ConcurrentMerge();

        /// <summary>
        /// Pulls server information, user credentials, and statistics,
        /// from servers of the specified or all groups, via available APIs.
        /// </summary>
        /// <param name="groupNames">If not empty, only include servers from these groups.</param>
        /// <param name="users">The <see cref="Users"/> object.</param>
        /// <param name="settings">The <see cref="Settings"/> object.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <returns>The task that represents the operation.</returns>
        /// <exception cref="ArgumentException">One or more group names are not found.</exception>
        public async Task PullGroupsAsync(ReadOnlyMemory<string> groupNames, Users users, Settings settings, CancellationToken cancellationToken = default)
        {
            IReadOnlyCollection<KeyValuePair<string, Group>> groups;

            if (groupNames.IsEmpty)
            {
                groups = Groups;
            }
            else
            {
                ReadOnlySpan<string> groupNamesSpan = groupNames.Span;
                KeyValuePair<string, Group>[] groupArray = new KeyValuePair<string, Group>[groupNamesSpan.Length];
                for (int i = 0; i < groupNamesSpan.Length; i++)
                {
                    string groupName = groupNamesSpan[i];
                    if (Groups.TryGetValue(groupName, out var group))
                    {
                        groupArray[i] = new(groupName, group);
                    }
                    else
                    {
                        throw new ArgumentException($"Group {groupName} not found", nameof(groupNames));
                    }
                }
                groups = groupArray;
            }

            int concurrency = int.Min(settings.ApiRequestConcurrency, groups.Count);

            Channel<KeyValuePair<string, Group>> channel = Channel.CreateBounded<KeyValuePair<string, Group>>(new BoundedChannelOptions(concurrency)
            {
                SingleWriter = true,
            });

            Task[] tasks = new Task[concurrency];

            for (int i = 0; i < concurrency; i++)
            {
                tasks[i] = DoPullJobAsync(channel.Reader, users, cancellationToken);
            }

            try
            {
                foreach (var groupEntry in groups)
                {
                    await channel.Writer.WriteAsync(groupEntry, cancellationToken);
                }
            }
            finally
            {
                channel.Writer.Complete();
            }

            await Task.WhenAll(tasks);
        }

        private async Task DoPullJobAsync(
            ChannelReader<KeyValuePair<string, Group>> reader,
            Users users,
            CancellationToken cancellationToken = default)
        {
            await foreach (KeyValuePair<string, Group> groupEntry in reader.ReadAllAsync(cancellationToken))
            {
                await foreach (Task task in PullGroupAsync(groupEntry, users, cancellationToken))
                {
                    try
                    {
                        await task;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }
        }

        /// <summary>
        /// Pulls server information, user credentials, and statistics,
        /// from the group's server via available APIs.
        /// </summary>
        /// <param name="groupEntry">Target group entry.</param>
        /// <param name="users">The <see cref="Users"/> object.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <returns>An <see cref="IAsyncEnumerable{T}"/> for iterating through completed tasks.</returns>
        public IAsyncEnumerable<Task> PullGroupAsync(KeyValuePair<string, Group> groupEntry, Users users, CancellationToken cancellationToken = default) =>
            groupEntry.Value.SSMv1Server is not null
                ? groupEntry.Value.SSMv1Server.PullAsync(_httpClient, groupEntry.Key, groupEntry.Value, users, cancellationToken)
                : groupEntry.Value.PullFromOutlineServer(groupEntry.Key, users, _httpClient, cancellationToken);

        /// <summary>
        /// Loads nodes from Nodes.json.
        /// </summary>
        /// <param name="cancellationToken">A token that may be used to cancel the read operation.</param>
        /// <returns>
        /// A ValueTuple containing a <see cref="Nodes"/> object and an optional error message.
        /// </returns>
        public static async Task<(Nodes nodes, string? errMsg)> LoadNodesAsync(CancellationToken cancellationToken = default)
        {
            var (nodes, errMsg) = await FileHelper.LoadJsonAsync<Nodes>("Nodes.json", FileHelper.DataJsonSerializerOptions, cancellationToken);
            if (errMsg is null && nodes.Version != DefaultVersion)
            {
                nodes.UpdateNodes();
                errMsg = await SaveNodesAsync(nodes, cancellationToken);
            }
            return (nodes, errMsg);
        }

        /// <summary>
        /// Saves nodes to Nodes.json.
        /// </summary>
        /// <param name="nodes">The <see cref="Nodes"/> object to save.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the write operation.</param>
        /// <returns>
        /// An optional error message.
        /// Null if no errors occurred.
        /// </returns>
        public static Task<string?> SaveNodesAsync(Nodes nodes, CancellationToken cancellationToken = default)
            => FileHelper.SaveJsonAsync("Nodes.json", nodes, FileHelper.DataJsonSerializerOptions, false, false, cancellationToken);

        /// <summary>
        /// Updates the current object to the latest version.
        /// </summary>
        public void UpdateNodes()
        {
            switch (Version)
            {
                case 0: // generate UUID for each node
                    // already generated by the constructor
                    Version++;
                    goto case 1;
                case 1: // nullify empty Plugin and PluginOpts strings
                    foreach (var groupEntry in Groups)
                        foreach (var nodeEntry in groupEntry.Value.NodeDict)
                        {
                            var node = nodeEntry.Value;
                            if (string.IsNullOrEmpty(node.Plugin))
                                node.Plugin = null;
                            if (string.IsNullOrEmpty(node.PluginOpts))
                                node.PluginOpts = null;
                        }
                    Version++;
                    goto default; // go to the next update path
                default:
                    break;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach (var group in Groups.Values)
                        group.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
