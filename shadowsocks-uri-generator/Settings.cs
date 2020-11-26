using System;
using System.Threading.Tasks;

namespace shadowsocks_uri_generator
{
    public class Settings
    {
        /// Gets the default configuration version
        /// used by this version of the app.
        /// </summary>
        public static int DefaultVersion => 1;

        /// <summary>
        /// Settings version number.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Decides if the generated servers list
        /// in an SIP008 JSON should be sorted by server name.
        /// Defaults to true.
        /// </summary>
        public bool OnlineConfigSortByName { get; set; }

        /// <summary>
        /// The output directory path
        /// for online configuration delivery (SIP008).
        /// Must NOT end with '/' or '\'.
        /// </summary>
        public string OnlineConfigOutputDirectory { get; set; }

        /// <summary>
        /// The URI of the folder which contains
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
        /// Load settings from Settings.json.
        /// </summary>
        /// <returns>A Settings object.</returns>
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
        /// Save settings to Settings.json.
        /// </summary>
        /// <param name="settings">The Settings object to save.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        public static async Task SaveSettingsAsync(Settings settings)
            => await Utilities.SaveJsonAsync("Settings.json", settings, Utilities.commonJsonSerializerOptions);

        /// <summary>
        /// Update the settings version.
        /// </summary>
        /// <param name="settings">The settings object to update.</param>
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
