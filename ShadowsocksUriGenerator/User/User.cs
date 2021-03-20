using System;
using System.Collections.Generic;
using System.Linq;

namespace ShadowsocksUriGenerator
{
    /// <summary>
    /// Each user has a unique name and a list of credentials.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Gets or sets the UUID of the user.
        /// Used for online configuration delivery (SIP008).
        /// </summary>
        public string Uuid { get; set; }

        /// <summary>
        /// Gets or sets the data limit of the user in bytes.
        /// </summary>
        public ulong DataLimitInBytes { get; set; }

        /// <summary>
        /// Gets or sets the data usage of the user in bytes.
        /// The value equals the sum of all node groups.
        /// </summary>
        public ulong BytesUsed { get; set; }

        /// <summary>
        /// Gets or sets the data remaining to be used in bytes.
        /// If any node group has a zero value, the value will be zero.
        /// Otherwise, the value equals the sum of all node groups.
        /// </summary>
        public ulong BytesRemaining { get; set; }

        /// <summary>
        /// Gets or sets the credential dictionary of the user.
        /// key is the associated group name.
        /// value is user's credential to the group's nodes.
        /// </summary>
        public Dictionary<string, Credential?> Credentials { get; set; }

        public User()
        {
            Uuid = Guid.NewGuid().ToString();
            Credentials = new();
        }

        /// <summary>
        /// Adds the user to a group with empty credential.
        /// </summary>
        /// <param name="group">Name of the group.</param>
        /// <returns><see cref="true"/> if the user was successfully added to the group. Otherwise <see cref="false"/>.</returns>
        public bool AddToGroup(string group) => Credentials.TryAdd(group, null);

        /// <summary>
        /// Adds a new credential to the user's credential dictionary.
        /// </summary>
        /// <param name="group">The new credential's group name.</param>
        /// <param name="method">Encryption method.</param>
        /// <param name="password">Password.</param>
        /// <returns>0 for success. 2 for duplicated credential.</returns>
        public int AddCredential(string group, string method, string password)
        {
            if (!Credentials.ContainsKey(group)) // not in group, add user to it
            {
                var credential = new Credential(method, password);
                Credentials.Add(group, credential);
            }
            else if (Credentials[group] == null) // already in group, add credential
                Credentials[group] = new(method, password);
            else // already in group with credential
                return 2;

            return 0;
        }

        /// <summary>
        /// Adds a new credential to the user's credential dictionary.
        /// </summary>
        /// <param name="group">The new credential's group name.</param>
        /// <param name="userinfoBase64url">userinfo (method:password) in base64url</param>
        /// <returns>0 for success. 2 for duplicated credential. -2 for invalid userinfo base64url.</returns>
        public int AddCredential(string group, string userinfoBase64url)
        {
            Credential credential;
            try
            {
                credential = new(userinfoBase64url);
            }
            catch
            {
                return -2;
            }

            if (!Credentials.ContainsKey(group)) // not in group, add user to it
                Credentials.Add(group, credential);
            else if (Credentials[group] == null) // already in group, add credential
                Credentials[group] = credential;
            else // already in group with credential
                return 2;

            return 0;
        }

        /// <summary>
        /// Removes the user from the specified group.
        /// </summary>
        /// <param name="group">Name of the group.</param>
        /// <returns>
        /// <see cref="true"/> if the user successfully left the group.
        /// Otherwise <see cref="false"/>, including already not in the group.
        /// </returns>
        public bool RemoveFromGroup(string group) => Credentials.Remove(group);

        /// <summary>
        /// Removes the credential associated with the group.
        /// </summary>
        /// <param name="group">Name of the group.</param>
        /// <returns>
        /// 0 when success.
        /// 1 when no associated credential.
        /// -1 when user not in group.
        /// </returns>
        public int RemoveCredential(string group)
        {
            if (Credentials.TryGetValue(group, out var credential))
            {
                if (credential == null)
                    return 1;
                else
                    Credentials[group] = null;
            }
            else
                return -1;

            return 0;
        }

        public List<Uri> GetSSUris(Nodes nodes, params string[] groups)
        {
            List<Uri> uris = new();
            foreach (var credEntry in Credentials)
            {
                // Skip if either
                // - User is in group but has no credential.
                // - Only retrieve ss URIs from specific groups, not including this group.
                if (credEntry.Value == null || groups.Length > 0 && !groups.Contains(credEntry.Key))
                    continue;
                var userinfoBase64url = credEntry.Value.UserinfoBase64url;
                if (nodes.Groups.TryGetValue(credEntry.Key, out Group? group)) // find credEntry's group
                {
                    foreach (var node in group.NodeDict)
                    {
                        if (node.Value.Deactivated)
                            continue;
                        var fragment = node.Key;
                        var host = node.Value.Host;
                        var port = node.Value.Port;
                        var plugin = node.Value.Plugin;
                        var pluginOpts = node.Value.PluginOpts;
                        uris.Add(SSUriBuilder(userinfoBase64url, host, port, fragment, plugin, pluginOpts));
                    }
                }
                else
                    continue; // ignoring is intentional, as groups may get removed.
            }
            return uris;
        }

        /// <summary>
        /// Gathers user's data usage metrics from all groups
        /// and calculates the total data usage of the user.
        /// </summary>
        /// <param name="username">Target user.</param>
        /// <param name="nodes">The <see cref="Nodes"/> object.</param>
        public void CalculateTotalDataUsage(string username, Nodes nodes)
        {
            BytesUsed = 0UL;
            var bytesUsedInGroup = 0UL; // make compiler happy
            
            foreach (var groupEntry in nodes.Groups)
            {
                // Filter out access key ids that belongs to the user.
                var filteredAccessKeyIds = groupEntry.Value.OutlineAccessKeys?.Where(x => x.Name == username).Select(x => x.Id);
                if (filteredAccessKeyIds != null)
                {
                    foreach (var id in filteredAccessKeyIds)
                    {
                        if (int.TryParse(id, out var keyId)
                            && groupEntry.Value.OutlineDataUsage?.BytesTransferredByUserId.TryGetValue(keyId, out bytesUsedInGroup) is bool hasDataUsage
                            && hasDataUsage)
                            BytesUsed += bytesUsedInGroup;
                    }
                }
            }
        }

        /// <summary>
        /// Gets all data usage records associated with the user.
        /// </summary>
        /// <param name="username">Target user.</param>
        /// <param name="nodes">The <see cref="Nodes"/> object.</param>
        /// <returns>A list of data usage records as tuples.</returns>
        public List<(string group, ulong bytesUsed, ulong bytesRemaining)> GetDataUsage(string username, Nodes nodes)
        {
            List<(string group, ulong bytesUsed, ulong bytesRemaining)> results = new();
            var bytesUsedInGroup = 0UL; // make compiler happy
            BytesUsed = 0UL;

            foreach (var groupEntry in nodes.Groups)
            {
                // Filter out access key ids that belongs to the user.
                var filteredAccessKeyIds = groupEntry.Value.OutlineAccessKeys?.Where(x => x.Name == username).Select(x => x.Id);
                if (filteredAccessKeyIds != null)
                {
                    foreach (var id in filteredAccessKeyIds)
                    {
                        if (int.TryParse(id, out var keyId)
                            && groupEntry.Value.OutlineDataUsage?.BytesTransferredByUserId.TryGetValue(keyId, out bytesUsedInGroup) is bool hasDataUsage
                            && hasDataUsage)
                        {
                            var group = groupEntry.Key;
                            var bytesUsed = bytesUsedInGroup;
                            var bytesRemaining = DataLimitInBytes == 0 ? 0 : DataLimitInBytes - bytesUsed;
                            BytesUsed += bytesUsed;
                            results.Add((group, bytesUsed, bytesRemaining));
                        }
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Sets the data limit.
        /// </summary>
        /// <param name="dataLimit">The data limit in bytes.</param>
        /// <param name="groups">Only set for these groups.</param>
        /// <returns>0 on success. -2 on group not found.</returns>
        public int SetDataLimit(ulong dataLimit, string[]? groups = null)
        {
            if (groups == null)
                DataLimitInBytes = dataLimit;
            else
                foreach (var group in groups)
                    if (Credentials.ContainsKey(group))
                    {
                        // TODO: set data limit to group.
                    }
                    else
                        return -2;
            return 0;
        }

        public static Uri SSUriBuilder(string userinfoBase64url, string host, int port, string fragment, string? plugin = null, string? pluginOpts = null)
        {
            UriBuilder ssUriBuilder = new("ss", host, port)
            {
                UserName = userinfoBase64url,
                Fragment = fragment
            };
            if (!string.IsNullOrEmpty(plugin))
                if (!string.IsNullOrEmpty(pluginOpts))
                    ssUriBuilder.Query = $"plugin={Uri.EscapeDataString($"{plugin};{pluginOpts}")}"; // manually escape as a workaround
                else
                    ssUriBuilder.Query = $"plugin={plugin}";
            return ssUriBuilder.Uri;
        }
    }
}
