using System;
using System.Threading.Tasks;

namespace ShadowsocksUriGenerator
{
    public class Settings
    {
        /// Gets the default configuration version
        /// used by this version of the app.
        /// </summary>
        public static int DefaultVersion => 1;

        /// <summary>
        /// Gets or sets the settings version number.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Gets or sets whether the generated servers list
        /// in an SIP008 JSON should be sorted by server name.
        /// Defaults to true.
        /// </summary>
        public bool OnlineConfigSortByName { get; set; }

        /// <summary>
        /// Gets or sets the output directory path
        /// for online configuration delivery (SIP008).
        /// Must NOT end with '/' or '\'.
        /// </summary>
        public string OnlineConfigOutputDirectory { get; set; }

        /// <summary>
        /// Gets or sets the URI of the folder which contains
        /// user configuration files for online configuration delivery.
        /// Must NOT end with '/'.
        /// </summary>
        public string OnlineConfigDeliveryRootUri { get; set; }

        public Settings()
        {
            Version = DefaultVersion;
            OnlineConfigSortByName = true;
            OnlineConfigOutputDirectory = Guid.NewGuid().ToString();
            OnlineConfigDeliveryRootUri = "";
        }

        /// <summary>
        /// Loads settings from Settings.json.
        /// </summary>
        /// <returns>A <see cref="Settings"/> object.</returns>
        public static async Task<Settings> LoadSettingsAsync()
        {
            Settings settings = await Utilities.LoadJsonAsync<Settings>("Settings.json", Utilities.commonJsonDeserializerOptions);
            if (settings.Version != DefaultVersion)
            {
                UpdateSettings(ref settings);
                await SaveSettingsAsync(settings);
            }
            return settings;
        }

        /// <summary>
        /// Saves settings to Settings.json.
        /// </summary>
        /// <param name="settings">The <see cref="Settings"/> object to save.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        public static async Task SaveSettingsAsync(Settings settings)
            => await Utilities.SaveJsonAsync("Settings.json", settings, Utilities.commonJsonSerializerOptions);

        /// <summary>
        /// Updates the settings version.
        /// </summary>
        /// <param name="settings">The <see cref="Settings"/> object to update.</param>
        public static void UpdateSettings(ref Settings settings)
        {
            switch (settings.Version)
            {
                case 0: // nothing to do
                    settings.Version++;
                    goto default; // go to the next update path
                default:
                    break;
            }
        }
    }
}
