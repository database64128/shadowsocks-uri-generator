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
            var loadUsersTask = Users.LoadUsersAsync();
            var loadNodesTask = Nodes.LoadNodesAsync();
            var loadSettingsTask = Settings.LoadSettingsAsync();

            var userAddCommand = new Command("add", "Add users.");
            var userRenameCommand = new Command("rename", "Renames an existing user with a new name.");
            var userRemoveCommand = new Command("remove", "Remove users.");
            var userListCommand = new Command("list", "List all users.");
            var userJoinGroupsCommand = new Command("join", "Join groups.");
            var userLeaveGroupsCommand = new Command("leave", "Leave groups.");
            var userAddCredentialCommand = new Command("add-credential", "Add a credential associated with a group for the user.");
            var userRemoveCredentialsCommand = new Command("remove-credentials", "Remove the group's credential from the user.");
            var userListCredentialsCommand = new Command("list-credentials", "List user-group credentials.");
            var userGetSSLinksCommand = new Command("get-ss-links", "Get the user's Shadowsocks URLs.");
            var userGetDataUsageCommand = new Command("get-data-usage", "Get the user's data usage records.");
            var userSetDataLimitCommand = new Command("set-data-limit", "Set a data limit for specified users and/or groups.");

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
                userSetDataLimitCommand,
            };

            var nodeAddCommand = new Command("add", "Add a node to a group.");
            var nodeRenameCommand = new Command("rename", "Rename an existing node with a new name.");
            var nodeActivateCommand = new Command("activate", "Activate a deactivated node to include it in delivery.");
            var nodeDeactivateCommand = new Command("deactivate", "Deactivate a node to exclude it from delivery.");
            var nodeRemoveCommand = new Command("remove", "Remove nodes from a group.");
            var nodeListCommand = new Command("list", "List nodes from the specified group or all groups.");

            var nodeCommand = new Command("node", "Manage nodes.")
            {
                nodeAddCommand,
                nodeRenameCommand,
                nodeActivateCommand,
                nodeDeactivateCommand,
                nodeRemoveCommand,
                nodeListCommand,
            };

            var groupAddCommand = new Command("add", "Add groups.");
            var groupRenameCommand = new Command("rename", "Renames an existing group with a new name.");
            var groupRemoveCommand = new Command("remove", "Remove groups and its nodes.");
            var groupListCommand = new Command("list", "List all groups.");
            var groupAddUsersCommand = new Command("add-users", "Add users to the group.");
            var groupRemoveUsersCommand = new Command("remove-users", "Remove users from the group.");
            var groupListUsersCommand = new Command("list-users", "List users in the group.");
            var groupGetDataUsageCommand = new Command("get-data-usage", "Get the group's data usage records.");
            var groupSetDataLimitCommand = new Command("set-data-limit", "Set a data limit for specified users and/or groups.");

            var groupCommand = new Command("group", "Manage groups.")
            {
                groupAddCommand,
                groupRenameCommand,
                groupRemoveCommand,
                groupListCommand,
                groupAddUsersCommand,
                groupRemoveUsersCommand,
                groupListUsersCommand,
                groupGetDataUsageCommand,
                groupSetDataLimitCommand,
            };

            var onlineConfigGenerateCommand = new Command("generate", "Generate SIP008-compliant online configuration delivery JSON files.");
            var onlineConfigGetLinksCommand = new Command("get-links", "Get the user's SIP008-compliant online configuration delivery URL.");
            var onlineConfigCleanCommand = new Command("clean", "Clean online configuration files for specified or all users.");

            var onlineConfigCommand = new Command("online-config", "Manage SIP008 online configuration.")
            {
                onlineConfigGenerateCommand,
                onlineConfigGetLinksCommand,
                onlineConfigCleanCommand,
            };

            var outlineServerAddCommand = new Command("add", "Associate an Outline server with a group.");
            var outlineServerGetCommand = new Command("get", "Get the associated Outline server's information.");
            var outlineServerSetCommand = new Command("set", "Change settings of the associated Outline server.");
            var outlineServerRemoveCommand = new Command("remove", "Remove the Outline server from the group.");
            var outlineServerPullCommand = new Command("pull", "Update server information, access keys, and metrics from the associated Outline server.");
            var outlineServerDeployCommand = new Command("deploy", "Deploy the group's configuration to the associated Outline server.");
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

            var rootCommand = new RootCommand("A light-weight command line automation tool for multi-user ss:// URL generation, SIP008 online configuration delivery, and Outline server deployment and management.")
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

            var usernamesArgument = new Argument<string[]>("usernames", "One or more usernames.");
            var nodenamesArgument = new Argument<string[]>("nodenames", "One or more node names.");
            var groupsArgument = new Argument<string[]>("groups", "One or more group names.");
            usernamesArgument.Arity = ArgumentArity.OneOrMore;
            nodenamesArgument.Arity = ArgumentArity.OneOrMore;
            groupsArgument.Arity = ArgumentArity.OneOrMore;

            var namesOnlyOption = new Option<bool>(new string[] { "-s", "--short", "--names-only" }, "Display names only, without a table.");
            var onePerLineOption = new Option<bool>(new string[] { "-1", "--one-per-line" }, "Display one name per line.");

            userCommand.AddAlias("u");
            nodeCommand.AddAlias("n");
            groupCommand.AddAlias("g");
            onlineConfigCommand.AddAlias("oc");
            onlineConfigCommand.AddAlias("online");
            onlineConfigCommand.AddAlias("sip008");
            outlineServerCommand.AddAlias("os");
            outlineServerCommand.AddAlias("outline");
            reportCommand.AddAlias("r");
            settingsCommand.AddAlias("s");
            interactiveCommand.AddAlias("i");
            interactiveCommand.AddAlias("repl");

            userAddCommand.AddAlias("a");
            userAddCommand.AddArgument(usernamesArgument);
            userAddCommand.Handler = CommandHandler.Create<string[]>(UserCommand.Add);

            userRenameCommand.AddArgument(new Argument<string>("oldName", "The existing username."));
            userRenameCommand.AddArgument(new Argument<string>("newName", "The new username."));
            userRenameCommand.Handler = CommandHandler.Create<string, string>(UserCommand.Rename);

            userRemoveCommand.AddAlias("rm");
            userRemoveCommand.AddAlias("del");
            userRemoveCommand.AddAlias("delete");
            userRemoveCommand.AddArgument(usernamesArgument);
            userRemoveCommand.Handler = CommandHandler.Create<string[]>(UserCommand.Remove);

            userListCommand.AddAlias("l");
            userListCommand.AddAlias("ls");
            userListCommand.AddOption(namesOnlyOption);
            userListCommand.AddOption(onePerLineOption);
            userListCommand.Handler = CommandHandler.Create<bool, bool>(UserCommand.List);

            userJoinGroupsCommand.AddArgument(new Argument<string>("username", "Target user."));
            userJoinGroupsCommand.AddArgument(new Argument<string[]>("groups", "Groups to join."));
            userJoinGroupsCommand.AddOption(new Option<bool>(new string[] { "-a", "--all", "--all-groups" }, "Join all groups."));
            userJoinGroupsCommand.Handler = CommandHandler.Create<string, string[], bool>(UserCommand.JoinGroups);

            userLeaveGroupsCommand.AddArgument(new Argument<string>("username", "Target user."));
            userLeaveGroupsCommand.AddArgument(new Argument<string[]>("groups", "Groups to leave."));
            userLeaveGroupsCommand.AddOption(new Option<bool>(new string[] { "-a", "--all", "--all-groups" }, "Leave all groups."));
            userLeaveGroupsCommand.Handler = CommandHandler.Create<string, string[], bool>(UserCommand.LeaveGroups);

            userAddCredentialCommand.AddAlias("ac");
            userAddCredentialCommand.AddArgument(new Argument<string>("username", "The user that the credential belongs to."));
            userAddCredentialCommand.AddArgument(new Argument<string>("group", "The group that the credential is for."));
            userAddCredentialCommand.AddOption(new Option<string?>("--method", "The encryption method. MUST be combined with --password."));
            userAddCredentialCommand.AddOption(new Option<string?>("--password", "The password. MUST be combined with --method."));
            userAddCredentialCommand.AddOption(new Option<string?>("--userinfo-base64url", "The userinfo encoded in URL-safe base64. Can't be used with any other option."));
            userAddCredentialCommand.Handler = CommandHandler.Create<string, string, string?, string?, string?>(UserCommand.AddCredential);

            userRemoveCredentialsCommand.AddAlias("rc");
            userRemoveCredentialsCommand.AddArgument(new Argument<string>("username", "Target user."));
            userRemoveCredentialsCommand.AddArgument(new Argument<string[]>("groups", "Credentials to these groups will be removed."));
            userRemoveCredentialsCommand.AddOption(new Option<bool>(new string[] { "-a", "--all", "--all-groups" }, "Remove credentials to all groups."));
            userRemoveCredentialsCommand.Handler = CommandHandler.Create<string, string[], bool>(UserCommand.RemoveCredentials);

            userListCredentialsCommand.AddAlias("lc");
            userListCredentialsCommand.AddOption(new Option<string[]>("--usernames", "Show credentials of these users."));
            userListCredentialsCommand.AddOption(new Option<string[]>("--groups", "Show credentials to these groups."));
            userListCredentialsCommand.Handler = CommandHandler.Create<string[], string[]>(UserCommand.ListCredentials);

            userGetSSLinksCommand.AddAlias("ss");
            userGetSSLinksCommand.AddArgument(new Argument<string>("username", "Target user."));
            userGetSSLinksCommand.AddOption(new Option<string[]>("--groups", "Get links for these groups."));
            userGetSSLinksCommand.Handler = CommandHandler.Create<string, string[]>(UserCommand.GetSSLinks);

            userGetDataUsageCommand.AddAlias("data");
            userGetDataUsageCommand.AddArgument(new Argument<string>("username", "Target user."));
            userGetDataUsageCommand.AddOption(new Option<SortBy?>("--sort-by", "Sort rule for data usage records."));
            userGetDataUsageCommand.Handler = CommandHandler.Create<string, SortBy?>(UserCommand.GetDataUsage);

            userSetDataLimitCommand.AddAlias("limit");
            userSetDataLimitCommand.AddArgument(new Argument<string>("dataLimit", "The data limit in bytes. Examples: '1024', '2K', '4M', '8G', '16T', '32P'."));
            userSetDataLimitCommand.AddArgument(new Argument<string[]>("usernames", "Target users."));
            userSetDataLimitCommand.AddOption(new Option<string[]?>("--groups", "Only set the data limit to these groups."));
            userSetDataLimitCommand.Handler = CommandHandler.Create<string, string[], string[]?>(UserCommand.SetDataLimit);

            nodeAddCommand.AddAlias("a");
            nodeAddCommand.AddArgument(new Argument<string>("group", "The group that the new node belongs to."));
            nodeAddCommand.AddArgument(new Argument<string>("nodename", "Name of the new node."));
            nodeAddCommand.AddArgument(new Argument<string>("host", "Hostname of the new node."));
            nodeAddCommand.AddArgument(new Argument<string>("portString", "Port number of the new node."));
            nodeAddCommand.AddOption(new Option<string?>("--plugin", "Plugin binary name of the new node."));
            nodeAddCommand.AddOption(new Option<string?>("--plugin-opts", "Plugin options of the new node."));
            nodeAddCommand.Handler = CommandHandler.Create<string, string, string, string, string?, string?>(NodeCommand.Add);

            nodeRenameCommand.AddArgument(new Argument<string>("group", "The group which contains the node."));
            nodeRenameCommand.AddArgument(new Argument<string>("oldName", "The existing node name."));
            nodeRenameCommand.AddArgument(new Argument<string>("newName", "The new node name."));
            nodeRenameCommand.Handler = CommandHandler.Create<string, string, string>(NodeCommand.Rename);

            nodeRemoveCommand.AddAlias("rm");
            nodeRemoveCommand.AddAlias("del");
            nodeRemoveCommand.AddAlias("delete");
            nodeRemoveCommand.AddArgument(new Argument<string>("group", "Group to delete nodes from."));
            nodeRemoveCommand.AddArgument(nodenamesArgument);
            nodeRemoveCommand.Handler = CommandHandler.Create<string, string[]>(NodeCommand.Remove);

            nodeListCommand.AddAlias("l");
            nodeListCommand.AddAlias("ls");
            nodeListCommand.AddArgument(new Argument<string[]?>("groups", "Only show nodes from these groups. Leave empty for all groups."));
            nodeListCommand.AddOption(namesOnlyOption);
            nodeListCommand.AddOption(onePerLineOption);
            nodeListCommand.Handler = CommandHandler.Create<string[]?, bool, bool>(NodeCommand.List);

            nodeActivateCommand.AddAlias("enable");
            nodeActivateCommand.AddAlias("unhide");
            nodeActivateCommand.AddArgument(new Argument<string>("group", "Target group."));
            nodeActivateCommand.AddArgument(new Argument<string[]>("nodenames", "Nodes to activate."));
            nodeActivateCommand.AddOption(new Option<bool>(new string[] { "-a", "--all", "--all-nodes" }, "Activate all nodes in target group."));
            nodeActivateCommand.Handler = CommandHandler.Create<string, string[], bool>(NodeCommand.Activate);

            nodeDeactivateCommand.AddAlias("disable");
            nodeDeactivateCommand.AddAlias("hide");
            nodeDeactivateCommand.AddArgument(new Argument<string>("group", "Target group."));
            nodeDeactivateCommand.AddArgument(new Argument<string[]>("nodenames", "Nodes to deactivate."));
            nodeDeactivateCommand.AddOption(new Option<bool>(new string[] { "-a", "--all", "--all-nodes" }, "Deactivate all nodes in target group."));
            nodeDeactivateCommand.Handler = CommandHandler.Create<string, string[], bool>(NodeCommand.Deactivate);

            groupAddCommand.AddAlias("a");
            groupAddCommand.AddArgument(groupsArgument);
            groupAddCommand.Handler = CommandHandler.Create<string[]>(GroupCommand.Add);

            groupRenameCommand.AddArgument(new Argument<string>("oldName", "The existing group name."));
            groupRenameCommand.AddArgument(new Argument<string>("newName", "The new group name."));
            groupRenameCommand.Handler = CommandHandler.Create<string, string>(GroupCommand.Rename);

            groupRemoveCommand.AddAlias("rm");
            groupRemoveCommand.AddAlias("del");
            groupRemoveCommand.AddAlias("delete");
            groupRemoveCommand.AddArgument(groupsArgument);
            groupRemoveCommand.Handler = CommandHandler.Create<string[]>(GroupCommand.Remove);

            groupListCommand.AddAlias("l");
            groupListCommand.AddAlias("ls");
            groupListCommand.AddOption(namesOnlyOption);
            groupListCommand.AddOption(onePerLineOption);
            groupListCommand.Handler = CommandHandler.Create<bool, bool>(GroupCommand.List);

            groupAddUsersCommand.AddAlias("au");
            groupAddUsersCommand.AddArgument(new Argument<string>("group", "Target group."));
            groupAddUsersCommand.AddArgument(new Argument<string[]>("usernames", "Users to add."));
            groupAddUsersCommand.AddOption(new Option<bool>(new string[] { "-a", "--all", "--all-users" }, "Add all users to target group."));
            groupAddUsersCommand.Handler = CommandHandler.Create<string, string[], bool>(GroupCommand.AddUsers);

            groupRemoveUsersCommand.AddAlias("ru");
            groupRemoveUsersCommand.AddArgument(new Argument<string>("group", "Target group."));
            groupRemoveUsersCommand.AddArgument(new Argument<string[]>("usernames", "Members to remove."));
            groupRemoveUsersCommand.AddOption(new Option<bool>(new string[] { "-a", "--all", "--all-users" }, "Remove all members of target group."));
            groupRemoveUsersCommand.Handler = CommandHandler.Create<string, string[], bool>(GroupCommand.RemoveUsers);

            groupListUsersCommand.AddAlias("lu");
            groupListUsersCommand.AddArgument(new Argument<string>("group", "Target group."));
            groupListUsersCommand.Handler = CommandHandler.Create<string>(GroupCommand.ListUsers);

            groupGetDataUsageCommand.AddAlias("data");
            groupGetDataUsageCommand.AddArgument(new Argument<string>("group", "Target group."));
            groupGetDataUsageCommand.AddOption(new Option<SortBy?>("--sort-by", "Sort rule for data usage records."));
            groupGetDataUsageCommand.Handler = CommandHandler.Create<string, SortBy?>(GroupCommand.GetDataUsage);

            groupSetDataLimitCommand.AddAlias("limit");
            groupSetDataLimitCommand.AddArgument(new Argument<string>("dataLimit", "The data limit in bytes. Examples: '1024', '2K', '4M', '8G', '16T', '32P'."));
            groupSetDataLimitCommand.AddArgument(groupsArgument);
            groupSetDataLimitCommand.AddOption(new Option<bool>("--global", "Set the global data limit of the group."));
            groupSetDataLimitCommand.AddOption(new Option<bool>("--per-user", "Set the same data limit for each user."));
            groupSetDataLimitCommand.AddOption(new Option<string[]?>("--usernames", "Only set the data limit to these users."));
            groupSetDataLimitCommand.Handler = CommandHandler.Create<string, string[], bool, bool, string[]?>(GroupCommand.SetDataLimit);

            onlineConfigGenerateCommand.AddAlias("g");
            onlineConfigGenerateCommand.AddAlias("gen");
            onlineConfigGenerateCommand.AddArgument(new Argument<string[]?>("usernames", "Specify users to generate for. Leave empty for all users."));
            onlineConfigGenerateCommand.Handler = CommandHandler.Create<string[]?>(OnlineConfigCommand.Generate);

            onlineConfigGetLinksCommand.AddAlias("l");
            onlineConfigGetLinksCommand.AddAlias("link");
            onlineConfigGetLinksCommand.AddAlias("links");
            onlineConfigGetLinksCommand.AddAlias("url");
            onlineConfigGetLinksCommand.AddAlias("urls");
            onlineConfigGetLinksCommand.AddArgument(new Argument<string[]?>("usernames", "Target users. Leave empty for all users."));
            onlineConfigGetLinksCommand.Handler = CommandHandler.Create<string[]?>(OnlineConfigCommand.GetLinks);

            onlineConfigCleanCommand.AddAlias("c");
            onlineConfigCleanCommand.AddAlias("clear");
            onlineConfigCleanCommand.AddArgument(new Argument<string[]?>("usernames", "Specify users to clean online configuration files for."));
            onlineConfigCleanCommand.AddOption(new Option<bool>("--all", "Clean for all users."));
            onlineConfigCleanCommand.Handler = CommandHandler.Create<string[]?, bool>(OnlineConfigCommand.Clean);

            outlineServerAddCommand.AddAlias("a");
            outlineServerAddCommand.AddArgument(new Argument<string>("group", "Specify a group to add the Outline server to."));
            outlineServerAddCommand.AddArgument(new Argument<string>("apiKey", "The Outline server API key."));
            outlineServerAddCommand.Handler = CommandHandler.Create<string, string, CancellationToken>(OutlineServerCommand.Add);

            outlineServerGetCommand.AddArgument(new Argument<string>("group", "The associated group."));
            outlineServerGetCommand.Handler = CommandHandler.Create<string>(OutlineServerCommand.Get);

            outlineServerSetCommand.AddArgument(new Argument<string>("group", "The associated group."));
            outlineServerSetCommand.AddOption(new Option<string?>("--name", "Name of the Outline server."));
            outlineServerSetCommand.AddOption(new Option<string?>("--hostname", "Hostname of the Outline server."));
            outlineServerSetCommand.AddOption(new Option<int?>("--port", "Port number for new access keys on the Outline server."));
            outlineServerSetCommand.AddOption(new Option<bool?>("--metrics", "Enable or disable telemetry on the Outline server."));
            outlineServerSetCommand.AddOption(new Option<string?>("--default-user", "The default user for Outline server's default access key (id: 0)."));
            outlineServerSetCommand.Handler = CommandHandler.Create<string, string?, string?, int?, bool?, string?, CancellationToken>(OutlineServerCommand.Set);

            outlineServerRemoveCommand.AddAlias("rm");
            outlineServerRemoveCommand.AddArgument(groupsArgument);
            outlineServerRemoveCommand.AddOption(new Option<bool>("--remove-creds", "Remove credentials from all associated users."));
            outlineServerRemoveCommand.Handler = CommandHandler.Create<string[], bool>(OutlineServerCommand.Remove);

            outlineServerPullCommand.AddAlias("update");
            outlineServerPullCommand.AddArgument(new Argument<string[]?>("groups", "Specify groups to update for."));
            outlineServerPullCommand.AddOption(new Option<bool>("--no-sync", "Do not update local user credential storage from retrieved access key list."));
            outlineServerPullCommand.Handler = CommandHandler.Create<string[]?, bool, CancellationToken>(OutlineServerCommand.Pull);

            outlineServerDeployCommand.AddArgument(new Argument<string[]?>("groups", "Groups to deploy for."));
            outlineServerDeployCommand.Handler = CommandHandler.Create<string[]?, CancellationToken>(OutlineServerCommand.Deploy);

            outlineServerRotatePasswordCommand.AddAlias("rotate");
            outlineServerRotatePasswordCommand.AddOption(new Option<string[]?>("--usernames", "Target users."));
            outlineServerRotatePasswordCommand.AddOption(new Option<string[]?>("--groups", "Target groups."));
            outlineServerRotatePasswordCommand.Handler = CommandHandler.Create<string[]?, string[]?, CancellationToken>(OutlineServerCommand.RotatePassword);

            reportCommand.AddOption(new Option<SortBy?>("--group-sort-by", "Sort rule for group data usage records."));
            reportCommand.AddOption(new Option<SortBy?>("--user-sort-by", "Sort rule for user data usage records."));
            reportCommand.Handler = CommandHandler.Create<SortBy?, SortBy?>(ReportCommand.Generate);

            settingsGetCommand.Handler = CommandHandler.Create(SettingsCommand.Get);

            settingsSetCommand.AddOption(new Option<SortBy?>("--user-data-usage-default-sort-by", "The default sort rule for user data usage report."));
            settingsSetCommand.AddOption(new Option<SortBy?>("--group-data-usage-default-sort-by", "The default sort rule for group data usage report."));
            settingsSetCommand.AddOption(new Option<bool?>("--online-config-sort-by-name", "Whether the generated servers list in an SIP008 JSON should be sorted by server name."));
            settingsSetCommand.AddOption(new Option<bool?>("--online-config-deliver-by-group", "Whether online config should be delivered to each user by group. Turning this on will generate one online config JSON for each group associated with the user, in addition to the single JSON that contains all associated servers."));
            settingsSetCommand.AddOption(new Option<bool?>("--online-config-clean-on-user-removal", "Whether the user's online configuration file should be removed when the user is being removed."));
            settingsSetCommand.AddOption(new Option<bool?>("--online-config-update-data-usage-on-generation", "Whether data usage metrics are updated from configured sources when generating online config."));
            settingsSetCommand.AddOption(new Option<string?>("--online-config-output-directory", "Online configuration generation output directory. No trailing slashes allowed."));
            settingsSetCommand.AddOption(new Option<string?>("--online-config-delivery-root-uri", "The URL base for SIP008 online configuration delivery. No trailing slashes allowed."));
            settingsSetCommand.AddOption(new Option<bool?>("--outline-server-deploy-on-change", "Whether changes made to local databases trigger deployments to linked Outline servers."));
            settingsSetCommand.AddOption(new Option<bool?>("--outline-server-apply-default-user-on-association", "Whether to apply the global default user when associating with Outline servers."));
            settingsSetCommand.AddOption(new Option<string?>("--outline-server-global-default-user", "The global setting for Outline server's default access key's user."));
            settingsSetCommand.Handler = CommandHandler.Create<SortBy?, SortBy?, bool?, bool?, bool?, bool?, string?, string?, bool?, bool?, string?>(SettingsCommand.Set);

            interactiveCommand.Handler = CommandHandler.Create(
                async () =>
                {
                    while (true)
                    {
                        Console.Write("> ");
                        var inputLine = Console.ReadLine()?.Trim();

                        // Verify input
                        if (inputLine == null)
                            continue;
                        if (inputLine is "exit" or "quit")
                            break;
                        if (inputLine is "i" or "interactive")
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
            serviceCommand.Handler = CommandHandler.Create<int, bool, bool, bool, bool, CancellationToken>(ServiceCommand.Run);

            Console.OutputEncoding = Encoding.UTF8;
            return rootCommand.InvokeAsync(args);
        }
    }
}
