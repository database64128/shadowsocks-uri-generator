namespace ShadowsocksUriGenerator.CLI.Binders;

public record SettingsSetChangeSet(
    SortBy? UserDataUsageDefaultSortBy,
    SortBy? GroupDataUsageDefaultSortBy,
    bool? OnlineConfigSortByName,
    bool? OnlineConfigDeliverByGroup,
    bool? OnlineConfigCleanOnUserRemoval,
    string? OnlineConfigOutputDirectory,
    string? OnlineConfigDeliveryRootUri,
    bool? OutlineServerApplyDefaultUserOnAssociation,
    bool? OutlineServerApplyDataLimitOnAssociation,
    string? OutlineServerGlobalDefaultUser,
    string? ApiServerBaseUrl,
    string? ApiServerSecretPath);
