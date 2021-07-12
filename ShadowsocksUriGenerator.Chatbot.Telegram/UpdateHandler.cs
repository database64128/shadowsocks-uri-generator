using ShadowsocksUriGenerator.Chatbot.Telegram.Commands;
using ShadowsocksUriGenerator.Chatbot.Telegram.Utils;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ShadowsocksUriGenerator.Chatbot.Telegram
{
    public class UpdateHandler
    {
        public static BotCommand[] BotCommands => new BotCommand[]
        {
            new() { Command = "start", Description = "Cleared for takeoff!", },
            new() { Command = "link", Description = "Link your Telegram account to a user", },
            new() { Command = "unlink", Description = "Unlink your Telegram account from the user", },
            new() { Command = "list_users", Description = "List all users", },
            new() { Command = "list_nodes", Description = "List all nodes or nodes from the specified group", },
            new() { Command = "list_node_annotations", Description = "List annotations (ownership and tags) on nodes from the specified group or all groups", },
            new() { Command = "list_groups", Description = "List all groups", },
            new() { Command = "list_group_members", Description = "List members of the specified group", },
            new() { Command = "list_owned_nodes", Description = "List nodes owned by you or the specified user", },
            new() { Command = "list_owned_groups", Description = "List groups owned by you or the specified user", },
            new() { Command = "get_user_data_usage", Description = "Get data usage statistics of the associated user or the specified user", },
            new() { Command = "get_user_data_limit", Description = "Get data limit settings of the associated user or the specified user", },
            new() { Command = "get_group_data_usage", Description = "Get data usage statistics of the specified group", },
            new() { Command = "get_group_data_limit", Description = "Get data limit settings of the specified group", },
            new() { Command = "get_ss_links", Description = "Get your ss:// links to all servers or servers from the specified group", },
            new() { Command = "get_legacy_sip008_links", Description = "Get legacy SIP008 static file delivery links", },
            new() { Command = "get_credentials", Description = "Get your credentials to all servers or servers from the specified group", },
            new() { Command = "report", Description = "Generate server usage report", },
            new() { Command = "report_csv", Description = "Generate server usage report in CSV format", },
        };

        private readonly string _botUsername;
        private readonly BotConfig _botConfig;

        public UpdateHandler(string botUsername, BotConfig botConfig)
        {
            _botUsername = botUsername;
            _botConfig = botConfig;
        }

        public async Task HandleUpdateStreamAsync(ITelegramBotClient botClient, IAsyncEnumerable<Update> updates, CancellationToken cancellationToken = default)
        {
            await foreach (var update in updates)
            {
                try
                {
                    if (update.Type == UpdateType.Message)
                        await HandleCommandAsync(botClient, update.Message, cancellationToken);
                }
                catch (Exception ex)
                {
                    await HandleErrorAsync(ex, cancellationToken);
                }
            }
        }

        public Task HandleCommandAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken = default)
        {
            var (command, argument) = ChatHelper.ParseMessageIntoCommandAndArgument(message.Text, _botUsername);

            // Handle command
            return command switch
            {
                "start" => AuthCommands.StartAsync(botClient, message, _botConfig, cancellationToken),
                "link" => AuthCommands.LinkAsync(botClient, message, argument, cancellationToken),
                "unlink" => AuthCommands.UnlinkAsync(botClient, message, cancellationToken),
                "list_users" => ListCommands.ListUsersAsync(botClient, message, cancellationToken),
                "list_nodes" => ListCommands.ListNodesAsync(botClient, message, argument, cancellationToken),
                "list_node_annotations" => ListCommands.ListNodeAnnotationsAsync(botClient, message, argument, cancellationToken),
                "list_groups" => ListCommands.ListGroupsAsync(botClient, message, cancellationToken),
                "list_group_members" => ListCommands.ListGroupMembersAsync(botClient, message, argument, cancellationToken),
                "list_owned_nodes" => ListCommands.ListOwnedNodesAsync(botClient, message, argument, cancellationToken),
                "list_owned_groups" => ListCommands.ListOwnedGroupsAsync(botClient, message, argument, cancellationToken),
                "get_user_data_usage" => DataCommands.GetUserDataUsageAsync(botClient, message, argument, cancellationToken),
                "get_user_data_limit" => DataCommands.GetUserDataLimitAsync(botClient, message, argument, cancellationToken),
                "get_group_data_usage" => DataCommands.GetGroupDataUsageAsync(botClient, message, argument, cancellationToken),
                "get_group_data_limit" => DataCommands.GetGroupDataLimitAsync(botClient, message, argument, cancellationToken),
                "get_ss_links" => CredCommands.GetSsLinksAsync(botClient, message, argument, cancellationToken),
                "get_legacy_sip008_links" => CredCommands.GetLegacySIP008StaticLinksAsync(botClient, message, cancellationToken),
                "get_credentials" => CredCommands.GetCredentialsAsync(botClient, message, argument, cancellationToken),
                "report" => ReportCommand.GenerateReportAsync(botClient, message, argument, cancellationToken),
                "report_csv" => ReportCommand.GenerateReportAsync(botClient, message, "csv", cancellationToken),
                _ => Task.CompletedTask, // unrecognized command, ignoring
            };
        }

        public static Task HandleErrorAsync(Exception ex, CancellationToken _ = default)
        {
            var errorMessage = ex switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error: [{apiRequestException.ErrorCode}] {apiRequestException.Message}",
                _ => ex.ToString(),
            };

            Console.WriteLine(errorMessage);
            return Task.CompletedTask;
        }
    }
}
