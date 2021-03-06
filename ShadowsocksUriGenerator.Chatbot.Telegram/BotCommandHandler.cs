﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ShadowsocksUriGenerator.Chatbot.Telegram
{
    public static class BotCommandHandler
    {
        public static Task Handle(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken = default)
        {
            var text = message.Text;

            // Empty message
            if (string.IsNullOrWhiteSpace(text))
                return Task.CompletedTask;

            // Not a command
            if (!text.StartsWith('/') || text.Length < 2)
                return Task.CompletedTask;

            // Remove the leading '/'
            text = text[1..];

            // Split command and argument
            var parsedText = text.Split(' ', 2);
            string command;
            string? argument = null;
            switch (parsedText.Length)
            {
                case <= 0:
                    return Task.CompletedTask;
                case 2:
                    argument = parsedText[1];
                    goto default;
                default:
                    command = parsedText[0];
                    break;
            }

            // Remove trailing '@bot' from command
            var atSignIndex = command.IndexOf('@');
            if (atSignIndex != -1)
                command = command[..atSignIndex];

            // Trim quotes and spaces from argument
            if (argument != null)
            {
                argument = argument.Trim();
                argument = argument.Trim('\'', '"');
            }

            // Handle command
            return command switch
            {
                "start" => HandleStartCommand(botClient, message, cancellationToken),
                "link" => HandleLinkCommand(botClient, message, argument, cancellationToken),
                "unlink" => HandleUnlinkCommand(botClient, message, cancellationToken),
                "list_users" => HandleListUsersCommand(botClient, message, cancellationToken),
                "list_nodes" => HandleListNodesCommand(botClient, message, argument, cancellationToken),
                "list_groups" => HandleListGroupsCommand(botClient, message, cancellationToken),
                "list_group_members" => HandleListGroupMembersCommand(botClient, message, argument, cancellationToken),
                "get_user_data_usage" => HandleGetUserDataUsageCommand(botClient, message, argument, cancellationToken),
                "get_group_data_usage" => HandleGetGroupDataUsageCommand(botClient, message, argument, cancellationToken),
                "get_ss_links" => HandleGetSsLinksCommand(botClient, message, cancellationToken),
                "get_sip008_links" => HandleGetSip008LinksCommand(botClient, message, cancellationToken),
                "get_credentials" => HandleGetCredentialsCommand(botClient, message, cancellationToken),
                "report" => HandleReportCommand(botClient, message, cancellationToken),
                _ => Task.CompletedTask, // unrecognized command, ignoring
            };
        }

        public static async Task HandleStartCommand(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"{message.From} executed {message.Text} in {message.Chat.Type.ToString().ToLower()} chat {(string.IsNullOrEmpty(message.Chat.Title) ? string.Empty : $"{message.Chat.Title} ")}({message.Chat.Id}).");
            var reply = @"🧑‍✈️ Good evening\! Thank you for choosing QDA\.

✈️ To get your boarding pass, please use `/link <UUID>` to link your Telegram account to your user\.";
            await ReplyToMessage(botClient, message, reply, ParseMode.MarkdownV2, cancellationToken: cancellationToken);
        }

        public static async Task HandleLinkCommand(ITelegramBotClient botClient, Message message, string? argument, CancellationToken cancellationToken = default)
        {
            Console.Write($"{message.From} executed {message.Text} in {message.Chat.Type.ToString().ToLower()} chat {(string.IsNullOrEmpty(message.Chat.Title) ? string.Empty : $"{message.Chat.Title} ")}({message.Chat.Id}).");
            string reply;
            var botConfig = await BotConfig.LoadBotConfigAsync();
            if (!botConfig.AllowChatAssociation)
            {
                reply = @"The admin has disabled Telegram association\.";
                Console.WriteLine(" Response: command disabled.");
            }
            else if (message.Chat.Type != ChatType.Private)
            {
                reply = @"Associations must be made in a private chat\.";
                Console.WriteLine(" Response: not in private chat.");
            }
            else if (string.IsNullOrEmpty(argument))
            {
                reply = @"Please provide your user UUID as the command argument\. You may find your user UUID from the JSON filename in your SIP008 delivery link\.";
                Console.WriteLine(" Response: missing argument.");
            }
            else
            {
                var users = await Users.LoadUsersAsync();
                var userSearchResult = users.UserDict.Where(x => string.Equals(x.Value.Uuid, argument, StringComparison.OrdinalIgnoreCase));
                KeyValuePair<string, User>? matchedUser = userSearchResult.Any() ? userSearchResult.First() : null;
                if (matchedUser == null)
                {
                    reply = @"User not found\.";
                    Console.WriteLine(" Response: user not found.");
                }
                else if (botConfig.ChatAssociations.TryGetValue(message.From.Id, out var userUuid))
                {
                    if (string.Equals(userUuid, argument, StringComparison.OrdinalIgnoreCase))
                    {
                        reply = $@"You are already linked to `{matchedUser.Value.Key}`\.";
                        Console.WriteLine(" Response: already linked.");
                    }
                    else
                    {
                        reply = $@"You are already linked to another user with UUID `{userUuid}`\.";
                        Console.WriteLine(" Response: already linked to another user.");
                    }
                }
                else
                {
                    botConfig.ChatAssociations.Add(message.From.Id, matchedUser.Value.Value.Uuid);
                    reply = $@"Successfully linked your Telegram account to `{matchedUser.Value.Key}`\.";
                    Console.WriteLine(" Response: success.");
                    await BotConfig.SaveBotConfigAsync(botConfig);
                }
            }
            await ReplyToMessage(botClient, message, reply, ParseMode.MarkdownV2, cancellationToken: cancellationToken);
        }

        public static async Task HandleUnlinkCommand(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken = default)
        {
            Console.Write($"{message.From} executed {message.Text} in {message.Chat.Type.ToString().ToLower()} chat {(string.IsNullOrEmpty(message.Chat.Title) ? string.Empty : $"{message.Chat.Title} ")}({message.Chat.Id}).");
            string reply;
            var botConfig = await BotConfig.LoadBotConfigAsync();
            if (!botConfig.AllowChatAssociation)
            {
                reply = @"The admin has disabled Telegram association\.";
                Console.WriteLine(" Response: command disabled.");
            }
            else if (message.Chat.Type != ChatType.Private)
            {
                reply = @"Associations must be made in a private chat\.";
                Console.WriteLine(" Response: not in private chat.");
            }
            else if (botConfig.ChatAssociations.Remove(message.From.Id, out var userUuid))
            {
                reply = $@"Successfully unlinked your Telegram account from `{userUuid}`\.";
                Console.WriteLine(" Response: success.");
                await BotConfig.SaveBotConfigAsync(botConfig);
            }
            else
            {
                reply = @"You are not linked to any user\.";
                Console.WriteLine(" Response: not linked");
            }
            await ReplyToMessage(botClient, message, reply, ParseMode.MarkdownV2, cancellationToken: cancellationToken);
        }

        public static async Task HandleListUsersCommand(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken = default)
        {
            Console.Write($"{message.From} executed {message.Text} in {message.Chat.Type.ToString().ToLower()} chat {(string.IsNullOrEmpty(message.Chat.Title) ? string.Empty : $"{message.Chat.Title} ")}({message.Chat.Id}).");
            string reply;
            var botConfig = await BotConfig.LoadBotConfigAsync();
            var users = await Users.LoadUsersAsync();
            if (!botConfig.UsersCanSeeAllUsers)
            {
                reply = @"The admin has disabled the command\.";
                Console.WriteLine(" Response: permission denied.");
            }
            else if (botConfig.ChatAssociations.TryGetValue(message.From.Id, out var userUuid) && TryLocateUserFromUuid(userUuid, users, out _))
            {
                var replyBuilder = new StringBuilder();

                var maxNameLength = users.UserDict.Select(x => x.Key.Length)
                                                  .DefaultIfEmpty()
                                                  .Max();
                var nameFieldWidth = maxNameLength > 4 ? maxNameLength + 2 : 6;

                replyBuilder.AppendLine("```");
                AppendTableBorder(ref replyBuilder, nameFieldWidth, 18);
                replyBuilder.AppendLine($"|{"User".PadRight(nameFieldWidth)}|{"Associated Groups",18}|");
                AppendTableBorder(ref replyBuilder, nameFieldWidth, 18);

                foreach (var user in users.UserDict)
                    replyBuilder.AppendLine($"|{user.Key.PadRight(nameFieldWidth)}|{user.Value.Credentials.Count,18}|");

                AppendTableBorder(ref replyBuilder, nameFieldWidth, 18);
                replyBuilder.AppendLine("```");

                reply = replyBuilder.ToString();
                Console.WriteLine(" Response: successful query.");
            }
            else
            {
                reply = @"You must link your Telegram account to your user first\.";
                Console.WriteLine(" Response: user not linked.");
            }
            await ReplyToMessage(botClient, message, reply, ParseMode.MarkdownV2, cancellationToken: cancellationToken);
        }

        public static async Task HandleListNodesCommand(ITelegramBotClient botClient, Message message, string? argument, CancellationToken cancellationToken = default)
        {
            Console.Write($"{message.From} executed {message.Text} in {message.Chat.Type.ToString().ToLower()} chat {(string.IsNullOrEmpty(message.Chat.Title) ? string.Empty : $"{message.Chat.Title} ")}({message.Chat.Id}).");
            string reply;
            var botConfig = await BotConfig.LoadBotConfigAsync();
            var users = await Users.LoadUsersAsync();
            if (botConfig.ChatAssociations.TryGetValue(message.From.Id, out var userUuid) && TryLocateUserFromUuid(userUuid, users, out var userEntry))
            {
                var nodes = await Nodes.LoadNodesAsync();
                var userGroups = userEntry.Value.Value.Credentials.Keys.ToList();
                var replyBuilder = new StringBuilder();

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

                replyBuilder.AppendLine("```");
                AppendTableBorder(ref replyBuilder, 7, nodeNameFieldWidth, groupNameFieldWidth, 36, hostnameFieldWidth, 5, pluginFieldWidth, pluginOptsFieldWidth);
                replyBuilder.AppendLine($"|{"Status",7}|{"Node".PadRight(nodeNameFieldWidth)}|{"Group".PadRight(groupNameFieldWidth)}|{"UUID",36}|{"Host".PadLeft(hostnameFieldWidth)}|{"Port",5}|{"Plugin".PadLeft(pluginFieldWidth)}|{"Plugin Options".PadLeft(pluginOptsFieldWidth)}|");
                AppendTableBorder(ref replyBuilder, 7, nodeNameFieldWidth, groupNameFieldWidth, 36, hostnameFieldWidth, 5, pluginFieldWidth, pluginOptsFieldWidth);

                foreach (var groupEntry in nodes.Groups)
                    if ((argument == null || argument == groupEntry.Key) && (botConfig.UsersCanSeeAllGroups || userGroups.Contains(groupEntry.Key)))
                        foreach (var node in groupEntry.Value.NodeDict)
                            replyBuilder.AppendLine($"|{(node.Value.Deactivated ? "🛑" : "✔"),7}|{node.Key.PadRight(nodeNameFieldWidth)}|{groupEntry.Key.PadRight(groupNameFieldWidth)}|{node.Value.Uuid,36}|{node.Value.Host.PadLeft(hostnameFieldWidth)}|{node.Value.Port,5}|{(node.Value.Plugin ?? string.Empty).PadLeft(pluginFieldWidth)}|{(node.Value.PluginOpts ?? string.Empty).PadLeft(pluginOptsFieldWidth)}|");

                AppendTableBorder(ref replyBuilder, 7, nodeNameFieldWidth, groupNameFieldWidth, 36, hostnameFieldWidth, 5, pluginFieldWidth, pluginOptsFieldWidth);
                replyBuilder.AppendLine("```");

                reply = replyBuilder.ToString();
                Console.WriteLine(" Response: successful query.");
            }
            else
            {
                reply = @"You must link your Telegram account to your user first\.";
                Console.WriteLine(" Response: user not linked.");
            }
            await ReplyToMessage(botClient, message, reply, ParseMode.MarkdownV2, cancellationToken: cancellationToken);
        }

        public static async Task HandleListGroupsCommand(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken = default)
        {
            Console.Write($"{message.From} executed {message.Text} in {message.Chat.Type.ToString().ToLower()} chat {(string.IsNullOrEmpty(message.Chat.Title) ? string.Empty : $"{message.Chat.Title} ")}({message.Chat.Id}).");
            string reply;
            var botConfig = await BotConfig.LoadBotConfigAsync();
            var users = await Users.LoadUsersAsync();
            if (botConfig.ChatAssociations.TryGetValue(message.From.Id, out var userUuid) && TryLocateUserFromUuid(userUuid, users, out var userEntry))
            {
                var nodes = await Nodes.LoadNodesAsync();
                var userGroups = userEntry.Value.Value.Credentials.Keys.ToList();
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
                AppendTableBorder(ref replyBuilder, groupNameFieldWidth, 16, outlineServerNameFieldWidth);
                replyBuilder.AppendLine($"|{"Group".PadRight(groupNameFieldWidth)}|{"Number of Nodes",16}|{"Outline Server".PadLeft(outlineServerNameFieldWidth)}|");
                AppendTableBorder(ref replyBuilder, groupNameFieldWidth, 16, outlineServerNameFieldWidth);

                foreach (var groupEntry in nodes.Groups)
                    if (botConfig.UsersCanSeeAllGroups || userGroups.Contains(groupEntry.Key))
                        replyBuilder.AppendLine($"|{groupEntry.Key.PadRight(groupNameFieldWidth)}|{groupEntry.Value.NodeDict.Count,16}|{(groupEntry.Value.OutlineServerInfo?.Name ?? "No").PadLeft(outlineServerNameFieldWidth)}|");

                AppendTableBorder(ref replyBuilder, groupNameFieldWidth, 16, outlineServerNameFieldWidth);
                replyBuilder.AppendLine("```");
                reply = replyBuilder.ToString();
                Console.WriteLine(" Response: successful query.");
            }
            else
            {
                reply = @"You must link your Telegram account to your user first\.";
                Console.WriteLine(" Response: user not linked.");
            }
            await ReplyToMessage(botClient, message, reply, ParseMode.MarkdownV2, cancellationToken: cancellationToken);
        }

        public static async Task HandleListGroupMembersCommand(ITelegramBotClient botClient, Message message, string? argument, CancellationToken cancellationToken = default)
        {
            Console.Write($"{message.From} executed {message.Text} in {message.Chat.Type.ToString().ToLower()} chat {(string.IsNullOrEmpty(message.Chat.Title) ? string.Empty : $"{message.Chat.Title} ")}({message.Chat.Id}).");
            string reply;
            var botConfig = await BotConfig.LoadBotConfigAsync();
            var users = await Users.LoadUsersAsync();
            if (!botConfig.UsersCanSeeAllUsers)
            {
                reply = @"The admin has disabled the command\.";
                Console.WriteLine(" Response: permission denied.");
            }
            else if (string.IsNullOrEmpty(argument))
            {
                reply = @"Please specify a group\.";
                Console.WriteLine(" Response: missing argument.");
            }
            else if (botConfig.ChatAssociations.TryGetValue(message.From.Id, out var userUuid) && TryLocateUserFromUuid(userUuid, users, out _))
            {
                var memberCount = 0;
                var memberListBuilder = new StringBuilder();

                foreach (var user in users.UserDict)
                    if (user.Value.Credentials.ContainsKey(argument))
                    {
                        memberListBuilder.AppendLine($@"- {user.Key}");
                        memberCount++;
                    }

                var replyBuilder = new StringBuilder();
                replyBuilder.AppendLine("```");
                replyBuilder.AppendLine($"{"Group",-16}{argument,-32}");
                replyBuilder.AppendLine($"{"Members",-16}{memberCount,-32}");
                replyBuilder.Append(memberListBuilder);
                replyBuilder.AppendLine("```");

                reply = replyBuilder.ToString();
                Console.WriteLine(" Response: successful query.");
            }
            else
            {
                reply = @"You must link your Telegram account to your user first\.";
                Console.WriteLine(" Response: user not linked.");
            }
            await ReplyToMessage(botClient, message, reply, ParseMode.MarkdownV2, cancellationToken: cancellationToken);
        }

        public static async Task HandleGetUserDataUsageCommand(ITelegramBotClient botClient, Message message, string? argument, CancellationToken cancellationToken = default)
        {
            Console.Write($"{message.From} executed {message.Text} in {message.Chat.Type.ToString().ToLower()} chat {(string.IsNullOrEmpty(message.Chat.Title) ? string.Empty : $"{message.Chat.Title} ")}({message.Chat.Id}).");
            string reply = @"An error occurred\."; // the initial value is only used when there's an error in the logic
            var botConfig = await BotConfig.LoadBotConfigAsync();
            var users = await Users.LoadUsersAsync();
            if (botConfig.ChatAssociations.TryGetValue(message.From.Id, out var userUuid) && TryLocateUserFromUuid(userUuid, users, out var userEntry))
            {
                if (argument != null && userEntry.Value.Key != argument) // look up specified user
                    if (!botConfig.UsersCanSeeAllUsers)
                    {
                        reply = @"The admin won't let you view other user's data usage\.";
                        Console.WriteLine(" Response: permission denied.");
                    }
                    else if (users.UserDict.TryGetValue(argument, out var user))
                        userEntry = new(argument, user);
                    else
                    {
                        reply = @"The specified user doesn't exist\.";
                        Console.WriteLine(" Response: target user not found.");
                    }
                if (argument == null || userEntry.Value.Key == argument) // no argument or successful lookup
                {
                    var nodes = await Nodes.LoadNodesAsync();
                    var username = userEntry.Value.Key;
                    var user = userEntry.Value.Value;
                    var records = userEntry.Value.Value.GetDataUsage(username, nodes);
                    // sort records
                    records = records.OrderByDescending(x => x.bytesUsed).ToList();
                    var maxNameLength = records.Select(x => x.group.Length)
                                               .DefaultIfEmpty()
                                               .Max();
                    var nameFieldWidth = maxNameLength > 5 ? maxNameLength + 2 : 7;

                    var replyBuilder = new StringBuilder();

                    replyBuilder.AppendLine("```");
                    replyBuilder.AppendLine($"{"User",-16}{username,-32}");
                    replyBuilder.AppendLine($"{"Data used",-16}{Utilities.HumanReadableDataString(user.BytesUsed),-32}");
                    if (user.BytesRemaining != 0UL)
                        replyBuilder.AppendLine($"{"Data remaining",-16}{Utilities.HumanReadableDataString(user.BytesRemaining),-32}");
                    if (user.DataLimitInBytes != 0UL)
                        replyBuilder.AppendLine($"{"Data limit",-16}{Utilities.HumanReadableDataString(user.DataLimitInBytes),-32}");

                    replyBuilder.AppendLine();

                    AppendTableBorder(ref replyBuilder, nameFieldWidth, 11, 16);
                    replyBuilder.AppendLine($"|{"Group".PadRight(nameFieldWidth)}|{"Data Used",11}|{"Data Remaining",16}|");
                    AppendTableBorder(ref replyBuilder, nameFieldWidth, 11, 16);

                    foreach (var (group, bytesUsed, bytesRemaining) in records)
                    {
                        replyBuilder.Append($"|{group.PadRight(nameFieldWidth)}|{Utilities.HumanReadableDataString(bytesUsed),11}|");
                        if (bytesRemaining != 0UL)
                            replyBuilder.AppendLine($"{Utilities.HumanReadableDataString(bytesRemaining),16}|");
                        else
                            replyBuilder.AppendLine($"{string.Empty,16}|");
                    }

                    AppendTableBorder(ref replyBuilder, nameFieldWidth, 11, 16);
                    replyBuilder.AppendLine("```");

                    reply = replyBuilder.ToString();
                    Console.WriteLine(" Response: successful query.");
                }
            }
            else
            {
                reply = @"You must link your Telegram account to your user first\.";
                Console.WriteLine(" Response: user not linked.");
            }
            await ReplyToMessage(botClient, message, reply, ParseMode.MarkdownV2, cancellationToken: cancellationToken);
        }

        public static async Task HandleGetGroupDataUsageCommand(ITelegramBotClient botClient, Message message, string? argument, CancellationToken cancellationToken = default)
        {
            Console.Write($"{message.From} executed {message.Text} in {message.Chat.Type.ToString().ToLower()} chat {(string.IsNullOrEmpty(message.Chat.Title) ? string.Empty : $"{message.Chat.Title} ")}({message.Chat.Id}).");
            string reply;
            var botConfig = await BotConfig.LoadBotConfigAsync();
            var users = await Users.LoadUsersAsync();
            if (!botConfig.UsersCanSeeGroupDataUsage)
            {
                reply = @"The admin has disabled the command\.";
                Console.WriteLine(" Response: permission denied.");
            }
            else if (string.IsNullOrEmpty(argument))
            {
                reply = @"Please specify a group\.";
                Console.WriteLine(" Response: missing argument.");
            }
            else if (botConfig.ChatAssociations.TryGetValue(message.From.Id, out var userUuid) && TryLocateUserFromUuid(userUuid, users, out var userEntry))
            {
                var userGroups = userEntry.Value.Value.Credentials.Keys.ToList();
                if (userGroups.Contains(argument) || botConfig.UsersCanSeeAllGroups) // user is allowed to view it
                {
                    var nodes = await Nodes.LoadNodesAsync();
                    var records = nodes.GetGroupDataUsage(argument);
                    if (records.Any())
                    {
                        // sort records
                        records = records.OrderByDescending(x => x.bytesUsed).ToList();
                        var maxNameLength = records.Select(x => x.username.Length)
                                                   .DefaultIfEmpty()
                                                   .Max();
                        var nameFieldWidth = maxNameLength > 4 ? maxNameLength + 2 : 6;

                        var replyBuilder = new StringBuilder();

                        replyBuilder.AppendLine("```");
                        replyBuilder.AppendLine($"{"Group",-16}{argument,-32}");
                        if (nodes.Groups.TryGetValue(argument, out var targetGroup))
                        {
                            replyBuilder.AppendLine($"{"Data used",-16}{Utilities.HumanReadableDataString(targetGroup.BytesUsed),-32}");
                            if (targetGroup.BytesRemaining != 0UL)
                                replyBuilder.AppendLine($"{"Data remaining",-16}{Utilities.HumanReadableDataString(targetGroup.BytesRemaining),-32}");
                            if (targetGroup.DataLimitInBytes != 0UL)
                                replyBuilder.AppendLine($"{"Data limit",-16}{Utilities.HumanReadableDataString(targetGroup.DataLimitInBytes),-32}");
                        }

                        replyBuilder.AppendLine();

                        AppendTableBorder(ref replyBuilder, nameFieldWidth, 11, 16);
                        replyBuilder.AppendLine($"|{"User".PadRight(nameFieldWidth)}|{"Data Used",11}|{"Data Remaining",16}|");
                        AppendTableBorder(ref replyBuilder, nameFieldWidth, 11, 16);

                        foreach (var (username, bytesUsed, bytesRemaining) in records)
                        {
                            replyBuilder.Append($"|{username.PadRight(nameFieldWidth)}|{Utilities.HumanReadableDataString(bytesUsed),11}|");
                            if (bytesRemaining != 0UL)
                                replyBuilder.AppendLine($"{Utilities.HumanReadableDataString(bytesRemaining),16}|");
                            else
                                replyBuilder.AppendLine($"{string.Empty,16}|");
                        }

                        AppendTableBorder(ref replyBuilder, nameFieldWidth, 11, 16);
                        replyBuilder.AppendLine("```");

                        reply = replyBuilder.ToString();
                        Console.WriteLine(" Response: successful query.");
                    }
                    else
                    {
                        reply = @"No data usage metrics available\.";
                        Console.WriteLine(" Response: successful query.");
                    }
                }
                else
                {
                    reply = @"You are not authorized to access the information\.";
                    Console.WriteLine(" Response: permission denied.");
                }
            }
            else
            {
                reply = @"You must link your Telegram account to your user first\.";
                Console.WriteLine(" Response: user not linked.");
            }
            await ReplyToMessage(botClient, message, reply, ParseMode.MarkdownV2, cancellationToken: cancellationToken);
        }

        public static async Task HandleGetSsLinksCommand(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken = default)
        {
            Console.Write($"{message.From} executed {message.Text} in {message.Chat.Type.ToString().ToLower()} chat {(string.IsNullOrEmpty(message.Chat.Title) ? string.Empty : $"{message.Chat.Title} ")}({message.Chat.Id}).");
            string reply;
            var botConfig = await BotConfig.LoadBotConfigAsync();
            var users = await Users.LoadUsersAsync();
            if (message.Chat.Type != ChatType.Private)
            {
                reply = @"Retrieval of sensitive information is only allowed in private chats\.";
                Console.WriteLine(" Response: not in private chat.");
            }
            else if (botConfig.ChatAssociations.TryGetValue(message.From.Id, out var userUuid) && TryLocateUserFromUuid(userUuid, users, out var userEntry))
            {
                var username = userEntry.Value.Key;
                var nodes = await Nodes.LoadNodesAsync();
                var uris = users.GetUserSSUris(username, nodes);
                if (uris.Count > 0)
                {
                    var replyBuilder = new StringBuilder();

                    replyBuilder.AppendLine("```");
                    foreach (var uri in uris)
                        replyBuilder.AppendLine($"{uri.AbsoluteUri}");
                    replyBuilder.AppendLine("```");

                    reply = replyBuilder.ToString();
                    Console.WriteLine(" Response: successful query.");
                }
                else
                {
                    reply = @"No links are available for you\.";
                    Console.WriteLine(" Response: successful query.");
                }
            }
            else
            {
                reply = @"You must link your Telegram account to your user first\.";
                Console.WriteLine(" Response: user not linked.");
            }
            await ReplyToMessage(botClient, message, reply, ParseMode.MarkdownV2, cancellationToken: cancellationToken);
        }

        public static async Task HandleGetSip008LinksCommand(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken = default)
        {
            Console.Write($"{message.From} executed {message.Text} in {message.Chat.Type.ToString().ToLower()} chat {(string.IsNullOrEmpty(message.Chat.Title) ? string.Empty : $"{message.Chat.Title} ")}({message.Chat.Id}).");
            string reply;
            var botConfig = await BotConfig.LoadBotConfigAsync();
            var users = await Users.LoadUsersAsync();
            if (message.Chat.Type != ChatType.Private)
                reply = @"Retrieval of sensitive information is only allowed in private chats\.";
            else if (botConfig.ChatAssociations.TryGetValue(message.From.Id, out var userUuid) && TryLocateUserFromUuid(userUuid, users, out var userEntry))
            {
                var settings = await Settings.LoadSettingsAsync();
                var replyBuilder = new StringBuilder();

                replyBuilder.AppendLine("SIP008 delivery link for all servers:");
                replyBuilder.AppendLine("```");
                replyBuilder.AppendLine($"{settings.OnlineConfigDeliveryRootUri}/{userEntry.Value.Value.Uuid}.json");
                replyBuilder.AppendLine("```");

                if (settings.OnlineConfigDeliverByGroup)
                {
                    replyBuilder.AppendLine();
                    replyBuilder.AppendLine("SIP008 delivery link by server group:");
                    replyBuilder.AppendLine("```");
                    foreach (var group in userEntry.Value.Value.Credentials.Keys)
                        replyBuilder.AppendLine($"{settings.OnlineConfigDeliveryRootUri}/{userEntry.Value.Value.Uuid}/{Uri.EscapeDataString(group)}.json");
                    replyBuilder.AppendLine("```");
                }
                reply = replyBuilder.ToString();
                Console.WriteLine(" Response: successful query.");
            }
            else
            {
                reply = @"You must link your Telegram account to your user first\.";
                Console.WriteLine(" Response: user not linked.");
            }
            await ReplyToMessage(botClient, message, reply, ParseMode.MarkdownV2, true, cancellationToken);
        }

        public static async Task HandleGetCredentialsCommand(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken = default)
        {
            Console.Write($"{message.From} executed {message.Text} in {message.Chat.Type.ToString().ToLower()} chat {(string.IsNullOrEmpty(message.Chat.Title) ? string.Empty : $"{message.Chat.Title} ")}({message.Chat.Id}).");
            string reply;
            var botConfig = await BotConfig.LoadBotConfigAsync();
            var users = await Users.LoadUsersAsync();
            if (message.Chat.Type != ChatType.Private)
                reply = @"Retrieval of sensitive information is only allowed in private chats\.";
            else if (botConfig.ChatAssociations.TryGetValue(message.From.Id, out var userUuid) && TryLocateUserFromUuid(userUuid, users, out var userEntry))
            {
                var username = userEntry.Value.Key;
                if (userEntry.Value.Value.Credentials.Count > 0)
                {
                    var replyBuilder = new StringBuilder();
                    var maxGroupNameLength = users.UserDict.SelectMany(x => x.Value.Credentials.Keys)
                                                           .Select(x => x.Length)
                                                           .DefaultIfEmpty()
                                                           .Max();
                    var maxPasswordLength = users.UserDict.SelectMany(x => x.Value.Credentials.Values)
                                                          .Select(x => x?.Password.Length ?? 0)
                                                          .DefaultIfEmpty()
                                                          .Max();
                    var groupNameFieldWidth = maxGroupNameLength > 5 ? maxGroupNameLength + 2 : 7;
                    var passwordFieldWidth = maxPasswordLength > 8 ? maxPasswordLength + 2 : 10;

                    replyBuilder.AppendLine("```");
                    replyBuilder.AppendLine($"{"User",-16}{username,-32}");
                    replyBuilder.AppendLine();

                    AppendTableBorder(ref replyBuilder, groupNameFieldWidth, 24, passwordFieldWidth);
                    replyBuilder.AppendLine($"|{"Group".PadRight(groupNameFieldWidth)}|{"Method",-24}|{"Password".PadRight(passwordFieldWidth)}|");
                    AppendTableBorder(ref replyBuilder, groupNameFieldWidth, 24, passwordFieldWidth);

                    foreach (var credEntry in userEntry.Value.Value.Credentials)
                    {
                        if (credEntry.Value == null)
                            replyBuilder.AppendLine($"|{credEntry.Key.PadRight(groupNameFieldWidth)}|{string.Empty,-24}|{string.Empty.PadRight(passwordFieldWidth)}|");
                        else
                            replyBuilder.AppendLine($"|{credEntry.Key.PadRight(groupNameFieldWidth)}|{credEntry.Value.Method,-24}|{credEntry.Value.Password.PadRight(passwordFieldWidth)}|");
                    }

                    AppendTableBorder(ref replyBuilder, groupNameFieldWidth, 24, passwordFieldWidth);
                    replyBuilder.AppendLine("```");

                    reply = replyBuilder.ToString();
                    Console.WriteLine(" Response: successful query.");
                }
                else
                {
                    reply = @"You have no credentials\.";
                    Console.WriteLine(" Response: successful query.");
                }
            }
            else
            {
                reply = @"You must link your Telegram account to your user first\.";
                Console.WriteLine(" Response: user not linked.");
            }
            await ReplyToMessage(botClient, message, reply, ParseMode.MarkdownV2, cancellationToken: cancellationToken);
        }

        public static async Task HandleReportCommand(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken = default)
        {
            Console.Write($"{message.From} executed {message.Text} in {message.Chat.Type.ToString().ToLower()} chat {(string.IsNullOrEmpty(message.Chat.Title) ? string.Empty : $"{message.Chat.Title} ")}({message.Chat.Id}).");
            string reply;
            var botConfig = await BotConfig.LoadBotConfigAsync();
            var users = await Users.LoadUsersAsync();
            if (!botConfig.UsersCanSeeAllUsers || !botConfig.UsersCanSeeAllGroups || !botConfig.UsersCanSeeGroupDataUsage)
            {
                reply = @"You are not authorized to view the report\.";
                Console.WriteLine(" Response: permission denied.");
            }
            else if (botConfig.ChatAssociations.TryGetValue(message.From.Id, out var userUuid) && TryLocateUserFromUuid(userUuid, users, out _))
            {
                var nodes = await Nodes.LoadNodesAsync();

                // collect data
                var totalBytesUsed = nodes.Groups.Select(x => x.Value.BytesUsed).Aggregate(0UL, (x, y) => x + y);
                var totalBytesRemaining = nodes.Groups.Select(x => x.Value.BytesRemaining).Aggregate(0UL, (x, y) => x + y);
                var recordsByGroup = nodes.GetDataUsageByGroup();
                var recordsByUser = users.GetDataUsageByUser(nodes);
                // sort records
                recordsByGroup = recordsByGroup.OrderByDescending(x => x.bytesUsed).ToList();
                recordsByUser = recordsByUser.OrderByDescending(x => x.bytesUsed).ToList();
                // calculate column width
                var maxGroupNameLength = recordsByGroup.Select(x => x.group.Length)
                                                       .DefaultIfEmpty()
                                                       .Max();
                var groupNameFieldWidth = maxGroupNameLength > 5 ? maxGroupNameLength + 2 : 7;
                var maxUsernameLength = recordsByUser.Select(x => x.username.Length)
                                                     .DefaultIfEmpty()
                                                     .Max();
                var usernameFieldWidth = maxUsernameLength > 4 ? maxUsernameLength + 2 : 6;

                // total
                var replyBuilder = new StringBuilder();
                replyBuilder.AppendLine("In the last 30 days");
                replyBuilder.AppendLine();
                if (totalBytesUsed != 0UL)
                    replyBuilder.AppendLine($"{"Total data used",-24}`{Utilities.HumanReadableDataString(totalBytesUsed)}`");
                if (totalBytesRemaining != 0UL)
                    replyBuilder.AppendLine($"{"Total data remaining",-24}`{Utilities.HumanReadableDataString(totalBytesRemaining)}`");
                replyBuilder.AppendLine();

                // by group
                replyBuilder.AppendLine("Data usage by group");
                replyBuilder.AppendLine("```");
                AppendTableBorder(ref replyBuilder, groupNameFieldWidth, 11, 16);
                replyBuilder.AppendLine($"|{"Group".PadRight(groupNameFieldWidth)}|{"Data Used",11}|{"Data Remaining",16}|");
                AppendTableBorder(ref replyBuilder, groupNameFieldWidth, 11, 16);
                foreach (var (group, bytesUsed, bytesRemaining) in recordsByGroup)
                {
                    replyBuilder.Append($"|{group.PadRight(groupNameFieldWidth)}|");
                    if (bytesUsed != 0UL)
                        replyBuilder.Append($"{Utilities.HumanReadableDataString(bytesUsed),11}|");
                    else
                        replyBuilder.Append($"{string.Empty,11}|");
                    if (bytesRemaining != 0UL)
                        replyBuilder.AppendLine($"{Utilities.HumanReadableDataString(bytesRemaining),16}|");
                    else
                        replyBuilder.AppendLine($"{string.Empty,16}|");
                }
                AppendTableBorder(ref replyBuilder, groupNameFieldWidth, 11, 16);
                replyBuilder.AppendLine("```");
                replyBuilder.AppendLine();

                // by user
                replyBuilder.AppendLine("Data usage by user");
                replyBuilder.AppendLine("```");
                AppendTableBorder(ref replyBuilder, usernameFieldWidth, 11, 16);
                replyBuilder.AppendLine($"|{"User".PadRight(usernameFieldWidth)}|{"Data Used",11}|{"Data Remaining",16}|");
                AppendTableBorder(ref replyBuilder, usernameFieldWidth, 11, 16);
                foreach (var (username, bytesUsed, bytesRemaining) in recordsByUser)
                {
                    replyBuilder.Append($"|{username.PadRight(usernameFieldWidth)}|");
                    if (bytesUsed != 0UL)
                        replyBuilder.Append($"{Utilities.HumanReadableDataString(bytesUsed),11}|");
                    else
                        replyBuilder.Append($"{string.Empty,11}|");
                    if (bytesRemaining != 0UL)
                        replyBuilder.AppendLine($"{Utilities.HumanReadableDataString(bytesRemaining),16}|");
                    else
                        replyBuilder.AppendLine($"{string.Empty,16}|");
                }
                AppendTableBorder(ref replyBuilder, usernameFieldWidth, 11, 16);
                replyBuilder.AppendLine("```");

                reply = replyBuilder.ToString();
                Console.WriteLine(" Response: successful query.");
            }
            else
            {
                reply = @"You must link your Telegram account to your user first\.";
                Console.WriteLine(" Response: user not linked.");
            }
            await ReplyToMessage(botClient, message, reply, ParseMode.MarkdownV2, cancellationToken: cancellationToken);
        }

        private static Task ReplyToMessage(
            ITelegramBotClient botClient,
            Message message, string reply,
            ParseMode parseMode = ParseMode.MarkdownV2,
            bool disableWebPagePreview = false,
            CancellationToken cancellationToken = default)
        {
            if (reply.Length <= 4096)
                return botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: reply,
                    parseMode: ParseMode.MarkdownV2,
                    disableWebPagePreview: disableWebPagePreview,
                    replyToMessageId: message.MessageId,
                    cancellationToken: cancellationToken);
            else // too large, send as file
            {
                var filename = parseMode switch
                {
                    ParseMode.Default => "reply.md",
                    ParseMode.Markdown => "reply.md",
                    ParseMode.Html => "reply.html",
                    ParseMode.MarkdownV2 => "reply.md",
                    _ => "reply",
                };
                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(reply));
                return botClient.SendDocumentAsync(
                    chatId: message.Chat.Id,
                    new(stream, filename),
                    replyToMessageId: message.MessageId,
                    cancellationToken: cancellationToken);
            }
        }

        private static bool TryLocateUserFromUuid(string userUuid, Users users, [NotNullWhen(true)] out KeyValuePair<string, User>? userEntry)
        {
            var userSearchResult = users.UserDict.Where(x => string.Equals(x.Value.Uuid, userUuid, StringComparison.OrdinalIgnoreCase));
            if (userSearchResult.Any())
            {
                userEntry = userSearchResult.First();
                return true;
            }
            else
            {
                userEntry = null;
                return false;
            }
        }

        public static void AppendTableBorder(ref StringBuilder message, params int[] columnWidths)
        {
            foreach (var columnWidth in columnWidths)
            {
                message.Append('+');
                for (var i = 0; i < columnWidth; i++)
                    message.Append('-');
            }
            message.Append("+\n");
        }
    }
}
