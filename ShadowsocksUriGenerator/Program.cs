using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Text;
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
            var nodeRenameCommand = new Command("rename", "Renames an existing node with a new name.");
            var nodeRemoveCommand = new Command("remove", "Remove nodes from a group.");
            var nodeListCommand = new Command("list", "List nodes from the specified group or all groups.");

            var nodeCommand = new Command("node", "Manage nodes.")
            {
                nodeAddCommand,
                nodeRenameCommand,
                nodeRemoveCommand,
                nodeListCommand,
            };

            var groupAddCommand = new Command("add", "Add groups.");
            var groupRenameCommand = new Command("rename", "Renames an existing group with a new name.");
            var groupRemoveCommand = new Command("remove", "Remove groups and its nodes.");
            var groupListCommand = new Command("list", "List all groups.");
            var groupAddUserCommand = new Command("add-user", "Add users to the group.");
            var groupRemoveUserCommand = new Command("remove-user", "Remove users from the group.");
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

            var settingsGetCommand = new Command("get", "Get and print all settings.");
            var settingsSetCommand = new Command("set", "Change settings.");

            var settingsCommand = new Command("settings", "Manage settings.")
            {
                settingsGetCommand,
                settingsSetCommand,
            };

            var rootCommand = new RootCommand()
            {
                userCommand,
                nodeCommand,
                groupCommand,
                onlineConfigCommand,
                outlineServerCommand,
                settingsCommand,
            };

            userAddCommand.AddArgument(new Argument<string[]>("usernames", "A list of usernames to add."));
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

            groupAddCommand.AddArgument(new Argument<string[]>("groups", "A list of group names to add."));
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
                    var result = users.RenameUser(oldName, newName);
                    if (result == -1)
                        Console.WriteLine($"User not found: {oldName}");
                    else if (result == -2)
                        Console.WriteLine($"A user with the same name already exists: {newName}");
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
            userRemoveCommand.AddArgument(new Argument<string[]>("usernames", "A list of users to remove."));
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
            nodeRemoveCommand.AddArgument(new Argument<string>("group", "The group that the target node belongs to."));
            nodeRemoveCommand.AddArgument(new Argument<string[]>("nodenames", "A list of node names to remove."));
            nodeRemoveCommand.Handler = CommandHandler.Create(
                async (string group, string[] nodenames) =>
                {
                    nodes = await loadNodesTask;
                    if (nodes.RemoveNodesFromGroup(group, nodenames) == -1)
                        Console.WriteLine($"Group not found: {group}.");
                    await Nodes.SaveNodesAsync(nodes);
                });

            groupRemoveCommand.AddAlias("rm");
            groupRemoveCommand.AddArgument(new Argument<string[]>("groups", "A list of groups to remove."));
            groupRemoveCommand.Handler = CommandHandler.Create(
                async (string[] groups) =>
                {
                    users = await loadUsersTask;
                    nodes = await loadNodesTask;
                    users.RemoveCredentialsFromAllUsers(groups);
                    nodes.RemoveGroups(groups);
                    await Users.SaveUsersAsync(users);
                    await Nodes.SaveNodesAsync(nodes);
                });

            userListCommand.AddAlias("ls");
            userListCommand.Handler = CommandHandler.Create(
                async () =>
                {
                    users = await loadUsersTask;

                    PrintTableBorder(16, 36, 21);
                    Console.WriteLine($"|{"User",-16}|{"UUID",36}|{"Number of Credentials",21}|");
                    PrintTableBorder(16, 36, 21);

                    foreach (var user in users.UserDict)
                        Console.WriteLine($"|{user.Key,-16}|{user.Value.Uuid,36}|{user.Value.Credentials.Count,21}|");

                    PrintTableBorder(16, 36, 21);
                });

            nodeListCommand.AddAlias("ls");
            nodeListCommand.AddArgument(new Argument<string?>("group", getDefaultValue: () => null, "Target group. Leave empty for all groups."));
            nodeListCommand.Handler = CommandHandler.Create(
                async (string? group) =>
                {
                    nodes = await loadNodesTask;

                    PrintTableBorder(32, 16, 36, 24, 5, 12, 32);
                    Console.WriteLine($"|{"Node",-32}|{"Group",-16}|{"UUID",36}|{"Host",24}|{"Port",5}|{"Plugin",12}|{"Plugin Options",32}|");
                    PrintTableBorder(32, 16, 36, 24, 5, 12, 32);

                    if (string.IsNullOrEmpty(group))
                    {
                        foreach (var groupEntry in nodes.Groups)
                            foreach (var node in groupEntry.Value.NodeDict)
                                Console.WriteLine($"|{node.Key,-32}|{groupEntry.Key,-16}|{node.Value.Uuid,36}|{node.Value.Host,24}|{node.Value.Port,5}|{node.Value.Plugin,12}|{node.Value.PluginOpts,32}|");
                    }
                    else if (nodes.Groups.TryGetValue(group, out Group? targetGroup))
                    {
                        foreach (var node in targetGroup.NodeDict)
                            Console.WriteLine($"|{node.Key,-32}|{group,-16}|{node.Value.Uuid,36}|{node.Value.Host,24}|{node.Value.Port,5}|{node.Value.Plugin,12}|{node.Value.PluginOpts,32}|");
                    }
                    else
                        Console.WriteLine($"Group not found: {group}.");

                    PrintTableBorder(32, 16, 36, 24, 5, 12, 32);
                });

            groupListCommand.AddAlias("ls");
            groupListCommand.Handler = CommandHandler.Create(
                async () =>
                {
                    nodes = await loadNodesTask;

                    PrintTableBorder(16, 16, 16);
                    Console.WriteLine($"|{"Group",-16}|{"Number of Nodes",16}|{"Outline Server",16}|");
                    PrintTableBorder(16, 16, 16);

                    foreach (var group in nodes.Groups)
                    {
                        Console.WriteLine($"|{group.Key,-16}|{group.Value.NodeDict.Count,16}|{group.Value.OutlineServerInfo?.Name ?? "No",16}|");
                    }

                    PrintTableBorder(16, 16, 16);
                });

            userJoinGroupCommand.AddArgument(new Argument<string>("username", "The user that the credential belongs to."));
            userJoinGroupCommand.AddArgument(new Argument<string[]>("groups", "The group that the credential is for."));
            userJoinGroupCommand.Handler = CommandHandler.Create(
                async (string username, string[] groups) =>
                {
                    users = await loadUsersTask;
                    nodes = await loadNodesTask;

                    foreach (var group in groups)
                    {
                        if (!nodes.Groups.ContainsKey(group))
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
                                Console.WriteLine($"The user is already in the group.");
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
            userLeaveGroupCommand.AddArgument(new Argument<string[]>("groups", "A list of groups the credentials are for."));
            userLeaveGroupCommand.Handler = CommandHandler.Create(
                async (string username, string[] groups) =>
                {
                    users = await loadUsersTask;

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

            userRemoveCredentialCommand.AddArgument(new Argument<string>("username", "Target user."));
            userRemoveCredentialCommand.AddArgument(new Argument<string[]>("groups", "A list of groups the credentials are for."));
            userRemoveCredentialCommand.Handler = CommandHandler.Create(
                async (string username, string[] groups) =>
                {
                    users = await loadUsersTask;

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

            userListCredentialsCommand.Handler = CommandHandler.Create(
                async () =>
                {
                    users = await loadUsersTask;

                    PrintTableBorder(16, 16, 24, 32);
                    Console.WriteLine($"|{"User",-16}|{"Group",-16}|{"Method",-24}|{"Password",-32}|");
                    PrintTableBorder(16, 16, 24, 32);

                    foreach (var user in users.UserDict)
                    {
                        foreach (var credEntry in user.Value.Credentials)
                        {
                            if (credEntry.Value == null)
                                Console.WriteLine($"|{user.Key,-16}|{credEntry.Key,-16}|{string.Empty,-24}|{string.Empty,-32}|");
                            else
                                Console.WriteLine($"|{user.Key,-16}|{credEntry.Key,-16}|{credEntry.Value.Method,-24}|{credEntry.Value.Password,-32}|");
                        }
                    }

                    PrintTableBorder(16, 16, 24, 32);
                });

            userGetSSLinksCommand.AddAlias("ss");
            userGetSSLinksCommand.AddArgument(new Argument<string>("username", "Target user."));
            userGetSSLinksCommand.Handler = CommandHandler.Create(
                async (string username) =>
                {
                    users = await loadUsersTask;
                    nodes = await loadNodesTask;
                    var uris = users.GetUserSSUris(username, nodes);
                    foreach (var uri in uris)
                        Console.WriteLine($"{uri.AbsoluteUri}");
                });

            userGetDataUsageCommand.AddArgument(new Argument<string>("username", "Target user."));
            userGetDataUsageCommand.AddOption(new Option<SortBy?>("--sort-by", "Sort rule used for the data usage records."));
            userGetDataUsageCommand.Handler = CommandHandler.Create(
                async (string username, SortBy? sortBy) =>
                {
                    users = await loadUsersTask;
                    nodes = await loadNodesTask;
                    settings = await loadSettingsTask;

                    var records = users.GetUserDataUsage(username, nodes);

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

                    PrintTableBorder(32, 11, 16);

                    Console.WriteLine($"|{"Group",-32}|{"Data Used",11}|{"Data Remaining",16}|");

                    PrintTableBorder(32, 11, 16);

                    foreach (var (group, bytesUsed, bytesRemaining) in records)
                    {
                        Console.Write($"|{group,-32}|{Utilities.HumanReadableDataString(bytesUsed),11}|");
                        if (bytesRemaining != 0UL)
                            Console.WriteLine($"{Utilities.HumanReadableDataString(bytesRemaining),16}|");
                        else
                            Console.WriteLine($"{string.Empty,16}|");
                    }

                    PrintTableBorder(32, 11, 16);
                });

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
                            if (result == -1)
                                Console.WriteLine($"User not found: {username}.");
                            else if (result == -2)
                                Console.WriteLine($"An error occurred while setting for {username}: some groups were not found.");
                        }
                    else
                        Console.WriteLine($"An error occurred while parsing the data limit: {dataLimit}");

                    await Users.SaveUsersAsync(users);
                });

            groupAddUserCommand.AddArgument(new Argument<string>("group", "Target group."));
            groupAddUserCommand.AddArgument(new Argument<string[]>("usernames", "Users to add."));
            groupAddUserCommand.Handler = CommandHandler.Create(
                async (string group, string[] usernames) =>
                {
                    users = await loadUsersTask;
                    nodes = await loadNodesTask;

                    if (!nodes.Groups.ContainsKey(group))
                    {
                        Console.WriteLine($"Group not found: {group}");
                        return;
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
                                Console.WriteLine($"The user is already in the group.");
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

            groupRemoveUserCommand.AddArgument(new Argument<string>("group", "Target group."));
            groupRemoveUserCommand.AddArgument(new Argument<string[]>("usernames", "Users to remove."));
            groupRemoveUserCommand.Handler = CommandHandler.Create(
                async (string group, string[] usernames) =>
                {
                    users = await loadUsersTask;
                    nodes = await loadNodesTask;

                    if (!nodes.Groups.ContainsKey(group))
                    {
                        Console.WriteLine($"Group not found: {group}");
                        return;
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

            groupGetDataUsageCommand.AddArgument(new Argument<string>("group", "Target group."));
            groupGetDataUsageCommand.AddOption(new Option<SortBy?>("--sort-by", "Sort rule used for the data usage records."));
            groupGetDataUsageCommand.Handler = CommandHandler.Create(
                async (string group, SortBy? sortBy) =>
                {
                    users = await loadUsersTask;
                    nodes = await loadNodesTask;
                    settings = await loadSettingsTask;

                    var records = nodes.GetGroupDataUsage(group);

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

                    PrintTableBorder(32, 11, 16);

                    Console.WriteLine($"|{"User",-32}|{"Data Used",11}|{"Data Remaining",16}|");

                    PrintTableBorder(32, 11, 16);

                    foreach (var (username, bytesUsed, bytesRemaining) in records)
                    {
                        Console.Write($"|{username,-32}|{Utilities.HumanReadableDataString(bytesUsed),11}|");
                        if (bytesRemaining != 0UL)
                            Console.WriteLine($"{Utilities.HumanReadableDataString(bytesRemaining),16}|");
                        else
                            Console.WriteLine($"{string.Empty,16}|");
                    }

                    PrintTableBorder(32, 11, 16);
                });

            groupSetDataLimitCommand.AddArgument(new Argument<string>("dataLimit", "The data limit in bytes. Examples: '1024', '2K', '4M', '8G', '16T', '32P'."));
            groupSetDataLimitCommand.AddArgument(new Argument<string[]>("groups", "Target groups."));
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

            onlineConfigGetLinkCommand.AddAlias("link");
            onlineConfigGetLinkCommand.AddArgument(new Argument<string[]?>("usernames", "Target users. Leave empty for all users."));
            onlineConfigGetLinkCommand.Handler = CommandHandler.Create(
                async (string[]? usernames) =>
                {
                    users = await loadUsersTask;
                    settings = await loadSettingsTask;

                    PrintTableBorder(16, 110);
                    Console.WriteLine($"|{"User",-16}|{"Online Configuration Delivery URL",110}|");
                    PrintTableBorder(16, 110);

                    if (usernames == null)
                        foreach (var user in users.UserDict)
                            Console.WriteLine($"|{user.Key,-16}|{$"{settings.OnlineConfigDeliveryRootUri}/{user.Value.Uuid}.json",110}|");
                    else
                        foreach (var username in usernames)
                            if (users.UserDict.TryGetValue(username, out User? user))
                                Console.WriteLine($"|{username,-16}|{$"{settings.OnlineConfigDeliveryRootUri}/{user.Uuid}.json",110}|");
                            else
                                Console.WriteLine($"User not found: {username}.");

                    PrintTableBorder(16, 110);
                });

            onlineConfigCleanCommand.AddArgument(new Argument<string[]?>("usernames", "Specify users to clean online configuration files for."));
            onlineConfigCleanCommand.AddOption(new Option<bool>("--all", "clean for all users."));
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

            outlineServerAddCommand.AddArgument(new Argument<string>("group", "Specify a group to add the Outline server to."));
            outlineServerAddCommand.AddArgument(new Argument<string>("apiKey", "The Outline server API key."));
            outlineServerAddCommand.Handler = CommandHandler.Create(
                async (string group, string apiKey) =>
                {
                    nodes = await loadNodesTask;
                    settings = await loadSettingsTask;

                    if (string.IsNullOrEmpty(apiKey))
                        Console.WriteLine("You must specify an API key.");

                    int result;

                    try
                    {
                        if (settings.OutlineServerApplyDefaultUserOnAssociation)
                            result = await nodes.AssociateOutlineServerWithGroup(group, apiKey, settings.OutlineServerGlobalDefaultUser);
                        else
                            result = await nodes.AssociateOutlineServerWithGroup(group, apiKey, null);
                    }
                    catch
                    {
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
                async (string group, string? name, string? hostname, int? port, bool? metrics, string? defaultUser) =>
                {
                    nodes = await loadNodesTask;

                    try
                    {
                        var statusCodes = await nodes.SetOutlineServerInGroup(group, name, hostname, port, metrics, defaultUser);
                        if (statusCodes != null)
                        {
                            foreach (var statusCode in statusCodes)
                                if (statusCode != System.Net.HttpStatusCode.NoContent)
                                    Console.WriteLine($"{statusCode:D} {statusCode:G}");
                        }
                        else
                            Console.WriteLine("Group not found or no associated Outline server.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"An error occurred while applying settings to Outline servers.\n{ex.Message}");
                    }

                    await Nodes.SaveNodesAsync(nodes);
                });

            outlineServerRemoveCommand.AddArgument(new Argument<string>("group", "The associated group."));
            outlineServerRemoveCommand.Handler = CommandHandler.Create(
                async (string group) =>
                {
                    nodes = await loadNodesTask;
                    if (nodes.RemoveOutlineServerFromGroup(group) != 0)
                        Console.WriteLine($"Group not found: {group}");
                    await Nodes.SaveNodesAsync(nodes);
                });

            outlineServerUpdateCommand.AddArgument(new Argument<string[]?>("groups", "Specify groups to update for."));
            outlineServerUpdateCommand.AddOption(new Option<bool>("--no-sync", "Do not update local user credential storage from retrieved access key list."));
            outlineServerUpdateCommand.Handler = CommandHandler.Create(
                async (string[]? groups, bool noSync) =>
                {
                    nodes = await loadNodesTask;
                    users = await loadUsersTask;

                    try
                    {
                        if (groups == null)
                            await nodes.UpdateOutlineServerForAllGroups(users, !noSync);
                        else
                            foreach (var group in groups)
                            {
                                var result = await nodes.UpdateGroupOutlineServer(group, users, !noSync);
                                if (result == 0)
                                {
                                }
                                else if (result == -1)
                                    Console.WriteLine($"Group not found: {group}");
                                else if (result == -2)
                                    Console.WriteLine($"Group not associated with an Outline server: {group}");
                            }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"An error occurred while updating from Outline servers.\n{ex.Message}");
                    }

                    await Users.SaveUsersAsync(users);
                    await Nodes.SaveNodesAsync(nodes);
                });

            outlineServerDeployCommand.AddArgument(new Argument<string[]?>("groups", "The associated group."));
            outlineServerDeployCommand.Handler = CommandHandler.Create(
                async (string[]? groups) =>
                {
                    users = await loadUsersTask;
                    nodes = await loadNodesTask;

                    try
                    {
                        if (groups == null)
                            await nodes.DeployAllOutlineServers(users);
                        else
                        {
                            var tasks = groups.Select(async x => await nodes.DeployGroupOutlineServer(x, users));
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
                    catch (Exception ex)
                    {
                        Console.WriteLine($"An error occurred while deploying Outline servers.\n{ex.Message}");
                    }

                    await Users.SaveUsersAsync(users);
                    await Nodes.SaveNodesAsync(nodes);
                });

            outlineServerRotatePasswordCommand.AddOption(new Option<string[]?>("--usernames", "Target users."));
            outlineServerRotatePasswordCommand.AddOption(new Option<string[]?>("--groups", "Target groups."));
            outlineServerRotatePasswordCommand.Handler = CommandHandler.Create(
                async (string[]? usernames, string[]? groups) =>
                {
                    users = await loadUsersTask;
                    nodes = await loadNodesTask;

                    try
                    {
                        if (groups != null)
                        {
                            var tasks = groups.Select(async x => await nodes.RotateGroupPassword(x, users, usernames));
                            await Task.WhenAll(tasks);
                        }
                        else if (usernames != null)
                        {
                            var targetGroups = usernames.Where(x => users.UserDict.ContainsKey(x))
                                                        .SelectMany(x => users.UserDict[x].Credentials.Keys)
                                                        .Distinct();
                            var tasks = targetGroups.Select(async x => await nodes.RotateGroupPassword(x, users, usernames));
                            await Task.WhenAll(tasks);
                        }
                        else
                            Console.WriteLine("Please provide either a username or a group, or both.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"An error occurred while connecting to Outline servers.\n{ex.Message}");
                    }

                    await Users.SaveUsersAsync(users);
                    await Nodes.SaveNodesAsync(nodes);
                });

            settingsGetCommand.Handler = CommandHandler.Create(
                async () =>
                {
                    settings = await loadSettingsTask;

                    PrintTableBorder(42, 40);
                    Console.WriteLine($"|{"Key",-42}|{"Value",40}|");
                    PrintTableBorder(42, 40);

                    Console.WriteLine($"|{"Version",-42}|{settings.Version,40}|");
                    Console.WriteLine($"|{"UserDataUsageDefaultSortBy",-42}|{settings.UserDataUsageDefaultSortBy,40}|");
                    Console.WriteLine($"|{"GroupDataUsageDefaultSortBy",-42}|{settings.GroupDataUsageDefaultSortBy,40}|");
                    Console.WriteLine($"|{"OnlineConfigSortByName",-42}|{settings.OnlineConfigSortByName,40}|");
                    Console.WriteLine($"|{"OnlineConfigCleanOnUserRemoval",-42}|{settings.OnlineConfigCleanOnUserRemoval,40}|");
                    Console.WriteLine($"|{"OnlineConfigUpdateDataUsageOnGeneration",-42}|{settings.OnlineConfigUpdateDataUsageOnGeneration,40}|");
                    Console.WriteLine($"|{"OnlineConfigOutputDirectory",-42}|{settings.OnlineConfigOutputDirectory,40}|");
                    Console.WriteLine($"|{"OnlineConfigDeliveryRootUri",-42}|{settings.OnlineConfigDeliveryRootUri,40}|");
                    Console.WriteLine($"|{"OutlineServerDeployOnChange",-42}|{settings.OutlineServerDeployOnChange,40}|");
                    Console.WriteLine($"|{"OutlineServerApplyDefaultUserOnAssociation",-42}|{settings.OutlineServerApplyDefaultUserOnAssociation,40}|");
                    Console.WriteLine($"|{"OutlineServerGlobalDefaultUser",-42}|{settings.OutlineServerGlobalDefaultUser,40}|");

                    PrintTableBorder(42, 40);
                });

            settingsSetCommand.AddOption(new Option<SortBy?>("--user-data-usage-default-sort-by", "The default sort rule for user data usage report."));
            settingsSetCommand.AddOption(new Option<SortBy?>("--group-data-usage-default-sort-by", "The default sort rule for group data usage report."));
            settingsSetCommand.AddOption(new Option<bool?>("--online-config-sort-by-name", "Whether the generated servers list in an SIP008 JSON should be sorted by server name."));
            settingsSetCommand.AddOption(new Option<bool?>("--online-config-clean-on-user-removal", "Whether the user's online configuration file should be removed when the user is being removed."));
            settingsSetCommand.AddOption(new Option<bool?>("--online-config-update-data-usage-on-generation", "Whether data usage metrics are updated from configured sources when generating online config."));
            settingsSetCommand.AddOption(new Option<string>("--online-config-output-directory", "Online configuration generation output directory. No trailing slashes allowed."));
            settingsSetCommand.AddOption(new Option<string>("--online-config-delivery-root-uri", "The URL base for SIP008 online configuration delivery. No trailing slashes allowed."));
            settingsSetCommand.AddOption(new Option<bool?>("--outline-server-deploy-on-change", "Whether changes made to local databases trigger deployments to linked Outline servers."));
            settingsSetCommand.AddOption(new Option<bool?>("--outline-server-apply-default-user-on-association", "Whether to apply the global default user when associating with Outline servers."));
            settingsSetCommand.AddOption(new Option<string?>("--outline-server-global-default-user", "The global setting for Outline server's default access key's user."));
            settingsSetCommand.Handler = CommandHandler.Create(
                async (SortBy? userDataUsageDefaultSortBy, SortBy? groupDataUsageDefaultSortBy, bool? onlineConfigSortByName, bool? onlineConfigCleanOnUserRemoval, bool? onlineConfigUpdateDataUsageOnGeneration, string onlineConfigOutputDirectory, string onlineConfigDeliveryRootUri, bool? outlineServerDeployOnChange, bool? outlineServerApplyDefaultUserOnAssociation, string? outlineServerGlobalDefaultUser) =>
                {
                    settings = await loadSettingsTask;
                    if (userDataUsageDefaultSortBy is SortBy userSortBy)
                        settings.UserDataUsageDefaultSortBy = userSortBy;
                    if (groupDataUsageDefaultSortBy is SortBy groupSortBy)
                        settings.GroupDataUsageDefaultSortBy = groupSortBy;
                    if (onlineConfigSortByName is bool sortByName)
                        settings.OnlineConfigSortByName = sortByName;
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
                    if (!string.IsNullOrEmpty(outlineServerGlobalDefaultUser))
                        settings.OutlineServerGlobalDefaultUser = outlineServerGlobalDefaultUser;
                    await Settings.SaveSettingsAsync(settings);
                });

            Console.OutputEncoding = Encoding.UTF8;
            return rootCommand.InvokeAsync(args);
        }

        public static void PrintTableBorder(params int[] columnWidths)
        {
            foreach (var columnWidth in columnWidths)
            {
                Console.Write('+');
                for (var i = 0; i < columnWidth; i++)
                    Console.Write('-');
            }
            Console.Write("+\n");
        }
    }
}
