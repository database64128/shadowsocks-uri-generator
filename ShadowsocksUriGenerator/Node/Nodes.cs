using ShadowsocksUriGenerator.Outline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ShadowsocksUriGenerator
{
    /// <summary>
    /// The class for storing node information in Nodes.json
    /// </summary>
    public class Nodes : IDisposable
    {
        private bool disposedValue;

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
        public int Version { get; set; }

        /// <summary>
        /// Gets or sets the group dictionary.
        /// key is group name.
        /// value is group info.
        /// </summary>
        public Dictionary<string, Group> Groups { get; set; }

        public Nodes()
        {
            Version = DefaultVersion;
            Groups = new();
        }

        /// <summary>
        /// Adds a new node group to the group dictionary.
        /// </summary>
        /// <param name="group">The group to add.</param>
        /// <returns>
        /// 0 for success.
        /// 1 when a group with the same name already exists.
        /// </returns>
        public int AddGroup(string group)
        {
            if (!Groups.ContainsKey(group))
            {
                Groups.Add(group, new());
                return 0;
            }
            else
                return 1;
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
        /// <see cref="true"/> if the group is successfully found and removed.
        /// Otherwise, <see cref="false"/>.
        /// </returns>
        public bool RemoveGroup(string group) => Groups.Remove(group);

        /// <summary>
        /// Adds a node to a node group.
        /// </summary>
        /// <param name="group">Destination group name.</param>
        /// <param name="node">Node name</param>
        /// <param name="host">Node's host</param>
        /// <param name="portString">Node's port string to be parsed.</param>
        /// <param name="plugin">Optional. Plugin binary name.</param>
        /// <param name="pluginOpts">Optional. Plugin options.</param>
        /// <returns>
        /// 0 if success.
        /// -1 if a node with the same name already exists.
        /// -2 if the group doesn't exist.
        /// -3 if the port number is invalid.
        /// </returns>
        public int AddNodeToGroup(string group, string node, string host, string portString, string? plugin = null, string? pluginOpts = null)
        {
            if (int.TryParse(portString, out int port))
                return AddNodeToGroup(group, node, host, port, plugin, pluginOpts);
            else
                return -3;
        }


        /// <summary>
        /// Adds a node to a node group.
        /// </summary>
        /// <param name="group">Destination group name.</param>
        /// <param name="node">Node name</param>
        /// <param name="host">Node's host</param>
        /// <param name="port">Node's port number.</param>
        /// <param name="plugin">Optional. Plugin binary name.</param>
        /// <param name="pluginOpts">Optional. Plugin options.</param>
        /// <returns>
        /// 0 if success.
        /// -1 if a node with the same name already exists.
        /// -2 if the group doesn't exist.
        /// </returns>
        public int AddNodeToGroup(string group, string node, string host, int port, string? plugin = null, string? pluginOpts = null)
        {
            if (Groups.TryGetValue(group, out Group? targetGroup))
            {
                return targetGroup.AddNode(node, host, port, plugin, pluginOpts);
            }
            else
                return -2;
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
        /// <returns>
        /// A list of data usage records as tuples.
        /// Null if the group doesn't exist.
        /// </returns>
        public List<(string username, ulong bytesUsed, ulong bytesRemaining)>? GetGroupDataUsage(string group)
        {
            if (Groups.TryGetValue(group, out var targetGroup))
                return targetGroup.GetDataUsage();
            else
                return null;
        }

        /// <summary>
        /// Gets data usage records that contains
        /// each group's total data usage.
        /// </summary>
        /// <returns>A list of data usage records as tuples.</returns>
        public List<(string group, ulong bytesUsed, ulong bytesRemaining)> GetDataUsageByGroup()
        {
            List<(string group, ulong bytesUsed, ulong bytesRemaining)> records = new();
            foreach (var groupEntry in Groups)
                records.Add((groupEntry.Key, groupEntry.Value.BytesUsed, groupEntry.Value.BytesRemaining));
            return records;
        }

        /// <summary>
        /// Sets the data limit for the specified group.
        /// </summary>
        /// <param name="dataLimit">The data limit in bytes.</param>
        /// <param name="group">Target group.</param>
        /// <param name="global">Set the global data limit of the group.</param>
        /// <param name="perUser">Set the same data limit for each user.</param>
        /// <param name="usernames">Only set the data limit to these users.</param>
        /// <returns>0 on success. -1 on group not found. -2 on user not found.</returns>
        public int SetDataLimitForGroup(ulong dataLimit, string group, bool global, bool perUser, string[]? usernames = null)
        {
            if (Groups.TryGetValue(group, out var targetGroup))
                return targetGroup.SetDataLimit(dataLimit, global, perUser, usernames);
            else
                return -1;
        }

        /// <summary>
        /// Associates the Outline server with the node group
        /// by setting the API key.
        /// </summary>
        /// <param name="group">Target group.</param>
        /// <param name="apiKey">Outline server API key.</param>
        /// <param name="globalDefaultUser">The global default user setting.</param>
        /// <returns>
        /// 0 when success.
        /// -1 when target group doesn't exist.
        /// -2 when the API key is not a valid JSON string.
        /// -3 when applying default user failed.
        /// </returns>
        public Task<int> AssociateOutlineServerWithGroup(string group, string apiKey, string? globalDefaultUser, CancellationToken cancellationToken = default)
        {
            if (Groups.TryGetValue(group, out var targetGroup))
                return targetGroup.AssociateOutlineServer(apiKey, globalDefaultUser, cancellationToken);
            else
                return Task.FromResult(-1);
        }

        /// <summary>
        /// Gets the Outline API key
        /// associated with the group.
        /// </summary>
        /// <param name="group">Name of the group.</param>
        /// <returns>
        /// The Outline API key object.
        /// Null if group not found or no associated API key.
        /// </returns>
        public ApiKey? GetOutlineApiKeyFromGroup(string group)
            => Groups.TryGetValue(group, out var targetGroup) ? targetGroup.OutlineApiKey : null;

        /// <summary>
        /// Gets the Outline API key
        /// associated with the group
        /// as a string.
        /// </summary>
        /// <param name="group">Name of the group.</param>
        /// <returns>
        /// The Outline API key string.
        /// Null if group not found or no associated API key.
        /// </returns>
        public string? GetOutlineApiKeyStringFromGroup(string group)
            => GetOutlineApiKeyFromGroup(group) is ApiKey apiKey ? JsonSerializer.Serialize(apiKey, Outline.Utilities.apiKeyJsonSerializerOptions) : null;

        /// <summary>
        /// Gets the Outline server information object.
        /// </summary>
        /// <param name="group">Name of the group.</param>
        /// <returns>
        /// The Outline server information object.
        /// Null if group not found or no associated information.
        /// </returns>
        public ServerInfo? GetOutlineServerInfoFromGroup(string group)
            => Groups.TryGetValue(group, out var targetGroup) ? targetGroup.OutlineServerInfo : null;

        /// <summary>
        /// Changes settings for the group's associated Outline server.
        /// </summary>
        /// <param name="group">Target group.</param>
        /// <param name="name">Server name.</param>
        /// <param name="hostname">Server hostname.</param>
        /// <param name="port">Port number for new access keys.</param>
        /// <param name="metrics">Enable telemetry.</param>
        /// <param name="defaultUser">The default username for access key id 0.</param>
        /// <returns>
        /// The task that represents the operation.
        /// Null if the group can't be found
        /// or no associated Outline server.</returns>
        public Task<List<HttpStatusCode>?> SetOutlineServerInGroup(string group, string? name, string? hostname, int? port, bool? metrics, string? defaultUser, CancellationToken cancellationToken = default)
        {
            if (Groups.TryGetValue(group, out var targetGroup))
                return targetGroup.SetOutlineServer(name, hostname, port, metrics, defaultUser, cancellationToken);
            else
                return Task.FromResult<List<HttpStatusCode>?>(null);
        }

        /// <summary>
        /// Removes the association of the Outline server
        /// and all saved data related to it from the target group.
        /// </summary>
        /// <param name="group">Target group.</param>
        /// <returns>0 for success. -1 when target group doesn't exist.</returns>
        public int RemoveOutlineServerFromGroup(string group)
        {
            if (Groups.TryGetValue(group, out var targetGroup))
            {
                targetGroup.RemoveOutlineServer();
                return 0;
            }
            else
                return -1;
        }

        /// <summary>
        /// Updates every assoicated Outline server's
        /// information, access keys, and data usage.
        /// </summary>
        /// <returns>A task representing the update process.</returns>
        public Task UpdateOutlineServerForAllGroups(Users users, bool updateLocalCredentials, CancellationToken cancellationToken = default)
        {
            var tasks = Groups.Select(async x => await x.Value.UpdateOutlineServer(x.Key, users, updateLocalCredentials, cancellationToken));
            return Task.WhenAll(tasks);
        }

        /// <summary>
        /// Updates the specified group's Outline server's
        /// information, access keys, and data usage.
        /// </summary>
        /// <param name="group"></param>
        /// <returns>
        /// 0 on success.
        /// -1 when target group doesn't exist.
        /// -2 when no associated Outline server.
        /// </returns>
        public Task<int> UpdateGroupOutlineServer(string group, Users users, bool updateLocalCredentials, CancellationToken cancellationToken = default)
        {
            if (Groups.TryGetValue(group, out var targetGroup))
                return targetGroup.UpdateOutlineServer(group, users, updateLocalCredentials, cancellationToken);
            else
                return Task.FromResult(-1);
        }

        /// <summary>
        /// Deploys the group's Outline server.
        /// </summary>
        /// <param name="group">Target group.</param>
        /// <param name="users">The object which contains all users' information.</param>
        /// <returns>
        /// 0 on success.
        /// -1 when target group doesn't exist.
        /// -2 when no associated Outline server.
        /// </returns>
        public Task<int> DeployGroupOutlineServer(string group, Users users, CancellationToken cancellationToken = default)
        {
            if (Groups.TryGetValue(group, out var targetGroup))
                return targetGroup.DeployToOutlineServer(group, users, cancellationToken);
            else
                return Task.FromResult(-1);
        }

        /// <summary>
        /// Deploy to every associated Outline server.
        /// </summary>
        /// <param name="users">The object which contains all users' information.</param>
        /// <returns>The task that represents the completion of all deployments.</returns>
        public Task DeployAllOutlineServers(Users users, CancellationToken cancellationToken = default)
        {
            var tasks = Groups.Select(async x => await x.Value.DeployToOutlineServer(x.Key, users, cancellationToken));
            return Task.WhenAll(tasks);
        }

        /// <summary>
        /// Renames the user and syncs with Outline server in the group.
        /// </summary>
        /// <param name="group">Group name.</param>
        /// <param name="oldName">The old username.</param>
        /// <param name="newName">The new username.</param>
        /// <returns>
        /// 0 on success.
        /// 1 when not an Outline user.
        /// 2 when not on the Outline server.
        /// -1 when target group doesn't exist.
        /// -2 when no associated Outline server.
        /// -3 when an error occurred while sending the request.
        /// </returns>
        public Task<int> RenameUserInGroup(string group, string oldName, string newName)
        {
            if (Groups.TryGetValue(group, out var targetGroup))
                return targetGroup.RenameUser(oldName, newName);
            else
                return Task.FromResult(-1);
        }

        /// <summary>
        /// Rotate the specified group's user password.
        /// </summary>
        /// <param name="group">Target group.</param>
        /// <param name="users">The object which contains all users' information.</param>
        /// <param name="usernames">Optional. The list of target users.</param>
        /// <returns>
        /// 0 on success.
        /// -1 when target group doesn't exist.
        /// -2 when no associated Outline server.
        /// </returns>
        public Task<int> RotateGroupPassword(string group, Users users, CancellationToken cancellationToken = default, params string[]? usernames)
        {
            if (Groups.TryGetValue(group, out var targetGroup))
                return targetGroup.RotatePassword(group, users, cancellationToken, usernames);
            else
                return Task.FromResult(-1);
        }

        /// <summary>
        /// Loads nodes from Nodes.json.
        /// </summary>
        /// <returns>A <see cref="Nodes"/> object.</returns>
        public static async Task<Nodes> LoadNodesAsync()
        {
            var nodes = await Utilities.LoadJsonAsync<Nodes>("Nodes.json", Utilities.commonJsonDeserializerOptions);
            if (nodes.Version != DefaultVersion)
            {
                UpdateNodes(ref nodes);
                await SaveNodesAsync(nodes);
            }
            return nodes;
        }

        /// <summary>
        /// Saves nodes to Nodes.json.
        /// </summary>
        /// <param name="nodes">The <see cref="Nodes"/> object to save.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        public static async Task SaveNodesAsync(Nodes nodes)
            => await Utilities.SaveJsonAsync("Nodes.json", nodes, Utilities.commonJsonSerializerOptions);

        /// <summary>
        /// Updates the nodes version.
        /// </summary>
        /// <param name="nodes">The <see cref="Nodes"/> object to update.</param>
        public static void UpdateNodes(ref Nodes nodes)
        {
            switch (nodes.Version)
            {
                case 0: // generate UUID for each node
                    // already generated by the constructor
                    nodes.Version++;
                    goto case 1;
                case 1: // nullify empty Plugin and PluginOpts strings
                    foreach (var groupEntry in nodes.Groups)
                        foreach (var nodeEntry in groupEntry.Value.NodeDict)
                        {
                            var node = nodeEntry.Value;
                            if (string.IsNullOrEmpty(node.Plugin))
                                node.Plugin = null;
                            if (string.IsNullOrEmpty(node.PluginOpts))
                                node.PluginOpts = null;
                        }
                    nodes.Version++;
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
