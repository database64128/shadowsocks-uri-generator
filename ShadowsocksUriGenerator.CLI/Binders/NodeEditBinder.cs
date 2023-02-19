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
    private readonly Option<string[]> _iPSKsOption;
    private readonly Option<bool> _clearIPSKsOption;

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
        Option<string[]> removeTagsOption,
        Option<string[]> iPSKsOption,
        Option<bool> clearIPSKsOption)
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
        _iPSKsOption = iPSKsOption;
        _clearIPSKsOption = clearIPSKsOption;
    }

    protected override NodeEditChangeSet GetBoundValue(BindingContext bindingContext) => new(
        bindingContext.ParseResult.GetValue(_groupArgument)!,
        bindingContext.ParseResult.GetValue(_nodenameArgument)!,
        bindingContext.ParseResult.GetValue(_hostOption),
        bindingContext.ParseResult.GetValue(_portOption),
        bindingContext.ParseResult.GetValue(_pluginNameOption),
        bindingContext.ParseResult.GetValue(_pluginVersionOption),
        bindingContext.ParseResult.GetValue(_pluginOptionsOption),
        bindingContext.ParseResult.GetValue(_pluginArgumentsOption),
        bindingContext.ParseResult.GetValue(_unsetPluginOption),
        bindingContext.ParseResult.GetValue(_ownerOption),
        bindingContext.ParseResult.GetValue(_unsetOwnerOption),
        bindingContext.ParseResult.GetValue(_clearTagsOption),
        bindingContext.ParseResult.GetValue(_addTagsOption) ?? Array.Empty<string>(),
        bindingContext.ParseResult.GetValue(_removeTagsOption) ?? Array.Empty<string>(),
        bindingContext.ParseResult.GetValue(_iPSKsOption) ?? Array.Empty<string>(),
        bindingContext.ParseResult.GetValue(_clearIPSKsOption));
}
