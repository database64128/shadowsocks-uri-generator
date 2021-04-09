using ShadowsocksUriGenerator.Outline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
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
    public class Group : IDisposable, IDataUsage, IDataLimit
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
        /// Gets or sets the default user for Outline server's default access key (id: 0).
        /// </summary>
        public string? OutlineDefaultUser { get; set; }

        /// <summary>
        /// Gets or sets the global data limit of the group in bytes.
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
        private bool disposedValue;

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
        /// Removes the node from <see cref="NodeDict"/>.
        /// </summary>
        /// <param name="node">The node to be removed.</param>
        /// <returns>
        /// <see cref="true"/> if the node is successfully found and removed.
        /// Otherwise <see cref="false"/>, including not already in the group.
        /// </returns>
        public bool RemoveNode(string node) => NodeDict.Remove(node);

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
        /// Activates all nodes in this group.
        /// </summary>
        public void ActivateAllNodes()
        {
            foreach (var nodeEntry in NodeDict)
                nodeEntry.Value.Deactivated = false;
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
        /// Deactivates all nodes in this group.
        /// </summary>
        public void DeactivateAllNodes()
        {
            foreach (var nodeEntry in NodeDict)
                nodeEntry.Value.Deactivated = true;
        }

        /// <summary>
        /// Calculates the group's total data usage
        /// with statistics from Outline server.
        /// </summary>
        public void CalculateTotalDataUsage()
        {
            BytesUsed = OutlineDataUsage?.BytesTransferredByUserId.Values.Aggregate(0UL, (x, y) => x + y) ?? 0UL;
            UpdateDataRemaining();
        }

        /// <summary>
        /// Updates the data remaining counter
        /// with respect to data limit and data used.
        /// Call this method when updating data limit and data used.
        /// </summary>
        public void UpdateDataRemaining()
        {
            BytesRemaining = DataLimitInBytes > 0UL ? DataLimitInBytes - BytesUsed : 0UL;
        }

        /// <summary>
        /// Gets all data usage records of the group.
        /// </summary>
        /// <returns>A list of data usage records as tuples.</returns>
        public List<(string username, ulong bytesUsed, ulong bytesRemaining)> GetDataUsage()
        {
            List<(string username, ulong bytesUsed, ulong bytesRemaining)> result = new();

            if (OutlineAccessKeys is null || OutlineDataUsage is null)
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
        /// Associates the Outline server
        /// by adding the API key.
        /// </summary>
        /// <param name="apiKey">The Outline server API key.</param>
        /// <param name="globalDefaultUser">The global default user setting.</param>
        /// <param name="applyDataLimit">Whether to apply the per-user data limit.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <returns>
        /// A ValueTuple of the result value and an optional error message.
        /// 0 for success.
        /// -2 for invalid JSON string.
        /// -3 when applying default user failed.
        /// -4 when applying data limit failed.
        /// Only return values of -3 and -4 have an error message.
        /// </returns>
        public async Task<(int result, string? errMsg)> AssociateOutlineServer(string apiKey, string? globalDefaultUser = null, bool applyDataLimit = true, CancellationToken cancellationToken = default)
        {
            try
            {
                OutlineApiKey = JsonSerializer.Deserialize<ApiKey>(apiKey, Outline.Utilities.apiKeyJsonSerializerOptions);
            }
            catch (JsonException)
            {
                return (-2, null);
            }

            if (OutlineApiKey is null)
                return (-2, null);
            _apiClient?.Dispose();
            _apiClient = new(OutlineApiKey);

            if (!string.IsNullOrEmpty(globalDefaultUser))
            {
                var result = await SetOutlineDefaultUser(globalDefaultUser, cancellationToken);
                if (!result.IsSuccessStatusCode)
                    return (-3, await result.Content.ReadAsStringAsync(cancellationToken));
            }

            if (applyDataLimit)
            {
                var result = await _apiClient.SetDataLimitAsync(PerUserDataLimitInBytes, cancellationToken);
                if (!result.IsSuccessStatusCode)
                    return (-4, await result.Content.ReadAsStringAsync(cancellationToken));
            }

            return (0, null);
        }

        /// <summary>
        /// Sets and applies the Outline server default user setting.
        /// </summary>
        /// <param name="defaultUser">The default username for access key id 0.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <returns>The task that represents the asynchronous operation.</returns>
        public Task<HttpResponseMessage> SetOutlineDefaultUser(string defaultUser, CancellationToken cancellationToken = default)
        {
            if (OutlineApiKey is null)
                throw new InvalidOperationException("Outline API key is not found.");
            _apiClient ??= new(OutlineApiKey);

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
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <returns>The task that represents the operation. An optional error message.</returns>
        public async Task<string?> SetOutlineServer(string? name, string? hostname, int? port, bool? metrics, string? defaultUser, CancellationToken cancellationToken = default)
        {
            if (OutlineApiKey is null)
                return "Error: the group is not linked to any Outline server.";
            _apiClient ??= new(OutlineApiKey);

            var tasks = new List<Task<HttpResponseMessage>>();
            var errMsgSB = new StringBuilder();

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

                if (!responseMessage.IsSuccessStatusCode)
                {
                    errMsgSB.AppendLine(await responseMessage.Content.ReadAsStringAsync(cancellationToken));
                }

                tasks.Remove(finishedTask);
            }

            OutlineServerInfo = await _apiClient.GetServerInfoAsync(cancellationToken);

            if (errMsgSB.Length > 0)
                return errMsgSB.ToString();
            else
                return null;
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
            OutlineDefaultUser = null;
        }

        /// <summary>
        /// Pulls server information, access keys, and data usage
        /// from the associated Outline server.
        /// Optionally updates user credential dictionary
        /// in the local storage.
        /// </summary>
        /// <param name="group">Group name.</param>
        /// <param name="users">The <see cref="Users"/> object.</param>
        /// <param name="updateLocalCredentials">
        /// Whether to update user credential dictionary.
        /// Defaults to true.
        /// </param>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <returns>0 on success. -2 when no associated Outline server.</returns>
        public async Task<int> PullFromOutlineServer(string group, Users users, bool updateLocalCredentials = true, CancellationToken cancellationToken = default)
        {
            if (OutlineApiKey is null)
                return -2;
            _apiClient ??= new(OutlineApiKey);

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
            if (OutlineAccessKeys is null)
                return;

            foreach (var userEntry in users.UserDict)
            {
                var filteredUserAccessKeys = OutlineAccessKeys.Where(x => x.Name == userEntry.Key);
                var userInGroup = userEntry.Value.Memberships.TryGetValue(group, out var memberInfo);

                if (filteredUserAccessKeys.Any()) // user has Outline access key
                {
                    var userAccessKey = filteredUserAccessKeys.First();
                    if (userInGroup) // user is in group, update credential
                    {
                        memberInfo!.Method = userAccessKey.Method;
                        memberInfo.Password = userAccessKey.Password;
                        if (userAccessKey.DataLimit is DataLimit dataLimit)
                            memberInfo.DataLimitInBytes = dataLimit.Bytes;
                    }
                    else // not in group, add to group
                    {
                        userEntry.Value.Memberships[group] = new(userAccessKey.Method, userAccessKey.Password, userAccessKey.DataLimit?.Bytes ?? 0UL);
                    }
                }
                else // user has no access key, clear local credential
                {
                    memberInfo?.ClearCredential();
                }
            }
        }

        /// <summary>
        /// Deploys local user configurations to the Outline server.
        /// </summary>
        /// <param name="group">Target group.</param>
        /// <param name="users">The <see cref="Users"/> object.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <returns>
        /// The task that represents the operation.
        /// An optional error message.
        /// </returns>
        public async Task<string?> DeployToOutlineServer(string group, Users users, CancellationToken cancellationToken = default)
        {
            if (OutlineApiKey is null)
                return "Error: the group is not linked to any Outline server.";
            if (OutlineAccessKeys is null)
                await PullFromOutlineServer(group, users, true, cancellationToken);
            if (OutlineServerInfo is null)
                throw new InvalidOperationException("Outline server information is not found.");
            OutlineAccessKeys ??= new();
            _apiClient ??= new(OutlineApiKey);

            var tasks = new List<Task>();
            var errMsgSB = new StringBuilder();

            // Per-user data limit
            if (OutlineServerInfo.AccessKeyDataLimit is null && PerUserDataLimitInBytes > 0UL)
            {
                // apply data limit to server
                tasks.Add(_apiClient.SetDataLimitAsync(PerUserDataLimitInBytes, cancellationToken));
            }
            else if (OutlineServerInfo.AccessKeyDataLimit is not null && PerUserDataLimitInBytes == 0UL)
            {
                // delete data limit from server
                tasks.Add(_apiClient.DeleteDataLimitAsync(cancellationToken));
            }
            else if (OutlineServerInfo.AccessKeyDataLimit is DataLimit perUserDataLimit && perUserDataLimit.Bytes != PerUserDataLimitInBytes)
            {
                // update server data limit
                tasks.Add(_apiClient.SetDataLimitAsync(PerUserDataLimitInBytes, cancellationToken));
            }

            // Filter out a list of users to create
            // and another list of users to remove
            var existingUsersOnOutlineServer = OutlineAccessKeys.Select(x => x.Name);
            var existingUsersInGroup = users.UserDict.Where(x => x.Value.Memberships.ContainsKey(group)).Select(x => x.Key);
            var usersToCreate = existingUsersInGroup.Except(existingUsersOnOutlineServer);
            var usersToRemove = existingUsersOnOutlineServer.Except(existingUsersInGroup);

            // Add
            foreach (var username in usersToCreate)
                tasks.Add(AddUserToOutlineServer(username, users.UserDict[username], group, cancellationToken));

            // Remove
            tasks.Add(RemoveUserFromOutlineServer(usersToRemove, users, group, cancellationToken));

            while (tasks.Count > 0)
            {
                var finishedTask = await Task.WhenAny(tasks);

                switch (finishedTask)
                {
                    case Task<HttpResponseMessage> dataLimitTask:
                        {
                            var responseMessage = await dataLimitTask;
                            if (!responseMessage.IsSuccessStatusCode)
                            {
                                var responseStr = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
                                errMsgSB.AppendLine($"Error when applying data limit to group {group}: {responseStr}");
                            }

                            break;
                        }

                    case Task<(string username, string group, string? errMsg)> addUserTask:
                        {
                            (var u, var g, var e) = await addUserTask;
                            if (!string.IsNullOrEmpty(e))
                            {
                                errMsgSB.AppendLine($"Error when adding user {u} to group {g}: {e}");
                            }

                            break;
                        }

                    case Task<(string username, HttpResponseMessage)[]> removeUserTask:
                        {
                            var results = await removeUserTask;
                            foreach ((var u, var responseMessage) in results)
                            {
                                if (!responseMessage.IsSuccessStatusCode)
                                {
                                    var responseStr = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
                                    errMsgSB.AppendLine($"Error when removing key of user {u}: {responseStr}");
                                }
                            }

                            break;
                        }
                }

                tasks.Remove(finishedTask);
            }

            OutlineServerInfo = await _apiClient.GetServerInfoAsync(cancellationToken);

            if (errMsgSB.Length > 0)
                return errMsgSB.ToString();
            else
                return null;
        }

        /// <summary>
        /// Renames a user and syncs with Outline server.
        /// </summary>
        /// <param name="oldName">The old username.</param>
        /// <param name="newName">The new username.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <returns>
        /// 0 on success.
        /// 1 when not an Outline user.
        /// 2 when not on the Outline server.
        /// -3 when an error occurred while sending the request.
        /// </returns>
        public async Task<int> RenameUser(string oldName, string newName, CancellationToken cancellationToken = default)
        {
            if (OutlineApiKey is null || OutlineAccessKeys is null)
                return 1;
            _apiClient ??= new(OutlineApiKey);

            // find user id
            var filteredUserIds = OutlineAccessKeys.Where(x => x.Name == oldName).Select(x => x.Id);
            var userId = filteredUserIds.Any() ? filteredUserIds.First() : null;
            if (userId is null)
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
        /// <param name="group">Target group.</param>
        /// <param name="users">The <see cref="Users"/> object.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <param name="usernames">Only target these members in group if specified.</param>
        /// <returns>
        /// The task that represents the operation.
        /// An optional error message.
        /// </returns>
        public async Task<string?> RotatePassword(string group, Users users, CancellationToken cancellationToken = default, params string[] usernames)
        {
            if (OutlineApiKey is null)
                return "Error: the group is not linked to any Outline server.";
            if (OutlineAccessKeys is null)
                await PullFromOutlineServer(group, users, true, cancellationToken);
            OutlineAccessKeys ??= new();
            _apiClient ??= new(OutlineApiKey);

            var targetUsers = usernames.Length > 0
                ? usernames
                : users.UserDict.Where(x => x.Value.Memberships.ContainsKey(group)).Select(x => x.Key);

            var errMsgSB = new StringBuilder();

            // Remove
            var removalResponse = await RemoveUserFromOutlineServer(targetUsers, users, group);
            foreach ((var username, var response) in removalResponse)
            {
                if (!response.IsSuccessStatusCode)
                {
                    var responseStr = await response.Content.ReadAsStringAsync(cancellationToken);
                    errMsgSB.AppendLine($"Error when removing key of user {username}: {responseStr}");
                }
            }

            // Add
            var tasks = targetUsers.Select(async username => await AddUserToOutlineServer(username, users.UserDict[username], group, cancellationToken));
            var results = await Task.WhenAll(tasks);
            foreach ((var u, var g, var e) in results)
            {
                if (!string.IsNullOrEmpty(e))
                {
                    errMsgSB.AppendLine($"Error when adding user {u} to group {g}: {e}");
                }
            }

            if (errMsgSB.Length > 0)
                return errMsgSB.ToString();
            else
                return null;
        }

        /// <summary>
        /// Adds the user to the Outline server.
        /// Updates local storage with the new access key.
        /// </summary>
        /// <param name="username">Target username.</param>
        /// <param name="user">Target <see cref="User"/> object.</param>
        /// <param name="group">Target group.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <returns>
        /// A ValueTuple of username, group, and an optional error message.
        /// At any stage of the operation, if an error occurs, the function immediately returns an error message.
        /// Null if no errors had ever occurred.
        /// </returns>
        private async Task<(string username, string group, string? errMsg)> AddUserToOutlineServer(string username, User user, string group, CancellationToken cancellationToken = default)
        {
            if (OutlineApiKey is null)
                throw new InvalidOperationException("Outline API key is not found.");
            _apiClient ??= new(OutlineApiKey);
            OutlineAccessKeys ??= new();

            // Create
            var response = await _apiClient.CreateAccessKeyAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
                return (username, group, await response.Content.ReadAsStringAsync(cancellationToken));

            // Deserialize access key
            var accessKey = await HttpContentJsonExtensions.ReadFromJsonAsync<AccessKey>(response.Content, Outline.Utilities.commonJsonDeserializerOptions, cancellationToken);
            if (accessKey is null)
                throw new Exception("An error occurred while creating the user.");

            // Set username
            var setNameResponse = await _apiClient.SetAccessKeyNameAsync(accessKey.Id, username, cancellationToken);
            if (!setNameResponse.IsSuccessStatusCode)
                return (username, group, await setNameResponse.Content.ReadAsStringAsync(cancellationToken));

            // Set data limit
            var dataLimit = user.GetDataLimitInGroup(group);
            if (dataLimit > 0UL)
            {
                var setLimitResponse = await _apiClient.SetAccessKeyDataLimitAsync(accessKey.Id, dataLimit, cancellationToken);
                if (!setLimitResponse.IsSuccessStatusCode)
                    return (username, group, await setLimitResponse.Content.ReadAsStringAsync(cancellationToken));
            }

            // Save the new key to access key list
            accessKey.Name = username;
            accessKey.DataLimit = new(dataLimit);
            var index = OutlineAccessKeys.IndexOf(accessKey);
            if (index != -1)
                OutlineAccessKeys[index] = accessKey;
            else
                OutlineAccessKeys.Add(accessKey);

            // Update existing member info or create a new one if nonexistent
            // Do not update/save data limit since it could be user per-group limit
            if (user.Memberships.TryGetValue(group, out var memberInfo))
            {
                memberInfo.Method = accessKey.Method;
                memberInfo.Password = accessKey.Password;
            }
            else
            {
                user.Memberships[group] = new(accessKey.Method, accessKey.Password);
            }

            return (username, group, null);
        }

        /// <summary>
        /// Removes the listed users from the Outline server.
        /// Removes the associated credentials from local storage.
        /// </summary>
        /// <param name="usernames">Target user.</param>
        /// <param name="users">The <see cref="Users"/> object.</param>
        /// <param name="group">Target group.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <returns>An array of ValueTuples of access key id and HTTP status code.</returns>
        private Task<(string username, HttpResponseMessage)[]> RemoveUserFromOutlineServer(IEnumerable<string> usernames, Users users, string group, CancellationToken cancellationToken = default)
        {
            if (OutlineApiKey is null || OutlineAccessKeys is null)
                throw new InvalidOperationException("Outline API key is not found.");
            _apiClient ??= new(OutlineApiKey);

            // Filter out a list of access keys to be removed
            var filteredAccessKeys = OutlineAccessKeys.Where(x => usernames.Contains(x.Name));

            // Remove
            var tasks = filteredAccessKeys.Select(async x => (x.Name, await _apiClient.DeleteAccessKeyAsync(x.Id, cancellationToken)));
            var result = Task.WhenAll(tasks);

            // Remove from access key list
            OutlineAccessKeys.RemoveAll(x => filteredAccessKeys.Contains(x));

            // Remove credentials from user credential dictionary
            foreach (var username in usernames)
                users.RemoveCredentialFromUser(username, group);

            return result;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _apiClient?.Dispose();
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
