using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DocBot.Services;
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
                LogSeverity.Info;
#endif

            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("config.json")
                .Build();

            var services = new ServiceCollection()
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig {
                    LogLevel = globalLogLevel,
                    MessageCacheSize = 1000
                }))
                .AddSingleton(new CommandService(new CommandServiceConfig {
                    LogLevel = globalLogLevel,
                    DefaultRunMode = RunMode.Async
                }))
                .AddSingleton<CommandHandler>()
                .AddSingleton<LoggingService>()
                .AddSingleton<StartupService>()
                .AddSingleton<PerformanceService>()
                .AddSingleton(config);

            var provider = services.BuildServiceProvider();

            var logger = provider.GetRequiredService<LoggingService>();
            await logger.LogInfo($"DocBot version {Assembly.GetEntryAssembly().GetName().Version}");
            await provider.GetRequiredService<StartupService>().StartAsync();

            // initialise the rest of the services
            provider.GetRequiredService<CommandHandler>();
            provider.GetRequiredService<PerformanceService>();

            await Task.Delay(-1);
        }
    }
}
