using Microsoft.Extensions.Logging;
using ShadowsocksUriGenerator.Chatbot.Telegram.Commands;
using ShadowsocksUriGenerator.Chatbot.Telegram.Utils;
using ShadowsocksUriGenerator.Services;
using System.Threading.Channels;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ShadowsocksUriGenerator.Chatbot.Telegram;

public sealed partial class UpdateHandler(string botUsername, BotConfig botConfig, DataService dataService, ILogger logger, ITelegramBotClient botClient)
{
    /// <summary>
    /// Gets public bot commands that are available to all types of chats.
    /// </summary>
    public static BotCommand[] BotCommandsPublic =>
    [
        new() { Command = "start", Description = "Cleared for takeoff!", },
        new() { Command = "list_users", Description = "List all users", },
        new() { Command = "list_nodes", Description = "List all nodes or nodes from the specified group", },
        new() { Command = "list_groups", Description = "List all groups", },
        new() { Command = "list_group_members", Description = "List members of the specified group", },
        new() { Command = "list_owned_nodes", Description = "List nodes owned by you or the specified user", },
        new() { Command = "list_owned_groups", Description = "List groups owned by you or the specified user", },
        new() { Command = "get_user_data_usage", Description = "Get data usage statistics of the associated user or the specified user", },
        new() { Command = "get_user_data_limit", Description = "Get data limit settings of the associated user or the specified user", },
        new() { Command = "get_group_data_usage", Description = "Get data usage statistics of groups you own or the specified group", },
        new() { Command = "get_group_data_limit", Description = "Get data limit settings of the specified group", },
        new() { Command = "report", Description = "Generate server usage report", },
        new() { Command = "report_csv", Description = "Generate server usage report in CSV format", },
    ];

    /// <summary>
    /// Gets private bot commands that are only available to private chats.
    /// </summary>
    public static BotCommand[] BotCommandsPrivate =>
    [
        new() { Command = "link", Description = "Link your Telegram account to a user", },
        new() { Command = "unlink", Description = "Unlink your Telegram account from the user", },
        new() { Command = "get_ss_links", Description = "Get your ss:// links to all servers or servers from the specified group", },
        new() { Command = "get_online_config_links", Description = "Get online config API URLs and tokens", },
        new() { Command = "get_credentials", Description = "Get your credentials to all servers or servers from the specified group", },
    ];

    public async Task RunAsync(ChannelReader<Update> reader, CancellationToken cancellationToken = default)
    {
        await foreach (Update update in reader.ReadAllAsync(cancellationToken))
        {
            LogReceivedUpdate(update.Id);

            if (update.Message is Message message)
            {
                await HandleCommandAsync(botClient, message, cancellationToken);
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Trace, Message = "Received update with ID {UpdateId}")]
    private partial void LogReceivedUpdate(int updateId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to handle update")]
    private partial void LogFailedToHandleUpdate(Exception ex);

    private async Task HandleCommandAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken = default)
    {
        var (command, argument) = ChatHelper.ParseMessageIntoCommandAndArgument(message.Text, botUsername);

        string result = command switch
        {
            "start" => await AuthCommands.StartAsync(botClient, message, botConfig, cancellationToken),
            "link" => await AuthCommands.LinkAsync(botClient, message, argument, botConfig, dataService, logger, cancellationToken),
            "unlink" => await AuthCommands.UnlinkAsync(botClient, message, botConfig, logger, cancellationToken),
            "list_users" => await ListCommands.ListUsersAsync(botClient, message, botConfig, dataService, cancellationToken),
            "list_nodes" => await ListCommands.ListNodesAsync(botClient, message, argument, botConfig, dataService, cancellationToken),
            "list_groups" => await ListCommands.ListGroupsAsync(botClient, message, botConfig, dataService, cancellationToken),
            "list_group_members" => await ListCommands.ListGroupMembersAsync(botClient, message, argument, botConfig, dataService, cancellationToken),
            "list_owned_nodes" => await ListCommands.ListOwnedNodesAsync(botClient, message, argument, botConfig, dataService, cancellationToken),
            "list_owned_groups" => await ListCommands.ListOwnedGroupsAsync(botClient, message, argument, botConfig, dataService, cancellationToken),
            "get_user_data_usage" => await DataCommands.GetUserDataUsageAsync(botClient, message, argument, botConfig, dataService, cancellationToken),
            "get_user_data_limit" => await DataCommands.GetUserDataLimitAsync(botClient, message, argument, botConfig, dataService, cancellationToken),
            "get_group_data_usage" => await DataCommands.GetGroupDataUsageAsync(botClient, message, argument, botConfig, dataService, cancellationToken),
            "get_group_data_limit" => await DataCommands.GetGroupDataLimitAsync(botClient, message, argument, botConfig, dataService, cancellationToken),
            "get_ss_links" => await CredCommands.GetSsLinksAsync(botClient, message, argument, botConfig, dataService, cancellationToken),
            "get_online_config_links" => await CredCommands.GetOnlineConfigLinksAsync(botClient, message, botConfig, dataService, cancellationToken),
            "get_credentials" => await CredCommands.GetCredentialsAsync(botClient, message, argument, botConfig, dataService, cancellationToken),
            "report" => await ReportCommand.GenerateReportAsync(botClient, message, argument, botConfig, dataService, cancellationToken),
            "report_csv" => await ReportCommand.GenerateReportAsync(botClient, message, "csv", botConfig, dataService, cancellationToken),
            _ => "unknown command",
        };

        LogHandledCommand(message.Text, message.From, message.Chat.Type, message.Chat.Title, message.Chat.Id, result);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Handled command {Text} from {From} in {ChatType} chat {ChatTitle} ({ChatId}): {Result}")]
    private partial void LogHandledCommand(string? text, User? from, ChatType chatType, string? chatTitle, long chatId, string result);
}
