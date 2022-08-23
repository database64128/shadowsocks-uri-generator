using ShadowsocksUriGenerator.CLI.Binders;
using ShadowsocksUriGenerator.CLI.Utils;
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
        public static void ValidateNodePlugin(CommandResult commandResult)
        {
            var hasPluginName = commandResult.ContainsSymbolWithName("plugin-name");
            var hasPluginVersion = commandResult.ContainsSymbolWithName("plugin-version");
            var hasPluginOptions = commandResult.ContainsSymbolWithName("plugin-options");
            var hasPluginArguments = commandResult.ContainsSymbolWithName("plugin-arguments");
            var hasUnsetPlugin = commandResult.ContainsSymbolWithName("unset-plugin");

            if (!hasPluginName && (hasPluginVersion || hasPluginOptions || hasPluginArguments))
                commandResult.ErrorMessage = "You didn't specify a plugin.";

            if (hasPluginName && hasUnsetPlugin)
                commandResult.ErrorMessage = "You can't set and unset plugin at the same time.";
        }

        public static async Task<int> Add(NodeAddChangeSet nodeAddChangeSet, CancellationToken cancellationToken = default)
        {
            var (loadedNodes, loadNodesErrMsg) = await Nodes.LoadNodesAsync(cancellationToken);
            if (loadNodesErrMsg is not null)
            {
                Console.WriteLine(loadNodesErrMsg);
                return 1;
            }
            using var nodes = loadedNodes;

            // Retrieve owner user UUID.
            string? ownerUuid = null;
            if (!string.IsNullOrEmpty(nodeAddChangeSet.Owner))
            {
                var (users, loadUsersErrMsg) = await Users.LoadUsersAsync(cancellationToken);
                if (loadUsersErrMsg is not null)
                {
                    Console.WriteLine(loadUsersErrMsg);
                    return 1;
                }

                if (users.UserDict.TryGetValue(nodeAddChangeSet.Owner, out var targetUser))
                {
                    ownerUuid = targetUser.Uuid;
                }
                else
                {
                    Console.WriteLine($"Warning: The specified owner {nodeAddChangeSet.Owner} is not a user. Skipping.");
                }
            }

            // Deduplicate tags.
            var tags = nodeAddChangeSet.Tags.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

            var result = nodes.AddNodeToGroup(nodeAddChangeSet.Group, nodeAddChangeSet.Nodename, nodeAddChangeSet.Host, nodeAddChangeSet.Port, nodeAddChangeSet.PluginName, nodeAddChangeSet.PluginVersion, nodeAddChangeSet.PluginOptions, nodeAddChangeSet.PluginArguments, ownerUuid, tags, nodeAddChangeSet.IdentityPSKs);
            switch (result)
            {
                case 0:
                    Console.WriteLine($"Added {nodeAddChangeSet.Nodename} to group {nodeAddChangeSet.Group}.");
                    break;
                case -1:
                    Console.WriteLine($"Error: A node with the name {nodeAddChangeSet.Nodename} already exists in group {nodeAddChangeSet.Group}.");
                    break;
                case -2:
                    Console.WriteLine($"Error: Group {nodeAddChangeSet.Group} doesn't exist.");
                    break;
                case -3:
                    Console.WriteLine($"Error: Invalid port number: {nodeAddChangeSet.Port}.");
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
            NodeEditChangeSet nodeEditChangeSet,
            CancellationToken cancellationToken = default)
        {
            var (loadedNodes, loadNodesErrMsg) = await Nodes.LoadNodesAsync(cancellationToken);
            if (loadNodesErrMsg is not null)
            {
                Console.WriteLine(loadNodesErrMsg);
                return 1;
            }
            using var nodes = loadedNodes;

            if (nodes.Groups.TryGetValue(nodeEditChangeSet.Group, out var targetGroup))
            {
                if (targetGroup.NodeDict.TryGetValue(nodeEditChangeSet.Nodename, out var node))
                {
                    if (!string.IsNullOrEmpty(nodeEditChangeSet.Host))
                        node.Host = nodeEditChangeSet.Host;

                    if (nodeEditChangeSet.Port > 0)
                        node.Port = nodeEditChangeSet.Port;

                    if (!string.IsNullOrEmpty(nodeEditChangeSet.PluginName))
                        node.Plugin = nodeEditChangeSet.PluginName;

                    if (!string.IsNullOrEmpty(nodeEditChangeSet.PluginVersion))
                        node.PluginVersion = nodeEditChangeSet.PluginVersion;

                    if (!string.IsNullOrEmpty(nodeEditChangeSet.PluginOptions))
                        node.PluginOpts = nodeEditChangeSet.PluginOptions;

                    if (!string.IsNullOrEmpty(nodeEditChangeSet.PluginArguments))
                        node.PluginArguments = nodeEditChangeSet.PluginArguments;

                    if (nodeEditChangeSet.UnsetPlugin)
                    {
                        node.Plugin = null;
                        node.PluginVersion = null;
                        node.PluginOpts = null;
                        node.PluginArguments = null;
                    }

                    if (!string.IsNullOrEmpty(nodeEditChangeSet.Owner))
                    {
                        var (users, loadUsersErrMsg) = await Users.LoadUsersAsync(cancellationToken);
                        if (loadUsersErrMsg is not null)
                        {
                            Console.WriteLine(loadUsersErrMsg);
                            return 1;
                        }

                        if (users.UserDict.TryGetValue(nodeEditChangeSet.Owner, out var targetUser))
                        {
                            node.OwnerUuid = targetUser.Uuid;
                        }
                        else
                        {
                            Console.WriteLine($"Warning: The specified owner {nodeEditChangeSet.Owner} is not a user. Skipping.");
                        }
                    }

                    if (nodeEditChangeSet.UnsetOwner)
                    {
                        node.OwnerUuid = null;
                    }

                    if (nodeEditChangeSet.ClearTags)
                    {
                        node.Tags.Clear();
                    }

                    if (nodeEditChangeSet.AddTags.Length > 0)
                    {
                        foreach (var tag in nodeEditChangeSet.AddTags)
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

                    if (nodeEditChangeSet.RemoveTags.Length > 0)
                    {
                        foreach (var tag in nodeEditChangeSet.RemoveTags)
                        {
                            if (node.Tags.RemoveAll(x => string.Equals(x, tag, StringComparison.OrdinalIgnoreCase)) == 0)
                            {
                                Console.WriteLine($"Warning: Tag {tag} doesn't exist.");
                            }
                        }
                    }

                    if (nodeEditChangeSet.IdentityPSKs.Length > 0)
                    {
                        node.IdentityPSKs = nodeEditChangeSet.IdentityPSKs.ToList();
                    }

                    if (nodeEditChangeSet.ClearIPSKs)
                    {
                        node.IdentityPSKs.Clear();
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
                    Console.WriteLine($"Error: Node {nodeEditChangeSet.Nodename} doesn't exist.");
                    return -1;
                }
            }
            else
            {
                Console.WriteLine($"Error: Group {nodeEditChangeSet.Group} doesn't exist.");
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
            var maxPluginVersionLength = filteredNodes.Max(x => x.node.PluginVersion?.Length);
            var maxPluginOptsLength = filteredNodes.Max(x => x.node.PluginOpts?.Length);
            var maxPluginArgumentsLength = filteredNodes.Max(x => x.node.PluginArguments?.Length);

            var nodeNameFieldWidth = maxNodeNameLength > 4 ? maxNodeNameLength + 2 : 6;
            var groupNameFieldWidth = maxGroupNameLength > 5 ? maxGroupNameLength + 2 : 7;
            var hostnameFieldWidth = maxHostnameLength > 4 ? maxHostnameLength + 2 : 6;

            // Nodes have no plugins. Do not display plugin and plugin options columns.
            if (maxPluginLength is null && maxPluginVersionLength is null && maxPluginOptsLength is null && maxPluginArgumentsLength is null)
            {
                ConsoleHelper.PrintTableBorder(6, nodeNameFieldWidth, groupNameFieldWidth, 36, hostnameFieldWidth, 5);
                Console.WriteLine($"|Status|{"Node".PadRight(nodeNameFieldWidth)}|{"Group".PadRight(groupNameFieldWidth)}|{"UUID",36}|{"Host".PadLeft(hostnameFieldWidth)}|{"Port",5}|");
                ConsoleHelper.PrintTableBorder(6, nodeNameFieldWidth, groupNameFieldWidth, 36, hostnameFieldWidth, 5);

                foreach (var (group, nodeName, node) in filteredNodes)
                {
                    Console.WriteLine($"|{(node.Deactivated ? "    🛑" : "    ✅")}|{nodeName.PadRight(nodeNameFieldWidth)}|{group.PadRight(groupNameFieldWidth)}|{node.Uuid,36}|{node.Host.PadLeft(hostnameFieldWidth)}|{node.Port,5}|");
                }

                ConsoleHelper.PrintTableBorder(6, nodeNameFieldWidth, groupNameFieldWidth, 36, hostnameFieldWidth, 5);
            }
            else // Nodes have plugins.
            {
                var pluginInfoLengths = new int?[] { maxPluginLength, maxPluginVersionLength, maxPluginArgumentsLength, };
                var pluginInfoLength = pluginInfoLengths.Sum();
                var pluginFieldWidth = pluginInfoLength > 6 ? pluginInfoLength.Value + 2 : 8;
                var pluginOptsFieldWidth = maxPluginOptsLength > 14 ? maxPluginOptsLength.Value + 2 : 16;

                ConsoleHelper.PrintTableBorder(6, nodeNameFieldWidth, groupNameFieldWidth, 36, hostnameFieldWidth, 5, pluginFieldWidth, pluginOptsFieldWidth);
                Console.WriteLine($"|Status|{"Node".PadRight(nodeNameFieldWidth)}|{"Group".PadRight(groupNameFieldWidth)}|{"UUID",36}|{"Host".PadLeft(hostnameFieldWidth)}|{"Port",5}|{"Plugin".PadLeft(pluginFieldWidth)}|{"Plugin Options".PadLeft(pluginOptsFieldWidth)}|");
                ConsoleHelper.PrintTableBorder(6, nodeNameFieldWidth, groupNameFieldWidth, 36, hostnameFieldWidth, 5, pluginFieldWidth, pluginOptsFieldWidth);

                foreach (var (group, nodeName, node) in filteredNodes)
                {
                    Console.WriteLine($"|{(node.Deactivated ? "    🛑" : "    ✅")}|{nodeName.PadRight(nodeNameFieldWidth)}|{group.PadRight(groupNameFieldWidth)}|{node.Uuid,36}|{node.Host.PadLeft(hostnameFieldWidth)}|{node.Port,5}|{($"{node.Plugin}{(node.PluginVersion is null ? "" : $"@{node.PluginVersion}")}{(node.PluginArguments is null ? "" : $" {node.PluginArguments}")}").PadLeft(pluginFieldWidth)}|{(node.PluginOpts ?? string.Empty).PadLeft(pluginOptsFieldWidth)}|");
                }

                ConsoleHelper.PrintTableBorder(6, nodeNameFieldWidth, groupNameFieldWidth, 36, hostnameFieldWidth, 5, pluginFieldWidth, pluginOptsFieldWidth);
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
                    var ownerUuid = nodeEntry.Value.OwnerUuid;
                    var owner = ownerUuid is null
                        ? "N/A"
                        : users.TryGetUserById(ownerUuid, out var userEntry)
                        ? userEntry.Key
                        : "N/A";
                    var tags = nodeEntry.Value.Tags;
                    var iPSKs = nodeEntry.Value.IdentityPSKs;

                    if (!onePerLine) // Full format
                    {
                        Console.WriteLine($"Group: {groupEntry.Key}");
                        Console.WriteLine($"Node:  {nodeEntry.Key}");

                        if (ownerUuid is not null)
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

                        Console.WriteLine($"iPSKs: {iPSKs.Count}");

                        foreach (var iPSK in iPSKs)
                        {
                            Console.WriteLine($"- {iPSK}");
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

                        Console.Write($" iPSKs {iPSKs.Count}");

                        foreach (var iPSK in iPSKs)
                        {
                            Console.Write($" {iPSK}");
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

        public static Task<int> AddTags(
            string[] tags,
            string[] groups,
            bool allGroups,
            string[] nodenames,
            bool allNodes,
            CancellationToken cancellationToken = default)
            => EditTags(groups, allGroups, nodenames, allNodes, false, tags, Array.Empty<string>(), cancellationToken);

        public static async Task<int> EditTags(
            string[] groups,
            bool allGroups,
            string[] nodenames,
            bool allNodes,
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

            foreach (var groupEntry in nodes.Groups)
            {
                if (!allGroups && !groups.Contains(groupEntry.Key))
                    continue;

                foreach (var nodeEntry in groupEntry.Value.NodeDict)
                {
                    if (!allNodes && !nodenames.Contains(nodeEntry.Key))
                        continue;

                    var nodename = nodeEntry.Key;
                    var node = nodeEntry.Value;

                    if (clearTags)
                    {
                        node.Tags.Clear();
                        Console.WriteLine($"Cleared all tags on node {nodename}.");
                    }

                    if (addTags.Length > 0)
                    {
                        foreach (var tag in addTags)
                        {
                            if (node.Tags.Exists(x => string.Equals(x, tag, StringComparison.OrdinalIgnoreCase)))
                            {
                                Console.WriteLine($"Warning: Tag {tag} already exists on node {nodename} in group {groupEntry.Key}. Skipping.");
                            }
                            else
                            {
                                node.Tags.Add(tag);
                                Console.WriteLine($"Added tag {tag} to node {nodename} in group {groupEntry.Key}.");
                            }
                        }
                    }

                    if (removeTags.Length > 0)
                    {
                        foreach (var tag in removeTags)
                        {
                            if (node.Tags.RemoveAll(x => string.Equals(x, tag, StringComparison.OrdinalIgnoreCase)) == 0)
                            {
                                Console.WriteLine($"Warning: Tag {tag} doesn't exist on node {nodename} in group {groupEntry.Key}.");
                            }
                            else
                            {
                                Console.WriteLine($"Removed tag {tag} from node {nodename} in group {groupEntry.Key}.");
                            }
                        }
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

        public static Task<int> RemoveTags(
            string[] tags,
            string[] groups,
            bool allGroups,
            string[] nodenames,
            bool allNodes,
            CancellationToken cancellationToken = default)
            => EditTags(groups, allGroups, nodenames, allNodes, false, Array.Empty<string>(), tags, cancellationToken);

        public static Task<int> ClearTags(
            string[] groups,
            bool allGroups,
            string[] nodenames,
            bool allNodes,
            CancellationToken cancellationToken = default)
            => EditTags(groups, allGroups, nodenames, allNodes, true, Array.Empty<string>(), Array.Empty<string>(), cancellationToken);

        public static Task<int> SetOwner(
            string owner,
            string[] groups,
            bool allGroups,
            string[] nodenames,
            bool allNodes,
            CancellationToken cancellationToken = default)
            => EditOwner(groups, allGroups, nodenames, allNodes, owner, false, cancellationToken);

        public static async Task<int> EditOwner(
            string[] groups,
            bool allGroups,
            string[] nodenames,
            bool allNodes,
            string owner,
            bool unsetOwner,
            CancellationToken cancellationToken = default)
        {
            var (loadedNodes, loadNodesErrMsg) = await Nodes.LoadNodesAsync(cancellationToken);
            if (loadNodesErrMsg is not null)
            {
                Console.WriteLine(loadNodesErrMsg);
                return 1;
            }
            using var nodes = loadedNodes;

            foreach (var groupEntry in nodes.Groups)
            {
                if (!allGroups && !groups.Contains(groupEntry.Key))
                    continue;

                foreach (var nodeEntry in groupEntry.Value.NodeDict)
                {
                    if (!allNodes && !nodenames.Contains(nodeEntry.Key))
                        continue;

                    var nodename = nodeEntry.Key;
                    var node = nodeEntry.Value;

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
                            Console.WriteLine($"Set user {owner} as owner of node {nodename} in group {groupEntry.Key}.");
                        }
                        else
                        {
                            Console.WriteLine($"Warning: The specified owner {owner} is not a user. Skipping.");
                        }
                    }

                    if (unsetOwner)
                    {
                        node.OwnerUuid = null;
                        Console.WriteLine($"Cleared ownership of node {nodename} in group {groupEntry.Key}.");
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

        public static Task<int> UnsetOwner(
            string[] groups,
            bool allGroups,
            string[] nodenames,
            bool allNodes,
            CancellationToken cancellationToken = default)
            => EditOwner(groups, allGroups, nodenames, allNodes, string.Empty, true, cancellationToken);
    }
}
