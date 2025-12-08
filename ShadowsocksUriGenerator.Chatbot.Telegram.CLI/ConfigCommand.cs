using ShadowsocksUriGenerator.Utils;

namespace ShadowsocksUriGenerator.Chatbot.Telegram.CLI;

public static class ConfigCommand
{
    public static async Task<int> Get(CancellationToken cancellationToken = default)
    {
        var (botConfig, loadBotConfigErrMsg) = await BotConfig.LoadBotConfigAsync(cancellationToken);
        if (loadBotConfigErrMsg is not null)
        {
            Console.WriteLine(loadBotConfigErrMsg);
            return 1;
        }

        ConsoleHelper.PrintTableBorder(28, 50);
        Console.WriteLine($"|{"Key",-28}|{"Value",50}|");
        ConsoleHelper.PrintTableBorder(28, 50);

        Console.WriteLine($"|{"Version",-28}|{botConfig.Version,50}|");
        Console.WriteLine($"|{"BotToken",-28}|{botConfig.BotToken,50}|");
        Console.WriteLine($"|{"ServiceName",-28}|{botConfig.ServiceName,50}|");
        Console.WriteLine($"|{"UsersCanSeeAllUsers",-28}|{botConfig.UsersCanSeeAllUsers,50}|");
        Console.WriteLine($"|{"UsersCanSeeAllGroups",-28}|{botConfig.UsersCanSeeAllGroups,50}|");
        Console.WriteLine($"|{"UsersCanSeeGroupDataUsage",-28}|{botConfig.UsersCanSeeGroupDataUsage,50}|");
        Console.WriteLine($"|{"UsersCanSeeGroupDataLimit",-28}|{botConfig.UsersCanSeeGroupDataLimit,50}|");
        Console.WriteLine($"|{"AllowChatAssociation",-28}|{botConfig.AllowChatAssociation,50}|");

        ConsoleHelper.PrintTableBorder(28, 50);

        return 0;
    }

    public static async Task<int> Set(string? botToken, string? serviceName, bool? usersCanSeeAllUsers, bool? usersCanSeeAllGroups, bool? usersCanSeeGroupDataUsage, bool? usersCanSeeGroupDataLimit, bool? allowChatAssociation, CancellationToken cancellationToken = default)
    {
        var (botConfig, loadBotConfigErrMsg) = await BotConfig.LoadBotConfigAsync(cancellationToken);
        if (loadBotConfigErrMsg is not null)
        {
            Console.WriteLine(loadBotConfigErrMsg);
            return 1;
        }

        if (!string.IsNullOrEmpty(botToken))
            botConfig.BotToken = botToken;
        if (!string.IsNullOrEmpty(serviceName))
            botConfig.ServiceName = serviceName;
        if (usersCanSeeAllUsers is bool canSeeUsers)
            botConfig.UsersCanSeeAllUsers = canSeeUsers;
        if (usersCanSeeAllGroups is bool canSeeGroups)
            botConfig.UsersCanSeeAllGroups = canSeeGroups;
        if (usersCanSeeGroupDataUsage is bool canSeeGroupDataUsage)
            botConfig.UsersCanSeeGroupDataUsage = canSeeGroupDataUsage;
        if (usersCanSeeGroupDataLimit is bool canSeeGroupDataLimit)
            botConfig.UsersCanSeeGroupDataLimit = canSeeGroupDataLimit;
        if (allowChatAssociation is bool allowLinking)
            botConfig.AllowChatAssociation = allowLinking;

        var saveBotConfigErrMsg = await BotConfig.SaveBotConfigAsync(botConfig, cancellationToken);
        if (saveBotConfigErrMsg is not null)
        {
            Console.WriteLine(saveBotConfigErrMsg);
            return 1;
        }

        return 0;
    }
}
