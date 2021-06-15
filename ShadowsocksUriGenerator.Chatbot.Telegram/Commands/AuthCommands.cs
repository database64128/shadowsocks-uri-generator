using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ShadowsocksUriGenerator.Chatbot.Telegram.Commands
{
    public static class AuthCommands
    {
        public static Task StartAsync(ITelegramBotClient botClient, Message message, BotConfig config, CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"{message.From} executed {message.Text} in {message.Chat.Type.ToString().ToLower()} chat {(string.IsNullOrEmpty(message.Chat.Title) ? string.Empty : $"{message.Chat.Title} ")}({message.Chat.Id}).");

            var serviceName = string.IsNullOrEmpty(config.ServiceName) ? "with us" : config.ServiceName;

            var replyMarkdownV2 = $@"🧑‍✈️ Good evening\! Thank you for flying {serviceName}\.

✈️ To get your boarding pass, please use `/link <UUID>` to link your Telegram account to your user\.";

            return botClient.SendTextMessageAsync(message.Chat.Id,
                                                  replyMarkdownV2,
                                                  ParseMode.MarkdownV2,
                                                  replyToMessageId: message.MessageId,
                                                  cancellationToken: cancellationToken);
        }

        public static async Task LinkAsync(ITelegramBotClient botClient, Message message, string? argument, CancellationToken cancellationToken = default)
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

            if (!botConfig.AllowChatAssociation)
            {
                reply = "The admin has disabled Telegram association.";
                Console.WriteLine(" Response: command disabled.");
            }
            else if (message.Chat.Type != ChatType.Private)
            {
                reply = "Associations must be made in a private chat.";
                Console.WriteLine(" Response: not in private chat.");
            }
            else if (string.IsNullOrEmpty(argument))
            {
                reply = "Please provide your user UUID as the command argument. You may find your user UUID from the JSON filename in your SIP008 delivery link.";
                Console.WriteLine(" Response: missing argument.");
            }
            else
            {
                var (users, loadUsersErrMsg) = await Users.LoadUsersAsync(cancellationToken);
                if (loadUsersErrMsg is not null)
                {
                    Console.WriteLine();
                    Console.WriteLine(loadUsersErrMsg);
                    return;
                }

                var userSearchResult = users.UserDict.Where(x => string.Equals(x.Value.Uuid, argument, StringComparison.OrdinalIgnoreCase));
                KeyValuePair<string, User>? matchedUser = userSearchResult.Any() ? userSearchResult.First() : null;
                if (matchedUser is null)
                {
                    reply = "User not found.";
                    Console.WriteLine(" Response: user not found.");
                }
                else if (botConfig.ChatAssociations.TryGetValue(message.From.Id, out var userUuid))
                {
                    if (string.Equals(userUuid, argument, StringComparison.OrdinalIgnoreCase))
                    {
                        reply = $"You are already linked to {matchedUser.Value.Key}.";
                        Console.WriteLine(" Response: already linked.");
                    }
                    else
                    {
                        reply = $"You are already linked to another user with UUID {userUuid}.";
                        Console.WriteLine(" Response: already linked to another user.");
                    }
                }
                else
                {
                    botConfig.ChatAssociations.Add(message.From.Id, matchedUser.Value.Value.Uuid);

                    reply = $"Successfully linked your Telegram account to {matchedUser.Value.Key}.";
                    Console.WriteLine(" Response: success.");

                    var saveBotConfigErrMsg = await BotConfig.SaveBotConfigAsync(botConfig, cancellationToken);
                    if (saveBotConfigErrMsg is not null)
                    {
                        Console.WriteLine(loadBotConfigErrMsg);
                        return;
                    }
                }
            }

            await botClient.SendTextMessageAsync(message.Chat.Id,
                                                 reply,
                                                 replyToMessageId: message.MessageId,
                                                 cancellationToken: cancellationToken);
        }

        public static async Task UnlinkAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken = default)
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

            if (!botConfig.AllowChatAssociation)
            {
                reply = "The admin has disabled Telegram association.";
                Console.WriteLine(" Response: command disabled.");
            }
            else if (message.Chat.Type != ChatType.Private)
            {
                reply = "Associations must be made in a private chat.";
                Console.WriteLine(" Response: not in private chat.");
            }
            else if (botConfig.ChatAssociations.Remove(message.From.Id, out var userUuid))
            {
                reply = $"Successfully unlinked your Telegram account from {userUuid}.";
                Console.WriteLine(" Response: success.");

                var saveBotConfigErrMsg = await BotConfig.SaveBotConfigAsync(botConfig, cancellationToken);
                if (saveBotConfigErrMsg is not null)
                {
                    Console.WriteLine(loadBotConfigErrMsg);
                    return;
                }
            }
            else
            {
                reply = "You are not linked to any user.";
                Console.WriteLine(" Response: not linked");
            }

            await botClient.SendTextMessageAsync(message.Chat.Id,
                                                 reply,
                                                 replyToMessageId: message.MessageId,
                                                 cancellationToken: cancellationToken);
        }
    }
}
