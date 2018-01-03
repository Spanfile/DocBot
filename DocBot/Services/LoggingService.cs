using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace DocBot.Services
{
    internal class LoggingService
    {
        private readonly DiscordSocketClient discord;
        private readonly CommandService commands;

        private readonly string logDirectory;
        private string LogFile => Path.Combine(logDirectory, $"{DateTime.UtcNow:yyyy-MM-dd}.txt");

        public LoggingService(DiscordSocketClient discord, CommandService commands)
        {
            logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");

            this.discord = discord;
            this.commands = commands;

            this.discord.Log += OnLog;
            this.commands.Log += OnLog;
        }

        public Task LogInfo(string message, string source = "DocBot", Exception ex = null) => Log(LogSeverity.Info, source, message, ex);
        public Task LogWarning(string message, string source = "DocBot", Exception ex = null) => Log(LogSeverity.Warning, source, message, ex);
        public Task LogError(string message, string source = "DocBot", Exception ex = null) => Log(LogSeverity.Error, source, message, ex);
        public Task LogCritical(string message, string source = "DocBot", Exception ex = null) => Log(LogSeverity.Critical, source, message, ex);
        public Task LogVerbose(string message, string source = "DocBot", Exception ex = null) => Log(LogSeverity.Verbose, source, message, ex);
        public Task LogDebug(string message, string source = "DocBot", Exception ex = null) => Log(LogSeverity.Debug, source, message, ex);

        private Task Log(LogSeverity severity, string source, string message, Exception ex) =>
            OnLog(new LogMessage(severity, source, message, ex));

        private Task OnLog(LogMessage message)
        {
            if (!Directory.Exists(logDirectory))
                Directory.CreateDirectory(logDirectory);

            if (!File.Exists(LogFile))
                File.Create(LogFile).Dispose();

            var logText =
                $"{DateTime.UtcNow:hh:mm:ss} [{message.Severity}] {message.Source}: {message.Exception?.ToString() ?? message.Message}";

            return Task.WhenAll(File.AppendAllTextAsync(LogFile, logText + '\n'), Console.Out.WriteLineAsync(logText));
        }
    }
}
