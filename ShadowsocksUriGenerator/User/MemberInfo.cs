using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json.Serialization;

namespace ShadowsocksUriGenerator
{
    /// <summary>
    /// Stores a user's credential and data limit in a group.
    /// </summary>
    public class MemberInfo : IEquatable<MemberInfo>
    {
        public string Method { get; set; }

        public string Password { get; set; }

        [JsonIgnore]
        public string Userinfo => $"{Method}:{Password}";

        [JsonIgnore]
        public string UserinfoBase64url => Base64UserinfoEncoder(Userinfo);

        /// <summary>
        /// Gets whether the member info contains credential.
        /// </summary>
        [JsonIgnore]
        public bool HasCredential => !string.IsNullOrEmpty(Method) && !string.IsNullOrEmpty(Password);

        /// <summary>
        /// Gets or sets the data limit in bytes
        /// enforced on the user in the group.
        /// Do not set this if it's a global or general per-key limit.
        /// Set this if the limit is specifically targeting this user in the group.
        /// </summary>
        public ulong DataLimitInBytes { get; set; }

        /// <summary>
        /// Parameterless constructor for System.Text.Json
        /// </summary>
        public MemberInfo()
        {
            Method = "";
            Password = "";
        }

        public MemberInfo(string method, string password, ulong dataLimitInBytes = 0UL)
        {
            Method = method;
            Password = password;
            DataLimitInBytes = dataLimitInBytes;
        }

        public bool Equals(MemberInfo? other) => Method == other?.Method && Password == other?.Password;

        public override bool Equals(object? obj) => Equals(obj as MemberInfo);

        public override int GetHashCode() => Userinfo.GetHashCode();

        /// <summary>
        /// Clears the credential information.
        /// </summary>
        public void ClearCredential()
        {
            Method = "";
            Password = "";
        }

        public static bool TryParseFromUserinfoBase64url(
            string userinfoBase64url,
            [NotNullWhen(true)] out string? method,
            [NotNullWhen(true)] out string? password)
        {
            var userinfo = Base64UserinfoDecoder(userinfoBase64url);
            var methodPasswordArray = userinfo.Split(':', 2);

            if (methodPasswordArray.Length == 2)
            {
                method = methodPasswordArray[0];
                password = methodPasswordArray[1];
                if (!string.IsNullOrEmpty(method) && !string.IsNullOrEmpty(password))
                    return true;
                else
                    return false;
            }
            else
            {
                method = null;
                password = null;
                return false;
            }
        }

        public static bool TryParseFromUserinfoBase64url(string userinfoBase64url, [NotNullWhen(true)] out MemberInfo? memberInfo)
        {
            if (TryParseFromUserinfoBase64url(userinfoBase64url, out var method, out var password))
            {
                memberInfo = new(method, password);
                return true;
            }
            else
            {
                memberInfo = null;
                return false;
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
