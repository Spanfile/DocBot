using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;

namespace DocBot.Modules
{
    [Name("Help")]
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService service;
        private readonly IConfigurationRoot config;

        public HelpModule(CommandService service, IConfigurationRoot config)
        {
            this.service = service;
            this.config = config;
        }

        [Command("help")]
        [Summary("Shows this help")]
        public async Task HelpAsync()
        {
            var prefix = config["prefix"];
            var builder = new EmbedBuilder {
                Color = new Color(100, 149, 237),
                Description = "These are the commands you can use. There ain't too many of 'em."
            };

            foreach (var module in service.Modules)
            {
                string desc = null;
                foreach (var command in module.Commands)
                {
                    var result = await command.CheckPreconditionsAsync(Context);
                    if (result.IsSuccess)
                        desc += $"{prefix}{command.Aliases.First()}\n";
                }

                if (!string.IsNullOrWhiteSpace(desc))
                {
                    builder.AddField(f =>
                    {
                        f.Name = module.Name;
                        f.Value = desc;
                        f.IsInline = false;
                    });
                }
            }

            await ReplyAsync("", embed: builder.Build());
        }

        [Command("help")]
        [Summary("Shows help for a certain command")]
        public async Task HelpAsync(string commandStr)
        {
            var result = service.Search(Context, commandStr);

            if (!result.IsSuccess)
            {
                await ReplyAsync($"What is `{commandStr}`?");
                return;
            }

            var prefix = config["prefix"];
            var builder = new EmbedBuilder {
                Color = new Color(100, 149, 237),
                Description = $"These commands look like `{commandStr}`"
            };

            foreach (var match in result.Commands)
            {
                var command = match.Command;

                builder.AddField(f =>
                {
                    f.Name = string.Join(", ", command.Aliases);
                    f.Value =
                        $"Parameters: {string.Join(", ", command.Parameters.Select(p => p.Name))}" +
                        $"Summary: {command.Summary}";
                    f.IsInline = false;
                });
            }

            await ReplyAsync("", embed: builder.Build());
        }
    }
}
