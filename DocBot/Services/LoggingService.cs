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

            this.discord.Log += Log;
            this.commands.Log += Log;
        }

        public Task Log(LogMessage message)
        {
            if (!Directory.Exists(logDirectory))
                Directory.CreateDirectory(logDirectory);

            if (!File.Exists(LogFile))
                File.Create(LogFile).Dispose();

            var logText =
                $"{DateTime.UtcNow:hh:mm:ss} [{message.Severity}] {message.Source}: {message.Exception?.ToString() ?? message.Message}";
            File.AppendAllText(LogFile, logText + '\n');
            return Console.Out.WriteLineAsync(logText);
        }
    }
}
