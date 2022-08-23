namespace ShadowsocksUriGenerator.CLI.Binders;

public record NodeEditChangeSet(
    string Group,
    string Nodename,
    string? Host,
    int Port,
    string? PluginName,
    string? PluginVersion,
    string? PluginOptions,
    string? PluginArguments,
    bool UnsetPlugin,
    string? Owner,
    bool UnsetOwner,
    bool ClearTags,
    string[] AddTags,
    string[] RemoveTags,
    string[] IdentityPSKs,
    bool ClearIPSKs);
