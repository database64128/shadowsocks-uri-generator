using ShadowsocksUriGenerator.Services;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ShadowsocksUriGenerator.Chatbot.Telegram.LongPolling;

public sealed partial class LongPollingBotService(DataService dataService, ILogger<LongPollingBotService> logger, IHttpClientFactory httpClientFactory) : BotService(dataService, logger, httpClientFactory.CreateClient())
{
    private Task? _pollUpdatesTask;

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        (TelegramBotClient bot, CancellationTokenSource cts) = await StartBotAsync(cancellationToken);

        // Disable webhook.
        while (true)
        {
            try
            {
                await bot.SetWebhook("", cancellationToken: cancellationToken);
                break;
            }
            catch (RequestException ex)
            {
                logger.LogWarning(ex, "Failed to disable webhook, retrying in 30 seconds");
                await Task.Delay(s_startupRetryInterval, cancellationToken);
            }
        }

        _pollUpdatesTask = PollUpdatesAsync(bot, cts.Token);
    }

    private async Task PollUpdatesAsync(ITelegramBotClient botClient, CancellationToken cancellationToken = default)
    {
        int? offset = null;

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                Update[] updates = await botClient.GetUpdates(offset, allowedUpdates: [UpdateType.Message], cancellationToken: cancellationToken);

                if (updates.Length > 0)
                {
                    offset = updates[^1].Id + 1;

                    foreach (Update update in updates)
                    {
                        await UpdateWriter.WriteAsync(update, cancellationToken);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                LogFailedToGetUpdates(ex);

                try
                {
                    await Task.Delay(s_getUpdatesRetryInterval, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    return;
                }
            }
        }
    }

    private static readonly TimeSpan s_getUpdatesRetryInterval = TimeSpan.FromSeconds(5);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to get updates")]
    private partial void LogFailedToGetUpdates(Exception ex);

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (Cts is null)
        {
            return;
        }

        try
        {
            await base.StopAsync(cancellationToken);
        }
        finally
        {
            if (_pollUpdatesTask is not null)
            {
                await _pollUpdatesTask.WaitAsync(cancellationToken).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
            }
        }
    }
}
