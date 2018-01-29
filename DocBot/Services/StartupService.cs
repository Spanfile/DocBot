using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace DocBot.Services
{
    internal class StartupService
    {
        private readonly DiscordSocketClient discord;
        private readonly CommandService commands;
        private readonly IConfigurationRoot config;

        public StartupService(DiscordSocketClient discord, CommandService commands, IConfigurationRoot config)
        {
            this.discord = discord;
            this.commands = commands;
            this.config = config;
        }

        public async Task StartAsync()
        {
            var token = config["discord:token"];
            if (string.IsNullOrWhiteSpace(token))
                throw new Exception("No bot token found in config.json");

            await discord.LoginAsync(TokenType.Bot, token);
            await discord.StartAsync();

            await commands.AddModulesAsync(Assembly.GetEntryAssembly());

            await discord.SetGameAsync($"{config["prefix"]}help");
        }
    }
}
