using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ShadowsocksUriGenerator.Server
{
    internal class Program
    {
        private static Task Main(string[] args)
        {
            var rootCommand = new RootCommand("Shadowsocks URI Generator API Server provides an API endpoint for basic management tasks and online config.");

            rootCommand.SetHandler<CancellationToken>(cancellationToken => cancellationToken.WaitHandle.WaitOne());

            var parser = new CommandLineBuilder(rootCommand)
                .UseHost(_ => Host.CreateDefaultBuilder()
                    .ConfigureAppConfiguration((hostingContext, config) =>
                    {
                        Trace.Assert(config.Sources.Count == 4);

                        config.Sources.RemoveAt(3);
                        config.Sources.RemoveAt(2);
                        config.Sources.RemoveAt(1);

                        config.SetBasePath(FileHelper.configDirectory)
                              .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                              .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);

                        config.AddEnvironmentVariables();
                    }),
                    host =>
                    {
                        host.ConfigureWebHostDefaults(webBuilder =>
                        {
                            webBuilder.UseStartup<Startup>();
                        });
                    })
                .UseDefaults()
                .Build();

            return parser.InvokeAsync(args);
        }
    }
}
