using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ShadowsocksUriGenerator.Chatbot.Telegram
{
    class Program
    {
        static Task<int> Main(string[] args)
        {
            var configGetCommand = new Command("get", "Get and print bot config.")
            {
                Handler = CommandHandler.Create(
                    async () =>
                    {
                        var botConfig = await BotConfig.LoadBotConfigAsync();

                        Utilities.PrintTableBorder(28, 50);
                        Console.WriteLine($"|{"Key",-28}|{"Value",50}|");
                        Utilities.PrintTableBorder(28, 50);

                        Console.WriteLine($"|{"Version",-28}|{botConfig.Version,50}|");
                        Console.WriteLine($"|{"BotToken",-28}|{botConfig.BotToken,50}|");
                        Console.WriteLine($"|{"UsersCanSeeAllUsers",-28}|{botConfig.UsersCanSeeAllUsers,50}|");
                        Console.WriteLine($"|{"UsersCanSeeAllGroups",-28}|{botConfig.UsersCanSeeAllGroups,50}|");
                        Console.WriteLine($"|{"UsersCanSeeGroupDataUsage",-28}|{botConfig.UsersCanSeeGroupDataUsage,50}|");
                        Console.WriteLine($"|{"AllowChatAssociation",-28}|{botConfig.AllowChatAssociation,50}|");

                        Utilities.PrintTableBorder(28, 50);
                    }),
            };

            var configSetCommand = new Command("set", "Change bot config.")
            {
                Handler = CommandHandler.Create(
                    async (string? botToken, bool? usersCanSeeAllUsers, bool? usersCanSeeAllGroups, bool? usersCanSeeGroupDataUsage, bool? allowChatAssociation) =>
                    {
                        var botConfig = await BotConfig.LoadBotConfigAsync();

                        if (!string.IsNullOrEmpty(botToken))
                            botConfig.BotToken = botToken;
                        if (usersCanSeeAllUsers is bool canSeeUsers)
                            botConfig.UsersCanSeeAllUsers = canSeeUsers;
                        if (usersCanSeeAllGroups is bool canSeeGroups)
                            botConfig.UsersCanSeeAllGroups = canSeeGroups;
                        if (usersCanSeeGroupDataUsage is bool canSeeGroupDataUsage)
                            botConfig.UsersCanSeeGroupDataUsage = canSeeGroupDataUsage;
                        if (allowChatAssociation is bool allowLinking)
                            botConfig.AllowChatAssociation = allowLinking;

                        await BotConfig.SaveBotConfigAsync(botConfig);
                    }),
            };
            configSetCommand.AddOption(new Option<string?>("--bot-token", "The Telegram bot token."));
            configSetCommand.AddOption(new Option<bool?>("--users-can-see-all-users", "Whether anyone is allowed to see every registered user."));
            configSetCommand.AddOption(new Option<bool?>("--users-can-see-all-groups", "Whether anyone is allowed to see every group."));
            configSetCommand.AddOption(new Option<bool?>("--users-can-see-group-data-usage", "Whether users are allowed to query group data usage metrics."));
            configSetCommand.AddOption(new Option<bool?>("--allow-chat-association", "Whether Telegram association through /link in chat is allowed."));

            var configCommand = new Command("config", "Print or change bot config.")
            {
                configGetCommand,
                configSetCommand,
            };

            var rootCommand = new RootCommand("A Telegram bot for user interactions with Shadowsocks URI Generator.")
            {
                configCommand,
            };
            rootCommand.AddOption(new Option<string?>("--bot-token", "The Telegram bot token."));
            rootCommand.Handler = CommandHandler.Create(
                async (string? botToken, CancellationToken cancellationToken) =>
                {
                    var botConfig = await BotConfig.LoadBotConfigAsync();

                    // Priority: commandline option > environment variable > config file
                    if (string.IsNullOrEmpty(botToken))
                        botToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
                    if (string.IsNullOrEmpty(botToken))
                        botToken = botConfig.BotToken;
                    if (string.IsNullOrEmpty(botToken))
                    {
                        Console.WriteLine("No valid bot token is provided.");
                        return;
                    }

                    try
                    {
                        var bot = new TelegramBotClient(botToken);
                        var me = await bot.GetMeAsync(cancellationToken);
                        Console.WriteLine($"Started Telegram bot: @{me.Username} ({me.Id})");
                        await bot.ReceiveAsync(new DefaultUpdateHandler(HandleUpdateAsync, HandleErrorAsync), cancellationToken);
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
                });

            Console.OutputEncoding = Encoding.UTF8;
            return rootCommand.InvokeAsync(args);
        }

        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken = default)
        {
            try
            {
                if (update.Type == UpdateType.Message)
                    await BotCommandHandler.Handle(botClient, update.Message, cancellationToken);
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(botClient, ex, cancellationToken);
            }
        }

        public static Task HandleErrorAsync(ITelegramBotClient botClient, Exception ex, CancellationToken cancellationToken = default)
        {
            var errorMessage = ex switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error: [{apiRequestException.ErrorCode}] {apiRequestException.Message}",
                _ => ex.ToString()
            };

            Console.WriteLine(errorMessage);
            return Task.CompletedTask;
        }
    }
}
