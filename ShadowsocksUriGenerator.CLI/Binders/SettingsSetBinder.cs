using System.CommandLine;
using System.CommandLine.Binding;

namespace ShadowsocksUriGenerator.CLI.Binders;

public class SettingsSetBinder : BinderBase<SettingsSetChangeSet>
{
    private readonly Option<SortBy?> _settingsUserDataUsageDefaultSortByOption;
    private readonly Option<SortBy?> _settingsGroupDataUsageDefaultSortByOption;
    private readonly Option<bool?> _settingsOnlineConfigSortByNameOption;
    private readonly Option<bool?> _settingsOnlineConfigDeliverByGroupOption;
    private readonly Option<bool?> _settingsOnlineConfigCleanOnUserRemovalOption;
    private readonly Option<string?> _settingsOnlineConfigOutputDirectoryOption;
    private readonly Option<string?> _settingsOnlineConfigDeliveryRootUriOption;
    private readonly Option<bool?> _settingsOutlineServerApplyDefaultUserOnAssociationOption;
    private readonly Option<bool?> _settingsOutlineServerApplyDataLimitOnAssociationOption;
    private readonly Option<string?> _settingsOutlineServerGlobalDefaultUserOption;
    private readonly Option<string?> _settingsApiServerBaseUrlOption;
    private readonly Option<string?> _settingsApiServerSecretPathOption;

    public SettingsSetBinder(
        Option<SortBy?> settingsUserDataUsageDefaultSortByOption,
        Option<SortBy?> settingsGroupDataUsageDefaultSortByOption,
        Option<bool?> settingsOnlineConfigSortByNameOption,
        Option<bool?> settingsOnlineConfigDeliverByGroupOption,
        Option<bool?> settingsOnlineConfigCleanOnUserRemovalOption,
        Option<string?> settingsOnlineConfigOutputDirectoryOption,
        Option<string?> settingsOnlineConfigDeliveryRootUriOption,
        Option<bool?> settingsOutlineServerApplyDefaultUserOnAssociationOption,
        Option<bool?> settingsOutlineServerApplyDataLimitOnAssociationOption,
        Option<string?> settingsOutlineServerGlobalDefaultUserOption,
        Option<string?> settingsApiServerBaseUrlOption,
        Option<string?> settingsApiServerSecretPathOption)
    {
        _settingsUserDataUsageDefaultSortByOption = settingsUserDataUsageDefaultSortByOption;
        _settingsGroupDataUsageDefaultSortByOption = settingsGroupDataUsageDefaultSortByOption;
        _settingsOnlineConfigSortByNameOption = settingsOnlineConfigSortByNameOption;
        _settingsOnlineConfigDeliverByGroupOption = settingsOnlineConfigDeliverByGroupOption;
        _settingsOnlineConfigCleanOnUserRemovalOption = settingsOnlineConfigCleanOnUserRemovalOption;
        _settingsOnlineConfigOutputDirectoryOption = settingsOnlineConfigOutputDirectoryOption;
        _settingsOnlineConfigDeliveryRootUriOption = settingsOnlineConfigDeliveryRootUriOption;
        _settingsOutlineServerApplyDefaultUserOnAssociationOption = settingsOutlineServerApplyDefaultUserOnAssociationOption;
        _settingsOutlineServerApplyDataLimitOnAssociationOption = settingsOutlineServerApplyDataLimitOnAssociationOption;
        _settingsOutlineServerGlobalDefaultUserOption = settingsOutlineServerGlobalDefaultUserOption;
        _settingsApiServerBaseUrlOption = settingsApiServerBaseUrlOption;
        _settingsApiServerSecretPathOption = settingsApiServerSecretPathOption;
    }

    protected override SettingsSetChangeSet GetBoundValue(BindingContext bindingContext) => new(
        bindingContext.ParseResult.GetValueForOption(_settingsUserDataUsageDefaultSortByOption),
        bindingContext.ParseResult.GetValueForOption(_settingsGroupDataUsageDefaultSortByOption),
        bindingContext.ParseResult.GetValueForOption(_settingsOnlineConfigSortByNameOption),
        bindingContext.ParseResult.GetValueForOption(_settingsOnlineConfigDeliverByGroupOption),
        bindingContext.ParseResult.GetValueForOption(_settingsOnlineConfigCleanOnUserRemovalOption),
        bindingContext.ParseResult.GetValueForOption(_settingsOnlineConfigOutputDirectoryOption),
        bindingContext.ParseResult.GetValueForOption(_settingsOnlineConfigDeliveryRootUriOption),
        bindingContext.ParseResult.GetValueForOption(_settingsOutlineServerApplyDefaultUserOnAssociationOption),
        bindingContext.ParseResult.GetValueForOption(_settingsOutlineServerApplyDataLimitOnAssociationOption),
        bindingContext.ParseResult.GetValueForOption(_settingsOutlineServerGlobalDefaultUserOption),
        bindingContext.ParseResult.GetValueForOption(_settingsApiServerBaseUrlOption),
        bindingContext.ParseResult.GetValueForOption(_settingsApiServerSecretPathOption));
}
