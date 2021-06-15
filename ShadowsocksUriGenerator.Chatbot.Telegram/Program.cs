using ShadowsocksUriGenerator.Chatbot.Telegram.CLI;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ShadowsocksUriGenerator.Chatbot.Telegram
{
    class Program
    {
        static Task<int> Main(string[] args)
        {
            var botTokenOption = new Option<string>("--bot-token", "Telegram bot token.");
            var serviceNameOption = new Option<string>("--service-name", "Service name. Will be displayed in the welcome message.");
            var usersCanSeeAllUsersOption = new Option<bool?>("--users-can-see-all-users", "Whether any registered user is allowed to see all registered users.");
            var usersCanSeeAllGroups = new Option<bool?>("--users-can-see-all-groups", "Whether any registered user is allowed to see all groups.");
            var usersCanSeeGroupDataUsage = new Option<bool?>("--users-can-see-group-data-usage", "Whether users are allowed to query group data usage metrics.");
            var usersCanSeeGroupDataLimit = new Option<bool?>("--users-can-see-group-data-limit", "Whether users are allowed to see other group member's data limit.");
            var allowChatAssociation = new Option<bool?>("--allow-chat-association", "Whether Telegram association through /link in chat is allowed.");

            var configGetCommand = new Command("get", "Get and print bot config.")
            {
                Handler = CommandHandler.Create<CancellationToken>(ConfigCommand.Get),
            };

            var configSetCommand = new Command("set", "Change bot config.")
            {
                botTokenOption,
                serviceNameOption,
                usersCanSeeAllUsersOption,
                usersCanSeeAllGroups,
                usersCanSeeGroupDataUsage,
                usersCanSeeGroupDataLimit,
                allowChatAssociation,
            };

            configSetCommand.Handler = CommandHandler.Create<string, string, bool?, bool?, bool?, bool?, bool?, CancellationToken>(ConfigCommand.Set);

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
            rootCommand.Handler = CommandHandler.Create<string, CancellationToken>(BotRunner.RunBot);

            Console.OutputEncoding = Encoding.UTF8;
            return rootCommand.InvokeAsync(args);
        }
    }
}
