using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DocBot.Services;
using DocBot.Services.Documentation;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DocBot
{
    internal class Program
    {
        internal static void Main(string[] args)
            => new Program().StartAsync().GetAwaiter().GetResult();

        internal async Task StartAsync()
        {
            const LogSeverity globalLogLevel =
#if DEBUG
                LogSeverity.Debug;
#else
                LogSeverity.Verbose;
#endif

            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("config.json")
                .Build();

            var services = new ServiceCollection()
                .AddSingleton(config)
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig {
                    LogLevel = globalLogLevel,
                    MessageCacheSize = 1000
                }))
                .AddSingleton(new CommandService(new CommandServiceConfig {
                    LogLevel = globalLogLevel,
                    DefaultRunMode = RunMode.Async
                }))
                .AddSingleton<BotInfoService>()
                .AddSingleton(Assembly.GetEntryAssembly().GetName().Version)
                .AddSingleton(serviceProvider => new LoggingService(
                    serviceProvider.GetRequiredService<DiscordSocketClient>(),
                    serviceProvider.GetRequiredService<CommandService>(),
                    globalLogLevel))
                .AddSingleton<CommandHandler>()
                .AddSingleton<StartupService>()
                .AddSingleton<PerformanceService>()
                .AddSingleton<DocumentationCacheService>()
                .AddSingleton<DocumentationService>()
                .AddSingleton<PhantomJsProvider>()
                .AddSingleton(new HtmlWeb {UserAgent = config["useragent"]});

            var provider = services.BuildServiceProvider();

            var logger = provider.GetRequiredService<LoggingService>();
            await logger.LogInfo($"DocBot version {provider.GetRequiredService<Version>()}");

            provider.GetRequiredService<CommandHandler>();
            provider.GetRequiredService<PerformanceService>();
            provider.GetRequiredService<BotInfoService>();

            // initialise the services that have to be explicitly initialised
            await provider.GetRequiredService<StartupService>().StartAsync();
            await provider.GetRequiredService<DocumentationService>().StartAsync();
            await provider.GetRequiredService<DocumentationCacheService>().Load();

            await Task.Delay(-1);
        }
    }
}
