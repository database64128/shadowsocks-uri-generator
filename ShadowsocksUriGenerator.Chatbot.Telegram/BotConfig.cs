using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ShadowsocksUriGenerator.Chatbot.Telegram
{
    public class BotConfig
    {
        /// Gets the default bot config version
        /// used by this version of the app.
        /// </summary>
        public static int DefaultVersion => 1;

        /// <summary>
        /// Gets or sets the bot config version number.
        /// </summary>
        public int Version { get; set; } = DefaultVersion;

        /// <summary>
        /// Gets or sets the Telegram bot token.
        /// </summary>
        public string BotToken { get; set; } = "";

        /// <summary>
        /// Gets or sets the name of the service.
        /// The service name will be displayed in the welcome message.
        /// </summary>
        public string ServiceName { get; set; } = "";

        /// <summary>
        /// Gets or sets whether to allow any user to
        /// see all registered users.
        /// Defaults to false.
        /// </summary>
        public bool UsersCanSeeAllUsers { get; set; }

        /// <summary>
        /// Gets or sets whether to allow any user to
        /// see all server groups.
        /// Defaults to false.
        /// Users can only see groups they are in.
        /// Change to true to allow everyone to see every group.
        /// </summary>
        public bool UsersCanSeeAllGroups { get; set; }

        /// <summary>
        /// Gets or sets whether users are allowed
        /// to query group data usage metrics.
        /// Defaults to false.
        /// </summary>
        public bool UsersCanSeeGroupDataUsage { get; set; }

        /// <summary>
        /// Gets or sets whether users are allowed
        /// to see other group member's data limit.
        /// Defaults to false;
        /// </summary>
        public bool UsersCanSeeGroupDataLimit { get; set; }

        /// <summary>
        /// Gets or sets whether to allow users to associate with
        /// their Telegram account. Authentication is done using
        /// user's UUID.
        /// Defaults to true.
        /// </summary>
        public bool AllowChatAssociation { get; set; } = true;

        /// <summary>
        /// Gets or sets the dictionary to store chat association data.
        /// Key is Telegram user ID.
        /// Value is user UUID.
        /// </summary>
        public Dictionary<long, string> ChatAssociations { get; set; } = new();

        /// <summary>
        /// Loads bot config from TelegramBotConfig.json.
        /// </summary>
        /// <param name="cancellationToken">A token that may be used to cancel the read operation.</param>
        /// <returns>
        /// A ValueTuple containing a <see cref="BotConfig"/> object and an optional error message.
        /// </returns>
        public static async Task<(BotConfig botConfig, string? errMsg)> LoadBotConfigAsync(CancellationToken cancellationToken = default)
        {
            var (botConfig, errMsg) = await FileHelper.LoadJsonAsync<BotConfig>("TelegramBotConfig.json", FileHelper.commonJsonDeserializerOptions, cancellationToken);
            if (errMsg is null && botConfig.Version != DefaultVersion)
            {
                botConfig.UpdateBotConfig();
                errMsg = await SaveBotConfigAsync(botConfig, cancellationToken);
            }
            return (botConfig, errMsg);
        }

        /// <summary>
        /// Saves bot config to TelegramBotConfig.json.
        /// </summary>
        /// <param name="botConfig">The <see cref="BotConfig"/> object to save.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the write operation.</param>
        /// <returns>
        /// An optional error message.
        /// Null if no errors occurred.
        /// </returns>
        public static Task<string?> SaveBotConfigAsync(BotConfig botConfig, CancellationToken cancellationToken = default)
            => FileHelper.SaveJsonAsync("TelegramBotConfig.json", botConfig, FileHelper.commonJsonSerializerOptions, false, false, cancellationToken);

        /// <summary>
        /// Updates the current object to the latest version.
        /// </summary>
        public void UpdateBotConfig()
        {
            switch (Version)
            {
                case 0: // nothing to do
                    Version++;
                    goto default; // go to the next update path
                default:
                    break;
            }
        }
    }
}
