using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.OpenApi.Models;
using ShadowsocksUriGenerator.OnlineConfig;
using ShadowsocksUriGenerator.Services;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IDataService, DataService>();
builder.Services.AddHostedService(provider => provider.GetService<IDataService>() as DataService ?? throw new Exception("Injected IDataService is not DataService."));
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
