using ShadowsocksUriGenerator.Outline;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ShadowsocksUriGenerator
{
    /// <summary>
    /// The class for storing node information in Nodes.json
    /// </summary>
    public class Nodes
    {
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
        /// Adds new node groups to the group dictionary.
        /// </summary>
        /// <param name="groups">The list of groups to be added.</param>
        /// <returns>A List of groups successfully added.</returns>
        public List<string> AddGroups(string[] groups)
        {
            List<string> addedGroups = new();

            foreach (var group in groups)
                if (!Groups.ContainsKey(group))
                {
                    Groups.Add(group, new Group());
                    addedGroups.Add(group);
                }

            return addedGroups;
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
        /// Removes groups from the group dictionary.
        /// </summary>
        /// <param name="groups">The list of groups to be removed.</param>
        public void RemoveGroups(string[] groups)
        {
            foreach (var group in groups)
                Groups.Remove(group);
        }

        /// <summary>
        /// Adds a node to a node group.
        /// </summary>
        /// <param name="group">Destination group name.</param>
        /// <param name="node">Node name</param>
        /// <param name="host">Node's host</param>
        /// <param name="portString">Node's port string to be parsed.</param>
        /// <param name="plugin">Optional. Plugin binary name.</param>
        /// <param name="pluginOpts">Optional. Plugin options.</param>
        /// <returns>0 for success. -1 for non-existing group, duplicated node, invalid port string.</returns>
        public int AddNodeToGroup(string group, string node, string host, string portString, string? plugin = null, string? pluginOpts = null)
        {
            if (int.TryParse(portString, out int port))
                return AddNodeToGroup(group, node, host, port, plugin, pluginOpts);
            else
                return -1;
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
        /// <returns>0 for success. -1 for non-existing group, duplicated node, bad port range.</returns>
        public int AddNodeToGroup(string group, string node, string host, int port, string? plugin = null, string? pluginOpts = null)
        {
            if (Groups.TryGetValue(group, out Group? targetGroup))
            {
                return targetGroup.AddNode(node, host, port, plugin, pluginOpts);
            }
            else
                return -1;
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
        /// Removes nodes from the node group.
        /// </summary>
        /// <param name="group">Group name to remove nodes from.</param>
        /// <param name="nodes">Node name to be removed.</param>
        /// <returns>0 for success or found target group. -1 for non-existing group.</returns>
        public int RemoveNodesFromGroup(string group, string[] nodes)
        {
            if (Groups.TryGetValue(group, out var targetGroup))
            {
                targetGroup.RemoveNodes(nodes);
                return 0;
            }
            else
                return -1;
        }

        /// <summary>
        /// Associates the Outline server with the node group
        /// by setting the API key.
        /// </summary>
        /// <param name="group">Target group.</param>
        /// <param name="apiKey">Outline server API key.</param>
        /// <returns>
        /// 0 when success.
        /// -1 when target group doesn't exist.
        /// -2 when the API key is not a valid JSON string.
        /// </returns>
        public int AssociateOutlineServerWithGroup(string group, string apiKey)
        {
            if (Groups.TryGetValue(group, out var targetGroup))
                return targetGroup.AssociateOutlineServer(apiKey);
            else
                return -1;
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
        /// <returns>
        /// The task that represents the operation.
        /// Null if the group can't be found
        /// or no associated Outline server.</returns>
        public Task<List<HttpStatusCode>?> SetOutlineServerInGroup(string group, string? name, string? hostname, int? port, bool? metrics, string? defaultUser)
        {
            if (Groups.TryGetValue(group, out var targetGroup))
            {
                if (!string.IsNullOrEmpty(defaultUser))
                    targetGroup.OutlineDefaultUser = defaultUser;
                return targetGroup.SetOutlineServer(name, hostname, port, metrics);
            }
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
    }

    /// <summary>
    /// Nodes in a group share the same credential for a user.
    /// They may provide different credentials for different users.
    /// </summary>
    public class Group
    {
        /// <summary>
        /// Gets or sets the Outline Access Keys Management API key object.
        /// </summary>
        public ApiKey? OutlineApiKey { get; set; }

        /// <summary>
        /// Gets or sets the Outline Server information object.
        /// </summary>
        public ServerInfo? OutlineServerInfo { get; set; }

        /// <summary>
        /// Gets or sets the list of Outline Server access keys.
        /// </summary>
        public List<AccessKey>? OutlineAccessKeys { get; set; }

        /// <summary>
        /// Gets or sets the default user for Outline server's default access key (id: 0).
        /// </summary>
        public string? OutlineDefaultUser { get; set; }

        /// <summary>
        /// Gets or sets the data limit of the group in bytes.
        /// </summary>
        public ulong DataLimitInBytes { get; set; }

        /// <summary>
        /// Gets or sets the data usage in bytes.
        /// </summary>
        public ulong BytesUsed { get; set; }

        /// <summary>
        /// Gets or sets the data remaining to be used in bytes.
        /// </summary>
        public ulong BytesRemaining { get; set; }

        /// <summary>
        /// Gets or sets the Node Dictionary.
        /// key is node name.
        /// value is node info.
        /// 
        /// address = hostname + ":" + port.
        /// Examples: 
        /// foo.bar:80
        /// [2001::1]:443
        /// 1.1.1.1:853
        /// </summary>
        [JsonPropertyName("Nodes")]
        public Dictionary<string, Node> NodeDict { get; set; }

        public Group()
        {
            NodeDict = new();
        }

        /// <summary>
        /// Adds a node to <see cref="NodeDict"/>
        /// </summary>
        /// <param name="name">Node name.</param>
        /// <param name="host">Node's host</param>
        /// <param name="port">Node's port number</param>
        /// <param name="plugin">Optional. Plugin binary name.</param>
        /// <param name="pluginOpts">Optional. Plugin options.</param>
        /// <returns>0 for success. -1 for duplicated name.</returns>
        public int AddNode(string name, string host, int port, string? plugin = null, string? pluginOpts = null)
        {
            if (!NodeDict.ContainsKey(name))
            {
                var node = new Node(host, port, plugin, pluginOpts);
                NodeDict.Add(name, node);
                return 0;
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Renames an existing node with a new name.
        /// </summary>
        /// <param name="oldName">The existing node name.</param>
        /// <param name="newName">The new node name.</param>
        /// <returns>
        /// 0 when success.
        /// -1 when old node name is not found.
        /// -2 when new node name already exists.
        /// </returns>
        public int RenameNode(string oldName, string newName)
        {
            if (NodeDict.ContainsKey(newName))
                return -2;
            if (!NodeDict.Remove(oldName, out var node))
                return -1;
            NodeDict.Add(newName, node);
            return 0;
        }

        /// <summary>
        /// Removes nodes from <see cref="NodeDict"/>.
        /// </summary>
        /// <param name="nodes">The list of nodes to be removed.</param>
        public void RemoveNodes(string[] nodes)
        {
            foreach (var node in nodes)
                NodeDict.Remove(node);
        }

        /// <summary>
        /// Associates the Outline server
        /// by adding the API key.
        /// </summary>
        /// <param name="apiKey">The Outline server API key.</param>
        /// <returns>0 for success. -2 for invalid JSON string.</returns>
        public int AssociateOutlineServer(string apiKey)
        {
            try
            {
                OutlineApiKey = JsonSerializer.Deserialize<ApiKey>(apiKey, Outline.Utilities.apiKeyJsonSerializerOptions);
                return 0;
            }
            catch (JsonException)
            {
                return -2;
            }
        }

        /// <summary>
        /// Changes settings for the associated Outline server.
        /// </summary>
        /// <param name="name">Server name.</param>
        /// <param name="hostname">Server hostname.</param>
        /// <param name="port">Port number for new access keys.</param>
        /// <param name="metrics">Enable telemetry.</param>
        /// <returns>The task that represents the operation. Null if no associated Outline server.</returns>
        public async Task<List<HttpStatusCode>?> SetOutlineServer(string? name, string? hostname, int? port, bool? metrics)
        {
            if (OutlineApiKey == null)
                return null;

            using var apiClient = new ApiClient(OutlineApiKey);
            var tasks = new List<Task<HttpResponseMessage>>();
            var statusCodes = new List<HttpStatusCode>();

            if (!string.IsNullOrEmpty(name))
                tasks.Add(apiClient.SetServerNameAsync(name));
            if (!string.IsNullOrEmpty(hostname))
                tasks.Add(apiClient.SetServerHostnameAsync(hostname));
            if (port is int portForNewAccessKeys)
                tasks.Add(apiClient.SetAccessKeysPortAsync(portForNewAccessKeys));
            if (metrics is bool enableMetrics)
                tasks.Add(apiClient.SetServerMetricsAsync(enableMetrics));

            while (tasks.Count > 0)
            {
                var finishedTask = await Task.WhenAny(tasks);
                var responseMessage = await finishedTask;
                statusCodes.Add(responseMessage.StatusCode);
                tasks.Remove(finishedTask);
            }

            return statusCodes;
        }

        /// <summary>
        /// Removes the association of the Outline server
        /// and all saved data related to it.
        /// </summary>
        public void RemoveOutlineServer()
        {
            OutlineApiKey = null;
            OutlineServerInfo = null;
            OutlineAccessKeys = null;
        }

        public async Task<int> UpdateOutlineServer()
        {
            if (OutlineApiKey == null)
                return -1;

            using var apiClient = new ApiClient(OutlineApiKey);
            var serverInfoTask = apiClient.GetServerInfoAsync();
            var accessKeysTask = apiClient.GetAccessKeysAsync();
            var dataUsageTask = apiClient.GetDataUsageAsync();
            var tasks = new List<Task>()
            {
                serverInfoTask,
                accessKeysTask,
                dataUsageTask,
            };

            while (tasks.Count > 0)
            {
                var finishedTask = await Task.WhenAny(tasks);
                if (finishedTask == serverInfoTask)
                    OutlineServerInfo = await (Task<ServerInfo?>)finishedTask;
                else if (finishedTask == accessKeysTask)
                    OutlineAccessKeys = await (Task<List<AccessKey>?>)finishedTask;
                else if (finishedTask == dataUsageTask)
                {
                    // update data usage
                }
                tasks.Remove(finishedTask);
            }

            return 0;
        }

        /// <summary>
        /// Sets the data limit.
        /// </summary>
        /// <param name="dataLimit">The data limit in bytes.</param>
        /// <param name="global">Set the global data limit of the group.</param>
        /// <param name="perUser">Set the same data limit for each user.</param>
        /// <param name="usernames">Only set the data limit to these users.</param>
        /// <returns></returns>
        public int SetDataLimit(ulong dataLimit, bool global, bool perUser, string[]? usernames = null)
        {
            if (global)
                DataLimitInBytes = dataLimit;
            return 0;
        }
    }

    /// <summary>
    /// Stores node's host and port.
    /// </summary>
    public class Node
    {
        /// <summary>
        /// Gets or sets the UUID of the node.
        /// Used for online configuration delivery (SIP008).
        /// </summary>
        public string Uuid { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string? Plugin { get; set; }
        public string? PluginOpts { get; set; }

        /// <summary>
        /// Parameterless constructor for System.Text.Json
        /// </summary>
        public Node()
        {
            Uuid = Guid.NewGuid().ToString();
            Host = "";
            Port = 0;
        }

        public Node(string host, int port, string? plugin = null, string? pluginOpts = null)
        {
            Uuid = Guid.NewGuid().ToString();
            Host = host;
            Port = port;
            Plugin = plugin;
            PluginOpts = pluginOpts;
        }
    }
}
