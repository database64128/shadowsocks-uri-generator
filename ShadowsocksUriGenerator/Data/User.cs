using ShadowsocksUriGenerator.Protocols.Shadowsocks;
using System.Text.Json.Serialization;

namespace ShadowsocksUriGenerator.Data
{
    /// <summary>
    /// Each user has a unique name and a list of credentials.
    /// </summary>
    public class User
    {
        private ulong _bytesUsed;

        /// <summary>
        /// Gets or sets the UUID of the user.
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
        public ulong BytesUsed
        {
            get => Interlocked.Read(in _bytesUsed);
            init => _bytesUsed = value;
        }

        /// <summary>
        /// Gets the data remaining to be used in bytes.
        /// If any node group has a zero value, the value will be zero.
        /// Otherwise, the value equals the sum of all node groups.
        /// </summary>
        [JsonIgnore]
        public ulong BytesRemaining => DataLimitInBytes > BytesUsed ? DataLimitInBytes - BytesUsed : 0UL;

        /// <summary>
        /// Gets or sets the membership dictionary of the user.
        /// Key is the associated group name.
        /// Value is user's member information to the group.
        /// </summary>
        public Dictionary<string, MemberInfo> Memberships { get; set; } = [];

        /// <summary>
        /// For compatibility only. Do not use.
        /// </summary>
        public Dictionary<string, MemberInfo?>? Credentials { get; set; }

        /// <summary>
        /// Adds the user to a group with empty credential.
        /// </summary>
        /// <param name="group">Name of the group.</param>
        /// <returns>
        /// <c>true</c> if the user was successfully added to the group.
        /// Otherwise, <c>false</c>.
        /// </returns>
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
            if (!Memberships.TryGetValue(group, out MemberInfo? memberInfo)) // not in group, add user to it
            {
                memberInfo = new MemberInfo(method, password);
                Memberships.Add(group, memberInfo);
                return 0;
            }
            else if (!memberInfo.HasCredential) // already in group without credential, add credential
            {
                memberInfo.Method = method;
                memberInfo.Password = password;
                return 0;
            }
            else // already in group with credential
            {
                return 2;
            }
        }

        /// <summary>
        /// Generates a new credential for the user in the specified group.
        /// </summary>
        /// <param name="group">Group name.</param>
        /// <param name="method">Method name.</param>
        /// <param name="overwriteExisting">Whether to overwrite existing credential.</param>
        /// <returns>0 for success. 2 for duplicated credential.</returns>
        public int GenerateCredential(string group, string method, bool overwriteExisting)
        {
            if (!Memberships.TryGetValue(group, out MemberInfo? memberInfo)) // not in group, add user to it
            {
                memberInfo = new MemberInfo(method);
                Memberships.Add(group, memberInfo);
                return 0;
            }
            else if (!memberInfo.HasCredential) // already in group without credential, add credential
            {
                memberInfo.Method = method;
                memberInfo.GeneratePassword();
                return 0;
            }
            else if (overwriteExisting) // already in group with credential, overwrite it
            {
                memberInfo.Method = method;
                memberInfo.GeneratePassword();
                return 0;
            }
            else // already in group with credential
            {
                return 2;
            }
        }

        /// <summary>
        /// Removes the user from the specified group.
        /// </summary>
        /// <param name="group">Name of the group.</param>
        /// <returns>
        /// <c>true</c> if the user successfully left the group.
        /// Otherwise <c>false</c>, including already not in the group.
        /// </returns>
        public bool RemoveFromGroup(string group)
        {
            if (Memberships.Remove(group, out MemberInfo? memberInfo))
            {
                SubBytesUsed(memberInfo.BytesUsed);
                return true;
            }
            return false;
        }

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

        /// <summary>
        /// Clears the data usage of the user in the specified group.
        /// </summary>
        /// <param name="groupName">The group name.</param>
        /// <param name="perUserDataLimitInBytes">The per-user data limit of the group in bytes.</param>
        /// <returns>
        /// <c>true</c> if the user is in the group and the data usage is cleared.
        /// Otherwise, <c>false</c>.
        /// </returns>
        public bool ClearGroupBytesUsed(string groupName, ulong perUserDataLimitInBytes)
        {
            if (Memberships.TryGetValue(groupName, out MemberInfo? memberInfo))
            {
                SubBytesUsed(memberInfo.BytesUsed);
                memberInfo.ClearBytesUsed(perUserDataLimitInBytes);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the user's Shadowsocks server configurations.
        /// </summary>
        /// <param name="users">The <see cref="Users"/> object.</param>
        /// <param name="nodes">The <see cref="Nodes"/> object.</param>
        /// <param name="groups">If not empty, only include servers from these groups.</param>
        /// <param name="tags">If not empty, only include servers with these tags.</param>
        /// <param name="groupOwnerIds">If not empty, only include servers from groups owned by these users.</param>
        /// <param name="nodeOwnerIds">If not empty, only include servers owned by these users.</param>
        /// <returns>The user's Shadowsocks server configurations.</returns>
        public IEnumerable<ShadowsocksServerConfig> GetShadowsocksServers(Users users, Nodes nodes, string[] groups, string[] tags, string[] groupOwnerIds, string[] nodeOwnerIds)
        {
            foreach (var membership in Memberships)
            {
                // Skip if any:
                // - User is in group but has no credential.
                // - Only want servers from specified groups, not including this one.
                // - Group not found.
                // - Only want servers from groups owned by specified owners, not including this one.
                if (!membership.Value.HasCredential ||
                    groups.Length > 0 && !groups.Contains(membership.Key) ||
                    !nodes.Groups.TryGetValue(membership.Key, out var group) ||
                    groupOwnerIds.Length > 0 && !groupOwnerIds.Contains(group.OwnerUuid))
                    continue;

                foreach (var nodeEntry in group.NodeDict)
                {
                    var node = nodeEntry.Value;
                    // Skip if any:
                    // - Node deactivated.
                    // - Only want servers owned by specified owners, not including this one.
                    // - Only want servers with specified tags. This one does not have all wanted tags.
                    if (node.Deactivated ||
                        nodeOwnerIds.Length > 0 && !nodeOwnerIds.Contains(node.OwnerUuid) ||
                        tags.Length > 0 && tags.Any(x => !node.Tags.Exists(y => string.Equals(x, y, StringComparison.OrdinalIgnoreCase))))
                        continue;

                    var owner = node.OwnerUuid is null
                        ? null
                        : users.TryGetUserById(node.OwnerUuid, out var ownerEntry)
                        ? ownerEntry.Key
                        : null;

                    yield return new ShadowsocksServerConfig()
                    {
                        Id = node.Uuid,
                        Name = nodeEntry.Key,
                        Host = node.Host,
                        Port = node.Port,
                        Method = membership.Value.Method,
                        UserPSK = membership.Value.Password,
                        IdentityPSKs = node.IdentityPSKs,
                        PluginName = node.Plugin,
                        PluginVersion = node.PluginVersion,
                        PluginOptions = node.PluginOpts,
                        PluginArguments = node.PluginArguments,
                        Group = membership.Key,
                        Owner = owner,
                        Tags = node.Tags,
                    };
                }
            }
        }

        public IEnumerable<Uri> GetSSUris(Users users, Nodes nodes, params string[] groups) =>
            GetShadowsocksServers(users, nodes, groups, [], [], [])
            .Select(x => x.ToUri());

        /// <summary>
        /// Calculates the total data usage of the user.
        /// </summary>
        public void CalculateTotalDataUsage() =>
            _bytesUsed = Memberships.Aggregate(0UL, (x, y) => x + y.Value.BytesUsed);

        public void AddBytesUsed(ulong bytesUsed) => Interlocked.Add(ref _bytesUsed, bytesUsed);

        public void SubBytesUsed(ulong bytesUsed)
        {
            while (true)
            {
                ulong currentBytesUsed = BytesUsed;
                if (currentBytesUsed < bytesUsed)
                    break;
                if (Interlocked.CompareExchange(ref _bytesUsed, currentBytesUsed - bytesUsed, currentBytesUsed) == currentBytesUsed)
                    break;
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
            List<(string group, ulong bytesUsed, ulong bytesRemaining)> results = [];
            HashSet<string> outlineGroupNameSet = [];

            foreach (KeyValuePair<string, Group> groupEntry in nodes.Groups)
            {
                if (groupEntry.Value.AddUserOutlineDataUsage(results, username, groupEntry.Key))
                {
                    outlineGroupNameSet.Add(groupEntry.Key);
                }
            }

            foreach (KeyValuePair<string, MemberInfo> membership in Memberships)
            {
                if (outlineGroupNameSet.Contains(membership.Key))
                    continue;

                results.Add((membership.Key, membership.Value.BytesUsed, membership.Value.BytesRemaining));
            }

            return results;
        }

        /// <summary>
        /// Gets the data limit of the user in the specified group.
        /// Group existence is not checked.
        /// Outline's user custom limit is not checked.
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
    }
}
