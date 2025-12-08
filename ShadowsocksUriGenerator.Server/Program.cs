using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.OpenApi;
using ShadowsocksUriGenerator.OnlineConfig;
using ShadowsocksUriGenerator.Server;
using ShadowsocksUriGenerator.Services;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using Telegram.Bot.Types;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddSingleton<DataService>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<DataService>());
builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
                    options.JsonSerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
                    options.JsonSerializerOptions.IgnoreReadOnlyProperties = true;

                    // STJ source generation only works with minimal API.
                    // So the following lines effectively do nothing right now.
                    options.JsonSerializerOptions.TypeInfoResolverChain.Add(OnlineConfigCamelCaseJsonSerializerContext.Default);
                    options.JsonSerializerOptions.TypeInfoResolverChain.Add(OnlineConfigSnakeCaseJsonSerializerContext.Default);
                });

TelegramBotWebhookOptions botWebhookOptions = new();
var botWebhookSection = builder.Configuration.GetSection(TelegramBotWebhookOptions.SectionName);
botWebhookSection.Bind(botWebhookOptions);
bool enableBotWebhook = !string.IsNullOrEmpty(botWebhookOptions.Url);

if (enableBotWebhook)
{
    builder.Services.Configure<TelegramBotWebhookOptions>(botWebhookSection);
    builder.Services.ConfigureTelegramBot<JsonOptions>(opt => opt.SerializerOptions);
    builder.Services.AddSingleton<TelegramBotWebhookService>();
    builder.Services.AddHostedService(p => p.GetRequiredService<TelegramBotWebhookService>());
}

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Shadowsocks URI Generator API Server",
        Description = "Shadowsocks URI Generator API Specifications",
        Version = "v1",
    });

    var xmlPath = $"{AppContext.BaseDirectory}{Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0])}.xml";

    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
    else
    {
        Console.WriteLine("Warning: XML comment file not found.");
    }
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.All;
    options.ForwardLimit = null;
    options.KnownProxies.Clear();
    options.KnownIPNetworks.Clear();
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseReDoc();

app.UseForwardedHeaders();

app.UseAuthorization();

app.MapControllers();

if (enableBotWebhook)
{
    app.MapPost(botWebhookOptions.RoutePattern, HandleUpdateAsync);
}

app.Run();

static async Task<IResult> HandleUpdateAsync(Update update, TelegramBotWebhookService botService, CancellationToken cancellationToken = default)
{
    await botService.UpdateWriter.WriteAsync(update, cancellationToken);
    return TypedResults.Ok();
}
