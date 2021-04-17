using System;
using System.Collections.Generic;
using System.Linq;

namespace ShadowsocksUriGenerator
{
    /// <summary>
    /// Each user has a unique name and a list of credentials.
    /// </summary>
    public class User : IDataUsage, IDataLimit
    {
        /// <summary>
        /// Gets or sets the UUID of the user.
        /// Used for online configuration delivery (SIP008).
        /// </summary>
        public string Uuid { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the global data limit of the user in bytes.
        /// </summary>
        public ulong DataLimitInBytes { get; set; }

        /// <summary>
        /// Gets or sets the per-group data limit of the user in bytes.
        /// </summary>
        public ulong PerGroupDataLimitInBytes { get; set; }

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
        /// Gets or sets the membership dictionary of the user.
        /// Key is the associated group name.
        /// Value is user's member information to the group.
        /// </summary>
        public Dictionary<string, MemberInfo> Memberships { get; set; } = new();

        /// <summary>
        /// For compatibility only. Do not use.
        /// </summary>
        public Dictionary<string, MemberInfo?>? Credentials { get; set; }

        /// <summary>
        /// Adds the user to a group with empty credential.
        /// </summary>
        /// <param name="group">Name of the group.</param>
        /// <returns><see cref="true"/> if the user was successfully added to the group. Otherwise <see cref="false"/>.</returns>
        public bool AddToGroup(string group) => Memberships.TryAdd(group, new());

        /// <summary>
        /// Adds a new credential to the user's credential dictionary.
        /// </summary>
        /// <param name="group">The new credential's group name.</param>
        /// <param name="method">Encryption method.</param>
        /// <param name="password">Password.</param>
        /// <returns>0 for success. 2 for duplicated credential.</returns>
        public int AddCredential(string group, string method, string password)
        {
            if (!Memberships.ContainsKey(group)) // not in group, add user to it
            {
                var memberInfo = new MemberInfo(method, password);
                Memberships.Add(group, memberInfo);
                return 0;
            }
            else if (!Memberships[group].HasCredential) // already in group without credential, add credential
            {
                Memberships[group].Method = method;
                Memberships[group].Password = password;
                return 0;
            }
            else // already in group with credential
            {
                return 2;
            }
        }

        /// <summary>
        /// Adds a new credential to the user's credential dictionary.
        /// </summary>
        /// <param name="group">The new credential's group name.</param>
        /// <param name="userinfoBase64url">userinfo (method:password) in base64url</param>
        /// <returns>0 for success. 2 for duplicated credential. -2 for invalid userinfo base64url.</returns>
        public int AddCredential(string group, string userinfoBase64url)
        {
            if (MemberInfo.TryParseFromUserinfoBase64url(userinfoBase64url, out var memberInfo))
            {
                if (!Memberships.ContainsKey(group)) // not in group, add user to it
                {
                    Memberships.Add(group, memberInfo);
                    return 0;
                }
                else if (!Memberships[group].HasCredential) // already in group without credential, add credential
                {
                    Memberships[group].Method = memberInfo.Method;
                    Memberships[group].Password = memberInfo.Password;
                    return 0;
                }
                else // already in group with credential
                {
                    return 2;
                }
            }
            else
            {
                return -2;
            }
        }

        /// <summary>
        /// Removes the user from the specified group.
        /// </summary>
        /// <param name="group">Name of the group.</param>
        /// <returns>
        /// <see cref="true"/> if the user successfully left the group.
        /// Otherwise <see cref="false"/>, including already not in the group.
        /// </returns>
        public bool RemoveFromGroup(string group) => Memberships.Remove(group);

        /// <summary>
        /// Removes the credential associated with the group.
        /// </summary>
        /// <param name="group">Name of the group.</param>
        /// <returns>
        /// 0 when the credential is successfully found and removed.
        /// 1 when the user is in the group but has no associated credential.
        /// -1 when the user is not in the group.
        /// </returns>
        public int RemoveCredential(string group)
        {
            if (Memberships.TryGetValue(group, out var memberInfo))
            {
                if (memberInfo.HasCredential)
                {
                    memberInfo.ClearCredential();
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            else
                return -1;
        }

        /// <summary>
        /// Removes all credentials from the user.
        /// </summary>
        public void RemoveAllCredentials()
        {
            foreach (var membership in Memberships)
            {
                membership.Value.ClearCredential();
            }
        }

        public List<Uri> GetSSUris(Nodes nodes, params string[] groups)
        {
            List<Uri> uris = new();

            foreach (var membership in Memberships)
            {
                // Skip if either
                // - User is in group but has no credential.
                // - Only retrieve ss URIs from specific groups, not including this group.
                if (!membership.Value.HasCredential || groups.Length > 0 && !groups.Contains(membership.Key))
                    continue;

                var userinfoBase64url = membership.Value.UserinfoBase64url;
                if (nodes.Groups.TryGetValue(membership.Key, out Group? group)) // find credEntry's group
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
                if (filteredAccessKeyIds is not null)
                {
                    foreach (var id in filteredAccessKeyIds)
                    {
                        if (int.TryParse(id, out var keyId)
                            && groupEntry.Value.OutlineDataUsage?.BytesTransferredByUserId.TryGetValue(keyId, out bytesUsedInGroup) == true)
                            BytesUsed += bytesUsedInGroup;
                    }
                }
            }

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
                if (filteredAccessKeyIds is not null)
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
        /// Gets the data limit of the user in the specified group.
        /// Group existence is not checked.
        /// </summary>
        /// <param name="group">Target group.</param>
        /// <returns>The data limit in bytes.</returns>
        public ulong GetDataLimitInGroup(string group)
            => Memberships.TryGetValue(group, out var memberInfo) && memberInfo.DataLimitInBytes > 0UL
                ? memberInfo.DataLimitInBytes
                : PerGroupDataLimitInBytes;

        /// <summary>
        /// Sets the data limit of the user in the specified group.
        /// </summary>
        /// <param name="group">Target group.</param>
        /// <param name="dataLimit">The data limit in bytes.</param>
        /// <returns>
        /// 0 if the user is in the group and the data limit is set.
        /// -2 if the user is not in the group.
        /// </returns>
        public int SetDataLimitInGroup(string group, ulong dataLimit)
        {
            if (Memberships.TryGetValue(group, out var memberInfo))
            {
                memberInfo.DataLimitInBytes = dataLimit;
                return 0;
            }
            else
                return -2;
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
