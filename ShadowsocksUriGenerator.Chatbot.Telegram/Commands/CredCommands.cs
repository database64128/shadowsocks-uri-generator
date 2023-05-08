using ShadowsocksUriGenerator.Chatbot.Telegram.Utils;
using ShadowsocksUriGenerator.CLI.Utils;
using ShadowsocksUriGenerator.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ShadowsocksUriGenerator.Chatbot.Telegram.Commands
{
    public static class CredCommands
    {
        public static async Task GetSsLinksAsync(ITelegramBotClient botClient, Message message, string? argument, CancellationToken cancellationToken = default)
        {
            Console.Write($"{message.From} executed {message.Text} in {message.Chat.Type.ToString().ToLower()} chat {(string.IsNullOrEmpty(message.Chat.Title) ? string.Empty : $"{message.Chat.Title} ")}({message.Chat.Id}).");

            string reply;

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

            if (message.Chat.Type != ChatType.Private || message.From is null)
            {
                reply = "Retrieval of sensitive information is only allowed in private chats.";
                Console.WriteLine(" Response: not in private chat.");
            }
            else if (botConfig.ChatAssociations.TryGetValue(message.From.Id, out var userUuid) && DataHelper.TryLocateUserFromUuid(userUuid, users, out var userEntry))
            {
                var user = userEntry.Value.Value;
                var (loadedNodes, loadNodesErrMsg) = await Nodes.LoadNodesAsync(cancellationToken);
                if (loadNodesErrMsg is not null)
                {
                    Console.WriteLine();
                    Console.WriteLine(loadNodesErrMsg);
                    return;
                }
                using var nodes = loadedNodes;

                var uris = string.IsNullOrEmpty(argument) ? user.GetSSUris(users, nodes) : user.GetSSUris(users, nodes, argument);
                if (uris.Any())
                {
                    var replyBuilder = new StringBuilder();

                    foreach (var uri in uris)
                        replyBuilder.AppendLine($"{uri.AbsoluteUri}");

                    reply = replyBuilder.ToString();
                    Console.WriteLine(" Response: successful query.");
                }
                else
                {
                    reply = "No links are available for you.";
                    Console.WriteLine(" Response: successful query.");
                }
            }
            else
            {
                reply = "You must link your Telegram account to your user first.";
                Console.WriteLine(" Response: user not linked.");
            }

            await botClient.SendPossiblyLongTextMessageAsync(message.Chat.Id,
                                                             reply,
                                                             disableWebPagePreview: true,
                                                             replyToMessageId: message.MessageId,
                                                             cancellationToken: cancellationToken);
        }

        public static async Task GetOnlineConfigLinksAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken = default)
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

            if (message.Chat.Type != ChatType.Private || message.From is null)
            {
                replyMarkdownV2 = @"Retrieval of sensitive information is only allowed in private chats\.";
                Console.WriteLine(" Response: not in private chat.");
            }
            else if (botConfig.ChatAssociations.TryGetValue(message.From.Id, out var userUuid) && DataHelper.TryLocateUserFromUuid(userUuid, users, out var userEntry))
            {
                var (settings, loadSettingsErrMsg) = await Settings.LoadSettingsAsync(cancellationToken);
                if (loadSettingsErrMsg is not null)
                {
                    Console.WriteLine();
                    Console.WriteLine(loadSettingsErrMsg);
                    return;
                }

                var user = userEntry.Value.Value;

                var replyBuilder = new StringBuilder();

                var printApiLinks = !string.IsNullOrEmpty(settings.ApiServerBaseUrl) && !string.IsNullOrEmpty(settings.ApiServerSecretPath);
                var printStaticLinks = !string.IsNullOrEmpty(settings.OnlineConfigDeliveryRootUri);

                if (printApiLinks)
                {
                    var oocv1ApiToken = new OpenOnlineConfig.v1.OOCv1ApiToken(1, settings.ApiServerBaseUrl, settings.ApiServerSecretPath, user.Uuid, null);
                    var oocv1ApiTokenString = JsonSerializer.Serialize(oocv1ApiToken, OpenOnlineConfig.Utils.JsonHelper.camelCaseMinifiedJsonSerializerOptions);

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
                Console.WriteLine(" Response: successful query.");
            }
            else
            {
                replyMarkdownV2 = @"You must link your Telegram account to your user first\.";
                Console.WriteLine(" Response: user not linked.");
            }

            await botClient.SendPossiblyLongTextMessageAsync(message.Chat.Id,
                                                             replyMarkdownV2,
                                                             parseMode: ParseMode.MarkdownV2,
                                                             disableWebPagePreview: true,
                                                             replyToMessageId: message.MessageId,
                                                             cancellationToken: cancellationToken);
        }

        public static async Task GetCredentialsAsync(ITelegramBotClient botClient, Message message, string? argument, CancellationToken cancellationToken = default)
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

            if (message.Chat.Type != ChatType.Private || message.From is null)
            {
                replyMarkdownV2 = @"Retrieval of sensitive information is only allowed in private chats\.";
                Console.WriteLine(" Response: not in private chat.");
            }
            else if (botConfig.ChatAssociations.TryGetValue(message.From.Id, out var userUuid) && DataHelper.TryLocateUserFromUuid(userUuid, users, out var userEntry))
            {
                var username = userEntry.Value.Key;
                var user = userEntry.Value.Value;

                List<(string group, string method, string password)> filteredCreds = new();

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
                Console.WriteLine(" Response: successful query.");
            }
            else
            {
                replyMarkdownV2 = @"You must link your Telegram account to your user first\.";
                Console.WriteLine(" Response: user not linked.");
            }

            await botClient.SendPossiblyLongTextMessageAsync(message.Chat.Id,
                                                             replyMarkdownV2,
                                                             parseMode: ParseMode.MarkdownV2,
                                                             replyToMessageId: message.MessageId,
                                                             cancellationToken: cancellationToken);
        }
    }
}
