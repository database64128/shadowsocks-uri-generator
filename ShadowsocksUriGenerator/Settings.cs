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
        /// Gets or sets the default sort rule for user data usage report.
        /// </summary>
        public SortBy UserDataUsageDefaultSortBy { get; set; }

        /// <summary>
        /// Gets or sets the default sort rule for group data usage report.
        /// </summary>
        public SortBy GroupDataUsageDefaultSortBy { get; set; }

        /// <summary>
        /// Gets or sets whether the generated servers list
        /// in an SIP008 JSON should be sorted by server name.
        /// Defaults to true.
        /// </summary>
        public bool OnlineConfigSortByName { get; set; }

        /// <summary>
        /// Gets or sets whether the user's online config file
        /// should be removed when the user is being removed.
        /// Defaults to true.
        /// </summary>
        public bool OnlineConfigCleanOnUserRemoval { get; set; }

        /// <summary>
        /// Gets or sets whether data usage metrics are updated
        /// from configured sources when generating online config.
        /// </summary>
        public bool OnlineConfigUpdateDataUsageOnGeneration { get; set; }

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

        /// <summary>
        /// Gets or sets whether changes made to local databases
        /// trigger deployments to linked Outline servers.
        /// </summary>
        public bool OutlineServerDeployOnChange { get; set; }

        /// <summary>
        /// Gets or sets whether to apply the global default user
        /// when associating with Outline servers.
        /// </summary>
        public bool OutlineServerApplyDefaultUserOnAssociation { get; set; }

        /// <summary>
        /// Gets or sets the global setting
        /// for Outline server's default access key's user.
        /// </summary>
        public string OutlineServerGlobalDefaultUser { get; set; }

        public Settings()
        {
            Version = DefaultVersion;
            UserDataUsageDefaultSortBy = SortBy.DefaultAscending;
            GroupDataUsageDefaultSortBy = SortBy.DataUsedDescending;
            OnlineConfigSortByName = true;
            OnlineConfigCleanOnUserRemoval = true;
            OnlineConfigUpdateDataUsageOnGeneration = true;
            OnlineConfigOutputDirectory = Guid.NewGuid().ToString();
            OnlineConfigDeliveryRootUri = "";
            OutlineServerDeployOnChange = false;
            OutlineServerApplyDefaultUserOnAssociation = true;
            OutlineServerGlobalDefaultUser = "";
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
        public static Task SaveSettingsAsync(Settings settings)
            => Utilities.SaveJsonAsync("Settings.json", settings, Utilities.commonJsonSerializerOptions);

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
