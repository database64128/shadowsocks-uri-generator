using ShadowsocksUriGenerator.CLI.Binders;
using ShadowsocksUriGenerator.CLI.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ShadowsocksUriGenerator.CLI
{
    public static class SettingsCommand
    {
        public static async Task<int> Get(CancellationToken cancellationToken = default)
        {
            var (settings, loadSettingsErrMsg) = await Settings.LoadSettingsAsync(cancellationToken);
            if (loadSettingsErrMsg is not null)
            {
                Console.WriteLine(loadSettingsErrMsg);
                return 1;
            }

            ConsoleHelper.PrintTableBorder(42, 40);
            Console.WriteLine($"|{"Key",-42}|{"Value",40}|");
            ConsoleHelper.PrintTableBorder(42, 40);

            Console.WriteLine($"|{"Version",-42}|{settings.Version,40}|");
            Console.WriteLine($"|{"UserDataUsageDefaultSortBy",-42}|{settings.UserDataUsageDefaultSortBy,40}|");
            Console.WriteLine($"|{"GroupDataUsageDefaultSortBy",-42}|{settings.GroupDataUsageDefaultSortBy,40}|");
            Console.WriteLine($"|{"OnlineConfigSortByName",-42}|{settings.OnlineConfigSortByName,40}|");
            Console.WriteLine($"|{"OnlineConfigDeliverByGroup",-42}|{settings.OnlineConfigDeliverByGroup,40}|");
            Console.WriteLine($"|{"OnlineConfigCleanOnUserRemoval",-42}|{settings.OnlineConfigCleanOnUserRemoval,40}|");
            Console.WriteLine($"|{"OnlineConfigOutputDirectory",-42}|{settings.OnlineConfigOutputDirectory,40}|");
            Console.WriteLine($"|{"OnlineConfigDeliveryRootUri",-42}|{settings.OnlineConfigDeliveryRootUri,40}|");
            Console.WriteLine($"|{"OutlineServerApplyDefaultUserOnAssociation",-42}|{settings.OutlineServerApplyDefaultUserOnAssociation,40}|");
            Console.WriteLine($"|{"OutlineServerApplyDataLimitOnAssociationn",-42}|{settings.OutlineServerApplyDataLimitOnAssociation,40}|");
            Console.WriteLine($"|{"OutlineServerGlobalDefaultUser",-42}|{settings.OutlineServerGlobalDefaultUser,40}|");
            Console.WriteLine($"|{"ApiServerBaseUrl",-42}|{settings.ApiServerBaseUrl,40}|");
            Console.WriteLine($"|{"ApiServerSecretPath",-42}|{settings.ApiServerSecretPath,40}|");

            ConsoleHelper.PrintTableBorder(42, 40);

            return 0;
        }

        public static async Task<int> Set(
            SettingsSetChangeSet settingsSetChangeSet,
            CancellationToken cancellationToken = default)
        {
            var (settings, loadSettingsErrMsg) = await Settings.LoadSettingsAsync(cancellationToken);
            if (loadSettingsErrMsg is not null)
            {
                Console.WriteLine(loadSettingsErrMsg);
                return 1;
            }

            if (settingsSetChangeSet.UserDataUsageDefaultSortBy is SortBy userSortBy)
                settings.UserDataUsageDefaultSortBy = userSortBy;
            if (settingsSetChangeSet.GroupDataUsageDefaultSortBy is SortBy groupSortBy)
                settings.GroupDataUsageDefaultSortBy = groupSortBy;

            if (settingsSetChangeSet.OnlineConfigSortByName is bool sortByName)
                settings.OnlineConfigSortByName = sortByName;
            if (settingsSetChangeSet.OnlineConfigDeliverByGroup is bool deliverByGroup)
                settings.OnlineConfigDeliverByGroup = deliverByGroup;
            if (settingsSetChangeSet.OnlineConfigCleanOnUserRemoval is bool cleanOnUserRemoval)
                settings.OnlineConfigCleanOnUserRemoval = cleanOnUserRemoval;
            if (!string.IsNullOrEmpty(settingsSetChangeSet.OnlineConfigOutputDirectory))
                settings.OnlineConfigOutputDirectory = settingsSetChangeSet.OnlineConfigOutputDirectory;
            if (!string.IsNullOrEmpty(settingsSetChangeSet.OnlineConfigDeliveryRootUri))
                settings.OnlineConfigDeliveryRootUri = settingsSetChangeSet.OnlineConfigDeliveryRootUri;

            if (settingsSetChangeSet.OutlineServerApplyDefaultUserOnAssociation is bool applyDefaultUserOnAssociation)
                settings.OutlineServerApplyDefaultUserOnAssociation = applyDefaultUserOnAssociation;
            if (settingsSetChangeSet.OutlineServerApplyDataLimitOnAssociation is bool applyDataLimitOnAssociation)
                settings.OutlineServerApplyDataLimitOnAssociation = applyDataLimitOnAssociation;
            if (!string.IsNullOrEmpty(settingsSetChangeSet.OutlineServerGlobalDefaultUser))
                settings.OutlineServerGlobalDefaultUser = settingsSetChangeSet.OutlineServerGlobalDefaultUser;

            if (!string.IsNullOrEmpty(settingsSetChangeSet.ApiServerBaseUrl))
                settings.ApiServerBaseUrl = settingsSetChangeSet.ApiServerBaseUrl;
            if (!string.IsNullOrEmpty(settingsSetChangeSet.ApiServerSecretPath))
                settings.ApiServerSecretPath = settingsSetChangeSet.ApiServerSecretPath;

            var saveSettingsErrMsg = await Settings.SaveSettingsAsync(settings, cancellationToken);
            if (saveSettingsErrMsg is not null)
            {
                Console.WriteLine(saveSettingsErrMsg);
                return 1;
            }

            return 0;
        }
    }
}
