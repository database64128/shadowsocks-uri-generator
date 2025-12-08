using Microsoft.Extensions.Logging;
using ShadowsocksUriGenerator.Chatbot.Telegram.Utils;
using ShadowsocksUriGenerator.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ShadowsocksUriGenerator.Chatbot.Telegram.Commands;

public static class AuthCommands
{
    public static async Task<string> StartAsync(ITelegramBotClient botClient, Message message, BotConfig config, CancellationToken cancellationToken = default)
    {
        var serviceName = string.IsNullOrEmpty(config.ServiceName) ? "with us" : config.ServiceName;

        var replyMarkdownV2 = $@"🧑‍✈️ Good evening\! Thank you for flying {serviceName}\.

✈️ To get your boarding pass, please use `/link <UUID>` to link your Telegram account to your user\.";

        await botClient.SendMessage(
            message.Chat.Id,
            replyMarkdownV2,
            parseMode: ParseMode.MarkdownV2,
            replyParameters: message,
            cancellationToken: cancellationToken);

        return "success";
    }

    public static async Task<string> LinkAsync(ITelegramBotClient botClient, Message message, string? argument, BotConfig config, DataService dataService, ILogger logger, CancellationToken cancellationToken = default)
    {
        string reply, result;

        if (!config.AllowChatAssociation)
        {
            reply = "The admin has disabled Telegram association.";
            result = "command disabled";
        }
        else if (message.Chat.Type != ChatType.Private || message.From is null)
        {
            reply = "Associations must be made in a private chat.";
            result = "not in private chat";
        }
        else if (string.IsNullOrEmpty(argument))
        {
            reply = "Please provide your user UUID as the command argument.";
            result = "missing argument";
        }
        else
        {
            if (!DataHelper.TryLocateUserFromUuid(argument, dataService.UsersData, out var matchedUser))
            {
                reply = "User not found.";
                result = "user not found";
            }
            else if (config.ChatAssociations.TryGetValue(message.From.Id, out var userUuid))
            {
                if (userUuid == matchedUser.Value.Value.Uuid)
                {
                    reply = $"You are already linked to {matchedUser.Value.Key}.";
                    result = "already linked";
                }
                else
                {
                    reply = $"You are already linked to another user with UUID {userUuid}.";
                    result = "already linked to another user";
                }
            }
            else
            {
                config.ChatAssociations.Add(message.From.Id, matchedUser.Value.Value.Uuid);

                reply = $"Successfully linked your Telegram account to {matchedUser.Value.Key}.";
                result = "success";

                try
                {
                    await config.SaveAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to save bot config after linking Telegram account");
                    return "error_saving_config";
                }
            }
        }

        _ = await botClient.SendMessage(
            message.Chat.Id,
            reply,
            replyParameters: message,
            cancellationToken: cancellationToken);

        return result;
    }

    public static async Task<string> UnlinkAsync(ITelegramBotClient botClient, Message message, BotConfig config, ILogger logger, CancellationToken cancellationToken = default)
    {
        string reply, result;

        if (!config.AllowChatAssociation)
        {
            reply = "The admin has disabled Telegram association.";
            result = "command disabled";
        }
        else if (message.Chat.Type != ChatType.Private || message.From is null)
        {
            reply = "Associations must be made in a private chat.";
            result = "not in private chat";
        }
        else if (config.ChatAssociations.Remove(message.From.Id, out var userUuid))
        {
            reply = $"Successfully unlinked your Telegram account from {userUuid}.";
            result = "success";

            try
            {
                await config.SaveAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to save bot config after unlinking Telegram account");
                return "error_saving_config";
            }
        }
        else
        {
            reply = "You are not linked to any user.";
            result = "not linked";
        }

        _ = await botClient.SendMessage(
            message.Chat.Id,
            reply,
            replyParameters: message,
            cancellationToken: cancellationToken);

        return result;
    }
}
