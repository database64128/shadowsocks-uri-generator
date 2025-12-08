using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ShadowsocksUriGenerator.Services;
using System.Threading.Channels;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;

namespace ShadowsocksUriGenerator.Chatbot.Telegram;

public abstract partial class BotService : IHostedService
{
    private readonly DataService _dataService;
    private readonly ILogger<BotService> _logger;
    private readonly HttpClient _httpClient;
    private readonly ChannelReader<Update> _updateReader;

    public BotService(DataService dataService, ILogger<BotService> logger, HttpClient httpClient)
    {
        _dataService = dataService;
        _logger = logger;
        _httpClient = httpClient;
        Channel<Update> updateChannel = Channel.CreateBounded<Update>(new BoundedChannelOptions(64)
        {
            SingleReader = true,
        });
        _updateReader = updateChannel.Reader;
        UpdateWriter = updateChannel.Writer;
    }

    public ChannelWriter<Update> UpdateWriter { get; }

    protected CancellationTokenSource? Cts { get; private set; }
    private Task? _handleUpdatesTask;

    public virtual Task StartAsync(CancellationToken cancellationToken) => StartBotAsync(cancellationToken);

    protected async Task<(TelegramBotClient, CancellationTokenSource)> StartBotAsync(CancellationToken cancellationToken = default)
    {
        BotConfig config;

        try
        {
            config = await BotConfig.LoadAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to load config", ex);
        }

        // Priority: commandline option > environment variable > config file
        var botToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
        if (string.IsNullOrEmpty(botToken))
            botToken = config.BotToken;
        if (string.IsNullOrEmpty(botToken))
            throw new Exception("Please provide a bot token with environment variable `TELEGRAM_BOT_TOKEN`, or in the config file.");

        TelegramBotClientOptions options = new(botToken)
        {
            RetryCount = 7,
        };
        TelegramBotClient bot = new(options, _httpClient);
        User me;

        while (true)
        {
            try
            {
                me = await bot.GetMe(cancellationToken);
                break;
            }
            catch (RequestException ex)
            {
                _logger.LogWarning(ex, "Failed to get bot info, retrying in 30 seconds");
                await Task.Delay(s_startupRetryInterval, cancellationToken);
            }
        }

        if (string.IsNullOrEmpty(me.Username))
            throw new Exception("Bot username is null or empty.");

        UpdateHandler updateHandler = new(me.Username, config, _dataService, _logger, bot);

        while (true)
        {
            try
            {
                await bot.SetMyCommands(UpdateHandler.BotCommandsPublic, cancellationToken: cancellationToken);
                LogRegisteredCommandsAllChats(UpdateHandler.BotCommandsPublic.Length);
                break;
            }
            catch (RequestException ex)
            {
                _logger.LogWarning(ex, "Failed to register commands for all chats, retrying in 30 seconds");
                await Task.Delay(s_startupRetryInterval, cancellationToken);
            }
        }

        while (true)
        {
            try
            {
                BotCommand[] privateChatCommands = new BotCommand[UpdateHandler.BotCommandsPrivate.Length + UpdateHandler.BotCommandsPublic.Length];
                UpdateHandler.BotCommandsPrivate.CopyTo(privateChatCommands, 0);
                UpdateHandler.BotCommandsPublic.CopyTo(privateChatCommands, UpdateHandler.BotCommandsPrivate.Length);
                await bot.SetMyCommands(privateChatCommands, BotCommandScope.AllPrivateChats(), cancellationToken: cancellationToken);
                LogRegisteredCommandsPrivateChats(privateChatCommands.Length);
                break;
            }
            catch (RequestException ex)
            {
                _logger.LogWarning(ex, "Failed to register commands for private chats, retrying in 30 seconds");
                await Task.Delay(s_startupRetryInterval, cancellationToken);
            }
        }

        LogStartedBot(me.Username, me.Id);

        Cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _handleUpdatesTask = updateHandler.RunAsync(_updateReader, Cts.Token);

        return (bot, Cts);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Registered {CommandCount} bot commands for all chats")]
    private partial void LogRegisteredCommandsAllChats(int commandCount);

    [LoggerMessage(Level = LogLevel.Information, Message = "Registered {CommandCount} bot commands for private chats")]
    private partial void LogRegisteredCommandsPrivateChats(int commandCount);

    [LoggerMessage(Level = LogLevel.Information, Message = "Started Telegram bot: @{BotUsername} ({BotId})")]
    private partial void LogStartedBot(string botUsername, long botId);

    protected static readonly TimeSpan s_startupRetryInterval = TimeSpan.FromSeconds(30);

    public virtual async Task StopAsync(CancellationToken cancellationToken)
    {
        if (Cts is null)
        {
            return;
        }

        try
        {
            Cts.Cancel();
        }
        finally
        {
            if (_handleUpdatesTask is not null)
            {
                await _handleUpdatesTask.WaitAsync(cancellationToken).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
            }
        }
    }
}
