using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ShadowsocksUriGenerator
{
    class Program
    {
        static Task<int> Main(string[] args)
        {
            Users users;
            Nodes nodes;
            Settings settings;

            var loadUsersTask = Users.LoadUsersAsync();
            var loadNodesTask = Nodes.LoadNodesAsync();
            var loadSettingsTask = Settings.LoadSettingsAsync();

            var userAddCommand = new Command("add", "Add users.");
            var userRenameCommand = new Command("rename", "Renames an existing user with a new name.");
            var userRemoveCommand = new Command("remove", "Remove users.");
            var userListCommand = new Command("list", "List all users.");
            var userJoinGroupCommand = new Command("join", "Join a group.");
            var userLeaveGroupCommand = new Command("leave", "Leave a group.");
            var userAddCredentialCommand = new Command("add-credential", "Add a credential associated with a group for the user.");
            var userRemoveCredentialCommand = new Command("remove-credential", "Remove the group's credential from the user.");
            var userListCredentialsCommand = new Command("list-credentials", "List all user credentials.");
            var userGetSSLinksCommand = new Command("get-ss-links", "Get the user's associated Shadowsocks URLs.");
            var userGetDataUsageCommand = new Command("get-data-usage", "Get the user's data usage records.");
            var userSetDataLimitCommand = new Command("set-data-limit", "Set a data limit for specified users and/or groups.");

            var userCommand = new Command("user", "Manage users.")
            {
                userAddCommand,
                userRenameCommand,
                userRemoveCommand,
                userListCommand,
                userJoinGroupCommand,
                userLeaveGroupCommand,
                userAddCredentialCommand,
                userRemoveCredentialCommand,
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
            var groupAddUserCommand = new Command("add-user", "Add users to the group.");
            var groupRemoveUserCommand = new Command("remove-user", "Remove users from the group.");
            var groupListUsersCommand = new Command("list-users", "List users in the group.");
            var groupGetDataUsageCommand = new Command("get-data-usage", "Get the group's data usage records.");
            var groupSetDataLimitCommand = new Command("set-data-limit", "Set a data limit for specified users and/or groups.");

            var groupCommand = new Command("group", "Manage groups.")
            {
                groupAddCommand,
                groupRenameCommand,
                groupRemoveCommand,
                groupListCommand,
                groupAddUserCommand,
                groupRemoveUserCommand,
                groupListUsersCommand,
                groupGetDataUsageCommand,
                groupSetDataLimitCommand,
            };

            var onlineConfigGenerateCommand = new Command("generate", "Generate SIP008-compliant online configuration delivery JSON files.");
            var onlineConfigGetLinkCommand = new Command("get-link", "Get the user's SIP008-compliant online configuration delivery URL.");
            var onlineConfigCleanCommand = new Command("clean", "Clean online configuration files for specified or all users.");

            var onlineConfigCommand = new Command("online-config", "Manage SIP008 online configuration.")
            {
                onlineConfigGenerateCommand,
                onlineConfigGetLinkCommand,
                onlineConfigCleanCommand,
            };

            var outlineServerAddCommand = new Command("add", "Associate an Outline server with a group.");
            var outlineServerGetCommand = new Command("get", "Get the associated Outline server's information.");
            var outlineServerSetCommand = new Command("set", "Change settings of the associated Outline server.");
            var outlineServerRemoveCommand = new Command("remove", "Remove the Outline server from the group.");
            var outlineServerUpdateCommand = new Command("update", "Update server information, access keys, and metrics from the associated Outline server.");
            var outlineServerDeployCommand = new Command("deploy", "Deploy the group's configuration to the associated Outline server.");
            var outlineServerRotatePasswordCommand = new Command("rotate-password", "Rotate passwords for the specified users and/or groups.");

            var outlineServerCommand = new Command("outline-server", "Manage Outline server.")
            {
                outlineServerAddCommand,
                outlineServerGetCommand,
                outlineServerSetCommand,
                outlineServerRemoveCommand,
                outlineServerUpdateCommand,
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
            outlineServerCommand.AddAlias("os");
            outlineServerCommand.AddAlias("outline");
            reportCommand.AddAlias("r");
            settingsCommand.AddAlias("s");
            interactiveCommand.AddAlias("i");

            userAddCommand.AddAlias("a");
            userAddCommand.AddArgument(usernamesArgument);
            userAddCommand.Handler = CommandHandler.Create(
                async (string[] usernames) =>
                {
                    users = await loadUsersTask;
                    var addedUsers = users.AddUsers(usernames);
                    await Users.SaveUsersAsync(users);
                    Console.WriteLine("Successfully added:");
                    foreach (var username in addedUsers)
                        Console.WriteLine($"{username}");
                });

            nodeAddCommand.AddAlias("a");
            nodeAddCommand.AddArgument(new Argument<string>("group", "The group that the new node belongs to."));
            nodeAddCommand.AddArgument(new Argument<string>("nodename", "Name of the new node."));
            nodeAddCommand.AddArgument(new Argument<string>("host", "Hostname of the new node."));
            nodeAddCommand.AddArgument(new Argument<string>("portString", "Port number of the new node."));
            nodeAddCommand.AddOption(new Option<string?>("--plugin", "Plugin binary name of the new node."));
            nodeAddCommand.AddOption(new Option<string?>("--plugin-opts", "Plugin options of the new node."));
            nodeAddCommand.Handler = CommandHandler.Create(
                async (string group, string nodename, string host, string portString, string? plugin, string? pluginOpts) =>
                {
                    nodes = await loadNodesTask;
                    if (nodes.AddNodeToGroup(group, nodename, host, portString, plugin, pluginOpts) == 0)
                        Console.WriteLine($"Added {nodename} to group {group}.");
                    else
                        Console.WriteLine($"Group not found. Or node already exists. Or bad port range.");
                    await Nodes.SaveNodesAsync(nodes);
                });

            groupAddCommand.AddAlias("a");
            groupAddCommand.AddArgument(groupsArgument);
            groupAddCommand.Handler = CommandHandler.Create(
                async (string[] groups) =>
                {
                    nodes = await loadNodesTask;
                    var addedNodes = nodes.AddGroups(groups);
                    await Nodes.SaveNodesAsync(nodes);
                    Console.WriteLine("Successfully added:");
                    foreach (var nodename in addedNodes)
                        Console.WriteLine($"{nodename}");
                });

            userRenameCommand.AddArgument(new Argument<string>("oldName", "The existing username."));
            userRenameCommand.AddArgument(new Argument<string>("newName", "The new username."));
            userRenameCommand.Handler = CommandHandler.Create(
                async (string oldName, string newName) =>
                {
                    users = await loadUsersTask;
                    nodes = await loadNodesTask;
                    var result = await users.RenameUser(oldName, newName, nodes);
                    if (result == -1)
                        Console.WriteLine($"User not found: {oldName}");
                    else if (result == -2)
                        Console.WriteLine($"A user with the same name already exists: {newName}");
                    else if (result == -3)
                        Console.WriteLine("An error occurred while sending renaming requests to Outline server.");
                    await Users.SaveUsersAsync(users);
                });

            nodeRenameCommand.AddArgument(new Argument<string>("group", "The group which contains the node."));
            nodeRenameCommand.AddArgument(new Argument<string>("oldName", "The existing node name."));
            nodeRenameCommand.AddArgument(new Argument<string>("newName", "The new node name."));
            nodeRenameCommand.Handler = CommandHandler.Create(
                async (string group, string oldName, string newName) =>
                {
                    nodes = await loadNodesTask;
                    var result = nodes.RenameNodeInGroup(group, oldName, newName);
                    if (result == -1)
                        Console.WriteLine($"Node not found: {oldName}");
                    else if (result == -2)
                        Console.WriteLine($"A node with the same name already exists: {newName}");
                    else if (result == -3)
                        Console.WriteLine($"Group not found: {group}");
                    await Nodes.SaveNodesAsync(nodes);
                });

            groupRenameCommand.AddArgument(new Argument<string>("oldName", "The existing group name."));
            groupRenameCommand.AddArgument(new Argument<string>("newName", "The new group name."));
            groupRenameCommand.Handler = CommandHandler.Create(
                async (string oldName, string newName) =>
                {
                    users = await loadUsersTask;
                    nodes = await loadNodesTask;
                    var result = nodes.RenameGroup(oldName, newName);
                    if (result == -1)
                        Console.WriteLine($"Group not found: {oldName}");
                    else if (result == -2)
                        Console.WriteLine($"A group with the same name already exists: {newName}");
                    else // success
                        users.UpdateCredentialGroupsForAllUsers(oldName, newName);
                    await Users.SaveUsersAsync(users);
                    await Nodes.SaveNodesAsync(nodes);
                });

            userRemoveCommand.AddAlias("rm");
            userRemoveCommand.AddAlias("del");
            userRemoveCommand.AddAlias("delete");
            userRemoveCommand.AddArgument(usernamesArgument);
            userRemoveCommand.Handler = CommandHandler.Create(
                async (string[] usernames) =>
                {
                    users = await loadUsersTask;
                    settings = await loadSettingsTask;
                    // Removing online config requires reading user entry.
                    if (settings.OnlineConfigCleanOnUserRemoval)
                        OnlineConfig.Remove(users, settings, usernames);
                    // Remove user entry.
                    users.RemoveUsers(usernames);
                    await Users.SaveUsersAsync(users);
                });

            nodeRemoveCommand.AddAlias("rm");
            nodeRemoveCommand.AddAlias("del");
            nodeRemoveCommand.AddAlias("delete");
            nodeRemoveCommand.AddArgument(new Argument<string>("group", "Group to delete nodes from."));
            nodeRemoveCommand.AddArgument(nodenamesArgument);
            nodeRemoveCommand.Handler = CommandHandler.Create(
                async (string group, string[] nodenames) =>
                {
                    nodes = await loadNodesTask;
                    if (nodes.RemoveNodesFromGroup(group, nodenames) == -1)
                        Console.WriteLine($"Group not found: {group}.");
                    await Nodes.SaveNodesAsync(nodes);
                });

            groupRemoveCommand.AddAlias("rm");
            groupRemoveCommand.AddAlias("del");
            groupRemoveCommand.AddAlias("delete");
            groupRemoveCommand.AddArgument(groupsArgument);
            groupRemoveCommand.Handler = CommandHandler.Create(
                async (string[] groups) =>
                {
                    users = await loadUsersTask;
                    nodes = await loadNodesTask;
                    users.RemoveAllUsersFromGroups(groups);
                    nodes.RemoveGroups(groups);
                    await Users.SaveUsersAsync(users);
                    await Nodes.SaveNodesAsync(nodes);
                });

            userListCommand.AddAlias("l");
            userListCommand.AddAlias("ls");
            userListCommand.AddOption(namesOnlyOption);
            userListCommand.AddOption(onePerLineOption);
            userListCommand.Handler = CommandHandler.Create(
                async (bool namesOnly, bool onePerLine) =>
                {
                    users = await loadUsersTask;

                    if (namesOnly)
                    {
                        var usernames = users.UserDict.Keys.ToList();
                        Utilities.PrintNameList(usernames, onePerLine);
                        return;
                    }

                    var maxNameLength = users.UserDict.Select(x => x.Key.Length)
                                                      .DefaultIfEmpty()
                                                      .Max();
                    var nameFieldWidth = maxNameLength > 4 ? maxNameLength + 2 : 6;

                    Utilities.PrintTableBorder(nameFieldWidth, 36, 18);
                    Console.WriteLine($"|{"User".PadRight(nameFieldWidth)}|{"UUID",36}|{"Associated Groups",18}|");
                    Utilities.PrintTableBorder(nameFieldWidth, 36, 18);

                    foreach (var user in users.UserDict)
                        Console.WriteLine($"|{user.Key.PadRight(nameFieldWidth)}|{user.Value.Uuid,36}|{user.Value.Credentials.Count,18}|");

                    Utilities.PrintTableBorder(nameFieldWidth, 36, 18);
                });

            nodeListCommand.AddAlias("l");
            nodeListCommand.AddAlias("ls");
            nodeListCommand.AddArgument(new Argument<string[]?>("groups", "Only show nodes from these groups. Leave empty for all groups."));
            nodeListCommand.AddOption(namesOnlyOption);
            nodeListCommand.AddOption(onePerLineOption);
            nodeListCommand.Handler = CommandHandler.Create(
                async (string[]? groups, bool namesOnly, bool onePerLine) =>
                {
                    nodes = await loadNodesTask;

                    if (namesOnly)
                    {
                        foreach (var groupEntry in nodes.Groups)
                        {
                            if (groups != null && !groups.Contains(groupEntry.Key))
                                continue;

                            Console.WriteLine($"Group: {groupEntry.Key}");
                            var keys = groupEntry.Value.NodeDict.Keys.ToList();
                            Utilities.PrintNameList(keys, onePerLine);
                            Console.WriteLine();
                        }

                        return;
                    }

                    var maxNodeNameLength = nodes.Groups.SelectMany(x => x.Value.NodeDict.Keys)
                                                        .Select(x => x.Length)
                                                        .DefaultIfEmpty()
                                                        .Max();
                    var maxGroupNameLength = nodes.Groups.Select(x => x.Key.Length)
                                                         .DefaultIfEmpty()
                                                         .Max();
                    var maxHostnameLength = nodes.Groups.SelectMany(x => x.Value.NodeDict.Values)
                                                        .Select(x => x.Host.Length)
                                                        .DefaultIfEmpty()
                                                        .Max();
                    var maxPluginLength = nodes.Groups.SelectMany(x => x.Value.NodeDict.Values)
                                                      .Select(x => x.Plugin?.Length ?? 0)
                                                      .DefaultIfEmpty()
                                                      .Max();
                    var maxPluginOptsLength = nodes.Groups.SelectMany(x => x.Value.NodeDict.Values)
                                                          .Select(x => x.PluginOpts?.Length ?? 0)
                                                          .DefaultIfEmpty()
                                                          .Max();
                    var nodeNameFieldWidth = maxNodeNameLength > 4 ? maxNodeNameLength + 2 : 6;
                    var groupNameFieldWidth = maxGroupNameLength > 5 ? maxGroupNameLength + 2 : 7;
                    var hostnameFieldWidth = maxHostnameLength > 4 ? maxHostnameLength + 2 : 6;
                    var pluginFieldWidth = maxPluginLength > 6 ? maxPluginLength + 2 : 8;
                    var pluginOptsFieldWidth = maxPluginOptsLength > 14 ? maxPluginOptsLength + 2 : 16;

                    Utilities.PrintTableBorder(7, nodeNameFieldWidth, groupNameFieldWidth, 36, hostnameFieldWidth, 5, pluginFieldWidth, pluginOptsFieldWidth);
                    Console.WriteLine($"|{"Status",7}|{"Node".PadRight(nodeNameFieldWidth)}|{"Group".PadRight(groupNameFieldWidth)}|{"UUID",36}|{"Host".PadLeft(hostnameFieldWidth)}|{"Port",5}|{"Plugin".PadLeft(pluginFieldWidth)}|{"Plugin Options".PadLeft(pluginOptsFieldWidth)}|");
                    Utilities.PrintTableBorder(7, nodeNameFieldWidth, groupNameFieldWidth, 36, hostnameFieldWidth, 5, pluginFieldWidth, pluginOptsFieldWidth);

                    foreach (var groupEntry in nodes.Groups)
                    {
                        if (groups != null && !groups.Contains(groupEntry.Key))
                            continue;

                        foreach (var node in groupEntry.Value.NodeDict)
                            PrintNodeInfo(node, groupEntry.Key);
                    }

                    Utilities.PrintTableBorder(7, nodeNameFieldWidth, groupNameFieldWidth, 36, hostnameFieldWidth, 5, pluginFieldWidth, pluginOptsFieldWidth);

                    void PrintNodeInfo(KeyValuePair<string, Node> node, string group)
                    {
                        Console.WriteLine($"|{(node.Value.Deactivated ? "🛑" : "✔"),7}|{node.Key.PadRight(nodeNameFieldWidth)}|{group.PadRight(groupNameFieldWidth)}|{node.Value.Uuid,36}|{node.Value.Host.PadLeft(hostnameFieldWidth)}|{node.Value.Port,5}|{(node.Value.Plugin ?? string.Empty).PadLeft(pluginFieldWidth)}|{(node.Value.PluginOpts ?? string.Empty).PadLeft(pluginOptsFieldWidth)}|");
                    }
                });

            groupListCommand.AddAlias("l");
            groupListCommand.AddAlias("ls");
            groupListCommand.AddOption(namesOnlyOption);
            groupListCommand.AddOption(onePerLineOption);
            groupListCommand.Handler = CommandHandler.Create(
                async (bool namesOnly, bool onePerLine) =>
                {
                    nodes = await loadNodesTask;

                    if (namesOnly)
                    {
                        var names = nodes.Groups.Keys.ToList();
                        Utilities.PrintNameList(names, onePerLine);
                        return;
                    }

                    var maxGroupNameLength = nodes.Groups.Select(x => x.Key.Length)
                                                         .DefaultIfEmpty()
                                                         .Max();
                    var maxOutlineServerNameLength = nodes.Groups.Select(x => x.Value.OutlineServerInfo?.Name.Length ?? 0)
                                                                 .DefaultIfEmpty()
                                                                 .Max();
                    var groupNameFieldWidth = maxGroupNameLength > 5 ? maxGroupNameLength + 2 : 7;
                    var outlineServerNameFieldWidth = maxOutlineServerNameLength > 14 ? maxOutlineServerNameLength + 2 : 16;

                    Utilities.PrintTableBorder(groupNameFieldWidth, 16, outlineServerNameFieldWidth);
                    Console.WriteLine($"|{"Group".PadRight(groupNameFieldWidth)}|{"Number of Nodes",16}|{"Outline Server".PadLeft(outlineServerNameFieldWidth)}|");
                    Utilities.PrintTableBorder(groupNameFieldWidth, 16, outlineServerNameFieldWidth);

                    foreach (var group in nodes.Groups)
                    {
                        Console.WriteLine($"|{group.Key.PadRight(groupNameFieldWidth)}|{group.Value.NodeDict.Count,16}|{(group.Value.OutlineServerInfo?.Name ?? "No").PadLeft(outlineServerNameFieldWidth)}|");
                    }

                    Utilities.PrintTableBorder(groupNameFieldWidth, 16, outlineServerNameFieldWidth);
                });

            userJoinGroupCommand.AddArgument(new Argument<string>("username", "Target user."));
            userJoinGroupCommand.AddArgument(new Argument<string[]>("groups", "Groups to join."));
            userJoinGroupCommand.AddOption(new Option<bool>(new string[] { "-a", "--all", "--all-groups" }, "Join all groups."));
            userJoinGroupCommand.Handler = CommandHandler.Create(
                async (string username, string[] groups, bool allGroups) =>
                {
                    users = await loadUsersTask;
                    nodes = await loadNodesTask;

                    if (allGroups)
                        groups = nodes.Groups.Keys.ToArray();

                    foreach (var group in groups)
                    {
                        if (!allGroups && !nodes.Groups.ContainsKey(group)) // check group existence when group is specified by user.
                        {
                            Console.WriteLine($"Group not found: {group}");
                            continue;
                        }

                        var result = users.AddUserToGroup(username, group);

                        switch (result)
                        {
                            case 0:
                                Console.WriteLine($"Successfully added {username} to {group}.");
                                break;
                            case 1:
                                Console.WriteLine($"The user is already in group {group}.");
                                break;
                            case -1:
                                Console.WriteLine("User not found.");
                                break;
                            default:
                                Console.WriteLine($"Unknown error: {result}.");
                                break;
                        }
                    }

                    await Users.SaveUsersAsync(users);
                });

            userLeaveGroupCommand.AddArgument(new Argument<string>("username", "Target user."));
            userLeaveGroupCommand.AddArgument(new Argument<string[]>("groups", "Groups to leave."));
            userLeaveGroupCommand.AddOption(new Option<bool>(new string[] { "-a", "--all", "--all-groups" }, "Leave all groups."));
            userLeaveGroupCommand.Handler = CommandHandler.Create(
                async (string username, string[] groups, bool allGroups) =>
                {
                    users = await loadUsersTask;

                    if (allGroups)
                    {
                        var result = users.RemoveUserFromAllGroups(username);

                        if (result == -2)
                            Console.WriteLine($"User not found: {username}");
                    }

                    foreach (var group in groups)
                    {
                        var result = users.RemoveUserFromGroup(username, group);

                        if (result == 1)
                            Console.WriteLine($"User {username} is not in group {group}.");
                        else if (result == -2)
                            Console.WriteLine($"User not found: {username}");
                    }

                    await Users.SaveUsersAsync(users);
                });

            userAddCredentialCommand.AddAlias("ac");
            userAddCredentialCommand.AddArgument(new Argument<string>("username", "The user that the credential belongs to."));
            userAddCredentialCommand.AddArgument(new Argument<string>("group", "The group that the credential is for."));
            userAddCredentialCommand.AddOption(new Option<string?>("--method", "The encryption method. MUST be combined with --password."));
            userAddCredentialCommand.AddOption(new Option<string?>("--password", "The password. MUST be combined with --method."));
            userAddCredentialCommand.AddOption(new Option<string?>("--userinfo-base64url", "The userinfo encoded in URL-safe base64. Can't be used with any other option."));
            userAddCredentialCommand.Handler = CommandHandler.Create(
                async (string username, string group, string? method, string? password, string? userinfoBase64url) =>
                {
                    users = await loadUsersTask;
                    nodes = await loadNodesTask;

                    if (!nodes.Groups.ContainsKey(group))
                    {
                        Console.WriteLine($"Group not found: {group}");
                        return;
                    }

                    int result;

                    if (!string.IsNullOrEmpty(method) && !string.IsNullOrEmpty(password))
                        result = users.AddCredentialToUser(username, group, method, password);
                    else if (!string.IsNullOrEmpty(userinfoBase64url))
                        result = users.AddCredentialToUser(username, group, userinfoBase64url);
                    else
                        result = users.AddUserToGroup(username, group);

                    switch (result)
                    {
                        case 0:
                            Console.WriteLine($"Successfully added {username} to {group}.");
                            break;
                        case 1:
                            Console.WriteLine($"The user is already in the group.");
                            break;
                        case 2:
                            Console.WriteLine("The user already has a credential for the group.");
                            break;
                        case -1:
                            Console.WriteLine("User not found.");
                            break;
                        case -2:
                            Console.WriteLine("The provided credential is invalid.");
                            break;
                        default:
                            Console.WriteLine($"Unknown error: {result}.");
                            break;
                    }

                    await Users.SaveUsersAsync(users);
                });

            userRemoveCredentialCommand.AddAlias("rc");
            userRemoveCredentialCommand.AddArgument(new Argument<string>("username", "Target user."));
            userRemoveCredentialCommand.AddArgument(new Argument<string[]>("groups", "Credentials to these groups will be removed."));
            userRemoveCredentialCommand.AddOption(new Option<bool>(new string[] { "-a", "--all", "--all-groups" }, "Remove credentials to all groups."));
            userRemoveCredentialCommand.Handler = CommandHandler.Create(
                async (string username, string[] groups, bool allGroups) =>
                {
                    users = await loadUsersTask;

                    if (allGroups)
                    {
                        var result = users.RemoveAllCredentialsFromUser(username);
                        if (result == -2)
                            Console.WriteLine($"User not found: {username}");
                    }

                    foreach (var group in groups)
                    {
                        var result = users.RemoveCredentialFromUser(username, group);
                        if (result == -1)
                            Console.WriteLine($"User {username} is not in group {group}.");
                        else if (result == -2)
                            Console.WriteLine($"User not found: {username}");
                    }

                    await Users.SaveUsersAsync(users);
                });

            userListCredentialsCommand.AddAlias("lc");
            userListCredentialsCommand.AddOption(new Option<string[]>("--usernames", "Show credentials of these users."));
            userListCredentialsCommand.AddOption(new Option<string[]>("--groups", "Show credentials to these groups."));
            userListCredentialsCommand.Handler = CommandHandler.Create(
                async (string[] usernames, string[] groups) =>
                {
                    users = await loadUsersTask;

                    var maxUsernameLength = users.UserDict.Select(x => x.Key.Length)
                                                          .DefaultIfEmpty()
                                                          .Max();
                    var maxGroupNameLength = users.UserDict.SelectMany(x => x.Value.Credentials.Keys)
                                                           .Select(x => x.Length)
                                                           .DefaultIfEmpty()
                                                           .Max();
                    var maxPasswordLength = users.UserDict.SelectMany(x => x.Value.Credentials.Values)
                                                          .Select(x => x?.Password.Length ?? 0)
                                                          .DefaultIfEmpty()
                                                          .Max();
                    var usernameFieldWidth = maxUsernameLength > 4 ? maxUsernameLength + 2 : 6;
                    var groupNameFieldWidth = maxGroupNameLength > 5 ? maxGroupNameLength + 2 : 7;
                    var passwordFieldWidth = maxPasswordLength > 8 ? maxPasswordLength + 2 : 10;

                    Utilities.PrintTableBorder(usernameFieldWidth, groupNameFieldWidth, 24, passwordFieldWidth);
                    Console.WriteLine($"|{"User".PadRight(usernameFieldWidth)}|{"Group".PadRight(groupNameFieldWidth)}|{"Method",-24}|{"Password".PadRight(passwordFieldWidth)}|");
                    Utilities.PrintTableBorder(usernameFieldWidth, groupNameFieldWidth, 24, passwordFieldWidth);

                    foreach (var user in users.UserDict)
                    {
                        if (usernames.Length > 0 && !usernames.Contains(user.Key))
                            continue;

                        foreach (var credEntry in user.Value.Credentials)
                        {
                            if (groups.Length > 0 && !groups.Contains(credEntry.Key))
                                continue;
                            
                            if (credEntry.Value == null)
                                Console.WriteLine($"|{user.Key.PadRight(usernameFieldWidth)}|{credEntry.Key.PadRight(groupNameFieldWidth)}|{string.Empty,-24}|{string.Empty.PadRight(passwordFieldWidth)}|");
                            else
                                Console.WriteLine($"|{user.Key.PadRight(usernameFieldWidth)}|{credEntry.Key.PadRight(groupNameFieldWidth)}|{credEntry.Value.Method,-24}|{credEntry.Value.Password.PadRight(passwordFieldWidth)}|");
                        }
                    }

                    Utilities.PrintTableBorder(usernameFieldWidth, groupNameFieldWidth, 24, passwordFieldWidth);
                });

            userGetSSLinksCommand.AddAlias("ss");
            userGetSSLinksCommand.AddArgument(new Argument<string>("username", "Target user."));
            userGetSSLinksCommand.AddOption(new Option<string[]>("--groups", "Get links for these groups."));
            userGetSSLinksCommand.Handler = CommandHandler.Create(
                async (string username, string[] groups) =>
                {
                    users = await loadUsersTask;
                    nodes = await loadNodesTask;
                    var uris = users.GetUserSSUris(username, nodes, groups);
                    foreach (var uri in uris)
                        Console.WriteLine($"{uri.AbsoluteUri}");
                });

            userGetDataUsageCommand.AddAlias("data");
            userGetDataUsageCommand.AddArgument(new Argument<string>("username", "Target user."));
            userGetDataUsageCommand.AddOption(new Option<SortBy?>("--sort-by", "Sort rule for data usage records."));
            userGetDataUsageCommand.Handler = CommandHandler.Create(
                async (string username, SortBy? sortBy) =>
                {
                    users = await loadUsersTask;
                    nodes = await loadNodesTask;
                    settings = await loadSettingsTask;

                    var records = users.GetUserDataUsage(username, nodes);
                    var maxNameLength = records.Select(x => x.group.Length)
                                               .DefaultIfEmpty()
                                               .Max();
                    var nameFieldWidth = maxNameLength > 5 ? maxNameLength + 2 : 7;

                    var sortByInEffect = settings.UserDataUsageDefaultSortBy;
                    if (sortBy is SortBy currentRunSortBy)
                        sortByInEffect = currentRunSortBy;
                    switch (sortByInEffect)
                    {
                        case SortBy.DefaultAscending:
                            break;
                        case SortBy.DefaultDescending:
                            records.Reverse();
                            break;
                        case SortBy.NameAscending:
                            records = records.OrderBy(x => x.group).ToList();
                            break;
                        case SortBy.NameDescending:
                            records = records.OrderByDescending(x => x.group).ToList();
                            break;
                        case SortBy.DataUsedAscending:
                            records = records.OrderBy(x => x.bytesUsed).ToList();
                            break;
                        case SortBy.DataUsedDescending:
                            records = records.OrderByDescending(x => x.bytesUsed).ToList();
                            break;
                        case SortBy.DataRemainingAscending:
                            records = records.OrderBy(x => x.bytesRemaining).ToList();
                            break;
                        case SortBy.DataRemainingDescending:
                            records = records.OrderByDescending(x => x.bytesRemaining).ToList();
                            break;
                    }

                    Console.WriteLine($"{"User",-16}{username,-32}");
                    if (users.UserDict.TryGetValue(username, out var user))
                    {
                        Console.WriteLine($"{"Data used",-16}{Utilities.HumanReadableDataString(user.BytesUsed),-32}");
                        if (user.BytesRemaining != 0UL)
                            Console.WriteLine($"{"Data remaining",-16}{Utilities.HumanReadableDataString(user.BytesRemaining),-32}");
                        if (user.DataLimitInBytes != 0UL)
                            Console.WriteLine($"{"Data limit",-16}{Utilities.HumanReadableDataString(user.DataLimitInBytes),-32}");
                    }

                    Console.WriteLine();

                    Utilities.PrintTableBorder(nameFieldWidth, 11, 16);

                    Console.WriteLine($"|{"Group".PadRight(nameFieldWidth)}|{"Data Used",11}|{"Data Remaining",16}|");

                    Utilities.PrintTableBorder(nameFieldWidth, 11, 16);

                    foreach (var (group, bytesUsed, bytesRemaining) in records)
                    {
                        Console.Write($"|{group.PadRight(nameFieldWidth)}|{Utilities.HumanReadableDataString(bytesUsed),11}|");
                        if (bytesRemaining != 0UL)
                            Console.WriteLine($"{Utilities.HumanReadableDataString(bytesRemaining),16}|");
                        else
                            Console.WriteLine($"{string.Empty,16}|");
                    }

                    Utilities.PrintTableBorder(nameFieldWidth, 11, 16);
                });

            userSetDataLimitCommand.AddAlias("limit");
            userSetDataLimitCommand.AddArgument(new Argument<string>("dataLimit", "The data limit in bytes. Examples: '1024', '2K', '4M', '8G', '16T', '32P'."));
            userSetDataLimitCommand.AddArgument(new Argument<string[]>("usernames", "Target users."));
            userSetDataLimitCommand.AddOption(new Option<string[]?>("--groups", "Only set the data limit to these groups."));
            userSetDataLimitCommand.Handler = CommandHandler.Create(
                async (string dataLimit, string[] usernames, string[]? groups) =>
                {
                    users = await loadUsersTask;
                    nodes = await loadNodesTask;

                    if (Utilities.TryParseDataLimitString(dataLimit, out var dataLimitInBytes))
                        foreach (var username in usernames)
                        {
                            var result = users.SetDataLimitToUser(dataLimitInBytes, username, groups);
                            switch (result)
                            {
                                case -1:
                                    Console.WriteLine($"User not found: {username}.");
                                    break;
                                case -2:
                                    Console.WriteLine($"An error occurred while setting for {username}: some groups were not found.");
                                    break;
                            }
                        }
                    else
                        Console.WriteLine($"An error occurred while parsing the data limit: {dataLimit}");

                    await Users.SaveUsersAsync(users);
                });

            nodeActivateCommand.AddAlias("enable");
            nodeActivateCommand.AddAlias("unhide");
            nodeActivateCommand.AddArgument(new Argument<string>("group", "Target group."));
            nodeActivateCommand.AddArgument(new Argument<string[]>("nodenames", "Nodes to activate."));
            nodeActivateCommand.AddOption(new Option<bool>(new string[] { "-a", "--all", "--all-nodes" }, "Activate all nodes in target group."));
            nodeActivateCommand.Handler = CommandHandler.Create(
                async (string group, string[] nodenames, bool allNodes) =>
                {
                    nodes = await loadNodesTask;

                    if (allNodes)
                    {
                        var result = nodes.ActivateAllNodesInGroup(group);

                        switch (result)
                        {
                            case 0:
                                Console.WriteLine($"Successfully activated all nodes in {group}");
                                break;
                            case -2:
                                Console.WriteLine($"Error: {group} is not found.");
                                break;
                        }
                    }

                    foreach (var nodename in nodenames)
                    {
                        var result = nodes.ActivateNodeInGroup(group, nodename);

                        switch (result)
                        {
                            case 0:
                                Console.WriteLine($"Successfully activated {nodename} in {group}.");
                                break;
                            case 1:
                                Console.WriteLine($"{nodename} in {group} is already activated.");
                                break;
                            case -1:
                                Console.WriteLine($"Error: {nodename} is not found in {group}.");
                                break;
                            case -2:
                                Console.WriteLine($"Error: {group} is not found.");
                                break;
                        }
                    }

                    await Nodes.SaveNodesAsync(nodes);
                });

            nodeDeactivateCommand.AddAlias("disable");
            nodeDeactivateCommand.AddAlias("hide");
            nodeDeactivateCommand.AddArgument(new Argument<string>("group", "Target group."));
            nodeDeactivateCommand.AddArgument(new Argument<string[]>("nodenames", "Nodes to deactivate."));
            nodeDeactivateCommand.AddOption(new Option<bool>(new string[] { "-a", "--all", "--all-nodes" }, "Deactivate all nodes in target group."));
            nodeDeactivateCommand.Handler = CommandHandler.Create(
                async (string group, string[] nodenames, bool allNodes) =>
                {
                    nodes = await loadNodesTask;

                    if (allNodes)
                    {
                        var result = nodes.DeactivateAllNodesInGroup(group);

                        switch (result)
                        {
                            case 0:
                                Console.WriteLine($"Successfully deactivated all nodes in {group}");
                                break;
                            case -2:
                                Console.WriteLine($"Error: {group} is not found.");
                                break;
                        }
                    }

                    foreach (var nodename in nodenames)
                    {
                        var result = nodes.DeactivateNodeInGroup(group, nodename);

                        switch (result)
                        {
                            case 0:
                                Console.WriteLine($"Successfully deactivated {nodename} in {group}.");
                                break;
                            case 1:
                                Console.WriteLine($"{nodename} in {group} is already deactivated.");
                                break;
                            case -1:
                                Console.WriteLine($"Error: {nodename} is not found in {group}.");
                                break;
                            case -2:
                                Console.WriteLine($"Error: {group} is not found.");
                                break;
                        }
                    }

                    await Nodes.SaveNodesAsync(nodes);
                });

            groupAddUserCommand.AddAlias("au");
            groupAddUserCommand.AddArgument(new Argument<string>("group", "Target group."));
            groupAddUserCommand.AddArgument(new Argument<string[]>("usernames", "Users to add."));
            groupAddUserCommand.AddOption(new Option<bool>(new string[] { "-a", "--all", "--all-users" }, "Add all users to target group."));
            groupAddUserCommand.Handler = CommandHandler.Create(
                async (string group, string[] usernames, bool allUsers) =>
                {
                    users = await loadUsersTask;
                    nodes = await loadNodesTask;

                    if (!nodes.Groups.ContainsKey(group))
                    {
                        Console.WriteLine($"Group not found: {group}");
                        return;
                    }

                    if (allUsers)
                    {
                        foreach (var userEntry in users.UserDict)
                        {
                            var username = userEntry.Key;
                            var user = userEntry.Value;

                            var result = user.AddToGroup(group);
                            if (result)
                                Console.WriteLine($"Successfully added {username} to {group}.");
                            else
                                Console.WriteLine($"User {username} is already in group {group}.");
                        }
                    }

                    foreach (var username in usernames)
                    {
                        var result = users.AddUserToGroup(username, group);

                        switch (result)
                        {
                            case 0:
                                Console.WriteLine($"Successfully added {username} to {group}.");
                                break;
                            case 1:
                                Console.WriteLine($"User {username} is already in group {group}.");
                                break;
                            case -1:
                                Console.WriteLine("User not found: {username}.");
                                break;
                            default:
                                Console.WriteLine($"Unknown error: {result}.");
                                break;
                        }
                    }

                    await Users.SaveUsersAsync(users);
                });

            groupRemoveUserCommand.AddAlias("ru");
            groupRemoveUserCommand.AddArgument(new Argument<string>("group", "Target group."));
            groupRemoveUserCommand.AddArgument(new Argument<string[]>("usernames", "Members to remove."));
            groupRemoveUserCommand.AddOption(new Option<bool>(new string[] { "-a", "--all", "--all-users" }, "Remove all members of target group."));
            groupRemoveUserCommand.Handler = CommandHandler.Create(
                async (string group, string[] usernames, bool allUsers) =>
                {
                    users = await loadUsersTask;
                    nodes = await loadNodesTask;

                    if (!nodes.Groups.ContainsKey(group))
                    {
                        Console.WriteLine($"Group not found: {group}");
                        return;
                    }

                    if (allUsers)
                    {
                        foreach (var userEntry in users.UserDict)
                            userEntry.Value.RemoveFromGroup(group);
                    }

                    foreach (var username in usernames)
                    {
                        var result = users.RemoveUserFromGroup(username, group);

                        if (result == 1)
                            Console.WriteLine($"User {username} is not in group {group}.");
                        else if (result == -2)
                            Console.WriteLine($"User not found: {username}");
                    }

                    await Users.SaveUsersAsync(users);
                });

            groupListUsersCommand.AddAlias("lu");
            groupListUsersCommand.AddArgument(new Argument<string>("group", "Target group."));
            groupListUsersCommand.Handler = CommandHandler.Create(
                async (string group) =>
                {
                    users = await loadUsersTask;

                    var maxUsernameLength = users.UserDict.Select(x => x.Key.Length)
                                                          .DefaultIfEmpty()
                                                          .Max();
                    var maxPasswordLength = users.UserDict.SelectMany(x => x.Value.Credentials.Values)
                                                          .Select(x => x?.Password.Length ?? 0)
                                                          .DefaultIfEmpty()
                                                          .Max();
                    var usernameFieldWidth = maxUsernameLength > 4 ? maxUsernameLength + 2 : 6;
                    var passwordFieldWidth = maxPasswordLength > 8 ? maxPasswordLength + 2 : 10;

                    Console.WriteLine($"{"Group",-16}{group,-32}");
                    Console.WriteLine();

                    Utilities.PrintTableBorder(usernameFieldWidth, 24, passwordFieldWidth);
                    Console.WriteLine($"|{"User".PadRight(usernameFieldWidth)}|{"Method",-24}|{"Password".PadRight(passwordFieldWidth)}|");
                    Utilities.PrintTableBorder(usernameFieldWidth, 24, passwordFieldWidth);

                    foreach (var user in users.UserDict)
                    {
                        if (user.Value.Credentials.TryGetValue(group, out var cred))
                        {
                            Console.WriteLine($"|{user.Key.PadRight(usernameFieldWidth)}|{cred?.Method,-24}|{(cred?.Password ?? string.Empty).PadRight(passwordFieldWidth)}|");
                        }
                    }

                    Utilities.PrintTableBorder(usernameFieldWidth, 24, passwordFieldWidth);
                }
            );

            groupGetDataUsageCommand.AddAlias("data");
            groupGetDataUsageCommand.AddArgument(new Argument<string>("group", "Target group."));
            groupGetDataUsageCommand.AddOption(new Option<SortBy?>("--sort-by", "Sort rule for data usage records."));
            groupGetDataUsageCommand.Handler = CommandHandler.Create(
                async (string group, SortBy? sortBy) =>
                {
                    users = await loadUsersTask;
                    nodes = await loadNodesTask;
                    settings = await loadSettingsTask;

                    var records = nodes.GetGroupDataUsage(group);
                    var maxNameLength = records.Select(x => x.username.Length)
                                               .DefaultIfEmpty()
                                               .Max();
                    var nameFieldWidth = maxNameLength > 4 ? maxNameLength + 2 : 6;

                    var sortByInEffect = settings.GroupDataUsageDefaultSortBy;
                    if (sortBy is SortBy currentRunSortBy)
                        sortByInEffect = currentRunSortBy;
                    switch (sortByInEffect)
                    {
                        case SortBy.DefaultAscending:
                            break;
                        case SortBy.DefaultDescending:
                            records.Reverse();
                            break;
                        case SortBy.NameAscending:
                            records = records.OrderBy(x => x.username).ToList();
                            break;
                        case SortBy.NameDescending:
                            records = records.OrderByDescending(x => x.username).ToList();
                            break;
                        case SortBy.DataUsedAscending:
                            records = records.OrderBy(x => x.bytesUsed).ToList();
                            break;
                        case SortBy.DataUsedDescending:
                            records = records.OrderByDescending(x => x.bytesUsed).ToList();
                            break;
                        case SortBy.DataRemainingAscending:
                            records = records.OrderBy(x => x.bytesRemaining).ToList();
                            break;
                        case SortBy.DataRemainingDescending:
                            records = records.OrderByDescending(x => x.bytesRemaining).ToList();
                            break;
                    }

                    Console.WriteLine($"{"Group",-16}{group,-32}");
                    if (nodes.Groups.TryGetValue(group, out var targetGroup))
                    {
                        Console.WriteLine($"{"Data used",-16}{Utilities.HumanReadableDataString(targetGroup.BytesUsed),-32}");
                        if (targetGroup.BytesRemaining != 0UL)
                            Console.WriteLine($"{"Data remaining",-16}{Utilities.HumanReadableDataString(targetGroup.BytesRemaining),-32}");
                        if (targetGroup.DataLimitInBytes != 0UL)
                            Console.WriteLine($"{"Data limit",-16}{Utilities.HumanReadableDataString(targetGroup.DataLimitInBytes),-32}");
                    }

                    Console.WriteLine();

                    Utilities.PrintTableBorder(nameFieldWidth, 11, 16);

                    Console.WriteLine($"|{"User".PadRight(nameFieldWidth)}|{"Data Used",11}|{"Data Remaining",16}|");

                    Utilities.PrintTableBorder(nameFieldWidth, 11, 16);

                    foreach (var (username, bytesUsed, bytesRemaining) in records)
                    {
                        Console.Write($"|{username.PadRight(nameFieldWidth)}|{Utilities.HumanReadableDataString(bytesUsed),11}|");
                        if (bytesRemaining != 0UL)
                            Console.WriteLine($"{Utilities.HumanReadableDataString(bytesRemaining),16}|");
                        else
                            Console.WriteLine($"{string.Empty,16}|");
                    }

                    Utilities.PrintTableBorder(nameFieldWidth, 11, 16);
                });

            groupSetDataLimitCommand.AddAlias("limit");
            groupSetDataLimitCommand.AddArgument(new Argument<string>("dataLimit", "The data limit in bytes. Examples: '1024', '2K', '4M', '8G', '16T', '32P'."));
            groupSetDataLimitCommand.AddArgument(groupsArgument);
            groupSetDataLimitCommand.AddOption(new Option<bool>("--global", "Set the global data limit of the group."));
            groupSetDataLimitCommand.AddOption(new Option<bool>("--per-user", "Set the same data limit for each user."));
            groupSetDataLimitCommand.AddOption(new Option<string[]?>("--usernames", "Only set the data limit to these users."));
            groupSetDataLimitCommand.Handler = CommandHandler.Create(
                async (string dataLimit, string[] groups, bool global, bool perUser, string[]? usernames) =>
                {
                    users = await loadUsersTask;
                    nodes = await loadNodesTask;

                    if (Utilities.TryParseDataLimitString(dataLimit, out var dataLimitInBytes))
                        foreach (var group in groups)
                        {
                            var result = nodes.SetDataLimitForGroup(dataLimitInBytes, group, global, perUser, usernames);
                            if (result == -1)
                                Console.WriteLine($"Group not found: {group}.");
                            else if (result == -2)
                                Console.WriteLine($"An error occurred while setting for {group}: some users were not found.");
                        }
                    else
                        Console.WriteLine($"An error occurred while parsing the data limit: {dataLimit}");

                    await Users.SaveUsersAsync(users);
                });

            onlineConfigGenerateCommand.AddAlias("g");
            onlineConfigGenerateCommand.AddAlias("gen");
            onlineConfigGenerateCommand.AddArgument(new Argument<string[]?>("usernames", "Specify users to generate for. Leave empty for all users."));
            onlineConfigGenerateCommand.Handler = CommandHandler.Create(
                async (string[]? usernames) =>
                {
                    users = await loadUsersTask;
                    nodes = await loadNodesTask;
                    settings = await loadSettingsTask;
                    int result;
                    if (usernames == null)
                        result = await OnlineConfig.GenerateAndSave(users, nodes, settings);
                    else
                        result = await OnlineConfig.GenerateAndSave(users, nodes, settings, usernames);
                    if (result == 404)
                        Console.WriteLine($"One or more specified users are not found.");
                });

            onlineConfigGetLinkCommand.AddAlias("l");
            onlineConfigGetLinkCommand.AddAlias("link");
            onlineConfigGetLinkCommand.AddAlias("links");
            onlineConfigGetLinkCommand.AddAlias("url");
            onlineConfigGetLinkCommand.AddAlias("urls");
            onlineConfigGetLinkCommand.AddArgument(new Argument<string[]?>("usernames", "Target users. Leave empty for all users."));
            onlineConfigGetLinkCommand.Handler = CommandHandler.Create(
                async (string[]? usernames) =>
                {
                    users = await loadUsersTask;
                    settings = await loadSettingsTask;

                    if (usernames == null)
                        foreach (var userEntry in users.UserDict)
                        {
                            var username = userEntry.Key;
                            var user = userEntry.Value;
                            PrintUserLinks(username, user, settings);
                        }
                    else
                        foreach (var username in usernames)
                            if (users.UserDict.TryGetValue(username, out User? user))
                                PrintUserLinks(username, user, settings);
                            else
                                Console.WriteLine($"User not found: {username}.");

                    static void PrintUserLinks(string username, User user, Settings settings)
                    {
                        Console.WriteLine($"{"User",-8}{username,-32}");
                        Console.WriteLine();
                        Console.WriteLine($"{settings.OnlineConfigDeliveryRootUri}/{user.Uuid}.json");
                        if (settings.OnlineConfigDeliverByGroup)
                            foreach (var group in user.Credentials.Keys)
                                Console.WriteLine($"{settings.OnlineConfigDeliveryRootUri}/{user.Uuid}/{Uri.EscapeDataString(group)}.json");
                        Console.WriteLine();
                    }
                });

            onlineConfigCleanCommand.AddAlias("c");
            onlineConfigCleanCommand.AddAlias("clear");
            onlineConfigCleanCommand.AddArgument(new Argument<string[]?>("usernames", "Specify users to clean online configuration files for."));
            onlineConfigCleanCommand.AddOption(new Option<bool>("--all", "Clean for all users."));
            onlineConfigCleanCommand.Handler = CommandHandler.Create(
                async (string[]? usernames, bool all) =>
                {
                    users = await loadUsersTask;
                    settings = await loadSettingsTask;
                    if (usernames != null && !all)
                        OnlineConfig.Remove(users, settings, usernames);
                    else if (usernames == null && all)
                        OnlineConfig.Remove(users, settings);
                    else
                        Console.WriteLine("Invalid arguments or options. Either specify usernames, or use '--all' to target all users.");
                });

            outlineServerAddCommand.AddAlias("a");
            outlineServerAddCommand.AddArgument(new Argument<string>("group", "Specify a group to add the Outline server to."));
            outlineServerAddCommand.AddArgument(new Argument<string>("apiKey", "The Outline server API key."));
            outlineServerAddCommand.Handler = CommandHandler.Create(
                async (string group, string apiKey, CancellationToken cancellationToken) =>
                {
                    nodes = await loadNodesTask;
                    settings = await loadSettingsTask;

                    if (string.IsNullOrEmpty(apiKey))
                        Console.WriteLine("You must specify an API key.");

                    int result;

                    try
                    {
                        if (settings.OutlineServerApplyDefaultUserOnAssociation)
                            result = await nodes.AssociateOutlineServerWithGroup(group, apiKey, settings.OutlineServerGlobalDefaultUser, cancellationToken);
                        else
                            result = await nodes.AssociateOutlineServerWithGroup(group, apiKey, null, cancellationToken);
                    }
                    catch (OperationCanceledException ex) when (ex is not TaskCanceledException)
                    {
                        Console.WriteLine(ex.Message);
                        result = -4;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        result = -3;
                    }

                    switch (result)
                    {
                        case 0:
                            Console.WriteLine($"Successfully associated the Outline server with {group}");
                            break;
                        case -1:
                            Console.WriteLine($"Group not found: {group}");
                            break;
                        case -2:
                            Console.WriteLine($"Invalid API key: {apiKey}");
                            break;
                        case -3:
                            Console.WriteLine($"An error occurred while applying the global default user setting.");
                            break;
                    }

                    await Nodes.SaveNodesAsync(nodes);
                });

            outlineServerGetCommand.AddArgument(new Argument<string>("group", "The associated group."));
            outlineServerGetCommand.Handler = CommandHandler.Create(
                async (string group) =>
                {
                    nodes = await loadNodesTask;

                    var outlineApiKeyString = nodes.GetOutlineApiKeyStringFromGroup(group);
                    var outlineServerInfo = nodes.GetOutlineServerInfoFromGroup(group);
                    if (outlineApiKeyString != null && outlineServerInfo != null)
                    {
                        Console.WriteLine($"{"Name",-32}{outlineServerInfo.Name,-36}");
                        Console.WriteLine($"{"ID",-32}{outlineServerInfo.ServerId,-36}");
                        Console.WriteLine($"{"API key",-32}{outlineApiKeyString,-36}");
                        Console.WriteLine($"{"Date created",-32}{outlineServerInfo.CreatedTimestampMs,-36}");
                        Console.WriteLine($"{"Version",-32}{outlineServerInfo.Version,-36}");
                        Console.WriteLine($"{"Hostname",-32}{outlineServerInfo.HostnameForAccessKeys,-36}");
                        Console.WriteLine($"{"Port for new keys",-32}{outlineServerInfo.PortForNewAccessKeys,-36}");
                        Console.WriteLine($"{"Telemetry enabled",-32}{outlineServerInfo.MetricsEnabled,-36}");
                    }

                    await Nodes.SaveNodesAsync(nodes);
                });

            outlineServerSetCommand.AddArgument(new Argument<string>("group", "The associated group."));
            outlineServerSetCommand.AddOption(new Option<string?>("--name", "Name of the Outline server."));
            outlineServerSetCommand.AddOption(new Option<string?>("--hostname", "Hostname of the Outline server."));
            outlineServerSetCommand.AddOption(new Option<int?>("--port", "Port number for new access keys on the Outline server."));
            outlineServerSetCommand.AddOption(new Option<bool?>("--metrics", "Enable or disable telemetry on the Outline server."));
            outlineServerSetCommand.AddOption(new Option<string?>("--default-user", "The default user for Outline server's default access key (id: 0)."));
            outlineServerSetCommand.Handler = CommandHandler.Create(
                async (string group, string? name, string? hostname, int? port, bool? metrics, string? defaultUser, CancellationToken cancellationToken) =>
                {
                    nodes = await loadNodesTask;

                    try
                    {
                        var statusCodes = await nodes.SetOutlineServerInGroup(group, name, hostname, port, metrics, defaultUser, cancellationToken);
                        if (statusCodes != null)
                        {
                            foreach (var statusCode in statusCodes)
                                if (statusCode != System.Net.HttpStatusCode.NoContent)
                                    Console.WriteLine($"{statusCode:D} {statusCode:G}");
                        }
                        else
                            Console.WriteLine("Group not found or no associated Outline server.");
                    }
                    catch (OperationCanceledException ex) when (ex is not TaskCanceledException)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"An error occurred while applying settings to Outline servers.");
                        Console.WriteLine(ex.Message);
                    }

                    await Nodes.SaveNodesAsync(nodes);
                });

            outlineServerRemoveCommand.AddAlias("rm");
            outlineServerRemoveCommand.AddArgument(groupsArgument);
            outlineServerRemoveCommand.AddOption(new Option<bool>("--remove-creds", "Remove credentials from all associated users."));
            outlineServerRemoveCommand.Handler = CommandHandler.Create(
                async (string[] groups, bool removeCreds) =>
                {
                    users = await loadUsersTask;
                    nodes = await loadNodesTask;

                    foreach (var group in groups)
                        if (nodes.RemoveOutlineServerFromGroup(group) != 0)
                            Console.WriteLine($"Group not found: {group}");

                    if (removeCreds)
                        users.RemoveCredentialsFromAllUsers(groups);

                    await Users.SaveUsersAsync(users);
                    await Nodes.SaveNodesAsync(nodes);
                });

            outlineServerUpdateCommand.AddAlias("pull");
            outlineServerUpdateCommand.AddArgument(new Argument<string[]?>("groups", "Specify groups to update for."));
            outlineServerUpdateCommand.AddOption(new Option<bool>("--no-sync", "Do not update local user credential storage from retrieved access key list."));
            outlineServerUpdateCommand.Handler = CommandHandler.Create(
                async (string[]? groups, bool noSync, CancellationToken cancellationToken) =>
                {
                    nodes = await loadNodesTask;
                    users = await loadUsersTask;

                    try
                    {
                        if (groups == null)
                            await nodes.UpdateOutlineServerForAllGroups(users, !noSync, cancellationToken);
                        else
                            foreach (var group in groups)
                            {
                                var result = await nodes.UpdateGroupOutlineServer(group, users, !noSync, cancellationToken);
                                if (result == 0)
                                {
                                }
                                else if (result == -1)
                                    Console.WriteLine($"Group not found: {group}");
                                else if (result == -2)
                                    Console.WriteLine($"Group not associated with an Outline server: {group}");
                            }
                    }
                    catch (OperationCanceledException ex) when (ex is not TaskCanceledException)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"An error occurred while updating from Outline servers.");
                        Console.WriteLine(ex.Message);
                    }

                    await Users.SaveUsersAsync(users);
                    await Nodes.SaveNodesAsync(nodes);
                });

            outlineServerDeployCommand.AddArgument(new Argument<string[]?>("groups", "Groups to deploy for."));
            outlineServerDeployCommand.Handler = CommandHandler.Create(
                async (string[]? groups, CancellationToken cancellationToken) =>
                {
                    users = await loadUsersTask;
                    nodes = await loadNodesTask;

                    try
                    {
                        if (groups == null)
                            await nodes.DeployAllOutlineServers(users, cancellationToken);
                        else
                        {
                            var tasks = groups.Select(async x => await nodes.DeployGroupOutlineServer(x, users, cancellationToken));
                            var results = await Task.WhenAll(tasks);
                            foreach (var result in results)
                                if (result == 0)
                                    Console.WriteLine("Success.");
                                else if (result == -1)
                                    Console.WriteLine("Target group doesn't exist.");
                                else if (result == -2)
                                    Console.WriteLine("No associated Outline server.");
                        }
                    }
                    catch (OperationCanceledException ex) when (ex is not TaskCanceledException)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"An error occurred while deploying Outline servers.");
                        Console.WriteLine(ex.Message);
                    }

                    await Users.SaveUsersAsync(users);
                    await Nodes.SaveNodesAsync(nodes);
                });

            outlineServerRotatePasswordCommand.AddAlias("rotate");
            outlineServerRotatePasswordCommand.AddOption(new Option<string[]?>("--usernames", "Target users."));
            outlineServerRotatePasswordCommand.AddOption(new Option<string[]?>("--groups", "Target groups."));
            outlineServerRotatePasswordCommand.Handler = CommandHandler.Create(
                async (string[]? usernames, string[]? groups, CancellationToken cancellationToken) =>
                {
                    users = await loadUsersTask;
                    nodes = await loadNodesTask;

                    try
                    {
                        if (groups != null)
                        {
                            var tasks = groups.Select(async x => await nodes.RotateGroupPassword(x, users, cancellationToken, usernames));
                            await Task.WhenAll(tasks);
                        }
                        else if (usernames != null)
                        {
                            var targetGroups = usernames.Where(x => users.UserDict.ContainsKey(x))
                                                        .SelectMany(x => users.UserDict[x].Credentials.Keys)
                                                        .Distinct();
                            var tasks = targetGroups.Select(async x => await nodes.RotateGroupPassword(x, users, cancellationToken, usernames));
                            await Task.WhenAll(tasks);
                        }
                        else
                            Console.WriteLine("Please provide either a username or a group, or both.");
                    }
                    catch (OperationCanceledException ex) when (ex is not TaskCanceledException)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"An error occurred while connecting to Outline servers.");
                        Console.WriteLine(ex.Message);
                    }

                    await Users.SaveUsersAsync(users);
                    await Nodes.SaveNodesAsync(nodes);
                });

            reportCommand.AddOption(new Option<SortBy?>("--group-sort-by", "Sort rule for group data usage records."));
            reportCommand.AddOption(new Option<SortBy?>("--user-sort-by", "Sort rule for user data usage records."));
            reportCommand.Handler = CommandHandler.Create(
                async (SortBy? groupSortBy, SortBy? userSortBy) =>
                {
                    users = await loadUsersTask;
                    nodes = await loadNodesTask;
                    settings = await loadSettingsTask;

                    // collect data
                    var totalBytesUsed = nodes.Groups.Select(x => x.Value.BytesUsed).Aggregate(0UL, (x, y) => x + y);
                    var totalBytesRemaining = nodes.Groups.Select(x => x.Value.BytesRemaining).Aggregate(0UL, (x, y) => x + y);
                    var recordsByGroup = nodes.GetDataUsageByGroup();
                    var recordsByUser = users.GetDataUsageByUser(nodes);

                    // calculate column width
                    var maxGroupNameLength = recordsByGroup.Select(x => x.group.Length)
                                                           .DefaultIfEmpty()
                                                           .Max();
                    var groupNameFieldWidth = maxGroupNameLength > 5 ? maxGroupNameLength + 2 : 7;
                    var maxUsernameLength = recordsByUser.Select(x => x.username.Length)
                                                         .DefaultIfEmpty()
                                                         .Max();
                    var usernameFieldWidth = maxUsernameLength > 4 ? maxUsernameLength + 2 : 6;

                    // sort
                    var groupSortByInEffect = settings.GroupDataUsageDefaultSortBy;
                    if (groupSortBy is SortBy currentRunGroupSortBy)
                        groupSortByInEffect = currentRunGroupSortBy;
                    switch (groupSortByInEffect)
                    {
                        case SortBy.DefaultAscending:
                            break;
                        case SortBy.DefaultDescending:
                            recordsByGroup.Reverse();
                            break;
                        case SortBy.NameAscending:
                            recordsByGroup = recordsByGroup.OrderBy(x => x.group).ToList();
                            break;
                        case SortBy.NameDescending:
                            recordsByGroup = recordsByGroup.OrderByDescending(x => x.group).ToList();
                            break;
                        case SortBy.DataUsedAscending:
                            recordsByGroup = recordsByGroup.OrderBy(x => x.bytesUsed).ToList();
                            break;
                        case SortBy.DataUsedDescending:
                            recordsByGroup = recordsByGroup.OrderByDescending(x => x.bytesUsed).ToList();
                            break;
                        case SortBy.DataRemainingAscending:
                            recordsByGroup = recordsByGroup.OrderBy(x => x.bytesRemaining).ToList();
                            break;
                        case SortBy.DataRemainingDescending:
                            recordsByGroup = recordsByGroup.OrderByDescending(x => x.bytesRemaining).ToList();
                            break;
                    }
                    var userSortByInEffect = settings.UserDataUsageDefaultSortBy;
                    if (userSortBy is SortBy currentRunUserSortBy)
                        userSortByInEffect = currentRunUserSortBy;
                    switch (userSortByInEffect)
                    {
                        case SortBy.DefaultAscending:
                            break;
                        case SortBy.DefaultDescending:
                            recordsByUser.Reverse();
                            break;
                        case SortBy.NameAscending:
                            recordsByUser = recordsByUser.OrderBy(x => x.username).ToList();
                            break;
                        case SortBy.NameDescending:
                            recordsByUser = recordsByUser.OrderByDescending(x => x.username).ToList();
                            break;
                        case SortBy.DataUsedAscending:
                            recordsByUser = recordsByUser.OrderBy(x => x.bytesUsed).ToList();
                            break;
                        case SortBy.DataUsedDescending:
                            recordsByUser = recordsByUser.OrderByDescending(x => x.bytesUsed).ToList();
                            break;
                        case SortBy.DataRemainingAscending:
                            recordsByUser = recordsByUser.OrderBy(x => x.bytesRemaining).ToList();
                            break;
                        case SortBy.DataRemainingDescending:
                            recordsByUser = recordsByUser.OrderByDescending(x => x.bytesRemaining).ToList();
                            break;
                    }

                    // total
                    Console.WriteLine("In the last 30 days");
                    Console.WriteLine();
                    if (totalBytesUsed != 0UL)
                        Console.WriteLine($"{"Total data used",-24}{Utilities.HumanReadableDataString(totalBytesUsed)}");
                    if (totalBytesRemaining != 0UL)
                        Console.WriteLine($"{"Total data remaining",-24}{Utilities.HumanReadableDataString(totalBytesRemaining)}");
                    Console.WriteLine();

                    // by group
                    Console.WriteLine("Data usage by group");
                    Utilities.PrintTableBorder(groupNameFieldWidth, 11, 16);
                    Console.WriteLine($"|{"Group".PadRight(groupNameFieldWidth)}|{"Data Used",11}|{"Data Remaining",16}|");
                    Utilities.PrintTableBorder(groupNameFieldWidth, 11, 16);
                    foreach (var (group, bytesUsed, bytesRemaining) in recordsByGroup)
                    {
                        Console.Write($"|{group.PadRight(groupNameFieldWidth)}|");
                        if (bytesUsed != 0UL)
                            Console.Write($"{Utilities.HumanReadableDataString(bytesUsed),11}|");
                        else
                            Console.Write($"{string.Empty,11}|");
                        if (bytesRemaining != 0UL)
                            Console.WriteLine($"{Utilities.HumanReadableDataString(bytesRemaining),16}|");
                        else
                            Console.WriteLine($"{string.Empty,16}|");
                    }
                    Utilities.PrintTableBorder(groupNameFieldWidth, 11, 16);
                    Console.WriteLine();

                    // by user
                    Console.WriteLine("Data usage by user");
                    Utilities.PrintTableBorder(usernameFieldWidth, 11, 16);
                    Console.WriteLine($"|{"User".PadRight(usernameFieldWidth)}|{"Data Used",11}|{"Data Remaining",16}|");
                    Utilities.PrintTableBorder(usernameFieldWidth, 11, 16);
                    foreach (var (username, bytesUsed, bytesRemaining) in recordsByUser)
                    {
                        Console.Write($"|{username.PadRight(usernameFieldWidth)}|");
                        if (bytesUsed != 0UL)
                            Console.Write($"{Utilities.HumanReadableDataString(bytesUsed),11}|");
                        else
                            Console.Write($"{string.Empty,11}|");
                        if (bytesRemaining != 0UL)
                            Console.WriteLine($"{Utilities.HumanReadableDataString(bytesRemaining),16}|");
                        else
                            Console.WriteLine($"{string.Empty,16}|");
                    }
                    Utilities.PrintTableBorder(usernameFieldWidth, 11, 16);
                });

            settingsGetCommand.Handler = CommandHandler.Create(
                async () =>
                {
                    settings = await loadSettingsTask;

                    Utilities.PrintTableBorder(42, 40);
                    Console.WriteLine($"|{"Key",-42}|{"Value",40}|");
                    Utilities.PrintTableBorder(42, 40);

                    Console.WriteLine($"|{"Version",-42}|{settings.Version,40}|");
                    Console.WriteLine($"|{"UserDataUsageDefaultSortBy",-42}|{settings.UserDataUsageDefaultSortBy,40}|");
                    Console.WriteLine($"|{"GroupDataUsageDefaultSortBy",-42}|{settings.GroupDataUsageDefaultSortBy,40}|");
                    Console.WriteLine($"|{"OnlineConfigSortByName",-42}|{settings.OnlineConfigSortByName,40}|");
                    Console.WriteLine($"|{"OnlineConfigDeliverByGroup",-42}|{settings.OnlineConfigDeliverByGroup,40}|");
                    Console.WriteLine($"|{"OnlineConfigCleanOnUserRemoval",-42}|{settings.OnlineConfigCleanOnUserRemoval,40}|");
                    Console.WriteLine($"|{"OnlineConfigUpdateDataUsageOnGeneration",-42}|{settings.OnlineConfigUpdateDataUsageOnGeneration,40}|");
                    Console.WriteLine($"|{"OnlineConfigOutputDirectory",-42}|{settings.OnlineConfigOutputDirectory,40}|");
                    Console.WriteLine($"|{"OnlineConfigDeliveryRootUri",-42}|{settings.OnlineConfigDeliveryRootUri,40}|");
                    Console.WriteLine($"|{"OutlineServerDeployOnChange",-42}|{settings.OutlineServerDeployOnChange,40}|");
                    Console.WriteLine($"|{"OutlineServerApplyDefaultUserOnAssociation",-42}|{settings.OutlineServerApplyDefaultUserOnAssociation,40}|");
                    Console.WriteLine($"|{"OutlineServerGlobalDefaultUser",-42}|{settings.OutlineServerGlobalDefaultUser,40}|");

                    Utilities.PrintTableBorder(42, 40);
                });

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
            settingsSetCommand.Handler = CommandHandler.Create(
                async (SortBy? userDataUsageDefaultSortBy, SortBy? groupDataUsageDefaultSortBy, bool? onlineConfigSortByName, bool? onlineConfigDeliverByGroup, bool? onlineConfigCleanOnUserRemoval, bool? onlineConfigUpdateDataUsageOnGeneration, string? onlineConfigOutputDirectory, string? onlineConfigDeliveryRootUri, bool? outlineServerDeployOnChange, bool? outlineServerApplyDefaultUserOnAssociation, string? outlineServerGlobalDefaultUser) =>
                {
                    settings = await loadSettingsTask;

                    if (userDataUsageDefaultSortBy is SortBy userSortBy)
                        settings.UserDataUsageDefaultSortBy = userSortBy;
                    if (groupDataUsageDefaultSortBy is SortBy groupSortBy)
                        settings.GroupDataUsageDefaultSortBy = groupSortBy;
                    if (onlineConfigSortByName is bool sortByName)
                        settings.OnlineConfigSortByName = sortByName;
                    if (onlineConfigDeliverByGroup is bool deliverByGroup)
                        settings.OnlineConfigDeliverByGroup = deliverByGroup;
                    if (onlineConfigCleanOnUserRemoval is bool cleanOnUserRemoval)
                        settings.OnlineConfigCleanOnUserRemoval = cleanOnUserRemoval;
                    if (onlineConfigUpdateDataUsageOnGeneration is bool updateDataUsageOnGeneration)
                        settings.OnlineConfigUpdateDataUsageOnGeneration = updateDataUsageOnGeneration;
                    if (!string.IsNullOrEmpty(onlineConfigOutputDirectory))
                        settings.OnlineConfigOutputDirectory = onlineConfigOutputDirectory;
                    if (!string.IsNullOrEmpty(onlineConfigDeliveryRootUri))
                        settings.OnlineConfigDeliveryRootUri = onlineConfigDeliveryRootUri;
                    if (outlineServerDeployOnChange is bool deployOnChange)
                        settings.OutlineServerDeployOnChange = deployOnChange;
                    if (outlineServerApplyDefaultUserOnAssociation is bool applyDefaultUserOnAssociation)
                        settings.OutlineServerApplyDefaultUserOnAssociation = applyDefaultUserOnAssociation;
                    if (outlineServerGlobalDefaultUser != null)
                        settings.OutlineServerGlobalDefaultUser = outlineServerGlobalDefaultUser;

                    await Settings.SaveSettingsAsync(settings);
                });

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

                        // Reload JSON before each run
                        loadUsersTask = Users.LoadUsersAsync();
                        loadNodesTask = Nodes.LoadNodesAsync();
                        loadSettingsTask = Settings.LoadSettingsAsync();

                        await rootCommand.InvokeAsync(inputLine);

                        // Dispose nodes
                        nodes = await loadNodesTask;
                        nodes.Dispose();
                    }
                });

            serviceCommand.AddOption(new Option<int>("--interval", () => 3600, "The interval between each scheduled run in seconds."));
            serviceCommand.AddOption(new Option<bool>("--pull-outline-server", "Pull from Outline servers for updates of server information, access keys, data usage."));
            serviceCommand.AddOption(new Option<bool>("--deploy-outline-server", "Deploy local configurations to Outline servers."));
            serviceCommand.AddOption(new Option<bool>("--generate-online-config", "Generate online config."));
            serviceCommand.AddOption(new Option<bool>("--regenerate-online-config", "Clean and regenerate online config."));
            serviceCommand.Handler = CommandHandler.Create(
                async (int interval, bool pullOutlineServer, bool deployOutlineServer, bool generateOnlineConfig, bool regenerateOnlineConfig, CancellationToken cancellationToken) =>
                {
                    if (interval < 60 || interval > int.MaxValue / 1000)
                    {
                        Console.WriteLine($"Interval can't be shorter than 60 seconds or longer than {int.MaxValue / 1000} seconds.");
                        return;
                    }

                    try
                    {
                        while (true)
                        {
                            if (pullOutlineServer)
                            {
                                await outlineServerUpdateCommand.InvokeAsync(Array.Empty<string>());
                                Console.WriteLine("Pulled from Outline servers.");
                            }
                            if (deployOutlineServer)
                            {
                                await outlineServerDeployCommand.InvokeAsync(Array.Empty<string>());
                                Console.WriteLine("Deployed to Outline servers.");
                            }
                            if (generateOnlineConfig)
                            {
                                await onlineConfigGenerateCommand.InvokeAsync(Array.Empty<string>());
                                Console.WriteLine("Generated online config.");
                            }
                            if (regenerateOnlineConfig)
                            {
                                await onlineConfigCleanCommand.InvokeAsync("--all");
                                Console.WriteLine("Cleaned online config.");
                                await onlineConfigGenerateCommand.InvokeAsync(Array.Empty<string>());
                                Console.WriteLine("Generated online config.");
                            }

                            // Dispose nodes
                            nodes = await loadNodesTask;
                            nodes.Dispose();

                            await Task.Delay(interval * 1000, cancellationToken);

                            // Reload JSON before each run
                            loadUsersTask = Users.LoadUsersAsync();
                            loadNodesTask = Nodes.LoadNodesAsync();
                            loadSettingsTask = Settings.LoadSettingsAsync();
                        }
                    }
                    catch (TaskCanceledException)
                    {
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"An error occurred while executing one of the scheduled tasks.");
                        Console.WriteLine(ex.Message);
                    }
                });

            Console.OutputEncoding = Encoding.UTF8;
            return rootCommand.InvokeAsync(args);
        }
    }
}
