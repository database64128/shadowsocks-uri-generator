using ShadowsocksUriGenerator.Chatbot.Telegram.Utils;
using ShadowsocksUriGenerator.CLI.Utils;
using ShadowsocksUriGenerator.Data;
using System.Text;
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
            else if (message.From is null)
            {
                replyMarkdownV2 = @"Can't determine your Telegram user ID\. Are you sending from a channel?";
                Console.WriteLine(" Response: missing message sender.");
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

            _ = await botClient.SendPossiblyLongTextMessageAsync(
                message.Chat.Id,
                replyMarkdownV2,
                parseMode: ParseMode.MarkdownV2,
                replyParameters: message,
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

            if (message.From is null)
            {
                replyMarkdownV2 = @"Can't determine your Telegram user ID\. Are you sending from a channel?";
                Console.WriteLine(" Response: missing message sender.");
            }
            else if (botConfig.ChatAssociations.TryGetValue(message.From.Id, out var userUuid) && DataHelper.TryLocateUserFromUuid(userUuid, users, out var userEntry))
            {
                var (loadedNodes, loadNodesErrMsg) = await Nodes.LoadNodesAsync(cancellationToken);
                if (loadNodesErrMsg is not null)
                {
                    Console.WriteLine();
                    Console.WriteLine(loadNodesErrMsg);
                    return;
                }
                using var nodes = loadedNodes;

                List<(string group, string nodeName, Node node)> filteredNodes = [];

                foreach (var groupEntry in nodes.Groups)
                {
                    if ((argument is null || argument == groupEntry.Key) && (botConfig.UsersCanSeeAllGroups || groupEntry.Value.OwnerUuid == userUuid || userEntry.Value.Value.Memberships.ContainsKey(groupEntry.Key)))
                    {
                        foreach (var node in groupEntry.Value.NodeDict)
                        {
                            filteredNodes.Add((groupEntry.Key, node.Key, node.Value));
                        }
                    }
                }

                var replyBuilder = new StringBuilder();

                replyBuilder.AppendLine($"*Nodes: {filteredNodes.Count}*");

                if (filteredNodes.Count > 0)
                {
                    replyBuilder.AppendLine();

                    foreach (var (group, nodeName, node) in filteredNodes)
                    {
                        replyBuilder.AppendLine($"Group: *{ChatHelper.EscapeMarkdownV2Plaintext(group)}*");
                        replyBuilder.AppendLine($"Node: *{ChatHelper.EscapeMarkdownV2Plaintext(nodeName)}*");
                        replyBuilder.AppendLine($"UUID: {ChatHelper.EscapeMarkdownV2Plaintext(node.Uuid)}");
                        replyBuilder.AppendLine($"Status: {(node.Deactivated ? "🛑" : "✅")}");
                        replyBuilder.AppendLine($"Host: `{ChatHelper.EscapeMarkdownV2CodeBlock(node.Host)}`");
                        replyBuilder.AppendLine($"Port: {node.Port}");

                        if (node.Plugin is not null)
                        {
                            replyBuilder.AppendLine($"Plugin: `{ChatHelper.EscapeMarkdownV2CodeBlock(node.Plugin)}`");
                        }

                        if (node.PluginVersion is not null)
                        {
                            replyBuilder.AppendLine($"Plugin Version: `{ChatHelper.EscapeMarkdownV2CodeBlock(node.PluginVersion)}`");
                        }

                        if (node.PluginOpts is not null)
                        {
                            replyBuilder.AppendLine($"Plugin Options: `{ChatHelper.EscapeMarkdownV2CodeBlock(node.PluginOpts)}`");
                        }

                        if (node.PluginArguments is not null)
                        {
                            replyBuilder.AppendLine($"Plugin Arguments: `{ChatHelper.EscapeMarkdownV2CodeBlock(node.PluginArguments)}`");
                        }

                        if (node.OwnerUuid is not null && users.TryGetUserById(node.OwnerUuid, out var ownerEntry))
                        {
                            replyBuilder.AppendLine($"Owner: {ChatHelper.EscapeMarkdownV2Plaintext(ownerEntry.Key)}");
                        }

                        replyBuilder.AppendLine($"Tags: {node.Tags.Count}");

                        foreach (var tag in node.Tags)
                        {
                            replyBuilder.AppendLine(ChatHelper.EscapeMarkdownV2Plaintext($"- {tag}"));
                        }

                        replyBuilder.AppendLine($"iPSKs: {node.IdentityPSKs.Count}");

                        foreach (var iPSK in node.IdentityPSKs)
                        {
                            replyBuilder.AppendLine($"\\- `{ChatHelper.EscapeMarkdownV2CodeBlock(iPSK)}`");
                        }

                        replyBuilder.AppendLine();
                    }
                }

                replyMarkdownV2 = replyBuilder.ToString();
                Console.WriteLine(" Response: successful query.");
            }
            else
            {
                replyMarkdownV2 = @"You must link your Telegram account to your user first\.";
                Console.WriteLine(" Response: user not linked.");
            }

            _ = await botClient.SendPossiblyLongTextMessageAsync(
                message.Chat.Id,
                replyMarkdownV2,
                parseMode: ParseMode.MarkdownV2,
                replyParameters: message,
                cancellationToken: cancellationToken);
        }

        public static async Task ListOwnedNodesAsync(ITelegramBotClient botClient, Message message, string? argument, CancellationToken cancellationToken = default)
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

            if (!botConfig.UsersCanSeeAllUsers || !botConfig.UsersCanSeeAllGroups)
            {
                replyMarkdownV2 = @"The admin has disabled the command\.";
                Console.WriteLine(" Response: permission denied.");
            }
            else if (message.From is null)
            {
                replyMarkdownV2 = @"Can't determine your Telegram user ID\. Are you sending from a channel?";
                Console.WriteLine(" Response: missing message sender.");
            }
            else if (botConfig.ChatAssociations.TryGetValue(message.From.Id, out var userUuid) && DataHelper.TryLocateUserFromUuid(userUuid, users, out var userEntry))
            {
                string? targetUserUuid = null;

                if (string.IsNullOrEmpty(argument))
                {
                    targetUserUuid = userUuid;
                }
                else if (users.UserDict.TryGetValue(argument, out var targetUser))
                {
                    targetUserUuid = targetUser.Uuid;
                }

                if (targetUserUuid is not null)
                {
                    var (loadedNodes, loadNodesErrMsg) = await Nodes.LoadNodesAsync(cancellationToken);
                    if (loadNodesErrMsg is not null)
                    {
                        Console.WriteLine();
                        Console.WriteLine(loadNodesErrMsg);
                        return;
                    }
                    using var nodes = loadedNodes;

                    var replyBuilder = new StringBuilder();

                    var totalCount = 0;

                    foreach (var groupEntry in nodes.Groups)
                    {
                        var ownedNodeEntries = groupEntry.Value.NodeDict.Where(x => x.Value.OwnerUuid == targetUserUuid);

                        if (ownedNodeEntries.Any())
                        {
                            totalCount += ownedNodeEntries.Count();

                            replyBuilder.AppendLine($"*From group {ChatHelper.EscapeMarkdownV2Plaintext(groupEntry.Key)}: {ownedNodeEntries.Count()}*");

                            foreach (var nodeEntry in ownedNodeEntries)
                            {
                                replyBuilder.AppendLine(ChatHelper.EscapeMarkdownV2Plaintext(nodeEntry.Key));
                            }

                            replyBuilder.AppendLine();
                        }
                    }

                    replyBuilder.AppendLine($"*Total owned nodes: {totalCount}*");

                    replyMarkdownV2 = replyBuilder.ToString();
                    Console.WriteLine(" Response: successful query.");
                }
                else
                {
                    replyMarkdownV2 = @"The specified user doesn't exist\.";
                    Console.WriteLine(" Response: target user not found.");
                }
            }
            else
            {
                replyMarkdownV2 = @"You must link your Telegram account to your user first\.";
                Console.WriteLine(" Response: user not linked.");
            }

            _ = await botClient.SendPossiblyLongTextMessageAsync(
                message.Chat.Id,
                replyMarkdownV2,
                parseMode: ParseMode.MarkdownV2,
                replyParameters: message,
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

            if (message.From is null)
            {
                replyMarkdownV2 = @"Can't determine your Telegram user ID\. Are you sending from a channel?";
                Console.WriteLine(" Response: missing message sender.");
            }
            else if (botConfig.ChatAssociations.TryGetValue(message.From.Id, out var userUuid) && DataHelper.TryLocateUserFromUuid(userUuid, users, out var userEntry))
            {
                var (loadedNodes, loadNodesErrMsg) = await Nodes.LoadNodesAsync(cancellationToken);
                if (loadNodesErrMsg is not null)
                {
                    Console.WriteLine();
                    Console.WriteLine(loadNodesErrMsg);
                    return;
                }
                using var nodes = loadedNodes;

                List<(string group, int nodeCount, string owner)> tableEntries = [];

                foreach (var groupEntry in nodes.Groups)
                {
                    // Add group if either:
                    // 1. Explicitly allowed by setting.
                    // 2. User is group owner.
                    // 3. User is group member.
                    if (botConfig.UsersCanSeeAllGroups || groupEntry.Value.OwnerUuid == userUuid || userEntry.Value.Value.Memberships.ContainsKey(groupEntry.Key))
                    {
                        if (groupEntry.Value.OwnerUuid == userUuid) // no need to lookup username
                        {
                            tableEntries.Add((groupEntry.Key, groupEntry.Value.NodeDict.Count, userEntry.Value.Key));
                        }
                        else if (groupEntry.Value.OwnerUuid is not null && users.TryGetUserById(groupEntry.Value.OwnerUuid, out var ownerEntry))
                        {
                            tableEntries.Add((groupEntry.Key, groupEntry.Value.NodeDict.Count, ownerEntry.Key));
                        }
                        else
                        {
                            tableEntries.Add((groupEntry.Key, groupEntry.Value.NodeDict.Count, "N/A"));
                        }
                    }
                }

                var replyBuilder = new StringBuilder();

                replyBuilder.AppendLine($"*Groups: {tableEntries.Count}*");

                if (tableEntries.Count > 0)
                {
                    var maxGroupNameLength = tableEntries.Max(x => x.group.Length);
                    var maxOwnerNameLength = tableEntries.Max(x => x.owner?.Length ?? 0);
                    var groupNameFieldWidth = maxGroupNameLength > 5 ? maxGroupNameLength + 2 : 7;
                    var ownerNameFieldWidth = maxOwnerNameLength > 5 ? maxOwnerNameLength + 2 : 7;

                    replyBuilder.AppendLine("```");
                    replyBuilder.AppendTableBorder(groupNameFieldWidth, 6, ownerNameFieldWidth);
                    replyBuilder.AppendLine($"|{"Group".PadRight(groupNameFieldWidth)}|{"Nodes",6}|{"Owner".PadLeft(ownerNameFieldWidth)}|");
                    replyBuilder.AppendTableBorder(groupNameFieldWidth, 6, ownerNameFieldWidth);

                    foreach (var (group, nodeCount, owner) in tableEntries)
                    {
                        replyBuilder.AppendLine($"|{ChatHelper.EscapeMarkdownV2CodeBlock(group).PadRight(groupNameFieldWidth)}|{nodeCount,6}|{ChatHelper.EscapeMarkdownV2CodeBlock(owner).PadLeft(ownerNameFieldWidth)}|");
                    }

                    replyBuilder.AppendTableBorder(groupNameFieldWidth, 6, ownerNameFieldWidth);
                    replyBuilder.AppendLine("```");
                }

                replyMarkdownV2 = replyBuilder.ToString();
                Console.WriteLine(" Response: successful query.");
            }
            else
            {
                replyMarkdownV2 = @"You must link your Telegram account to your user first\.";
                Console.WriteLine(" Response: user not linked.");
            }

            _ = await botClient.SendPossiblyLongTextMessageAsync(
                message.Chat.Id,
                replyMarkdownV2,
                parseMode: ParseMode.MarkdownV2,
                replyParameters: message,
                cancellationToken: cancellationToken);
        }

        public static async Task ListOwnedGroupsAsync(ITelegramBotClient botClient, Message message, string? argument, CancellationToken cancellationToken = default)
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

            if (!botConfig.UsersCanSeeAllUsers || !botConfig.UsersCanSeeAllGroups)
            {
                replyMarkdownV2 = @"The admin has disabled the command\.";
                Console.WriteLine(" Response: permission denied.");
            }
            else if (message.From is null)
            {
                replyMarkdownV2 = @"Can't determine your Telegram user ID\. Are you sending from a channel?";
                Console.WriteLine(" Response: missing message sender.");
            }
            else if (botConfig.ChatAssociations.TryGetValue(message.From.Id, out var userUuid) && DataHelper.TryLocateUserFromUuid(userUuid, users, out var userEntry))
            {
                string? targetUserUuid = null;

                if (string.IsNullOrEmpty(argument))
                {
                    targetUserUuid = userUuid;
                }
                else if (users.UserDict.TryGetValue(argument, out var targetUser))
                {
                    targetUserUuid = targetUser.Uuid;
                }

                if (targetUserUuid is not null)
                {
                    var (loadedNodes, loadNodesErrMsg) = await Nodes.LoadNodesAsync(cancellationToken);
                    if (loadNodesErrMsg is not null)
                    {
                        Console.WriteLine();
                        Console.WriteLine(loadNodesErrMsg);
                        return;
                    }
                    using var nodes = loadedNodes;

                    var replyBuilder = new StringBuilder();

                    var ownedGroupEntries = nodes.Groups.Where(x => x.Value.OwnerUuid == targetUserUuid);

                    replyBuilder.AppendLine($"*Owned Groups: {ownedGroupEntries.Count()}*");

                    foreach (var groupEntry in ownedGroupEntries)
                    {
                        replyBuilder.AppendLine(ChatHelper.EscapeMarkdownV2Plaintext(groupEntry.Key));
                    }

                    replyMarkdownV2 = replyBuilder.ToString();
                    Console.WriteLine(" Response: successful query.");
                }
                else
                {
                    replyMarkdownV2 = @"The specified user doesn't exist\.";
                    Console.WriteLine(" Response: target user not found.");
                }
            }
            else
            {
                replyMarkdownV2 = @"You must link your Telegram account to your user first\.";
                Console.WriteLine(" Response: user not linked.");
            }

            _ = await botClient.SendPossiblyLongTextMessageAsync(
                message.Chat.Id,
                replyMarkdownV2,
                parseMode: ParseMode.MarkdownV2,
                replyParameters: message,
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
            else if (message.From is null)
            {
                replyMarkdownV2 = @"Can't determine your Telegram user ID\. Are you sending from a channel?";
                Console.WriteLine(" Response: missing message sender.");
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

            _ = await botClient.SendPossiblyLongTextMessageAsync(
                message.Chat.Id,
                replyMarkdownV2,
                parseMode: ParseMode.MarkdownV2,
                replyParameters: message,
                cancellationToken: cancellationToken);
        }
    }
}
