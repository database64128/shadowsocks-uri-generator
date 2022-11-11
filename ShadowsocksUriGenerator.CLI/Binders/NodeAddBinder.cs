using System;
using System.CommandLine;
using System.CommandLine.Binding;

namespace ShadowsocksUriGenerator.CLI.Binders;

public class NodeAddBinder : BinderBase<NodeAddChangeSet>
{
    private readonly Argument<string> _groupArgument;
    private readonly Argument<string> _nodenameArgument;
    private readonly Argument<string> _hostArgument;
    private readonly Argument<int> _portArgument;
    private readonly Option<string?> _pluginNameOption;
    private readonly Option<string?> _pluginVersionOption;
    private readonly Option<string?> _pluginOptionsOption;
    private readonly Option<string?> _pluginArgumentsOption;
    private readonly Option<string?> _ownerOption;
    private readonly Option<string[]> _tagsOption;
    private readonly Option<string[]> _iPSKsOption;

    public NodeAddBinder(
        Argument<string> groupArgument,
        Argument<string> nodenameArgument,
        Argument<string> hostArgument,
        Argument<int> portArgument,
        Option<string?> pluginNameOption,
        Option<string?> pluginVersionOption,
        Option<string?> pluginOptionsOption,
        Option<string?> pluginArgumentsOption,
        Option<string?> ownerOption,
        Option<string[]> tagsOption,
        Option<string[]> iPSKsOption)
    {
        _groupArgument = groupArgument;
        _nodenameArgument = nodenameArgument;
        _hostArgument = hostArgument;
        _portArgument = portArgument;
        _pluginNameOption = pluginNameOption;
        _pluginVersionOption = pluginVersionOption;
        _pluginOptionsOption = pluginOptionsOption;
        _pluginArgumentsOption = pluginArgumentsOption;
        _ownerOption = ownerOption;
        _tagsOption = tagsOption;
        _iPSKsOption = iPSKsOption;
    }

    protected override NodeAddChangeSet GetBoundValue(BindingContext bindingContext) => new(
        bindingContext.ParseResult.GetValue(_groupArgument),
        bindingContext.ParseResult.GetValue(_nodenameArgument),
        bindingContext.ParseResult.GetValue(_hostArgument),
        bindingContext.ParseResult.GetValue(_portArgument),
        bindingContext.ParseResult.GetValue(_pluginNameOption),
        bindingContext.ParseResult.GetValue(_pluginVersionOption),
        bindingContext.ParseResult.GetValue(_pluginOptionsOption),
        bindingContext.ParseResult.GetValue(_pluginArgumentsOption),
        bindingContext.ParseResult.GetValue(_ownerOption),
        bindingContext.ParseResult.GetValue(_tagsOption) ?? Array.Empty<string>(),
        bindingContext.ParseResult.GetValue(_iPSKsOption) ?? Array.Empty<string>());
}
