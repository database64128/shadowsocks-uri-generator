﻿using ShadowsocksUriGenerator.CLI.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ShadowsocksUriGenerator.CLI
{
    public static class NodeCommand
    {
        public static async Task<int> Add(
            string group,
            string nodename,
            string host,
            string portString,
            string? plugin,
            string? pluginOpts,
            CancellationToken cancellationToken = default)
        {
            using var nodes = await JsonHelper.LoadNodesAsync(cancellationToken);

            var result = nodes.AddNodeToGroup(group, nodename, host, portString, plugin, pluginOpts);
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
                    Console.WriteLine($"Error: Invalid port number: {portString}.");
                    break;
                default:
                    Console.WriteLine($"Unknown error.");
                    break;
            }

            await JsonHelper.SaveNodesAsync(nodes, cancellationToken);
            return result;
        }

        public static async Task<int> Rename(string group, string oldName, string newName, CancellationToken cancellationToken = default)
        {
            using var nodes = await JsonHelper.LoadNodesAsync(cancellationToken);

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

            await JsonHelper.SaveNodesAsync(nodes, cancellationToken);
            return result;
        }

        public static async Task<int> Remove(string group, string[] nodenames, CancellationToken cancellationToken = default)
        {
            var commandResult = 0;
            using var nodes = await JsonHelper.LoadNodesAsync(cancellationToken);

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

            await JsonHelper.SaveNodesAsync(nodes, cancellationToken);
            return commandResult;
        }

        public static async Task List(string[]? groups, bool namesOnly, bool onePerLine, CancellationToken cancellationToken = default)
        {
            using var nodes = await JsonHelper.LoadNodesAsync(cancellationToken);

            if (namesOnly)
            {
                foreach (var groupEntry in nodes.Groups)
                {
                    if (groups != null && !groups.Contains(groupEntry.Key))
                        continue;

                    Console.WriteLine($"Group: {groupEntry.Key}");
                    var keys = groupEntry.Value.NodeDict.Keys.ToList();
                    ConsoleHelper.PrintNameList(keys, onePerLine);
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

            ConsoleHelper.PrintTableBorder(7, nodeNameFieldWidth, groupNameFieldWidth, 36, hostnameFieldWidth, 5, pluginFieldWidth, pluginOptsFieldWidth);
            Console.WriteLine($"|{"Status",7}|{"Node".PadRight(nodeNameFieldWidth)}|{"Group".PadRight(groupNameFieldWidth)}|{"UUID",36}|{"Host".PadLeft(hostnameFieldWidth)}|{"Port",5}|{"Plugin".PadLeft(pluginFieldWidth)}|{"Plugin Options".PadLeft(pluginOptsFieldWidth)}|");
            ConsoleHelper.PrintTableBorder(7, nodeNameFieldWidth, groupNameFieldWidth, 36, hostnameFieldWidth, 5, pluginFieldWidth, pluginOptsFieldWidth);

            foreach (var groupEntry in nodes.Groups)
            {
                if (groups != null && !groups.Contains(groupEntry.Key))
                    continue;

                foreach (var node in groupEntry.Value.NodeDict)
                    PrintNodeInfo(node, groupEntry.Key);
            }

            ConsoleHelper.PrintTableBorder(7, nodeNameFieldWidth, groupNameFieldWidth, 36, hostnameFieldWidth, 5, pluginFieldWidth, pluginOptsFieldWidth);

            void PrintNodeInfo(KeyValuePair<string, Node> node, string group)
            {
                Console.WriteLine($"|{(node.Value.Deactivated ? "🛑" : "✔"),7}|{node.Key.PadRight(nodeNameFieldWidth)}|{group.PadRight(groupNameFieldWidth)}|{node.Value.Uuid,36}|{node.Value.Host.PadLeft(hostnameFieldWidth)}|{node.Value.Port,5}|{(node.Value.Plugin ?? string.Empty).PadLeft(pluginFieldWidth)}|{(node.Value.PluginOpts ?? string.Empty).PadLeft(pluginOptsFieldWidth)}|");
            }
        }

        public static async Task<int> Activate(string group, string[] nodenames, bool allNodes, CancellationToken cancellationToken = default)
        {
            var commandResult = 0;
            using var nodes = await JsonHelper.LoadNodesAsync(cancellationToken);

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

            await JsonHelper.SaveNodesAsync(nodes, cancellationToken);
            return commandResult;
        }

        public static async Task<int> Deactivate(string group, string[] nodenames, bool allNodes, CancellationToken cancellationToken = default)
        {
            var commandResult = 0;
            using var nodes = await JsonHelper.LoadNodesAsync(cancellationToken);

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

            await JsonHelper.SaveNodesAsync(nodes, cancellationToken);
            return commandResult;
        }
    }
}
