using ShadowsocksUriGenerator.Utils;

namespace ShadowsocksUriGenerator
{
    public class Settings
    {
        /// <summary>
        /// Gets the default configuration version
        /// used by this version of the app.
        /// </summary>
        public static int DefaultVersion => 1;

        /// <summary>
        /// Gets or sets the settings version number.
        /// Defaults to <see cref="DefaultVersion"/>.
        /// </summary>
        public int Version { get; set; } = DefaultVersion;

        /// <summary>
        /// Gets or sets the default sort rule for user data usage report.
        /// Defaults to <see cref="SortBy.DefaultAscending"/>.
        /// </summary>
        public SortBy UserDataUsageDefaultSortBy { get; set; } = SortBy.DefaultAscending;

        /// <summary>
        /// Gets or sets the default sort rule for group data usage report.
        /// Defaults to <see cref="SortBy.DataUsedDescending"/>.
        /// </summary>
        public SortBy GroupDataUsageDefaultSortBy { get; set; } = SortBy.DataUsedDescending;

        /// <summary>
        /// Gets or sets whether online config should sort servers by name.
        /// Defaults to true.
        /// </summary>
        public bool OnlineConfigSortByName { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the legacy SIP008 online config static file generator
        /// should generate per-group SIP008 delivery JSON in addition to the single JSON
        /// that contains all associated servers of the user.
        /// Defaults to false.
        /// </summary>
        public bool OnlineConfigDeliverByGroup { get; set; }

        /// <summary>
        /// Gets or sets whether the user's generated static online config files
        /// should be removed when the user is being removed.
        /// Defaults to true.
        /// </summary>
        public bool OnlineConfigCleanOnUserRemoval { get; set; } = true;

        /// <summary>
        /// Gets or sets the output directory of the
        /// legacy SIP008 online config static file generator
        /// MUST NOT end with '/' or '\'.
        /// Defaults to a randomly generated UUID.
        /// </summary>
        public string OnlineConfigOutputDirectory { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the base access URL of the SIP008 static file delivery links.
        /// Must NOT end with '/'.
        /// Defaults to an empty string.
        /// </summary>
        public string OnlineConfigDeliveryRootUri { get; set; } = "";

        /// <summary>
        /// Gets or sets whether to apply the global default user
        /// when associating with Outline servers.
        /// Defaults to true.
        /// </summary>
        public bool OutlineServerApplyDefaultUserOnAssociation { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to apply the group's per-user data limit
        /// to the Outline server when linking.
        /// Defaults to true.
        /// </summary>
        public bool OutlineServerApplyDataLimitOnAssociation { get; set; } = true;

        /// <summary>
        /// Gets or sets the global setting
        /// for Outline server's default access key's user.
        /// Defaults to an empty string.
        /// </summary>
        public string OutlineServerGlobalDefaultUser { get; set; } = "";

        /// <summary>
        /// Gets or sets the maximum number of concurrent API requests.
        /// Defaults to 32.
        /// </summary>
        public int ApiRequestConcurrency { get; set; } = 32;

        /// <summary>
        /// Gets or sets the base URL of the API server.
        /// MUST NOT contain a trailing slash.
        /// Defaults to an empty string.
        /// </summary>
        public string ApiServerBaseUrl { get; set; } = "";

        /// <summary>
        /// Gets or sets the secret path to the API endpoint.
        /// This is required to conceal the presence of the API.
        /// The secret MAY contain zero or more forward slashes (/) to allow flexible path hierarchy.
        /// But it's recommended to put non-secret part of the path in the base URL.
        /// Defaults to an empty string.
        /// </summary>
        public string ApiServerSecretPath { get; set; } = "";

        /// <summary>
        /// Gets or sets the interval in seconds between each scheduled run of the service.
        /// Defaults to 900 seconds (15 minutes).
        /// </summary>
        public int ServiceRunIntervalSecs { get; set; } = 900;

        /// <summary>
        /// Gets or sets whether the service should pull from servers for updates of
        /// server information, user credentials, and data usage.
        /// Defaults to <c>true</c>.
        /// </summary>
        public bool ServicePullFromServers { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the service should deploy local configurations to servers.
        /// Defaults to <c>false</c>.
        /// </summary>
        public bool ServiceDeployToServers { get; set; }

        /// <summary>
        /// Gets or sets whether the service should generate online config static files.
        /// Defaults to <c>false</c>.
        /// </summary>
        public bool ServiceGenerateOnlineConfig { get; set; }

        /// <summary>
        /// Gets or sets whether the service should clean and regenerate online config static files.
        /// Defaults to <c>false</c>.
        /// </summary>
        public bool ServiceRegenerateOnlineConfig { get; set; }

        /// <summary>
        /// Loads settings from Settings.json.
        /// </summary>
        /// <param name="cancellationToken">A token that may be used to cancel the read operation.</param>
        /// <returns>
        /// A ValueTuple containing a <see cref="Settings"/> object and an optional error message.
        /// </returns>
        public static async Task<(Settings settings, string? errMsg)> LoadSettingsAsync(CancellationToken cancellationToken = default)
        {
            var (settings, errMsg) = await FileHelper.LoadJsonAsync("Settings.json", SettingsJsonSerializerContext.Default.Settings, cancellationToken);
            if (errMsg is null && settings.Version != DefaultVersion)
            {
                settings.UpdateSettings();
                errMsg = await SaveSettingsAsync(settings, cancellationToken);
            }
            return (settings, errMsg);
        }

        /// <summary>
        /// Saves settings to Settings.json.
        /// </summary>
        /// <param name="settings">The <see cref="Settings"/> object to save.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the write operation.</param>
        /// <returns>
        /// An optional error message.
        /// Null if no errors occurred.
        /// </returns>
        public static Task<string?> SaveSettingsAsync(Settings settings, CancellationToken cancellationToken = default)
            => FileHelper.SaveJsonAsync("Settings.json", settings, SettingsJsonSerializerContext.Default.Settings, false, false, cancellationToken);

        /// <summary>
        /// Updates the current object to the latest version.
        /// </summary>
        public void UpdateSettings()
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
