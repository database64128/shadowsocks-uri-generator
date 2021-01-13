using ShadowsocksUriGenerator.Outline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
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

        /// <summary>
        /// The Outline API client instance.
        /// </summary>
        private ApiClient? _apiClient;

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
        /// Activates the specified node.
        /// </summary>
        /// <param name="nodeName">Name of the node.</param>
        /// <returns>
        /// 0 if successfully activated the node.
        /// 1 if already activated.
        /// -1 if node not found.
        /// </returns>
        public int ActivateNode(string nodeName)
        {
            if (NodeDict.TryGetValue(nodeName, out var node))
            {
                if (!node.Deactivated)
                    return 1;
                else
                {
                    node.Deactivated = false;
                    return 0;
                }
            }
            else
                return -1;
        }

        /// <summary>
        /// Deactivates the specified node.
        /// </summary>
        /// <param name="nodeName">Name of the node.</param>
        /// <returns>
        /// 0 if successfully deactivated the node.
        /// 1 if already deactivated.
        /// -1 if node not found.
        /// </returns>
        public int DeactivateNode(string nodeName)
        {
            if (NodeDict.TryGetValue(nodeName, out var node))
            {
                if (node.Deactivated)
                    return 1;
                else
                {
                    node.Deactivated = true;
                    return 0;
                }
            }
            else
                return -1;
        }

        /// <summary>
        /// Calculates the group's total data usage
        /// with statistics from Outline server.
        /// </summary>
        public void CalculateTotalDataUsage()
        {
            var bytesUsedByUser = OutlineDataUsage?.BytesTransferredByUserId.Values;
            if (bytesUsedByUser != null && bytesUsedByUser.Count > 0)
                BytesUsed = bytesUsedByUser.Aggregate((x, y) => x + y);
            else
                BytesUsed = 0UL;

            if (DataLimitInBytes > 0UL)
                BytesRemaining = DataLimitInBytes - BytesUsed;
        }

        /// <summary>
        /// Gets all data usage records of the group.
        /// </summary>
        /// <returns>A list of data usage records as tuples.</returns>
        public List<(string username, ulong bytesUsed, ulong bytesRemaining)> GetDataUsage()
        {
            List<(string username, ulong bytesUsed, ulong bytesRemaining)> result = new();

            if (OutlineAccessKeys == null || OutlineDataUsage == null)
                return result;

            foreach (var dataUsage in OutlineDataUsage.BytesTransferredByUserId)
            {
                var usernames = OutlineAccessKeys.Where(x => x.Id == dataUsage.Key.ToString()).Select(x => x.Name);
                var username = usernames.Any() ? usernames.First() : "";
                var bytesUsed = dataUsage.Value;
                var bytesRemaining = PerUserDataLimitInBytes == 0 ? 0 : PerUserDataLimitInBytes - bytesUsed;
                result.Add((username, bytesUsed, bytesRemaining));
            }

            return result;
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

        /// <summary>
        /// Associates the Outline server
        /// by adding the API key.
        /// </summary>
        /// <param name="apiKey">The Outline server API key.</param>
        /// <param name="globalDefaultUser">The global default user setting.</param>
        /// <returns>0 for success. -2 for invalid JSON string. -3 when applying default user failed.</returns>
        public async Task<int> AssociateOutlineServer(string apiKey, string? globalDefaultUser, CancellationToken cancellationToken = default)
        {
            try
            {
                OutlineApiKey = JsonSerializer.Deserialize<ApiKey>(apiKey, Outline.Utilities.apiKeyJsonSerializerOptions);
            }
            catch (JsonException)
            {
                return -2;
            }

            if (OutlineApiKey == null)
                return -2;
            _apiClient = new(OutlineApiKey);

            if (!string.IsNullOrEmpty(globalDefaultUser))
            {
                var result = await SetOutlineDefaultUser(globalDefaultUser, cancellationToken);
                if (!result.IsSuccessStatusCode)
                    return -3;
            }

            return 0;
        }

        /// <summary>
        /// Sets and applies the Outline server default user setting.
        /// </summary>
        /// <param name="defaultUser">The default username for access key id 0.</param>
        /// <returns>The task that represents the asynchronous operation.</returns>
        public Task<HttpResponseMessage> SetOutlineDefaultUser(string defaultUser, CancellationToken cancellationToken = default)
        {
            if (OutlineApiKey == null)
                throw new InvalidOperationException("Outline API key is not found.");
            if (_apiClient == null)
                _apiClient = new(OutlineApiKey);

            OutlineDefaultUser = defaultUser;
            return _apiClient.SetAccessKeyNameAsync("0", defaultUser, cancellationToken);
        }

        /// <summary>
        /// Changes settings for the associated Outline server.
        /// </summary>
        /// <param name="name">Server name.</param>
        /// <param name="hostname">Server hostname.</param>
        /// <param name="port">Port number for new access keys.</param>
        /// <param name="metrics">Enable telemetry.</param>
        /// <param name="defaultUser">The default username for access key id 0.</param>
        /// <returns>The task that represents the operation. Null if no associated Outline server.</returns>
        public async Task<List<HttpStatusCode>?> SetOutlineServer(string? name, string? hostname, int? port, bool? metrics, string? defaultUser, CancellationToken cancellationToken = default)
        {
            if (OutlineApiKey == null)
                return null;
            if (_apiClient == null)
                _apiClient = new(OutlineApiKey);

            var tasks = new List<Task<HttpResponseMessage>>();
            var statusCodes = new List<HttpStatusCode>();

            if (!string.IsNullOrEmpty(name))
                tasks.Add(_apiClient.SetServerNameAsync(name, cancellationToken));
            if (!string.IsNullOrEmpty(hostname))
                tasks.Add(_apiClient.SetServerHostnameAsync(hostname, cancellationToken));
            if (port is int portForNewAccessKeys)
                tasks.Add(_apiClient.SetAccessKeysPortAsync(portForNewAccessKeys, cancellationToken));
            if (metrics is bool enableMetrics)
                tasks.Add(_apiClient.SetServerMetricsAsync(enableMetrics, cancellationToken));
            if (!string.IsNullOrEmpty(defaultUser))
                tasks.Add(SetOutlineDefaultUser(defaultUser, cancellationToken));

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
            OutlineDataUsage = null;
            OutlineUserDataLimit = null;
            OutlineDefaultUser = null;
        }

        /// <summary>
        /// Updates the associated Outline server's
        /// information, access keys, and data usage.
        /// Optionally updates user credential dictionary
        /// in the local storage.
        /// </summary>
        /// <param name="group">Group name.</param>
        /// <param name="users">The <see cref="Users"/> object.</param>
        /// <param name="updateLocalCredentials">
        /// Whether to update user credential dictionary.
        /// </param>
        /// <returns>0 on success. -2 when no associated Outline server.</returns>
        public async Task<int> UpdateOutlineServer(string group, Users users, bool updateLocalCredentials, CancellationToken cancellationToken = default)
        {
            if (OutlineApiKey == null)
                return -2;
            if (_apiClient == null)
                _apiClient = new(OutlineApiKey);

            var serverInfoTask = _apiClient.GetServerInfoAsync(cancellationToken);
            var accessKeysTask = _apiClient.GetAccessKeysAsync(cancellationToken);
            var dataUsageTask = _apiClient.GetDataUsageAsync(cancellationToken);
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
                    OutlineAccessKeys = (await (Task<AccessKeysResponse?>)finishedTask)?.AccessKeys;
                else if (finishedTask == dataUsageTask)
                    OutlineDataUsage = await (Task<DataUsage?>)finishedTask;
                tasks.Remove(finishedTask);
            }

            if (updateLocalCredentials)
                UpdateLocalCredentials(group, users);

            CalculateTotalDataUsage();

            return 0;
        }

        /// <summary>
        /// Syncs <see cref="OutlineAccessKeys"/> with user credential dictionary.
        /// </summary>
        /// <param name="group">Group name.</param>
        /// <param name="users">The <see cref="Users"/> object.</param>
        private void UpdateLocalCredentials(string group, Users users)
        {
            if (OutlineAccessKeys == null)
                return;

            var outlineUsers = OutlineAccessKeys.Select(x => x.Name);

            foreach (var userEntry in users.UserDict)
            {
                var userHasAccessKey = outlineUsers.Contains(userEntry.Key);
                var userInGroup = userEntry.Value.Credentials.TryGetValue(group, out var credential);

                AccessKey userAccessKey = null!; // Null forgiving reason: we can guarantee a non-null reference with userHasAccessKey.
                if (userHasAccessKey)
                    userAccessKey = OutlineAccessKeys.Where(x => x.Name == userEntry.Key).First();

                if (userHasAccessKey && userInGroup)
                {
                    if (credential == null) // No credential. Add it.
                        userEntry.Value.Credentials[group] = new(userAccessKey.Method, userAccessKey.Password);
                    else if (credential.Method == userAccessKey.Method && credential.Password == userAccessKey.Password) // Has credential. Compare credential with access key
                    {
                    }
                    else // Unequal credential.
                        userEntry.Value.Credentials[group] = new(userAccessKey.Method, userAccessKey.Password);
                }
                else if (userHasAccessKey) // User not in group. Add to group with credential.
                {
                    userEntry.Value.AddCredential(group, userAccessKey.Method, userAccessKey.Password);
                }
                else if (userInGroup) // User has no access key. Make sure no associated group credential.
                {
                    userEntry.Value.Credentials[group] = null;
                }
            }
        }

        /// <summary>
        /// Deploys local user configurations to the Outline server.
        /// </summary>
        /// <returns>0 on success. -2 when no associated Outline server.</returns>
        public async Task<int> DeployToOutlineServer(string group, Users users, CancellationToken cancellationToken = default)
        {
            if (OutlineApiKey == null)
                return -2;
            if (OutlineAccessKeys == null)
                await UpdateOutlineServer(group, users, true, cancellationToken);
            if (OutlineAccessKeys == null)
                OutlineAccessKeys = new();

            var tasks = new List<Task<HttpStatusCode[]>>();
            var statusCodes = new List<HttpStatusCode>();

            // Filter out a list of users to create
            // and another list of users to remove
            var existingUsersOnOutlineServer = OutlineAccessKeys.Select(x => x.Name);
            var existingUsersInGroup = users.UserDict.Where(x => x.Value.Credentials.ContainsKey(group)).Select(x => x.Key);
            var usersToCreate = existingUsersInGroup.Except(existingUsersOnOutlineServer);
            var usersToRemove = existingUsersOnOutlineServer.Except(existingUsersInGroup);

            // Add
            foreach (var username in usersToCreate)
                tasks.Add(AddUserToOutlineServer(username, users.UserDict[username].DataLimitInBytes, users.UserDict[username].Credentials, group, cancellationToken));

            // Remove
            tasks.Add(RemoveUserFromOutlineServer(usersToRemove, users, group, cancellationToken));

            while (tasks.Count > 0)
            {
                var finishedTask = await Task.WhenAny(tasks);
                statusCodes.AddRange(await finishedTask);
                tasks.Remove(finishedTask);
            }

            return 0;
        }

        /// <summary>
        /// Renames a user and syncs with Outline server.
        /// </summary>
        /// <param name="oldName">The old username.</param>
        /// <param name="newName">The new username.</param>
        /// <returns>
        /// 0 on success.
        /// 1 when not an Outline user.
        /// 2 when not on the Outline server.
        /// -3 when an error occurred while sending the request.
        /// </returns>
        public async Task<int> RenameUser(string oldName, string newName, CancellationToken cancellationToken = default)
        {
            if (OutlineApiKey == null || OutlineAccessKeys == null)
                return 1;
            if (_apiClient == null)
                _apiClient = new(OutlineApiKey);

            // find user id
            var filteredUserIds = OutlineAccessKeys.Where(x => x.Name == oldName).Select(x => x.Id);
            var userId = filteredUserIds.Any() ? filteredUserIds.First() : null;
            if (userId == null)
                return 2;

            // send request
            var response = await _apiClient.SetAccessKeyNameAsync(userId, newName, cancellationToken);
            if (response.IsSuccessStatusCode)
                return 0;
            else
                return -3;
        }

        /// <summary>
        /// Rotates password for the specified user or all users in the group.
        /// </summary>
        /// <param name="username">Target user.</param>
        /// <returns>0 on success. -2 when no associated Outline server.</returns>
        public async Task<int> RotatePassword(string group, Users users, CancellationToken cancellationToken = default, params string[]? usernames)
        {
            if (OutlineApiKey == null)
                return -2;
            if (OutlineAccessKeys == null)
                await UpdateOutlineServer(group, users, true, cancellationToken);
            if (OutlineAccessKeys == null)
                OutlineAccessKeys = new();
            if (_apiClient == null)
                _apiClient = new(OutlineApiKey);

            var tasks = new List<Task<HttpStatusCode[]>>();
            var statusCodes = new List<HttpStatusCode>();
            var targetUsers = usernames ?? users.UserDict.Where(x => x.Value.Credentials.ContainsKey(group)).Select(x => x.Key);

            // Remove
            var removalResponse = await RemoveUserFromOutlineServer(targetUsers, users, group);
            statusCodes.AddRange(removalResponse);

            // Add
            foreach (var username in targetUsers)
                tasks.Add(AddUserToOutlineServer(username, users.UserDict[username].DataLimitInBytes, users.UserDict[username].Credentials, group, cancellationToken));

            while (tasks.Count > 0)
            {
                var finishedTask = await Task.WhenAny(tasks);
                statusCodes.AddRange(await finishedTask);
                tasks.Remove(finishedTask);
            }

            return 0;
        }

        /// <summary>
        /// Adds the user to the Outline server.
        /// Updates local storage with the new access key.
        /// </summary>
        /// <param name="username">Target username.</param>
        /// <param name="userDataLimit">The user's data limit.</param>
        /// <returns>The HTTP status codes from API operations.</returns>
        private async Task<HttpStatusCode[]> AddUserToOutlineServer(string username, ulong userDataLimit, Dictionary<string, Credential?> credentials, string group, CancellationToken cancellationToken = default)
        {
            if (OutlineApiKey == null)
                throw new InvalidOperationException("Outline API key is not found.");
            if (_apiClient == null)
                _apiClient = new(OutlineApiKey);
            if (OutlineAccessKeys == null)
                OutlineAccessKeys = new();

            var statusCodes = new List<HttpStatusCode>();

            // Create
            var response = await _apiClient.CreateAccessKeyAsync(cancellationToken);
            var accessKey = await HttpContentJsonExtensions.ReadFromJsonAsync<AccessKey>(response.Content, Outline.Utilities.commonJsonDeserializerOptions, cancellationToken);
            if (accessKey == null)
                throw new Exception("An error occurred while creating the user.");

            // Set username
            var setNameResponge = await _apiClient.SetAccessKeyNameAsync(accessKey.Id, username, cancellationToken);
            statusCodes.Add(setNameResponge.StatusCode);

            // Set data limit
            var dataLimit = PerUserDataLimitInBytes;
            if (dataLimit == 0)
                dataLimit = userDataLimit;
            if (dataLimit != 0)
            {
                var setLimitResponse = await _apiClient.SetAccessKeyDataLimitAsync(accessKey.Id, userDataLimit, cancellationToken);
                statusCodes.Add(setLimitResponse.StatusCode);
            }

            // Save the new key to access key list
            accessKey.Name = username;
            var index = OutlineAccessKeys.IndexOf(accessKey);
            if (index != -1)
                OutlineAccessKeys[index] = accessKey;
            else
                OutlineAccessKeys.Add(accessKey);

            // Save the new key to user credential dictionary
            credentials[group] = new(accessKey.Method, accessKey.Password);

            return statusCodes.ToArray();
        }

        /// <summary>
        /// Removes the listed users from the Outline server.
        /// Removes the associated credentials from local storage.
        /// </summary>
        /// <param name="usernames">Target user.</param>
        /// <returns>The HTTP status codes from API operations.</returns>
        private Task<HttpStatusCode[]> RemoveUserFromOutlineServer(IEnumerable<string> usernames, Users users, string group, CancellationToken cancellationToken = default)
        {
            if (OutlineApiKey == null || OutlineAccessKeys == null)
                throw new InvalidOperationException("Outline API key is not found.");
            if (_apiClient == null)
                _apiClient = new(OutlineApiKey);

            // Get ID list
            var userIDs = OutlineAccessKeys.Where(x => usernames.Contains(x.Name)).Select(x => x.Id);

            // Remove
            var tasks = userIDs.Select(async x => (await _apiClient.DeleteAccessKeyAsync(x, cancellationToken)).StatusCode);
            var result = Task.WhenAll(tasks);

            // Remove from access key list
            OutlineAccessKeys.RemoveAll(x => userIDs.Contains(x.Id));

            // Remove credentials from user credential dictionary
            foreach (var username in usernames)
                users.RemoveCredentialFromUser(username, group);

            return result;
        }
    }
}
