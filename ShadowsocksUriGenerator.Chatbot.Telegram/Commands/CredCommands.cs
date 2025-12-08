using ShadowsocksUriGenerator.Chatbot.Telegram.Utils;
using ShadowsocksUriGenerator.OnlineConfig;
using ShadowsocksUriGenerator.Services;
using ShadowsocksUriGenerator.Utils;
using System.Text;
using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ShadowsocksUriGenerator.Chatbot.Telegram.Commands;

public static class CredCommands
{
    public static async Task<string> GetSsLinksAsync(ITelegramBotClient botClient, Message message, string? argument, BotConfig config, DataService dataService, CancellationToken cancellationToken = default)
    {
        string reply, result;

        if (message.Chat.Type != ChatType.Private || message.From is null)
        {
            reply = "Retrieval of sensitive information is only allowed in private chats.";
            result = "not in private chat";
        }
        else if (config.ChatAssociations.TryGetValue(message.From.Id, out var userUuid) && DataHelper.TryLocateUserFromUuid(userUuid, dataService.UsersData, out var userEntry))
        {
            var user = userEntry.Value.Value;

            var uris = string.IsNullOrEmpty(argument) ? user.GetSSUris(dataService.UsersData, dataService.NodesData) : user.GetSSUris(dataService.UsersData, dataService.NodesData, argument);
            if (uris.Any())
            {
                var replyBuilder = new StringBuilder();

                foreach (var uri in uris)
                    replyBuilder.AppendLine($"{uri.AbsoluteUri}");

                reply = replyBuilder.ToString();
                result = "success";
            }
            else
            {
                reply = "No links are available for you.";
                result = "success";
            }
        }
        else
        {
            reply = "You must link your Telegram account to your user first.";
            result = "user not linked";
        }

        _ = await botClient.SendPossiblyLongTextMessageAsync(
            message.Chat.Id,
            reply,
            linkPreviewOptions: new() { IsDisabled = true, },
            replyParameters: message,
            cancellationToken: cancellationToken);

        return result;
    }

    public static async Task<string> GetOnlineConfigLinksAsync(ITelegramBotClient botClient, Message message, BotConfig config, DataService dataService, CancellationToken cancellationToken = default)
    {
        string replyMarkdownV2, result;

        if (message.Chat.Type != ChatType.Private || message.From is null)
        {
            replyMarkdownV2 = @"Retrieval of sensitive information is only allowed in private chats\.";
            result = "not in private chat";
        }
        else if (config.ChatAssociations.TryGetValue(message.From.Id, out var userUuid) && DataHelper.TryLocateUserFromUuid(userUuid, dataService.UsersData, out var userEntry))
        {
            var settings = dataService.SettingsData;
            var user = userEntry.Value.Value;

            var replyBuilder = new StringBuilder();

            var printApiLinks = !string.IsNullOrEmpty(settings.ApiServerBaseUrl) && !string.IsNullOrEmpty(settings.ApiServerSecretPath);
            var printStaticLinks = !string.IsNullOrEmpty(settings.OnlineConfigDeliveryRootUri);

            if (printApiLinks)
            {
                OOCv1ApiToken oocv1ApiToken = new(1, settings.ApiServerBaseUrl, settings.ApiServerSecretPath, user.Uuid, null);
                string oocv1ApiTokenString = JsonSerializer.Serialize(oocv1ApiToken, OnlineConfigCamelCaseJsonSerializerContext.Default.OOCv1ApiToken);

                replyBuilder.AppendLine($"OOCv1 API Token: `{ChatHelper.EscapeMarkdownV2CodeBlock(oocv1ApiTokenString)}`");
                replyBuilder.AppendLine(ChatHelper.EscapeMarkdownV2Plaintext($"OOCv1 API URL: {settings.ApiServerBaseUrl}/{settings.ApiServerSecretPath}/ooc/v1/{user.Uuid}"));
                replyBuilder.AppendLine(ChatHelper.EscapeMarkdownV2Plaintext($"Shadowsocks Go Client Config URL: {settings.ApiServerBaseUrl}/{settings.ApiServerSecretPath}/shadowsocks-go/clients/{user.Uuid}"));
                replyBuilder.AppendLine(ChatHelper.EscapeMarkdownV2Plaintext($"Sing Box Outbound Config URL: {settings.ApiServerBaseUrl}/{settings.ApiServerSecretPath}/sing-box/outbounds/{user.Uuid}"));
                replyBuilder.AppendLine(ChatHelper.EscapeMarkdownV2Plaintext($"SIP008 Delivery URL: {settings.ApiServerBaseUrl}/{settings.ApiServerSecretPath}/sip008/{user.Uuid}"));
                replyBuilder.AppendLine();
            }

            if (printStaticLinks)
            {
                replyBuilder.AppendLine(ChatHelper.EscapeMarkdownV2Plaintext($"Legacy SIP008 Static File Delivery URL (All Servers): {settings.OnlineConfigDeliveryRootUri}/{user.Uuid}.json"));

                if (settings.OnlineConfigDeliverByGroup)
                {
                    replyBuilder.AppendLine();
                    replyBuilder.AppendLine(ChatHelper.EscapeMarkdownV2Plaintext("Legacy SIP008 Static File Delivery URLs (By Group):"));
                    replyBuilder.AppendLine();

                    foreach (var group in user.Memberships.Keys)
                    {
                        replyBuilder.AppendLine(ChatHelper.EscapeMarkdownV2Plaintext($"{settings.OnlineConfigDeliveryRootUri}/{user.Uuid}/{Uri.EscapeDataString(group)}.json"));
                    }
                }
            }

            replyBuilder.AppendLine(ChatHelper.EscapeMarkdownV2Plaintext($"Swagger UI URL: {settings.ApiServerBaseUrl}/{settings.ApiServerSecretPath}/swagger/"));
            replyBuilder.AppendLine(ChatHelper.EscapeMarkdownV2Plaintext($"ReDoc UI URL: {settings.ApiServerBaseUrl}/{settings.ApiServerSecretPath}/api-docs/"));

            replyMarkdownV2 = replyBuilder.ToString();
            result = "success";
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
            linkPreviewOptions: new() { IsDisabled = true, },
            replyParameters: message,
            cancellationToken: cancellationToken);

        return result;
    }

    public static async Task<string> GetCredentialsAsync(ITelegramBotClient botClient, Message message, string? argument, BotConfig config, DataService dataService, CancellationToken cancellationToken = default)
    {
        string replyMarkdownV2, result;

        if (message.Chat.Type != ChatType.Private || message.From is null)
        {
            replyMarkdownV2 = @"Retrieval of sensitive information is only allowed in private chats\.";
            result = "not in private chat";
        }
        else if (config.ChatAssociations.TryGetValue(message.From.Id, out var userUuid) && DataHelper.TryLocateUserFromUuid(userUuid, dataService.UsersData, out var userEntry))
        {
            var username = userEntry.Value.Key;
            var user = userEntry.Value.Value;

            List<(string group, string method, string password)> filteredCreds = [];

            foreach (var membership in user.Memberships)
            {
                if (!string.IsNullOrEmpty(argument) && argument != membership.Key)
                    continue;

                filteredCreds.Add((membership.Key, membership.Value.Method, membership.Value.Password));
            }

            var replyBuilder = new StringBuilder();
            replyBuilder.AppendLine("```");
            replyBuilder.AppendLine($"{"Credentials",-16}{filteredCreds.Count}");

            if (filteredCreds.Count > 0)
            {
                var maxGroupNameLength = filteredCreds.Max(x => x.group.Length);
                var maxMethodLength = filteredCreds.Max(x => x.method.Length);
                var maxPasswordLength = filteredCreds.Max(x => x.password.Length);

                var groupNameFieldWidth = maxGroupNameLength > 5 ? maxGroupNameLength + 2 : 7;
                var methodFieldWidth = maxMethodLength > 6 ? maxMethodLength + 2 : 8;
                var passwordFieldWidth = maxPasswordLength > 8 ? maxPasswordLength + 2 : 10;

                replyBuilder.AppendLine();
                replyBuilder.AppendTableBorder(groupNameFieldWidth, methodFieldWidth, passwordFieldWidth);
                replyBuilder.AppendLine($"|{"Group".PadRight(groupNameFieldWidth)}|{"Method".PadRight(methodFieldWidth)}|{"Password".PadRight(passwordFieldWidth)}|");
                replyBuilder.AppendTableBorder(groupNameFieldWidth, methodFieldWidth, passwordFieldWidth);

                foreach (var (group, method, password) in filteredCreds)
                {
                    replyBuilder.AppendLine($"|{ChatHelper.EscapeMarkdownV2CodeBlock(group).PadRight(groupNameFieldWidth)}|{ChatHelper.EscapeMarkdownV2CodeBlock(method).PadRight(methodFieldWidth)}|{ChatHelper.EscapeMarkdownV2CodeBlock(password).PadRight(passwordFieldWidth)}|");
                }

                replyBuilder.AppendTableBorder(groupNameFieldWidth, methodFieldWidth, passwordFieldWidth);
            }

            replyBuilder.AppendLine("```");
            replyMarkdownV2 = replyBuilder.ToString();
            result = "success";
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
