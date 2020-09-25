using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace shadowsocks_uri_generator
{
    /// <summary>
    /// The class for storing user information in Users.json.
    /// </summary>
    public class Users
    {
        /// <summary>
        /// Configuration version number.
        /// 0 for the legacy config version
        /// without a version number property.
        /// Newer config versions start from 1.
        /// Update if older config is present.
        /// Throw error if config is newer than supported.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// The user dictionary.
        /// key is username.
        /// value is user info.
        /// </summary>
        [JsonPropertyName("Users")]
        public Dictionary<string, User> UserDict { get; set; }

        public Users()
        {
            Version = 1;
            UserDict = new Dictionary<string, User>();
        }

        /// <summary>
        /// Adds users to UserDict.
        /// </summary>
        /// <param name="users">The list of users to be added to UserDict.</param>
        /// <returns>A List of users successfully added to UserDict.</returns>
        public List<string> AddUsers(string[] users)
        {
            List<string> addedUsers = new List<string>();

            foreach (var user in users)
                if (!UserDict.ContainsKey(user))
                {
                    UserDict.Add(user, new User());
                    addedUsers.Add(user);
                }

            return addedUsers;
        }

        /// <summary>
        /// Removes users from UserDict
        /// </summary>
        /// <param name="users">The list of users to be removed from UserDict</param>
        public void RemoveUsers(string[] users)
        {
            foreach (var user in users)
                UserDict.Remove(user);
        }

        public int AddCredentialToUser(string user, string group, string method, string password, Nodes nodes)
        {
            if (UserDict.TryGetValue(user, out User? targetUser) && nodes.Groups.ContainsKey(group))
            {
                return targetUser.AddCredential(group, method, password);
            }
            else
                return -1;
        }

        public int AddCredentialToUser(string user, string group, string userinfo_base64url, Nodes nodes)
        {
            if (UserDict.TryGetValue(user, out User? targetUser) && nodes.Groups.ContainsKey(group))
            {
                return targetUser.AddCredential(group, userinfo_base64url);
            }
            else
                return -1;
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

        /// <summary>
        /// Get Shadowsocks URIs associated with a username.
        /// </summary>
        /// <param name="username">Target username.</param>
        /// <returns>
        /// A list of the user's associated Shadowsocks URIs.
        /// Empty list if target user not found or no associated nodes.</returns>
        public List<Uri> GetUserSSUris(string username, Nodes nodes)
        {
            if (UserDict.TryGetValue(username, out User? user))
            {
                return user.GetSSUris(nodes);
            }
            return new List<Uri>();
        }

        /// <summary>
        /// Load users from Users.json.
        /// </summary>
        /// <returns>A Users object.</returns>
        public static async Task<Users> LoadUsersAsync()
        {
            var jsonSerializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            };
            Users users = await Utilities.LoadJsonAsync<Users>("Users.json", jsonSerializerOptions);
            if (users.Version != 1)
            {
                UpdateUsers(ref users);
                await SaveUsersAsync(users);
            }
            return users;
        }

        /// <summary>
        /// Save users to Users.json.
        /// </summary>
        /// <param name="users">The Users object to save.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        public static async Task SaveUsersAsync(Users users)
        {
            var jsonSerializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            };
            await Utilities.SaveJsonAsync("Users.json", users, jsonSerializerOptions);
        }

        /// <summary>
        /// Update the users version.
        /// </summary>
        /// <param name="users">The users object to update.</param>
        public static void UpdateUsers(ref Users users)
        {
            switch (users.Version)
            {
                case 0: // generate UUID for each user
                    users.Version++;
                    // already generated by the constructor
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
        /// UUID of a user.
        /// Used for online configuration delivery (SIP008).
        /// </summary>
        public string Uuid { get; set; }

        /// <summary>
        /// The credential dictionary of the user.
        /// key is the associated group name.
        /// value is user's credential to the group's nodes.
        /// </summary>
        public Dictionary<string, Credential> Credentials { get; set; }

        public User()
        {
            Uuid = Guid.NewGuid().ToString();
            Credentials = new Dictionary<string, Credential>();
        }

        /// <summary>
        /// Adds a new credential to the user's credential dictionary.
        /// </summary>
        /// <param name="_group">The new credential's group name.</param>
        /// <param name="_method">Encryption method.</param>
        /// <param name="_password">Password.</param>
        /// <returns>0 for success. -1 for duplicated credential.</returns>
        public int AddCredential(string _group, string _method, string _password)
        {
            if (!Credentials.ContainsKey(_group))
            {
                var _credential = new Credential(_method, _password);
                Credentials.Add(_group, _credential);
                return 0;
            }
            return -1;
        }

        /// <summary>
        /// Adds a new credential to the user's credential dictionary.
        /// </summary>
        /// <param name="_group">The new credential's group name.</param>
        /// <param name="_userinfo_base64url">userinfo (method:password) in base64url</param>
        /// <returns>0 for success. -1 for duplicated credential or invalid userinfo base64url.</returns>
        public int AddCredential(string _group, string _userinfo_base64url)
        {
            try
            {
                if (!Credentials.ContainsKey(_group))
                {
                    var _credential = new Credential(_userinfo_base64url);
                    Credentials.Add(_group, _credential);
                    return 0;
                }
            }
            catch
            {
            }
            return -1;
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
            List<Uri> uris = new List<Uri>();
            foreach (var credEntry in Credentials)
            {
                var userinfo_base64url = credEntry.Value.Userinfo_base64url;
                if (nodes.Groups.TryGetValue(credEntry.Key, out Group? group)) // find credEntry's group
                {
                    foreach (var node in group.NodeDict)
                    {
                        var fragment = node.Key;
                        var host = node.Value.Host;
                        var port = node.Value.Port;
                        var plugin = node.Value.Plugin;
                        var pluginOpts = node.Value.PluginOpts;
                        uris.Add(SSUriBuilder(userinfo_base64url, host, port, fragment, plugin, pluginOpts));
                    }
                }
                else
                    continue; // ignoring is intentional, as groups may get removed.
            }
            return uris;
        }

        public static Uri SSUriBuilder(string userinfo_base64url, string host, int port, string fragment, string plugin = "", string pluginOpts = "")
        {
            UriBuilder ssUriBuilder = new UriBuilder("ss", host, port)
            {
                UserName = userinfo_base64url,
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
    /// userinfo = method + ":" + password.
    /// userinfo_base64 = base64url(userinfo).
    /// </summary>
    public class Credential
    {
        public string Method { get; set; }
        public string Password { get; set; }
        public string Userinfo { get; set; }
        public string Userinfo_base64url { get; set; }

        /// <summary>
        /// Parameterless constructor for System.Text.Json
        /// </summary>
        public Credential()
        {
            Method = "";
            Password = "";
            Userinfo = "";
            Userinfo_base64url = "";
        }

        public Credential(string _method, string _password)
        {
            Method = _method;
            Password = _password;
            Userinfo = _method + ":" + _password;
            Userinfo_base64url = Base64UserinfoEncoder(Userinfo);
        }

        public Credential(string _userinfo_base64url)
        {
            Userinfo_base64url = _userinfo_base64url;
            Userinfo = Base64UserinfoDecoder(_userinfo_base64url);
            string[] methodPasswordArray = Userinfo.Split(':');
            Method = methodPasswordArray[0];
            if (methodPasswordArray.Length == 2)
                Password = methodPasswordArray[1];
            else
                Password = "";
        }

        public static string Base64UserinfoEncoder(string _userinfo)
        {
            byte[] userinfoBytes = Encoding.UTF8.GetBytes(_userinfo);
            return Convert.ToBase64String(userinfoBytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        public static string Base64UserinfoDecoder(string _base64userinfo)
        {
            string base64userinfo = _base64userinfo.Replace('_', '/').Replace('-', '+');
            base64userinfo = base64userinfo.PadRight(base64userinfo.Length + (4 - base64userinfo.Length % 4) % 4, '=');
            byte[] userinfoBytes = Convert.FromBase64String(base64userinfo);
            return Encoding.UTF8.GetString(userinfoBytes);
        }
    }
}
