using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using ShadowsocksUriGenerator;
using ShadowsocksUriGenerator.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureAppConfiguration((hostingContext, config) =>
{
    Trace.Assert(config.Sources.Count == 4);

    config.Sources.RemoveAt(3);
    config.Sources.RemoveAt(2);
    config.Sources.RemoveAt(1);

    config.SetBasePath(FileHelper.configDirectory)
          .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
          .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);

    config.AddEnvironmentVariables();
});

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
