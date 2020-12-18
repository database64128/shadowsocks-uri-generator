using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
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
        /// Adds users to UserDict.
        /// </summary>
        /// <param name="users">The list of users to be added to <see cref="UserDict"/>.</param>
        /// <returns>A List of users successfully added to <see cref="UserDict"/>.</returns>
        public List<string> AddUsers(string[] users)
        {
            List<string> addedUsers = new();

            foreach (var user in users)
                if (!UserDict.ContainsKey(user))
                {
                    UserDict.Add(user, new());
                    addedUsers.Add(user);
                }

            return addedUsers;
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
        /// </returns>
        public int RenameUser(string oldName, string newName)
        {
            if (UserDict.ContainsKey(newName))
                return -2;
            if (!UserDict.Remove(oldName, out var user))
                return -1;
            UserDict.Add(newName, user);
            return 0;
        }

        /// <summary>
        /// Removes users from <see cref="UserDict"/>.
        /// </summary>
        /// <param name="users">The list of users to be removed from <see cref="UserDict"/>.</param>
        public void RemoveUsers(string[] users)
        {
            foreach (var user in users)
                UserDict.Remove(user);
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
            if (UserDict.TryGetValue(user, out User? targetUser))
                return targetUser.AddCredential(group, method, password);
            else
                return -1;
        }

        /// <inheritdoc cref="AddCredentialToUser(string, string, string, string)"/>
        public int AddCredentialToUser(string user, string group, string userinfoBase64url)
        {
            if (UserDict.TryGetValue(user, out User? targetUser))
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
        /// Removes group credential from user.
        /// </summary>
        /// <param name="user">Target user.</param>
        /// <param name="group">Target group.</param>
        /// <returns>0 when success. -1 when user not in group. -2 when user not found.</returns>
        public int RemoveCredentialFromUser(string user, string group)
        {
            if (UserDict.TryGetValue(user, out User? targetUser))
                return targetUser.RemoveCredential(group);
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
                userEntry.Value.RemoveCredentials(groups);
        }

        /// <summary>
        /// Gets Shadowsocks URIs associated with a username.
        /// </summary>
        /// <param name="username">Target username.</param>
        /// <returns>
        /// A list of the user's associated Shadowsocks URIs.
        /// Empty list if target user not found or no associated nodes.
        /// </returns>
        public List<Uri> GetUserSSUris(string username, Nodes nodes)
        {
            if (UserDict.TryGetValue(username, out User? user))
            {
                return user.GetSSUris(nodes);
            }
            return new();
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
            if (UserDict.TryGetValue(username, out User? user))
                return user.SetDataLimit(dataLimit, groups);
            else
                return -1;
        }

        /// <summary>
        /// Loads users from Users.json.
        /// </summary>
        /// <returns>A <see cref="Users"/> object.</returns>
        public static async Task<Users> LoadUsersAsync()
        {
            var users = await Utilities.LoadJsonAsync<Users>("Users.json", Utilities.commonJsonDeserializerOptions);
            if (users.Version != DefaultVersion)
            {
                UpdateUsers(ref users);
                await SaveUsersAsync(users);
            }
            return users;
        }

        /// <summary>
        /// Saves users to Users.json.
        /// </summary>
        /// <param name="users">The <see cref="Users"/> object to save.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        public static async Task SaveUsersAsync(Users users)
            => await Utilities.SaveJsonAsync("Users.json", users, Utilities.commonJsonSerializerOptions);

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
