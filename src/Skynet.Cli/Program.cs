using Karambolo.Extensions.Logging.File;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sia.Skynet;
using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using System.Threading.Tasks;

namespace Skynet.Cli
{
    class Program
    {
        RootCommand Setup()
        {
            var rootCommand = new RootCommand("A command line tool to upload files to Sia Skynet");
            rootCommand.AddCommand(new UploadCommand());
            return rootCommand;
        }

        static async Task<int> Main(string[] args)
        {
            return await new CommandLineBuilder(new Program().Setup())
                .UseDefaults()
                .UseHost(CreateHostBuilder, host =>
                {
                    host.ConfigureLogging((context, logging) =>
                    {
                        logging.AddFile(file =>
                        {
                            file.RootPath = context.HostingEnvironment.ContentRootPath;
                            file.BasePath = "logs";
                            file.Files = new LogFileOptions[]
                            {
                                new LogFileOptions { Path = "<date>.skynet.log" }
                            };
                        });
                        logging.SetMinimumLevel(LogLevel.Debug);
                    });
                    host.ConfigureServices(services =>
                    {
                        services.AddHttpClient<ISkynetWebPortal, SkynetWebPortal>(client =>
                        {
                            client.BaseAddress = new Uri("https://siasky.net");
                        });
                    });
                })
                .Build()
                .InvokeAsync(args);
        }

        static IHostBuilder CreateHostBuilder(string[] args)
        {
            return new HostBuilder();
        }
    }
}
