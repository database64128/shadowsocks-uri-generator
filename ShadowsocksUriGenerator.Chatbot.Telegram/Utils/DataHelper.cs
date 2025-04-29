using ShadowsocksUriGenerator.Data;
using System.Diagnostics.CodeAnalysis;

namespace ShadowsocksUriGenerator.Chatbot.Telegram.Utils
{
    public static class DataHelper
    {
        public static bool TryLocateUserFromUuid(string userUuid, Users users, [NotNullWhen(true)] out KeyValuePair<string, User>? userEntry)
        {
            var userSearchResult = users.UserDict.Where(x => string.Equals(x.Value.Uuid, userUuid, StringComparison.OrdinalIgnoreCase));
            if (userSearchResult.Any())
            {
                userEntry = userSearchResult.First();
                return true;
            }
            else
            {
                userEntry = null;
                return false;
            }
        }
    }
}
