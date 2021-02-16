using System.Collections.Generic;
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
        public int Version { get; set; }

        /// <summary>
        /// Gets or sets the Telegram bot token.
        /// </summary>
        public string BotToken { get; set; }

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
        /// Gets or sets whether to allow users to associate with
        /// their Telegram account. Authentication is done using
        /// user's UUID.
        /// Defaults to true.
        /// </summary>
        public bool AllowChatAssociation { get; set; }

        /// <summary>
        /// Gets or sets the dictionary to store chat association data.
        /// Key is Telegram user ID.
        /// Value is user UUID.
        /// </summary>
        public Dictionary<int, string> ChatAssociations { get; set; }

        public BotConfig()
        {
            Version = DefaultVersion;
            BotToken = "";
            UsersCanSeeAllUsers = false;
            UsersCanSeeAllGroups = false;
            UsersCanSeeGroupDataUsage = false;
            AllowChatAssociation = true;
            ChatAssociations = new();
        }

        /// <summary>
        /// Loads bot config from TelegramBotConfig.json.
        /// </summary>
        /// <returns>A <see cref="BotConfig"/> object.</returns>
        public static async Task<BotConfig> LoadBotConfigAsync()
        {
            BotConfig botConfig = await Utilities.LoadJsonAsync<BotConfig>("TelegramBotConfig.json", Utilities.commonJsonDeserializerOptions);
            if (botConfig.Version != DefaultVersion)
            {
                UpdateBotConfig(ref botConfig);
                await SaveBotConfigAsync(botConfig);
            }
            return botConfig;
        }

        /// <summary>
        /// Saves bot config to TelegramBotConfig.json.
        /// </summary>
        /// <param name="botConfig">The <see cref="BotConfig"/> object to save.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        public static Task SaveBotConfigAsync(BotConfig botConfig)
            => Utilities.SaveJsonAsync("TelegramBotConfig.json", botConfig, Utilities.commonJsonSerializerOptions);

        /// <summary>
        /// Updates the bot config version.
        /// </summary>
        /// <param name="botConfig">The <see cref="BotConfig"/> object to update.</param>
        public static void UpdateBotConfig(ref BotConfig botConfig)
        {
            switch (botConfig.Version)
            {
                case 0: // nothing to do
                    botConfig.Version++;
                    goto default; // go to the next update path
                default:
                    break;
            }
        }
    }
}
