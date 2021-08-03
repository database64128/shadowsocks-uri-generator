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
    public static class ReportCommand
    {
        public static async Task GenerateReportAsync(ITelegramBotClient botClient, Message message, string? argument, CancellationToken cancellationToken = default)
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

            if (!botConfig.UsersCanSeeAllUsers || !botConfig.UsersCanSeeAllGroups || !botConfig.UsersCanSeeGroupDataUsage)
            {
                replyMarkdownV2 = @"You are not authorized to view the report\.";
                Console.WriteLine(" Response: permission denied.");
            }
            else if (botConfig.ChatAssociations.TryGetValue(message.From.Id, out var userUuid) && DataHelper.TryLocateUserFromUuid(userUuid, users, out _))
            {
                var (loadedNodes, loadNodesErrMsg) = await Nodes.LoadNodesAsync(cancellationToken);
                if (loadNodesErrMsg is not null)
                {
                    Console.WriteLine();
                    Console.WriteLine(loadNodesErrMsg);
                    return;
                }
                using var nodes = loadedNodes;

                // collect data
                var totalBytesUsed = nodes.Groups.Select(x => x.Value.BytesUsed).Aggregate(0UL, (x, y) => x + y);
                var totalBytesRemaining = nodes.Groups.All(x => x.Value.DataLimitInBytes > 0UL)
                    ? nodes.Groups.Select(x => x.Value.BytesRemaining).Aggregate(0UL, (x, y) => x + y)
                    : 0UL;

                var recordsByGroup = nodes.GetDataUsageByGroup();
                var recordsByUser = users.GetDataUsageByUser();

                // calculate column width
                var maxGroupNameLength = recordsByGroup.Select(x => x.group.Length)
                                                       .DefaultIfEmpty()
                                                       .Max();
                var groupNameFieldWidth = maxGroupNameLength > 5 ? maxGroupNameLength + 2 : 7;
                var maxUsernameLength = recordsByUser.Select(x => x.username.Length)
                                                     .DefaultIfEmpty()
                                                     .Max();
                var usernameFieldWidth = maxUsernameLength > 4 ? maxUsernameLength + 2 : 6;

                // sort records
                recordsByGroup = recordsByGroup.OrderByDescending(x => x.bytesUsed);
                recordsByUser = recordsByUser.OrderByDescending(x => x.bytesUsed);

                // total
                var replyBuilder = new StringBuilder();

                replyBuilder.AppendLine("In the last 30 days:");
                replyBuilder.AppendLine();

                if (totalBytesUsed != 0UL)
                    replyBuilder.AppendLine($"Total data used: *{ChatHelper.EscapeMarkdownV2Plaintext(Utilities.HumanReadableDataString1024(totalBytesUsed))}*");

                if (totalBytesRemaining != 0UL)
                    replyBuilder.AppendLine($"Total data remaining: *{ChatHelper.EscapeMarkdownV2Plaintext(Utilities.HumanReadableDataString1024(totalBytesRemaining))}*");

                replyBuilder.AppendLine();

                // CSV
                if (string.Equals(argument, "csv", StringComparison.OrdinalIgnoreCase))
                {
                    replyMarkdownV2 = replyBuilder.ToString();
                    var (dataUsageByGroup, dataUsageByUser) = ReportHelper.GenerateDataUsageCSV(recordsByGroup, recordsByUser);
                    Console.WriteLine(" Response: successful query.");

                    var sendSummaryTask = botClient.SendTextMessageAsync(message.Chat.Id,
                                                                         replyMarkdownV2,
                                                                         ParseMode.MarkdownV2,
                                                                         cancellationToken: cancellationToken);

                    var sendDataUsageByGroup = botClient.SendTextFileFromStringAsync(message.Chat.Id,
                                                                                "data-usage-by-group.csv",
                                                                                dataUsageByGroup,
                                                                                caption: "Data usage by group",
                                                                                cancellationToken: cancellationToken);

                    var sendDataUsageByUser = botClient.SendTextFileFromStringAsync(message.Chat.Id,
                                                                                "data-usage-by-user.csv",
                                                                                dataUsageByUser,
                                                                                caption: "Data usage by user",
                                                                                cancellationToken: cancellationToken);

                    await Task.WhenAll(sendSummaryTask, sendDataUsageByGroup, sendDataUsageByUser);
                    return;
                }

                // by group
                replyBuilder.AppendLine("*Data usage by group*");
                replyBuilder.AppendLine("```");

                if (recordsByGroup.All(x => x.bytesRemaining == 0UL)) // Omit data remaining column if no data.
                {
                    replyBuilder.AppendTableBorder(groupNameFieldWidth, 11);
                    replyBuilder.AppendLine($"|{"Group".PadRight(groupNameFieldWidth)}|{"Data Used",11}|");
                    replyBuilder.AppendTableBorder(groupNameFieldWidth, 11);

                    foreach (var (group, bytesUsed, _) in recordsByGroup)
                    {
                        replyBuilder.Append($"|{ChatHelper.EscapeMarkdownV2CodeBlock(group).PadRight(groupNameFieldWidth)}|");

                        if (bytesUsed != 0UL)
                            replyBuilder.AppendLine($"{Utilities.HumanReadableDataString1024(bytesUsed),11}|");
                        else
                            replyBuilder.AppendLine($"{string.Empty,11}|");
                    }

                    replyBuilder.AppendTableBorder(groupNameFieldWidth, 11);
                }
                else
                {
                    replyBuilder.AppendTableBorder(groupNameFieldWidth, 11, 16);
                    replyBuilder.AppendLine($"|{"Group".PadRight(groupNameFieldWidth)}|{"Data Used",11}|{"Data Remaining",16}|");
                    replyBuilder.AppendTableBorder(groupNameFieldWidth, 11, 16);

                    foreach (var (group, bytesUsed, bytesRemaining) in recordsByGroup)
                    {
                        replyBuilder.Append($"|{ChatHelper.EscapeMarkdownV2CodeBlock(group).PadRight(groupNameFieldWidth)}|");

                        if (bytesUsed != 0UL)
                            replyBuilder.Append($"{Utilities.HumanReadableDataString1024(bytesUsed),11}|");
                        else
                            replyBuilder.Append($"{string.Empty,11}|");

                        if (bytesRemaining != 0UL)
                            replyBuilder.AppendLine($"{Utilities.HumanReadableDataString1024(bytesRemaining),16}|");
                        else
                            replyBuilder.AppendLine($"{string.Empty,16}|");
                    }

                    replyBuilder.AppendTableBorder(groupNameFieldWidth, 11, 16);
                }

                replyBuilder.AppendLine("```");
                replyBuilder.AppendLine();

                // by user
                replyBuilder.AppendLine("*Data usage by user*");
                replyBuilder.AppendLine("```");

                if (recordsByUser.All(x => x.bytesRemaining == 0UL)) // Omit data remaining column if no data.
                {
                    replyBuilder.AppendTableBorder(usernameFieldWidth, 11);
                    replyBuilder.AppendLine($"|{"User".PadRight(usernameFieldWidth)}|{"Data Used",11}|");
                    replyBuilder.AppendTableBorder(usernameFieldWidth, 11);

                    foreach (var (username, bytesUsed, _) in recordsByUser)
                    {
                        replyBuilder.Append($"|{ChatHelper.EscapeMarkdownV2CodeBlock(username).PadRight(usernameFieldWidth)}|");

                        if (bytesUsed != 0UL)
                            replyBuilder.AppendLine($"{Utilities.HumanReadableDataString1024(bytesUsed),11}|");
                        else
                            replyBuilder.AppendLine($"{string.Empty,11}|");
                    }

                    replyBuilder.AppendTableBorder(usernameFieldWidth, 11);
                }
                else
                {
                    replyBuilder.AppendTableBorder(usernameFieldWidth, 11, 16);
                    replyBuilder.AppendLine($"|{"User".PadRight(usernameFieldWidth)}|{"Data Used",11}|{"Data Remaining",16}|");
                    replyBuilder.AppendTableBorder(usernameFieldWidth, 11, 16);

                    foreach (var (username, bytesUsed, bytesRemaining) in recordsByUser)
                    {
                        replyBuilder.Append($"|{ChatHelper.EscapeMarkdownV2CodeBlock(username).PadRight(usernameFieldWidth)}|");

                        if (bytesUsed != 0UL)
                            replyBuilder.Append($"{Utilities.HumanReadableDataString1024(bytesUsed),11}|");
                        else
                            replyBuilder.Append($"{string.Empty,11}|");

                        if (bytesRemaining != 0UL)
                            replyBuilder.AppendLine($"{Utilities.HumanReadableDataString1024(bytesRemaining),16}|");
                        else
                            replyBuilder.AppendLine($"{string.Empty,16}|");
                    }

                    replyBuilder.AppendTableBorder(usernameFieldWidth, 11, 16);
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
    }
}
