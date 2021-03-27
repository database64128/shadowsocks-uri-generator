using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace ShadowsocksUriGenerator
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
        public static int DefaultVersion => 2;

        /// <summary>
        /// Gets or sets the configuration version number.
        /// 0 for the legacy config version
        /// without a version number property.
        /// Newer config versions start from 1.
        /// Update if older config is present.
        /// Throw error if config is newer than supported.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Gets or sets the user dictionary.
        /// key is username.
        /// value is user info.
        /// </summary>
        [JsonPropertyName("Users")]
        public Dictionary<string, User> UserDict { get; set; }

        public Users()
        {
            Version = DefaultVersion;
            UserDict = new();
        }

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
        /// <returns>
        /// 0 when success.
        /// -1 when old username is not found.
        /// -2 when new username already exists.
        /// -3 when an error occurred while deploying the change to Outline server.
        /// </returns>
        public async Task<int> RenameUser(string oldName, string newName, Nodes nodes)
        {
            if (UserDict.ContainsKey(newName))
                return -2;
            if (!UserDict.Remove(oldName, out var user))
                return -1;
            UserDict.Add(newName, user);
            var tasks = user.Credentials.Select(async x => await nodes.RenameUserInGroup(x.Key, oldName, newName));
            var results = await Task.WhenAll(tasks);
            if (results.Any(x => x < 0))
                return -3;
            else
                return 0;
        }

        /// <summary>
        /// Removes the user from <see cref="UserDict"/>.
        /// </summary>
        /// <param name="user">The user to be removed from <see cref="UserDict"/>.</param>
        /// <returns>
        /// <see cref="true"/> if the user is successfully found and removed.
        /// Otherwise, <see cref="false"/>.
        /// </returns>
        public bool RemoveUser(string username) => UserDict.Remove(username);

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

        /// <inheritdoc cref="AddCredentialToUser(string, string, string, string)"/>
        public int AddCredentialToUser(string user, string group, string userinfoBase64url)
        {
            if (UserDict.TryGetValue(user, out var targetUser))
                return targetUser.AddCredential(group, userinfoBase64url);
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
                var credentials = userEntry.Value.Credentials;
                if (credentials.Remove(oldGroupName, out var credential))
                    credentials.Add(newGroupName, credential);
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
                targetUser.Credentials.Clear();
                return 0;
            }
            else
                return -2;
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
        /// <returns>0 when success. -1 when user not in group. -2 when user not found.</returns>
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
                foreach (var credEntry in targetUser.Credentials)
                    targetUser.Credentials[credEntry.Key] = null;
                return 0;
            }
            else
                return -2;
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
        /// <returns>
        /// A list of the user's associated Shadowsocks URIs.
        /// Null if target user doesn't exist.
        /// </returns>
        public List<Uri>? GetUserSSUris(string username, Nodes nodes, params string[] groups)
        {
            if (UserDict.TryGetValue(username, out var user))
                return user.GetSSUris(nodes, groups);
            else
                return null;
        }

        /// <summary>
        /// Calculates data usage for all users.
        /// This method is intended to be called
        /// by online config generator.
        /// </summary>
        /// <param name="nodes"></param>
        public void CalculateDataUsageForAllUsers(Nodes nodes)
        {
            foreach (var userEntry in UserDict)
                userEntry.Value.CalculateTotalDataUsage(userEntry.Key, nodes);
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
        /// Calculates total data usage for all users
        /// and returns a list of total data usage tuples of all users.
        /// </summary>
        /// <param name="nodes">The <see cref="Nodes"/> object.</param>
        /// <returns>A list of data usage records as tuples.</returns>
        public List<(string username, ulong bytesUsed, ulong bytesRemaining)> GetDataUsageByUser(Nodes nodes)
        {
            List<(string username, ulong bytesUsed, ulong bytesRemaining)> records = new();
            foreach (var userEntry in UserDict)
            {
                userEntry.Value.CalculateTotalDataUsage(userEntry.Key, nodes);
                records.Add((userEntry.Key, userEntry.Value.BytesUsed, userEntry.Value.BytesRemaining));
            }
            return records;
        }

        /// <summary>
        /// Sets the data limit to the specified user.
        /// </summary>
        /// <param name="dataLimit">The data limit in bytes.</param>
        /// <param name="username">Target user.</param>
        /// <param name="groups">Only set for these groups.</param>
        /// <returns>0 on success. -1 on user not found. -2 on group not found.</returns>
        public int SetDataLimitToUser(ulong dataLimit, string username, string[]? groups = null)
        {
            if (UserDict.TryGetValue(username, out var user))
                return user.SetDataLimit(dataLimit, groups);
            else
                return -1;
        }

        /// <summary>
        /// Loads users from Users.json.
        /// </summary>
        /// <param name="cancellationToken">A token that may be used to cancel the read operation.</param>
        /// <returns>
        /// A ValueTuple containing a <see cref="Users"/> object and an error message.
        /// </returns>
        public static async Task<(Users, string? errMsg)> LoadUsersAsync(CancellationToken cancellationToken = default)
        {
            var (users, errMsg) = await Utilities.LoadJsonAsync<Users>("Users.json", Utilities.commonJsonDeserializerOptions, cancellationToken);
            if (errMsg is null && users.Version != DefaultVersion)
            {
                UpdateUsers(ref users);
                errMsg = await SaveUsersAsync(users, cancellationToken);
            }
            return (users, errMsg);
        }

        /// <summary>
        /// Saves users to Users.json.
        /// </summary>
        /// <param name="users">The <see cref="Users"/> object to save.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the write operation.</param>
        /// <returns>An error message. Null if no errors occurred.</returns>
        public static Task<string?> SaveUsersAsync(Users users, CancellationToken cancellationToken = default)
            => Utilities.SaveJsonAsync("Users.json", users, Utilities.commonJsonSerializerOptions, cancellationToken);

        /// <summary>
        /// Updates the users version.
        /// </summary>
        /// <param name="users">The <see cref="Users"/> object to update.</param>
        public static void UpdateUsers(ref Users users)
        {
            switch (users.Version)
            {
                case 0: // generate UUID for each user
                    // already generated by the constructor
                    users.Version++;
                    goto case 1;
                case 1: // Userinfo_base64url => UserinfoBase64url
                    foreach (var userEntry in users.UserDict)
                        foreach (var credEntry in userEntry.Value.Credentials)
                        {
                            if (credEntry.Value == null)
                                continue;
                            credEntry.Value.UserinfoBase64url = Credential.Base64UserinfoEncoder(credEntry.Value.Userinfo);
                        }
                    users.Version++;
                    goto default; // go to the next update path
                default:
                    break;
            }
        }
    }
}
