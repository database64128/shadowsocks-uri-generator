using System;
using System.Text;

namespace ShadowsocksUriGenerator
{
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
