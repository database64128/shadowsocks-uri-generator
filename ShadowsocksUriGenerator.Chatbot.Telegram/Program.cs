using ShadowsocksUriGenerator.Chatbot.Telegram.CLI;
using ShadowsocksUriGenerator.CLI.Utils;
using System;
using System.CommandLine;
using System.Text;
using System.Threading.Tasks;

namespace ShadowsocksUriGenerator.Chatbot.Telegram;

internal class Program
{
    private static Task<int> Main(string[] args)
    {
        var botTokenOption = new Option<string?>("--bot-token", "Telegram bot token.");
        var serviceNameOption = new Option<string?>("--service-name", "Service name. Will be displayed in the welcome message.");
        var usersCanSeeAllUsersOption = new Option<bool?>("--users-can-see-all-users", "Whether any registered user is allowed to see all registered users.");
        var usersCanSeeAllGroupsOption = new Option<bool?>("--users-can-see-all-groups", "Whether any registered user is allowed to see all groups.");
        var usersCanSeeGroupDataUsageOption = new Option<bool?>("--users-can-see-group-data-usage", "Whether users are allowed to query group data usage metrics.");
        var usersCanSeeGroupDataLimitOption = new Option<bool?>("--users-can-see-group-data-limit", "Whether users are allowed to see other group member's data limit.");
        var allowChatAssociationOption = new Option<bool?>("--allow-chat-association", "Whether Telegram association through /link in chat is allowed.");

        var configGetCommand = new Command("get", "Get and print bot config.");

        var configSetCommand = new Command("set", "Change bot config.")
        {
            botTokenOption,
            serviceNameOption,
            usersCanSeeAllUsersOption,
            usersCanSeeAllGroupsOption,
            usersCanSeeGroupDataUsageOption,
            usersCanSeeGroupDataLimitOption,
            allowChatAssociationOption,
        };

        var cancellationTokenBinder = new CancellationTokenBinder();

        configGetCommand.SetHandler(ConfigCommand.Get, cancellationTokenBinder);
        configSetCommand.SetHandler(ConfigCommand.Set, botTokenOption, serviceNameOption, usersCanSeeAllUsersOption, usersCanSeeAllGroupsOption, usersCanSeeGroupDataUsageOption, usersCanSeeGroupDataLimitOption, allowChatAssociationOption, cancellationTokenBinder);

        var configCommand = new Command("config", "Print or change bot config.")
        {
            configGetCommand,
            configSetCommand,
        };

        var rootCommand = new RootCommand("A Telegram bot for user interactions with Shadowsocks URI Generator.")
        {
            configCommand,
        };

        rootCommand.AddOption(botTokenOption);
        rootCommand.SetHandler(BotRunner.RunBot, botTokenOption, cancellationTokenBinder);

        Console.OutputEncoding = Encoding.UTF8;
        return rootCommand.InvokeAsync(args);
    }
}
