using ShadowsocksUriGenerator.Outline;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ShadowsocksUriGenerator
{
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
        /// Gets or sets the Outline server information object.
        /// </summary>
        public ServerInfo? OutlineServerInfo { get; set; }

        /// <summary>
        /// Gets or sets the list of Outline server access keys.
        /// </summary>
        public List<AccessKey>? OutlineAccessKeys { get; set; }

        /// <summary>
        /// Gets or sets the Outline server data usage object.
        /// </summary>
        public DataUsage? OutlineDataUsage { get; set; }

        /// <summary>
        /// Gets or sets the dictionary object for Outline server's
        /// per-user data limit.
        /// </summary>
        public Dictionary<int, ulong>? OutlineUserDataLimit { get; set; }

        /// <summary>
        /// Gets or sets the default user for Outline server's default access key (id: 0).
        /// </summary>
        public string? OutlineDefaultUser { get; set; }

        /// <summary>
        /// Gets or sets the data limit of the group in bytes.
        /// </summary>
        public ulong DataLimitInBytes { get; set; }

        /// <summary>
        /// Gets or sets the per-user data limit of the group in bytes.
        /// </summary>
        public ulong PerUserDataLimitInBytes { get; set; }

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

        /// <summary>
        /// Updates the associated Outline server's
        /// information, access keys, and data usage.
        /// </summary>
        /// <returns>0 on success. -2 when no associated Outline server.</returns>
        public async Task<int> UpdateOutlineServer()
        {
            if (OutlineApiKey == null)
                return -2;

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
                    OutlineDataUsage = await (Task<DataUsage?>)finishedTask;
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
            if (perUser)
                PerUserDataLimitInBytes = dataLimit;
            if (usernames != null)
            {
                // TODO: resolve user id and save data limit to dictionary.
            }
            return 0;
        }
    }
}
