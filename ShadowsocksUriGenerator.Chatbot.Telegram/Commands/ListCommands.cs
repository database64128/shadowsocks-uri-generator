using ShadowsocksUriGenerator.Chatbot.Telegram.Utils;
using ShadowsocksUriGenerator.CLI.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ShadowsocksUriGenerator.Chatbot.Telegram.Commands
{
    public static class ListCommands
    {
        public static async Task ListUsersAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken = default)
        {
            Console.Write($"{message.From} executed {message.Text} in {message.Chat.Type.ToString().ToLower()} chat {(string.IsNullOrEmpty(message.Chat.Title) ? string.Empty : $"{message.Chat.Title} ")}({message.Chat.Id}).");

            string replyMarkdownV2;

            var (botConfig, loadBotConfigErrMsg) = await BotConfig.LoadBotConfigAsync(cancellationToken);
            if (loadBotConfigErrMsg is not null)
            {
                Console.WriteLine();
                Console.WriteLine(loadBotConfigErrMsg);
                return;
            }

            var (users, loadUsersErrMsg) = await Users.LoadUsersAsync(cancellationToken);
            if (loadUsersErrMsg is not null)
            {
                Console.WriteLine();
                Console.WriteLine(loadUsersErrMsg);
                return;
            }

            if (!botConfig.UsersCanSeeAllUsers)
            {
                replyMarkdownV2 = @"The admin has disabled the command\.";
                Console.WriteLine(" Response: permission denied.");
            }
            else if (botConfig.ChatAssociations.TryGetValue(message.From.Id, out var userUuid) && DataHelper.TryLocateUserFromUuid(userUuid, users, out _))
            {
                var replyBuilder = new StringBuilder();

                var maxNameLength = users.UserDict.Select(x => x.Key.Length)
                                                  .DefaultIfEmpty()
                                                  .Max();
                var nameFieldWidth = maxNameLength > 4 ? maxNameLength + 2 : 6;

                replyBuilder.AppendLine("```");
                replyBuilder.AppendTableBorder(nameFieldWidth, 18);
                replyBuilder.AppendLine($"|{"User".PadRight(nameFieldWidth)}|{"Associated Groups",18}|");
                replyBuilder.AppendTableBorder(nameFieldWidth, 18);

                foreach (var user in users.UserDict)
                    replyBuilder.AppendLine($"|{ChatHelper.EscapeMarkdownV2CodeBlock(user.Key).PadRight(nameFieldWidth)}|{user.Value.Memberships.Count,18}|");

                replyBuilder.AppendTableBorder(nameFieldWidth, 18);
                replyBuilder.AppendLine("```");

                replyMarkdownV2 = replyBuilder.ToString();
                Console.WriteLine(" Response: successful query.");
            }
            else
            {
                replyMarkdownV2 = @"You must link your Telegram account to your user first\.";
                Console.WriteLine(" Response: user not linked.");
            }

            await botClient.SendPossiblyLongTextMessageAsync(message.Chat.Id,
                                                             replyMarkdownV2,
                                                             ParseMode.MarkdownV2,
                                                             replyToMessageId: message.MessageId,
                                                             cancellationToken: cancellationToken);
        }

        public static async Task ListNodesAsync(ITelegramBotClient botClient, Message message, string? argument, CancellationToken cancellationToken = default)
        {
            Console.Write($"{message.From} executed {message.Text} in {message.Chat.Type.ToString().ToLower()} chat {(string.IsNullOrEmpty(message.Chat.Title) ? string.Empty : $"{message.Chat.Title} ")}({message.Chat.Id}).");

            string replyMarkdownV2;

            var (botConfig, loadBotConfigErrMsg) = await BotConfig.LoadBotConfigAsync(cancellationToken);
            if (loadBotConfigErrMsg is not null)
            {
                Console.WriteLine();
                Console.WriteLine(loadBotConfigErrMsg);
                return;
            }

            var (users, loadUsersErrMsg) = await Users.LoadUsersAsync(cancellationToken);
            if (loadUsersErrMsg is not null)
            {
                Console.WriteLine();
                Console.WriteLine(loadUsersErrMsg);
                return;
            }

            if (botConfig.ChatAssociations.TryGetValue(message.From.Id, out var userUuid) && DataHelper.TryLocateUserFromUuid(userUuid, users, out var userEntry))
            {
                var (loadedNodes, loadNodesErrMsg) = await Nodes.LoadNodesAsync(cancellationToken);
                if (loadNodesErrMsg is not null)
                {
                    Console.WriteLine();
                    Console.WriteLine(loadNodesErrMsg);
                    return;
                }
                using var nodes = loadedNodes;

                var userGroups = userEntry.Value.Value.Memberships.Keys.ToList();

                List<(string group, string nodeName, Node node)> filteredNodes = new();

                foreach (var groupEntry in nodes.Groups)
                {
                    if ((argument is null || argument == groupEntry.Key) && (botConfig.UsersCanSeeAllGroups || userGroups.Contains(groupEntry.Key)))
                    {
                        foreach (var node in groupEntry.Value.NodeDict)
                        {
                            filteredNodes.Add((groupEntry.Key, node.Key, node.Value));
                        }
                    }
                }

                var replyBuilder = new StringBuilder();
                replyBuilder.AppendLine("```");
                replyBuilder.AppendLine($"{"Nodes",-16}{filteredNodes.Count}");

                if (filteredNodes.Count > 0)
                {
                    replyBuilder.AppendLine();

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
                        replyBuilder.AppendTableBorder(7, nodeNameFieldWidth, groupNameFieldWidth, 36, hostnameFieldWidth, 5);
                        replyBuilder.AppendLine($"|{"Status",7}|{"Node".PadRight(nodeNameFieldWidth)}|{"Group".PadRight(groupNameFieldWidth)}|{"UUID",36}|{"Host".PadLeft(hostnameFieldWidth)}|{"Port",5}|");
                        replyBuilder.AppendTableBorder(7, nodeNameFieldWidth, groupNameFieldWidth, 36, hostnameFieldWidth, 5);

                        foreach (var (group, nodeName, node) in filteredNodes)
                        {
                            replyBuilder.AppendLine($"|{(node.Deactivated ? "🛑" : "✔"),7}|{ChatHelper.EscapeMarkdownV2CodeBlock(nodeName).PadRight(nodeNameFieldWidth)}|{ChatHelper.EscapeMarkdownV2CodeBlock(group).PadRight(groupNameFieldWidth)}|{node.Uuid,36}|{node.Host.PadLeft(hostnameFieldWidth)}|{node.Port,5}|");
                        }

                        replyBuilder.AppendTableBorder(7, nodeNameFieldWidth, groupNameFieldWidth, 36, hostnameFieldWidth, 5);
                    }
                    else // Nodes have plugins.
                    {
                        var pluginFieldWidth = maxPluginLength > 6 ? maxPluginLength.Value + 2 : 8;
                        var pluginOptsFieldWidth = maxPluginOptsLength > 14 ? maxPluginOptsLength.Value + 2 : 16;

                        replyBuilder.AppendTableBorder(7, nodeNameFieldWidth, groupNameFieldWidth, 36, hostnameFieldWidth, 5, pluginFieldWidth, pluginOptsFieldWidth);
                        replyBuilder.AppendLine($"|{"Status",7}|{"Node".PadRight(nodeNameFieldWidth)}|{"Group".PadRight(groupNameFieldWidth)}|{"UUID",36}|{"Host".PadLeft(hostnameFieldWidth)}|{"Port",5}|{"Plugin".PadLeft(pluginFieldWidth)}|{"Plugin Options".PadLeft(pluginOptsFieldWidth)}|");
                        replyBuilder.AppendTableBorder(7, nodeNameFieldWidth, groupNameFieldWidth, 36, hostnameFieldWidth, 5, pluginFieldWidth, pluginOptsFieldWidth);

                        foreach (var (group, nodeName, node) in filteredNodes)
                        {
                            replyBuilder.AppendLine($"|{(node.Deactivated ? "🛑" : "✔"),7}|{ChatHelper.EscapeMarkdownV2CodeBlock(nodeName).PadRight(nodeNameFieldWidth)}|{ChatHelper.EscapeMarkdownV2CodeBlock(group).PadRight(groupNameFieldWidth)}|{node.Uuid,36}|{node.Host.PadLeft(hostnameFieldWidth)}|{node.Port,5}|{ChatHelper.EscapeMarkdownV2CodeBlock(node.Plugin ?? string.Empty).PadLeft(pluginFieldWidth)}|{ChatHelper.EscapeMarkdownV2CodeBlock(node.PluginOpts ?? string.Empty).PadLeft(pluginOptsFieldWidth)}|");
                        }

                        replyBuilder.AppendTableBorder(7, nodeNameFieldWidth, groupNameFieldWidth, 36, hostnameFieldWidth, 5, pluginFieldWidth, pluginOptsFieldWidth);
                    }
                }

                replyBuilder.AppendLine("```");
                replyMarkdownV2 = replyBuilder.ToString();
                Console.WriteLine(" Response: successful query.");
            }
            else
            {
                replyMarkdownV2 = @"You must link your Telegram account to your user first\.";
                Console.WriteLine(" Response: user not linked.");
            }

            await botClient.SendPossiblyLongTextMessageAsync(message.Chat.Id,
                                                             replyMarkdownV2,
                                                             ParseMode.MarkdownV2,
                                                             replyToMessageId: message.MessageId,
                                                             cancellationToken: cancellationToken);
        }

        public static async Task ListGroupsAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken = default)
        {
            Console.Write($"{message.From} executed {message.Text} in {message.Chat.Type.ToString().ToLower()} chat {(string.IsNullOrEmpty(message.Chat.Title) ? string.Empty : $"{message.Chat.Title} ")}({message.Chat.Id}).");

            string replyMarkdownV2;

            var (botConfig, loadBotConfigErrMsg) = await BotConfig.LoadBotConfigAsync(cancellationToken);
            if (loadBotConfigErrMsg is not null)
            {
                Console.WriteLine();
                Console.WriteLine(loadBotConfigErrMsg);
                return;
            }

            var (users, loadUsersErrMsg) = await Users.LoadUsersAsync(cancellationToken);
            if (loadUsersErrMsg is not null)
            {
                Console.WriteLine();
                Console.WriteLine(loadUsersErrMsg);
                return;
            }

            if (botConfig.ChatAssociations.TryGetValue(message.From.Id, out var userUuid) && DataHelper.TryLocateUserFromUuid(userUuid, users, out var userEntry))
            {
                var (loadedNodes, loadNodesErrMsg) = await Nodes.LoadNodesAsync(cancellationToken);
                if (loadNodesErrMsg is not null)
                {
                    Console.WriteLine();
                    Console.WriteLine(loadNodesErrMsg);
                    return;
                }
                using var nodes = loadedNodes;

                var userGroups = userEntry.Value.Value.Memberships.Keys.ToList();
                var replyBuilder = new StringBuilder();

                var maxGroupNameLength = nodes.Groups.Select(x => x.Key.Length)
                                                     .DefaultIfEmpty()
                                                     .Max();
                var maxOutlineServerNameLength = nodes.Groups.Select(x => x.Value.OutlineServerInfo?.Name.Length ?? 0)
                                                             .DefaultIfEmpty()
                                                             .Max();
                var groupNameFieldWidth = maxGroupNameLength > 5 ? maxGroupNameLength + 2 : 7;
                var outlineServerNameFieldWidth = maxOutlineServerNameLength > 14 ? maxOutlineServerNameLength + 2 : 16;

                replyBuilder.AppendLine("```");
                replyBuilder.AppendTableBorder(groupNameFieldWidth, 16, outlineServerNameFieldWidth);
                replyBuilder.AppendLine($"|{"Group".PadRight(groupNameFieldWidth)}|{"Number of Nodes",16}|{"Outline Server".PadLeft(outlineServerNameFieldWidth)}|");
                replyBuilder.AppendTableBorder(groupNameFieldWidth, 16, outlineServerNameFieldWidth);

                foreach (var groupEntry in nodes.Groups)
                    if (botConfig.UsersCanSeeAllGroups || userGroups.Contains(groupEntry.Key))
                        replyBuilder.AppendLine($"|{ChatHelper.EscapeMarkdownV2CodeBlock(groupEntry.Key).PadRight(groupNameFieldWidth)}|{groupEntry.Value.NodeDict.Count,16}|{ChatHelper.EscapeMarkdownV2CodeBlock(groupEntry.Value.OutlineServerInfo?.Name ?? "No").PadLeft(outlineServerNameFieldWidth)}|");

                replyBuilder.AppendTableBorder(groupNameFieldWidth, 16, outlineServerNameFieldWidth);
                replyBuilder.AppendLine("```");
                replyMarkdownV2 = replyBuilder.ToString();
                Console.WriteLine(" Response: successful query.");
            }
            else
            {
                replyMarkdownV2 = @"You must link your Telegram account to your user first\.";
                Console.WriteLine(" Response: user not linked.");
            }

            await botClient.SendPossiblyLongTextMessageAsync(message.Chat.Id,
                                                             replyMarkdownV2,
                                                             ParseMode.MarkdownV2,
                                                             replyToMessageId: message.MessageId,
                                                             cancellationToken: cancellationToken);
        }

        public static async Task ListGroupMembersAsync(ITelegramBotClient botClient, Message message, string? argument, CancellationToken cancellationToken = default)
        {
            Console.Write($"{message.From} executed {message.Text} in {message.Chat.Type.ToString().ToLower()} chat {(string.IsNullOrEmpty(message.Chat.Title) ? string.Empty : $"{message.Chat.Title} ")}({message.Chat.Id}).");

            string replyMarkdownV2;

            var (botConfig, loadBotConfigErrMsg) = await BotConfig.LoadBotConfigAsync(cancellationToken);
            if (loadBotConfigErrMsg is not null)
            {
                Console.WriteLine();
                Console.WriteLine(loadBotConfigErrMsg);
                return;
            }

            var (users, loadUsersErrMsg) = await Users.LoadUsersAsync(cancellationToken);
            if (loadUsersErrMsg is not null)
            {
                Console.WriteLine();
                Console.WriteLine(loadUsersErrMsg);
                return;
            }

            if (!botConfig.UsersCanSeeAllUsers)
            {
                replyMarkdownV2 = @"The admin has disabled the command\.";
                Console.WriteLine(" Response: permission denied.");
            }
            else if (string.IsNullOrEmpty(argument))
            {
                replyMarkdownV2 = @"Please specify a group\.";
                Console.WriteLine(" Response: missing argument.");
            }
            else if (botConfig.ChatAssociations.TryGetValue(message.From.Id, out var userUuid) && DataHelper.TryLocateUserFromUuid(userUuid, users, out _))
            {
                var memberCount = 0;
                var memberListBuilder = new StringBuilder();

                foreach (var user in users.UserDict)
                {
                    if (user.Value.Memberships.ContainsKey(argument))
                    {
                        memberListBuilder.AppendLine(ChatHelper.EscapeMarkdownV2Plaintext($"- {user.Key}"));
                        memberCount++;
                    }
                }

                var replyBuilder = new StringBuilder();
                replyBuilder.AppendLine($"*Group: {ChatHelper.EscapeMarkdownV2Plaintext(argument)}*");
                replyBuilder.AppendLine($"*Members: {memberCount}*");
                replyBuilder.Append(memberListBuilder);

                replyMarkdownV2 = replyBuilder.ToString();
                Console.WriteLine(" Response: successful query.");
            }
            else
            {
                replyMarkdownV2 = @"You must link your Telegram account to your user first\.";
                Console.WriteLine(" Response: user not linked.");
            }

            await botClient.SendPossiblyLongTextMessageAsync(message.Chat.Id,
                                                             replyMarkdownV2,
                                                             ParseMode.MarkdownV2,
                                                             replyToMessageId: message.MessageId,
                                                             cancellationToken: cancellationToken);
        }
    }
}
