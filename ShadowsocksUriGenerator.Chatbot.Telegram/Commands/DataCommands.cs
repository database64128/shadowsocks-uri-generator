using ShadowsocksUriGenerator.Chatbot.Telegram.Utils;
using ShadowsocksUriGenerator.CLI.Utils;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ShadowsocksUriGenerator.Chatbot.Telegram.Commands
{
    public static class DataCommands
    {
        public static async Task GetUserDataUsageAsync(ITelegramBotClient botClient, Message message, string? argument, CancellationToken cancellationToken = default)
        {
            Console.Write($"{message.From} executed {message.Text} in {message.Chat.Type.ToString().ToLower()} chat {(string.IsNullOrEmpty(message.Chat.Title) ? string.Empty : $"{message.Chat.Title} ")}({message.Chat.Id}).");

            string replyMarkdownV2 = @"An error occurred\."; // the initial value is only used when there's an error in the logic

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
                if (argument is not null && userEntry.Value.Key != argument) // look up specified user
                {
                    if (!botConfig.UsersCanSeeAllUsers)
                    {
                        replyMarkdownV2 = @"The admin won't let you view other user's data usage\.";
                        Console.WriteLine(" Response: permission denied.");
                    }
                    else if (users.UserDict.TryGetValue(argument, out var user))
                    {
                        userEntry = new(argument, user);
                    }
                    else
                    {
                        replyMarkdownV2 = @"The specified user doesn't exist\.";
                        Console.WriteLine(" Response: target user not found.");
                    }
                }

                if (argument is null || userEntry.Value.Key == argument) // no argument or successful lookup
                {
                    var (loadedNodes, loadNodesErrMsg) = await Nodes.LoadNodesAsync(cancellationToken);
                    if (loadNodesErrMsg is not null)
                    {
                        Console.WriteLine();
                        Console.WriteLine(loadNodesErrMsg);
                        return;
                    }
                    using var nodes = loadedNodes;

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
                    replyBuilder.AppendLine($"{"User",-16}{ChatHelper.EscapeMarkdownV2CodeBlock(username),-32}");
                    replyBuilder.AppendLine($"{"Data used",-16}{Utilities.HumanReadableDataString(user.BytesUsed),-32}");
                    if (user.BytesRemaining != 0UL)
                        replyBuilder.AppendLine($"{"Data remaining",-16}{Utilities.HumanReadableDataString(user.BytesRemaining),-32}");
                    if (user.DataLimitInBytes != 0UL)
                        replyBuilder.AppendLine($"{"Data limit",-16}{Utilities.HumanReadableDataString(user.DataLimitInBytes),-32}");

                    replyBuilder.AppendLine();

                    replyBuilder.AppendTableBorder(nameFieldWidth, 11, 16);

                    replyBuilder.AppendLine($"|{"Group".PadRight(nameFieldWidth)}|{"Data Used",11}|{"Data Remaining",16}|");

                    replyBuilder.AppendTableBorder(nameFieldWidth, 11, 16);

                    foreach (var (group, bytesUsed, bytesRemaining) in records)
                    {
                        replyBuilder.Append($"|{ChatHelper.EscapeMarkdownV2CodeBlock(group).PadRight(nameFieldWidth)}|{Utilities.HumanReadableDataString(bytesUsed),11}|");
                        if (bytesRemaining != 0UL)
                            replyBuilder.AppendLine($"{Utilities.HumanReadableDataString(bytesRemaining),16}|");
                        else
                            replyBuilder.AppendLine($"{string.Empty,16}|");
                    }

                    replyBuilder.AppendTableBorder(nameFieldWidth, 11, 16);
                    replyBuilder.AppendLine("```");

                    replyMarkdownV2 = replyBuilder.ToString();
                    Console.WriteLine(" Response: successful query.");
                }
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

        public static async Task GetUserDataLimitAsync(ITelegramBotClient botClient, Message message, string? argument, CancellationToken cancellationToken = default)
        {
            Console.Write($"{message.From} executed {message.Text} in {message.Chat.Type.ToString().ToLower()} chat {(string.IsNullOrEmpty(message.Chat.Title) ? string.Empty : $"{message.Chat.Title} ")}({message.Chat.Id}).");

            string replyMarkdownV2 = @"An error occurred\."; // the initial value is only used when there's an error in the logic

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
                if (argument is not null && userEntry.Value.Key != argument) // look up specified user
                {
                    if (!botConfig.UsersCanSeeAllUsers)
                    {
                        replyMarkdownV2 = @"The admin won't let you view other user's data limit\.";
                        Console.WriteLine(" Response: permission denied.");
                    }
                    else if (users.UserDict.TryGetValue(argument, out var user))
                    {
                        userEntry = new(argument, user);
                    }
                    else
                    {
                        replyMarkdownV2 = @"The specified user doesn't exist\.";
                        Console.WriteLine(" Response: target user not found.");
                    }
                }

                if (argument is null || userEntry.Value.Key == argument) // no argument or successful lookup
                {
                    var (loadedNodes, loadNodesErrMsg) = await Nodes.LoadNodesAsync(cancellationToken);
                    if (loadNodesErrMsg is not null)
                    {
                        Console.WriteLine();
                        Console.WriteLine(loadNodesErrMsg);
                        return;
                    }
                    using var nodes = loadedNodes;

                    var username = userEntry.Value.Key;
                    var user = userEntry.Value.Value;

                    var replyBuilder = new StringBuilder();

                    replyBuilder.AppendLine("```");
                    replyBuilder.AppendLine($"{"User",-24}{ChatHelper.EscapeMarkdownV2CodeBlock(username),-32}");
                    if (user.DataLimitInBytes != 0UL)
                        replyBuilder.AppendLine($"{"Global data limit",-24}{Utilities.HumanReadableDataString(user.DataLimitInBytes),-32}");
                    if (user.PerGroupDataLimitInBytes != 0UL)
                        replyBuilder.AppendLine($"{"Per-group data limit",-24}{Utilities.HumanReadableDataString(user.PerGroupDataLimitInBytes),-32}");

                    var customLimits = user.Memberships.Where(x => x.Value.DataLimitInBytes > 0UL).Select(x => (x.Key, x.Value.DataLimitInBytes));

                    if (customLimits.Any())
                    {
                        var maxNameLength = customLimits.Select(x => x.Key.Length)
                                                        .DefaultIfEmpty()
                                                        .Max();
                        var nameFieldWidth = maxNameLength > 5 ? maxNameLength + 2 : 7;

                        replyBuilder.AppendLine();

                        replyBuilder.AppendTableBorder(nameFieldWidth, 19);

                        replyBuilder.AppendLine($"|{"Group".PadRight(nameFieldWidth)}|{"Custom Data Limit",19}|");

                        replyBuilder.AppendTableBorder(nameFieldWidth, 19);

                        foreach ((var group, var dataLimitInBytes) in customLimits)
                        {
                            replyBuilder.AppendLine($"|{ChatHelper.EscapeMarkdownV2CodeBlock(group).PadRight(nameFieldWidth)}|{Utilities.HumanReadableDataString(dataLimitInBytes),19}|");
                        }

                        replyBuilder.AppendTableBorder(nameFieldWidth, 19);
                    }

                    replyBuilder.AppendLine("```");

                    replyMarkdownV2 = replyBuilder.ToString();
                    Console.WriteLine(" Response: successful query.");
                }
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

        public static async Task GetGroupDataUsageAsync(ITelegramBotClient botClient, Message message, string? argument, CancellationToken cancellationToken = default)
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

            if (!botConfig.UsersCanSeeGroupDataUsage)
            {
                replyMarkdownV2 = @"The admin has disabled the command\.";
                Console.WriteLine(" Response: permission denied.");
            }
            else if (string.IsNullOrEmpty(argument))
            {
                replyMarkdownV2 = @"Please specify a group\.";
                Console.WriteLine(" Response: missing argument.");
            }
            else if (botConfig.ChatAssociations.TryGetValue(message.From.Id, out var userUuid) && DataHelper.TryLocateUserFromUuid(userUuid, users, out var userEntry))
            {
                if (userEntry.Value.Value.Memberships.ContainsKey(argument) || botConfig.UsersCanSeeAllGroups) // user is allowed to view it
                {
                    var (loadedNodes, loadNodesErrMsg) = await Nodes.LoadNodesAsync(cancellationToken);
                    if (loadNodesErrMsg is not null)
                    {
                        Console.WriteLine();
                        Console.WriteLine(loadNodesErrMsg);
                        return;
                    }
                    using var nodes = loadedNodes;

                    var records = nodes.GetGroupDataUsage(argument);
                    if (records is null)
                    {
                        replyMarkdownV2 = @"The specified group doesn't exist\.";
                        Console.WriteLine(" Response: nonexistent group.");
                    }
                    else if (records.Any())
                    {
                        // sort records
                        records = records.OrderByDescending(x => x.bytesUsed).ToList();

                        var maxNameLength = records.Select(x => x.username.Length)
                                                   .DefaultIfEmpty()
                                                   .Max();
                        var nameFieldWidth = maxNameLength > 4 ? maxNameLength + 2 : 6;

                        var replyBuilder = new StringBuilder();

                        replyBuilder.AppendLine("```");
                        replyBuilder.AppendLine($"{"Group",-16}{ChatHelper.EscapeMarkdownV2CodeBlock(argument),-32}");
                        if (nodes.Groups.TryGetValue(argument, out var targetGroup))
                        {
                            replyBuilder.AppendLine($"{"Data used",-16}{Utilities.HumanReadableDataString(targetGroup.BytesUsed),-32}");
                            if (targetGroup.BytesRemaining != 0UL)
                                replyBuilder.AppendLine($"{"Data remaining",-16}{Utilities.HumanReadableDataString(targetGroup.BytesRemaining),-32}");
                            if (targetGroup.DataLimitInBytes != 0UL)
                                replyBuilder.AppendLine($"{"Data limit",-16}{Utilities.HumanReadableDataString(targetGroup.DataLimitInBytes),-32}");
                        }

                        replyBuilder.AppendLine();

                        replyBuilder.AppendTableBorder(nameFieldWidth, 11, 16);

                        replyBuilder.AppendLine($"|{"User".PadRight(nameFieldWidth)}|{"Data Used",11}|{"Data Remaining",16}|");

                        replyBuilder.AppendTableBorder(nameFieldWidth, 11, 16);

                        foreach (var (username, bytesUsed, bytesRemaining) in records)
                        {
                            replyBuilder.Append($"|{ChatHelper.EscapeMarkdownV2CodeBlock(username).PadRight(nameFieldWidth)}|{Utilities.HumanReadableDataString(bytesUsed),11}|");
                            if (bytesRemaining != 0UL)
                                replyBuilder.AppendLine($"{Utilities.HumanReadableDataString(bytesRemaining),16}|");
                            else
                                replyBuilder.AppendLine($"{string.Empty,16}|");
                        }

                        replyBuilder.AppendTableBorder(nameFieldWidth, 11, 16);
                        replyBuilder.AppendLine("```");

                        replyMarkdownV2 = replyBuilder.ToString();
                        Console.WriteLine(" Response: successful query.");
                    }
                    else
                    {
                        replyMarkdownV2 = @"No data usage metrics available\.";
                        Console.WriteLine(" Response: successful query.");
                    }
                }
                else
                {
                    replyMarkdownV2 = @"You are not authorized to access the information\.";
                    Console.WriteLine(" Response: permission denied.");
                }
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

        public static async Task GetGroupDataLimitAsync(ITelegramBotClient botClient, Message message, string? argument, CancellationToken cancellationToken = default)
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

            if (!botConfig.UsersCanSeeGroupDataLimit)
            {
                replyMarkdownV2 = @"The admin has disabled the command\.";
                Console.WriteLine(" Response: permission denied.");
            }
            else if (string.IsNullOrEmpty(argument))
            {
                replyMarkdownV2 = @"Please specify a group\.";
                Console.WriteLine(" Response: missing argument.");
            }
            else if (botConfig.ChatAssociations.TryGetValue(message.From.Id, out var userUuid) && DataHelper.TryLocateUserFromUuid(userUuid, users, out var userEntry))
            {
                if (userEntry.Value.Value.Memberships.ContainsKey(argument) || botConfig.UsersCanSeeAllGroups) // user is allowed to view it
                {
                    var (loadedNodes, loadNodesErrMsg) = await Nodes.LoadNodesAsync(cancellationToken);
                    if (loadNodesErrMsg is not null)
                    {
                        Console.WriteLine();
                        Console.WriteLine(loadNodesErrMsg);
                        return;
                    }
                    using var nodes = loadedNodes;

                    if (nodes.Groups.TryGetValue(argument, out var targetGroup))
                    {
                        var replyBuilder = new StringBuilder();

                        replyBuilder.AppendLine("```");
                        replyBuilder.AppendLine($"{"Group",-24}{ChatHelper.EscapeMarkdownV2CodeBlock(argument),-32}");
                        if (targetGroup.DataLimitInBytes != 0UL)
                            replyBuilder.AppendLine($"{"Global data limit",-24}{Utilities.HumanReadableDataString(targetGroup.DataLimitInBytes),-32}");
                        if (targetGroup.PerUserDataLimitInBytes != 0UL)
                            replyBuilder.AppendLine($"{"Per-user data limit",-24}{Utilities.HumanReadableDataString(targetGroup.PerUserDataLimitInBytes),-32}");

                        var outlineAccessKeyCustomLimits = targetGroup.OutlineAccessKeys?.Where(x => x.DataLimit is not null).Select(x => (x.Name, x.DataLimit!.Bytes));

                        if (outlineAccessKeyCustomLimits?.Any() ?? false)
                        {
                            var maxNameLength = outlineAccessKeyCustomLimits.Select(x => x.Name.Length)
                                                                            .DefaultIfEmpty()
                                                                            .Max();
                            var nameFieldWidth = maxNameLength > 4 ? maxNameLength + 2 : 6;

                            replyBuilder.AppendLine();

                            replyBuilder.AppendTableBorder(nameFieldWidth, 19);

                            replyBuilder.AppendLine($"|{"User".PadRight(nameFieldWidth)}|{"Custom Data Limit",19}|");

                            replyBuilder.AppendTableBorder(nameFieldWidth, 19);

                            foreach ((var username, var dataLimitInBytes) in outlineAccessKeyCustomLimits)
                            {
                                replyBuilder.AppendLine($"|{ChatHelper.EscapeMarkdownV2CodeBlock(username).PadRight(nameFieldWidth)}|{Utilities.HumanReadableDataString(dataLimitInBytes),19}|");
                            }

                            replyBuilder.AppendTableBorder(nameFieldWidth, 19);
                        }

                        replyBuilder.AppendLine("```");

                        replyMarkdownV2 = replyBuilder.ToString();
                        Console.WriteLine(" Response: successful query.");
                    }
                    else
                    {
                        replyMarkdownV2 = @"The specified group doesn't exist\.";
                        Console.WriteLine(" Response: nonexistent group.");
                    }
                }
                else
                {
                    replyMarkdownV2 = @"You are not authorized to access the information\.";
                    Console.WriteLine(" Response: permission denied.");
                }
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
