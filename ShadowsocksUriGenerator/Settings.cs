using System;
using System.Threading;
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
        /// Gets or sets whether the generated servers list
        /// in an SIP008 JSON should be sorted by server name.
        /// Defaults to true.
        /// </summary>
        public bool OnlineConfigSortByName { get; set; } = true;

        /// <summary>
        /// Gets or sets whether online config should be delivered
        /// to each user by group. Turning this on will generate
        /// one online config JSON for each group associated with the user,
        /// in addition to the single JSON that contains all associated servers.
        /// Defaults to false.
        /// </summary>
        public bool OnlineConfigDeliverByGroup { get; set; }

        /// <summary>
        /// Gets or sets whether the user's online config file
        /// should be removed when the user is being removed.
        /// Defaults to true.
        /// </summary>
        public bool OnlineConfigCleanOnUserRemoval { get; set; } = true;

        /// <summary>
        /// Gets or sets the output directory path
        /// for online configuration delivery (SIP008).
        /// Must NOT end with '/' or '\'.
        /// Defaults to a randomly generated UUID.
        /// </summary>
        public string OnlineConfigOutputDirectory { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the URI of the folder which contains
        /// user configuration files for online configuration delivery.
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
        /// Loads settings from Settings.json.
        /// </summary>
        /// <param name="cancellationToken">A token that may be used to cancel the read operation.</param>
        /// <returns>
        /// A ValueTuple containing a <see cref="Settings"/> object and an error message.
        /// </returns>
        public static async Task<(Settings, string? errMsg)> LoadSettingsAsync(CancellationToken cancellationToken = default)
        {
            var (settings, errMsg) = await Utilities.LoadJsonAsync<Settings>("Settings.json", Utilities.commonJsonDeserializerOptions, cancellationToken);
            if (errMsg is null && settings.Version != DefaultVersion)
            {
                UpdateSettings(ref settings);
                errMsg = await SaveSettingsAsync(settings, cancellationToken);
            }
            return (settings, errMsg);
        }

        /// <summary>
        /// Saves settings to Settings.json.
        /// </summary>
        /// <param name="settings">The <see cref="Settings"/> object to save.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the write operation.</param>
        /// <returns>An error message. Null if no errors occurred.</returns>
        public static Task<string?> SaveSettingsAsync(Settings settings, CancellationToken cancellationToken = default)
            => Utilities.SaveJsonAsync("Settings.json", settings, Utilities.commonJsonSerializerOptions, false, false, cancellationToken);

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
