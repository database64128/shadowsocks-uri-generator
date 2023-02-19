using ShadowsocksUriGenerator.CLI.Binders;
using ShadowsocksUriGenerator.CLI.Utils;
using System;
using System.CommandLine;
using System.Text;
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

            var methodArgument = new Argument<string>("method", Parsers.ParseShadowsocksAEADMethod, false, "The encryption method. Use with --password.");
            var passwordArgument = new Argument<string>("password", "The password. Use with --method.");

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

            var iPSKOption = new Option<string[]>("--iPSKs", "Identity PSKs.")
            {
                AllowMultipleArgumentsPerToken = true,
            };
            var clearIPSKOption = new Option<bool>("--clear-iPSKs", "Remove all identity PSKs from the node.");

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

            var nodeAddBinder = new NodeAddBinder(groupArgument, nodenameArgument, hostArgument, portArgument, pluginNameOption, pluginVersionOption, pluginOptionsOption, pluginArgumentsOption, ownerOption, tagsOption, iPSKOption);
            var nodeEditBinder = new NodeEditBinder(groupArgument, nodenameArgument, hostOption, portOption, pluginNameOption, pluginVersionOption, pluginOptionsOption, pluginArgumentsOption, unsetPluginOption, ownerOption, unsetOwnerOption, clearTagsOption, addTagsOption, removeTagsOption, iPSKOption, clearIPSKOption);
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
            userAddCommand.Arguments.Add(usernamesArgumentOneOrMore);
            userAddCommand.SetHandler(UserCommand.Add, usernamesArgumentOneOrMore);

            userRenameCommand.Arguments.Add(oldNameArgument);
            userRenameCommand.Arguments.Add(newNameArgument);
            userRenameCommand.SetHandler(UserCommand.Rename, oldNameArgument, newNameArgument);

            userRemoveCommand.AddAlias("rm");
            userRemoveCommand.AddAlias("del");
            userRemoveCommand.AddAlias("delete");
            userRemoveCommand.Arguments.Add(usernamesArgumentOneOrMore);
            userRemoveCommand.SetHandler(UserCommand.Remove, usernamesArgumentOneOrMore);

            userListCommand.AddAlias("l");
            userListCommand.AddAlias("ls");
            userListCommand.Options.Add(namesOnlyOption);
            userListCommand.Options.Add(onePerLineOption);
            userListCommand.SetHandler(UserCommand.List, namesOnlyOption, onePerLineOption);

            userJoinGroupsCommand.Arguments.Add(usernameArgument);
            userJoinGroupsCommand.Arguments.Add(groupsArgumentZeroOrMore);
            userJoinGroupsCommand.Options.Add(allGroupsOption);
            userJoinGroupsCommand.Validators.Add(Validators.EnforceZeroGroupsWhenAll);
            userJoinGroupsCommand.SetHandler(UserCommand.JoinGroups, usernameArgument, groupsArgumentZeroOrMore, allGroupsOption);

            userLeaveGroupsCommand.Arguments.Add(usernameArgument);
            userLeaveGroupsCommand.Arguments.Add(groupsArgumentZeroOrMore);
            userLeaveGroupsCommand.Options.Add(allGroupsOption);
            userLeaveGroupsCommand.Validators.Add(Validators.EnforceZeroGroupsWhenAll);
            userLeaveGroupsCommand.SetHandler(UserCommand.LeaveGroups, usernameArgument, groupsArgumentZeroOrMore, allGroupsOption);

            userAddCredentialCommand.AddAlias("ac");
            userAddCredentialCommand.Arguments.Add(usernameArgument);
            userAddCredentialCommand.Arguments.Add(methodArgument);
            userAddCredentialCommand.Arguments.Add(passwordArgument);
            userAddCredentialCommand.Arguments.Add(groupsArgumentZeroOrMore);
            userAddCredentialCommand.Options.Add(allGroupsOption);
            userAddCredentialCommand.Validators.Add(Validators.EnforceZeroGroupsWhenAll);
            userAddCredentialCommand.SetHandler(UserCommand.AddCredential, usernameArgument, methodArgument, passwordArgument, groupsArgumentZeroOrMore, allGroupsOption);

            userRemoveCredentialsCommand.AddAlias("rc");
            userRemoveCredentialsCommand.Arguments.Add(usernameArgument);
            userRemoveCredentialsCommand.Arguments.Add(groupsArgumentZeroOrMore);
            userRemoveCredentialsCommand.Options.Add(allGroupsOption);
            userRemoveCredentialsCommand.Validators.Add(Validators.EnforceZeroGroupsWhenAll);
            userRemoveCredentialsCommand.SetHandler(UserCommand.RemoveCredentials, usernameArgument, groupsArgumentZeroOrMore, allGroupsOption);

            userListCredentialsCommand.AddAlias("lc");
            userListCredentialsCommand.Options.Add(usernamesOption);
            userListCredentialsCommand.Options.Add(groupsOption);
            userListCredentialsCommand.SetHandler(UserCommand.ListCredentials, usernamesOption, groupsOption);

            userGetSSLinksCommand.AddAlias("ss");
            userGetSSLinksCommand.Arguments.Add(usernameArgument);
            userGetSSLinksCommand.Options.Add(groupsOption);
            userGetSSLinksCommand.SetHandler(UserCommand.GetSSLinks, usernameArgument, groupsOption);

            userGetDataUsageCommand.AddAlias("data");
            userGetDataUsageCommand.Arguments.Add(usernameArgument);
            userGetDataUsageCommand.Options.Add(sortByOption);
            userGetDataUsageCommand.SetHandler(UserCommand.GetDataUsage, usernameArgument, sortByOption);

            userGetDataLimitCommand.AddAlias("gl");
            userGetDataLimitCommand.AddAlias("gdl");
            userGetDataLimitCommand.AddAlias("limit");
            userGetDataLimitCommand.AddAlias("get-limit");
            userGetDataLimitCommand.Arguments.Add(usernameArgument);
            userGetDataLimitCommand.SetHandler(UserCommand.GetDataLimit, usernameArgument);

            userSetDataLimitCommand.AddAlias("sl");
            userSetDataLimitCommand.AddAlias("sdl");
            userSetDataLimitCommand.AddAlias("set-limit");
            userSetDataLimitCommand.Arguments.Add(usernamesArgumentOneOrMore);
            userSetDataLimitCommand.Options.Add(globalDataLimitOption);
            userSetDataLimitCommand.Options.Add(perGroupDataLimitOption);
            userSetDataLimitCommand.Options.Add(groupsOption);
            userSetDataLimitCommand.Validators.Add(UserCommand.ValidateSetDataLimit);
            userSetDataLimitCommand.SetHandler(UserCommand.SetDataLimit, usernamesArgumentOneOrMore, globalDataLimitOption, perGroupDataLimitOption, groupsOption);

            userOwnGroupsCommand.AddAlias("og");
            userOwnGroupsCommand.Arguments.Add(usernameArgument);
            userOwnGroupsCommand.Arguments.Add(groupsArgumentZeroOrMore);
            userOwnGroupsCommand.Options.Add(allGroupsOption);
            userOwnGroupsCommand.Options.Add(forceOption);
            userOwnGroupsCommand.Validators.Add(Validators.EnforceZeroGroupsWhenAll);
            userOwnGroupsCommand.SetHandler(UserCommand.OwnGroups, usernameArgument, groupsArgumentZeroOrMore, allGroupsOption, forceOption);

            userDisownGroupsCommand.AddAlias("dg");
            userDisownGroupsCommand.Arguments.Add(usernameArgument);
            userDisownGroupsCommand.Arguments.Add(groupsArgumentZeroOrMore);
            userDisownGroupsCommand.Options.Add(allGroupsOption);
            userDisownGroupsCommand.Validators.Add(Validators.EnforceZeroGroupsWhenAll);
            userDisownGroupsCommand.SetHandler(UserCommand.DisownGroups, usernameArgument, groupsArgumentZeroOrMore, allGroupsOption);

            userOwnNodesCommand.AddAlias("on");
            userOwnNodesCommand.Arguments.Add(usernameArgument);
            userOwnNodesCommand.Options.Add(groupsOption);
            userOwnNodesCommand.Options.Add(allGroupsNoAliasesOption);
            userOwnNodesCommand.Options.Add(nodenamesOption);
            userOwnNodesCommand.Options.Add(allNodesNoAliasesOption);
            userOwnNodesCommand.Options.Add(forceOption);
            userOwnNodesCommand.Validators.Add(Validators.EnforceZeroGroupsWhenAll);
            userOwnNodesCommand.Validators.Add(Validators.EnforceZeroNodenamesWhenAll);
            userOwnNodesCommand.SetHandler(UserCommand.OwnNodes, usernameArgument, groupsOption, allGroupsNoAliasesOption, nodenamesOption, allNodesNoAliasesOption, forceOption);

            userDisownNodesCommand.AddAlias("dn");
            userDisownNodesCommand.Arguments.Add(usernameArgument);
            userDisownNodesCommand.Options.Add(groupsOption);
            userDisownNodesCommand.Options.Add(allGroupsNoAliasesOption);
            userDisownNodesCommand.Options.Add(nodenamesOption);
            userDisownNodesCommand.Options.Add(allNodesNoAliasesOption);
            userDisownNodesCommand.Validators.Add(Validators.EnforceZeroGroupsWhenAll);
            userDisownNodesCommand.Validators.Add(Validators.EnforceZeroNodenamesWhenAll);
            userDisownNodesCommand.SetHandler(UserCommand.DisownNodes, usernameArgument, groupsOption, allGroupsNoAliasesOption, nodenamesOption, allNodesNoAliasesOption);

            userListOwnedGroupsCommand.AddAlias("log");
            userListOwnedGroupsCommand.Arguments.Add(usernameArgument);
            userListOwnedGroupsCommand.SetHandler(UserCommand.ListOwnedGroups, usernameArgument);

            userListOwnedNodesCmmand.AddAlias("lon");
            userListOwnedNodesCmmand.Arguments.Add(usernameArgument);
            userListOwnedNodesCmmand.Arguments.Add(groupsArgumentZeroOrMore);
            userListOwnedNodesCmmand.SetHandler(UserCommand.ListOwnedNodes, usernameArgument, groupsArgumentZeroOrMore);

            nodeAddCommand.AddAlias("a");
            nodeAddCommand.Arguments.Add(groupArgument);
            nodeAddCommand.Arguments.Add(nodenameArgument);
            nodeAddCommand.Arguments.Add(hostArgument);
            nodeAddCommand.Arguments.Add(portArgument);
            nodeAddCommand.Options.Add(pluginNameOption);
            nodeAddCommand.Options.Add(pluginVersionOption);
            nodeAddCommand.Options.Add(pluginOptionsOption);
            nodeAddCommand.Options.Add(pluginArgumentsOption);
            nodeAddCommand.Options.Add(ownerOption);
            nodeAddCommand.Options.Add(tagsOption);
            nodeAddCommand.Options.Add(iPSKOption);
            nodeAddCommand.Validators.Add(NodeCommand.ValidateNodePlugin);
            nodeAddCommand.SetHandler(NodeCommand.Add, nodeAddBinder);

            nodeEditCommand.AddAlias("e");
            nodeEditCommand.Arguments.Add(groupArgument);
            nodeEditCommand.Arguments.Add(nodenameArgument);
            nodeEditCommand.Options.Add(hostOption);
            nodeEditCommand.Options.Add(portOption);
            nodeEditCommand.Options.Add(pluginNameOption);
            nodeEditCommand.Options.Add(pluginVersionOption);
            nodeEditCommand.Options.Add(pluginOptionsOption);
            nodeEditCommand.Options.Add(pluginArgumentsOption);
            nodeEditCommand.Options.Add(unsetPluginOption);
            nodeEditCommand.Options.Add(ownerOption);
            nodeEditCommand.Options.Add(unsetOwnerOption);
            nodeEditCommand.Options.Add(clearTagsOption);
            nodeEditCommand.Options.Add(addTagsOption);
            nodeEditCommand.Options.Add(removeTagsOption);
            nodeEditCommand.Options.Add(iPSKOption);
            nodeEditCommand.Options.Add(clearIPSKOption);
            nodeEditCommand.Validators.Add(NodeCommand.ValidateNodePlugin);
            nodeEditCommand.Validators.Add(Validators.ValidateOwnerOptions);
            nodeEditCommand.SetHandler(NodeCommand.Edit, nodeEditBinder);

            nodeRenameCommand.Arguments.Add(groupArgument);
            nodeRenameCommand.Arguments.Add(oldNameArgument);
            nodeRenameCommand.Arguments.Add(newNameArgument);
            nodeRenameCommand.SetHandler(NodeCommand.Rename, groupArgument, oldNameArgument, newNameArgument);

            nodeRemoveCommand.AddAlias("rm");
            nodeRemoveCommand.AddAlias("del");
            nodeRemoveCommand.AddAlias("delete");
            nodeRemoveCommand.Arguments.Add(groupArgument);
            nodeRemoveCommand.Arguments.Add(nodenamesArgumentOneOrMore);
            nodeRemoveCommand.SetHandler(NodeCommand.Remove, groupArgument, nodenamesArgumentOneOrMore);

            nodeListCommand.AddAlias("l");
            nodeListCommand.AddAlias("ls");
            nodeListCommand.Arguments.Add(groupsArgumentZeroOrMore);
            nodeListCommand.Options.Add(namesOnlyOption);
            nodeListCommand.Options.Add(onePerLineOption);
            nodeListCommand.SetHandler(NodeCommand.List, groupsArgumentZeroOrMore, namesOnlyOption, onePerLineOption);

            nodeListAnnotationsCommand.AddAlias("la");
            nodeListAnnotationsCommand.AddAlias("lsa");
            nodeListAnnotationsCommand.Arguments.Add(groupsArgumentZeroOrMore);
            nodeListAnnotationsCommand.Options.Add(onePerLineOption);
            nodeListAnnotationsCommand.SetHandler(NodeCommand.ListAnnotations, groupsArgumentZeroOrMore, onePerLineOption);

            nodeActivateCommand.AddAlias("enable");
            nodeActivateCommand.AddAlias("unhide");
            nodeActivateCommand.Arguments.Add(groupArgument);
            nodeActivateCommand.Arguments.Add(nodenamesArgumentZeroOrMore);
            nodeActivateCommand.Options.Add(allNodesOption);
            nodeActivateCommand.Validators.Add(Validators.EnforceZeroNodenamesWhenAll);
            nodeActivateCommand.SetHandler(NodeCommand.Activate, groupArgument, nodenamesArgumentZeroOrMore, allNodesOption);

            nodeDeactivateCommand.AddAlias("disable");
            nodeDeactivateCommand.AddAlias("hide");
            nodeDeactivateCommand.Arguments.Add(groupArgument);
            nodeDeactivateCommand.Arguments.Add(nodenamesArgumentZeroOrMore);
            nodeDeactivateCommand.Options.Add(allNodesOption);
            nodeDeactivateCommand.Validators.Add(Validators.EnforceZeroNodenamesWhenAll);
            nodeDeactivateCommand.SetHandler(NodeCommand.Deactivate, groupArgument, nodenamesArgumentZeroOrMore, allNodesOption);

            nodeAddTagsCommand.AddAlias("at");
            nodeAddTagsCommand.Arguments.Add(tagsArgument);
            nodeAddTagsCommand.Options.Add(groupsOption);
            nodeAddTagsCommand.Options.Add(allGroupsNoAliasesOption);
            nodeAddTagsCommand.Options.Add(nodenamesOption);
            nodeAddTagsCommand.Options.Add(allNodesNoAliasesOption);
            nodeAddTagsCommand.Validators.Add(Validators.EnforceZeroGroupsWhenAll);
            nodeAddTagsCommand.Validators.Add(Validators.EnforceZeroNodenamesWhenAll);
            nodeAddTagsCommand.SetHandler(NodeCommand.AddTags, tagsArgument, groupsOption, allGroupsNoAliasesOption, nodenamesOption, allNodesNoAliasesOption);

            nodeEditTagsCommand.AddAlias("et");
            nodeEditTagsCommand.Options.Add(groupsOption);
            nodeEditTagsCommand.Options.Add(allGroupsNoAliasesOption);
            nodeEditTagsCommand.Options.Add(nodenamesOption);
            nodeEditTagsCommand.Options.Add(allNodesNoAliasesOption);
            nodeEditTagsCommand.Options.Add(clearTagsOption);
            nodeEditTagsCommand.Options.Add(addTagsOption);
            nodeEditTagsCommand.Options.Add(removeTagsOption);
            nodeEditTagsCommand.Validators.Add(Validators.EnforceZeroGroupsWhenAll);
            nodeEditTagsCommand.Validators.Add(Validators.EnforceZeroNodenamesWhenAll);
            nodeEditTagsCommand.SetHandler(NodeCommand.EditTags, groupsOption, allGroupsNoAliasesOption, nodenamesOption, allNodesNoAliasesOption, clearTagsOption, addTagsOption, removeTagsOption);

            nodeRemoveTagsCommand.AddAlias("rt");
            nodeRemoveTagsCommand.Arguments.Add(tagsArgument);
            nodeRemoveTagsCommand.Options.Add(groupsOption);
            nodeRemoveTagsCommand.Options.Add(allGroupsNoAliasesOption);
            nodeRemoveTagsCommand.Options.Add(nodenamesOption);
            nodeRemoveTagsCommand.Options.Add(allNodesNoAliasesOption);
            nodeRemoveTagsCommand.Validators.Add(Validators.EnforceZeroGroupsWhenAll);
            nodeRemoveTagsCommand.Validators.Add(Validators.EnforceZeroNodenamesWhenAll);
            nodeRemoveTagsCommand.SetHandler(NodeCommand.RemoveTags, tagsArgument, groupsOption, allGroupsNoAliasesOption, nodenamesOption, allNodesNoAliasesOption);

            nodeClearTagsCommand.AddAlias("ct");
            nodeClearTagsCommand.Options.Add(groupsOption);
            nodeClearTagsCommand.Options.Add(allGroupsNoAliasesOption);
            nodeClearTagsCommand.Options.Add(nodenamesOption);
            nodeClearTagsCommand.Options.Add(allNodesNoAliasesOption);
            nodeClearTagsCommand.Validators.Add(Validators.EnforceZeroGroupsWhenAll);
            nodeClearTagsCommand.Validators.Add(Validators.EnforceZeroNodenamesWhenAll);
            nodeClearTagsCommand.SetHandler(NodeCommand.ClearTags, groupsOption, allGroupsNoAliasesOption, nodenamesOption, allNodesNoAliasesOption);

            nodeSetOwnerCommand.AddAlias("so");
            nodeSetOwnerCommand.Arguments.Add(ownerArgument);
            nodeSetOwnerCommand.Options.Add(groupsOption);
            nodeSetOwnerCommand.Options.Add(allGroupsNoAliasesOption);
            nodeSetOwnerCommand.Options.Add(nodenamesOption);
            nodeSetOwnerCommand.Options.Add(allNodesNoAliasesOption);
            nodeSetOwnerCommand.Validators.Add(Validators.EnforceZeroGroupsWhenAll);
            nodeSetOwnerCommand.Validators.Add(Validators.EnforceZeroNodenamesWhenAll);
            nodeSetOwnerCommand.SetHandler(NodeCommand.SetOwner, ownerArgument, groupsOption, allGroupsNoAliasesOption, nodenamesOption, allNodesNoAliasesOption);

            nodeUnsetOwnerCommand.AddAlias("uo");
            nodeUnsetOwnerCommand.Options.Add(groupsOption);
            nodeUnsetOwnerCommand.Options.Add(allGroupsNoAliasesOption);
            nodeUnsetOwnerCommand.Options.Add(nodenamesOption);
            nodeUnsetOwnerCommand.Options.Add(allNodesNoAliasesOption);
            nodeUnsetOwnerCommand.Validators.Add(Validators.EnforceZeroGroupsWhenAll);
            nodeUnsetOwnerCommand.Validators.Add(Validators.EnforceZeroNodenamesWhenAll);
            nodeUnsetOwnerCommand.SetHandler(NodeCommand.UnsetOwner, groupsOption, allGroupsNoAliasesOption, nodenamesOption, allNodesNoAliasesOption);

            groupAddCommand.AddAlias("a");
            groupAddCommand.Arguments.Add(groupsArgumentOneOrMore);
            groupAddCommand.Options.Add(ownerOption);
            groupAddCommand.SetHandler(GroupCommand.Add, groupsArgumentOneOrMore, ownerOption);

            groupEditCommand.AddAlias("e");
            groupEditCommand.Arguments.Add(groupsArgumentOneOrMore);
            groupEditCommand.Options.Add(ownerOption);
            groupEditCommand.Options.Add(unsetOwnerOption);
            groupEditCommand.Validators.Add(Validators.ValidateOwnerOptions);
            groupEditCommand.SetHandler(GroupCommand.Edit, groupsArgumentOneOrMore, ownerOption, unsetOwnerOption);

            groupRenameCommand.Arguments.Add(oldNameArgument);
            groupRenameCommand.Arguments.Add(newNameArgument);
            groupRenameCommand.SetHandler(GroupCommand.Rename, oldNameArgument, newNameArgument);

            groupRemoveCommand.AddAlias("rm");
            groupRemoveCommand.AddAlias("del");
            groupRemoveCommand.AddAlias("delete");
            groupRemoveCommand.Arguments.Add(groupsArgumentOneOrMore);
            groupRemoveCommand.SetHandler(GroupCommand.Remove, groupsArgumentOneOrMore);

            groupListCommand.AddAlias("l");
            groupListCommand.AddAlias("ls");
            groupListCommand.Options.Add(namesOnlyOption);
            groupListCommand.Options.Add(onePerLineOption);
            groupListCommand.SetHandler(GroupCommand.List, namesOnlyOption, onePerLineOption);

            groupAddUsersCommand.AddAlias("au");
            groupAddUsersCommand.Arguments.Add(groupArgument);
            groupAddUsersCommand.Arguments.Add(usernamesArgumentZeroOrMore);
            groupAddUsersCommand.Options.Add(allUsersOption);
            groupAddUsersCommand.Validators.Add(Validators.EnforceZeroUsernamesWhenAll);
            groupAddUsersCommand.SetHandler(GroupCommand.AddUsers, groupArgument, usernamesArgumentZeroOrMore, allUsersOption);

            groupRemoveUsersCommand.AddAlias("ru");
            groupRemoveUsersCommand.Arguments.Add(groupArgument);
            groupRemoveUsersCommand.Arguments.Add(usernamesArgumentZeroOrMore);
            groupRemoveUsersCommand.Options.Add(allUsersOption);
            groupRemoveUsersCommand.Validators.Add(Validators.EnforceZeroUsernamesWhenAll);
            groupRemoveUsersCommand.SetHandler(GroupCommand.RemoveUsers, groupArgument, usernamesArgumentZeroOrMore, allUsersOption);

            groupListUsersCommand.AddAlias("lc");
            groupListUsersCommand.AddAlias("lm");
            groupListUsersCommand.AddAlias("lu");
            groupListUsersCommand.AddAlias("list-credentials");
            groupListUsersCommand.AddAlias("list-members");
            groupListUsersCommand.Arguments.Add(groupArgument);
            groupListUsersCommand.SetHandler(GroupCommand.ListUsers, groupArgument);

            groupAddCredentialCommand.AddAlias("ac");
            groupAddCredentialCommand.Arguments.Add(groupArgument);
            groupAddCredentialCommand.Arguments.Add(methodArgument);
            groupAddCredentialCommand.Arguments.Add(passwordArgument);
            groupAddCredentialCommand.Arguments.Add(usernamesArgumentZeroOrMore);
            groupAddCredentialCommand.Options.Add(allUsersOption);
            groupAddCredentialCommand.Validators.Add(Validators.EnforceZeroUsernamesWhenAll);
            groupAddCredentialCommand.SetHandler(GroupCommand.AddCredential, groupArgument, methodArgument, passwordArgument, usernamesArgumentZeroOrMore, allUsersOption);

            groupRemoveCredentialsCommand.AddAlias("rc");
            groupRemoveCredentialsCommand.Arguments.Add(groupArgument);
            groupRemoveCredentialsCommand.Arguments.Add(usernamesArgumentZeroOrMore);
            groupRemoveCredentialsCommand.Options.Add(allUsersOption);
            groupRemoveCredentialsCommand.Validators.Add(Validators.EnforceZeroUsernamesWhenAll);
            groupRemoveCredentialsCommand.SetHandler(GroupCommand.RemoveCredentials, groupArgument, usernamesArgumentZeroOrMore, allUsersOption);

            groupGetDataUsageCommand.AddAlias("data");
            groupGetDataUsageCommand.Arguments.Add(groupArgument);
            groupGetDataUsageCommand.Options.Add(sortByOption);
            groupGetDataUsageCommand.SetHandler(GroupCommand.GetDataUsage, groupArgument, sortByOption);

            groupGetDataLimitCommand.AddAlias("gl");
            groupGetDataLimitCommand.AddAlias("gdl");
            groupGetDataLimitCommand.AddAlias("limit");
            groupGetDataLimitCommand.AddAlias("get-limit");
            groupGetDataLimitCommand.Arguments.Add(groupArgument);
            groupGetDataLimitCommand.SetHandler(GroupCommand.GetDataLimit, groupArgument);

            groupSetDataLimitCommand.AddAlias("sl");
            groupSetDataLimitCommand.AddAlias("sdl");
            groupSetDataLimitCommand.AddAlias("set-limit");
            groupSetDataLimitCommand.Arguments.Add(groupsArgumentOneOrMore);
            groupSetDataLimitCommand.Options.Add(globalDataLimitOption);
            groupSetDataLimitCommand.Options.Add(perUserDataLimitOption);
            groupSetDataLimitCommand.Options.Add(usernamesOption);
            groupSetDataLimitCommand.Validators.Add(GroupCommand.ValidateSetDataLimit);
            groupSetDataLimitCommand.SetHandler(GroupCommand.SetDataLimit, groupsArgumentOneOrMore, globalDataLimitOption, perUserDataLimitOption, usernamesOption);

            onlineConfigGenerateCommand.AddAlias("g");
            onlineConfigGenerateCommand.AddAlias("gen");
            onlineConfigGenerateCommand.Arguments.Add(usernamesArgumentZeroOrMore);
            onlineConfigGenerateCommand.SetHandler(OnlineConfigCommand.Generate, usernamesArgumentZeroOrMore);

            onlineConfigGetLinksCommand.AddAlias("l");
            onlineConfigGetLinksCommand.AddAlias("link");
            onlineConfigGetLinksCommand.AddAlias("links");
            onlineConfigGetLinksCommand.AddAlias("token");
            onlineConfigGetLinksCommand.AddAlias("tokens");
            onlineConfigGetLinksCommand.AddAlias("url");
            onlineConfigGetLinksCommand.AddAlias("urls");
            onlineConfigGetLinksCommand.Arguments.Add(usernamesArgumentZeroOrMore);
            onlineConfigGetLinksCommand.SetHandler(OnlineConfigCommand.GetLinks, usernamesArgumentZeroOrMore);

            onlineConfigCleanCommand.AddAlias("c");
            onlineConfigCleanCommand.AddAlias("clear");
            onlineConfigCleanCommand.Arguments.Add(usernamesArgumentZeroOrMore);
            onlineConfigCleanCommand.Options.Add(allUsersOption);
            onlineConfigCleanCommand.Validators.Add(Validators.EnforceZeroUsernamesWhenAll);
            onlineConfigCleanCommand.SetHandler(OnlineConfigCommand.Clean, usernamesArgumentZeroOrMore, allUsersOption);

            outlineServerAddCommand.AddAlias("a");
            outlineServerAddCommand.Arguments.Add(groupArgument);
            outlineServerAddCommand.Arguments.Add(outlineApiKeyArgument);
            outlineServerAddCommand.SetHandler(OutlineServerCommand.Add, groupArgument, outlineApiKeyArgument);

            outlineServerGetCommand.Arguments.Add(groupArgument);
            outlineServerGetCommand.SetHandler(OutlineServerCommand.Get, groupArgument);

            outlineServerSetCommand.Arguments.Add(groupArgument);
            outlineServerSetCommand.Options.Add(outlineServerNameOption);
            outlineServerSetCommand.Options.Add(outlineServerHostnameOption);
            outlineServerSetCommand.Options.Add(outlineServerPortOption);
            outlineServerSetCommand.Options.Add(outlineServerMetricsOption);
            outlineServerSetCommand.Options.Add(outlineServerDefaultUserOption);
            outlineServerSetCommand.SetHandler(OutlineServerCommand.Set, groupArgument, outlineServerNameOption, outlineServerHostnameOption, outlineServerPortOption, outlineServerMetricsOption, outlineServerDefaultUserOption);

            outlineServerRemoveCommand.AddAlias("rm");
            outlineServerRemoveCommand.Arguments.Add(groupsArgumentOneOrMore);
            outlineServerRemoveCommand.Options.Add(removeCredsOption);
            outlineServerRemoveCommand.SetHandler(OutlineServerCommand.Remove, groupsArgumentOneOrMore, removeCredsOption);

            outlineServerPullCommand.AddAlias("update");
            outlineServerPullCommand.Arguments.Add(groupsArgumentZeroOrMore);
            outlineServerPullCommand.Options.Add(noSyncOption);
            outlineServerPullCommand.SetHandler(OutlineServerCommand.Pull, groupsArgumentZeroOrMore, noSyncOption);

            outlineServerDeployCommand.Arguments.Add(groupsArgumentZeroOrMore);
            outlineServerDeployCommand.SetHandler(OutlineServerCommand.Deploy, groupsArgumentZeroOrMore);

            outlineServerRotatePasswordCommand.AddAlias("rotate");
            outlineServerRotatePasswordCommand.Options.Add(usernamesOption);
            outlineServerRotatePasswordCommand.Options.Add(groupsOption);
            outlineServerRotatePasswordCommand.Options.Add(allGroupsOption);
            outlineServerRotatePasswordCommand.Validators.Add(OutlineServerCommand.ValidateRotatePassword);
            outlineServerRotatePasswordCommand.SetHandler(OutlineServerCommand.RotatePassword, usernamesOption, groupsOption, allGroupsOption);

            reportCommand.Options.Add(groupSortByOption);
            reportCommand.Options.Add(userSortByOption);
            reportCommand.Options.Add(csvOutdirOption);
            reportCommand.SetHandler(ReportCommand.Generate, groupSortByOption, userSortByOption, csvOutdirOption);

            settingsGetCommand.SetHandler(SettingsCommand.Get);

            settingsSetCommand.Options.Add(settingsUserDataUsageDefaultSortByOption);
            settingsSetCommand.Options.Add(settingsGroupDataUsageDefaultSortByOption);
            settingsSetCommand.Options.Add(settingsOnlineConfigSortByNameOption);
            settingsSetCommand.Options.Add(settingsOnlineConfigDeliverByGroupOption);
            settingsSetCommand.Options.Add(settingsOnlineConfigCleanOnUserRemovalOption);
            settingsSetCommand.Options.Add(settingsOnlineConfigOutputDirectoryOption);
            settingsSetCommand.Options.Add(settingsOnlineConfigDeliveryRootUriOption);
            settingsSetCommand.Options.Add(settingsOutlineServerApplyDefaultUserOnAssociationOption);
            settingsSetCommand.Options.Add(settingsOutlineServerApplyDataLimitOnAssociationOption);
            settingsSetCommand.Options.Add(settingsOutlineServerGlobalDefaultUserOption);
            settingsSetCommand.Options.Add(settingsApiServerBaseUrlOption);
            settingsSetCommand.Options.Add(settingsApiServerSecretPathOption);
            settingsSetCommand.SetHandler(SettingsCommand.Set, settingsSetBinder);

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

            serviceCommand.Options.Add(serviceIntervalOption);
            serviceCommand.Options.Add(servicePullOutlineServerOption);
            serviceCommand.Options.Add(serviceDeployOutlineServerOption);
            serviceCommand.Options.Add(serviceGenerateOnlineConfigOption);
            serviceCommand.Options.Add(serviceRegenerateOnlineConfigOption);
            serviceCommand.Validators.Add(ServiceCommand.ValidateRun);
            serviceCommand.SetHandler(ServiceCommand.Run, serviceIntervalOption, servicePullOutlineServerOption, serviceDeployOutlineServerOption, serviceGenerateOnlineConfigOption, serviceRegenerateOnlineConfigOption);

            Console.OutputEncoding = Encoding.UTF8;
            return rootCommand.InvokeAsync(args);
        }
    }
}
