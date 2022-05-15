using ShadowsocksUriGenerator.CLI.Binders;
using ShadowsocksUriGenerator.CLI.Utils;
using System;
using System.CommandLine;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ShadowsocksUriGenerator.CLI
{
    class Program
    {
        static Task<int> Main(string[] args)
        {
            var userAddCommand = new Command("add", "Add users.");
            var userRenameCommand = new Command("rename", "Renames an existing user with a new name.");
            var userRemoveCommand = new Command("remove", "Remove users.");
            var userListCommand = new Command("list", "List all users.");
            var userJoinGroupsCommand = new Command("join", "Join groups.");
            var userLeaveGroupsCommand = new Command("leave", "Leave groups.");
            var userAddCredentialCommand = new Command("add-credential", "Add a credential for the user to access nodes in the group.");
            var userRemoveCredentialsCommand = new Command("remove-credentials", "Remove the group's credential from the user.");
            var userListCredentialsCommand = new Command("list-credentials", "List user-group credentials.");
            var userGetSSLinksCommand = new Command("get-ss-links", "Get the user's Shadowsocks URLs.");
            var userGetDataUsageCommand = new Command("get-data-usage", "Get the user's data usage records.");
            var userGetDataLimitCommand = new Command("get-data-limit", "Get the user's data limit settings.");
            var userSetDataLimitCommand = new Command("set-data-limit", "Set a global or per-group data limit on the specified users in all or the specified groups.");

            var userOwnGroupsCommand = new Command("own-groups", "Set as owner of groups.");
            var userDisownGroupsCommand = new Command("disown-groups", "Disown groups.");
            var userOwnNodesCommand = new Command("own-nodes", "Set as owner of nodes.");
            var userDisownNodesCommand = new Command("disown-nodes", "Disown nodes.");
            var userListOwnedGroupsCommand = new Command("list-owned-groups", "List owned groups.");
            var userListOwnedNodesCmmand = new Command("list-owned-nodes", "List owned nodes.");

            var userCommand = new Command("user", "Manage users.")
            {
                userAddCommand,
                userRenameCommand,
                userRemoveCommand,
                userListCommand,
                userJoinGroupsCommand,
                userLeaveGroupsCommand,
                userAddCredentialCommand,
                userRemoveCredentialsCommand,
                userListCredentialsCommand,
                userGetSSLinksCommand,
                userGetDataUsageCommand,
                userGetDataLimitCommand,
                userSetDataLimitCommand,
                userOwnGroupsCommand,
                userDisownGroupsCommand,
                userOwnNodesCommand,
                userDisownNodesCommand,
                userListOwnedGroupsCommand,
                userListOwnedNodesCmmand,
            };

            var nodeAddCommand = new Command("add", "Add a node to a group.");
            var nodeEditCommand = new Command("edit", "Edit an existing node in a group.");
            var nodeRenameCommand = new Command("rename", "Rename an existing node with a new name.");
            var nodeRemoveCommand = new Command("remove", "Remove nodes from a group.");
            var nodeListCommand = new Command("list", "List nodes from the specified group or all groups.");
            var nodeListAnnotationsCommand = new Command("list-annotations", "List annotations (ownership and tags) on nodes from the specified group or all groups.");
            var nodeActivateCommand = new Command("activate", "Activate a deactivated node to include it in delivery.");
            var nodeDeactivateCommand = new Command("deactivate", "Deactivate a node to exclude it from delivery.");

            var nodeAddTagsCommand = new Command("add-tags", "Add new tags to the node.");
            var nodeEditTagsCommand = new Command("edit-tags", "Edit tags on the node.");
            var nodeRemoveTagsCommand = new Command("remove-tags", "Remove tags from the node.");
            var nodeClearTagsCommand = new Command("clear-tags", "Clear tags from the node.");

            var nodeSetOwnerCommand = new Command("set-owner", "Set node owner.");
            var nodeUnsetOwnerCommand = new Command("unset-owner", "Unset node owner.");

            var nodeCommand = new Command("node", "Manage nodes.")
            {
                nodeAddCommand,
                nodeEditCommand,
                nodeRenameCommand,
                nodeRemoveCommand,
                nodeListCommand,
                nodeListAnnotationsCommand,
                nodeActivateCommand,
                nodeDeactivateCommand,
                nodeAddTagsCommand,
                nodeEditTagsCommand,
                nodeRemoveTagsCommand,
                nodeClearTagsCommand,
                nodeSetOwnerCommand,
                nodeUnsetOwnerCommand,
            };

            var groupAddCommand = new Command("add", "Add groups.");
            var groupEditCommand = new Command("edit", "Edit groups.");
            var groupRenameCommand = new Command("rename", "Renames an existing group with a new name.");
            var groupRemoveCommand = new Command("remove", "Remove groups and its nodes.");
            var groupListCommand = new Command("list", "List all groups.");
            var groupAddUsersCommand = new Command("add-users", "Add users to the group.");
            var groupRemoveUsersCommand = new Command("remove-users", "Remove users from the group.");
            var groupListUsersCommand = new Command("list-users", "List group members and credentials.");
            var groupAddCredentialCommand = new Command("add-credential", "Add credential to selected users in the group.");
            var groupRemoveCredentialsCommand = new Command("remove-credentials", "Remove credentials from selected users in the group.");
            var groupGetDataUsageCommand = new Command("get-data-usage", "Get the group's data usage records.");
            var groupGetDataLimitCommand = new Command("get-data-limit", "Get the group's data limit settings.");
            var groupSetDataLimitCommand = new Command("set-data-limit", "Set a global or per-user data limit in the specified groups on all or the specified users.");

            var groupCommand = new Command("group", "Manage groups.")
            {
                groupAddCommand,
                groupEditCommand,
                groupRenameCommand,
                groupRemoveCommand,
                groupListCommand,
                groupAddUsersCommand,
                groupRemoveUsersCommand,
                groupListUsersCommand,
                groupAddCredentialCommand,
                groupRemoveCredentialsCommand,
                groupGetDataUsageCommand,
                groupGetDataLimitCommand,
                groupSetDataLimitCommand,
            };

            var onlineConfigGenerateCommand = new Command("generate", "[Legacy] Generate static SIP008 delivery JSON files for specified or all users.");
            var onlineConfigGetLinksCommand = new Command("get-links", "Get online config API URLs and tokens for specified or all users.");
            var onlineConfigCleanCommand = new Command("clean", "[Legacy] Clean static SIP008 delivery files for specified or all users.");

            var onlineConfigCommand = new Command("online-config", "Manage online config.")
            {
                onlineConfigGenerateCommand,
                onlineConfigGetLinksCommand,
                onlineConfigCleanCommand,
            };

            var outlineServerAddCommand = new Command("add", "Associate an Outline server with a group.");
            var outlineServerGetCommand = new Command("get", "Get the associated Outline server's information.");
            var outlineServerSetCommand = new Command("set", "Change settings of the associated Outline server.");
            var outlineServerRemoveCommand = new Command("remove", "Remove the Outline server from the group.");
            var outlineServerPullCommand = new Command("pull", "Pull server information, access keys, and metrics from Outline servers of specified or all groups.");
            var outlineServerDeployCommand = new Command("deploy", "Deploy local configuration to Outline servers of specified or all groups.");
            var outlineServerRotatePasswordCommand = new Command("rotate-password", "Rotate passwords for the specified users and/or groups.");

            var outlineServerCommand = new Command("outline-server", "Manage Outline servers.")
            {
                outlineServerAddCommand,
                outlineServerGetCommand,
                outlineServerSetCommand,
                outlineServerRemoveCommand,
                outlineServerPullCommand,
                outlineServerDeployCommand,
                outlineServerRotatePasswordCommand,
            };

            var reportCommand = new Command("report", "Generate data usage report.");

            var settingsGetCommand = new Command("get", "Get and print all settings.");
            var settingsSetCommand = new Command("set", "Change settings.");

            var settingsCommand = new Command("settings", "Manage settings.")
            {
                settingsGetCommand,
                settingsSetCommand,
            };

            var interactiveCommand = new Command("interactive", "Enter interactive mode (REPL). Exit with 'exit' or 'quit'.");

            var serviceCommand = new Command("service", "Run as a service to execute scheduled tasks.");

            var rootCommand = new RootCommand("A light-weight command line automation tool for managing federated Shadowsocks servers. Automate deployments of Outline servers. Deliver configurations to users with Open Online Config (OOC).")
            {
                userCommand,
                nodeCommand,
                groupCommand,
                onlineConfigCommand,
                outlineServerCommand,
                reportCommand,
                settingsCommand,
                interactiveCommand,
                serviceCommand,
            };

            var usernameArgument = new Argument<string>("username", "Target user.");
            var nodenameArgument = new Argument<string>("nodename", "Name of the node.");
            var groupArgument = new Argument<string>("group", "Target group.");

            var oldNameArgument = new Argument<string>("oldName", "Current name.");
            var newNameArgument = new Argument<string>("newName", "New name.");

            var hostArgument = new Argument<string>("host", "Hostname of the node.");
            var portArgument = new Argument<int>("port", Parsers.ParsePortNumber, false, "Port number of the node.");

            var ownerArgument = new Argument<string>("owner", "Set the owner.");
            var tagsArgument = new Argument<string[]>("tags", "Tags that annotate the node. Will be deduplicated in a case-insensitive manner.")
            {
                Arity = ArgumentArity.OneOrMore,
            };

            var usernamesArgumentZeroOrMore = new Argument<string[]>("usernames", "Zero or more usernames.");
            var nodenamesArgumentZeroOrMore = new Argument<string[]>("nodenames", "Zero or more node names.");
            var groupsArgumentZeroOrMore = new Argument<string[]>("groups", "Zero or more group names.");

            var usernamesArgumentOneOrMore = new Argument<string[]>("usernames", "One or more usernames.")
            {
                Arity = ArgumentArity.OneOrMore,
            };
            var nodenamesArgumentOneOrMore = new Argument<string[]>("nodenames", "One or more node names.")
            {
                Arity = ArgumentArity.OneOrMore,
            };
            var groupsArgumentOneOrMore = new Argument<string[]>("groups", "One or more group names.")
            {
                Arity = ArgumentArity.OneOrMore,
            };

            var outlineApiKeyArgument = new Argument<string>("apiKey", "The Outline server API key.");

            var usernamesOption = new Option<string[]>("--usernames", "Target these specific users. If unspecified, target all users.")
            {
                AllowMultipleArgumentsPerToken = true,
            };
            var nodenamesOption = new Option<string[]>("--nodenames", "Target these specific nodes. If unspecified, target all nodes.")
            {
                AllowMultipleArgumentsPerToken = true,
            };
            var groupsOption = new Option<string[]>("--groups", "Target these specific groups. If unspecified, target all groups.")
            {
                AllowMultipleArgumentsPerToken = true,
            };

            var hostOption = new Option<string?>("--host", "Hostname of the node.");
            var portOption = new Option<int>("--port", Parsers.ParsePortNumber, false, "Port number of the node.");
            var pluginNameOption = new Option<string?>("--plugin-name", "Plugin name.");
            var pluginVersionOption = new Option<string?>("--plugin-version", "Required plugin version.");
            var pluginOptionsOption = new Option<string?>("--plugin-options", "Plugin options, passed as environment variable 'SS_PLUGIN_OPTIONS'.");
            var pluginArgumentsOption = new Option<string?>("--plugin-arguments", "Plugin startup arguments.");
            var unsetPluginOption = new Option<bool>("--unset-plugin", "Remove plugin and plugin options from the node.");

            var ownerOption = new Option<string?>("--owner", "Set the owner.");
            var unsetOwnerOption = new Option<bool>("--unset-owner", "Unset the owner.");

            var forceOption = new Option<bool>(new string[] { "-f", "--force" }, "Forcibly overwrite existing settings.");

            var tagsOption = new Option<string[]>("--tags", "Tags that annotate the node. Will be deduplicated in a case-insensitive manner.")
            {
                AllowMultipleArgumentsPerToken = true,
            };
            var addTagsOption = new Option<string[]>("--add-tags", "Tags to add to the node. Will be deduplicated in a case-insensitive manner.")
            {
                AllowMultipleArgumentsPerToken = true,
            };
            var removeTagsOption = new Option<string[]>("--remove-tags", "Tags to remove from the node. Matched in a case-insensitive manner.")
            {
                AllowMultipleArgumentsPerToken = true,
            };
            var clearTagsOption = new Option<bool>("--clear-tags", "Remove all tags from the node.");

            var methodOption = new Option<string?>("--method", Parsers.ParseShadowsocksAEADMethod, false, "The encryption method. Use with --password.");
            var passwordOption = new Option<string?>("--password", "The password. Use with --method.");
            var userinfoBase64urlOption = new Option<string?>("--userinfo-base64url", "The userinfo (method + ':' + password) encoded in URL-safe base64. Do not specify with '--method' or '--password'.");

            var globalDataLimitOption = new Option<ulong?>("--global", Parsers.ParseDataString, false, "The global data limit in bytes. 0 is interpreted as unlimited. Examples: '1024', '2K', '4M', '8G', '16T', '32P'.");
            var perUserDataLimitOption = new Option<ulong?>("--per-user", Parsers.ParseDataString, false, "The per-user data limit in bytes. 0 is interpreted as unlimited. Examples: '1024', '2K', '4M', '8G', '16T', '32P'.");
            var perGroupDataLimitOption = new Option<ulong?>("--per-group", Parsers.ParseDataString, false, "The per-group data limit in bytes. 0 is interpreted as unlimited. Examples: '1024', '2K', '4M', '8G', '16T', '32P'.");

            var namesOnlyOption = new Option<bool>(new string[] { "-s", "--short", "--names-only" }, "Display names only, without a table.");
            var onePerLineOption = new Option<bool>(new string[] { "-1", "--one-per-line" }, "Display one name per line.");

            var allUsersOption = new Option<bool>(new string[] { "-a", "--all", "--all-users" }, "Target all users.");
            var allNodesOption = new Option<bool>(new string[] { "-a", "--all", "--all-nodes" }, "Target all nodes in target group.");
            var allGroupsOption = new Option<bool>(new string[] { "-a", "--all", "--all-groups" }, "Target all groups.");

            var allUsersNoAliasesOption = new Option<bool>("--all-users", "Target all users.");
            var allNodesNoAliasesOption = new Option<bool>("--all-nodes", "Target all nodes in target group.");
            var allGroupsNoAliasesOption = new Option<bool>("--all-groups", "Target all groups.");

            var sortByOption = new Option<SortBy?>("--sort-by", "Sort rule for data usage records.");
            var userSortByOption = new Option<SortBy?>("--user-sort-by", "Sort rule for user data usage records.");
            var groupSortByOption = new Option<SortBy?>("--group-sort-by", "Sort rule for group data usage records.");

            var outlineServerNameOption = new Option<string?>("--name", "Name of the Outline server.");
            var outlineServerHostnameOption = new Option<string?>("--hostname", "Hostname of the Outline server.");
            var outlineServerPortOption = new Option<int?>("--port", "Port number for new access keys on the Outline server.");
            var outlineServerMetricsOption = new Option<bool?>("--metrics", "Enable or disable telemetry on the Outline server.");
            var outlineServerDefaultUserOption = new Option<string?>("--default-user", "The default user for Outline server's default access key (id: 0).");

            var removeCredsOption = new Option<bool>("--remove-creds", "Remove credentials from all associated users.");
            var noSyncOption = new Option<bool>("--no-sync", "Do not update local user membership storage from retrieved access key list.");

            var csvOutdirOption = new Option<string?>("--csv-outdir", "Export as CSV to the specified directory.");

            var settingsUserDataUsageDefaultSortByOption = new Option<SortBy?>("--user-data-usage-default-sort-by", "The default sort rule for user data usage report.");
            var settingsGroupDataUsageDefaultSortByOption = new Option<SortBy?>("--group-data-usage-default-sort-by", "The default sort rule for group data usage report.");
            var settingsOnlineConfigSortByNameOption = new Option<bool?>("--online-config-sort-by-name", "Whether online config should sort servers by name.");
            var settingsOnlineConfigDeliverByGroupOption = new Option<bool?>("--online-config-deliver-by-group", "Whether the legacy SIP008 online config static file generator should generate per-group SIP008 delivery JSON in addition to the single JSON that contains all associated servers of the user.");
            var settingsOnlineConfigCleanOnUserRemovalOption = new Option<bool?>("--online-config-clean-on-user-removal", "Whether the user's generated static online config files should be removed when the user is being removed.");
            var settingsOnlineConfigOutputDirectoryOption = new Option<string?>("--online-config-output-directory", "Legacy SIP008 online config static file generator output directory. No trailing slashes allowed.");
            var settingsOnlineConfigDeliveryRootUriOption = new Option<string?>("--online-config-delivery-root-uri", "URL base for the SIP008 static file delivery links. No trailing slashes allowed.");
            var settingsOutlineServerApplyDefaultUserOnAssociationOption = new Option<bool?>("--outline-server-apply-default-user-on-association", "Whether to apply the global default user when associating with Outline servers.");
            var settingsOutlineServerApplyDataLimitOnAssociationOption = new Option<bool?>("--outline-server-apply-data-limit-on-association", "Whether to apply the group's per-user data limit when associating with Outline servers.");
            var settingsOutlineServerGlobalDefaultUserOption = new Option<string?>("--outline-server-global-default-user", "The global setting for Outline server's default access key's user.");
            var settingsApiServerBaseUrlOption = new Option<string?>("--api-server-base-url", "The base URL of the API server. MUST NOT contain a trailing slash.");
            var settingsApiServerSecretPathOption = new Option<string?>("--api-server-secret-path", "The secret path to the API endpoint. This is required to conceal the presence of the API. The secret MAY contain zero or more forward slashes (/) to allow flexible path hierarchy. But it's recommended to put non-secret part of the path in the base URL.");

            var serviceIntervalOption = new Option<int>("--interval", () => 3600, "The interval between each scheduled run in seconds.");
            var servicePullOutlineServerOption = new Option<bool>("--pull-outline-server", "Pull from Outline servers for updates of server information, access keys, data usage.");
            var serviceDeployOutlineServerOption = new Option<bool>("--deploy-outline-server", "Deploy local configurations to Outline servers.");
            var serviceGenerateOnlineConfigOption = new Option<bool>("--generate-online-config", "Generate online config.");
            var serviceRegenerateOnlineConfigOption = new Option<bool>("--regenerate-online-config", "Clean and regenerate online config.");

            var cancellationTokenBinder = new CancellationTokenBinder();
            var nodeAddBinder = new NodeAddBinder(groupArgument, nodenameArgument, hostArgument, portArgument, pluginNameOption, pluginVersionOption, pluginOptionsOption, pluginArgumentsOption, ownerOption, tagsOption);
            var nodeEditBinder = new NodeEditBinder(groupArgument, nodenameArgument, hostOption, portOption, pluginNameOption, pluginVersionOption, pluginOptionsOption, pluginArgumentsOption, unsetPluginOption, ownerOption, unsetOwnerOption, clearTagsOption, addTagsOption, removeTagsOption);
            var settingsSetBinder = new SettingsSetBinder(settingsUserDataUsageDefaultSortByOption, settingsGroupDataUsageDefaultSortByOption, settingsOnlineConfigSortByNameOption, settingsOnlineConfigDeliverByGroupOption, settingsOnlineConfigCleanOnUserRemovalOption, settingsOnlineConfigOutputDirectoryOption, settingsOnlineConfigDeliveryRootUriOption, settingsOutlineServerApplyDefaultUserOnAssociationOption, settingsOutlineServerApplyDataLimitOnAssociationOption, settingsOutlineServerGlobalDefaultUserOption, settingsApiServerBaseUrlOption, settingsApiServerSecretPathOption);

            userCommand.AddAlias("u");
            nodeCommand.AddAlias("n");
            groupCommand.AddAlias("g");
            onlineConfigCommand.AddAlias("oc");
            onlineConfigCommand.AddAlias("ooc");
            onlineConfigCommand.AddAlias("online");
            outlineServerCommand.AddAlias("os");
            outlineServerCommand.AddAlias("outline");
            reportCommand.AddAlias("r");
            settingsCommand.AddAlias("s");
            interactiveCommand.AddAlias("i");
            interactiveCommand.AddAlias("repl");

            userAddCommand.AddAlias("a");
            userAddCommand.AddArgument(usernamesArgumentOneOrMore);
            userAddCommand.SetHandler(UserCommand.Add, usernamesArgumentOneOrMore, cancellationTokenBinder);

            userRenameCommand.AddArgument(oldNameArgument);
            userRenameCommand.AddArgument(newNameArgument);
            userRenameCommand.SetHandler(UserCommand.Rename, oldNameArgument, newNameArgument, cancellationTokenBinder);

            userRemoveCommand.AddAlias("rm");
            userRemoveCommand.AddAlias("del");
            userRemoveCommand.AddAlias("delete");
            userRemoveCommand.AddArgument(usernamesArgumentOneOrMore);
            userRemoveCommand.SetHandler(UserCommand.Remove, usernamesArgumentOneOrMore, cancellationTokenBinder);

            userListCommand.AddAlias("l");
            userListCommand.AddAlias("ls");
            userListCommand.AddOption(namesOnlyOption);
            userListCommand.AddOption(onePerLineOption);
            userListCommand.SetHandler(UserCommand.List, namesOnlyOption, onePerLineOption, cancellationTokenBinder);

            userJoinGroupsCommand.AddArgument(usernameArgument);
            userJoinGroupsCommand.AddArgument(groupsArgumentZeroOrMore);
            userJoinGroupsCommand.AddOption(allGroupsOption);
            userJoinGroupsCommand.AddValidator(Validators.EnforceZeroGroupsWhenAll);
            userJoinGroupsCommand.SetHandler(UserCommand.JoinGroups, usernameArgument, groupsArgumentZeroOrMore, allGroupsOption, cancellationTokenBinder);

            userLeaveGroupsCommand.AddArgument(usernameArgument);
            userLeaveGroupsCommand.AddArgument(groupsArgumentZeroOrMore);
            userLeaveGroupsCommand.AddOption(allGroupsOption);
            userLeaveGroupsCommand.AddValidator(Validators.EnforceZeroGroupsWhenAll);
            userLeaveGroupsCommand.SetHandler(UserCommand.LeaveGroups, usernameArgument, groupsArgumentZeroOrMore, allGroupsOption, cancellationTokenBinder);

            userAddCredentialCommand.AddAlias("ac");
            userAddCredentialCommand.AddArgument(usernameArgument);
            userAddCredentialCommand.AddArgument(groupsArgumentZeroOrMore);
            userAddCredentialCommand.AddOption(methodOption);
            userAddCredentialCommand.AddOption(passwordOption);
            userAddCredentialCommand.AddOption(userinfoBase64urlOption);
            userAddCredentialCommand.AddOption(allGroupsOption);
            userAddCredentialCommand.AddValidator(Validators.EnforceZeroGroupsWhenAll);
            userAddCredentialCommand.AddValidator(Validators.ValidateAddCredential);
            userAddCredentialCommand.SetHandler<string, string[], string?, string?, string?, bool, CancellationToken>(UserCommand.AddCredential, usernameArgument, groupsArgumentZeroOrMore, methodOption, passwordOption, userinfoBase64urlOption, allGroupsOption, cancellationTokenBinder);

            userRemoveCredentialsCommand.AddAlias("rc");
            userRemoveCredentialsCommand.AddArgument(usernameArgument);
            userRemoveCredentialsCommand.AddArgument(groupsArgumentZeroOrMore);
            userRemoveCredentialsCommand.AddOption(allGroupsOption);
            userRemoveCredentialsCommand.AddValidator(Validators.EnforceZeroGroupsWhenAll);
            userRemoveCredentialsCommand.SetHandler(UserCommand.RemoveCredentials, usernameArgument, groupsArgumentZeroOrMore, allGroupsOption, cancellationTokenBinder);

            userListCredentialsCommand.AddAlias("lc");
            userListCredentialsCommand.AddOption(usernamesOption);
            userListCredentialsCommand.AddOption(groupsOption);
            userListCredentialsCommand.SetHandler(UserCommand.ListCredentials, usernamesOption, groupsOption, cancellationTokenBinder);

            userGetSSLinksCommand.AddAlias("ss");
            userGetSSLinksCommand.AddArgument(usernameArgument);
            userGetSSLinksCommand.AddOption(groupsOption);
            userGetSSLinksCommand.SetHandler(UserCommand.GetSSLinks, usernameArgument, groupsOption, cancellationTokenBinder);

            userGetDataUsageCommand.AddAlias("data");
            userGetDataUsageCommand.AddArgument(usernameArgument);
            userGetDataUsageCommand.AddOption(sortByOption);
            userGetDataUsageCommand.SetHandler(UserCommand.GetDataUsage, usernameArgument, sortByOption, cancellationTokenBinder);

            userGetDataLimitCommand.AddAlias("gl");
            userGetDataLimitCommand.AddAlias("gdl");
            userGetDataLimitCommand.AddAlias("limit");
            userGetDataLimitCommand.AddAlias("get-limit");
            userGetDataLimitCommand.AddArgument(usernameArgument);
            userGetDataLimitCommand.SetHandler(UserCommand.GetDataLimit, usernameArgument, cancellationTokenBinder);

            userSetDataLimitCommand.AddAlias("sl");
            userSetDataLimitCommand.AddAlias("sdl");
            userSetDataLimitCommand.AddAlias("set-limit");
            userSetDataLimitCommand.AddArgument(usernamesArgumentOneOrMore);
            userSetDataLimitCommand.AddOption(globalDataLimitOption);
            userSetDataLimitCommand.AddOption(perGroupDataLimitOption);
            userSetDataLimitCommand.AddOption(groupsOption);
            userSetDataLimitCommand.AddValidator(UserCommand.ValidateSetDataLimit);
            userSetDataLimitCommand.SetHandler(UserCommand.SetDataLimit, usernamesArgumentOneOrMore, globalDataLimitOption, perGroupDataLimitOption, groupsOption, cancellationTokenBinder);

            userOwnGroupsCommand.AddAlias("og");
            userOwnGroupsCommand.AddArgument(usernameArgument);
            userOwnGroupsCommand.AddArgument(groupsArgumentZeroOrMore);
            userOwnGroupsCommand.AddOption(allGroupsOption);
            userOwnGroupsCommand.AddOption(forceOption);
            userOwnGroupsCommand.AddValidator(Validators.EnforceZeroGroupsWhenAll);
            userOwnGroupsCommand.SetHandler(UserCommand.OwnGroups, usernameArgument, groupsArgumentZeroOrMore, allGroupsOption, forceOption, cancellationTokenBinder);

            userDisownGroupsCommand.AddAlias("dg");
            userDisownGroupsCommand.AddArgument(usernameArgument);
            userDisownGroupsCommand.AddArgument(groupsArgumentZeroOrMore);
            userDisownGroupsCommand.AddOption(allGroupsOption);
            userDisownGroupsCommand.AddValidator(Validators.EnforceZeroGroupsWhenAll);
            userDisownGroupsCommand.SetHandler(UserCommand.DisownGroups, usernameArgument, groupsArgumentZeroOrMore, allGroupsOption, cancellationTokenBinder);

            userOwnNodesCommand.AddAlias("on");
            userOwnNodesCommand.AddArgument(usernameArgument);
            userOwnNodesCommand.AddOption(groupsOption);
            userOwnNodesCommand.AddOption(allGroupsNoAliasesOption);
            userOwnNodesCommand.AddOption(nodenamesOption);
            userOwnNodesCommand.AddOption(allNodesNoAliasesOption);
            userOwnNodesCommand.AddOption(forceOption);
            userOwnNodesCommand.AddValidator(Validators.EnforceZeroGroupsWhenAll);
            userOwnNodesCommand.AddValidator(Validators.EnforceZeroNodenamesWhenAll);
            userOwnNodesCommand.SetHandler(UserCommand.OwnNodes, usernameArgument, groupsOption, allGroupsNoAliasesOption, nodenamesOption, allNodesNoAliasesOption, forceOption, cancellationTokenBinder);

            userDisownNodesCommand.AddAlias("dn");
            userDisownNodesCommand.AddArgument(usernameArgument);
            userDisownNodesCommand.AddOption(groupsOption);
            userDisownNodesCommand.AddOption(allGroupsNoAliasesOption);
            userDisownNodesCommand.AddOption(nodenamesOption);
            userDisownNodesCommand.AddOption(allNodesNoAliasesOption);
            userDisownNodesCommand.AddValidator(Validators.EnforceZeroGroupsWhenAll);
            userDisownNodesCommand.AddValidator(Validators.EnforceZeroNodenamesWhenAll);
            userDisownNodesCommand.SetHandler(UserCommand.DisownNodes, usernameArgument, groupsOption, allGroupsNoAliasesOption, nodenamesOption, allNodesNoAliasesOption, cancellationTokenBinder);

            userListOwnedGroupsCommand.AddAlias("log");
            userListOwnedGroupsCommand.AddArgument(usernameArgument);
            userListOwnedGroupsCommand.SetHandler(UserCommand.ListOwnedGroups, usernameArgument, cancellationTokenBinder);

            userListOwnedNodesCmmand.AddAlias("lon");
            userListOwnedNodesCmmand.AddArgument(usernameArgument);
            userListOwnedNodesCmmand.AddArgument(groupsArgumentZeroOrMore);
            userListOwnedNodesCmmand.SetHandler(UserCommand.ListOwnedNodes, usernameArgument, groupsArgumentZeroOrMore, cancellationTokenBinder);

            nodeAddCommand.AddAlias("a");
            nodeAddCommand.AddArgument(groupArgument);
            nodeAddCommand.AddArgument(nodenameArgument);
            nodeAddCommand.AddArgument(hostArgument);
            nodeAddCommand.AddArgument(portArgument);
            nodeAddCommand.AddOption(pluginNameOption);
            nodeAddCommand.AddOption(pluginVersionOption);
            nodeAddCommand.AddOption(pluginOptionsOption);
            nodeAddCommand.AddOption(pluginArgumentsOption);
            nodeAddCommand.AddOption(ownerOption);
            nodeAddCommand.AddOption(tagsOption);
            nodeAddCommand.AddValidator(NodeCommand.ValidateNodePlugin);
            nodeAddCommand.SetHandler(NodeCommand.Add, nodeAddBinder, cancellationTokenBinder);

            nodeEditCommand.AddAlias("e");
            nodeEditCommand.AddArgument(groupArgument);
            nodeEditCommand.AddArgument(nodenameArgument);
            nodeEditCommand.AddOption(hostOption);
            nodeEditCommand.AddOption(portOption);
            nodeEditCommand.AddOption(pluginNameOption);
            nodeEditCommand.AddOption(pluginVersionOption);
            nodeEditCommand.AddOption(pluginOptionsOption);
            nodeEditCommand.AddOption(pluginArgumentsOption);
            nodeEditCommand.AddOption(unsetPluginOption);
            nodeEditCommand.AddOption(ownerOption);
            nodeEditCommand.AddOption(unsetOwnerOption);
            nodeEditCommand.AddOption(clearTagsOption);
            nodeEditCommand.AddOption(addTagsOption);
            nodeEditCommand.AddOption(removeTagsOption);
            nodeEditCommand.AddValidator(NodeCommand.ValidateNodePlugin);
            nodeEditCommand.AddValidator(Validators.ValidateOwnerOptions);
            nodeEditCommand.SetHandler(NodeCommand.Edit, nodeEditBinder, cancellationTokenBinder);

            nodeRenameCommand.AddArgument(groupArgument);
            nodeRenameCommand.AddArgument(oldNameArgument);
            nodeRenameCommand.AddArgument(newNameArgument);
            nodeRenameCommand.SetHandler(NodeCommand.Rename, groupArgument, oldNameArgument, newNameArgument, cancellationTokenBinder);

            nodeRemoveCommand.AddAlias("rm");
            nodeRemoveCommand.AddAlias("del");
            nodeRemoveCommand.AddAlias("delete");
            nodeRemoveCommand.AddArgument(groupArgument);
            nodeRemoveCommand.AddArgument(nodenamesArgumentOneOrMore);
            nodeRemoveCommand.SetHandler(NodeCommand.Remove, groupArgument, nodenamesArgumentOneOrMore, cancellationTokenBinder);

            nodeListCommand.AddAlias("l");
            nodeListCommand.AddAlias("ls");
            nodeListCommand.AddArgument(groupsArgumentZeroOrMore);
            nodeListCommand.AddOption(namesOnlyOption);
            nodeListCommand.AddOption(onePerLineOption);
            nodeListCommand.SetHandler(NodeCommand.List, groupsArgumentZeroOrMore, namesOnlyOption, onePerLineOption, cancellationTokenBinder);

            nodeListAnnotationsCommand.AddAlias("la");
            nodeListAnnotationsCommand.AddAlias("lsa");
            nodeListAnnotationsCommand.AddArgument(groupsArgumentZeroOrMore);
            nodeListAnnotationsCommand.AddOption(onePerLineOption);
            nodeListAnnotationsCommand.SetHandler(NodeCommand.ListAnnotations, groupsArgumentZeroOrMore, onePerLineOption, cancellationTokenBinder);

            nodeActivateCommand.AddAlias("enable");
            nodeActivateCommand.AddAlias("unhide");
            nodeActivateCommand.AddArgument(groupArgument);
            nodeActivateCommand.AddArgument(nodenamesArgumentZeroOrMore);
            nodeActivateCommand.AddOption(allNodesOption);
            nodeActivateCommand.AddValidator(Validators.EnforceZeroNodenamesWhenAll);
            nodeActivateCommand.SetHandler(NodeCommand.Activate, groupArgument, nodenamesArgumentZeroOrMore, allNodesOption, cancellationTokenBinder);

            nodeDeactivateCommand.AddAlias("disable");
            nodeDeactivateCommand.AddAlias("hide");
            nodeDeactivateCommand.AddArgument(groupArgument);
            nodeDeactivateCommand.AddArgument(nodenamesArgumentZeroOrMore);
            nodeDeactivateCommand.AddOption(allNodesOption);
            nodeDeactivateCommand.AddValidator(Validators.EnforceZeroNodenamesWhenAll);
            nodeDeactivateCommand.SetHandler(NodeCommand.Deactivate, groupArgument, nodenamesArgumentZeroOrMore, allNodesOption, cancellationTokenBinder);

            nodeAddTagsCommand.AddAlias("at");
            nodeAddTagsCommand.AddArgument(tagsArgument);
            nodeAddTagsCommand.AddOption(groupsOption);
            nodeAddTagsCommand.AddOption(allGroupsNoAliasesOption);
            nodeAddTagsCommand.AddOption(nodenamesOption);
            nodeAddTagsCommand.AddOption(allNodesNoAliasesOption);
            nodeAddTagsCommand.AddValidator(Validators.EnforceZeroGroupsWhenAll);
            nodeAddTagsCommand.AddValidator(Validators.EnforceZeroNodenamesWhenAll);
            nodeAddTagsCommand.SetHandler(NodeCommand.AddTags, tagsArgument, groupsOption, allGroupsNoAliasesOption, nodenamesOption, allNodesNoAliasesOption, cancellationTokenBinder);

            nodeEditTagsCommand.AddAlias("et");
            nodeEditTagsCommand.AddOption(groupsOption);
            nodeEditTagsCommand.AddOption(allGroupsNoAliasesOption);
            nodeEditTagsCommand.AddOption(nodenamesOption);
            nodeEditTagsCommand.AddOption(allNodesNoAliasesOption);
            nodeEditTagsCommand.AddOption(clearTagsOption);
            nodeEditTagsCommand.AddOption(addTagsOption);
            nodeEditTagsCommand.AddOption(removeTagsOption);
            nodeEditTagsCommand.AddValidator(Validators.EnforceZeroGroupsWhenAll);
            nodeEditTagsCommand.AddValidator(Validators.EnforceZeroNodenamesWhenAll);
            nodeEditTagsCommand.SetHandler(NodeCommand.EditTags, groupsOption, allGroupsNoAliasesOption, nodenamesOption, allNodesNoAliasesOption, clearTagsOption, addTagsOption, removeTagsOption, cancellationTokenBinder);

            nodeRemoveTagsCommand.AddAlias("rt");
            nodeRemoveTagsCommand.AddArgument(tagsArgument);
            nodeRemoveTagsCommand.AddOption(groupsOption);
            nodeRemoveTagsCommand.AddOption(allGroupsNoAliasesOption);
            nodeRemoveTagsCommand.AddOption(nodenamesOption);
            nodeRemoveTagsCommand.AddOption(allNodesNoAliasesOption);
            nodeRemoveTagsCommand.AddValidator(Validators.EnforceZeroGroupsWhenAll);
            nodeRemoveTagsCommand.AddValidator(Validators.EnforceZeroNodenamesWhenAll);
            nodeRemoveTagsCommand.SetHandler(NodeCommand.RemoveTags, tagsArgument, groupsOption, allGroupsNoAliasesOption, nodenamesOption, allNodesNoAliasesOption, cancellationTokenBinder);

            nodeClearTagsCommand.AddAlias("ct");
            nodeClearTagsCommand.AddOption(groupsOption);
            nodeClearTagsCommand.AddOption(allGroupsNoAliasesOption);
            nodeClearTagsCommand.AddOption(nodenamesOption);
            nodeClearTagsCommand.AddOption(allNodesNoAliasesOption);
            nodeClearTagsCommand.AddValidator(Validators.EnforceZeroGroupsWhenAll);
            nodeClearTagsCommand.AddValidator(Validators.EnforceZeroNodenamesWhenAll);
            nodeClearTagsCommand.SetHandler(NodeCommand.ClearTags, groupsOption, allGroupsNoAliasesOption, nodenamesOption, allNodesNoAliasesOption, cancellationTokenBinder);

            nodeSetOwnerCommand.AddAlias("so");
            nodeSetOwnerCommand.AddArgument(ownerArgument);
            nodeSetOwnerCommand.AddOption(groupsOption);
            nodeSetOwnerCommand.AddOption(allGroupsNoAliasesOption);
            nodeSetOwnerCommand.AddOption(nodenamesOption);
            nodeSetOwnerCommand.AddOption(allNodesNoAliasesOption);
            nodeSetOwnerCommand.AddValidator(Validators.EnforceZeroGroupsWhenAll);
            nodeSetOwnerCommand.AddValidator(Validators.EnforceZeroNodenamesWhenAll);
            nodeSetOwnerCommand.SetHandler(NodeCommand.SetOwner, ownerArgument, groupsOption, allGroupsNoAliasesOption, nodenamesOption, allNodesNoAliasesOption, cancellationTokenBinder);

            nodeUnsetOwnerCommand.AddAlias("uo");
            nodeUnsetOwnerCommand.AddOption(groupsOption);
            nodeUnsetOwnerCommand.AddOption(allGroupsNoAliasesOption);
            nodeUnsetOwnerCommand.AddOption(nodenamesOption);
            nodeUnsetOwnerCommand.AddOption(allNodesNoAliasesOption);
            nodeUnsetOwnerCommand.AddValidator(Validators.EnforceZeroGroupsWhenAll);
            nodeUnsetOwnerCommand.AddValidator(Validators.EnforceZeroNodenamesWhenAll);
            nodeUnsetOwnerCommand.SetHandler(NodeCommand.UnsetOwner, groupsOption, allGroupsNoAliasesOption, nodenamesOption, allNodesNoAliasesOption, cancellationTokenBinder);

            groupAddCommand.AddAlias("a");
            groupAddCommand.AddArgument(groupsArgumentOneOrMore);
            groupAddCommand.AddOption(ownerOption);
            groupAddCommand.SetHandler(GroupCommand.Add, groupsArgumentOneOrMore, ownerOption, cancellationTokenBinder);

            groupEditCommand.AddAlias("e");
            groupEditCommand.AddArgument(groupsArgumentOneOrMore);
            groupEditCommand.AddOption(ownerOption);
            groupEditCommand.AddOption(unsetOwnerOption);
            groupEditCommand.AddValidator(Validators.ValidateOwnerOptions);
            groupEditCommand.SetHandler(GroupCommand.Edit, groupsArgumentOneOrMore, ownerOption, unsetOwnerOption, cancellationTokenBinder);

            groupRenameCommand.AddArgument(oldNameArgument);
            groupRenameCommand.AddArgument(newNameArgument);
            groupRenameCommand.SetHandler(GroupCommand.Rename, oldNameArgument, newNameArgument, cancellationTokenBinder);

            groupRemoveCommand.AddAlias("rm");
            groupRemoveCommand.AddAlias("del");
            groupRemoveCommand.AddAlias("delete");
            groupRemoveCommand.AddArgument(groupsArgumentOneOrMore);
            groupRemoveCommand.SetHandler(GroupCommand.Remove, groupsArgumentOneOrMore, cancellationTokenBinder);

            groupListCommand.AddAlias("l");
            groupListCommand.AddAlias("ls");
            groupListCommand.AddOption(namesOnlyOption);
            groupListCommand.AddOption(onePerLineOption);
            groupListCommand.SetHandler(GroupCommand.List, namesOnlyOption, onePerLineOption, cancellationTokenBinder);

            groupAddUsersCommand.AddAlias("au");
            groupAddUsersCommand.AddArgument(groupArgument);
            groupAddUsersCommand.AddArgument(usernamesArgumentZeroOrMore);
            groupAddUsersCommand.AddOption(allUsersOption);
            groupAddUsersCommand.AddValidator(Validators.EnforceZeroUsernamesWhenAll);
            groupAddUsersCommand.SetHandler(GroupCommand.AddUsers, groupArgument, usernamesArgumentZeroOrMore, allUsersOption, cancellationTokenBinder);

            groupRemoveUsersCommand.AddAlias("ru");
            groupRemoveUsersCommand.AddArgument(groupArgument);
            groupRemoveUsersCommand.AddArgument(usernamesArgumentZeroOrMore);
            groupRemoveUsersCommand.AddOption(allUsersOption);
            groupRemoveUsersCommand.AddValidator(Validators.EnforceZeroUsernamesWhenAll);
            groupRemoveUsersCommand.SetHandler(GroupCommand.RemoveUsers, groupArgument, usernamesArgumentZeroOrMore, allUsersOption, cancellationTokenBinder);

            groupListUsersCommand.AddAlias("lc");
            groupListUsersCommand.AddAlias("lm");
            groupListUsersCommand.AddAlias("lu");
            groupListUsersCommand.AddAlias("list-credentials");
            groupListUsersCommand.AddAlias("list-members");
            groupListUsersCommand.AddArgument(groupArgument);
            groupListUsersCommand.SetHandler(GroupCommand.ListUsers, groupArgument, cancellationTokenBinder);

            groupAddCredentialCommand.AddAlias("ac");
            groupAddCredentialCommand.AddArgument(groupArgument);
            groupAddCredentialCommand.AddArgument(usernamesArgumentZeroOrMore);
            groupAddCredentialCommand.AddOption(methodOption);
            groupAddCredentialCommand.AddOption(passwordOption);
            groupAddCredentialCommand.AddOption(userinfoBase64urlOption);
            groupAddCredentialCommand.AddOption(allUsersOption);
            groupAddCredentialCommand.AddValidator(Validators.EnforceZeroUsernamesWhenAll);
            groupAddCredentialCommand.AddValidator(Validators.ValidateAddCredential);
            groupAddCredentialCommand.SetHandler(GroupCommand.AddCredential, groupArgument, usernamesArgumentZeroOrMore, methodOption, passwordOption, userinfoBase64urlOption, allUsersOption, cancellationTokenBinder);

            groupRemoveCredentialsCommand.AddAlias("rc");
            groupRemoveCredentialsCommand.AddArgument(groupArgument);
            groupRemoveCredentialsCommand.AddArgument(usernamesArgumentZeroOrMore);
            groupRemoveCredentialsCommand.AddOption(allUsersOption);
            groupRemoveCredentialsCommand.AddValidator(Validators.EnforceZeroUsernamesWhenAll);
            groupRemoveCredentialsCommand.SetHandler(GroupCommand.RemoveCredentials, groupArgument, usernamesArgumentZeroOrMore, allUsersOption, cancellationTokenBinder);

            groupGetDataUsageCommand.AddAlias("data");
            groupGetDataUsageCommand.AddArgument(groupArgument);
            groupGetDataUsageCommand.AddOption(sortByOption);
            groupGetDataUsageCommand.SetHandler(GroupCommand.GetDataUsage, groupArgument, sortByOption, cancellationTokenBinder);

            groupGetDataLimitCommand.AddAlias("gl");
            groupGetDataLimitCommand.AddAlias("gdl");
            groupGetDataLimitCommand.AddAlias("limit");
            groupGetDataLimitCommand.AddAlias("get-limit");
            groupGetDataLimitCommand.AddArgument(groupArgument);
            groupGetDataLimitCommand.SetHandler(GroupCommand.GetDataLimit, groupArgument, cancellationTokenBinder);

            groupSetDataLimitCommand.AddAlias("sl");
            groupSetDataLimitCommand.AddAlias("sdl");
            groupSetDataLimitCommand.AddAlias("set-limit");
            groupSetDataLimitCommand.AddArgument(groupsArgumentOneOrMore);
            groupSetDataLimitCommand.AddOption(globalDataLimitOption);
            groupSetDataLimitCommand.AddOption(perUserDataLimitOption);
            groupSetDataLimitCommand.AddOption(usernamesOption);
            groupSetDataLimitCommand.AddValidator(GroupCommand.ValidateSetDataLimit);
            groupSetDataLimitCommand.SetHandler(GroupCommand.SetDataLimit, groupsArgumentOneOrMore, globalDataLimitOption, perUserDataLimitOption, usernamesOption, cancellationTokenBinder);

            onlineConfigGenerateCommand.AddAlias("g");
            onlineConfigGenerateCommand.AddAlias("gen");
            onlineConfigGenerateCommand.AddArgument(usernamesArgumentZeroOrMore);
            onlineConfigGenerateCommand.SetHandler(OnlineConfigCommand.Generate, usernamesArgumentZeroOrMore, cancellationTokenBinder);

            onlineConfigGetLinksCommand.AddAlias("l");
            onlineConfigGetLinksCommand.AddAlias("link");
            onlineConfigGetLinksCommand.AddAlias("links");
            onlineConfigGetLinksCommand.AddAlias("token");
            onlineConfigGetLinksCommand.AddAlias("tokens");
            onlineConfigGetLinksCommand.AddAlias("url");
            onlineConfigGetLinksCommand.AddAlias("urls");
            onlineConfigGetLinksCommand.AddArgument(usernamesArgumentZeroOrMore);
            onlineConfigGetLinksCommand.SetHandler(OnlineConfigCommand.GetLinks, usernamesArgumentZeroOrMore, cancellationTokenBinder);

            onlineConfigCleanCommand.AddAlias("c");
            onlineConfigCleanCommand.AddAlias("clear");
            onlineConfigCleanCommand.AddArgument(usernamesArgumentZeroOrMore);
            onlineConfigCleanCommand.AddOption(allUsersOption);
            onlineConfigCleanCommand.AddValidator(Validators.EnforceZeroUsernamesWhenAll);
            onlineConfigCleanCommand.SetHandler(OnlineConfigCommand.Clean, usernamesArgumentZeroOrMore, allUsersOption, cancellationTokenBinder);

            outlineServerAddCommand.AddAlias("a");
            outlineServerAddCommand.AddArgument(groupArgument);
            outlineServerAddCommand.AddArgument(outlineApiKeyArgument);
            outlineServerAddCommand.SetHandler(OutlineServerCommand.Add, groupArgument, outlineApiKeyArgument, cancellationTokenBinder);

            outlineServerGetCommand.AddArgument(groupArgument);
            outlineServerGetCommand.SetHandler(OutlineServerCommand.Get, groupArgument, cancellationTokenBinder);

            outlineServerSetCommand.AddArgument(groupArgument);
            outlineServerSetCommand.AddOption(outlineServerNameOption);
            outlineServerSetCommand.AddOption(outlineServerHostnameOption);
            outlineServerSetCommand.AddOption(outlineServerPortOption);
            outlineServerSetCommand.AddOption(outlineServerMetricsOption);
            outlineServerSetCommand.AddOption(outlineServerDefaultUserOption);
            outlineServerSetCommand.SetHandler(OutlineServerCommand.Set, groupArgument, outlineServerNameOption, outlineServerHostnameOption, outlineServerPortOption, outlineServerMetricsOption, outlineServerDefaultUserOption, cancellationTokenBinder);

            outlineServerRemoveCommand.AddAlias("rm");
            outlineServerRemoveCommand.AddArgument(groupsArgumentOneOrMore);
            outlineServerRemoveCommand.AddOption(removeCredsOption);
            outlineServerRemoveCommand.SetHandler(OutlineServerCommand.Remove, groupsArgumentOneOrMore, removeCredsOption, cancellationTokenBinder);

            outlineServerPullCommand.AddAlias("update");
            outlineServerPullCommand.AddArgument(groupsArgumentZeroOrMore);
            outlineServerPullCommand.AddOption(noSyncOption);
            outlineServerPullCommand.SetHandler(OutlineServerCommand.Pull, groupsArgumentZeroOrMore, noSyncOption, cancellationTokenBinder);

            outlineServerDeployCommand.AddArgument(groupsArgumentZeroOrMore);
            outlineServerDeployCommand.SetHandler(OutlineServerCommand.Deploy, groupsArgumentZeroOrMore, cancellationTokenBinder);

            outlineServerRotatePasswordCommand.AddAlias("rotate");
            outlineServerRotatePasswordCommand.AddOption(usernamesOption);
            outlineServerRotatePasswordCommand.AddOption(groupsOption);
            outlineServerRotatePasswordCommand.AddOption(allGroupsOption);
            outlineServerRotatePasswordCommand.AddValidator(OutlineServerCommand.ValidateRotatePassword);
            outlineServerRotatePasswordCommand.SetHandler(OutlineServerCommand.RotatePassword, usernamesOption, groupsOption, allGroupsOption, cancellationTokenBinder);

            reportCommand.AddOption(groupSortByOption);
            reportCommand.AddOption(userSortByOption);
            reportCommand.AddOption(csvOutdirOption);
            reportCommand.SetHandler(ReportCommand.Generate, groupSortByOption, userSortByOption, csvOutdirOption, cancellationTokenBinder);

            settingsGetCommand.SetHandler(SettingsCommand.Get, cancellationTokenBinder);

            settingsSetCommand.AddOption(settingsUserDataUsageDefaultSortByOption);
            settingsSetCommand.AddOption(settingsGroupDataUsageDefaultSortByOption);
            settingsSetCommand.AddOption(settingsOnlineConfigSortByNameOption);
            settingsSetCommand.AddOption(settingsOnlineConfigDeliverByGroupOption);
            settingsSetCommand.AddOption(settingsOnlineConfigCleanOnUserRemovalOption);
            settingsSetCommand.AddOption(settingsOnlineConfigOutputDirectoryOption);
            settingsSetCommand.AddOption(settingsOnlineConfigDeliveryRootUriOption);
            settingsSetCommand.AddOption(settingsOutlineServerApplyDefaultUserOnAssociationOption);
            settingsSetCommand.AddOption(settingsOutlineServerApplyDataLimitOnAssociationOption);
            settingsSetCommand.AddOption(settingsOutlineServerGlobalDefaultUserOption);
            settingsSetCommand.AddOption(settingsApiServerBaseUrlOption);
            settingsSetCommand.AddOption(settingsApiServerSecretPathOption);
            settingsSetCommand.SetHandler(SettingsCommand.Set, settingsSetBinder, cancellationTokenBinder);

            interactiveCommand.SetHandler(
                async () =>
                {
                    while (true)
                    {
                        Console.Write("> ");
                        var inputLine = Console.ReadLine()?.Trim();

                        // Verify input
                        if (inputLine is null or "exit" or "quit")
                            break;
                        if (inputLine is "i" or "interactive" or "repl")
                        {
                            Console.WriteLine("🛑 I see what you're trying to do!");
                            continue;
                        }

                        await rootCommand.InvokeAsync(inputLine);
                    }
                });

            serviceCommand.AddOption(serviceIntervalOption);
            serviceCommand.AddOption(servicePullOutlineServerOption);
            serviceCommand.AddOption(serviceDeployOutlineServerOption);
            serviceCommand.AddOption(serviceGenerateOnlineConfigOption);
            serviceCommand.AddOption(serviceRegenerateOnlineConfigOption);
            serviceCommand.AddValidator(ServiceCommand.ValidateRun);
            serviceCommand.SetHandler(ServiceCommand.Run, serviceIntervalOption, servicePullOutlineServerOption, serviceDeployOutlineServerOption, serviceGenerateOnlineConfigOption, serviceRegenerateOnlineConfigOption, cancellationTokenBinder);

            Console.OutputEncoding = Encoding.UTF8;
            return rootCommand.InvokeAsync(args);
        }
    }
}
