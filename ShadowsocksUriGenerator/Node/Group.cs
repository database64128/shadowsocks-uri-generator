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
        /// -3 when applying data limit failed.
        /// Only the return value of -3 is accompanied by an error message.
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
                _ = await SetOutlineDefaultUser(globalDefaultUser, cancellationToken); // silently ignore the failure
            }

            if (applyDataLimit)
            {
                var result = await _apiClient.SetDataLimitAsync(PerUserDataLimitInBytes, cancellationToken);
                if (!result.IsSuccessStatusCode)
                    return (-3, await result.Content.ReadAsStringAsync(cancellationToken));
            }

            return (0, null);
        }

        /// <summary>
        /// Sets and applies the Outline server default user setting.
        /// The response may be 404 if the admin key doesn't exist.
        /// </summary>
        /// <param name="defaultUser">The default username for access key id 0.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <returns>The task that represents the asynchronous operation.</returns>
        public async Task<HttpResponseMessage> SetOutlineDefaultUser(string defaultUser, CancellationToken cancellationToken = default)
        {
            if (OutlineApiKey is null)
                throw new InvalidOperationException("Outline API key is not found.");
            _apiClient ??= new(OutlineApiKey);

            var responseMessage = await _apiClient.SetAccessKeyNameAsync("0", defaultUser, cancellationToken);
            if (responseMessage.IsSuccessStatusCode)
                OutlineDefaultUser = defaultUser;

            return responseMessage;
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
        public async Task<string?> SetOutlineServer(string name, string hostname, int? port, bool? metrics, string defaultUser, CancellationToken cancellationToken = default)
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
            OutlineAccessKeys ??= new();
            _apiClient ??= new(OutlineApiKey);

            // Filter out
            // a list of users to create
            // a list of access keys to remove
            // a list of access keys to update data limit
            // a list of access keys to remove data limit
            var existingUsernamesOnOutlineServer = OutlineAccessKeys.Select(x => x.Name);
            var existingAccessKeysOnOutlineServerWithDataLimit = OutlineAccessKeys.Where(x => x.DataLimit is not null);
            var existingUsersInGroup = users.UserDict.Where(x => x.Value.Memberships.ContainsKey(group));
            var existingUsersInGroupWithDataLimit = existingUsersInGroup.Where(x => x.Value.Memberships[group].DataLimitInBytes > 0UL);
            var existingUsersInGroupWithNoDataLimit = existingUsersInGroup.Where(x => x.Value.Memberships[group].DataLimitInBytes == 0UL);
            var existingUsernamesInGroup = existingUsersInGroup.Select(x => x.Key);
            var existingUsernamesInGroupWithNoDataLimit = existingUsersInGroupWithNoDataLimit.Select(x => x.Key);

            var usersToCreate = existingUsersInGroup.Where(x => !existingUsernamesOnOutlineServer.Contains(x.Key));
            var accessKeysToRemove = OutlineAccessKeys.Where(x => !existingUsernamesInGroup.Contains(x.Name));
            var accessKeysToUpdateDataLimit = OutlineAccessKeys.SelectMany(accessKey => existingUsersInGroupWithDataLimit.Where(userEntry => userEntry.Key == accessKey.Name && userEntry.Value.Memberships[group].DataLimitInBytes != (accessKey.DataLimit?.Bytes ?? 0UL)).Select(userEntry => (accessKey, userEntry.Value.Memberships[group].DataLimitInBytes)));
            var accessKeysToRemoveDataLimit = existingAccessKeysOnOutlineServerWithDataLimit.Where(x => existingUsernamesInGroupWithNoDataLimit.Contains(x.Name));

            var tasks = new List<Task<string?>>();
            var errMsgSB = new StringBuilder();

            // Per-user data limit
            tasks.Add(ApplyPerUserDataLimitToOutlineServer(group, cancellationToken));

            // Add
            tasks.AddRange(usersToCreate.Select(userEntry => AddUserToOutlineServer(userEntry.Key, userEntry.Value, group, cancellationToken)));

            // Remove
            tasks.AddRange(accessKeysToRemove.Select(accessKey => RemoveUserFromOutlineServer(accessKey, users, group, cancellationToken)));

            // Update data limit
            tasks.AddRange(accessKeysToUpdateDataLimit.Select(x => SetAccessKeyDataLimitOnOutlineServer(x.accessKey, x.DataLimitInBytes, group, cancellationToken)));

            // Remove data limit
            tasks.AddRange(accessKeysToRemoveDataLimit.Select(accessKey => DeleteAccessKeyDataLimitOnOutlineServer(accessKey, group, cancellationToken)));

            var errMsgs = await Task.WhenAll(tasks);
            foreach (var errMsg in errMsgs)
            {
                if (errMsg is not null)
                {
                    errMsgSB.AppendLine(errMsg);
                }
            }

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

            // Filter out access keys that are linked to a user.
            // This is important because later we need to access UserDict with key names.
            var accessKeysLinkedToUser = OutlineAccessKeys.Where(x => users.UserDict.ContainsKey(x.Name));
            var targetAccessKeys = usernames.Length > 0
                ? accessKeysLinkedToUser.Where(x => usernames.Contains(x.Name))
                : accessKeysLinkedToUser;

            var errMsgSB = new StringBuilder();

            // Remove
            var removalTasks = targetAccessKeys.Select(async accessKey => await RemoveUserFromOutlineServer(accessKey, users, group, cancellationToken));
            var removalErrMsgs = await Task.WhenAll(removalTasks);
            foreach (var errMsg in removalErrMsgs)
            {
                if (errMsg is not null)
                {
                    errMsgSB.AppendLine(errMsg);
                }
            }

            // Add
            var addTasks = targetAccessKeys.Select(async accessKey => await AddUserToOutlineServer(accessKey.Name, users.UserDict[accessKey.Name], group, cancellationToken));
            var addErrMsgs = await Task.WhenAll(addTasks);
            foreach (var errMsg in addErrMsgs)
            {
                if (errMsg is not null)
                {
                    errMsgSB.AppendLine(errMsg);
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
        /// An optional error message.
        /// At any stage of the operation, if an error occurs, the function immediately returns an error message.
        /// Null if no errors had ever occurred.
        /// </returns>
        private async Task<string?> AddUserToOutlineServer(string username, User user, string group, CancellationToken cancellationToken = default)
        {
            if (OutlineApiKey is null)
                throw new InvalidOperationException("Outline API key is not found.");
            _apiClient ??= new(OutlineApiKey);
            OutlineAccessKeys ??= new();

            // Create
            var response = await _apiClient.CreateAccessKeyAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
                return $"Error when creating user for {username} on group {group}'s Outline server: {await response.Content.ReadAsStringAsync(cancellationToken)}";

            // Deserialize access key
            var accessKey = await HttpContentJsonExtensions.ReadFromJsonAsync<AccessKey>(response.Content, Outline.Utilities.commonJsonDeserializerOptions, cancellationToken);
            if (accessKey is null)
                throw new Exception("An error occurred while creating the user.");

            // Set username
            var setNameResponse = await _apiClient.SetAccessKeyNameAsync(accessKey.Id, username, cancellationToken);
            if (!setNameResponse.IsSuccessStatusCode)
                return $"Error when setting username for user {username} on group {group}'s Outline server: {await setNameResponse.Content.ReadAsStringAsync(cancellationToken)}";
            accessKey.Name = username;

            // Set data limit
            var dataLimit = user.GetDataLimitInGroup(group);
            if (dataLimit > 0UL)
            {
                var setLimitResponse = await _apiClient.SetAccessKeyDataLimitAsync(accessKey.Id, dataLimit, cancellationToken);
                if (!setLimitResponse.IsSuccessStatusCode)
                    return $"Error when setting data limit for user {username} on group {group}'s Outline server: {await setLimitResponse.Content.ReadAsStringAsync(cancellationToken)}";
                accessKey.DataLimit = new(dataLimit);
            }

            // Save the new key to access key list
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

            return null;
        }

        /// <summary>
        /// Removes the access key from the Outline server.
        /// Removes the associated credential from local storage.
        /// </summary>
        /// <param name="accessKey">The access key to be deleted.</param>
        /// <param name="users">The <see cref="Users"/> object.</param>
        /// <param name="group">Target group.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <returns>
        /// An optional error message.
        /// At any stage of the operation, if an error occurs, the function immediately returns an error message.
        /// Null if no errors had ever occurred.
        /// </returns>
        private async Task<string?> RemoveUserFromOutlineServer(AccessKey accessKey, Users users, string group, CancellationToken cancellationToken = default)
        {
            if (OutlineApiKey is null || OutlineAccessKeys is null)
                throw new InvalidOperationException("Outline API key is not found.");
            _apiClient ??= new(OutlineApiKey);

            // Remove
            var response = await _apiClient.DeleteAccessKeyAsync(accessKey.Id, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return $"Error when removing key of user {accessKey.Name} from group {group}: {await response.Content.ReadAsStringAsync(cancellationToken)}";

            // Remove from access key list
            OutlineAccessKeys.Remove(accessKey);

            // Remove credential from local storage
            _ = users.RemoveCredentialFromUser(accessKey.Name, group);

            return null;
        }

        /// <summary>
        /// Applies the group's per-user data limit
        /// to the linked Outline server.
        /// Updates <see cref="OutlineServerInfo"/>.
        /// </summary>
        /// <param name="group">The group name. Used in the returned error message.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <returns>
        /// An optional error message.
        /// At any stage of the operation, if an error occurs, the function immediately returns an error message.
        /// Null if no errors had ever occurred.
        /// </returns>
        public async Task<string?> ApplyPerUserDataLimitToOutlineServer(string group, CancellationToken cancellationToken = default)
        {
            if (OutlineApiKey is null)
                throw new InvalidOperationException("Outline API key is not found.");
            if (OutlineServerInfo is null)
                throw new InvalidOperationException("Outline server information is not found.");
            _apiClient ??= new(OutlineApiKey);

            if (PerUserDataLimitInBytes == 0UL && OutlineServerInfo.AccessKeyDataLimit is not null)
            {
                // delete data limit from server
                var responseMessage = await _apiClient.DeleteDataLimitAsync(cancellationToken);
                if (!responseMessage.IsSuccessStatusCode)
                    return $"Error when deleting per-user data limit from group {group}'s Outline server: {await responseMessage.Content.ReadAsStringAsync(cancellationToken)}";

                OutlineServerInfo.AccessKeyDataLimit = null;
            }
            else if (PerUserDataLimitInBytes != (OutlineServerInfo.AccessKeyDataLimit?.Bytes ?? 0UL))
            {
                // update server data limit
                var responseMessage = await _apiClient.SetDataLimitAsync(PerUserDataLimitInBytes, cancellationToken);
                if (!responseMessage.IsSuccessStatusCode)
                    return $"Error when applying per-user data limit {Utilities.HumanReadableDataString(PerUserDataLimitInBytes)} to group {group}'s Outline server: {await responseMessage.Content.ReadAsStringAsync(cancellationToken)}";

                OutlineServerInfo.AccessKeyDataLimit = new(PerUserDataLimitInBytes);
            }

            return null;
        }

        /// <summary>
        /// Sets a custom data limit on the specified access key.
        /// Applies the change to Outline server and updates the
        /// corresponding local entry.
        /// </summary>
        /// <param name="accessKey">The access key to apply the custom data limit on.</param>
        /// <param name="dataLimitInBytes">The custom data limit in bytes.</param>
        /// <param name="group">The group name. Used in the returned error message.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <returns>
        /// An optional error message.
        /// At any stage of the operation, if an error occurs, the function immediately returns an error message.
        /// Null if no errors had ever occurred.
        /// </returns>
        public async Task<string?> SetAccessKeyDataLimitOnOutlineServer(AccessKey accessKey, ulong dataLimitInBytes, string group, CancellationToken cancellationToken = default)
        {
            if (OutlineApiKey is null)
                throw new InvalidOperationException("Outline API key is not found.");
            _apiClient ??= new(OutlineApiKey);

            var responseMessage = await _apiClient.SetAccessKeyDataLimitAsync(accessKey.Id, dataLimitInBytes, cancellationToken);
            if (!responseMessage.IsSuccessStatusCode)
                return $"Error when applying the custom data limit {Utilities.HumanReadableDataString(dataLimitInBytes)} to user {accessKey.Name}'s access key on group {group}'s Outline server: {await responseMessage.Content.ReadAsStringAsync(cancellationToken)}";

            accessKey.DataLimit = new(dataLimitInBytes);

            return null;
        }

        /// <summary>
        /// Deletes the specified access key's custom data limit.
        /// Applies the change to Outline server and updates the
        /// corresponding local entry.
        /// </summary>
        /// <param name="accessKey">The access key to apply the custom data limit on.</param>
        /// <param name="group">The group name. Used in the returned error message.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <returns>
        /// An optional error message.
        /// At any stage of the operation, if an error occurs, the function immediately returns an error message.
        /// Null if no errors had ever occurred.
        /// </returns>
        public async Task<string?> DeleteAccessKeyDataLimitOnOutlineServer(AccessKey accessKey, string group, CancellationToken cancellationToken = default)
        {
            if (OutlineApiKey is null)
                throw new InvalidOperationException("Outline API key is not found.");
            _apiClient ??= new(OutlineApiKey);

            var responseMessage = await _apiClient.DeleteAccessKeyDataLimitAsync(accessKey.Id, cancellationToken);
            if (!responseMessage.IsSuccessStatusCode)
                return $"Error when deleting the custom data limit from user {accessKey.Name}'s access key on group {group}'s Outline server: {await responseMessage.Content.ReadAsStringAsync(cancellationToken)}";

            accessKey.DataLimit = null;

            return null;
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
