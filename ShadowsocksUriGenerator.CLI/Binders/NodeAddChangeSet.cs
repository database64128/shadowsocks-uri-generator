namespace ShadowsocksUriGenerator.CLI.Binders;

public record NodeAddChangeSet(
    string Group,
    string Nodename,
    string Host,
    int Port,
    string? PluginName,
    string? PluginVersion,
    string? PluginOptions,
    string? PluginArguments,
    string? Owner,
    string[] Tags);
