﻿using System;
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
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("config.json")
                .Build();

            var services = new ServiceCollection()
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig {
#if DEBUG
                    LogLevel = LogSeverity.Debug,
#else
                    LogLevel = LogSeverity.Info,
#endif
                    MessageCacheSize = 1000
                }))
                .AddSingleton(new CommandService(new CommandServiceConfig {
#if DEBUG
                    LogLevel = LogSeverity.Debug,
#else
                    LogLevel = LogSeverity.Info,
#endif
                    DefaultRunMode = RunMode.Async
                }))
                .AddSingleton<CommandHandler>()
                .AddSingleton<LoggingService>()
                .AddSingleton<StartupService>()
                .AddSingleton(config);

            var provider = services.BuildServiceProvider();

            var logger = provider.GetRequiredService<LoggingService>();
            await logger.Log(new LogMessage(
                LogSeverity.Info, "DocBot", $"DocBot version {Assembly.GetEntryAssembly().GetName().Version}"));
            await provider.GetRequiredService<StartupService>().StartAsync();
            provider.GetRequiredService<CommandHandler>();

            await Task.Delay(-1);
        }
    }
}
