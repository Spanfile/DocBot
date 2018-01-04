using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace DocBot.Services
{
    internal class CommandHandler
    {
        private readonly DiscordSocketClient discord;
        private readonly CommandService commands;
        private readonly IConfigurationRoot config;
        private readonly IServiceProvider provider;

        public CommandHandler(DiscordSocketClient discord, CommandService commands, IConfigurationRoot config,
            IServiceProvider provider)
        {
            this.discord = discord;
            this.commands = commands;
            this.config = config;
            this.provider = provider;

            this.discord.MessageReceived += OnMessageReceivedAsync;
        }

        private async Task OnMessageReceivedAsync(SocketMessage s)
        {
            if (!(s is SocketUserMessage))
                return;

            var message = (SocketUserMessage)s;

            if (message.Author.Id == discord.CurrentUser.Id)
                return;

            var context = new SocketCommandContext(discord, message);

            var argPos = 0;
            if (message.HasStringPrefix(config["prefix"], ref argPos) ||
                message.HasMentionPrefix(discord.CurrentUser, ref argPos))
            {
                var result = await commands.ExecuteAsync(context, argPos, provider);

                if (!result.IsSuccess)
                    await context.Channel.SendMessageAsync(
                        $"I'm not sure what that means. Try `{config["prefix"]}help` or `{config["prefix"]}docs`");
            }
        }
    }
}
