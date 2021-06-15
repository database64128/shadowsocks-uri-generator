using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;

namespace ShadowsocksUriGenerator.Chatbot.Telegram.CLI
{
    public static class BotRunner
    {
        public static async Task<int> RunBot(string? botToken, CancellationToken cancellationToken = default)
        {
            var (botConfig, loadBotConfigErrMsg) = await BotConfig.LoadBotConfigAsync(cancellationToken);
            if (loadBotConfigErrMsg is not null)
            {
                Console.WriteLine(loadBotConfigErrMsg);
                return 1;
            }

            // Priority: commandline option > environment variable > config file
            if (string.IsNullOrEmpty(botToken))
                botToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
            if (string.IsNullOrEmpty(botToken))
                botToken = botConfig.BotToken;
            if (string.IsNullOrEmpty(botToken))
            {
                Console.WriteLine("Please provide a bot token with command line option `--bot-token`, environment variable `TELEGRAM_BOT_TOKEN`, or in the config file.");
                return -1;
            }

            try
            {
                var bot = new TelegramBotClient(botToken);
                Console.WriteLine("Created Telegram bot instance with API token.");

                var me = await bot.GetMeAsync(cancellationToken);
                if (string.IsNullOrEmpty(me.Username))
                    throw new Exception("Error: bot username is null or empty.");

                var updateHandler = new UpdateHandler(me.Username, botConfig);
                await bot.SetMyCommandsAsync(UpdateHandler.BotCommands, cancellationToken);
                Console.WriteLine($"Registered {UpdateHandler.BotCommands.Length} bot commands.");
                Console.WriteLine($"Started Telegram bot: @{me.Username} ({me.Id}).");

                var updateReceiver = new QueuedUpdateReceiver(bot);
                updateReceiver.StartReceiving(null, UpdateHandler.HandleErrorAsync, cancellationToken);

                var updateStream = updateReceiver.YieldUpdatesAsync();
                await updateHandler.HandleUpdateStreamAsync(bot, updateStream, cancellationToken);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Invalid access token: {ex.Message}");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"A network error occurred: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return 0;
        }
    }
}
