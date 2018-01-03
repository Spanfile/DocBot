using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using DocBot.Services;
using Microsoft.Extensions.Configuration;

namespace DocBot.Modules
{
    [Name("Help")]
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService service;
        private readonly IConfigurationRoot config;
        private readonly LoggingService logger;

        public HelpModule(CommandService service, IConfigurationRoot config, LoggingService logger)
        {
            this.service = service;
            this.config = config;
            this.logger = logger;
        }

        [Command("help")]
        [Summary("Shows help for commands")]
        public async Task HelpAsync(
            [Summary("Command to see help for")]
            string command = null)
        {
            CommandInfo[] commands = null;
            var message = "";
            var description = "These are the commands you can use. There ain't too many of them";

            if (!string.IsNullOrWhiteSpace(command))
            {
                var result = service.Search(Context, command);

                if (result.IsSuccess)
                {
                    commands = result.Commands.Select(c => c.Command).ToArray();
                    description = $"These are the commands you can use that look like `{command}`";
                }
                else
                    message = $"I don't recognise `{command}`, so here's all the commands";
            }
            else
                commands = service.Modules.SelectMany(m => m.Commands).ToArray();

            if (!commands?.Any() ?? true)
            {
                await ReplyAsync("I failed to find any commands at all - none whatsoever, from anywhere. That's troubling");
                await logger.LogWarning("Could not find any commands", "HelpModule");
                return;
            }

            var prefix = config["prefix"];
            var builder = new EmbedBuilder
            {
                Color = new Color(100, 149, 237),
                Description = description
            };

            foreach (var cmd in commands)
            {
                if (!(await cmd.CheckPreconditionsAsync(Context)).IsSuccess)
                    continue;

                var valueBuilder = new StringBuilder();
                var nameBuilder = new StringBuilder();

                nameBuilder
                    .Append(prefix)
                    .Append(cmd.Aliases.First());

                valueBuilder
                    .AppendLine(cmd.Summary);

                foreach (var param in cmd.Parameters)
                {
                    nameBuilder.Append(' ');

                    if (param.IsOptional)
                    {
                        nameBuilder.Append($"[{param.Name}]");
                        valueBuilder.Append("[Optional] ");
                    }
                    else
                        nameBuilder.Append(param.Name);

                    valueBuilder
                        .Append($"**{param.Name}** ")
                        .AppendLine(param.Summary);
                }

                builder.AddField(f =>
                {
                    f.Name = nameBuilder.ToString();
                    f.Value = valueBuilder.ToString();
                });
            }

            await ReplyAsync(message, embed: builder.Build());
        }
    }
}
