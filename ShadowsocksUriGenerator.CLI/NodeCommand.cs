﻿using ShadowsocksUriGenerator.CLI.Utils;
using System;
using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ShadowsocksUriGenerator.CLI
{
    public static class NodeCommand
    {
        public static string? ValidateNodePlugin(CommandResult commandResult)
        {
            var hasPlugin = commandResult.Children.Contains("--plugin");
            var hasPluginOpts = commandResult.Children.Contains("--plugin-opts");

            if (!hasPlugin && hasPluginOpts)
                return "You didn't specify a plugin.";
            else
                return null;
        }

        public static async Task<int> Add(
            string group,
            string nodename,
            string host,
            int port,
            string? plugin,
            string? pluginOpts,
            string owner,
            string[] tags,
            CancellationToken cancellationToken = default)
        {
            var (loadedNodes, loadNodesErrMsg) = await Nodes.LoadNodesAsync(cancellationToken);
            if (loadNodesErrMsg is not null)
            {
                Console.WriteLine(loadNodesErrMsg);
                return 1;
            }
            using var nodes = loadedNodes;

            // Turn empty strings into null
            if (string.IsNullOrEmpty(plugin))
                plugin = null;
            if (string.IsNullOrEmpty(pluginOpts))
                pluginOpts = null;

            // Retrieve owner user UUID.
            string? ownerUuid = null;
            if (!string.IsNullOrEmpty(owner))
            {
                var (users, loadUsersErrMsg) = await Users.LoadUsersAsync(cancellationToken);
                if (loadUsersErrMsg is not null)
                {
                    Console.WriteLine(loadUsersErrMsg);
                    return 1;
                }

                if (users.UserDict.TryGetValue(owner, out var targetUser))
                {
                    ownerUuid = targetUser.Uuid;
                }
                else
                {
                    Console.WriteLine($"Warning: The specified owner {owner} is not a user. Skipping.");
                }
            }

            // Deduplicate tags.
            tags = tags.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

            var result = nodes.AddNodeToGroup(group, nodename, host, port, plugin, pluginOpts, ownerUuid, tags);
            switch (result)
            {
                case 0:
                    Console.WriteLine($"Added {nodename} to group {group}.");
                    break;
                case -1:
                    Console.WriteLine($"Error: A node with the name {nodename} already exists in group {group}.");
                    break;
                case -2:
                    Console.WriteLine($"Error: Group {group} doesn't exist.");
                    break;
                case -3:
                    Console.WriteLine($"Error: Invalid port number: {port}.");
                    break;
                default:
                    Console.WriteLine($"Unknown error.");
                    break;
            }

            var saveNodesErrMsg = await Nodes.SaveNodesAsync(nodes, cancellationToken);
            if (saveNodesErrMsg is not null)
            {
                Console.WriteLine(saveNodesErrMsg);
                return 1;
            }

            return result;
        }

        public static async Task<int> Edit(
            string group,
            string nodename,
            string host,
            int port,
            string plugin,
            string pluginOpts,
            bool unsetPlugin,
            string owner,
            bool unsetOwner,
            bool clearTags,
            string[] addTags,
            string[] removeTags,
            CancellationToken cancellationToken = default)
        {
            var (loadedNodes, loadNodesErrMsg) = await Nodes.LoadNodesAsync(cancellationToken);
            if (loadNodesErrMsg is not null)
            {
                Console.WriteLine(loadNodesErrMsg);
                return 1;
            }
            using var nodes = loadedNodes;

            if (nodes.Groups.TryGetValue(group, out var targetGroup))
            {
                if (targetGroup.NodeDict.TryGetValue(nodename, out var node))
                {
                    if (!string.IsNullOrEmpty(host))
                        node.Host = host;

                    if (port > 0)
                        node.Port = port;

                    if (!string.IsNullOrEmpty(plugin))
                        node.Plugin = plugin;

                    if (!string.IsNullOrEmpty(pluginOpts))
                        node.PluginOpts = pluginOpts;

                    if (unsetPlugin)
                    {
                        node.Plugin = null;
                        node.PluginOpts = null;
                    }

                    if (!string.IsNullOrEmpty(owner))
                    {
                        var (users, loadUsersErrMsg) = await Users.LoadUsersAsync(cancellationToken);
                        if (loadUsersErrMsg is not null)
                        {
                            Console.WriteLine(loadUsersErrMsg);
                            return 1;
                        }

                        if (users.UserDict.TryGetValue(owner, out var targetUser))
                        {
                            node.OwnerUuid = targetUser.Uuid;
                        }
                        else
                        {
                            Console.WriteLine($"Warning: The specified owner {owner} is not a user. Skipping.");
                        }
                    }

                    if (unsetOwner)
                    {
                        node.OwnerUuid = null;
                    }

                    if (clearTags)
                    {
                        node.Tags.Clear();
                    }

                    if (addTags.Length > 0)
                    {
                        foreach (var tag in addTags)
                        {
                            if (node.Tags.Exists(x => string.Equals(x, tag, StringComparison.OrdinalIgnoreCase)))
                            {
                                Console.WriteLine($"Warning: Tag {tag} already exists. Skipping.");
                            }
                            else
                            {
                                node.Tags.Add(tag);
                            }
                        }
                    }

                    if (removeTags.Length > 0)
                    {
                        foreach (var tag in removeTags)
                        {
                            if (node.Tags.RemoveAll(x => string.Equals(x, tag, StringComparison.OrdinalIgnoreCase)) == 0)
                            {
                                Console.WriteLine($"Warning: Tag {tag} doesn't exist.");
                            }
                        }
                    }

                    var saveNodesErrMsg = await Nodes.SaveNodesAsync(nodes, cancellationToken);
                    if (saveNodesErrMsg is not null)
                    {
                        Console.WriteLine(saveNodesErrMsg);
                        return 1;
                    }

                    return 0;
                }
                else
                {
                    Console.WriteLine($"Error: Node {nodename} doesn't exist.");
                    return -1;
                }
            }
            else
            {
                Console.WriteLine($"Error: Group {group} doesn't exist.");
                return -2;
            }
        }

        public static async Task<int> Rename(string group, string oldName, string newName, CancellationToken cancellationToken = default)
        {
            var (loadedNodes, loadNodesErrMsg) = await Nodes.LoadNodesAsync(cancellationToken);
            if (loadNodesErrMsg is not null)
            {
                Console.WriteLine(loadNodesErrMsg);
                return 1;
            }
            using var nodes = loadedNodes;

            var result = nodes.RenameNodeInGroup(group, oldName, newName);
            switch (result)
            {
                case 0:
                    Console.WriteLine($"Renamed {oldName} to {newName}.");
                    break;
                case -1:
                    Console.WriteLine($"Error: Node {oldName} doesn't exist.");
                    break;
                case -2:
                    Console.WriteLine($"Error: A node with the same name already exists: {newName}");
                    break;
                case -3:
                    Console.WriteLine($"Error: Group {group} doesn't exist.");
                    break;
                default:
                    Console.WriteLine($"Unknown error.");
                    break;
            }

            var saveNodesErrMsg = await Nodes.SaveNodesAsync(nodes, cancellationToken);
            if (saveNodesErrMsg is not null)
            {
                Console.WriteLine(saveNodesErrMsg);
                return 1;
            }

            return result;
        }

        public static async Task<int> Remove(string group, string[] nodenames, CancellationToken cancellationToken = default)
        {
            var commandResult = 0;
            var (loadedNodes, loadNodesErrMsg) = await Nodes.LoadNodesAsync(cancellationToken);
            if (loadNodesErrMsg is not null)
            {
                Console.WriteLine(loadNodesErrMsg);
                return 1;
            }
            using var nodes = loadedNodes;

            foreach (var nodename in nodenames)
            {
                var result = nodes.RemoveNodeFromGroup(group, nodename);
                switch (result)
                {
                    case 0:
                        Console.WriteLine($"Removed {nodename} from {group}.");
                        break;
                    case -1:
                        Console.WriteLine($"Error: node {nodename} doesn't exist.");
                        break;
                    case -2:
                        Console.WriteLine($"Error: Group {group} doesn't exist.");
                        break;
                    default:
                        Console.WriteLine($"Unknown error.");
                        break;
                }
                commandResult += result;
            }

            var saveNodesErrMsg = await Nodes.SaveNodesAsync(nodes, cancellationToken);
            if (saveNodesErrMsg is not null)
            {
                Console.WriteLine(saveNodesErrMsg);
                return 1;
            }

            return commandResult;
        }

        public static async Task<int> List(string[] groups, bool namesOnly, bool onePerLine, CancellationToken cancellationToken = default)
        {
            var (loadedNodes, loadNodesErrMsg) = await Nodes.LoadNodesAsync(cancellationToken);
            if (loadNodesErrMsg is not null)
            {
                Console.WriteLine(loadNodesErrMsg);
                return 1;
            }
            using var nodes = loadedNodes;

            if (namesOnly)
            {
                foreach (var groupEntry in nodes.Groups)
                {
                    if (groups.Length > 0 && !groups.Contains(groupEntry.Key))
                        continue;

                    var keys = groupEntry.Value.NodeDict.Keys;

                    Console.WriteLine($"Group: {groupEntry.Key}");
                    Console.WriteLine($"Nodes: {keys.Count}");
                    ConsoleHelper.PrintNameList(keys, onePerLine);
                    Console.WriteLine();
                }

                return 0;
            }

            List<(string group, string nodeName, Node node)> filteredNodes = new();

            foreach (var groupEntry in nodes.Groups)
            {
                if (groups.Length > 0 && !groups.Contains(groupEntry.Key))
                    continue;

                foreach (var node in groupEntry.Value.NodeDict)
                    filteredNodes.Add((groupEntry.Key, node.Key, node.Value));
            }

            Console.WriteLine($"Nodes: {filteredNodes.Count}");

            if (filteredNodes.Count == 0)
            {
                return 0;
            }

            var maxNodeNameLength = filteredNodes.Max(x => x.nodeName.Length);
            var maxGroupNameLength = filteredNodes.Max(x => x.group.Length);
            var maxHostnameLength = filteredNodes.Max(x => x.node.Host.Length);
            var maxPluginLength = filteredNodes.Max(x => x.node.Plugin?.Length);
            var maxPluginOptsLength = filteredNodes.Max(x => x.node.PluginOpts?.Length);

            var nodeNameFieldWidth = maxNodeNameLength > 4 ? maxNodeNameLength + 2 : 6;
            var groupNameFieldWidth = maxGroupNameLength > 5 ? maxGroupNameLength + 2 : 7;
            var hostnameFieldWidth = maxHostnameLength > 4 ? maxHostnameLength + 2 : 6;

            // Nodes have no plugins. Do not display plugin and plugin options columns.
            if (maxPluginLength is null && maxPluginOptsLength is null)
            {
                ConsoleHelper.PrintTableBorder(7, nodeNameFieldWidth, groupNameFieldWidth, 36, hostnameFieldWidth, 5);
                Console.WriteLine($"|{"Status",7}|{"Node".PadRight(nodeNameFieldWidth)}|{"Group".PadRight(groupNameFieldWidth)}|{"UUID",36}|{"Host".PadLeft(hostnameFieldWidth)}|{"Port",5}|");
                ConsoleHelper.PrintTableBorder(7, nodeNameFieldWidth, groupNameFieldWidth, 36, hostnameFieldWidth, 5);

                foreach (var (group, nodeName, node) in filteredNodes)
                {
                    Console.WriteLine($"|{(node.Deactivated ? "🛑" : "✔"),7}|{nodeName.PadRight(nodeNameFieldWidth)}|{group.PadRight(groupNameFieldWidth)}|{node.Uuid,36}|{node.Host.PadLeft(hostnameFieldWidth)}|{node.Port,5}|");
                }

                ConsoleHelper.PrintTableBorder(7, nodeNameFieldWidth, groupNameFieldWidth, 36, hostnameFieldWidth, 5);
            }
            else // Nodes have plugins.
            {
                var pluginFieldWidth = maxPluginLength > 6 ? maxPluginLength.Value + 2 : 8;
                var pluginOptsFieldWidth = maxPluginOptsLength > 14 ? maxPluginOptsLength.Value + 2 : 16;

                ConsoleHelper.PrintTableBorder(7, nodeNameFieldWidth, groupNameFieldWidth, 36, hostnameFieldWidth, 5, pluginFieldWidth, pluginOptsFieldWidth);
                Console.WriteLine($"|{"Status",7}|{"Node".PadRight(nodeNameFieldWidth)}|{"Group".PadRight(groupNameFieldWidth)}|{"UUID",36}|{"Host".PadLeft(hostnameFieldWidth)}|{"Port",5}|{"Plugin".PadLeft(pluginFieldWidth)}|{"Plugin Options".PadLeft(pluginOptsFieldWidth)}|");
                ConsoleHelper.PrintTableBorder(7, nodeNameFieldWidth, groupNameFieldWidth, 36, hostnameFieldWidth, 5, pluginFieldWidth, pluginOptsFieldWidth);

                foreach (var (group, nodeName, node) in filteredNodes)
                {
                    Console.WriteLine($"|{(node.Deactivated ? "🛑" : "✔"),7}|{nodeName.PadRight(nodeNameFieldWidth)}|{group.PadRight(groupNameFieldWidth)}|{node.Uuid,36}|{node.Host.PadLeft(hostnameFieldWidth)}|{node.Port,5}|{(node.Plugin ?? string.Empty).PadLeft(pluginFieldWidth)}|{(node.PluginOpts ?? string.Empty).PadLeft(pluginOptsFieldWidth)}|");
                }

                ConsoleHelper.PrintTableBorder(7, nodeNameFieldWidth, groupNameFieldWidth, 36, hostnameFieldWidth, 5, pluginFieldWidth, pluginOptsFieldWidth);
            }

            return 0;
        }

        public static async Task<int> ListAnnotations(string[] groups, bool onePerLine, CancellationToken cancellationToken = default)
        {
            var (users, loadUsersErrMsg) = await Users.LoadUsersAsync(cancellationToken);
            if (loadUsersErrMsg is not null)
            {
                Console.WriteLine(loadUsersErrMsg);
                return 1;
            }

            var (loadedNodes, loadNodesErrMsg) = await Nodes.LoadNodesAsync(cancellationToken);
            if (loadNodesErrMsg is not null)
            {
                Console.WriteLine(loadNodesErrMsg);
                return 1;
            }
            using var nodes = loadedNodes;

            foreach (var groupEntry in nodes.Groups)
            {
                if (groups.Length > 0 && !groups.Contains(groupEntry.Key))
                    continue;

                foreach (var nodeEntry in groupEntry.Value.NodeDict)
                {
                    string? owner = null;
                    var tags = nodeEntry.Value.Tags;

                    // Resolve owner username
                    if (nodeEntry.Value.OwnerUuid is string ownerUuid)
                    {
                        owner = users.UserDict.Where(x => x.Value.Uuid == ownerUuid)
                                              .Select(x => x.Key)
                                              .FirstOrDefault();
                    }

                    if (!onePerLine) // Full format
                    {
                        Console.WriteLine($"Group: {groupEntry.Key}");
                        Console.WriteLine($"Node:  {nodeEntry.Key}");

                        if (!string.IsNullOrEmpty(owner))
                        {
                            Console.WriteLine($"Owner: {owner} ({nodeEntry.Value.OwnerUuid})");
                        }
                        else
                        {
                            Console.WriteLine("Owner: N/A");
                        }

                        Console.WriteLine($"Tags:  {tags.Count}");

                        foreach (var tag in tags)
                        {
                            Console.WriteLine($"- {tag}");
                        }

                        Console.WriteLine();
                    }
                    else // One node per line
                    {
                        Console.Write($"Group {groupEntry.Key} Node {nodeEntry.Key} Owner {owner} Tags {tags.Count}");

                        foreach (var tag in tags)
                        {
                            Console.Write($" {tag}");
                        }

                        Console.WriteLine();
                    }
                }
            }

            return 0;
        }

        public static async Task<int> Activate(string group, string[] nodenames, bool allNodes, CancellationToken cancellationToken = default)
        {
            var commandResult = 0;
            var (loadedNodes, loadNodesErrMsg) = await Nodes.LoadNodesAsync(cancellationToken);
            if (loadNodesErrMsg is not null)
            {
                Console.WriteLine(loadNodesErrMsg);
                return 1;
            }
            using var nodes = loadedNodes;

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
                commandResult += result;
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
                        commandResult += result;
                        break;
                    case -2:
                        Console.WriteLine($"Error: {group} is not found.");
                        commandResult += result;
                        break;
                }
            }

            var saveNodesErrMsg = await Nodes.SaveNodesAsync(nodes, cancellationToken);
            if (saveNodesErrMsg is not null)
            {
                Console.WriteLine(saveNodesErrMsg);
                return 1;
            }

            return commandResult;
        }

        public static async Task<int> Deactivate(string group, string[] nodenames, bool allNodes, CancellationToken cancellationToken = default)
        {
            var commandResult = 0;
            var (loadedNodes, loadNodesErrMsg) = await Nodes.LoadNodesAsync(cancellationToken);
            if (loadNodesErrMsg is not null)
            {
                Console.WriteLine(loadNodesErrMsg);
                return 1;
            }
            using var nodes = loadedNodes;

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
                commandResult += result;
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
                        commandResult += result;
                        break;
                    case -2:
                        Console.WriteLine($"Error: {group} is not found.");
                        commandResult += result;
                        break;
                }
            }

            var saveNodesErrMsg = await Nodes.SaveNodesAsync(nodes, cancellationToken);
            if (saveNodesErrMsg is not null)
            {
                Console.WriteLine(saveNodesErrMsg);
                return 1;
            }

            return commandResult;
        }
    }
}
