using System;
using System.CommandLine;
using System.CommandLine.Invocation;
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
            var groupListUsersCommand = new Command("list-users", "List users in the group.");
            var groupAddCredentialCommand = new Command("add-credential", "Add credential to selected users in the group.");
            var groupRemoveCredentialsCommand = new Command("remove-credentials", "Remove credentials from selected users in the group.");
            var groupListCredentialsCommand = new Command("list-credentials", "List credentials in the group.");
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
                groupListCredentialsCommand,
                groupGetDataUsageCommand,
                groupGetDataLimitCommand,
                groupSetDataLimitCommand,
            };

            var onlineConfigGenerateCommand = new Command("generate", "[Legacy] Generate static SIP008 delivery JSON files for specified or all users.");
            var onlineConfigGetLinksCommand = new Command("get-links", "[Legacy] Get SIP008 static file delivery URLs for specified or all users.");
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

            var usernamesOption = new Option<string[]>("--usernames", "Target these specific users. If unspecified, target all users.");
            var nodenamesOption = new Option<string[]>("--nodenames", "Target these specific nodes. If unspecified, target all nodes.");
            var groupsOption = new Option<string[]>("--groups", "Target these specific groups. If unspecified, target all groups.");

            var hostOption = new Option<string>("--host", "Hostname of the node.");
            var portOption = new Option<int>("--port", Parsers.ParsePortNumber, false, "Port number of the node.");
            var pluginOption = new Option<string>("--plugin", "Plugin binary name of the node.");
            var pluginOptsOption = new Option<string>("--plugin-opts", "Plugin options of the node.");
            var unsetPluginOption = new Option<bool>("--unset-plugin", "Remove plugin and plugin options from the node.");

            var ownerOption = new Option<string>("--owner", "Set the owner.");
            var unsetOwnerOption = new Option<bool>("--unset-owner", "Unset the owner.");

            var forceOption = new Option<bool>(new string[] { "-f", "--force" }, "Forcibly overwrite existing settings.");

            var tagsOption = new Option<string[]>("--tags", "Tags that annotate the node. Will be deduplicated in a case-insensitive manner.");
            var addTagsOption = new Option<string[]>("--add-tags", "Tags to add to the node. Will be deduplicated in a case-insensitive manner.");
            var removeTagsOption = new Option<string[]>("--remove-tags", "Tags to remove from the node. Matched in a case-insensitive manner.");
            var clearTagsOption = new Option<bool>("--clear-tags", "Remove all tags from the node.");

            var methodOption = new Option<string>("--method", Parsers.ParseShadowsocksAEADMethod, false, "The encryption method. Use with --password.");
            var passwordOption = new Option<string>("--password", "The password. Use with --method.");
            var userinfoBase64urlOption = new Option<string>("--userinfo-base64url", "The userinfo (method + ':' + password) encoded in URL-safe base64. Do not specify with '--method' or '--password'.");

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

            var csvOutdirOption = new Option<string>("--csv-outdir", "Export as CSV to the specified directory.");

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
            userAddCommand.Handler = CommandHandler.Create<string[], CancellationToken>(UserCommand.Add);

            userRenameCommand.AddArgument(oldNameArgument);
            userRenameCommand.AddArgument(newNameArgument);
            userRenameCommand.Handler = CommandHandler.Create<string, string, CancellationToken>(UserCommand.Rename);

            userRemoveCommand.AddAlias("rm");
            userRemoveCommand.AddAlias("del");
            userRemoveCommand.AddAlias("delete");
            userRemoveCommand.AddArgument(usernamesArgumentOneOrMore);
            userRemoveCommand.Handler = CommandHandler.Create<string[], CancellationToken>(UserCommand.Remove);

            userListCommand.AddAlias("l");
            userListCommand.AddAlias("ls");
            userListCommand.AddOption(namesOnlyOption);
            userListCommand.AddOption(onePerLineOption);
            userListCommand.Handler = CommandHandler.Create<bool, bool, CancellationToken>(UserCommand.List);

            userJoinGroupsCommand.AddArgument(usernameArgument);
            userJoinGroupsCommand.AddArgument(groupsArgumentZeroOrMore);
            userJoinGroupsCommand.AddOption(allGroupsOption);
            userJoinGroupsCommand.AddValidator(Validators.EnforceZeroGroupsWhenAll);
            userJoinGroupsCommand.Handler = CommandHandler.Create<string, string[], bool, CancellationToken>(UserCommand.JoinGroups);

            userLeaveGroupsCommand.AddArgument(usernameArgument);
            userLeaveGroupsCommand.AddArgument(groupsArgumentZeroOrMore);
            userLeaveGroupsCommand.AddOption(allGroupsOption);
            userLeaveGroupsCommand.AddValidator(Validators.EnforceZeroGroupsWhenAll);
            userLeaveGroupsCommand.Handler = CommandHandler.Create<string, string[], bool, CancellationToken>(UserCommand.LeaveGroups);

            userAddCredentialCommand.AddAlias("ac");
            userAddCredentialCommand.AddArgument(usernameArgument);
            userAddCredentialCommand.AddArgument(groupsArgumentZeroOrMore);
            userAddCredentialCommand.AddOption(methodOption);
            userAddCredentialCommand.AddOption(passwordOption);
            userAddCredentialCommand.AddOption(userinfoBase64urlOption);
            userAddCredentialCommand.AddOption(allGroupsOption);
            userAddCredentialCommand.AddValidator(Validators.EnforceZeroGroupsWhenAll);
            userAddCredentialCommand.AddValidator(Validators.ValidateAddCredential);
            userAddCredentialCommand.Handler = CommandHandler.Create<string, string[], string, string, string, bool, CancellationToken>(UserCommand.AddCredential);

            userRemoveCredentialsCommand.AddAlias("rc");
            userRemoveCredentialsCommand.AddArgument(usernameArgument);
            userRemoveCredentialsCommand.AddArgument(groupsArgumentZeroOrMore);
            userRemoveCredentialsCommand.AddOption(allGroupsOption);
            userRemoveCredentialsCommand.AddValidator(Validators.EnforceZeroGroupsWhenAll);
            userRemoveCredentialsCommand.Handler = CommandHandler.Create<string, string[], bool, CancellationToken>(UserCommand.RemoveCredentials);

            userListCredentialsCommand.AddAlias("lc");
            userListCredentialsCommand.AddOption(usernamesOption);
            userListCredentialsCommand.AddOption(groupsOption);
            userListCredentialsCommand.Handler = CommandHandler.Create<string[], string[], CancellationToken>(UserCommand.ListCredentials);

            userGetSSLinksCommand.AddAlias("ss");
            userGetSSLinksCommand.AddArgument(usernameArgument);
            userGetSSLinksCommand.AddOption(groupsOption);
            userGetSSLinksCommand.Handler = CommandHandler.Create<string, string[], CancellationToken>(UserCommand.GetSSLinks);

            userGetDataUsageCommand.AddAlias("data");
            userGetDataUsageCommand.AddArgument(usernameArgument);
            userGetDataUsageCommand.AddOption(sortByOption);
            userGetDataUsageCommand.Handler = CommandHandler.Create<string, SortBy?, CancellationToken>(UserCommand.GetDataUsage);

            userGetDataLimitCommand.AddAlias("gl");
            userGetDataLimitCommand.AddAlias("gdl");
            userGetDataLimitCommand.AddAlias("limit");
            userGetDataLimitCommand.AddAlias("get-limit");
            userGetDataLimitCommand.AddArgument(usernameArgument);
            userGetDataLimitCommand.Handler = CommandHandler.Create<string, CancellationToken>(UserCommand.GetDataLimit);

            userSetDataLimitCommand.AddAlias("sl");
            userSetDataLimitCommand.AddAlias("sdl");
            userSetDataLimitCommand.AddAlias("set-limit");
            userSetDataLimitCommand.AddArgument(usernamesArgumentOneOrMore);
            userSetDataLimitCommand.AddOption(globalDataLimitOption);
            userSetDataLimitCommand.AddOption(perGroupDataLimitOption);
            userSetDataLimitCommand.AddOption(groupsOption);
            userSetDataLimitCommand.AddValidator(UserCommand.ValidateSetDataLimit);
            userSetDataLimitCommand.Handler = CommandHandler.Create<string[], ulong?, ulong?, string[], CancellationToken>(UserCommand.SetDataLimit);

            userOwnGroupsCommand.AddAlias("og");
            userOwnGroupsCommand.AddArgument(usernameArgument);
            userOwnGroupsCommand.AddArgument(groupsArgumentZeroOrMore);
            userOwnGroupsCommand.AddOption(allGroupsOption);
            userOwnGroupsCommand.AddOption(forceOption);
            userOwnGroupsCommand.AddValidator(Validators.EnforceZeroGroupsWhenAll);
            userOwnGroupsCommand.Handler = CommandHandler.Create<string, string[], bool, bool, CancellationToken>(UserCommand.OwnGroups);

            userDisownGroupsCommand.AddAlias("dg");
            userDisownGroupsCommand.AddArgument(usernameArgument);
            userDisownGroupsCommand.AddArgument(groupsArgumentZeroOrMore);
            userDisownGroupsCommand.AddOption(allGroupsOption);
            userDisownGroupsCommand.AddValidator(Validators.EnforceZeroGroupsWhenAll);
            userDisownGroupsCommand.Handler = CommandHandler.Create<string, string[], bool, CancellationToken>(UserCommand.DisownGroups);

            userOwnNodesCommand.AddAlias("on");
            userOwnNodesCommand.AddArgument(usernameArgument);
            userOwnNodesCommand.AddOption(groupsOption);
            userOwnNodesCommand.AddOption(allGroupsNoAliasesOption);
            userOwnNodesCommand.AddOption(nodenamesOption);
            userOwnNodesCommand.AddOption(allNodesNoAliasesOption);
            userOwnNodesCommand.AddOption(forceOption);
            userOwnNodesCommand.AddValidator(Validators.EnforceZeroGroupsWhenAll);
            userOwnNodesCommand.AddValidator(Validators.EnforceZeroNodenamesWhenAll);
            userOwnNodesCommand.Handler = CommandHandler.Create<string, string[], bool, string[], bool, bool, CancellationToken>(UserCommand.OwnNodes);

            userDisownNodesCommand.AddAlias("dn");
            userDisownNodesCommand.AddArgument(usernameArgument);
            userDisownNodesCommand.AddOption(groupsOption);
            userDisownNodesCommand.AddOption(allGroupsNoAliasesOption);
            userDisownNodesCommand.AddOption(nodenamesOption);
            userDisownNodesCommand.AddOption(allNodesNoAliasesOption);
            userDisownNodesCommand.AddValidator(Validators.EnforceZeroGroupsWhenAll);
            userDisownNodesCommand.AddValidator(Validators.EnforceZeroNodenamesWhenAll);
            userDisownNodesCommand.Handler = CommandHandler.Create<string, string[], bool, string[], bool, CancellationToken>(UserCommand.DisownNodes);

            userListOwnedGroupsCommand.AddAlias("log");
            userListOwnedGroupsCommand.AddArgument(usernameArgument);
            userListOwnedGroupsCommand.Handler = CommandHandler.Create<string, CancellationToken>(UserCommand.ListOwnedGroups);

            userListOwnedNodesCmmand.AddAlias("lon");
            userListOwnedNodesCmmand.AddArgument(usernameArgument);
            userListOwnedNodesCmmand.AddArgument(groupsArgumentZeroOrMore);
            userListOwnedNodesCmmand.Handler = CommandHandler.Create<string, string[], CancellationToken>(UserCommand.ListOwnedNodes);

            nodeAddCommand.AddAlias("a");
            nodeAddCommand.AddArgument(groupArgument);
            nodeAddCommand.AddArgument(nodenameArgument);
            nodeAddCommand.AddArgument(hostArgument);
            nodeAddCommand.AddArgument(portArgument);
            nodeAddCommand.AddOption(pluginOption);
            nodeAddCommand.AddOption(pluginOptsOption);
            nodeAddCommand.AddOption(ownerOption);
            nodeAddCommand.AddOption(tagsOption);
            nodeAddCommand.AddValidator(NodeCommand.ValidateNodePlugin);
            nodeAddCommand.Handler = CommandHandler.Create<string, string, string, int, string, string, string, string[], CancellationToken>(NodeCommand.Add);

            nodeEditCommand.AddAlias("e");
            nodeEditCommand.AddArgument(groupArgument);
            nodeEditCommand.AddArgument(nodenameArgument);
            nodeEditCommand.AddOption(hostOption);
            nodeEditCommand.AddOption(portOption);
            nodeEditCommand.AddOption(pluginOption);
            nodeEditCommand.AddOption(pluginOptsOption);
            nodeEditCommand.AddOption(unsetPluginOption);
            nodeEditCommand.AddOption(ownerOption);
            nodeEditCommand.AddOption(unsetOwnerOption);
            nodeEditCommand.AddOption(clearTagsOption);
            nodeEditCommand.AddOption(addTagsOption);
            nodeEditCommand.AddOption(removeTagsOption);
            nodeEditCommand.AddValidator(NodeCommand.ValidateNodePlugin);
            nodeEditCommand.AddValidator(Validators.ValidateOwnerOptions);
            nodeEditCommand.Handler = CommandHandler.Create<string, string, string, int, string, string, bool, string, bool, bool, string[], string[], CancellationToken>(NodeCommand.Edit);

            nodeRenameCommand.AddArgument(groupArgument);
            nodeRenameCommand.AddArgument(oldNameArgument);
            nodeRenameCommand.AddArgument(newNameArgument);
            nodeRenameCommand.Handler = CommandHandler.Create<string, string, string, CancellationToken>(NodeCommand.Rename);

            nodeRemoveCommand.AddAlias("rm");
            nodeRemoveCommand.AddAlias("del");
            nodeRemoveCommand.AddAlias("delete");
            nodeRemoveCommand.AddArgument(groupArgument);
            nodeRemoveCommand.AddArgument(nodenamesArgumentOneOrMore);
            nodeRemoveCommand.Handler = CommandHandler.Create<string, string[], CancellationToken>(NodeCommand.Remove);

            nodeListCommand.AddAlias("l");
            nodeListCommand.AddAlias("ls");
            nodeListCommand.AddArgument(groupsArgumentZeroOrMore);
            nodeListCommand.AddOption(namesOnlyOption);
            nodeListCommand.AddOption(onePerLineOption);
            nodeListCommand.Handler = CommandHandler.Create<string[], bool, bool, CancellationToken>(NodeCommand.List);

            nodeListAnnotationsCommand.AddAlias("la");
            nodeListAnnotationsCommand.AddAlias("lsa");
            nodeListAnnotationsCommand.AddArgument(groupsArgumentZeroOrMore);
            nodeListAnnotationsCommand.AddOption(onePerLineOption);
            nodeListAnnotationsCommand.Handler = CommandHandler.Create<string[], bool, CancellationToken>(NodeCommand.ListAnnotations);

            nodeActivateCommand.AddAlias("enable");
            nodeActivateCommand.AddAlias("unhide");
            nodeActivateCommand.AddArgument(groupArgument);
            nodeActivateCommand.AddArgument(nodenamesArgumentZeroOrMore);
            nodeActivateCommand.AddOption(allNodesOption);
            nodeActivateCommand.AddValidator(Validators.EnforceZeroNodenamesWhenAll);
            nodeActivateCommand.Handler = CommandHandler.Create<string, string[], bool, CancellationToken>(NodeCommand.Activate);

            nodeDeactivateCommand.AddAlias("disable");
            nodeDeactivateCommand.AddAlias("hide");
            nodeDeactivateCommand.AddArgument(groupArgument);
            nodeDeactivateCommand.AddArgument(nodenamesArgumentZeroOrMore);
            nodeDeactivateCommand.AddOption(allNodesOption);
            nodeDeactivateCommand.AddValidator(Validators.EnforceZeroNodenamesWhenAll);
            nodeDeactivateCommand.Handler = CommandHandler.Create<string, string[], bool, CancellationToken>(NodeCommand.Deactivate);

            nodeAddTagsCommand.AddAlias("at");
            nodeAddTagsCommand.AddArgument(tagsArgument);
            nodeAddTagsCommand.AddOption(groupsOption);
            nodeAddTagsCommand.AddOption(allGroupsNoAliasesOption);
            nodeAddTagsCommand.AddOption(nodenamesOption);
            nodeAddTagsCommand.AddOption(allNodesNoAliasesOption);
            nodeAddTagsCommand.AddValidator(Validators.EnforceZeroGroupsWhenAll);
            nodeAddTagsCommand.AddValidator(Validators.EnforceZeroNodenamesWhenAll);
            nodeAddTagsCommand.Handler = CommandHandler.Create<string[], string[], bool, string[], bool, CancellationToken>(NodeCommand.AddTags);

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
            nodeEditTagsCommand.Handler = CommandHandler.Create<string[], bool, string[], bool, bool, string[], string[], CancellationToken>(NodeCommand.EditTags);

            nodeRemoveTagsCommand.AddAlias("rt");
            nodeRemoveTagsCommand.AddArgument(tagsArgument);
            nodeRemoveTagsCommand.AddOption(groupsOption);
            nodeRemoveTagsCommand.AddOption(allGroupsNoAliasesOption);
            nodeRemoveTagsCommand.AddOption(nodenamesOption);
            nodeRemoveTagsCommand.AddOption(allNodesNoAliasesOption);
            nodeRemoveTagsCommand.AddValidator(Validators.EnforceZeroGroupsWhenAll);
            nodeRemoveTagsCommand.AddValidator(Validators.EnforceZeroNodenamesWhenAll);
            nodeRemoveTagsCommand.Handler = CommandHandler.Create<string[], string[], bool, string[], bool, CancellationToken>(NodeCommand.RemoveTags);

            nodeClearTagsCommand.AddAlias("ct");
            nodeClearTagsCommand.AddOption(groupsOption);
            nodeClearTagsCommand.AddOption(allGroupsNoAliasesOption);
            nodeClearTagsCommand.AddOption(nodenamesOption);
            nodeClearTagsCommand.AddOption(allNodesNoAliasesOption);
            nodeClearTagsCommand.AddValidator(Validators.EnforceZeroGroupsWhenAll);
            nodeClearTagsCommand.AddValidator(Validators.EnforceZeroNodenamesWhenAll);
            nodeClearTagsCommand.Handler = CommandHandler.Create<string[], bool, string[], bool, CancellationToken>(NodeCommand.ClearTags);

            nodeSetOwnerCommand.AddAlias("so");
            nodeSetOwnerCommand.AddArgument(ownerArgument);
            nodeSetOwnerCommand.AddOption(groupsOption);
            nodeSetOwnerCommand.AddOption(allGroupsNoAliasesOption);
            nodeSetOwnerCommand.AddOption(nodenamesOption);
            nodeSetOwnerCommand.AddOption(allNodesNoAliasesOption);
            nodeSetOwnerCommand.AddValidator(Validators.EnforceZeroGroupsWhenAll);
            nodeSetOwnerCommand.AddValidator(Validators.EnforceZeroNodenamesWhenAll);
            nodeSetOwnerCommand.Handler = CommandHandler.Create<string, string[], bool, string[], bool, CancellationToken>(NodeCommand.SetOwner);

            nodeUnsetOwnerCommand.AddAlias("uo");
            nodeUnsetOwnerCommand.AddOption(groupsOption);
            nodeUnsetOwnerCommand.AddOption(allGroupsNoAliasesOption);
            nodeUnsetOwnerCommand.AddOption(nodenamesOption);
            nodeUnsetOwnerCommand.AddOption(allNodesNoAliasesOption);
            nodeUnsetOwnerCommand.AddValidator(Validators.EnforceZeroGroupsWhenAll);
            nodeUnsetOwnerCommand.AddValidator(Validators.EnforceZeroNodenamesWhenAll);
            nodeUnsetOwnerCommand.Handler = CommandHandler.Create<string[], bool, string[], bool, CancellationToken>(NodeCommand.UnsetOwner);

            groupAddCommand.AddAlias("a");
            groupAddCommand.AddArgument(groupsArgumentOneOrMore);
            groupAddCommand.AddOption(ownerOption);
            groupAddCommand.Handler = CommandHandler.Create<string[], string, CancellationToken>(GroupCommand.Add);

            groupEditCommand.AddAlias("e");
            groupEditCommand.AddArgument(groupsArgumentOneOrMore);
            groupEditCommand.AddOption(ownerOption);
            groupEditCommand.AddOption(unsetOwnerOption);
            groupEditCommand.AddValidator(Validators.ValidateOwnerOptions);
            groupEditCommand.Handler = CommandHandler.Create<string[], string, bool, CancellationToken>(GroupCommand.Edit);

            groupRenameCommand.AddArgument(oldNameArgument);
            groupRenameCommand.AddArgument(newNameArgument);
            groupRenameCommand.Handler = CommandHandler.Create<string, string, CancellationToken>(GroupCommand.Rename);

            groupRemoveCommand.AddAlias("rm");
            groupRemoveCommand.AddAlias("del");
            groupRemoveCommand.AddAlias("delete");
            groupRemoveCommand.AddArgument(groupsArgumentOneOrMore);
            groupRemoveCommand.Handler = CommandHandler.Create<string[], CancellationToken>(GroupCommand.Remove);

            groupListCommand.AddAlias("l");
            groupListCommand.AddAlias("ls");
            groupListCommand.AddOption(namesOnlyOption);
            groupListCommand.AddOption(onePerLineOption);
            groupListCommand.Handler = CommandHandler.Create<bool, bool, CancellationToken>(GroupCommand.List);

            groupAddUsersCommand.AddAlias("au");
            groupAddUsersCommand.AddArgument(groupArgument);
            groupAddUsersCommand.AddArgument(usernamesArgumentZeroOrMore);
            groupAddUsersCommand.AddOption(allUsersOption);
            groupAddUsersCommand.AddValidator(Validators.EnforceZeroUsernamesWhenAll);
            groupAddUsersCommand.Handler = CommandHandler.Create<string, string[], bool, CancellationToken>(GroupCommand.AddUsers);

            groupRemoveUsersCommand.AddAlias("ru");
            groupRemoveUsersCommand.AddArgument(groupArgument);
            groupRemoveUsersCommand.AddArgument(usernamesArgumentZeroOrMore);
            groupRemoveUsersCommand.AddOption(allUsersOption);
            groupRemoveUsersCommand.AddValidator(Validators.EnforceZeroUsernamesWhenAll);
            groupRemoveUsersCommand.Handler = CommandHandler.Create<string, string[], bool, CancellationToken>(GroupCommand.RemoveUsers);

            groupListUsersCommand.AddAlias("lu");
            groupListUsersCommand.AddArgument(groupArgument);
            groupListUsersCommand.Handler = CommandHandler.Create<string, CancellationToken>(GroupCommand.ListUsers);

            groupAddCredentialCommand.AddAlias("ac");
            groupAddCredentialCommand.AddArgument(groupArgument);
            groupAddCredentialCommand.AddArgument(usernamesArgumentZeroOrMore);
            groupAddCredentialCommand.AddOption(methodOption);
            groupAddCredentialCommand.AddOption(passwordOption);
            groupAddCredentialCommand.AddOption(userinfoBase64urlOption);
            groupAddCredentialCommand.AddOption(allUsersOption);
            groupAddCredentialCommand.AddValidator(Validators.EnforceZeroUsernamesWhenAll);
            groupAddCredentialCommand.AddValidator(Validators.ValidateAddCredential);
            groupAddCredentialCommand.Handler = CommandHandler.Create<string, string[], string, string, string, bool, CancellationToken>(GroupCommand.AddCredential);

            groupRemoveCredentialsCommand.AddAlias("rc");
            groupRemoveCredentialsCommand.AddArgument(groupArgument);
            groupRemoveCredentialsCommand.AddArgument(usernamesArgumentZeroOrMore);
            groupRemoveCredentialsCommand.AddOption(allUsersOption);
            groupRemoveCredentialsCommand.AddValidator(Validators.EnforceZeroUsernamesWhenAll);
            groupRemoveCredentialsCommand.Handler = CommandHandler.Create<string, string[], bool, CancellationToken>(GroupCommand.RemoveCredentials);

            groupListCredentialsCommand.AddAlias("lc");
            groupListCredentialsCommand.AddArgument(groupArgument);
            groupListCredentialsCommand.Handler = CommandHandler.Create<string, CancellationToken>(GroupCommand.ListCredentials);

            groupGetDataUsageCommand.AddAlias("data");
            groupGetDataUsageCommand.AddArgument(groupArgument);
            groupGetDataUsageCommand.AddOption(sortByOption);
            groupGetDataUsageCommand.Handler = CommandHandler.Create<string, SortBy?, CancellationToken>(GroupCommand.GetDataUsage);

            groupGetDataLimitCommand.AddAlias("gl");
            groupGetDataLimitCommand.AddAlias("gdl");
            groupGetDataLimitCommand.AddAlias("limit");
            groupGetDataLimitCommand.AddAlias("get-limit");
            groupGetDataLimitCommand.AddArgument(groupArgument);
            groupGetDataLimitCommand.Handler = CommandHandler.Create<string, CancellationToken>(GroupCommand.GetDataLimit);

            groupSetDataLimitCommand.AddAlias("sl");
            groupSetDataLimitCommand.AddAlias("sdl");
            groupSetDataLimitCommand.AddAlias("set-limit");
            groupSetDataLimitCommand.AddArgument(groupsArgumentOneOrMore);
            groupSetDataLimitCommand.AddOption(globalDataLimitOption);
            groupSetDataLimitCommand.AddOption(perUserDataLimitOption);
            groupSetDataLimitCommand.AddOption(usernamesOption);
            groupSetDataLimitCommand.AddValidator(GroupCommand.ValidateSetDataLimit);
            groupSetDataLimitCommand.Handler = CommandHandler.Create<string[], ulong?, ulong?, string[], CancellationToken>(GroupCommand.SetDataLimit);

            onlineConfigGenerateCommand.AddAlias("g");
            onlineConfigGenerateCommand.AddAlias("gen");
            onlineConfigGenerateCommand.AddArgument(usernamesArgumentZeroOrMore);
            onlineConfigGenerateCommand.Handler = CommandHandler.Create<string[], CancellationToken>(OnlineConfigCommand.Generate);

            onlineConfigGetLinksCommand.AddAlias("l");
            onlineConfigGetLinksCommand.AddAlias("link");
            onlineConfigGetLinksCommand.AddAlias("links");
            onlineConfigGetLinksCommand.AddAlias("url");
            onlineConfigGetLinksCommand.AddAlias("urls");
            onlineConfigGetLinksCommand.AddArgument(usernamesArgumentZeroOrMore);
            onlineConfigGetLinksCommand.Handler = CommandHandler.Create<string[], CancellationToken>(OnlineConfigCommand.GetLinks);

            onlineConfigCleanCommand.AddAlias("c");
            onlineConfigCleanCommand.AddAlias("clear");
            onlineConfigCleanCommand.AddArgument(usernamesArgumentZeroOrMore);
            onlineConfigCleanCommand.AddOption(allUsersOption);
            onlineConfigCleanCommand.AddValidator(Validators.EnforceZeroUsernamesWhenAll);
            onlineConfigCleanCommand.Handler = CommandHandler.Create<string[], CancellationToken>(OnlineConfigCommand.Clean);

            outlineServerAddCommand.AddAlias("a");
            outlineServerAddCommand.AddArgument(groupArgument);
            outlineServerAddCommand.AddArgument(new Argument<string>("apiKey", "The Outline server API key."));
            outlineServerAddCommand.Handler = CommandHandler.Create<string, string, CancellationToken>(OutlineServerCommand.Add);

            outlineServerGetCommand.AddArgument(groupArgument);
            outlineServerGetCommand.Handler = CommandHandler.Create<string, CancellationToken>(OutlineServerCommand.Get);

            outlineServerSetCommand.AddArgument(groupArgument);
            outlineServerSetCommand.AddOption(new Option<string>("--name", "Name of the Outline server."));
            outlineServerSetCommand.AddOption(new Option<string>("--hostname", "Hostname of the Outline server."));
            outlineServerSetCommand.AddOption(new Option<int?>("--port", "Port number for new access keys on the Outline server."));
            outlineServerSetCommand.AddOption(new Option<bool?>("--metrics", "Enable or disable telemetry on the Outline server."));
            outlineServerSetCommand.AddOption(new Option<string>("--default-user", "The default user for Outline server's default access key (id: 0)."));
            outlineServerSetCommand.Handler = CommandHandler.Create<string, string, string, int?, bool?, string, CancellationToken>(OutlineServerCommand.Set);

            outlineServerRemoveCommand.AddAlias("rm");
            outlineServerRemoveCommand.AddArgument(groupsArgumentOneOrMore);
            outlineServerRemoveCommand.AddOption(new Option<bool>("--remove-creds", "Remove credentials from all associated users."));
            outlineServerRemoveCommand.Handler = CommandHandler.Create<string[], bool, CancellationToken>(OutlineServerCommand.Remove);

            outlineServerPullCommand.AddAlias("update");
            outlineServerPullCommand.AddArgument(groupsArgumentZeroOrMore);
            outlineServerPullCommand.AddOption(new Option<bool>("--no-sync", "Do not update local user credential storage from retrieved access key list."));
            outlineServerPullCommand.Handler = CommandHandler.Create<string[], bool, CancellationToken>(OutlineServerCommand.Pull);

            outlineServerDeployCommand.AddArgument(groupsArgumentZeroOrMore);
            outlineServerDeployCommand.Handler = CommandHandler.Create<string[], CancellationToken>(OutlineServerCommand.Deploy);

            outlineServerRotatePasswordCommand.AddAlias("rotate");
            outlineServerRotatePasswordCommand.AddOption(usernamesOption);
            outlineServerRotatePasswordCommand.AddOption(groupsOption);
            outlineServerRotatePasswordCommand.AddValidator(OutlineServerCommand.ValidateRotatePassword);
            outlineServerRotatePasswordCommand.Handler = CommandHandler.Create<string[], string[], CancellationToken>(OutlineServerCommand.RotatePassword);

            reportCommand.AddOption(groupSortByOption);
            reportCommand.AddOption(userSortByOption);
            reportCommand.AddOption(csvOutdirOption);
            reportCommand.Handler = CommandHandler.Create<SortBy?, SortBy?, string, CancellationToken>(ReportCommand.Generate);

            settingsGetCommand.Handler = CommandHandler.Create<CancellationToken>(SettingsCommand.Get);

            settingsSetCommand.AddOption(new Option<SortBy?>("--user-data-usage-default-sort-by", "The default sort rule for user data usage report."));
            settingsSetCommand.AddOption(new Option<SortBy?>("--group-data-usage-default-sort-by", "The default sort rule for group data usage report."));
            settingsSetCommand.AddOption(new Option<bool?>("--online-config-sort-by-name", "Whether online config should sort servers by name."));
            settingsSetCommand.AddOption(new Option<bool?>("--online-config-deliver-by-group", "Whether the legacy SIP008 online config static file generator should generate per-group SIP008 delivery JSON in addition to the single JSON that contains all associated servers of the user."));
            settingsSetCommand.AddOption(new Option<bool?>("--online-config-clean-on-user-removal", "Whether the user's generated static online config files should be removed when the user is being removed."));
            settingsSetCommand.AddOption(new Option<string>("--online-config-output-directory", "Legacy SIP008 online config static file generator output directory. No trailing slashes allowed."));
            settingsSetCommand.AddOption(new Option<string>("--online-config-delivery-root-uri", "URL base for the SIP008 static file delivery links. No trailing slashes allowed."));
            settingsSetCommand.AddOption(new Option<bool?>("--outline-server-apply-default-user-on-association", "Whether to apply the global default user when associating with Outline servers."));
            settingsSetCommand.AddOption(new Option<bool?>("--outline-server-apply-data-limit-on-association", "Whether to apply the group's per-user data limit when associating with Outline servers."));
            settingsSetCommand.AddOption(new Option<string>("--outline-server-global-default-user", "The global setting for Outline server's default access key's user."));
            settingsSetCommand.Handler = CommandHandler.Create<SortBy?, SortBy?, bool?, bool?, bool?, string, string, bool?, bool?, string, CancellationToken>(SettingsCommand.Set);

            interactiveCommand.Handler = CommandHandler.Create(
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

            serviceCommand.AddOption(new Option<int>("--interval", () => 3600, "The interval between each scheduled run in seconds."));
            serviceCommand.AddOption(new Option<bool>("--pull-outline-server", "Pull from Outline servers for updates of server information, access keys, data usage."));
            serviceCommand.AddOption(new Option<bool>("--deploy-outline-server", "Deploy local configurations to Outline servers."));
            serviceCommand.AddOption(new Option<bool>("--generate-online-config", "Generate online config."));
            serviceCommand.AddOption(new Option<bool>("--regenerate-online-config", "Clean and regenerate online config."));
            serviceCommand.AddValidator(ServiceCommand.ValidateRun);
            serviceCommand.Handler = CommandHandler.Create<int, bool, bool, bool, bool, CancellationToken>(ServiceCommand.Run);

            Console.OutputEncoding = Encoding.UTF8;
            return rootCommand.InvokeAsync(args);
        }
    }
}
