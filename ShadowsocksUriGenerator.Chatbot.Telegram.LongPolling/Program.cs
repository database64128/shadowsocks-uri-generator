using ShadowsocksUriGenerator.Chatbot.Telegram.LongPolling;
using ShadowsocksUriGenerator.Services;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHttpClient();
builder.Services.AddSingleton<DataService>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<DataService>());
builder.Services.AddHostedService<LongPollingBotService>();

var host = builder.Build();
host.Run();
