using ShadowsocksUriGenerator.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace ShadowsocksUriGenerator.Data
{
    /// <summary>
    /// The class for storing user information in Users.json.
    /// </summary>
    public class Users
    {
        /// <summary>
        /// Gets the default configuration version
        /// used by this version of the app.
        /// </summary>
        public static int DefaultVersion => 3;

        /// <summary>
        /// Gets or sets the configuration version number.
        /// 0 for the legacy config version
        /// without a version number property.
        /// Newer config versions start from 1.
        /// Update if older config is present.
        /// Throw error if config is newer than supported.
        /// </summary>
        public int Version { get; set; } = DefaultVersion;

        /// <summary>
        /// Gets or sets the user dictionary.
        /// key is username.
        /// value is user info.
        /// </summary>
        [JsonPropertyName("Users")]
        public Dictionary<string, User> UserDict { get; set; } = [];

        /// <summary>
        /// Adds a new user to UserDict.
        /// </summary>
        /// <param name="username">The new user to be added to <see cref="UserDict"/>.</param>
        /// <returns>
        /// 0 for success.
        /// 1 when a user with the same username already exists.
        /// </returns>
        public int AddUser(string username)
        {
            if (!UserDict.ContainsKey(username))
            {
                UserDict.Add(username, new());
                return 0;
            }
            else
                return 1;
        }

        /// <summary>
        /// Renames an existing user with a new name.
        /// </summary>
        /// <param name="oldName">The existing username.</param>
        /// <param name="newName">The new username.</param>
        /// <param name="nodes">The <see cref="Nodes"/> object.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <returns>
        /// The task that represents the operation.
        /// An optional error message.
        /// </returns>
        public async Task<string?> RenameUser(string oldName, string newName, Nodes nodes, CancellationToken cancellationToken = default)
        {
            if (UserDict.ContainsKey(newName))
                return $"Error: the new username {newName} is already used. Please choose another username.";

            if (!UserDict.Remove(oldName, out var user))
                return $"Error: user {oldName} doesn't exist.";

            UserDict.Add(newName, user);

            var tasks = user.Memberships.Select(x => nodes.RenameUserInGroup(x.Key, oldName, newName, cancellationToken));
            var errMsgSB = new StringBuilder();

            await foreach (var finishedTask in Task.WhenEach(tasks))
            {
                var errMsg = await finishedTask;
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
        /// Removes the user from <see cref="UserDict"/>.
        /// </summary>
        /// <param name="username">The user to be removed from <see cref="UserDict"/>.</param>
        /// <returns>
        /// True if the user is successfully found and removed.
        /// Otherwise, false.
        /// </returns>
        public bool RemoveUser(string username) => UserDict.Remove(username);

        /// <summary>
        /// Gets the user associated with the ID.
        /// </summary>
        /// <param name="id">The user's UUID.</param>
        /// <param name="userEntry">The user's username and <see cref="User"/> object.</param>
        /// <returns>
        /// True if a user with the specified UUID is found.
        /// Otherwise, false.
        /// </returns>
        public bool TryGetUserById(string id, out KeyValuePair<string, User> userEntry)
        {
            var matchedUserEntries = UserDict.Where(x => x.Value.Uuid == id);
            userEntry = matchedUserEntries.FirstOrDefault();
            return matchedUserEntries.Any();
        }

        /// <summary>
        /// Adds the user to the specified group.
        /// Make sure to check if the target group exists
        /// before calling this method.
        /// </summary>
        /// <param name="user">Target user.</param>
        /// <param name="group">Target group.</param>
        /// <returns>0 when success. 1 when already in group. -1 when user not found.</returns>
        public int AddUserToGroup(string user, string group)
        {
            if (UserDict.TryGetValue(user, out var targetUser))
                return targetUser.AddToGroup(group) ? 0 : 1;
            else
                return -1;
        }

        /// <summary>
        /// Adds a group credential to the specified user.
        /// </summary>
        /// <returns>
        /// 0 for success.
        /// 2 for duplicated credential.
        /// -1 for non-existing target user.
        /// -2 for invalid userinfoBase64url.
        /// </returns>
        public int AddCredentialToUser(string user, string group, string method, string password)
        {
            if (UserDict.TryGetValue(user, out var targetUser))
                return targetUser.AddCredential(group, method, password);
            else
                return -1;
        }

        /// <summary>
        /// Generates a credential for the specified user.
        /// </summary>
        /// <param name="user">Username.</param>
        /// <param name="group">Group name.</param>
        /// <param name="method">Method name.</param>
        /// <param name="overwriteExisting">Whether to overwrite existing credential.</param>
        /// <returns>
        /// 0 for success.
        /// 2 for duplicated credential.
        /// -1 for non-existing target user.
        /// </returns>
        public int GenerateCredentialForUser(string user, string group, string method, bool overwriteExisting)
        {
            if (UserDict.TryGetValue(user, out var targetUser))
                return targetUser.GenerateCredential(group, method, overwriteExisting);
            else
                return -1;
        }

        /// <summary>
        /// Updates credential entries for all users when group name changes.
        /// </summary>
        /// <param name="oldGroupName">The old group name.</param>
        /// <param name="newGroupName">The new group name.</param>
        public void UpdateCredentialGroupsForAllUsers(string oldGroupName, string newGroupName)
        {
            foreach (var userEntry in UserDict)
            {
                var memberships = userEntry.Value.Memberships;

                if (memberships.Remove(oldGroupName, out var memberinfo))
                    memberships.Add(newGroupName, memberinfo);
            }
        }

        /// <summary>
        /// Removes the user from the group.
        /// </summary>
        /// <param name="user">Target user.</param>
        /// <param name="group">Target group.</param>
        /// <returns>0 when success. 1 when not in group. -2 when user not found.</returns>
        public int RemoveUserFromGroup(string user, string group)
        {
            if (UserDict.TryGetValue(user, out var targetUser))
                return targetUser.RemoveFromGroup(group) ? 0 : 1;
            else
                return -2;
        }

        /// <summary>
        /// Removes the user from all groups.
        /// </summary>
        /// <param name="user">Target user.</param>
        /// <returns>0 for success. -2 when user is not found.</returns>
        public int RemoveUserFromAllGroups(string user)
        {
            if (UserDict.TryGetValue(user, out var targetUser))
            {
                targetUser.Memberships.Clear();
                return 0;
            }
            else
                return -2;
        }

        /// <summary>
        /// Removes all members from the group.
        /// </summary>
        /// <param name="group">Target group.</param>
        public void RemoveAllUsersFromGroup(string group)
        {
            foreach (var userEntry in UserDict)
                userEntry.Value.RemoveFromGroup(group);
        }

        /// <summary>
        /// Removes all users from the specified groups.
        /// </summary>
        /// <param name="groups">Target groups.</param>
        public void RemoveAllUsersFromGroups(IEnumerable<string> groups)
        {
            foreach (var userEntry in UserDict)
                foreach (var group in groups)
                    userEntry.Value.RemoveFromGroup(group);
        }

        /// <summary>
        /// Removes group credential from user.
        /// </summary>
        /// <param name="user">Target user.</param>
        /// <param name="group">Target group.</param>
        /// <returns>
        /// 0 when success.
        /// 1 when no associated credential.
        /// -1 when user not in group.
        /// -2 when user not found.
        /// </returns>
        public int RemoveCredentialFromUser(string user, string group)
        {
            if (UserDict.TryGetValue(user, out var targetUser))
                return targetUser.RemoveCredential(group);
            else
                return -2;
        }

        /// <summary>
        /// Removes the user's all associated credentials.
        /// </summary>
        /// <param name="user">Target user.</param>
        /// <returns>0 for success. -2 when the user is not found.</returns>
        public int RemoveAllCredentialsFromUser(string user)
        {
            if (UserDict.TryGetValue(user, out var targetUser))
            {
                targetUser.RemoveAllCredentials();
                return 0;
            }
            else
                return -2;
        }

        /// <summary>
        /// Removes the group credential from all users.
        /// </summary>
        /// <param name="group">Target group.</param>
        public void RemoveCredentialsFromAllUsers(string group)
        {
            foreach (var userEntry in UserDict)
                userEntry.Value.RemoveCredential(group);
        }

        /// <summary>
        /// Removes credentials associated with the specified groups from all users.
        /// </summary>
        /// <param name="groups">A list of groups.</param>
        public void RemoveCredentialsFromAllUsers(IEnumerable<string> groups)
        {
            foreach (var userEntry in UserDict)
                foreach (var group in groups)
                    userEntry.Value.RemoveCredential(group);
        }

        /// <summary>
        /// Gets Shadowsocks URIs associated with a username.
        /// </summary>
        /// <param name="username">Target username.</param>
        /// <param name="nodes">The <see cref="Nodes"/> object.</param>
        /// <param name="groups">Only retrieve URLs for servers of these groups.</param>
        /// <returns>
        /// A list of the user's associated Shadowsocks URIs.
        /// Null if target user doesn't exist.
        /// </returns>
        public IEnumerable<Uri>? GetUserSSUris(string username, Nodes nodes, string[] groups)
        {
            if (UserDict.TryGetValue(username, out var user))
                return user.GetSSUris(this, nodes, groups);
            else
                return null;
        }

        /// <summary>
        /// Calculates data usage for all users.
        /// This method is intended to be called
        /// on data updates (e.g. Outline server pulls).
        /// </summary>
        /// <param name="nodes">The <see cref="Nodes"/> object.</param>
        public void CalculateDataUsageForAllUsers(Nodes nodes)
        {
            foreach (var userEntry in UserDict)
            {
                userEntry.Value.CalculateTotalDataUsage(userEntry.Key, nodes);
            }
        }

        /// <summary>
        /// Collects the user's data usage records from all groups
        /// and returns a list of data usage tuples.
        /// </summary>
        /// <param name="username">Target user.</param>
        /// <param name="nodes">The <see cref="Nodes"/> object.</param>
        /// <returns>
        /// A list of data usage records as tuples.
        /// Null if the user doesn't exist.
        /// </returns>
        public List<(string group, ulong bytesUsed, ulong bytesRemaining)>? GetUserDataUsage(string username, Nodes nodes)
        {
            if (UserDict.TryGetValue(username, out var user))
                return user.GetDataUsage(username, nodes);
            else
                return null;
        }

        /// <summary>
        /// Gets data usage records that contain
        /// each user's total data usage.
        /// </summary>
        /// <returns>A sequence of data usage records as tuples.</returns>
        public IEnumerable<(string username, ulong bytesUsed, ulong bytesRemaining)> GetDataUsageByUser()
            => UserDict.Select(userEntry => (userEntry.Key, userEntry.Value.BytesUsed, userEntry.Value.BytesRemaining));

        /// <summary>
        /// Sets the global data limit on the specified user.
        /// </summary>
        /// <param name="username">Target user.</param>
        /// <param name="dataLimit">The data limit in bytes.</param>
        /// <returns>
        /// 0 if the global data limit is successfully set on the user.
        /// -1 if the user doesn't exist.
        /// </returns>
        public int SetUserGlobalDataLimit(string username, ulong dataLimit)
        {
            if (UserDict.TryGetValue(username, out var user))
            {
                user.DataLimitInBytes = dataLimit;
                user.UpdateDataRemaining();
                return 0;
            }
            else
                return -1;
        }

        /// <summary>
        /// Sets the per-group data limit on the user.
        /// </summary>
        /// <param name="username">Target user.</param>
        /// <param name="dataLimit">The data limit in bytes.</param>
        /// <returns>
        /// 0 if the per-group data limit is successfully set on the user.
        /// -1 if the user doesn't exist.
        /// </returns>
        public int SetUserPerGroupDataLimit(string username, ulong dataLimit)
        {
            if (UserDict.TryGetValue(username, out var user))
            {
                user.PerGroupDataLimitInBytes = dataLimit;
                return 0;
            }
            else
                return -1;
        }

        /// <summary>
        /// Gets the data limit in effect on the user in the group.
        /// Group existence is not checked.
        /// Outline's user custom limit is not checked.
        /// </summary>
        /// <param name="username">Target user.</param>
        /// <param name="group">Target group.</param>
        /// <returns>The data limit in bytes.</returns>
        public ulong GetUserDataLimitInGroup(string username, string group)
            => UserDict.TryGetValue(username, out var user)
                ? user.GetDataLimitInGroup(group)
                : 0UL;

        /// <summary>
        /// Sets a custom data limit on the user in the group.
        /// </summary>
        /// <param name="username">Target user.</param>
        /// <param name="group">Target group.</param>
        /// <param name="dataLimit">The data limit in bytes.</param>
        /// <returns>
        /// 0 if the data limit is successfully set on the user in the group.
        /// -1 if the user doesn't exist.
        /// -2 if the user is not in the group.
        /// </returns>
        public int SetUserDataLimitInGroup(string username, string group, ulong dataLimit)
            => UserDict.TryGetValue(username, out var user) ? user.SetDataLimitInGroup(group, dataLimit) : -1;

        /// <summary>
        /// Loads users from Users.json.
        /// </summary>
        /// <param name="cancellationToken">A token that may be used to cancel the read operation.</param>
        /// <returns>
        /// A ValueTuple containing a <see cref="Users"/> object and an optional error message.
        /// </returns>
        public static async Task<(Users users, string? errMsg)> LoadUsersAsync(CancellationToken cancellationToken = default)
        {
            var (users, errMsg) = await FileHelper.LoadJsonAsync<Users>("Users.json", FileHelper.DataJsonSerializerOptions, cancellationToken);
            if (errMsg is null && users.Version != DefaultVersion)
            {
                users.UpdateUsers();
                errMsg = await SaveUsersAsync(users, cancellationToken);
            }
            return (users, errMsg);
        }

        /// <summary>
        /// Saves users to Users.json.
        /// </summary>
        /// <param name="users">The <see cref="Users"/> object to save.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the write operation.</param>
        /// <returns>
        /// An optional error message.
        /// Null if no errors occurred.
        /// </returns>
        public static Task<string?> SaveUsersAsync(Users users, CancellationToken cancellationToken = default)
            => FileHelper.SaveJsonAsync("Users.json", users, FileHelper.DataJsonSerializerOptions, false, false, cancellationToken);

        /// <summary>
        /// Updates the current object to the latest version.
        /// </summary>
        public void UpdateUsers()
        {
            switch (Version)
            {
                case 0: // generate UUID for each user
                    // already generated by the constructor
                    Version++;
                    goto case 1;

                case 1: // Userinfo_base64url => UserinfoBase64url
                    // not needed anymore
                    Version++;
                    goto case 2;

                case 2: // Credentials => Memberships, Credential? => MemberInfo
                    foreach (var userEntry in UserDict)
                    {
                        if (userEntry.Value.Credentials is not null)
                        {
                            foreach (var credEntry in userEntry.Value.Credentials)
                            {
                                userEntry.Value.Memberships.Add(credEntry.Key, credEntry.Value ?? new());
                            }

                            userEntry.Value.Credentials = null;
                        }
                    }
                    Version++;
                    goto default;

                default:
                    break;
            }
        }
    }
}
