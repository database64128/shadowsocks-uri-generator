using System;
using System.Collections.Generic;
using System.Text;
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
        /// Adds a group credential to the specified user.
        /// </summary>
        /// <returns>
        /// 0 for success.
        /// 1 for duplicated credential.
        /// -1 for non-existing target user or group.
        /// -2 for invalid userinfoBase64url.
        /// </returns>
        public int AddCredentialToUser(string user, string group, string method, string password, Nodes nodes)
        {
            if (UserDict.TryGetValue(user, out User? targetUser) && nodes.Groups.ContainsKey(group))
            {
                return targetUser.AddCredential(group, method, password);
            }
            else
                return -1;
        }

        /// <inheritdoc cref="AddCredentialToUser(string, string, string, string, Nodes)"/>
        public int AddCredentialToUser(string user, string group, string userinfoBase64url, Nodes nodes)
        {
            if (UserDict.TryGetValue(user, out User? targetUser) && nodes.Groups.ContainsKey(group))
            {
                return targetUser.AddCredential(group, userinfoBase64url);
            }
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

        public int RemoveCredentialsFromUser(string user, string[] groups)
        {
            if (UserDict.TryGetValue(user, out User? targetUser))
            {
                targetUser.RemoveCredentials(groups);
                return 0;
            }
            else
                return -1;
        }

        public void RemoveCredentialsFromAllUsers(string[] groups)
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
                            credEntry.Value.UserinfoBase64url = Credential.Base64UserinfoEncoder(credEntry.Value.Userinfo);
                    users.Version++;
                    goto default; // go to the next update path
                default:
                    break;
            }
        }
    }

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
        /// Gets or sets the credential dictionary of the user.
        /// key is the associated group name.
        /// value is user's credential to the group's nodes.
        /// </summary>
        public Dictionary<string, Credential> Credentials { get; set; }

        public User()
        {
            Uuid = Guid.NewGuid().ToString();
            Credentials = new();
        }

        /// <summary>
        /// Adds a new credential to the user's credential dictionary.
        /// </summary>
        /// <param name="group">The new credential's group name.</param>
        /// <param name="method">Encryption method.</param>
        /// <param name="password">Password.</param>
        /// <returns>0 for success. 1 for duplicated credential.</returns>
        public int AddCredential(string group, string method, string password)
        {
            if (!Credentials.ContainsKey(group))
            {
                var credential = new Credential(method, password);
                Credentials.Add(group, credential);
                return 0;
            }
            return 1;
        }

        /// <summary>
        /// Adds a new credential to the user's credential dictionary.
        /// </summary>
        /// <param name="group">The new credential's group name.</param>
        /// <param name="userinfoBase64url">userinfo (method:password) in base64url</param>
        /// <returns>0 for success. 1 for duplicated credential. -2 for invalid userinfo base64url.</returns>
        public int AddCredential(string group, string userinfoBase64url)
        {
            try
            {
                if (!Credentials.ContainsKey(group))
                {
                    var credential = new Credential(userinfoBase64url);
                    Credentials.Add(group, credential);
                    return 0;
                }
                return 1;
            }
            catch
            {
                return -2;
            }
        }

        /// <summary>
        /// Removes credentials from the user's credential dictionary.
        /// </summary>
        /// <param name="groups">
        /// The list of group entries to be removed from the user's credential dictionary.
        /// </param>
        public void RemoveCredentials(string[] groups)
        {
            foreach (var group in groups)
                Credentials.Remove(group);
        }

        public List<Uri> GetSSUris(Nodes nodes)
        {
            List<Uri> uris = new();
            foreach (var credEntry in Credentials)
            {
                var userinfoBase64url = credEntry.Value.UserinfoBase64url;
                if (nodes.Groups.TryGetValue(credEntry.Key, out Group? group)) // find credEntry's group
                {
                    foreach (var node in group.NodeDict)
                    {
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

    /// <summary>
    /// Each credential corresponds to a node group.
    /// Userinfo = Method + ":" + Password.
    /// UserinfoBase64url = base64url(Userinfo).
    /// </summary>
    public class Credential
    {
        public string Method { get; set; }
        public string Password { get; set; }
        public string Userinfo { get; set; }
        public string UserinfoBase64url { get; set; }

        /// <summary>
        /// Parameterless constructor for System.Text.Json
        /// </summary>
        public Credential()
        {
            Method = "";
            Password = "";
            Userinfo = "";
            UserinfoBase64url = "";
        }

        public Credential(string method, string password)
        {
            Method = method;
            Password = password;
            Userinfo = method + ":" + password;
            UserinfoBase64url = Base64UserinfoEncoder(Userinfo);
        }

        public Credential(string userinfoBase64url)
        {
            UserinfoBase64url = userinfoBase64url;
            Userinfo = Base64UserinfoDecoder(userinfoBase64url);
            var methodPasswordArray = Userinfo.Split(':', 2);
            if (methodPasswordArray.Length == 2)
            {
                Method = methodPasswordArray[0];
                Password = methodPasswordArray[1];
            }
            else
            {
                throw new ArgumentException("Cannot parse into method:password.", nameof(userinfoBase64url));
            }
        }

        public static string Base64UserinfoEncoder(string userinfo)
        {
            var userinfoBytes = Encoding.UTF8.GetBytes(userinfo);
            return Convert.ToBase64String(userinfoBytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        public static string Base64UserinfoDecoder(string userinfoBase64url)
        {
            var parsedUserinfoBase64 = userinfoBase64url.Replace('_', '/').Replace('-', '+');
            parsedUserinfoBase64 = parsedUserinfoBase64.PadRight(parsedUserinfoBase64.Length + (4 - parsedUserinfoBase64.Length % 4) % 4, '=');
            var userinfoBytes = Convert.FromBase64String(parsedUserinfoBase64);
            return Encoding.UTF8.GetString(userinfoBytes);
        }
    }
}
