using ShadowsocksUriGenerator.Outline;
using ShadowsocksUriGenerator.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

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
        /// Gets or sets the group's owner.
        /// </summary>
        public string? OwnerUuid { get; set; }

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
        public Dictionary<string, Node> NodeDict { get; set; } = new();

        /// <summary>
        /// The Outline API client instance.
        /// </summary>
        private ApiClient? _apiClient;
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
                    Tags = tags.ToList(),
                    IdentityPSKs = iPSKs.ToList(),
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
                OutlineApiKey = JsonSerializer.Deserialize<ApiKey>(apiKey, Utilities.apiKeyJsonSerializerOptions);
            }
            catch (JsonException)
            {
                return "Error: Invalid API key: deserialization failed.";
            }

            if (OutlineApiKey is null)
                return "Error: Invalid API key: deserialization returned null.";

            // Validate ApiUrl
            if (Uri.TryCreate(OutlineApiKey.ApiUrl, UriKind.Absolute, out var apiUri))
            {
                if (apiUri.Scheme != "https")
                    return "Error: The API URL must use HTTPS.";

                // Remove trailing '/'
                if (OutlineApiKey.ApiUrl.EndsWith('/'))
                    OutlineApiKey = OutlineApiKey with { ApiUrl = OutlineApiKey.ApiUrl[..^1] };
            }
            else
            {
                return "Error: The API URL is not a valid URI.";
            }

            // Validate CertSha256
            if (!string.IsNullOrEmpty(OutlineApiKey.CertSha256) && OutlineApiKey.CertSha256.Length != 64)
                return "Error: Malformed CertSha256: length is not 64.";

            // Clean up potential leftover API client
            _apiClient?.Dispose();
            _apiClient = new(OutlineApiKey, httpClient);

            // Pull from Outline server without syncing with local user db
            var errMsg = await PullFromOutlineServer(group, users, httpClient, false, false, cancellationToken);
            if (errMsg is not null)
                return errMsg;

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
                var dataLimitErrMsg = await ApplyPerUserDataLimitToOutlineServer(group, httpClient, cancellationToken);
                if (dataLimitErrMsg is not null)
                    return dataLimitErrMsg;
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

            _apiClient ??= new(OutlineApiKey, httpClient);

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

            _apiClient ??= new(OutlineApiKey, httpClient);

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

                var results = await Task.WhenAll(tasks);
                foreach (var responseMessage in results)
                {
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
        public void RemoveOutlineServer()
        {
            OutlineApiKey = null;
            OutlineServerInfo = null;
            OutlineAccessKeys = null;
            OutlineDataUsage = null;

            CalculateTotalDataUsage();
        }

        /// <summary>
        /// Pulls server information, access keys, and data usage
        /// from the associated Outline server.
        /// Optionally updates user membership dictionary
        /// in the local storage.
        /// Remember to call <see cref="Users.CalculateDataUsageForAllUsers(Nodes)"/>
        /// after all pulls are finished.
        /// </summary>
        /// <param name="group">Group name.</param>
        /// <param name="users">The <see cref="Users"/> object.</param>
        /// <param name="httpClient">An instance of generic HTTP client to use when creating Outline API client.</param>
        /// <param name="updateLocalUserMemberships">
        /// Whether to update local user memberships from the retrieved access keys.
        /// Defaults to true.
        /// </param>
        /// <param name="silentlySkipNonOutline">
        /// Set to true to silently return without emitting an error
        /// if this server is not linked to any Outline server.
        /// Defaults to false.
        /// </param>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <returns>
        /// The task that represents the operation.
        /// An optional error message.
        /// </returns>
        public async Task<string?> PullFromOutlineServer(string group, Users users, HttpClient httpClient, bool updateLocalUserMemberships = true, bool silentlySkipNonOutline = false, CancellationToken cancellationToken = default)
        {
            if (OutlineApiKey is null)
            {
                if (silentlySkipNonOutline)
                    return null;
                else
                    return $"Error: Group {group} is not linked to any Outline server.";
            }

            _apiClient ??= new(OutlineApiKey, httpClient);

            try
            {
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

                if (updateLocalUserMemberships)
                    UpdateLocalUserMemberships(group, users);

                CalculateTotalDataUsage();
            }
            catch (OperationCanceledException ex) when (ex.InnerException is not TimeoutException) // canceled
            {
                return ex.Message;
            }
            catch (Exception ex) // timeout and other errors
            {
                return $"Error when pulling from group {group}'s Outline server: {ex.Message}";
            }

            return null;
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
        /// <param name="httpClient">An instance of generic HTTP client to use when creating Outline API client.</param>
        /// <param name="silentlySkipNonOutline">
        /// Set to true to silently return without emitting an error
        /// if this server is not linked to any Outline server.
        /// Defaults to false.
        /// </param>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <returns>
        /// An async-enumerable sequence whose elements are error messages.
        /// </returns>
        public async IAsyncEnumerable<string> DeployToOutlineServer(string group, Users users, HttpClient httpClient, bool silentlySkipNonOutline = false, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (OutlineApiKey is null)
            {
                if (!silentlySkipNonOutline)
                    yield return $"Error: Group {group} is not linked to any Outline server.";

                yield break;
            }

            if (OutlineAccessKeys is null)
            {
                var errMsg = await PullFromOutlineServer(group, users, httpClient, true, false, cancellationToken);
                if (errMsg is not null)
                {
                    yield return errMsg;
                    yield break;
                }
            }

            OutlineAccessKeys ??= new();
            _apiClient ??= new(OutlineApiKey, httpClient);

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

            var tasks = new List<Task<string?>>
            {
                // Per-user data limit
                ApplyPerUserDataLimitToOutlineServer(group, httpClient, cancellationToken),
            };

            // Add
            tasks.AddRange(usersToCreate.Select(userEntry => AddUserToOutlineServer(userEntry.Key, userEntry.Value, group, httpClient, cancellationToken)));

            // Remove
            tasks.AddRange(accessKeysToRemove.Select(accessKey => RemoveUserFromOutlineServer(accessKey, users, group, httpClient, cancellationToken)));

            // Update data limit
            tasks.AddRange(accessKeysToUpdateDataLimit.Select(x => SetAccessKeyDataLimitOnOutlineServer(x.accessKey, x.DataLimitInBytes, group, httpClient, cancellationToken)));

            // Remove data limit
            tasks.AddRange(accessKeysToRemoveDataLimit.Select(accessKey => DeleteAccessKeyDataLimitOnOutlineServer(accessKey, group, httpClient, cancellationToken)));

            while (tasks.Count > 0)
            {
                var finishedTask = await Task.WhenAny(tasks);

                var errMsg = await finishedTask;
                if (errMsg is not null)
                {
                    yield return errMsg;
                }

                tasks.Remove(finishedTask);
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

            _apiClient ??= new(OutlineApiKey, httpClient);

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
        public async IAsyncEnumerable<string> RotatePassword(string group, Users users, HttpClient httpClient, bool silentlySkipNonOutline = false, [EnumeratorCancellation] CancellationToken cancellationToken = default, params string[] usernames)
        {
            if (OutlineApiKey is null)
            {
                if (!silentlySkipNonOutline)
                {
                    yield return $"Warning: Group {group} does not support automatic password rotation, because it is not linked to any Outline server. You have to manually change the password if you intend to rotate it.";
                }

                yield break;
            }

            if (OutlineAccessKeys is null)
            {
                var errMsg = await PullFromOutlineServer(group, users, httpClient, true, false, cancellationToken);
                if (errMsg is not null)
                {
                    yield return errMsg;
                    yield break;
                }
            }

            OutlineAccessKeys ??= new();
            _apiClient ??= new(OutlineApiKey, httpClient);

            // Filter out access keys that are linked to a user.
            // This is important because later we need to access UserDict with key names.
            // The result has to be freezed by calling .ToArray().
            // Otherwise the query result will change after removal.
            var accessKeysLinkedToUser = OutlineAccessKeys.Where(x => users.UserDict.ContainsKey(x.Name));
            var targetAccessKeys = usernames.Length > 0
                ? accessKeysLinkedToUser.Where(x => usernames.Contains(x.Name)).ToArray()
                : accessKeysLinkedToUser.ToArray();

            // Remove
            var removalTasks = targetAccessKeys.Select(accessKey => RemoveUserFromOutlineServer(accessKey, users, group, httpClient, cancellationToken)).ToList();
            while (removalTasks.Count > 0)
            {
                var finishedTask = await Task.WhenAny(removalTasks);

                var errMsg = await finishedTask;
                if (errMsg is not null)
                {
                    yield return errMsg;
                }

                removalTasks.Remove(finishedTask);
            }

            // Add
            var addTasks = targetAccessKeys.Select(accessKey => AddUserToOutlineServer(accessKey.Name, users.UserDict[accessKey.Name], group, httpClient, cancellationToken)).ToList();
            while (addTasks.Count > 0)
            {
                var finishedTask = await Task.WhenAny(addTasks);

                var errMsg = await finishedTask;
                if (errMsg is not null)
                {
                    yield return errMsg;
                }

                addTasks.Remove(finishedTask);
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
        private async Task<string?> AddUserToOutlineServer(string username, User user, string group, HttpClient httpClient, CancellationToken cancellationToken = default)
        {
            if (OutlineApiKey is null)
                throw new InvalidOperationException("Outline API key is not found.");

            _apiClient ??= new(OutlineApiKey, httpClient);
            OutlineAccessKeys ??= new();

            try
            {
                // Create
                var response = await _apiClient.CreateAccessKeyAsync(cancellationToken);
                if (!response.IsSuccessStatusCode)
                    return $"Error when creating user for {username} on group {group}'s Outline server: {await response.Content.ReadAsStringAsync(cancellationToken)}";

                // Deserialize access key
                var accessKey = await response.Content.ReadFromJsonAsync<AccessKey>(Utilities.commonJsonDeserializerOptions, cancellationToken);
                if (accessKey is null)
                    return $"Error when deserializing access key for user {username} on group {group}'s Outline server: the result is null.";

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
            catch (OperationCanceledException ex) when (ex.InnerException is not TimeoutException) // canceled
            {
                return ex.Message;
            }
            catch (Exception ex) // timeout and other errors
            {
                return $"Error when adding user {username} to group {group}'s Outline server: {ex.Message}";
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
        /// <param name="httpClient">An instance of generic HTTP client to use when creating Outline API client.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <returns>
        /// An optional error message.
        /// At any stage of the operation, if an error occurs, the function immediately returns an error message.
        /// Null if no errors had ever occurred.
        /// </returns>
        private async Task<string?> RemoveUserFromOutlineServer(AccessKey accessKey, Users users, string group, HttpClient httpClient, CancellationToken cancellationToken = default)
        {
            if (OutlineApiKey is null || OutlineAccessKeys is null)
                throw new InvalidOperationException("Outline API key is not found.");

            _apiClient ??= new(OutlineApiKey, httpClient);

            try
            {
                // Remove
                var response = await _apiClient.DeleteAccessKeyAsync(accessKey.Id, cancellationToken);
                if (!response.IsSuccessStatusCode)
                    return $"Error when removing key of user {accessKey.Name} from group {group}'s Outline server: {await response.Content.ReadAsStringAsync(cancellationToken)}";

                // Remove from access key list
                OutlineAccessKeys.Remove(accessKey);

                // Remove credential from local storage
                _ = users.RemoveCredentialFromUser(accessKey.Name, group);
            }
            catch (OperationCanceledException ex) when (ex.InnerException is not TimeoutException) // canceled
            {
                return ex.Message;
            }
            catch (Exception ex) // timeout and other errors
            {
                return $"Error when removing key of user {accessKey.Name} from group {group}'s Outline server: {ex.Message}";
            }

            return null;
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
        private async Task<string?> ApplyPerUserDataLimitToOutlineServer(string group, HttpClient httpClient, CancellationToken cancellationToken = default)
        {
            if (OutlineApiKey is null)
                throw new InvalidOperationException("Outline API key is not found.");
            if (OutlineServerInfo is null)
                throw new InvalidOperationException("Outline server information is not found.");

            _apiClient ??= new(OutlineApiKey, httpClient);

            try
            {
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
                        return $"Error when applying per-user data limit {InteractionHelper.HumanReadableDataString1024(PerUserDataLimitInBytes)} to group {group}'s Outline server: {await responseMessage.Content.ReadAsStringAsync(cancellationToken)}";

                    OutlineServerInfo.AccessKeyDataLimit = new(PerUserDataLimitInBytes);
                }
            }
            catch (OperationCanceledException ex) when (ex.InnerException is not TimeoutException) // canceled
            {
                return ex.Message;
            }
            catch (Exception ex) // timeout and other errors
            {
                return $"Error when updating per-user data limit setting for group {group}'s Outline server: {ex.Message}";
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
        /// <param name="httpClient">An instance of generic HTTP client to use when creating Outline API client.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <returns>
        /// An optional error message.
        /// At any stage of the operation, if an error occurs, the function immediately returns an error message.
        /// Null if no errors had ever occurred.
        /// </returns>
        private async Task<string?> SetAccessKeyDataLimitOnOutlineServer(AccessKey accessKey, ulong dataLimitInBytes, string group, HttpClient httpClient, CancellationToken cancellationToken = default)
        {
            if (OutlineApiKey is null)
                throw new InvalidOperationException("Outline API key is not found.");

            _apiClient ??= new(OutlineApiKey, httpClient);

            try
            {
                var responseMessage = await _apiClient.SetAccessKeyDataLimitAsync(accessKey.Id, dataLimitInBytes, cancellationToken);
                if (!responseMessage.IsSuccessStatusCode)
                    return $"Error when applying the custom data limit {InteractionHelper.HumanReadableDataString1024(dataLimitInBytes)} to user {accessKey.Name}'s access key on group {group}'s Outline server: {await responseMessage.Content.ReadAsStringAsync(cancellationToken)}";

                accessKey.DataLimit = new(dataLimitInBytes);
            }
            catch (OperationCanceledException ex) when (ex.InnerException is not TimeoutException) // canceled
            {
                return ex.Message;
            }
            catch (Exception ex) // timeout and other errors
            {
                return $"Error when applying the custom data limit {InteractionHelper.HumanReadableDataString1024(dataLimitInBytes)} to user {accessKey.Name}'s access key on group {group}'s Outline server: {ex.Message}";
            }

            return null;
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
        private async Task<string?> DeleteAccessKeyDataLimitOnOutlineServer(AccessKey accessKey, string group, HttpClient httpClient, CancellationToken cancellationToken = default)
        {
            if (OutlineApiKey is null)
                throw new InvalidOperationException("Outline API key is not found.");

            _apiClient ??= new(OutlineApiKey, httpClient);

            try
            {
                var responseMessage = await _apiClient.DeleteAccessKeyDataLimitAsync(accessKey.Id, cancellationToken);
                if (!responseMessage.IsSuccessStatusCode)
                    return $"Error when deleting the custom data limit from user {accessKey.Name}'s access key on group {group}'s Outline server: {await responseMessage.Content.ReadAsStringAsync(cancellationToken)}";

                accessKey.DataLimit = null;
            }
            catch (OperationCanceledException ex) when (ex.InnerException is not TimeoutException) // canceled
            {
                return ex.Message;
            }
            catch (Exception ex) // timeout and other errors
            {
                return $"Error when deleting the custom data limit from user {accessKey.Name}'s access key on group {group}'s Outline server: {ex.Message}";
            }

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
