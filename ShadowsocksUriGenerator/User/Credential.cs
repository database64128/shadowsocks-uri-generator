using System;
using System.Text;
using System.Text.Json.Serialization;

namespace ShadowsocksUriGenerator
{
    /// <summary>
    /// Stores user credential information
    /// that can be used to connect to nodes.
    /// </summary>
    public class Credential : IEquatable<Credential>
    {
        public string Method { get; set; }
        public string Password { get; set; }
        [JsonIgnore]
        public string Userinfo => $"{Method}:{Password}";
        [JsonIgnore]
        public string UserinfoBase64url => Base64UserinfoEncoder(Userinfo);

        /// <summary>
        /// Parameterless constructor for System.Text.Json
        /// </summary>
        public Credential()
        {
            Method = "";
            Password = "";
        }

        public Credential(string method, string password)
        {
            Method = method;
            Password = password;
        }

        public Credential(string userinfoBase64url)
        {
            var userinfo = Base64UserinfoDecoder(userinfoBase64url);
            var methodPasswordArray = userinfo.Split(':', 2);
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

        public bool Equals(Credential? other) => Method == other?.Method && Password == other?.Password;

        public override bool Equals(object? obj) => Equals(obj as Credential);

        public override int GetHashCode() => Userinfo.GetHashCode();

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
