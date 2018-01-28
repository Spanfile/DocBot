using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace DocBot.Services
{
    public class LoggingService
    {
        private readonly DiscordSocketClient discord;
        private readonly CommandService commands;
        private readonly LogSeverity globalSeverity;

        public LoggingService(DiscordSocketClient discord, CommandService commands, LogSeverity globalSeverity)
        {
            this.discord = discord;
            this.commands = commands;
            this.globalSeverity = globalSeverity;

            this.discord.Log += OnLog;
            this.commands.Log += OnLog;
        }

        public Task LogInfo(string message, string source = "DocBot", Exception ex = null) => Log(LogSeverity.Info, source, message, ex);
        public Task LogWarning(string message, string source = "DocBot", Exception ex = null) => Log(LogSeverity.Warning, source, message, ex);
        public Task LogError(string message, string source = "DocBot", Exception ex = null) => Log(LogSeverity.Error, source, message, ex);
        public Task LogCritical(string message, string source = "DocBot", Exception ex = null) => Log(LogSeverity.Critical, source, message, ex);
        public Task LogVerbose(string message, string source = "DocBot", Exception ex = null) => Log(LogSeverity.Verbose, source, message, ex);
        public Task LogDebug(string message, string source = "DocBot", Exception ex = null) => Log(LogSeverity.Debug, source, message, ex);

        private Task Log(LogSeverity severity, string source, string message, Exception ex) => OnLog(new LogMessage(severity, source, message, ex));

        private Task OnLog(LogMessage message)
        {
            if (globalSeverity >= message.Severity)
            {
                return Console.Out.WriteLineAsync(
                    $"{DateTime.UtcNow:hh:mm:ss} [{message.Severity}] {message.Source}: {message.Exception?.ToString() ?? message.Message}");
            }

            return Task.CompletedTask;
        }
    }
}
