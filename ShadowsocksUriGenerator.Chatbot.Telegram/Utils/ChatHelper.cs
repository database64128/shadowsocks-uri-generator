using System.Text;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace ShadowsocksUriGenerator.Chatbot.Telegram.Utils
{
    public static partial class ChatHelper
    {
        /// <summary>
        /// Sends a possibly long text message.
        /// Short messages are sent as text messages.
        /// Long messages are sent as text files.
        /// </summary>
        /// <inheritdoc cref="TelegramBotClientExtensions.SendMessage"/>
        public static Task<Message> SendPossiblyLongTextMessageAsync(
            this ITelegramBotClient botClient,
            ChatId chatId,
            string text,
            ParseMode parseMode = default,
            ReplyParameters? replyParameters = default,
            ReplyMarkup? replyMarkup = default,
            LinkPreviewOptions? linkPreviewOptions = default,
            int? messageThreadId = default,
            IEnumerable<MessageEntity>? entities = default,
            bool disableNotification = default,
            bool protectContent = default,
            string? messageEffectId = default,
            string? businessConnectionId = default,
            bool allowPaidBroadcast = default,
            CancellationToken cancellationToken = default)
            => text.Length switch
            {
                <= 4096 => botClient.SendMessage(
                    chatId,
                    text,
                    parseMode,
                    replyParameters,
                    replyMarkup,
                    linkPreviewOptions,
                    messageThreadId,
                    entities,
                    disableNotification,
                    protectContent,
                    messageEffectId,
                    businessConnectionId,
                    allowPaidBroadcast,
                    cancellationToken),
                _ => botClient.SendTextFileFromStringAsync(
                    chatId,
                    parseMode switch
                    {
                        ParseMode.Markdown => "long-message.md",
                        ParseMode.Html => "long-message.html",
                        ParseMode.MarkdownV2 => "long-message.md",
                        _ => "long-message.txt",
                    },
                    text,
                    parseMode: parseMode,
                    replyParameters: replyParameters,
                    replyMarkup: replyMarkup,
                    messageThreadId: messageThreadId,
                    disableNotification: disableNotification,
                    protectContent: protectContent,
                    messageEffectId: messageEffectId,
                    businessConnectionId: businessConnectionId,
                    allowPaidBroadcast: allowPaidBroadcast,
                    cancellationToken: cancellationToken)
            };

        /// <summary>
        /// Sends a string as a text file.
        /// </summary>
        /// <param name="filename">Filename.</param>
        /// <param name="text">The string to send.</param>
        /// <inheritdoc cref="TelegramBotClientExtensions.SendDocument"/>
        public static async Task<Message> SendTextFileFromStringAsync(
            this ITelegramBotClient botClient,
            ChatId chatId,
            string filename,
            string text,
            string? caption = default,
            ParseMode parseMode = default,
            ReplyParameters? replyParameters = default,
            ReplyMarkup? replyMarkup = default,
            InputFile? thumbnail = default,
            int? messageThreadId = default,
            IEnumerable<MessageEntity>? captionEntities = default,
            bool disableContentTypeDetection = default,
            bool disableNotification = default,
            bool protectContent = default,
            string? messageEffectId = default,
            string? businessConnectionId = default,
            bool allowPaidBroadcast = default,
            CancellationToken cancellationToken = default)
        {
            await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(text));
            return await botClient.SendDocument(
                chatId,
                InputFile.FromStream(stream, filename),
                caption,
                parseMode,
                replyParameters,
                replyMarkup,
                thumbnail,
                messageThreadId,
                captionEntities,
                disableContentTypeDetection,
                disableNotification,
                protectContent,
                messageEffectId,
                businessConnectionId,
                allowPaidBroadcast,
                cancellationToken);
        }

        [GeneratedRegex("[_*[\\]()~`>#+\\-=|{}.!]")]
        private static partial Regex EscapeMarkdownV2PlaintextRegex();

        [GeneratedRegex("[`\\\\]")]
        private static partial Regex EscapeMarkdownV2CodeBlockRegex();

        /// <summary>
        /// Escapes the plaintext per the MarkdownV2 requirements.
        /// This method does not handle markdown entities.
        /// </summary>
        /// <param name="plaintext">The plaintext to be escaped.</param>
        /// <returns>An escaped string.</returns>
        public static string EscapeMarkdownV2Plaintext(string plaintext)
            => EscapeMarkdownV2PlaintextRegex().Replace(plaintext, @"\$&");

        /// <summary>
        /// Escapes the code per the MarkdownV2 requirements.
        /// </summary>
        /// <param name="code">The code to be escaped.</param>
        /// <returns>The escaped code.</returns>
        public static string EscapeMarkdownV2CodeBlock(string code)
            => EscapeMarkdownV2CodeBlockRegex().Replace(code, @"\$&");

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
        public static (string? command, string? argument) ParseMessageIntoCommandAndArgument(ReadOnlySpan<char> text, string botUsername)
        {
            // Empty message
            if (text.IsEmpty)
                return (null, null);

            // Not a command
            if (text[0] != '/' || text.Length < 2)
                return (null, null);

            // Remove the leading '/'
            text = text[1..];

            // Split command and argument
            ReadOnlySpan<char> command, argument;
            var spacePos = text.IndexOf(' ');
            if (spacePos == -1)
            {
                command = text;
                argument = [];
            }
            else if (spacePos == text.Length - 1)
            {
                command = text[..spacePos];
                argument = [];
            }
            else
            {
                command = text[..spacePos];
                argument = text[(spacePos + 1)..];
            }

            // Verify and remove trailing '@bot' from command
            var atSignIndex = command.IndexOf('@');
            if (atSignIndex != -1)
            {
                if (atSignIndex != command.Length - 1)
                {
                    var atUsername = command[(atSignIndex + 1)..];
                    if (!atUsername.SequenceEqual(botUsername))
                    {
                        return (null, null);
                    }
                }

                command = command[..atSignIndex];
            }

            // Trim leading and trailing spaces from argument
            argument = argument.Trim();

            // Convert back to string
            string? commandString = null;
            string? argumentString = null;
            if (!command.IsEmpty)
            {
                commandString = command.ToString();

                if (!argument.IsEmpty)
                {
                    argumentString = argument.ToString();
                }
            }

            return (commandString, argumentString);
        }
    }
}
