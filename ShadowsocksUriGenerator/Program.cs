using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace ShadowsocksUriGenerator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Users users;
            Nodes nodes;
            Settings settings;
            Task<Users> loadUsersTask = Users.LoadUsersAsync();
            Task<Nodes> loadNodesTask = Nodes.LoadNodesAsync();
            Task<Settings> loadSettingsTask = Settings.LoadSettingsAsync();

            var addUsersCommand = new Command("add-users", "Add users.");
            var addNodeCommand = new Command("add-node", "Add a node to a node group.");
            var addNodeGroupsCommand = new Command("add-node-groups", "Add node groups.");
            var addCredentialCommand = new Command("add-credential", "Add a credential to a node group for a user.");
            var renameUserCommand = new Command("rename-user", "Renames an existing user with a new name.");
            var renameNodeCommand = new Command("rename-node", "Renames an existing node with a new name.");
            var renameNodeGroupCommand = new Command("rename-node-group", "Renames an existing node group with a new name.");
            var rmUsersCommand = new Command("rm-users", "Remove users.");
            var rmNodesCommand = new Command("rm-nodes", "Remove nodes from a node group.");
            var rmNodeGroupsCommand = new Command("rm-node-groups", "Remove node groups and its nodes.");
            var rmCredentialsCommand = new Command("rm-credential", "Remove a node group's credential from a user.");
            var lsUsersCommand = new Command("ls-users", "List all users.");
            var lsNodesCommand = new Command("ls-nodes", "List nodes from the specified group or all groups.");
            var lsNodeGroupsCommand = new Command("ls-node-groups", "List all node groups.");
            var lsCredentialsCommand = new Command("ls-credentials", "List all user credentials.");
            var getUserSSUrisCommand = new Command("get-user-ss-uris", "Get the user's associated Shadowsocks URIs.");
            var getUserOnlineConfigUriCommand = new Command("get-user-online-config-uri", "Get the user's SIP008-compliant online configuration delivery URL.");
            var getSettingsCommand = new Command("get-settings", "Get and print all settings.");
            var changeSettingsCommand = new Command("change-settings", "Change settings.");
            var generateOnlineConfigCommand = new Command("gen-online-config", "Generate SIP008-compliant online configuration delivery JSON files.");

            var rootCommand = new RootCommand()
            {
                addUsersCommand,
                addNodeCommand,
                addNodeGroupsCommand,
                addCredentialCommand,
                renameUserCommand,
                renameNodeCommand,
                renameNodeGroupCommand,
                rmUsersCommand,
                rmNodesCommand,
                rmNodeGroupsCommand,
                rmCredentialsCommand,
                lsUsersCommand,
                lsNodesCommand,
                lsNodeGroupsCommand,
                lsCredentialsCommand,
                getUserSSUrisCommand,
                getUserOnlineConfigUriCommand,
                getSettingsCommand,
                changeSettingsCommand,
                generateOnlineConfigCommand,
            };

            addUsersCommand.AddArgument(new Argument<string[]>("usernames", "A list of usernames to add."));
            addUsersCommand.Handler = CommandHandler.Create(
                async (string[] usernames) =>
                {
                    users = await loadUsersTask;
                    List<string> addedUsers = users.AddUsers(usernames);
                    await Users.SaveUsersAsync(users);
                    Console.WriteLine("Successfully added:");
                    foreach (var username in addedUsers)
                        Console.WriteLine($"{username}");
                });

            addNodeCommand.AddArgument(new Argument<string>("group", "The node group that the new node belongs to."));
            addNodeCommand.AddArgument(new Argument<string>("nodename", "Name of the new node."));
            addNodeCommand.AddArgument(new Argument<string>("host", "Hostname of the new node."));
            addNodeCommand.AddArgument(new Argument<string>("portString", "Port number of the new node."));
            addNodeCommand.AddOption(new Option<string?>("--plugin", getDefaultValue: () => null, "Plugin binary name of the new node."));
            addNodeCommand.AddOption(new Option<string?>("--plugin-opts", getDefaultValue: () => null, "Plugin options of the new node."));
            addNodeCommand.Handler = CommandHandler.Create(
                async (string group, string nodename, string host, string portString, string? plugin, string? pluginOpts) =>
                {
                    nodes = await loadNodesTask;
                    if (nodes.AddNodeToGroup(group, nodename, host, portString, plugin, pluginOpts) == 0)
                        Console.WriteLine($"Added {nodename} to group {group}.");
                    else
                        Console.WriteLine($"Group not found. Or node already exists.");
                    await Nodes.SaveNodesAsync(nodes);
                });

            addNodeGroupsCommand.AddArgument(new Argument<string[]>("groups", "A list of group names to add."));
            addNodeGroupsCommand.Handler = CommandHandler.Create(
                async (string[] groups) =>
                {
                    nodes = await loadNodesTask;
                    List<string> addedNodes = nodes.AddGroups(groups);
                    await Nodes.SaveNodesAsync(nodes);
                    Console.WriteLine("Successfully added:");
                    foreach (var nodename in addedNodes)
                        Console.WriteLine($"{nodename}");
                });

            renameUserCommand.AddArgument(new Argument<string>("oldName", "The existing username."));
            renameUserCommand.AddArgument(new Argument<string>("newName", "The new username."));
            renameUserCommand.Handler = CommandHandler.Create(
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

            renameNodeCommand.AddArgument(new Argument<string>("group", "The node group which contains the node."));
            renameNodeCommand.AddArgument(new Argument<string>("oldName", "The existing node name."));
            renameNodeCommand.AddArgument(new Argument<string>("newName", "The new node name."));
            renameNodeCommand.Handler = CommandHandler.Create(
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

            renameNodeGroupCommand.AddArgument(new Argument<string>("oldName", "The existing group name."));
            renameNodeGroupCommand.AddArgument(new Argument<string>("newName", "The new group name."));
            renameNodeGroupCommand.Handler = CommandHandler.Create(
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

            rmUsersCommand.AddArgument(new Argument<string[]>("usernames", "A list of users to remove."));
            rmUsersCommand.Handler = CommandHandler.Create(
                async (string[] usernames) =>
                {
                    users = await loadUsersTask;
                    users.RemoveUsers(usernames);
                    await Users.SaveUsersAsync(users);
                });

            rmNodesCommand.AddArgument(new Argument<string>("group", "The node group that the target node belongs to."));
            rmNodesCommand.AddArgument(new Argument<string[]>("nodenames", "A list of node names to remove."));
            rmNodesCommand.Handler = CommandHandler.Create(
                async (string group, string[] nodenames) =>
                {
                    nodes = await loadNodesTask;
                    if (nodes.RemoveNodesFromGroup(group, nodenames) == -1)
                        Console.WriteLine($"Group not found: {group}.");
                    await Nodes.SaveNodesAsync(nodes);
                });

            rmNodeGroupsCommand.AddArgument(new Argument<string[]>("groups", "A list of groups to remove."));
            rmNodeGroupsCommand.Handler = CommandHandler.Create(
                async (string[] groups) =>
                {
                    users = await loadUsersTask;
                    nodes = await loadNodesTask;
                    users.RemoveCredentialsFromAllUsers(groups);
                    nodes.RemoveGroups(groups);
                    await Users.SaveUsersAsync(users);
                    await Nodes.SaveNodesAsync(nodes);
                });

            lsUsersCommand.Handler = CommandHandler.Create(
                async () =>
                {
                    Console.WriteLine($"|{"User",-16}|{"UUID",36}|{"Number of Credentials",21}|");
                    users = await loadUsersTask;
                    foreach (var user in users.UserDict)
                        Console.WriteLine($"|{user.Key,-16}|{user.Value.Uuid,36}|{user.Value.Credentials.Count,21}|");
                });

            lsCredentialsCommand.Handler = CommandHandler.Create(
                async () =>
                {
                    Console.WriteLine($"|{"User",-16}|{"Group",-16}|{"Method",-24}|{"Password",-32}|");
                    users = await loadUsersTask;
                    foreach (var user in users.UserDict)
                    {
                        foreach (var credEntry in user.Value.Credentials)
                        {
                            Console.WriteLine($"|{user.Key,-16}|{credEntry.Key,-16}|{credEntry.Value.Method,-24}|{credEntry.Value.Password,-32}|");
                        }
                    }
                });

            lsNodesCommand.AddArgument(new Argument<string?>("group", getDefaultValue: () => null, "Target group. Leave empty for all groups."));
            lsNodesCommand.Handler = CommandHandler.Create(
                async (string? group) =>
                {
                    Console.WriteLine($"|{"Node",-32}|{"Group",-16}|{"UUID",36}|{"Host",-40}|{"Port",5}|{"Plugin",12}|{"Plugin Options",24}|");
                    nodes = await loadNodesTask;
                    if (string.IsNullOrEmpty(group))
                    {
                        foreach (var groupEntry in nodes.Groups)
                            foreach (var node in groupEntry.Value.NodeDict)
                                Console.WriteLine($"|{node.Key,-32}|{groupEntry.Key,-16}|{node.Value.Uuid,36}|{node.Value.Host,-40}|{node.Value.Port,5}|{node.Value.Plugin,12}|{node.Value.PluginOpts,24}|");
                    }
                    else if (nodes.Groups.TryGetValue(group, out Group? targetGroup))
                    {
                        foreach (var node in targetGroup.NodeDict)
                            Console.WriteLine($"|{node.Key,-32}|{group,-16}|{node.Value.Uuid,36}|{node.Value.Host,-40}|{node.Value.Port,5}|{node.Value.Plugin,12}|{node.Value.PluginOpts,24}|");
                    }
                    else
                        Console.WriteLine($"Group not found: {group}.");
                });

            lsNodeGroupsCommand.Handler = CommandHandler.Create(
                async () =>
                {
                    Console.WriteLine($"|{"Group",-16}|{"Number of Nodes",16}|");
                    nodes = await loadNodesTask;
                    foreach (var group in nodes.Groups)
                    {
                        Console.WriteLine($"|{group.Key,-16}|{group.Value.NodeDict.Count,16}|");
                    }
                });

            addCredentialCommand.AddArgument(new Argument<string>("username", "The user that the credential belongs to."));
            addCredentialCommand.AddArgument(new Argument<string>("group", "The group that the credential is for."));
            addCredentialCommand.AddOption(new Option<string>("--method", "The encryption method. MUST be combined with --password."));
            addCredentialCommand.AddOption(new Option<string>("--password", "The password. MUST be combined with --method."));
            addCredentialCommand.AddOption(new Option<string>("--userinfo-base64url", "The userinfo encoded in URL-safe base64. Can't be used with any other option."));
            addCredentialCommand.Handler = CommandHandler.Create(
                async (string username, string group, string method, string password, string userinfoBase64url) =>
                {
                    users = await loadUsersTask;
                    nodes = await loadNodesTask;
                    int result;
                    if (!string.IsNullOrEmpty(method) && !string.IsNullOrEmpty(password))
                        result = users.AddCredentialToUser(username, group, method, password, nodes);
                    else if (!string.IsNullOrEmpty(userinfoBase64url))
                        result = users.AddCredentialToUser(username, group, userinfoBase64url, nodes);
                    else
                    {
                        Console.WriteLine("Not enough options. Either provide a method and a password, or provide a userinfo base64url.");
                        return;
                    }
                    if (result == 0)
                        Console.WriteLine($"Successfully added {group}'s credential to {username}");
                    else if (result == 1)
                        Console.WriteLine("The user already has a credential for the group.");
                    else
                        Console.WriteLine("User or group not found.");
                    await Users.SaveUsersAsync(users);
                });

            rmCredentialsCommand.AddArgument(new Argument<string>("username", "Target user."));
            rmCredentialsCommand.AddArgument(new Argument<string[]>("groups", "A list of groups the credentials are for."));
            rmCredentialsCommand.Handler = CommandHandler.Create(
                async (string username, string[] groups) =>
                {
                    users = await loadUsersTask;
                    if (users.RemoveCredentialsFromUser(username, groups) == -1)
                        Console.WriteLine($"User not found: {username}");
                    await Users.SaveUsersAsync(users);
                });

            getUserSSUrisCommand.AddArgument(new Argument<string>("username", "Target user."));
            getUserSSUrisCommand.Handler = CommandHandler.Create(
                async (string username) =>
                {
                    users = await loadUsersTask;
                    nodes = await loadNodesTask;
                    List<Uri> uris = users.GetUserSSUris(username, nodes);
                    foreach (var uri in uris)
                        Console.WriteLine($"{uri.AbsoluteUri}");
                });

            getUserOnlineConfigUriCommand.AddArgument(new Argument<string?>("username", getDefaultValue: () => null, "Specifies the target user. Leave empty for all users."));
            getUserOnlineConfigUriCommand.Handler = CommandHandler.Create(
                async (string? username) =>
                {
                    Console.WriteLine($"|{"User",-16}|{"Online Configuration Delivery URL",110}|");
                    users = await loadUsersTask;
                    settings = await loadSettingsTask;
                    if (string.IsNullOrEmpty(username))
                        foreach (var user in users.UserDict)
                            Console.WriteLine($"|{user.Key,-16}|{$"{settings.OnlineConfigDeliveryRootUri}/{user.Value.Uuid}.json",110}|");
                    else if (users.UserDict.TryGetValue(username, out User? user))
                        Console.WriteLine($"|{username,-16}|{$"{settings.OnlineConfigDeliveryRootUri}/{user.Uuid}.json",110}|");
                    else
                        Console.WriteLine($"User not found: {username}.");
                });

            getSettingsCommand.Handler = CommandHandler.Create(
                async () =>
                {
                    Console.WriteLine($"|{"Key",-32}|{"Value",40}|");
                    settings = await loadSettingsTask;
                    Console.WriteLine($"|{"Version",-32}|{settings.Version,40}|");
                    Console.WriteLine($"|{"OnlineConfigSortByName",-32}|{settings.OnlineConfigSortByName,40}|");
                    Console.WriteLine($"|{"OnlineConfigOutputDirectory",-32}|{settings.OnlineConfigOutputDirectory,40}|");
                    Console.WriteLine($"|{"OnlineConfigDeliveryRootUri",-32}|{settings.OnlineConfigDeliveryRootUri,40}|");
                });

            changeSettingsCommand.AddOption(new Option<bool?>("--online-config-sort-by-name", "Whether the generated servers list in an SIP008 JSON should be sorted by server name."));
            changeSettingsCommand.AddOption(new Option<string>("--online-config-output-directory", "Online configuration generation output directory. No trailing slashes allowed."));
            changeSettingsCommand.AddOption(new Option<string>("--online-config-delivery-root-uri", "The URL base for SIP008 online configuration delivery. No trailing slashes allowed."));
            changeSettingsCommand.Handler = CommandHandler.Create(
                async (bool? onlineConfigSortByName, string onlineConfigOutputDirectory, string onlineConfigDeliveryRootUri) =>
                {
                    settings = await loadSettingsTask;
                    if (onlineConfigSortByName is bool _onlineConfigSortByName)
                        settings.OnlineConfigSortByName = _onlineConfigSortByName;
                    if (!string.IsNullOrEmpty(onlineConfigOutputDirectory))
                        settings.OnlineConfigOutputDirectory = onlineConfigOutputDirectory;
                    if (!string.IsNullOrEmpty(onlineConfigDeliveryRootUri))
                        settings.OnlineConfigDeliveryRootUri = onlineConfigDeliveryRootUri;
                    await Settings.SaveSettingsAsync(settings);
                });

            generateOnlineConfigCommand.AddArgument(new Argument<string?>("username", getDefaultValue: () => null, "Specify a user to generate for. Leave empty for all users."));
            generateOnlineConfigCommand.Handler = CommandHandler.Create(
                async (string? username) =>
                {
                    users = await loadUsersTask;
                    nodes = await loadNodesTask;
                    settings = await loadSettingsTask;
                    await OnlineConfig.GenerateAndSave(users, nodes, settings, username);
                });

            Console.OutputEncoding = System.Text.Encoding.UTF8;
            await rootCommand.InvokeAsync(args);
        }
    }
}
