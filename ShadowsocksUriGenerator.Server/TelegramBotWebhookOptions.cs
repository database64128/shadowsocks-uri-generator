namespace ShadowsocksUriGenerator.Server;

public sealed class TelegramBotWebhookOptions
{
    public const string SectionName = "TelegramBotWebhook";

    /// <summary>
    /// Gets or sets the route pattern for receiving incoming webhook requests.
    /// </summary>
    public string RoutePattern { get; set; } = "/flight-attendant";

    /// <summary>
    /// Gets or sets the webhook URL.
    /// </summary>
    /// <remarks>If null or empty, webhook will be disabled.</remarks>
    public string? Url { get; set; }

    /// <summary>
    /// Gets or sets an optional secret token for the webhook.
    /// </summary>
    public string? SecretToken { get; set; }
}
