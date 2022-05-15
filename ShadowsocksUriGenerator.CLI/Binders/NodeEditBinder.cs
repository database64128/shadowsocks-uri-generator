using System;
using System.CommandLine;
using System.CommandLine.Binding;

namespace ShadowsocksUriGenerator.CLI.Binders;

public class NodeEditBinder : BinderBase<NodeEditChangeSet>
{
    private readonly Argument<string> _groupArgument;
    private readonly Argument<string> _nodenameArgument;
    private readonly Option<string?> _hostOption;
    private readonly Option<int> _portOption;
    private readonly Option<string?> _pluginNameOption;
    private readonly Option<string?> _pluginVersionOption;
    private readonly Option<string?> _pluginOptionsOption;
    private readonly Option<string?> _pluginArgumentsOption;
    private readonly Option<bool> _unsetPluginOption;
    private readonly Option<string?> _ownerOption;
    private readonly Option<bool> _unsetOwnerOption;
    private readonly Option<bool> _clearTagsOption;
    private readonly Option<string[]> _addTagsOption;
    private readonly Option<string[]> _removeTagsOption;

    public NodeEditBinder(
        Argument<string> groupArgument,
        Argument<string> nodenameArgument,
        Option<string?> hostOption,
        Option<int> portOption,
        Option<string?> pluginNameOption,
        Option<string?> pluginVersionOption,
        Option<string?> pluginOptionsOption,
        Option<string?> pluginArgumentsOption,
        Option<bool> unsetPluginOption,
        Option<string?> ownerOption,
        Option<bool> unsetOwnerOption,
        Option<bool> clearTagsOption,
        Option<string[]> addTagsOption,
        Option<string[]> removeTagsOption)
    {
        _groupArgument = groupArgument;
        _nodenameArgument = nodenameArgument;
        _hostOption = hostOption;
        _portOption = portOption;
        _pluginNameOption = pluginNameOption;
        _pluginVersionOption = pluginVersionOption;
        _pluginOptionsOption = pluginOptionsOption;
        _pluginArgumentsOption = pluginArgumentsOption;
        _unsetPluginOption = unsetPluginOption;
        _ownerOption = ownerOption;
        _unsetOwnerOption = unsetOwnerOption;
        _clearTagsOption = clearTagsOption;
        _addTagsOption = addTagsOption;
        _removeTagsOption = removeTagsOption;
    }

    protected override NodeEditChangeSet GetBoundValue(BindingContext bindingContext) => new(
        bindingContext.ParseResult.GetValueForArgument(_groupArgument),
        bindingContext.ParseResult.GetValueForArgument(_nodenameArgument),
        bindingContext.ParseResult.GetValueForOption(_hostOption),
        bindingContext.ParseResult.GetValueForOption(_portOption),
        bindingContext.ParseResult.GetValueForOption(_pluginNameOption),
        bindingContext.ParseResult.GetValueForOption(_pluginVersionOption),
        bindingContext.ParseResult.GetValueForOption(_pluginOptionsOption),
        bindingContext.ParseResult.GetValueForOption(_pluginArgumentsOption),
        bindingContext.ParseResult.GetValueForOption(_unsetPluginOption),
        bindingContext.ParseResult.GetValueForOption(_ownerOption),
        bindingContext.ParseResult.GetValueForOption(_unsetOwnerOption),
        bindingContext.ParseResult.GetValueForOption(_clearTagsOption),
        bindingContext.ParseResult.GetValueForOption(_addTagsOption) ?? Array.Empty<string>(),
        bindingContext.ParseResult.GetValueForOption(_removeTagsOption) ?? Array.Empty<string>());
}
