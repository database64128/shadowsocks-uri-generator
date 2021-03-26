using System;
using System.Threading.Tasks;

namespace ShadowsocksUriGenerator.CLI
{
    public static class SettingsCommand
    {
        public static async Task Get()
        {
            var settings = await Settings.LoadSettingsAsync();

            Utilities.PrintTableBorder(42, 40);
            Console.WriteLine($"|{"Key",-42}|{"Value",40}|");
            Utilities.PrintTableBorder(42, 40);

            Console.WriteLine($"|{"Version",-42}|{settings.Version,40}|");
            Console.WriteLine($"|{"UserDataUsageDefaultSortBy",-42}|{settings.UserDataUsageDefaultSortBy,40}|");
            Console.WriteLine($"|{"GroupDataUsageDefaultSortBy",-42}|{settings.GroupDataUsageDefaultSortBy,40}|");
            Console.WriteLine($"|{"OnlineConfigSortByName",-42}|{settings.OnlineConfigSortByName,40}|");
            Console.WriteLine($"|{"OnlineConfigDeliverByGroup",-42}|{settings.OnlineConfigDeliverByGroup,40}|");
            Console.WriteLine($"|{"OnlineConfigCleanOnUserRemoval",-42}|{settings.OnlineConfigCleanOnUserRemoval,40}|");
            Console.WriteLine($"|{"OnlineConfigUpdateDataUsageOnGeneration",-42}|{settings.OnlineConfigUpdateDataUsageOnGeneration,40}|");
            Console.WriteLine($"|{"OnlineConfigOutputDirectory",-42}|{settings.OnlineConfigOutputDirectory,40}|");
            Console.WriteLine($"|{"OnlineConfigDeliveryRootUri",-42}|{settings.OnlineConfigDeliveryRootUri,40}|");
            Console.WriteLine($"|{"OutlineServerDeployOnChange",-42}|{settings.OutlineServerDeployOnChange,40}|");
            Console.WriteLine($"|{"OutlineServerApplyDefaultUserOnAssociation",-42}|{settings.OutlineServerApplyDefaultUserOnAssociation,40}|");
            Console.WriteLine($"|{"OutlineServerGlobalDefaultUser",-42}|{settings.OutlineServerGlobalDefaultUser,40}|");

            Utilities.PrintTableBorder(42, 40);
        }

        public static async Task Set(
            SortBy? userDataUsageDefaultSortBy,
            SortBy? groupDataUsageDefaultSortBy,
            bool? onlineConfigSortByName,
            bool? onlineConfigDeliverByGroup,
            bool? onlineConfigCleanOnUserRemoval,
            bool? onlineConfigUpdateDataUsageOnGeneration,
            string? onlineConfigOutputDirectory,
            string? onlineConfigDeliveryRootUri,
            bool? outlineServerDeployOnChange,
            bool? outlineServerApplyDefaultUserOnAssociation,
            string? outlineServerGlobalDefaultUser)
        {
            var settings = await Settings.LoadSettingsAsync();

            if (userDataUsageDefaultSortBy is SortBy userSortBy)
                settings.UserDataUsageDefaultSortBy = userSortBy;
            if (groupDataUsageDefaultSortBy is SortBy groupSortBy)
                settings.GroupDataUsageDefaultSortBy = groupSortBy;
            if (onlineConfigSortByName is bool sortByName)
                settings.OnlineConfigSortByName = sortByName;
            if (onlineConfigDeliverByGroup is bool deliverByGroup)
                settings.OnlineConfigDeliverByGroup = deliverByGroup;
            if (onlineConfigCleanOnUserRemoval is bool cleanOnUserRemoval)
                settings.OnlineConfigCleanOnUserRemoval = cleanOnUserRemoval;
            if (onlineConfigUpdateDataUsageOnGeneration is bool updateDataUsageOnGeneration)
                settings.OnlineConfigUpdateDataUsageOnGeneration = updateDataUsageOnGeneration;
            if (!string.IsNullOrEmpty(onlineConfigOutputDirectory))
                settings.OnlineConfigOutputDirectory = onlineConfigOutputDirectory;
            if (!string.IsNullOrEmpty(onlineConfigDeliveryRootUri))
                settings.OnlineConfigDeliveryRootUri = onlineConfigDeliveryRootUri;
            if (outlineServerDeployOnChange is bool deployOnChange)
                settings.OutlineServerDeployOnChange = deployOnChange;
            if (outlineServerApplyDefaultUserOnAssociation is bool applyDefaultUserOnAssociation)
                settings.OutlineServerApplyDefaultUserOnAssociation = applyDefaultUserOnAssociation;
            if (outlineServerGlobalDefaultUser != null)
                settings.OutlineServerGlobalDefaultUser = outlineServerGlobalDefaultUser;

            await Settings.SaveSettingsAsync(settings);
        }
    }
}
