using ShadowsocksUriGenerator.Chatbot.Telegram.Utils;
using ShadowsocksUriGenerator.Services;
using ShadowsocksUriGenerator.Utils;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ShadowsocksUriGenerator.Chatbot.Telegram.Commands;

public static class ReportCommand
{
    public static async Task<string> GenerateReportAsync(ITelegramBotClient botClient, Message message, string? argument, BotConfig config, DataService dataService, CancellationToken cancellationToken = default)
    {
        string replyMarkdownV2, result;

        if (!config.UsersCanSeeAllUsers || !config.UsersCanSeeAllGroups || !config.UsersCanSeeGroupDataUsage)
        {
            replyMarkdownV2 = @"You are not authorized to view the report\.";
            result = "permission denied";
        }
        else if (message.From is null)
        {
            replyMarkdownV2 = @"Can't determine your Telegram user ID\. Are you sending from a channel?";
            result = "missing message sender";
        }
        else if (config.ChatAssociations.TryGetValue(message.From.Id, out var userUuid) && DataHelper.TryLocateUserFromUuid(userUuid, dataService.UsersData, out _))
        {
            var users = dataService.UsersData;
            var nodes = dataService.NodesData;

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
                replyBuilder.AppendLine($"Total data used: *{ChatHelper.EscapeMarkdownV2Plaintext(InteractionHelper.HumanReadableDataString1024(totalBytesUsed))}*");

            if (totalBytesRemaining != 0UL)
                replyBuilder.AppendLine($"Total data remaining: *{ChatHelper.EscapeMarkdownV2Plaintext(InteractionHelper.HumanReadableDataString1024(totalBytesRemaining))}*");

            // CSV
            if (string.Equals(argument, "csv", StringComparison.OrdinalIgnoreCase))
            {
                replyMarkdownV2 = replyBuilder.ToString();
                var (dataUsageByGroup, dataUsageByUser) = ReportHelper.GenerateDataUsageCSV(recordsByGroup, recordsByUser);

                Task sendSummaryTask = botClient.SendMessage(
                    message.Chat.Id,
                    replyMarkdownV2,
                    parseMode: ParseMode.MarkdownV2,
                    cancellationToken: cancellationToken);

                Task sendDataUsageByGroup = botClient.SendTextFileFromStringAsync(
                    message.Chat.Id,
                    "data-usage-by-group.csv",
                    dataUsageByGroup,
                    caption: "Data usage by group",
                    cancellationToken: cancellationToken);

                Task sendDataUsageByUser = botClient.SendTextFileFromStringAsync(
                    message.Chat.Id,
                    "data-usage-by-user.csv",
                    dataUsageByUser,
                    caption: "Data usage by user",
                    cancellationToken: cancellationToken);

                await Task.WhenAll(sendSummaryTask, sendDataUsageByGroup, sendDataUsageByUser);
                return "success";
            }

            // by group
            var byGroupSB = new StringBuilder();

            byGroupSB.AppendLine("*Data usage by group*");
            byGroupSB.AppendLine("```");

            if (recordsByGroup.All(x => x.bytesRemaining == 0UL)) // Omit data remaining column if no data.
            {
                byGroupSB.AppendTableBorder(groupNameFieldWidth, 11);
                byGroupSB.AppendLine($"|{"Group".PadRight(groupNameFieldWidth)}|{"Data Used",11}|");
                byGroupSB.AppendTableBorder(groupNameFieldWidth, 11);

                foreach (var (group, bytesUsed, _) in recordsByGroup)
                {
                    byGroupSB.Append($"|{ChatHelper.EscapeMarkdownV2CodeBlock(group).PadRight(groupNameFieldWidth)}|");

                    if (bytesUsed != 0UL)
                        byGroupSB.AppendLine($"{InteractionHelper.HumanReadableDataString1024(bytesUsed),11}|");
                    else
                        byGroupSB.AppendLine($"{string.Empty,11}|");
                }

                byGroupSB.AppendTableBorder(groupNameFieldWidth, 11);
            }
            else
            {
                byGroupSB.AppendTableBorder(groupNameFieldWidth, 11, 16);
                byGroupSB.AppendLine($"|{"Group".PadRight(groupNameFieldWidth)}|{"Data Used",11}|{"Data Remaining",16}|");
                byGroupSB.AppendTableBorder(groupNameFieldWidth, 11, 16);

                foreach (var (group, bytesUsed, bytesRemaining) in recordsByGroup)
                {
                    byGroupSB.Append($"|{ChatHelper.EscapeMarkdownV2CodeBlock(group).PadRight(groupNameFieldWidth)}|");

                    if (bytesUsed != 0UL)
                        byGroupSB.Append($"{InteractionHelper.HumanReadableDataString1024(bytesUsed),11}|");
                    else
                        byGroupSB.Append($"{string.Empty,11}|");

                    if (bytesRemaining != 0UL)
                        byGroupSB.AppendLine($"{InteractionHelper.HumanReadableDataString1024(bytesRemaining),16}|");
                    else
                        byGroupSB.AppendLine($"{string.Empty,16}|");
                }

                byGroupSB.AppendTableBorder(groupNameFieldWidth, 11, 16);
            }

            byGroupSB.AppendLine("```");

            // by user
            var byUserSB = new StringBuilder();

            byUserSB.AppendLine("*Data usage by user*");
            byUserSB.AppendLine("```");

            if (recordsByUser.All(x => x.bytesRemaining == 0UL)) // Omit data remaining column if no data.
            {
                byUserSB.AppendTableBorder(usernameFieldWidth, 11);
                byUserSB.AppendLine($"|{"User".PadRight(usernameFieldWidth)}|{"Data Used",11}|");
                byUserSB.AppendTableBorder(usernameFieldWidth, 11);

                foreach (var (username, bytesUsed, _) in recordsByUser)
                {
                    byUserSB.Append($"|{ChatHelper.EscapeMarkdownV2CodeBlock(username).PadRight(usernameFieldWidth)}|");

                    if (bytesUsed != 0UL)
                        byUserSB.AppendLine($"{InteractionHelper.HumanReadableDataString1024(bytesUsed),11}|");
                    else
                        byUserSB.AppendLine($"{string.Empty,11}|");
                }

                byUserSB.AppendTableBorder(usernameFieldWidth, 11);
            }
            else
            {
                byUserSB.AppendTableBorder(usernameFieldWidth, 11, 16);
                byUserSB.AppendLine($"|{"User".PadRight(usernameFieldWidth)}|{"Data Used",11}|{"Data Remaining",16}|");
                byUserSB.AppendTableBorder(usernameFieldWidth, 11, 16);

                foreach (var (username, bytesUsed, bytesRemaining) in recordsByUser)
                {
                    byUserSB.Append($"|{ChatHelper.EscapeMarkdownV2CodeBlock(username).PadRight(usernameFieldWidth)}|");

                    if (bytesUsed != 0UL)
                        byUserSB.Append($"{InteractionHelper.HumanReadableDataString1024(bytesUsed),11}|");
                    else
                        byUserSB.Append($"{string.Empty,11}|");

                    if (bytesRemaining != 0UL)
                        byUserSB.AppendLine($"{InteractionHelper.HumanReadableDataString1024(bytesRemaining),16}|");
                    else
                        byUserSB.AppendLine($"{string.Empty,16}|");
                }

                byUserSB.AppendTableBorder(usernameFieldWidth, 11, 16);
            }

            byUserSB.AppendLine("```");

            if (replyBuilder.Length + Environment.NewLine.Length + byGroupSB.Length + Environment.NewLine.Length + byUserSB.Length <= 4096)
            {
                replyBuilder.AppendLine();
                replyBuilder.Append(byGroupSB);
                replyBuilder.AppendLine();
                replyBuilder.Append(byUserSB);
                replyMarkdownV2 = replyBuilder.ToString();
                result = "success";
            }
            else
            {
                replyMarkdownV2 = replyBuilder.ToString();

                _ = await botClient.SendMessage(
                    message.Chat.Id,
                    replyMarkdownV2,
                    parseMode: ParseMode.MarkdownV2,
                    replyParameters: message,
                    cancellationToken: cancellationToken);

                _ = await botClient.SendPossiblyLongTextMessageAsync(
                    message.Chat.Id,
                    byGroupSB.ToString(),
                    parseMode: ParseMode.MarkdownV2,
                    cancellationToken: cancellationToken);

                _ = await botClient.SendPossiblyLongTextMessageAsync(
                    message.Chat.Id,
                    byUserSB.ToString(),
                    parseMode: ParseMode.MarkdownV2,
                    cancellationToken: cancellationToken);

                return "success";
            }
        }
        else
        {
            replyMarkdownV2 = @"You must link your Telegram account to your user first\.";
            result = "user not linked";
        }

        _ = await botClient.SendMessage(
            message.Chat.Id,
            replyMarkdownV2,
            parseMode: ParseMode.MarkdownV2,
            replyParameters: message,
            cancellationToken: cancellationToken);

        return result;
    }
}
