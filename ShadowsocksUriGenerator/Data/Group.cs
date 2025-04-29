using ShadowsocksUriGenerator.Outline;
using ShadowsocksUriGenerator.Utils;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShadowsocksUriGenerator.Data
{
    /// <summary>
    /// Nodes in a group share the same credential for a user.
    /// They may provide different credentials for different users.
    /// </summary>
    public class Group : IDisposable
    {
        /// <summary>
        /// Gets or sets the Outline Access Keys Management API key object.
        /// </summary>
        public OutlineApiKey? OutlineApiKey { get; set; }

        /// <summary>
        /// Gets or sets the Outline server information object.
        /// </summary>
        public OutlineServerInfo? OutlineServerInfo { get; set; }

        /// <summary>
        /// Gets or sets the list of Outline server access keys.
        /// </summary>
        public List<OutlineAccessKey>? OutlineAccessKeys { get; set; }

        /// <summary>
        /// Gets or sets the Outline server data usage object.
        /// </summary>
        public OutlineDataUsage? OutlineDataUsage { get; set; }

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
        /// Gets the data remaining to be used in bytes.
        /// </summary>
        [JsonIgnore]
        public ulong BytesRemaining => DataLimitInBytes > BytesUsed ? DataLimitInBytes - BytesUsed : 0UL;

        /// <summary>
        /// Gets or sets the group's owner.
        /// </summary>
        public string? OwnerUuid { get; set; }

        /// <summary>
        /// Gets or sets the Shadowsocks Server Management API v1 (SSMv1) server object.
        /// </summary>
        public SSMv1Server? SSMv1Server { get; set; }

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
        public Dictionary<string, Node> NodeDict { get; set; } = [];

        /// <summary>
        /// The Outline API client instance.
        /// </summary>
        private OutlineApiClient? _apiClient;
        private bool disposedValue;

        /// <summary>
        /// Adds a node to <see cref="NodeDict"/>.
        /// </summary>
        /// <param name="name">Node name.</param>
        /// <param name="host">Node's hostname.</param>
        /// <param name="port">Node's port number.</param>
        /// <param name="plugin">Optional. Plugin binary name.</param>
        /// <param name="pluginVersion">Optional. Required plugin version.</param>
        /// <param name="pluginOpts">Optional. Plugin options.</param>
        /// <param name="pluginArguments">Optional. Plugin startup arguments.</param>
        /// <param name="ownerUuid">Optional. Node owner's user UUID.</param>
        /// <param name="tags">Node's tags.</param>
        /// <param name="iPSKs">Node's identity PSKs.</param>
        /// <returns>0 for success. -1 for duplicated name.</returns>
        public int AddNode(string name, string host, int port, string? plugin, string? pluginVersion, string? pluginOpts, string? pluginArguments, string? ownerUuid, string[] tags, string[] iPSKs)
        {
            if (!NodeDict.ContainsKey(name))
            {
                var node = new Node()
                {
                    Host = host,
                    Port = port,
                    Plugin = plugin,
                    PluginVersion = pluginVersion,
                    PluginOpts = pluginOpts,
                    PluginArguments = pluginArguments,
                    OwnerUuid = ownerUuid,
                    Tags = [.. tags],
                    IdentityPSKs = [.. iPSKs],
                };

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
        /// True if the node is successfully found and removed.
        /// Otherwise false, including not already in the group.
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
        public void CalculateTotalDataUsage() =>
            BytesUsed = OutlineDataUsage?.BytesTransferredByUserId.Values.Aggregate(0UL, (x, y) => x + y) ?? 0UL;

        public void AddBytesUsed(ulong bytesUsed) => BytesUsed += bytesUsed;

        public void SubBytesUsed(ulong bytesUsed) => BytesUsed = BytesUsed >= bytesUsed ? BytesUsed - bytesUsed : 0UL;

        /// <summary>
        /// Gets all data usage records of the group.
        /// </summary>
        /// <returns>A list of data usage records as tuples.</returns>
        public List<(string username, ulong bytesUsed, ulong bytesRemaining)> GetDataUsage(string groupName, Users users)
        {
            List<(string username, ulong bytesUsed, ulong bytesRemaining)> result = [];

            if (AddOutlineDataUsage(result))
                return result;

            foreach (KeyValuePair<string, User> userEntry in users.UserDict)
            {
                if (userEntry.Value.Memberships.TryGetValue(groupName, out MemberInfo? memberInfo))
                {
                    result.Add((userEntry.Key, memberInfo.BytesUsed, memberInfo.BytesRemaining));
                }
            }

            return result;
        }

        private bool AddOutlineDataUsage(List<(string username, ulong bytesUsed, ulong bytesRemaining)> result)
        {
            if (OutlineAccessKeys is null || OutlineDataUsage is null)
                return false;

            foreach (OutlineAccessKey accessKey in OutlineAccessKeys)
            {
                if (int.TryParse(accessKey.Id, out int keyId) &&
                    OutlineDataUsage.BytesTransferredByUserId.TryGetValue(keyId, out ulong bytesUsed))
                {
                    ulong dataLimitInBytes = accessKey.DataLimit?.Bytes ?? PerUserDataLimitInBytes;
                    ulong bytesRemaining = dataLimitInBytes > bytesUsed ? dataLimitInBytes - bytesUsed : 0UL;
                    result.Add((accessKey.Name, bytesUsed, bytesRemaining));
                }
            }

            return true;
        }

        public bool AddUserOutlineDataUsage(
            List<(string group, ulong bytesUsed, ulong bytesRemaining)> result,
            string username,
            string groupName)
        {
            if (OutlineAccessKeys is null || OutlineDataUsage is null)
                return false;

            foreach (OutlineAccessKey accessKey in OutlineAccessKeys)
            {
                if (accessKey.Name == username &&
                    int.TryParse(accessKey.Id, out int keyId) &&
                    OutlineDataUsage.BytesTransferredByUserId.TryGetValue(keyId, out ulong bytesUsed))
                {
                    ulong dataLimitInBytes = accessKey.DataLimit?.Bytes ?? PerUserDataLimitInBytes;
                    ulong bytesRemaining = dataLimitInBytes > bytesUsed ? dataLimitInBytes - bytesUsed : 0UL;
                    result.Add((groupName, bytesUsed, bytesRemaining));
                }
            }

            return true;
        }

        /// <summary>
        /// Associates the Outline server with the group.
        /// Saves the API key. Pulls from the Outline server.
        /// Optionally sets the admin key username.
        /// Optionally sets the per-user data limit.
        /// </summary>
        /// <param name="group">Group name.</param>
        /// <param name="apiKey">The Outline server API key.</param>
        /// <param name="users">The <see cref="Users"/> object.</param>
        /// <param name="httpClient">An instance of generic HTTP client to use when creating Outline API client.</param>
        /// <param name="globalDefaultUser">The global default user setting.</param>
        /// <param name="applyDataLimit">Whether to apply the per-user data limit.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <returns>
        /// The task that represents the operation.
        /// An optional error message.
        /// </returns>
        public async Task<string?> AssociateOutlineServer(string group, string apiKey, Users users, HttpClient httpClient, string? globalDefaultUser = null, bool applyDataLimit = true, CancellationToken cancellationToken = default)
        {
            try
            {
                OutlineApiKey = JsonSerializer.Deserialize(apiKey, OutlineJsonSerializerContext.Default.OutlineApiKey);
            }
            catch (JsonException)
            {
                return "Error: Invalid API key: deserialization failed.";
            }

            if (OutlineApiKey is null)
                return "Error: Invalid API key: deserialization returned null.";

            // Validate CertSha256
            if (!string.IsNullOrEmpty(OutlineApiKey.CertSha256) && OutlineApiKey.CertSha256.Length != 64)
                return "Error: Malformed CertSha256: length is not 64.";

            // Clean up potential leftover API client
            _apiClient?.Dispose();
            _apiClient = new(httpClient, OutlineApiKey);

            // Pull from Outline server without syncing with local user db
            await foreach (Task task in PullFromOutlineServer(group, users, httpClient, cancellationToken))
            {
                await task;
            }

            // Apply default username
            if (!string.IsNullOrEmpty(globalDefaultUser))
            {
                var defaultUserErrMsg = await SetOutlineDefaultUser(globalDefaultUser, httpClient, cancellationToken);
                if (defaultUserErrMsg is not null)
                    return defaultUserErrMsg;
            }

            // Apply per-user data limit
            if (applyDataLimit)
            {
                await ApplyPerUserDataLimitToOutlineServer(group, httpClient, cancellationToken);
            }

            // Sync with local user db
            UpdateLocalUserMemberships(group, users);

            return null;
        }

        /// <summary>
        /// Gets the linked Outline server's admin key username.
        /// </summary>
        /// <returns>
        /// A string if the admin key exists.
        /// The string may be an empty string.
        /// Null if the admin key doesn't exist.
        /// </returns>
        public string? GetOutlineDefaultUser()
        {
            if (OutlineAccessKeys is null)
                return null;

            var adminKey = OutlineAccessKeys.Where(x => x.Id == "0");

            if (adminKey.Any())
                return adminKey.First().Name;
            else
                return null;
        }

        /// <summary>
        /// Checks if the admin key exists on the Outline server.
        /// If it does, set a username on it.
        /// Otherwise, do nothing and don't return error.
        /// Make sure you have pulled from Outline server before calling this method.
        /// </summary>
        /// <param name="defaultUser">The default username for access key id 0.</param>
        /// <param name="httpClient">An instance of generic HTTP client to use when creating Outline API client.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <returns>The task that represents the operation. An optional error message.</returns>
        public async Task<string?> SetOutlineDefaultUser(string defaultUser, HttpClient httpClient, CancellationToken cancellationToken = default)
        {
            if (OutlineApiKey is null)
                throw new InvalidOperationException("Outline API key is not found.");

            _apiClient ??= new(httpClient, OutlineApiKey);

            try
            {
                var currentDefaultUser = GetOutlineDefaultUser();
                if (currentDefaultUser is not null && currentDefaultUser != defaultUser)
                {
                    var responseMessage = await _apiClient.SetAccessKeyNameAsync("0", defaultUser, cancellationToken);
                    if (!responseMessage.IsSuccessStatusCode)
                        return $"Error when setting admin key username to Outline server: {await responseMessage.Content.ReadAsStringAsync(cancellationToken)}";
                }
            }
            catch (OperationCanceledException ex) when (ex.InnerException is not TimeoutException) // canceled
            {
                return ex.Message;
            }
            catch (Exception ex) // timeout and other errors
            {
                return $"Error when setting admin key username to Outline server: {ex.Message}";
            }

            return null;
        }

        /// <summary>
        /// Changes settings for the associated Outline server.
        /// </summary>
        /// <param name="group">Group name.</param>
        /// <param name="name">Server name.</param>
        /// <param name="hostname">Server hostname.</param>
        /// <param name="port">Port number for new access keys.</param>
        /// <param name="metrics">Enable telemetry.</param>
        /// <param name="defaultUser">The default username for access key id 0.</param>
        /// <param name="httpClient">An instance of generic HTTP client to use when creating Outline API client.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <returns>The task that represents the operation. An optional error message.</returns>
        public async Task<string?> SetOutlineServer(string group, string? name, string? hostname, int? port, bool? metrics, string? defaultUser, HttpClient httpClient, CancellationToken cancellationToken = default)
        {
            if (OutlineApiKey is null)
                return $"Error: Group {group} is not linked to any Outline server.";

            _apiClient ??= new(httpClient, OutlineApiKey);

            var tasks = new List<Task<HttpResponseMessage>>();
            var errMsgSB = new StringBuilder();

            try
            {
                if (!string.IsNullOrEmpty(name))
                    tasks.Add(_apiClient.SetServerNameAsync(name, cancellationToken));
                if (!string.IsNullOrEmpty(hostname))
                    tasks.Add(_apiClient.SetServerHostnameAsync(hostname, cancellationToken));
                if (port is int portForNewAccessKeys)
                    tasks.Add(_apiClient.SetAccessKeysPortAsync(portForNewAccessKeys, cancellationToken));
                if (metrics is bool enableMetrics)
                    tasks.Add(_apiClient.SetServerMetricsAsync(enableMetrics, cancellationToken));
                if (!string.IsNullOrEmpty(defaultUser))
                {
                    var currentDefaultUser = GetOutlineDefaultUser();
                    if (currentDefaultUser is null)
                        errMsgSB.AppendLine("Warning: the admin key doesn't exist. Skipping.");
                    else if (currentDefaultUser == defaultUser)
                        errMsgSB.AppendLine($"Warning: the default user is already {defaultUser}. Skipping.");
                    else
                        tasks.Add(_apiClient.SetAccessKeyNameAsync("0", defaultUser, cancellationToken));
                }

                await foreach (var finishedTask in Task.WhenEach(tasks))
                {
                    var responseMessage = await finishedTask;
                    if (!responseMessage.IsSuccessStatusCode)
                    {
                        errMsgSB.AppendLine($"Error when applying settings to Outline server: {await responseMessage.Content.ReadAsStringAsync(cancellationToken)}");
                    }
                }

                OutlineServerInfo = await _apiClient.GetServerInfoAsync(cancellationToken);
            }
            catch (OperationCanceledException ex) when (ex.InnerException is not TimeoutException) // canceled
            {
                errMsgSB.AppendLine(ex.Message);
            }
            catch (Exception ex) // timeout and other errors
            {
                errMsgSB.AppendLine($"Error when applying settings to Outline server: {ex.Message}");
            }

            if (errMsgSB.Length > 0)
                return errMsgSB.ToString();
            else
                return null;
        }

        /// <summary>
        /// Removes the association of the Outline server
        /// and all saved data related to it.
        /// </summary>
        public void RemoveOutlineServer(string group, Users users)
        {
            if (OutlineApiKey is not null)
            {
                if (OutlineAccessKeys is not null && OutlineDataUsage is not null)
                {
                    foreach (OutlineAccessKey accessKey in OutlineAccessKeys)
                    {
                        _ = users.ClearGroupBytesUsed(accessKey.Name, group, PerUserDataLimitInBytes);
                    }
                }

                BytesUsed = 0;

                OutlineApiKey = null;
                OutlineServerInfo = null;
                OutlineAccessKeys = null;
                OutlineDataUsage = null;
            }
        }

        /// <summary>
        /// Pulls server information, access keys, and data usage
        /// from the associated Outline server.
        /// Optionally updates user membership dictionary
        /// in the local storage.
        /// </summary>
        /// <param name="group">Group name.</param>
        /// <param name="users">The <see cref="Users"/> object.</param>
        /// <param name="httpClient">An instance of generic HTTP client to use when creating Outline API client.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <returns>
        /// The task that represents the operation.
        /// An optional error message.
        /// </returns>
        public async IAsyncEnumerable<Task> PullFromOutlineServer(string group, Users users, HttpClient httpClient, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (OutlineApiKey is null)
                yield break;

            _apiClient ??= new(httpClient, OutlineApiKey);

            await foreach (Task task in Task.WhenEach(
                PullServerInfoAsync(_apiClient, group, cancellationToken),
                PullAccessKeysAsync(_apiClient, group, cancellationToken),
                PullDataUsageAsync(_apiClient, group, cancellationToken)))
            {
                yield return task;
            }

            UpdateLocalUserMemberships(group, users);
            CalculateTotalDataUsage();
        }

        private async Task PullServerInfoAsync(OutlineApiClient apiClient, string groupName, CancellationToken cancellationToken = default)
        {
            try
            {
                OutlineServerInfo = await apiClient.GetServerInfoAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new GroupApiRequestException(groupName, "Failed to pull server information", ex);
            }
        }

        private async Task PullAccessKeysAsync(OutlineApiClient apiClient, string groupName, CancellationToken cancellationToken = default)
        {
            try
            {
                OutlineAccessKeysResponse? accessKeysResponse = await apiClient.GetAccessKeysAsync(cancellationToken);
                if (accessKeysResponse is not null)
                {
                    OutlineAccessKeys = accessKeysResponse.AccessKeys;
                }
            }
            catch (Exception ex)
            {
                throw new GroupApiRequestException(groupName, "Failed to pull access keys", ex);
            }
        }

        private async Task PullDataUsageAsync(OutlineApiClient apiClient, string groupName, CancellationToken cancellationToken = default)
        {
            try
            {
                OutlineDataUsage = await apiClient.GetDataUsageAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new GroupApiRequestException(groupName, "Failed to pull data usage", ex);
            }
        }

        /// <summary>
        /// Reads information from <see cref="OutlineAccessKeys"/>
        /// and saves to local user membership dictionary.
        /// This method updates membership status, credential, and custom in-group data limit
        /// for each username matched in <see cref="OutlineAccessKeys"/>.
        /// </summary>
        /// <param name="group">Group name.</param>
        /// <param name="users">The <see cref="Users"/> object.</param>
        private void UpdateLocalUserMemberships(string group, Users users)
        {
            if (OutlineAccessKeys is null || OutlineDataUsage is null)
                return;

            // Previously, this method would remove local credentials without
            // a matching access key. We now feel like this is probably not
            // what users want.

            foreach (OutlineAccessKey accessKey in OutlineAccessKeys)
            {
                if (users.UserDict.TryGetValue(accessKey.Name, out User? user))
                {
                    if (user.Memberships.TryGetValue(group, out MemberInfo? memberInfo))
                    {
                        memberInfo.Method = accessKey.Method;
                        memberInfo.Password = accessKey.Password;
                        if (accessKey.DataLimit is OutlineDataLimit dataLimit)
                        {
                            memberInfo.DataLimitInBytes = dataLimit.Bytes;
                        }
                    }
                    else
                    {
                        memberInfo = new(accessKey.Method, accessKey.Password);
                        if (accessKey.DataLimit is OutlineDataLimit dataLimit)
                        {
                            memberInfo.DataLimitInBytes = dataLimit.Bytes;
                        }
                        user.Memberships.Add(group, memberInfo);
                    }

                    if (int.TryParse(accessKey.Id, out int keyId) &&
                        OutlineDataUsage.BytesTransferredByUserId.TryGetValue(keyId, out ulong bytesUsed))
                    {
                        long oldBytesUsed = (long)memberInfo.BytesUsed;
                        long diff = (long)bytesUsed - oldBytesUsed;
                        if (diff > 0)
                        {
                            ulong n = (ulong)diff;
                            user.AddBytesUsed(n);
                            memberInfo.AddBytesUsed(n, PerUserDataLimitInBytes);
                        }
                        else if (diff < 0)
                        {
                            ulong n = (ulong)-diff;
                            user.SubBytesUsed(n);
                            memberInfo.SubBytesUsed(n, PerUserDataLimitInBytes);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Deploys local user configurations to the Outline server.
        /// </summary>
        /// <param name="group">Target group.</param>
        /// <param name="users">The <see cref="Users"/> object.</param>
        /// <param name="httpClient">An instance of generic HTTP client to use when creating Outline API client.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <returns>
        /// An async-enumerable sequence whose elements are error messages.
        /// </returns>
        public async IAsyncEnumerable<Task> DeployToOutlineServer(string group, Users users, HttpClient httpClient, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (OutlineApiKey is null)
            {
                yield break;
            }

            if (OutlineAccessKeys is null)
            {
                await foreach (Task task in PullFromOutlineServer(group, users, httpClient, cancellationToken))
                {
                    yield return task;
                }
            }

            OutlineAccessKeys ??= [];
            _apiClient ??= new(httpClient, OutlineApiKey);

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

            ReadOnlySpan<Task> tasks =
            [
                // Per-user data limit
                ApplyPerUserDataLimitToOutlineServer(group, httpClient, cancellationToken),

                // Add
                .. usersToCreate.Select(userEntry => AddUserToOutlineServer(userEntry.Key, userEntry.Value, group, httpClient, cancellationToken)),

                // Remove
                .. accessKeysToRemove.Select(accessKey => RemoveUserFromOutlineServer(accessKey, users, group, httpClient, cancellationToken)),

                // Update data limit
                .. accessKeysToUpdateDataLimit.Select(x => SetAccessKeyDataLimitOnOutlineServer(x.accessKey, x.DataLimitInBytes, group, httpClient, cancellationToken)),

                // Remove data limit
                .. accessKeysToRemoveDataLimit.Select(accessKey => DeleteAccessKeyDataLimitOnOutlineServer(accessKey, group, httpClient, cancellationToken)),
            ];

            await foreach (Task task in Task.WhenEach(tasks))
            {
                yield return task;
            }
        }

        /// <summary>
        /// Renames a user and syncs with Outline server.
        /// </summary>
        /// <param name="oldName">The old username.</param>
        /// <param name="newName">The new username.</param>
        /// <param name="httpClient">An instance of generic HTTP client to use when creating Outline API client.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <returns>
        /// The task that represents the operation.
        /// An optional error message.
        /// </returns>
        public async Task<string?> RenameUser(string oldName, string newName, HttpClient httpClient, CancellationToken cancellationToken = default)
        {
            if (OutlineApiKey is null || OutlineAccessKeys is null)
                return null;

            _apiClient ??= new(httpClient, OutlineApiKey);

            // Find access key
            var filteredAccessKeys = OutlineAccessKeys.Where(x => x.Name == oldName);
            if (!filteredAccessKeys.Any())
                return $"Error: found no access keys with the name {oldName}.";
            var accessKey = filteredAccessKeys.First();

            try
            {
                // Apply to Outline server
                var response = await _apiClient.SetAccessKeyNameAsync(accessKey.Id, newName, cancellationToken);
                if (response.IsSuccessStatusCode)
                    return $"Error when changing access key username from {oldName} to {newName} on Outline server: {await response.Content.ReadAsStringAsync(cancellationToken)}";

                // Save new name to local access key
                accessKey.Name = newName;
            }
            catch (OperationCanceledException ex) when (ex.InnerException is not TimeoutException) // canceled
            {
                return ex.Message;
            }
            catch (Exception ex) // timeout and other errors
            {
                return $"Error when changing access key username from {oldName} to {newName} on Outline server: {ex.Message}";
            }

            return null;
        }

        /// <summary>
        /// Rotates password for the specified user or all users in the group.
        /// </summary>
        /// <param name="group">Target group.</param>
        /// <param name="users">The <see cref="Users"/> object.</param>
        /// <param name="httpClient">An instance of generic HTTP client to use when creating Outline API client.</param>
        /// <param name="silentlySkipNonOutline">
        /// Set to true to silently return without emitting an error
        /// if this server is not linked to any Outline server.
        /// Defaults to false.
        /// </param>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <param name="usernames">Only target these members in group if specified.</param>
        /// <returns>
        /// An async-enumerable sequence whose elements are error messages.
        /// </returns>
        public async IAsyncEnumerable<Task> RotatePassword(string group, Users users, HttpClient httpClient, bool silentlySkipNonOutline = false, [EnumeratorCancellation] CancellationToken cancellationToken = default, params string[] usernames)
        {
            if (OutlineApiKey is null)
            {
                if (!silentlySkipNonOutline)
                {
                    throw new ArgumentException($"Warning: Group {group} does not support automatic password rotation, because it is not linked to any Outline server. You have to manually change the password if you intend to rotate it.", nameof(group));
                }

                yield break;
            }

            if (OutlineAccessKeys is null)
            {
                await foreach (Task task in PullFromOutlineServer(group, users, httpClient, cancellationToken))
                {
                    await task;
                }
            }

            OutlineAccessKeys ??= [];
            _apiClient ??= new(httpClient, OutlineApiKey);

            // Filter out access keys that are linked to a user.
            // This is important because later we need to access UserDict with key names.
            // The result has to be freezed by calling .ToArray().
            // Otherwise the query result will change after removal.
            var accessKeysLinkedToUser = OutlineAccessKeys.Where(x => users.UserDict.ContainsKey(x.Name));
            if (usernames.Length > 0)
                accessKeysLinkedToUser = accessKeysLinkedToUser.Where(x => usernames.Contains(x.Name));
            OutlineAccessKey[] targetAccessKeys = [.. accessKeysLinkedToUser];

            // Remove
            var removalTasks = targetAccessKeys.Select(accessKey => RemoveUserFromOutlineServer(accessKey, users, group, httpClient, cancellationToken));
            await foreach (Task task in Task.WhenEach(removalTasks))
            {
                await task;
            }

            // Add
            var addTasks = targetAccessKeys.Select(accessKey => AddUserToOutlineServer(accessKey.Name, users.UserDict[accessKey.Name], group, httpClient, cancellationToken));
            await foreach (Task task in Task.WhenEach(addTasks))
            {
                await task;
            }
        }

        /// <summary>
        /// Adds the user to the Outline server.
        /// Updates local storage with the new access key.
        /// </summary>
        /// <param name="username">Target username.</param>
        /// <param name="user">Target <see cref="User"/> object.</param>
        /// <param name="group">Target group.</param>
        /// <param name="httpClient">An instance of generic HTTP client to use when creating Outline API client.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <returns>
        /// An optional error message.
        /// At any stage of the operation, if an error occurs, the function immediately returns an error message.
        /// Null if no errors had ever occurred.
        /// </returns>
        private async Task AddUserToOutlineServer(string username, User user, string group, HttpClient httpClient, CancellationToken cancellationToken = default)
        {
            if (OutlineApiKey is null)
                throw new InvalidOperationException("Outline API key is not found.");

            _apiClient ??= new(httpClient, OutlineApiKey);
            OutlineAccessKeys ??= [];

            // Create
            var response = await _apiClient.CreateAccessKeyAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                string errMsg = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new GroupApiRequestException(group, $"Failed to create user {username} on Outline server: {errMsg}", null);
            }

            // Deserialize access key
            OutlineAccessKey accessKey = await response.Content.ReadFromJsonAsync(OutlineJsonSerializerContext.Default.OutlineAccessKey, cancellationToken)
                ?? throw new GroupApiRequestException(group, $"Deserialized access key for user {username} is null", null);

            // Set username
            var setNameResponse = await _apiClient.SetAccessKeyNameAsync(accessKey.Id, username, cancellationToken);
            if (!setNameResponse.IsSuccessStatusCode)
            {
                string errMsg = await setNameResponse.Content.ReadAsStringAsync(cancellationToken);
                throw new GroupApiRequestException(group, $"Failed to set username for user {username} on Outline server: {errMsg}", null);
            }

            accessKey.Name = username;

            // Set data limit
            var dataLimit = user.GetDataLimitInGroup(group);
            if (dataLimit > 0UL)
            {
                var setLimitResponse = await _apiClient.SetAccessKeyDataLimitAsync(accessKey.Id, dataLimit, cancellationToken);
                if (!setLimitResponse.IsSuccessStatusCode)
                {
                    string errMsg = await setLimitResponse.Content.ReadAsStringAsync(cancellationToken);
                    throw new GroupApiRequestException(group, $"Failed to set data limit for user {username} on Outline server: {errMsg}", null);
                }

                accessKey.DataLimit = new(dataLimit);
            }

            // Save the new key to access key list
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
        }

        /// <summary>
        /// Removes the access key from the Outline server.
        /// Removes the associated credential from local storage.
        /// </summary>
        /// <param name="accessKey">The access key to be deleted.</param>
        /// <param name="users">The <see cref="Users"/> object.</param>
        /// <param name="group">Target group.</param>
        /// <param name="httpClient">An instance of generic HTTP client to use when creating Outline API client.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <returns>
        /// An optional error message.
        /// At any stage of the operation, if an error occurs, the function immediately returns an error message.
        /// Null if no errors had ever occurred.
        /// </returns>
        private async Task RemoveUserFromOutlineServer(OutlineAccessKey accessKey, Users users, string group, HttpClient httpClient, CancellationToken cancellationToken = default)
        {
            if (OutlineApiKey is null || OutlineAccessKeys is null)
                throw new InvalidOperationException("Outline API key is not found.");

            _apiClient ??= new(httpClient, OutlineApiKey);

            // Remove
            var response = await _apiClient.DeleteAccessKeyAsync(accessKey.Id, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                string errMsg = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new GroupApiRequestException(group, $"Failed to remove user {accessKey.Name} from Outline server: {errMsg}", null);
            }

            // Remove from access key list
            OutlineAccessKeys.Remove(accessKey);

            // Remove credential from local storage
            _ = users.RemoveCredentialFromUser(accessKey.Name, group);
        }

        /// <summary>
        /// Applies the group's per-user data limit
        /// to the linked Outline server.
        /// Updates <see cref="OutlineServerInfo"/>.
        /// </summary>
        /// <param name="group">The group name. Used in the returned error message.</param>
        /// <param name="httpClient">An instance of generic HTTP client to use when creating Outline API client.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <returns>
        /// An optional error message.
        /// At any stage of the operation, if an error occurs, the function immediately returns an error message.
        /// Null if no errors had ever occurred.
        /// </returns>
        private async Task ApplyPerUserDataLimitToOutlineServer(string group, HttpClient httpClient, CancellationToken cancellationToken = default)
        {
            if (OutlineApiKey is null)
                throw new InvalidOperationException("Outline API key is not found.");
            if (OutlineServerInfo is null)
                throw new InvalidOperationException("Outline server information is not found.");

            _apiClient ??= new(httpClient, OutlineApiKey);

            if (PerUserDataLimitInBytes == 0UL)
            {
                if (OutlineServerInfo.AccessKeyDataLimit is not null)
                {
                    // delete data limit from server
                    var responseMessage = await _apiClient.DeleteDataLimitAsync(cancellationToken);
                    if (!responseMessage.IsSuccessStatusCode)
                    {
                        string errMsg = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
                        throw new GroupApiRequestException(group, $"Failed to delete per-user data limit from Outline server: {errMsg}", null);
                    }

                    OutlineServerInfo.AccessKeyDataLimit = null;
                }
            }
            else
            {
                if (OutlineServerInfo.AccessKeyDataLimit is OutlineDataLimit dataLimit && dataLimit.Bytes != PerUserDataLimitInBytes)
                {
                    // update server data limit
                    var responseMessage = await _apiClient.SetDataLimitAsync(PerUserDataLimitInBytes, cancellationToken);
                    if (!responseMessage.IsSuccessStatusCode)
                    {
                        string errMsg = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
                        throw new GroupApiRequestException(group, $"Failed to apply per-user data limit {InteractionHelper.HumanReadableDataString1024(PerUserDataLimitInBytes)} to Outline server: {errMsg}", null);
                    }

                    OutlineServerInfo.AccessKeyDataLimit = new(PerUserDataLimitInBytes);
                }
            }
        }

        /// <summary>
        /// Sets a custom data limit on the specified access key.
        /// Applies the change to Outline server and updates the
        /// corresponding local entry.
        /// </summary>
        /// <param name="accessKey">The access key to apply the custom data limit on.</param>
        /// <param name="dataLimitInBytes">The custom data limit in bytes.</param>
        /// <param name="group">The group name. Used in the returned error message.</param>
        /// <param name="httpClient">An instance of generic HTTP client to use when creating Outline API client.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <returns>
        /// An optional error message.
        /// At any stage of the operation, if an error occurs, the function immediately returns an error message.
        /// Null if no errors had ever occurred.
        /// </returns>
        private async Task SetAccessKeyDataLimitOnOutlineServer(OutlineAccessKey accessKey, ulong dataLimitInBytes, string group, HttpClient httpClient, CancellationToken cancellationToken = default)
        {
            if (OutlineApiKey is null)
                throw new InvalidOperationException("Outline API key is not found.");

            _apiClient ??= new(httpClient, OutlineApiKey);

            var responseMessage = await _apiClient.SetAccessKeyDataLimitAsync(accessKey.Id, dataLimitInBytes, cancellationToken);
            if (!responseMessage.IsSuccessStatusCode)
            {
                string errMsg = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
                throw new GroupApiRequestException(group, $"Failed to apply the custom data limit {InteractionHelper.HumanReadableDataString1024(dataLimitInBytes)} to user {accessKey.Name}'s access key on Outline server: {errMsg}", null);
            }

            accessKey.DataLimit = new(dataLimitInBytes);
        }

        /// <summary>
        /// Deletes the specified access key's custom data limit.
        /// Applies the change to Outline server and updates the
        /// corresponding local entry.
        /// </summary>
        /// <param name="accessKey">The access key to apply the custom data limit on.</param>
        /// <param name="group">The group name. Used in the returned error message.</param>
        /// <param name="httpClient">An instance of generic HTTP client to use when creating Outline API client.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <returns>
        /// An optional error message.
        /// At any stage of the operation, if an error occurs, the function immediately returns an error message.
        /// Null if no errors had ever occurred.
        /// </returns>
        private async Task DeleteAccessKeyDataLimitOnOutlineServer(OutlineAccessKey accessKey, string group, HttpClient httpClient, CancellationToken cancellationToken = default)
        {
            if (OutlineApiKey is null)
                throw new InvalidOperationException("Outline API key is not found.");

            _apiClient ??= new(httpClient, OutlineApiKey);

            var responseMessage = await _apiClient.DeleteAccessKeyDataLimitAsync(accessKey.Id, cancellationToken);
            if (!responseMessage.IsSuccessStatusCode)
            {
                string errMsg = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
                throw new GroupApiRequestException(group, $"Failed to delete the custom data limit from user {accessKey.Name}'s access key on Outline server: {errMsg}", null);
            }

            accessKey.DataLimit = null;
        }

        /// <summary>
        /// Removes the association of the SSMv1 server
        /// and all saved data related to it.
        /// </summary>
        /// <param name="groupName">The group name.</param>
        /// <param name="users">The <see cref="Users"/> object.</param>
        public void RemoveSSMv1Server(string groupName, Users users)
        {
            if (SSMv1Server is not null)
            {
                SSMv1Server.ClearServerStats(groupName, this, users);
                SSMv1Server = null;
            }
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
