using System.Collections.Generic;

namespace ShadowsocksUriGenerator.Server.Utils
{
    public static class FilterHelper
    {
        /// <summary>
        /// Attempts to resolve the array of usernames
        /// into an array of user IDs.
        /// </summary>
        /// <param name="users">The <see cref="Users"/> object.</param>
        /// <param name="usernames">The array of usernames to resolve.</param>
        /// <param name="ids">The resolved user IDs. Contains all resolved user IDs, no matter the return value.</param>
        /// <returns>
        /// True if all usernames are successfully resolved.
        /// Otherwise false.
        /// </returns>
        public static bool TryGetUserIds(Users users, string[] usernames, out string[] ids)
        {
            var result = true;
            var idList = new List<string>();

            foreach (var username in usernames)
            {
                if (users.UserDict.TryGetValue(username, out var user))
                {
                    idList.Add(user.Uuid);
                }
                else
                {
                    result = false;
                }
            }

            ids = idList.ToArray();
            return result;
        }
    }
}
