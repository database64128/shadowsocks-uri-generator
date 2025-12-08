using Microsoft.Extensions.Options;
using ShadowsocksUriGenerator.Chatbot.Telegram;
using ShadowsocksUriGenerator.Services;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;

namespace ShadowsocksUriGenerator.Server;

public sealed partial class TelegramBotWebhookService(
    DataService dataService,
    ILogger<TelegramBotWebhookService> logger,
    IOptions<TelegramBotWebhookOptions> options,
    IHttpClientFactory httpClientFactory) : BotService(dataService, logger, httpClientFactory.CreateClient())
{
    private readonly TelegramBotWebhookOptions _options = options.Value;

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_options.Url))
        {
            return;
        }

        (TelegramBotClient bot, CancellationTokenSource _) = await StartBotAsync(cancellationToken);

        // Enable webhook.
        while (true)
        {
            try
            {
                await bot.SetWebhook(
                    _options.Url,
                    allowedUpdates: [UpdateType.Message],
                    secretToken: _options.SecretToken,
                    cancellationToken: cancellationToken);
                break;
            }
            catch (RequestException ex)
            {
                logger.LogWarning(ex, "Failed to enable webhook, retrying in 30 seconds");
                await Task.Delay(s_startupRetryInterval, cancellationToken);
            }
        }
    }
}
