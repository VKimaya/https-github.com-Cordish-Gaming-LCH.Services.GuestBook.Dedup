// Copyright (c) 2023 CG Shared Services, LLC
// File: LCH.Services..GuestBook.Dedup.Program.cs
// ---------------------------------------------------------------------------------------------------
// Modifications:
// Date:                                       Name:                                  Description:

using System.Diagnostics;
using System.IO;
using System.Linq;
using LCH.Services.GuestBook.Dedup.DataAccess;
using LCH.Services.GuestBook.Dedup.Services;
using LCH.Services.Logging;
using LCH.Services.Models.Configuration;
using LCH.Services.Models.Session;
using LCH.Services.Mq;
using LCH.Services.Mq.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Host = Microsoft.Extensions.Hosting.Host;
using Path = System.IO.Path;

namespace LCH.Services.GuestBook.Dedup;

public class Program
{
    private static IConfiguration configuration;

    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .UseWindowsService()
            .ConfigureAppConfiguration((context, config) =>
            {
                // Set up to run as a service if not in Debug mode or if a command line argument is not --console
                bool isService = !(Debugger.IsAttached || args.Contains("--console"));
                if (isService)
                {
                    ProcessModule processModule = Process.GetCurrentProcess().MainModule;
                    if (processModule != null)
                    {
                        string pathToExe = processModule.FileName;
                        string pathToContentRoot = Path.GetDirectoryName(pathToExe);
                        Directory.SetCurrentDirectory(pathToContentRoot);
                    }
                }

                config.SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("serilog.json", false, true)
                    .AddJsonFile("appsettings.json", false, true);
                configuration = config.Build();
            })
            .ConfigureServices((hostContext, services) =>
            {
                // Inject Configuration from MqSettings of appSettings to IOptionsMonitor<MqSettings> mqSettings
                services.Configure<MqSettings>(options =>
                    hostContext.Configuration.GetSection("MqSettings").Bind(options));
                services.Configure<AppSettings>(options =>
                    hostContext.Configuration.GetSection("AppSettings").Bind(options));
                services.Configure<ConnectionStrings>(options =>
                    hostContext.Configuration.GetSection("ConnectionStrings").Bind(options));

                // Configure Mq classes
                services.AddSingleton<IConnect, Connect>();
                services.AddTransient<IPublisher, Publisher>();

                // Inject Configuration explicitly to avoid future errors
                services.AddSingleton(configuration);

                // Inject the services
                services.AddSingleton<IRequestMetadata, RequestMetadata>();
                services.AddSingleton<IHeartBeatService, HeartBeatService>();
                services.AddSingleton<IGuestBookDupPoller, GuestBookDupPoller>();
                services.AddSingleton<IGuestBookDao, GuestBookDao>();

                // Configure Logging using LCH.Logging library
                services.AddSingleton<ISeriloggingProvider, SeriloggingProvider>();
                ConfigureLogging();

                // Configure WindowsService
                services.AddHostedService<WindowsService>();
            });
    }

    private static void ConfigureLogging()
    {
        ILogger subLogger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .MinimumLevel.Verbose()
            .CreateLogger();

        MqSettings mqSettings = new();
        configuration.GetSection("MqSettings").Bind(mqSettings);
        ISeriloggingProvider seriloggingProvider = new SeriloggingProvider();
        ILogger logger = seriloggingProvider.GetLogger("log", mqSettings, subLogger);
        Log.Logger = logger;
    }
}