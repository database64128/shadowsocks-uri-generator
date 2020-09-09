using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace shadowsocks_uri_generator
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
            var rmUsersCommand = new Command("rm-users", "Remove users.");
            var rmNodesCommand = new Command("rm-nodes", "Remove nodes from a node group.");
            var rmNodeGroupsCommand = new Command("rm-node-groups", "Remove node groups and its nodes.");
            var rmCredentialsCommand = new Command("rm-credential", "Remove a node group's credential from a user.");
            var lsUsersCommand = new Command("ls-users", "List all users.");
            var lsNodesCommand = new Command("ls-nodes", "List nodes from the specified group.");
            var lsNodeGroupsCommand = new Command("ls-node-groups", "List all node groups.");
            var lsCredentialsCommand = new Command("ls-credentials", "List all user credentials.");
            var getUserSSUrisCommand = new Command("get-user-ss-uris", "Get the user's associated Shadowsocks URIs.");
            var getUserOnlineConfigUriCommand = new Command("get-user-online-config-uri", "Get the user's SIP008-compliant online configuration delivery URL.");
            var getSettingsCommand = new Command("get-settings", "Get and print all settings.");
            var changeSettingsCommand = new Command("change-settings", "Change any settings");
            var generateOnlineConfigCommand = new Command("gen-online-config", "Generate SIP008-compliant online configuration delivery JSON files.");

            var rootCommand = new RootCommand()
            {
                addUsersCommand,
                addNodeCommand,
                addNodeGroupsCommand,
                addCredentialCommand,
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
                generateOnlineConfigCommand
            };

            addUsersCommand.AddArgument(new Argument<string[]>("usernames"));
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

            addNodeCommand.AddArgument(new Argument<string>("group"));
            addNodeCommand.AddArgument(new Argument<string>("nodename"));
            addNodeCommand.AddArgument(new Argument<string>("host"));
            addNodeCommand.AddArgument(new Argument<string>("portString"));
            addNodeCommand.Handler = CommandHandler.Create(
                async (string group, string nodename, string host, string portString) =>
                {
                    nodes = await loadNodesTask;
                    if (nodes.AddNodeToGroup(group, nodename, host, portString) == 0)
                        Console.WriteLine($"Added {nodename} to group {group}.");
                    else
                        Console.WriteLine($"Group not found. Or node already exists.");
                    await Nodes.SaveNodesAsync(nodes);
                });

            addNodeGroupsCommand.AddArgument(new Argument<string[]>("groups"));
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

            rmUsersCommand.AddArgument(new Argument<string[]>("usernames"));
            rmUsersCommand.Handler = CommandHandler.Create(
                async (string[] usernames) =>
                {
                    users = await loadUsersTask;
                    users.RemoveUsers(usernames);
                    await Users.SaveUsersAsync(users);
                });

            rmNodesCommand.AddArgument(new Argument<string>("group"));
            rmNodesCommand.AddArgument(new Argument<string[]>("nodenames"));
            rmNodesCommand.Handler = CommandHandler.Create(
                async (string group, string[] nodenames) =>
                {
                    nodes = await loadNodesTask;
                    if (nodes.RemoveNodesFromGroup(group, nodenames) == -1)
                        Console.WriteLine($"Removal failed.");
                    await Nodes.SaveNodesAsync(nodes);
                });

            rmNodeGroupsCommand.AddArgument(new Argument<string[]>("groups"));
            rmNodeGroupsCommand.Handler = CommandHandler.Create(
                async (string[] groups) =>
                {
                    nodes = await loadNodesTask;
                    nodes.RemoveGroups(groups);
                    await Nodes.SaveNodesAsync(nodes);
                });

            lsUsersCommand.Handler = CommandHandler.Create(
                async () =>
                {
                    Console.WriteLine($"|{"User", -16}|{"UUID", 36}|{"Number of Credentials", 21}|");
                    users = await loadUsersTask;
                    foreach (var user in users.UserDict)
                        Console.WriteLine($"|{user.Key, -16}|{user.Value.Uuid, 36}|{user.Value.Credentials.Count, 21}|");
                });

            lsCredentialsCommand.Handler = CommandHandler.Create(
                async () =>
                {
                    Console.WriteLine($"|{"User", -16}|{"Group", -16}|{"Method", -24}|{"Password", -32}|");
                    users = await loadUsersTask;
                    foreach (var user in users.UserDict)
                    {
                        foreach (var credEntry in user.Value.Credentials)
                        {
                            Console.WriteLine($"|{user.Key, -16}|{credEntry.Key, -16}|{credEntry.Value.Method, -24}|{credEntry.Value.Password, -32}|");
                        }
                    }
                });

            lsNodesCommand.AddArgument(new Argument<string>("group"));
            lsNodesCommand.Handler = CommandHandler.Create(
                async (string group) =>
                {
                    nodes = await loadNodesTask;
                    if (nodes.Groups.TryGetValue(group, out Group? targetGroup))
                    {
                        Console.WriteLine($"|{"Node", -32}|{"UUID", 36}|{"Host", -40}|{"Port", 5}|");
                        foreach (var node in targetGroup.NodeDict)
                            Console.WriteLine($"|{node.Key, -32}|{node.Value.Uuid, 36}|{node.Value.Host, -40}|{node.Value.Port, 5}|");
                    }
                    else
                        Console.WriteLine($"Group not found: {group}.");
                });

            lsNodeGroupsCommand.Handler = CommandHandler.Create(
                async () =>
                {
                    Console.WriteLine($"|{"Group", -16}|{"Number of Nodes", 16}|");
                    nodes = await loadNodesTask;
                    foreach (var group in nodes.Groups)
                    {
                        Console.WriteLine($"|{group.Key, -16}|{group.Value.NodeDict.Count, 16}|");
                    }
                });

            addCredentialCommand.AddArgument(new Argument<string>("username"));
            addCredentialCommand.AddArgument(new Argument<string>("group"));
            addCredentialCommand.AddOption(new Option<string>("--method"));
            addCredentialCommand.AddOption(new Option<string>("--password"));
            addCredentialCommand.AddOption(new Option<string>("--userinfo-base64url"));
            addCredentialCommand.Handler = CommandHandler.Create(
                async (string username, string group, string method, string password, string userinfoBase64url) =>
                {
                    users = await loadUsersTask;
                    nodes = await loadNodesTask;
                    if (!string.IsNullOrEmpty(method) && !string.IsNullOrEmpty(password))
                    {
                        if (users.AddCredentialToUser(username, group, method, password, nodes) == 0)
                            Console.WriteLine($"Successfully added {group}'s credential to {username}");
                        else
                            Console.WriteLine("Group not found. Or credential already exists.");
                    }
                    else if (!string.IsNullOrEmpty(userinfoBase64url))
                    {
                        if (users.AddCredentialToUser(username, group, userinfoBase64url, nodes) == 0)
                            Console.WriteLine($"Successfully added {group}'s credential to {username}");
                        else
                            Console.WriteLine("Group not found. Or credential already exists.");
                    }
                    else
                    {
                        Console.WriteLine("Not enough options. Either provide a method and a password, or provide a userinfo base64url.");
                    }
                    await Users.SaveUsersAsync(users);
                });

            rmCredentialsCommand.AddArgument(new Argument<string>("username"));
            rmCredentialsCommand.AddArgument(new Argument<string[]>("groups"));
            rmCredentialsCommand.Handler = CommandHandler.Create(
                async (string username, string[] groups) =>
                {
                    users = await loadUsersTask;
                    if (users.RemoveCredentialsFromUser(username, groups) == -1)
                        Console.WriteLine("User not found.");
                    await Users.SaveUsersAsync(users);
                });

            getUserSSUrisCommand.AddArgument(new Argument<string>("username"));
            getUserSSUrisCommand.Handler = CommandHandler.Create(
                async (string username) =>
                {
                    users = await loadUsersTask;
                    nodes = await loadNodesTask;
                    List<Uri> uris = users.GetUserSSUris(username, nodes);
                    foreach (var uri in uris)
                        Console.WriteLine($"{uri.AbsoluteUri}");
                });

            getUserOnlineConfigUriCommand.AddArgument(new Argument<string>("username", getDefaultValue: () => ""));
            getUserOnlineConfigUriCommand.Handler = CommandHandler.Create(
                async (string username) =>
                {
                    Console.WriteLine($"|{"User", -16}|{"Online Configuration Delivery URL", 110}|");
                    users = await loadUsersTask;
                    settings = await loadSettingsTask;
                    if (string.IsNullOrEmpty(username))
                        foreach (var user in users.UserDict)
                            Console.WriteLine($"|{user.Key, -16}|{$"{settings.OnlineConfigDeliveryRootUri}/{user.Value.Uuid}.json",110}|");
                    else
                        if (users.UserDict.TryGetValue(username, out User? user))
                            Console.WriteLine($"|{username,-16}|{$"{settings.OnlineConfigDeliveryRootUri}/{user.Uuid}.json",110}|");
                });

            getSettingsCommand.Handler = CommandHandler.Create(
                async () =>
                {
                    Console.WriteLine($"|{"Key", -32}|{"Value", 40}|");
                    settings = await loadSettingsTask;
                    Console.WriteLine($"|{"Version", -32}|{settings.Version, 40}|");
                    Console.WriteLine($"|{"OnlineConfigOutputDirectory", -32}|{settings.OnlineConfigOutputDirectory, 40}|");
                    Console.WriteLine($"|{"OnlineConfigDeliveryRootUri", -32}|{settings.OnlineConfigDeliveryRootUri, 40}|");
                });

            changeSettingsCommand.AddOption(new Option<string>("--online-config-output-directory"));
            changeSettingsCommand.AddOption(new Option<string>("--online-config-delivery-root-uri"));
            changeSettingsCommand.Handler = CommandHandler.Create(
                async (string onlineConfigOutputDirectory, string onlineConfigDeliveryRootUri) =>
                {
                    settings = await loadSettingsTask;
                    if (!string.IsNullOrEmpty(onlineConfigOutputDirectory))
                        settings.OnlineConfigOutputDirectory = onlineConfigOutputDirectory;
                    if (!string.IsNullOrEmpty(onlineConfigDeliveryRootUri))
                        settings.OnlineConfigDeliveryRootUri = onlineConfigDeliveryRootUri;
                    await Settings.SaveSettingsAsync(settings);
                });

            generateOnlineConfigCommand.AddArgument(new Argument<string>("username", getDefaultValue: () => ""));
            generateOnlineConfigCommand.Handler = CommandHandler.Create(
                async (string username) =>
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
