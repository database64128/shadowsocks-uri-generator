using ShadowsocksUriGenerator.Chatbot.Telegram.Utils;
using ShadowsocksUriGenerator.Data;
using ShadowsocksUriGenerator.Services;
using ShadowsocksUriGenerator.Utils;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ShadowsocksUriGenerator.Chatbot.Telegram.Commands;

public static class DataCommands
{
    public static async Task<string> GetUserDataUsageAsync(ITelegramBotClient botClient, Message message, string? argument, BotConfig config, DataService dataService, CancellationToken cancellationToken = default)
    {
        string replyMarkdownV2 = @"An error occurred\."; // the initial value is only used when there's an error in the logic
        string result = "error_unknown";

        if (message.From is null)
        {
            replyMarkdownV2 = @"Can't determine your Telegram user ID\. Are you sending from a channel?";
            result = "missing message sender";
        }
        else if (config.ChatAssociations.TryGetValue(message.From.Id, out var userUuid) && DataHelper.TryLocateUserFromUuid(userUuid, dataService.UsersData, out var userEntry))
        {
            if (argument is not null && userEntry.Value.Key != argument) // look up specified user
            {
                if (!config.UsersCanSeeAllUsers)
                {
                    replyMarkdownV2 = @"The admin won't let you view other user's data usage\.";
                    result = "permission denied";
                }
                else if (dataService.UsersData.UserDict.TryGetValue(argument, out var user))
                {
                    userEntry = new(argument, user);
                }
                else
                {
                    replyMarkdownV2 = @"The specified user doesn't exist\.";
                    result = "target user not found";
                }
            }

            if (argument is null || userEntry.Value.Key == argument) // no argument or successful lookup
            {
                var username = userEntry.Value.Key;
                var user = userEntry.Value.Value;

                var records = userEntry.Value.Value.GetDataUsage(username, dataService.NodesData);

                // sort records
                records = [.. records.OrderByDescending(x => x.bytesUsed)];

                var maxNameLength = records.Select(x => x.group.Length)
                                           .DefaultIfEmpty()
                                           .Max();
                var nameFieldWidth = maxNameLength > 5 ? maxNameLength + 2 : 7;

                var replyBuilder = new StringBuilder();

                replyBuilder.AppendLine($"User: *{ChatHelper.EscapeMarkdownV2Plaintext(username)}*");
                replyBuilder.AppendLine($"Data used: *{ChatHelper.EscapeMarkdownV2Plaintext(InteractionHelper.HumanReadableDataString1024(user.BytesUsed))}*");

                if (user.BytesRemaining != 0UL)
                    replyBuilder.AppendLine($"Data remaining: *{ChatHelper.EscapeMarkdownV2Plaintext(InteractionHelper.HumanReadableDataString1024(user.BytesRemaining))}*");

                if (user.DataLimitInBytes != 0UL)
                    replyBuilder.AppendLine($"Data limit: *{ChatHelper.EscapeMarkdownV2Plaintext(InteractionHelper.HumanReadableDataString1024(user.DataLimitInBytes))}*");

                replyBuilder.AppendLine("```");

                if (records.All(x => x.bytesRemaining == 0UL)) // Omit data remaining column if no data.
                {
                    replyBuilder.AppendTableBorder(nameFieldWidth, 11);
                    replyBuilder.AppendLine($"|{"Group".PadRight(nameFieldWidth)}|{"Data Used",11}|");
                    replyBuilder.AppendTableBorder(nameFieldWidth, 11);

                    foreach (var (group, bytesUsed, _) in records)
                    {
                        replyBuilder.AppendLine($"|{ChatHelper.EscapeMarkdownV2CodeBlock(group).PadRight(nameFieldWidth)}|{InteractionHelper.HumanReadableDataString1024(bytesUsed),11}|");
                    }

                    replyBuilder.AppendTableBorder(nameFieldWidth, 11);
                }
                else
                {
                    replyBuilder.AppendTableBorder(nameFieldWidth, 11, 16);
                    replyBuilder.AppendLine($"|{"Group".PadRight(nameFieldWidth)}|{"Data Used",11}|{"Data Remaining",16}|");
                    replyBuilder.AppendTableBorder(nameFieldWidth, 11, 16);

                    foreach (var (group, bytesUsed, bytesRemaining) in records)
                    {
                        replyBuilder.Append($"|{ChatHelper.EscapeMarkdownV2CodeBlock(group).PadRight(nameFieldWidth)}|{InteractionHelper.HumanReadableDataString1024(bytesUsed),11}|");

                        if (bytesRemaining != 0UL)
                            replyBuilder.AppendLine($"{InteractionHelper.HumanReadableDataString1024(bytesRemaining),16}|");
                        else
                            replyBuilder.AppendLine($"{string.Empty,16}|");
                    }

                    replyBuilder.AppendTableBorder(nameFieldWidth, 11, 16);
                }

                replyBuilder.AppendLine("```");

                replyMarkdownV2 = replyBuilder.ToString();
                result = "success";
            }
        }
        else
        {
            replyMarkdownV2 = @"You must link your Telegram account to your user first\.";
            result = "user not linked";
        }

        _ = await botClient.SendPossiblyLongTextMessageAsync(
            message.Chat.Id,
            replyMarkdownV2,
            parseMode: ParseMode.MarkdownV2,
            replyParameters: message,
            cancellationToken: cancellationToken);

        return result;
    }

    public static async Task<string> GetUserDataLimitAsync(ITelegramBotClient botClient, Message message, string? argument, BotConfig config, DataService dataService, CancellationToken cancellationToken = default)
    {
        string replyMarkdownV2 = @"An error occurred\."; // the initial value is only used when there's an error in the logic
        string result = "error_unknown";

        if (message.From is null)
        {
            replyMarkdownV2 = @"Can't determine your Telegram user ID\. Are you sending from a channel?";
            result = "missing message sender";
        }
        else if (config.ChatAssociations.TryGetValue(message.From.Id, out var userUuid) && DataHelper.TryLocateUserFromUuid(userUuid, dataService.UsersData, out var userEntry))
        {
            if (argument is not null && userEntry.Value.Key != argument) // look up specified user
            {
                if (!config.UsersCanSeeAllUsers)
                {
                    replyMarkdownV2 = @"The admin won't let you view other user's data limit\.";
                    result = "permission denied";
                }
                else if (dataService.UsersData.UserDict.TryGetValue(argument, out var user))
                {
                    userEntry = new(argument, user);
                }
                else
                {
                    replyMarkdownV2 = @"The specified user doesn't exist\.";
                    result = "target user not found";
                }
            }

            if (argument is null || userEntry.Value.Key == argument) // no argument or successful lookup
            {
                var username = userEntry.Value.Key;
                var user = userEntry.Value.Value;

                var replyBuilder = new StringBuilder();

                replyBuilder.AppendLine($"User: *{ChatHelper.EscapeMarkdownV2Plaintext(username)}*");

                if (user.DataLimitInBytes != 0UL)
                    replyBuilder.AppendLine($"Global data limit: *{ChatHelper.EscapeMarkdownV2Plaintext(InteractionHelper.HumanReadableDataString1024(user.DataLimitInBytes))}*");

                if (user.PerGroupDataLimitInBytes != 0UL)
                    replyBuilder.AppendLine($"Per-group data limit: *{ChatHelper.EscapeMarkdownV2Plaintext(InteractionHelper.HumanReadableDataString1024(user.PerGroupDataLimitInBytes))}*");

                var customLimits = user.Memberships.Where(x => x.Value.DataLimitInBytes > 0UL).Select(x => (x.Key, x.Value.DataLimitInBytes));

                if (customLimits.Any())
                {
                    var maxNameLength = customLimits.Max(x => x.Key.Length);
                    var nameFieldWidth = maxNameLength > 5 ? maxNameLength + 2 : 7;

                    replyBuilder.AppendLine("```");
                    replyBuilder.AppendTableBorder(nameFieldWidth, 19);
                    replyBuilder.AppendLine($"|{"Group".PadRight(nameFieldWidth)}|{"Custom Data Limit",19}|");
                    replyBuilder.AppendTableBorder(nameFieldWidth, 19);

                    foreach ((var group, var dataLimitInBytes) in customLimits)
                    {
                        replyBuilder.AppendLine($"|{ChatHelper.EscapeMarkdownV2CodeBlock(group).PadRight(nameFieldWidth)}|{InteractionHelper.HumanReadableDataString1024(dataLimitInBytes),19}|");
                    }

                    replyBuilder.AppendTableBorder(nameFieldWidth, 19);
                    replyBuilder.AppendLine("```");
                }

                replyMarkdownV2 = replyBuilder.ToString();
                result = "success";
            }
        }
        else
        {
            replyMarkdownV2 = @"You must link your Telegram account to your user first\.";
            result = "user not linked";
        }

        _ = await botClient.SendPossiblyLongTextMessageAsync(
            message.Chat.Id,
            replyMarkdownV2,
            parseMode: ParseMode.MarkdownV2,
            replyParameters: message,
            cancellationToken: cancellationToken);

        return result;
    }

    public static async Task<string> GetGroupDataUsageAsync(ITelegramBotClient botClient, Message message, string? argument, BotConfig config, DataService dataService, CancellationToken cancellationToken = default)
    {
        string replyMarkdownV2, result;

        if (!config.UsersCanSeeGroupDataUsage)
        {
            replyMarkdownV2 = @"The admin has disabled the command\.";
            result = "permission denied";
        }
        else if (message.From is null)
        {
            replyMarkdownV2 = @"Can't determine your Telegram user ID\. Are you sending from a channel?";
            result = "missing message sender";
        }
        else if (config.ChatAssociations.TryGetValue(message.From.Id, out var userUuid) && DataHelper.TryLocateUserFromUuid(userUuid, dataService.UsersData, out var userEntry))
        {
            var replyBuilder = new StringBuilder();

            if (string.IsNullOrEmpty(argument)) // target the user's groups
            {
                var ownedGroupEntries = dataService.NodesData.Groups.Where(x => x.Value.OwnerUuid == userUuid);

                replyBuilder.AppendLine($"Owned Groups: {ownedGroupEntries.Count()}");
                replyBuilder.AppendLine();

                foreach (var groupEntry in ownedGroupEntries)
                {
                    GetGroupDataUsageCore(replyBuilder, groupEntry, dataService.UsersData);
                }

                replyMarkdownV2 = replyBuilder.ToString();
                result = "success";
            }
            else if (userEntry.Value.Value.Memberships.ContainsKey(argument) || config.UsersCanSeeAllGroups) // user is allowed to view it
            {
                if (dataService.NodesData.Groups.TryGetValue(argument, out var targetGroup))
                {
                    GetGroupDataUsageCore(replyBuilder, new(argument, targetGroup), dataService.UsersData);
                    replyMarkdownV2 = replyBuilder.ToString();
                    result = "success";
                }
                else
                {
                    replyMarkdownV2 = @"The specified group doesn't exist\.";
                    result = "nonexistent group";
                }
            }
            else
            {
                replyMarkdownV2 = @"You are not authorized to access the information\.";
                result = "permission denied";
            }
        }
        else
        {
            replyMarkdownV2 = @"You must link your Telegram account to your user first\.";
            result = "user not linked";
        }

        _ = await botClient.SendPossiblyLongTextMessageAsync(
            message.Chat.Id,
            replyMarkdownV2,
            parseMode: ParseMode.MarkdownV2,
            replyParameters: message,
            cancellationToken: cancellationToken);

        return result;
    }

    private static void GetGroupDataUsageCore(StringBuilder replyBuilder, KeyValuePair<string, Group> groupEntry, Users users)
    {
        var records = groupEntry.Value.GetDataUsage(groupEntry.Key, users)
                                      .OrderByDescending(x => x.bytesUsed)
                                      .ToList();

        replyBuilder.AppendLine($"Group: *{ChatHelper.EscapeMarkdownV2Plaintext(groupEntry.Key)}*");
        replyBuilder.AppendLine($"Data used: *{ChatHelper.EscapeMarkdownV2Plaintext(InteractionHelper.HumanReadableDataString1024(groupEntry.Value.BytesUsed))}*");
        if (groupEntry.Value.BytesRemaining != 0UL)
            replyBuilder.AppendLine($"Data remaining: *{ChatHelper.EscapeMarkdownV2Plaintext(InteractionHelper.HumanReadableDataString1024(groupEntry.Value.BytesRemaining))}*");
        if (groupEntry.Value.DataLimitInBytes != 0UL)
            replyBuilder.AppendLine($"Data limit: *{ChatHelper.EscapeMarkdownV2Plaintext(InteractionHelper.HumanReadableDataString1024(groupEntry.Value.DataLimitInBytes))}*");

        if (records.Count == 0)
            return;

        var maxNameLength = records.Max(x => x.username.Length);
        var nameFieldWidth = maxNameLength > 4 ? maxNameLength + 2 : 6;

        replyBuilder.AppendLine("```");

        if (records.All(x => x.bytesRemaining == 0UL)) // Omit data remaining column if no data.
        {
            replyBuilder.AppendTableBorder(nameFieldWidth, 11);
            replyBuilder.AppendLine($"|{"User".PadRight(nameFieldWidth)}|{"Data Used",11}|");
            replyBuilder.AppendTableBorder(nameFieldWidth, 11);

            foreach (var (username, bytesUsed, bytesRemaining) in records)
            {
                replyBuilder.AppendLine($"|{ChatHelper.EscapeMarkdownV2CodeBlock(username).PadRight(nameFieldWidth)}|{InteractionHelper.HumanReadableDataString1024(bytesUsed),11}|");
            }

            replyBuilder.AppendTableBorder(nameFieldWidth, 11);
        }
        else
        {
            replyBuilder.AppendTableBorder(nameFieldWidth, 11, 16);
            replyBuilder.AppendLine($"|{"User".PadRight(nameFieldWidth)}|{"Data Used",11}|{"Data Remaining",16}|");
            replyBuilder.AppendTableBorder(nameFieldWidth, 11, 16);

            foreach (var (username, bytesUsed, bytesRemaining) in records)
            {
                replyBuilder.Append($"|{ChatHelper.EscapeMarkdownV2CodeBlock(username).PadRight(nameFieldWidth)}|{InteractionHelper.HumanReadableDataString1024(bytesUsed),11}|");

                if (bytesRemaining != 0UL)
                    replyBuilder.AppendLine($"{InteractionHelper.HumanReadableDataString1024(bytesRemaining),16}|");
                else
                    replyBuilder.AppendLine($"{string.Empty,16}|");
            }

            replyBuilder.AppendTableBorder(nameFieldWidth, 11, 16);
        }

        replyBuilder.AppendLine("```");
    }

    public static async Task<string> GetGroupDataLimitAsync(ITelegramBotClient botClient, Message message, string? argument, BotConfig config, DataService dataService, CancellationToken cancellationToken = default)
    {
        string replyMarkdownV2, result;

        if (!config.UsersCanSeeGroupDataLimit)
        {
            replyMarkdownV2 = @"The admin has disabled the command\.";
            result = "permission denied";
        }
        else if (string.IsNullOrEmpty(argument))
        {
            replyMarkdownV2 = @"Please specify a group\.";
            result = "missing argument";
        }
        else if (message.From is null)
        {
            replyMarkdownV2 = @"Can't determine your Telegram user ID\. Are you sending from a channel?";
            result = "missing message sender";
        }
        else if (config.ChatAssociations.TryGetValue(message.From.Id, out var userUuid) && DataHelper.TryLocateUserFromUuid(userUuid, dataService.UsersData, out var userEntry))
        {
            if (userEntry.Value.Value.Memberships.ContainsKey(argument) || config.UsersCanSeeAllGroups) // user is allowed to view it
            {
                if (dataService.NodesData.Groups.TryGetValue(argument, out var targetGroup))
                {
                    var replyBuilder = new StringBuilder();

                    replyBuilder.AppendLine($"Group: *{ChatHelper.EscapeMarkdownV2Plaintext(argument)}*");

                    if (targetGroup.DataLimitInBytes != 0UL)
                        replyBuilder.AppendLine($"Global data limit: *{ChatHelper.EscapeMarkdownV2Plaintext(InteractionHelper.HumanReadableDataString1024(targetGroup.DataLimitInBytes))}*");

                    if (targetGroup.PerUserDataLimitInBytes != 0UL)
                        replyBuilder.AppendLine($"Per-user data limit: *{ChatHelper.EscapeMarkdownV2Plaintext(InteractionHelper.HumanReadableDataString1024(targetGroup.PerUserDataLimitInBytes))}*");

                    var outlineAccessKeyCustomLimits = targetGroup.OutlineAccessKeys?.Where(x => x.DataLimit is not null).Select(x => (x.Name, x.DataLimit!.Bytes));

                    if (outlineAccessKeyCustomLimits?.Any() ?? false)
                    {
                        var maxNameLength = outlineAccessKeyCustomLimits.Max(x => x.Name.Length);
                        var nameFieldWidth = maxNameLength > 4 ? maxNameLength + 2 : 6;

                        replyBuilder.AppendLine("```");
                        replyBuilder.AppendTableBorder(nameFieldWidth, 19);
                        replyBuilder.AppendLine($"|{"User".PadRight(nameFieldWidth)}|{"Custom Data Limit",19}|");
                        replyBuilder.AppendTableBorder(nameFieldWidth, 19);

                        foreach ((var username, var dataLimitInBytes) in outlineAccessKeyCustomLimits)
                        {
                            replyBuilder.AppendLine($"|{ChatHelper.EscapeMarkdownV2CodeBlock(username).PadRight(nameFieldWidth)}|{InteractionHelper.HumanReadableDataString1024(dataLimitInBytes),19}|");
                        }

                        replyBuilder.AppendTableBorder(nameFieldWidth, 19);
                        replyBuilder.AppendLine("```");
                    }

                    replyMarkdownV2 = replyBuilder.ToString();
                    result = "success";
                }
                else
                {
                    replyMarkdownV2 = @"The specified group doesn't exist\.";
                    result = "nonexistent group";
                }
            }
            else
            {
                replyMarkdownV2 = @"You are not authorized to access the information\.";
                result = "permission denied";
            }
        }
        else
        {
            replyMarkdownV2 = @"You must link your Telegram account to your user first\.";
            result = "user not linked";
        }

        _ = await botClient.SendPossiblyLongTextMessageAsync(
            message.Chat.Id,
            replyMarkdownV2,
            parseMode: ParseMode.MarkdownV2,
            replyParameters: message,
            cancellationToken: cancellationToken);

        return result;
    }
}
