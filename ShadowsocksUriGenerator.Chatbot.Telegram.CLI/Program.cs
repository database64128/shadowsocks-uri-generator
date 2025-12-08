using ShadowsocksUriGenerator.Chatbot.Telegram.CLI;
using System.CommandLine;
using System.Text;

var botTokenOption = new Option<string?>("--bot-token")
{
    Description = "Telegram bot token.",
};
var serviceNameOption = new Option<string?>("--service-name")
{
    Description = "Service name. Will be displayed in the welcome message.",
};
var usersCanSeeAllUsersOption = new Option<bool?>("--users-can-see-all-users")
{
    Description = "Whether any registered user is allowed to see all registered users.",
};
var usersCanSeeAllGroupsOption = new Option<bool?>("--users-can-see-all-groups")
{
    Description = "Whether any registered user is allowed to see all groups.",
};
var usersCanSeeGroupDataUsageOption = new Option<bool?>("--users-can-see-group-data-usage")
{
    Description = "Whether users are allowed to query group data usage metrics.",
};
var usersCanSeeGroupDataLimitOption = new Option<bool?>("--users-can-see-group-data-limit")
{
    Description = "Whether users are allowed to see other group member's data limit.",
};
var allowChatAssociationOption = new Option<bool?>("--allow-chat-association")
{
    Description = "Whether Telegram association through /link in chat is allowed.",
};

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

var configCommand = new Command("config", "Print or change bot config.")
        {
            configGetCommand,
            configSetCommand,
        };

var rootCommand = new RootCommand("A Telegram bot for user interactions with Shadowsocks URI Generator.")
        {
            configCommand,
        };

Console.OutputEncoding = Encoding.UTF8;
return await rootCommand.Parse(args).InvokeAsync();
