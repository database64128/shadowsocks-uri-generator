using ShadowsocksUriGenerator.CLI.Binders;
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

            var usernameArgument = new Argument<string>("username")
            {
                Description = "Target user.",
            };
            var nodenameArgument = new Argument<string>("nodename")
            {
                Description = "Name of the node.",
            };
            var groupArgument = new Argument<string>("group")
            {
                Description = "Target group.",
            };

            var oldNameArgument = new Argument<string>("oldName")
            {
                Description = "Current name.",
            };
            var newNameArgument = new Argument<string>("newName")
            {
                Description = "New name.",
            };

            var hostArgument = new Argument<string>("host")
            {
                Description = "Hostname of the node.",
            };
            var portArgument = new Argument<int>("port")
            {
                Description = "Port number of the node.",
                CustomParser = Parsers.ParsePortNumber,
            };

            var methodArgument = new Argument<string>("method")
            {
                Description = "The encryption method. Use with --password.",
                CustomParser = Parsers.ParseShadowsocksAEADMethod,
            };
            var passwordArgument = new Argument<string>("password")
            {
                Description = "The password. Use with --method.",
            };

            var ownerArgument = new Argument<string>("owner")
            {
                Description = "Set the owner.",
            };
            var tagsArgument = new Argument<string[]>("tags")
            {
                Description = "Tags that annotate the node. Will be deduplicated in a case-insensitive manner.",
                Arity = ArgumentArity.OneOrMore,
            };

            var usernamesArgumentZeroOrMore = new Argument<string[]>("usernames")
            {
                Description = "Zero or more usernames.",
            };
            var nodenamesArgumentZeroOrMore = new Argument<string[]>("nodenames")
            {
                Description = "Zero or more node names.",
            };
            var groupsArgumentZeroOrMore = new Argument<string[]>("groups")
            {
                Description = "Zero or more group names.",
            };

            var usernamesArgumentOneOrMore = new Argument<string[]>("usernames")
            {
                Description = "One or more usernames.",
                Arity = ArgumentArity.OneOrMore,
            };
            var nodenamesArgumentOneOrMore = new Argument<string[]>("nodenames")
            {
                Description = "One or more node names.",
                Arity = ArgumentArity.OneOrMore,
            };
            var groupsArgumentOneOrMore = new Argument<string[]>("groups")
            {
                Description = "One or more group names.",
                Arity = ArgumentArity.OneOrMore,
            };

            var outlineApiKeyArgument = new Argument<string>("apiKey")
            {
                Description = "The Outline server API key.",
            };

            var usernamesOption = new Option<string[]>("--usernames")
            {
                Description = "Target these specific users. If unspecified, target all users.",
                AllowMultipleArgumentsPerToken = true,
            };
            var nodenamesOption = new Option<string[]>("--nodenames")
            {
                Description = "Target these specific nodes. If unspecified, target all nodes.",
                AllowMultipleArgumentsPerToken = true,
            };
            var groupsOption = new Option<string[]>("--groups")
            {
                Description = "Target these specific groups. If unspecified, target all groups.",
                AllowMultipleArgumentsPerToken = true,
            };

            var hostOption = new Option<string?>("--host")
            {
                Description = "Hostname of the node.",
            };
            var portOption = new Option<int>("--port")
            {
                Description = "Port number of the node.",
                CustomParser = Parsers.ParsePortNumber,
            };
            var pluginNameOption = new Option<string?>("--plugin-name")
            {
                Description = "Plugin name.",
            };
            var pluginVersionOption = new Option<string?>("--plugin-version")
            {
                Description = "Required plugin version.",
            };
            var pluginOptionsOption = new Option<string?>("--plugin-options")
            {
                Description = "Plugin options, passed as environment variable 'SS_PLUGIN_OPTIONS'.",
            };
            var pluginArgumentsOption = new Option<string?>("--plugin-arguments")
            {
                Description = "Plugin startup arguments.",
            };
            var unsetPluginOption = new Option<bool>("--unset-plugin")
            {
                Description = "Remove plugin and plugin options from the node.",
            };

            var ownerOption = new Option<string?>("--owner")
            {
                Description = "Set the owner.",
            };
            var unsetOwnerOption = new Option<bool>("--unset-owner")
            {
                Description = "Unset the owner.",
            };

            var forceOption = new Option<bool>("--force", "-f")
            {
                Description = "Forcibly overwrite existing settings.",
            };

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
            var clearTagsOption = new Option<bool>("--clear-tags")
            {
                Description = "Remove all tags from the node.",
            };

            var iPSKOption = new Option<string[]>("--iPSKs", "Identity PSKs.")
            {
                AllowMultipleArgumentsPerToken = true,
            };
            var clearIPSKOption = new Option<bool>("--clear-iPSKs")
            {
                Description = "Remove all identity PSKs from the node.",
            };

            var globalDataLimitOption = new Option<ulong?>("--global")
            {
                Description = "The global data limit in bytes. 0 is interpreted as unlimited. Examples: '1024', '2K', '4M', '8G', '16T', '32P'.",
                CustomParser = Parsers.ParseDataString,
            };
            var perUserDataLimitOption = new Option<ulong?>("--per-user")
            {
                Description = "The per-user data limit in bytes. 0 is interpreted as unlimited. Examples: '1024', '2K', '4M', '8G', '16T', '32P'.",
                CustomParser = Parsers.ParseDataString,
            };
            var perGroupDataLimitOption = new Option<ulong?>("--per-group")
            {
                Description = "The per-group data limit in bytes. 0 is interpreted as unlimited. Examples: '1024', '2K', '4M', '8G', '16T', '32P'.",
                CustomParser = Parsers.ParseDataString,
            };

            var namesOnlyOption = new Option<bool>("--names-only", "--short", "-s")
            {
                Description = "Display names only, without a table.",
            };
            var onePerLineOption = new Option<bool>("--one-per-line", "-1")
            {
                Description = "Display one name per line.",
            };

            var allUsersOption = new Option<bool>("--all-users", "--all", "-a")
            {
                Description = "Target all users.",
            };
            var allNodesOption = new Option<bool>("--all-nodes", "--all", "-a")
            {
                Description = "Target all nodes in target group.",
            };
            var allGroupsOption = new Option<bool>("--all-groups", "--all", "-a")
            {
                Description = "Target all groups.",
            };

            var allUsersNoAliasesOption = new Option<bool>("--all-users")
            {
                Description = "Target all users.",
            };
            var allNodesNoAliasesOption = new Option<bool>("--all-nodes")
            {
                Description = "Target all nodes in target group.",
            };
            var allGroupsNoAliasesOption = new Option<bool>("--all-groups")
            {
                Description = "Target all groups.",
            };

            var sortByOption = new Option<SortBy?>("--sort-by")
            {
                Description = "Sort rule for data usage records.",
            };
            var userSortByOption = new Option<SortBy?>("--user-sort-by")
            {
                Description = "Sort rule for user data usage records.",
            };
            var groupSortByOption = new Option<SortBy?>("--group-sort-by")
            {
                Description = "Sort rule for group data usage records.",
            };

            var outlineServerNameOption = new Option<string?>("--name")
            {
                Description = "Name of the Outline server.",
            };
            var outlineServerHostnameOption = new Option<string?>("--hostname")
            {
                Description = "Hostname of the Outline server.",
            };
            var outlineServerPortOption = new Option<int?>("--port")
            {
                Description = "Port number for new access keys on the Outline server.",
            };
            var outlineServerMetricsOption = new Option<bool?>("--metrics")
            {
                Description = "Enable or disable telemetry on the Outline server.",
            };
            var outlineServerDefaultUserOption = new Option<string?>("--default-user")
            {
                Description = "The default user for Outline server's default access key (id: 0).",
            };

            var removeCredsOption = new Option<bool>("--remove-creds")
            {
                Description = "Remove credentials from all associated users.",
            };
            var noSyncOption = new Option<bool>("--no-sync")
            {
                Description = "Do not update local user membership storage from retrieved access key list.",
            };

            var csvOutdirOption = new Option<string?>("--csv-outdir")
            {
                Description = "Export as CSV to the specified directory.",
            };

            var settingsUserDataUsageDefaultSortByOption = new Option<SortBy?>("--user-data-usage-default-sort-by")
            {
                Description = "The default sort rule for user data usage report.",
            };
            var settingsGroupDataUsageDefaultSortByOption = new Option<SortBy?>("--group-data-usage-default-sort-by")
            {
                Description = "The default sort rule for group data usage report.",
            };
            var settingsOnlineConfigSortByNameOption = new Option<bool?>("--online-config-sort-by-name")
            {
                Description = "Whether online config should sort servers by name.",
            };
            var settingsOnlineConfigDeliverByGroupOption = new Option<bool?>("--online-config-deliver-by-group")
            {
                Description = "Whether the legacy SIP008 online config static file generator should generate per-group SIP008 delivery JSON in addition to the single JSON that contains all associated servers of the user.",
            };
            var settingsOnlineConfigCleanOnUserRemovalOption = new Option<bool?>("--online-config-clean-on-user-removal")
            {
                Description = "Whether the user's generated static online config files should be removed when the user is being removed.",
            };
            var settingsOnlineConfigOutputDirectoryOption = new Option<string?>("--online-config-output-directory")
            {
                Description = "Legacy SIP008 online config static file generator output directory. No trailing slashes allowed.",
            };
            var settingsOnlineConfigDeliveryRootUriOption = new Option<string?>("--online-config-delivery-root-uri")
            {
                Description = "URL base for the SIP008 static file delivery links. No trailing slashes allowed.",
            };
            var settingsOutlineServerApplyDefaultUserOnAssociationOption = new Option<bool?>("--outline-server-apply-default-user-on-association")
            {
                Description = "Whether to apply the global default user when associating with Outline servers.",
            };
            var settingsOutlineServerApplyDataLimitOnAssociationOption = new Option<bool?>("--outline-server-apply-data-limit-on-association")
            {
                Description = "Whether to apply the group's per-user data limit when associating with Outline servers.",
            };
            var settingsOutlineServerGlobalDefaultUserOption = new Option<string?>("--outline-server-global-default-user")
            {
                Description = "The global setting for Outline server's default access key's user.",
            };
            var settingsApiServerBaseUrlOption = new Option<string?>("--api-server-base-url")
            {
                Description = "The base URL of the API server. MUST NOT contain a trailing slash.",
            };
            var settingsApiServerSecretPathOption = new Option<string?>("--api-server-secret-path")
            {
                Description = "The secret path to the API endpoint. This is required to conceal the presence of the API. The secret MAY contain zero or more forward slashes (/) to allow flexible path hierarchy. But it's recommended to put non-secret part of the path in the base URL.",
            };

            var serviceIntervalOption = new Option<int>("--interval")
            {
                Description = "The interval between each scheduled run in seconds.",
                DefaultValueFactory = _ => 3600,
            };
            var servicePullOutlineServerOption = new Option<bool>("--pull-outline-server")
            {
                Description = "Pull from Outline servers for updates of server information, access keys, data usage.",
            };
            var serviceDeployOutlineServerOption = new Option<bool>("--deploy-outline-server")
            {
                Description = "Deploy local configurations to Outline servers.",
            };
            var serviceGenerateOnlineConfigOption = new Option<bool>("--generate-online-config")
            {
                Description = "Generate online config.",
            };
            var serviceRegenerateOnlineConfigOption = new Option<bool>("--regenerate-online-config")
            {
                Description = "Clean and regenerate online config.",
            };

            var nodeAddBinder = new NodeAddBinder(groupArgument, nodenameArgument, hostArgument, portArgument, pluginNameOption, pluginVersionOption, pluginOptionsOption, pluginArgumentsOption, ownerOption, tagsOption, iPSKOption);
            var nodeEditBinder = new NodeEditBinder(groupArgument, nodenameArgument, hostOption, portOption, pluginNameOption, pluginVersionOption, pluginOptionsOption, pluginArgumentsOption, unsetPluginOption, ownerOption, unsetOwnerOption, clearTagsOption, addTagsOption, removeTagsOption, iPSKOption, clearIPSKOption);
            var settingsSetBinder = new SettingsSetBinder(settingsUserDataUsageDefaultSortByOption, settingsGroupDataUsageDefaultSortByOption, settingsOnlineConfigSortByNameOption, settingsOnlineConfigDeliverByGroupOption, settingsOnlineConfigCleanOnUserRemovalOption, settingsOnlineConfigOutputDirectoryOption, settingsOnlineConfigDeliveryRootUriOption, settingsOutlineServerApplyDefaultUserOnAssociationOption, settingsOutlineServerApplyDataLimitOnAssociationOption, settingsOutlineServerGlobalDefaultUserOption, settingsApiServerBaseUrlOption, settingsApiServerSecretPathOption);

            userCommand.Aliases.Add("u");
            nodeCommand.Aliases.Add("n");
            groupCommand.Aliases.Add("g");
            onlineConfigCommand.Aliases.Add("oc");
            onlineConfigCommand.Aliases.Add("ooc");
            onlineConfigCommand.Aliases.Add("online");
            outlineServerCommand.Aliases.Add("os");
            outlineServerCommand.Aliases.Add("outline");
            reportCommand.Aliases.Add("r");
            settingsCommand.Aliases.Add("s");
            interactiveCommand.Aliases.Add("i");
            interactiveCommand.Aliases.Add("repl");

            userAddCommand.Aliases.Add("a");
            userAddCommand.Arguments.Add(usernamesArgumentOneOrMore);
            userAddCommand.SetHandler(UserCommand.Add, usernamesArgumentOneOrMore);

            userRenameCommand.Arguments.Add(oldNameArgument);
            userRenameCommand.Arguments.Add(newNameArgument);
            userRenameCommand.SetHandler(UserCommand.Rename, oldNameArgument, newNameArgument);

            userRemoveCommand.Aliases.Add("rm");
            userRemoveCommand.Aliases.Add("del");
            userRemoveCommand.Aliases.Add("delete");
            userRemoveCommand.Arguments.Add(usernamesArgumentOneOrMore);
            userRemoveCommand.SetHandler(UserCommand.Remove, usernamesArgumentOneOrMore);

            userListCommand.Aliases.Add("l");
            userListCommand.Aliases.Add("ls");
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

            userAddCredentialCommand.Aliases.Add("ac");
            userAddCredentialCommand.Arguments.Add(usernameArgument);
            userAddCredentialCommand.Arguments.Add(methodArgument);
            userAddCredentialCommand.Arguments.Add(passwordArgument);
            userAddCredentialCommand.Arguments.Add(groupsArgumentZeroOrMore);
            userAddCredentialCommand.Options.Add(allGroupsOption);
            userAddCredentialCommand.Validators.Add(Validators.EnforceZeroGroupsWhenAll);
            userAddCredentialCommand.SetHandler(UserCommand.AddCredential, usernameArgument, methodArgument, passwordArgument, groupsArgumentZeroOrMore, allGroupsOption);

            userRemoveCredentialsCommand.Aliases.Add("rc");
            userRemoveCredentialsCommand.Arguments.Add(usernameArgument);
            userRemoveCredentialsCommand.Arguments.Add(groupsArgumentZeroOrMore);
            userRemoveCredentialsCommand.Options.Add(allGroupsOption);
            userRemoveCredentialsCommand.Validators.Add(Validators.EnforceZeroGroupsWhenAll);
            userRemoveCredentialsCommand.SetHandler(UserCommand.RemoveCredentials, usernameArgument, groupsArgumentZeroOrMore, allGroupsOption);

            userListCredentialsCommand.Aliases.Add("lc");
            userListCredentialsCommand.Options.Add(usernamesOption);
            userListCredentialsCommand.Options.Add(groupsOption);
            userListCredentialsCommand.SetHandler(UserCommand.ListCredentials, usernamesOption, groupsOption);

            userGetSSLinksCommand.Aliases.Add("ss");
            userGetSSLinksCommand.Arguments.Add(usernameArgument);
            userGetSSLinksCommand.Options.Add(groupsOption);
            userGetSSLinksCommand.SetHandler(UserCommand.GetSSLinks, usernameArgument, groupsOption);

            userGetDataUsageCommand.Aliases.Add("data");
            userGetDataUsageCommand.Arguments.Add(usernameArgument);
            userGetDataUsageCommand.Options.Add(sortByOption);
            userGetDataUsageCommand.SetHandler(UserCommand.GetDataUsage, usernameArgument, sortByOption);

            userGetDataLimitCommand.Aliases.Add("gl");
            userGetDataLimitCommand.Aliases.Add("gdl");
            userGetDataLimitCommand.Aliases.Add("limit");
            userGetDataLimitCommand.Aliases.Add("get-limit");
            userGetDataLimitCommand.Arguments.Add(usernameArgument);
            userGetDataLimitCommand.SetHandler(UserCommand.GetDataLimit, usernameArgument);

            userSetDataLimitCommand.Aliases.Add("sl");
            userSetDataLimitCommand.Aliases.Add("sdl");
            userSetDataLimitCommand.Aliases.Add("set-limit");
            userSetDataLimitCommand.Arguments.Add(usernamesArgumentOneOrMore);
            userSetDataLimitCommand.Options.Add(globalDataLimitOption);
            userSetDataLimitCommand.Options.Add(perGroupDataLimitOption);
            userSetDataLimitCommand.Options.Add(groupsOption);
            userSetDataLimitCommand.Validators.Add(UserCommand.ValidateSetDataLimit);
            userSetDataLimitCommand.SetHandler(UserCommand.SetDataLimit, usernamesArgumentOneOrMore, globalDataLimitOption, perGroupDataLimitOption, groupsOption);

            userOwnGroupsCommand.Aliases.Add("og");
            userOwnGroupsCommand.Arguments.Add(usernameArgument);
            userOwnGroupsCommand.Arguments.Add(groupsArgumentZeroOrMore);
            userOwnGroupsCommand.Options.Add(allGroupsOption);
            userOwnGroupsCommand.Options.Add(forceOption);
            userOwnGroupsCommand.Validators.Add(Validators.EnforceZeroGroupsWhenAll);
            userOwnGroupsCommand.SetHandler(UserCommand.OwnGroups, usernameArgument, groupsArgumentZeroOrMore, allGroupsOption, forceOption);

            userDisownGroupsCommand.Aliases.Add("dg");
            userDisownGroupsCommand.Arguments.Add(usernameArgument);
            userDisownGroupsCommand.Arguments.Add(groupsArgumentZeroOrMore);
            userDisownGroupsCommand.Options.Add(allGroupsOption);
            userDisownGroupsCommand.Validators.Add(Validators.EnforceZeroGroupsWhenAll);
            userDisownGroupsCommand.SetHandler(UserCommand.DisownGroups, usernameArgument, groupsArgumentZeroOrMore, allGroupsOption);

            userOwnNodesCommand.Aliases.Add("on");
            userOwnNodesCommand.Arguments.Add(usernameArgument);
            userOwnNodesCommand.Options.Add(groupsOption);
            userOwnNodesCommand.Options.Add(allGroupsNoAliasesOption);
            userOwnNodesCommand.Options.Add(nodenamesOption);
            userOwnNodesCommand.Options.Add(allNodesNoAliasesOption);
            userOwnNodesCommand.Options.Add(forceOption);
            userOwnNodesCommand.Validators.Add(Validators.EnforceZeroGroupsWhenAll);
            userOwnNodesCommand.Validators.Add(Validators.EnforceZeroNodenamesWhenAll);
            userOwnNodesCommand.SetHandler(UserCommand.OwnNodes, usernameArgument, groupsOption, allGroupsNoAliasesOption, nodenamesOption, allNodesNoAliasesOption, forceOption);

            userDisownNodesCommand.Aliases.Add("dn");
            userDisownNodesCommand.Arguments.Add(usernameArgument);
            userDisownNodesCommand.Options.Add(groupsOption);
            userDisownNodesCommand.Options.Add(allGroupsNoAliasesOption);
            userDisownNodesCommand.Options.Add(nodenamesOption);
            userDisownNodesCommand.Options.Add(allNodesNoAliasesOption);
            userDisownNodesCommand.Validators.Add(Validators.EnforceZeroGroupsWhenAll);
            userDisownNodesCommand.Validators.Add(Validators.EnforceZeroNodenamesWhenAll);
            userDisownNodesCommand.SetHandler(UserCommand.DisownNodes, usernameArgument, groupsOption, allGroupsNoAliasesOption, nodenamesOption, allNodesNoAliasesOption);

            userListOwnedGroupsCommand.Aliases.Add("log");
            userListOwnedGroupsCommand.Arguments.Add(usernameArgument);
            userListOwnedGroupsCommand.SetHandler(UserCommand.ListOwnedGroups, usernameArgument);

            userListOwnedNodesCmmand.Aliases.Add("lon");
            userListOwnedNodesCmmand.Arguments.Add(usernameArgument);
            userListOwnedNodesCmmand.Arguments.Add(groupsArgumentZeroOrMore);
            userListOwnedNodesCmmand.SetHandler(UserCommand.ListOwnedNodes, usernameArgument, groupsArgumentZeroOrMore);

            nodeAddCommand.Aliases.Add("a");
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

            nodeEditCommand.Aliases.Add("e");
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

            nodeRemoveCommand.Aliases.Add("rm");
            nodeRemoveCommand.Aliases.Add("del");
            nodeRemoveCommand.Aliases.Add("delete");
            nodeRemoveCommand.Arguments.Add(groupArgument);
            nodeRemoveCommand.Arguments.Add(nodenamesArgumentOneOrMore);
            nodeRemoveCommand.SetHandler(NodeCommand.Remove, groupArgument, nodenamesArgumentOneOrMore);

            nodeListCommand.Aliases.Add("l");
            nodeListCommand.Aliases.Add("ls");
            nodeListCommand.Arguments.Add(groupsArgumentZeroOrMore);
            nodeListCommand.Options.Add(namesOnlyOption);
            nodeListCommand.Options.Add(onePerLineOption);
            nodeListCommand.SetHandler(NodeCommand.List, groupsArgumentZeroOrMore, namesOnlyOption, onePerLineOption);

            nodeListAnnotationsCommand.Aliases.Add("la");
            nodeListAnnotationsCommand.Aliases.Add("lsa");
            nodeListAnnotationsCommand.Arguments.Add(groupsArgumentZeroOrMore);
            nodeListAnnotationsCommand.Options.Add(onePerLineOption);
            nodeListAnnotationsCommand.SetHandler(NodeCommand.ListAnnotations, groupsArgumentZeroOrMore, onePerLineOption);

            nodeActivateCommand.Aliases.Add("enable");
            nodeActivateCommand.Aliases.Add("unhide");
            nodeActivateCommand.Arguments.Add(groupArgument);
            nodeActivateCommand.Arguments.Add(nodenamesArgumentZeroOrMore);
            nodeActivateCommand.Options.Add(allNodesOption);
            nodeActivateCommand.Validators.Add(Validators.EnforceZeroNodenamesWhenAll);
            nodeActivateCommand.SetHandler(NodeCommand.Activate, groupArgument, nodenamesArgumentZeroOrMore, allNodesOption);

            nodeDeactivateCommand.Aliases.Add("disable");
            nodeDeactivateCommand.Aliases.Add("hide");
            nodeDeactivateCommand.Arguments.Add(groupArgument);
            nodeDeactivateCommand.Arguments.Add(nodenamesArgumentZeroOrMore);
            nodeDeactivateCommand.Options.Add(allNodesOption);
            nodeDeactivateCommand.Validators.Add(Validators.EnforceZeroNodenamesWhenAll);
            nodeDeactivateCommand.SetHandler(NodeCommand.Deactivate, groupArgument, nodenamesArgumentZeroOrMore, allNodesOption);

            nodeAddTagsCommand.Aliases.Add("at");
            nodeAddTagsCommand.Arguments.Add(tagsArgument);
            nodeAddTagsCommand.Options.Add(groupsOption);
            nodeAddTagsCommand.Options.Add(allGroupsNoAliasesOption);
            nodeAddTagsCommand.Options.Add(nodenamesOption);
            nodeAddTagsCommand.Options.Add(allNodesNoAliasesOption);
            nodeAddTagsCommand.Validators.Add(Validators.EnforceZeroGroupsWhenAll);
            nodeAddTagsCommand.Validators.Add(Validators.EnforceZeroNodenamesWhenAll);
            nodeAddTagsCommand.SetHandler(NodeCommand.AddTags, tagsArgument, groupsOption, allGroupsNoAliasesOption, nodenamesOption, allNodesNoAliasesOption);

            nodeEditTagsCommand.Aliases.Add("et");
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

            nodeRemoveTagsCommand.Aliases.Add("rt");
            nodeRemoveTagsCommand.Arguments.Add(tagsArgument);
            nodeRemoveTagsCommand.Options.Add(groupsOption);
            nodeRemoveTagsCommand.Options.Add(allGroupsNoAliasesOption);
            nodeRemoveTagsCommand.Options.Add(nodenamesOption);
            nodeRemoveTagsCommand.Options.Add(allNodesNoAliasesOption);
            nodeRemoveTagsCommand.Validators.Add(Validators.EnforceZeroGroupsWhenAll);
            nodeRemoveTagsCommand.Validators.Add(Validators.EnforceZeroNodenamesWhenAll);
            nodeRemoveTagsCommand.SetHandler(NodeCommand.RemoveTags, tagsArgument, groupsOption, allGroupsNoAliasesOption, nodenamesOption, allNodesNoAliasesOption);

            nodeClearTagsCommand.Aliases.Add("ct");
            nodeClearTagsCommand.Options.Add(groupsOption);
            nodeClearTagsCommand.Options.Add(allGroupsNoAliasesOption);
            nodeClearTagsCommand.Options.Add(nodenamesOption);
            nodeClearTagsCommand.Options.Add(allNodesNoAliasesOption);
            nodeClearTagsCommand.Validators.Add(Validators.EnforceZeroGroupsWhenAll);
            nodeClearTagsCommand.Validators.Add(Validators.EnforceZeroNodenamesWhenAll);
            nodeClearTagsCommand.SetHandler(NodeCommand.ClearTags, groupsOption, allGroupsNoAliasesOption, nodenamesOption, allNodesNoAliasesOption);

            nodeSetOwnerCommand.Aliases.Add("so");
            nodeSetOwnerCommand.Arguments.Add(ownerArgument);
            nodeSetOwnerCommand.Options.Add(groupsOption);
            nodeSetOwnerCommand.Options.Add(allGroupsNoAliasesOption);
            nodeSetOwnerCommand.Options.Add(nodenamesOption);
            nodeSetOwnerCommand.Options.Add(allNodesNoAliasesOption);
            nodeSetOwnerCommand.Validators.Add(Validators.EnforceZeroGroupsWhenAll);
            nodeSetOwnerCommand.Validators.Add(Validators.EnforceZeroNodenamesWhenAll);
            nodeSetOwnerCommand.SetHandler(NodeCommand.SetOwner, ownerArgument, groupsOption, allGroupsNoAliasesOption, nodenamesOption, allNodesNoAliasesOption);

            nodeUnsetOwnerCommand.Aliases.Add("uo");
            nodeUnsetOwnerCommand.Options.Add(groupsOption);
            nodeUnsetOwnerCommand.Options.Add(allGroupsNoAliasesOption);
            nodeUnsetOwnerCommand.Options.Add(nodenamesOption);
            nodeUnsetOwnerCommand.Options.Add(allNodesNoAliasesOption);
            nodeUnsetOwnerCommand.Validators.Add(Validators.EnforceZeroGroupsWhenAll);
            nodeUnsetOwnerCommand.Validators.Add(Validators.EnforceZeroNodenamesWhenAll);
            nodeUnsetOwnerCommand.SetHandler(NodeCommand.UnsetOwner, groupsOption, allGroupsNoAliasesOption, nodenamesOption, allNodesNoAliasesOption);

            groupAddCommand.Aliases.Add("a");
            groupAddCommand.Arguments.Add(groupsArgumentOneOrMore);
            groupAddCommand.Options.Add(ownerOption);
            groupAddCommand.SetHandler(GroupCommand.Add, groupsArgumentOneOrMore, ownerOption);

            groupEditCommand.Aliases.Add("e");
            groupEditCommand.Arguments.Add(groupsArgumentOneOrMore);
            groupEditCommand.Options.Add(ownerOption);
            groupEditCommand.Options.Add(unsetOwnerOption);
            groupEditCommand.Validators.Add(Validators.ValidateOwnerOptions);
            groupEditCommand.SetHandler(GroupCommand.Edit, groupsArgumentOneOrMore, ownerOption, unsetOwnerOption);

            groupRenameCommand.Arguments.Add(oldNameArgument);
            groupRenameCommand.Arguments.Add(newNameArgument);
            groupRenameCommand.SetHandler(GroupCommand.Rename, oldNameArgument, newNameArgument);

            groupRemoveCommand.Aliases.Add("rm");
            groupRemoveCommand.Aliases.Add("del");
            groupRemoveCommand.Aliases.Add("delete");
            groupRemoveCommand.Arguments.Add(groupsArgumentOneOrMore);
            groupRemoveCommand.SetHandler(GroupCommand.Remove, groupsArgumentOneOrMore);

            groupListCommand.Aliases.Add("l");
            groupListCommand.Aliases.Add("ls");
            groupListCommand.Options.Add(namesOnlyOption);
            groupListCommand.Options.Add(onePerLineOption);
            groupListCommand.SetHandler(GroupCommand.List, namesOnlyOption, onePerLineOption);

            groupAddUsersCommand.Aliases.Add("au");
            groupAddUsersCommand.Arguments.Add(groupArgument);
            groupAddUsersCommand.Arguments.Add(usernamesArgumentZeroOrMore);
            groupAddUsersCommand.Options.Add(allUsersOption);
            groupAddUsersCommand.Validators.Add(Validators.EnforceZeroUsernamesWhenAll);
            groupAddUsersCommand.SetHandler(GroupCommand.AddUsers, groupArgument, usernamesArgumentZeroOrMore, allUsersOption);

            groupRemoveUsersCommand.Aliases.Add("ru");
            groupRemoveUsersCommand.Arguments.Add(groupArgument);
            groupRemoveUsersCommand.Arguments.Add(usernamesArgumentZeroOrMore);
            groupRemoveUsersCommand.Options.Add(allUsersOption);
            groupRemoveUsersCommand.Validators.Add(Validators.EnforceZeroUsernamesWhenAll);
            groupRemoveUsersCommand.SetHandler(GroupCommand.RemoveUsers, groupArgument, usernamesArgumentZeroOrMore, allUsersOption);

            groupListUsersCommand.Aliases.Add("lc");
            groupListUsersCommand.Aliases.Add("lm");
            groupListUsersCommand.Aliases.Add("lu");
            groupListUsersCommand.Aliases.Add("list-credentials");
            groupListUsersCommand.Aliases.Add("list-members");
            groupListUsersCommand.Arguments.Add(groupArgument);
            groupListUsersCommand.SetHandler(GroupCommand.ListUsers, groupArgument);

            groupAddCredentialCommand.Aliases.Add("ac");
            groupAddCredentialCommand.Arguments.Add(groupArgument);
            groupAddCredentialCommand.Arguments.Add(methodArgument);
            groupAddCredentialCommand.Arguments.Add(passwordArgument);
            groupAddCredentialCommand.Arguments.Add(usernamesArgumentZeroOrMore);
            groupAddCredentialCommand.Options.Add(allUsersOption);
            groupAddCredentialCommand.Validators.Add(Validators.EnforceZeroUsernamesWhenAll);
            groupAddCredentialCommand.SetHandler(GroupCommand.AddCredential, groupArgument, methodArgument, passwordArgument, usernamesArgumentZeroOrMore, allUsersOption);

            groupRemoveCredentialsCommand.Aliases.Add("rc");
            groupRemoveCredentialsCommand.Arguments.Add(groupArgument);
            groupRemoveCredentialsCommand.Arguments.Add(usernamesArgumentZeroOrMore);
            groupRemoveCredentialsCommand.Options.Add(allUsersOption);
            groupRemoveCredentialsCommand.Validators.Add(Validators.EnforceZeroUsernamesWhenAll);
            groupRemoveCredentialsCommand.SetHandler(GroupCommand.RemoveCredentials, groupArgument, usernamesArgumentZeroOrMore, allUsersOption);

            groupGetDataUsageCommand.Aliases.Add("data");
            groupGetDataUsageCommand.Arguments.Add(groupArgument);
            groupGetDataUsageCommand.Options.Add(sortByOption);
            groupGetDataUsageCommand.SetHandler(GroupCommand.GetDataUsage, groupArgument, sortByOption);

            groupGetDataLimitCommand.Aliases.Add("gl");
            groupGetDataLimitCommand.Aliases.Add("gdl");
            groupGetDataLimitCommand.Aliases.Add("limit");
            groupGetDataLimitCommand.Aliases.Add("get-limit");
            groupGetDataLimitCommand.Arguments.Add(groupArgument);
            groupGetDataLimitCommand.SetHandler(GroupCommand.GetDataLimit, groupArgument);

            groupSetDataLimitCommand.Aliases.Add("sl");
            groupSetDataLimitCommand.Aliases.Add("sdl");
            groupSetDataLimitCommand.Aliases.Add("set-limit");
            groupSetDataLimitCommand.Arguments.Add(groupsArgumentOneOrMore);
            groupSetDataLimitCommand.Options.Add(globalDataLimitOption);
            groupSetDataLimitCommand.Options.Add(perUserDataLimitOption);
            groupSetDataLimitCommand.Options.Add(usernamesOption);
            groupSetDataLimitCommand.Validators.Add(GroupCommand.ValidateSetDataLimit);
            groupSetDataLimitCommand.SetHandler(GroupCommand.SetDataLimit, groupsArgumentOneOrMore, globalDataLimitOption, perUserDataLimitOption, usernamesOption);

            onlineConfigGenerateCommand.Aliases.Add("g");
            onlineConfigGenerateCommand.Aliases.Add("gen");
            onlineConfigGenerateCommand.Arguments.Add(usernamesArgumentZeroOrMore);
            onlineConfigGenerateCommand.SetHandler(OnlineConfigCommand.Generate, usernamesArgumentZeroOrMore);

            onlineConfigGetLinksCommand.Aliases.Add("l");
            onlineConfigGetLinksCommand.Aliases.Add("link");
            onlineConfigGetLinksCommand.Aliases.Add("links");
            onlineConfigGetLinksCommand.Aliases.Add("token");
            onlineConfigGetLinksCommand.Aliases.Add("tokens");
            onlineConfigGetLinksCommand.Aliases.Add("url");
            onlineConfigGetLinksCommand.Aliases.Add("urls");
            onlineConfigGetLinksCommand.Arguments.Add(usernamesArgumentZeroOrMore);
            onlineConfigGetLinksCommand.SetHandler(OnlineConfigCommand.GetLinks, usernamesArgumentZeroOrMore);

            onlineConfigCleanCommand.Aliases.Add("c");
            onlineConfigCleanCommand.Aliases.Add("clear");
            onlineConfigCleanCommand.Arguments.Add(usernamesArgumentZeroOrMore);
            onlineConfigCleanCommand.Options.Add(allUsersOption);
            onlineConfigCleanCommand.Validators.Add(Validators.EnforceZeroUsernamesWhenAll);
            onlineConfigCleanCommand.SetHandler(OnlineConfigCommand.Clean, usernamesArgumentZeroOrMore, allUsersOption);

            outlineServerAddCommand.Aliases.Add("a");
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

            outlineServerRemoveCommand.Aliases.Add("rm");
            outlineServerRemoveCommand.Arguments.Add(groupsArgumentOneOrMore);
            outlineServerRemoveCommand.Options.Add(removeCredsOption);
            outlineServerRemoveCommand.SetHandler(OutlineServerCommand.Remove, groupsArgumentOneOrMore, removeCredsOption);

            outlineServerPullCommand.Aliases.Add("update");
            outlineServerPullCommand.Arguments.Add(groupsArgumentZeroOrMore);
            outlineServerPullCommand.Options.Add(noSyncOption);
            outlineServerPullCommand.SetHandler(OutlineServerCommand.Pull, groupsArgumentZeroOrMore, noSyncOption);

            outlineServerDeployCommand.Arguments.Add(groupsArgumentZeroOrMore);
            outlineServerDeployCommand.SetHandler(OutlineServerCommand.Deploy, groupsArgumentZeroOrMore);

            outlineServerRotatePasswordCommand.Aliases.Add("rotate");
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
