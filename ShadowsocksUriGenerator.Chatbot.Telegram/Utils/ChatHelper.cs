using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace ShadowsocksUriGenerator.Chatbot.Telegram.Utils
{
    public static class ChatHelper
    {
        /// <summary>
        /// Sends a possibly long text message.
        /// Short messages are sent as text messages.
        /// Long messages are sent as text files.
        /// </summary>
        /// <inheritdoc cref="ITelegramBotClient.SendTextMessageAsync(ChatId, string, ParseMode, IEnumerable{MessageEntity}, bool, bool, int, bool, IReplyMarkup, CancellationToken)"/>
        public static Task SendPossiblyLongTextMessageAsync(
            this ITelegramBotClient botClient,
            ChatId chatId,
            string text,
            ParseMode parseMode = ParseMode.Default,
            IEnumerable<MessageEntity>? entities = null,
            bool disableWebPagePreview = false,
            bool disableNotification = false,
            int replyToMessageId = 0,
            bool allowSendingWithoutReply = false,
            IReplyMarkup? replyMarkup = null,
            CancellationToken cancellationToken = default)
        {
            if (text.Length <= 4096)
            {
                return botClient.SendTextMessageAsync(chatId,
                                                      text,
                                                      parseMode,
                                                      entities,
                                                      disableWebPagePreview,
                                                      disableNotification,
                                                      replyToMessageId,
                                                      allowSendingWithoutReply,
                                                      replyMarkup,
                                                      cancellationToken);
            }
            else // too large, send as file
            {
                var filename = parseMode switch
                {
                    ParseMode.Default => "long-message.txt",
                    ParseMode.Markdown => "long-message.md",
                    ParseMode.Html => "long-message.html",
                    ParseMode.MarkdownV2 => "long-message.md",
                    _ => "long-message",
                };

                return botClient.SendTextFileFromStringAsync(chatId,
                                                             filename,
                                                             text,
                                                             null,
                                                             null,
                                                             parseMode,
                                                             entities,
                                                             false,
                                                             disableNotification,
                                                             replyToMessageId,
                                                             false,
                                                             replyMarkup,
                                                             cancellationToken);
            }
        }

        /// <summary>
        /// Sends a string as a text file.
        /// </summary>
        /// <param name="filename">Filename.</param>
        /// <param name="text">The string to send.</param>
        /// <inheritdoc cref="ITelegramBotClient.SendDocumentAsync(ChatId, InputOnlineFile, InputMedia, string, ParseMode, IEnumerable{MessageEntity}, bool, bool, int, bool, IReplyMarkup, CancellationToken)"/>
        public static async Task<Message> SendTextFileFromStringAsync(
            this ITelegramBotClient botClient,
            ChatId chatId,
            string filename,
            string text,
            InputMedia? thumb = null,
            string? caption = null,
            ParseMode parseMode = ParseMode.Default,
            IEnumerable<MessageEntity>? captionEntities = null,
            bool disableContentTypeDetection = false,
            bool disableNotification = false,
            int replyToMessageId = 0,
            bool allowSendingWithoutReply = false,
            IReplyMarkup? replyMarkup = null,
            CancellationToken cancellationToken = default)
        {
            await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(text));
            return await botClient.SendDocumentAsync(chatId,
                                                     new(stream, filename),
                                                     thumb,
                                                     caption,
                                                     parseMode,
                                                     captionEntities,
                                                     disableContentTypeDetection,
                                                     disableNotification,
                                                     replyToMessageId,
                                                     allowSendingWithoutReply,
                                                     replyMarkup,
                                                     cancellationToken);
        }

        /// <summary>
        /// Escapes the plaintext per the MarkdownV2 requirements.
        /// This method does not handle markdown entities.
        /// </summary>
        /// <param name="plaintext">The plaintext to be escaped.</param>
        /// <returns>An escaped string.</returns>
        public static string EscapeMarkdownV2Plaintext(string plaintext)
            => Regex.Replace(plaintext, @"[_*[\]()~`>#+\-=|{}.!]", @"\$&");

        /// <summary>
        /// Escapes the code per the MarkdownV2 requirements.
        /// </summary>
        /// <param name="code">The code to be escaped.</param>
        /// <returns>The escaped code.</returns>
        public static string EscapeMarkdownV2CodeBlock(string code)
            => Regex.Replace(code, @"[`\\]", @"\$&");

        /// <summary>
        /// Parses a text message into a command and an argument if applicable.
        /// </summary>
        /// <param name="text">The text message to process.</param>
        /// <param name="botUsername">The bot username. Does not start with '@'.</param>
        /// <returns>
        /// A ValueTuple of the parsed command and argument.
        /// Command is null if the text message is not a command to the bot.
        /// Argument can be null.
        /// </returns>
        public static (string? command, string? argument) ParseMessageIntoCommandAndArgument(string text, string botUsername)
        {
            // Empty message
            if (string.IsNullOrWhiteSpace(text))
                return (null, null);

            // Not a command
            if (!text.StartsWith('/') || text.Length < 2)
                return (null, null);

            // Remove the leading '/'
            text = text[1..];

            // Split command and argument
            var parsedText = text.Split(' ', 2);
            string command;
            string? argument = null;
            switch (parsedText.Length)
            {
                case <= 0:
                    return (null, null);
                case 2:
                    argument = parsedText[1];
                    goto default;
                default:
                    command = parsedText[0];
                    break;
            }

            // Verify and remove trailing '@bot' from command
            var atSignIndex = command.IndexOf('@');
            if (atSignIndex != -1)
            {
                var atUsername = command[atSignIndex..];
                if (atUsername != $"@{botUsername}")
                    return (null, null);

                command = command[..atSignIndex];
            }

            // Trim quotes and spaces from argument
            if (argument is not null)
            {
                argument = argument.Trim();
                argument = argument.Trim('\'', '"');
            }

            return (command, argument);
        }
    }
}
