using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace shadowsocks_uri_generator
{
    public class Settings
    {
        /// <summary>
        /// Settings version number.
        /// </summary>
        public int Version { get; set; }

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
            Version = 1;
            OnlineConfigOutputDirectory = $"{Guid.NewGuid().ToString()}";
            OnlineConfigDeliveryRootUri = "";
        }

        /// <summary>
        /// Load settings from Settings.json.
        /// </summary>
        /// <returns>A Settings object.</returns>
        public static async Task<Settings> LoadSettingsAsync()
        {
            var jsonSerializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            };
            Settings settings = await Utilities.LoadJsonAsync<Settings>("Settings.json", jsonSerializerOptions);
            if (settings.Version != 1)
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
        {
            var jsonSerializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            };
            await Utilities.SaveJsonAsync("Settings.json", settings, jsonSerializerOptions);
        }

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
