using System;
using System.Collections;
using System.Collections.Generic;
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

        [Command("help"), Alias("h")]
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
                    });
                }
            }

            await ReplyAsync("", embed: builder.Build());
        }

        [Command("help"), Alias("h")]
        [Summary("Shows help for a certain command or commands")]
        public async Task HelpAsync(string commandStr)
        {
            CommandInfo[] commands = null;
            var message = "";
            var description = "These are the commands you can use. There ain't too many of them";

            if (!string.IsNullOrWhiteSpace(commandStr))
            {
                var result = service.Search(Context, commandStr);

                if (result.IsSuccess)
                {
                    commands = result.Commands.Select(c => c.Command).ToArray();
                    description = $"These are the commands you can use that look like `{commandStr}`";
                }
                else
                    message = $"I don't recognise `{commandStr}`?";
            }
            else
            {
                commands = service.Modules.SelectMany(m => m.Commands).ToArray();
            }

            if (!commands?.Any() ?? true)
            {
                await ReplyAsync("I failed to find any commands at all - none whatsoever, from anywhere. That's troubling");
                return;
            }

            var prefix = config["prefix"];
            var builder = new EmbedBuilder
            {
                Color = new Color(100, 149, 237),
                Description = description
            };

            foreach (var command in commands)
            {
                var parameters = string.Join(", ", command.Parameters.Select(p => p.Name));

                builder.AddField(f =>
                {
                    f.Name = string.Join(", ", command.Aliases.Select(a => $"{prefix} {a}"));
                    f.Value = $"Parameters: {parameters}\nSummary: {command.Summary}";
                });
            }

            await ReplyAsync(message, embed: builder.Build());
        }
    }
}
