using ShadowsocksUriGenerator.Chatbot.Telegram.CLI;
using System;
using System.CommandLine;
using System.Text;
using System.Threading.Tasks;

namespace ShadowsocksUriGenerator.Chatbot.Telegram;

internal class Program
{
    private static Task<int> Main(string[] args)
    {
        var botTokenOption = new CliOption<string?>("--bot-token")
        {
            Description = "Telegram bot token.",
        };
        var serviceNameOption = new CliOption<string?>("--service-name")
        {
            Description = "Service name. Will be displayed in the welcome message.",
        };
        var usersCanSeeAllUsersOption = new CliOption<bool?>("--users-can-see-all-users")
        {
            Description = "Whether any registered user is allowed to see all registered users.",
        };
        var usersCanSeeAllGroupsOption = new CliOption<bool?>("--users-can-see-all-groups")
        {
            Description = "Whether any registered user is allowed to see all groups.",
        };
        var usersCanSeeGroupDataUsageOption = new CliOption<bool?>("--users-can-see-group-data-usage")
        {
            Description = "Whether users are allowed to query group data usage metrics.",
        };
        var usersCanSeeGroupDataLimitOption = new CliOption<bool?>("--users-can-see-group-data-limit")
        {
            Description = "Whether users are allowed to see other group member's data limit.",
        };
        var allowChatAssociationOption = new CliOption<bool?>("--allow-chat-association")
        {
            Description = "Whether Telegram association through /link in chat is allowed.",
        };

        var configGetCommand = new CliCommand("get", "Get and print bot config.");

        var configSetCommand = new CliCommand("set", "Change bot config.")
        {
            botTokenOption,
            serviceNameOption,
            usersCanSeeAllUsersOption,
            usersCanSeeAllGroupsOption,
            usersCanSeeGroupDataUsageOption,
            usersCanSeeGroupDataLimitOption,
            allowChatAssociationOption,
        };

        configGetCommand.SetAction((_, cancellationToken) => ConfigCommand.Get(cancellationToken));
        configSetCommand.SetAction((parseResult, cancellationToken) =>
        {
            var botToken = parseResult.GetValue(botTokenOption);
            var serviceName = parseResult.GetValue(serviceNameOption);
            var usersCanSeeAllUsers = parseResult.GetValue(usersCanSeeAllUsersOption);
            var usersCanSeeAllGroups = parseResult.GetValue(usersCanSeeAllGroupsOption);
            var usersCanSeeGroupDataUsage = parseResult.GetValue(usersCanSeeGroupDataUsageOption);
            var usersCanSeeGroupDataLimit = parseResult.GetValue(usersCanSeeGroupDataLimitOption);
            var allowChatAssociation = parseResult.GetValue(allowChatAssociationOption);
            return ConfigCommand.Set(botToken, serviceName, usersCanSeeAllUsers, usersCanSeeAllGroups, usersCanSeeGroupDataUsage, usersCanSeeGroupDataLimit, allowChatAssociation, cancellationToken);
        });

        var configCommand = new CliCommand("config", "Print or change bot config.")
        {
            configGetCommand,
            configSetCommand,
        };

        var rootCommand = new CliRootCommand("A Telegram bot for user interactions with Shadowsocks URI Generator.")
        {
            configCommand,
        };

        rootCommand.Options.Add(botTokenOption);
        rootCommand.SetAction((parseResult, cancellationToken) =>
        {
            var botToken = parseResult.GetValue(botTokenOption);
            return BotRunner.RunBot(botToken, cancellationToken);
        });

        Console.OutputEncoding = Encoding.UTF8;
        return rootCommand.Parse(args).InvokeAsync();
    }
}
