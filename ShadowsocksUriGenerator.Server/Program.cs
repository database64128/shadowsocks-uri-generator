using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using ShadowsocksUriGenerator;
using ShadowsocksUriGenerator.Services;
using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Workaround for https://github.com/dotnet/runtime/issues/61675.
// Can be removed in .NET 7.
IConfigurationBuilder config = builder.Configuration;

// Remove JSON and env providers.
// This appears to be the only way. If we clear Sources, ASPNETCORE_URLS won't work.
config.Sources.RemoveAt(config.Sources.Count - 1);
config.Sources.RemoveAt(config.Sources.Count - 1);
config.Sources.RemoveAt(config.Sources.Count - 1);

builder.Configuration.SetBasePath(FileHelper.configDirectory)
                     .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                     .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddSingleton<IDataService, DataService>();
builder.Services.AddHostedService(provider => provider.GetService<IDataService>() as DataService ?? throw new Exception("Injected IDataService is not DataService."));
builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
                    options.JsonSerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
                    options.JsonSerializerOptions.IgnoreReadOnlyProperties = true;
                });

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
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.All;
    options.ForwardLimit = null;
    options.KnownProxies.Clear();
    options.KnownNetworks.Clear();
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseReDoc();

app.UseForwardedHeaders();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
