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
        public async Task HelpAsync()
        {
            var allowedModules = new[] {"Help", "Info"};
            var commands = service.Modules
                .Where(m => allowedModules.Contains(m.Name))
                .SelectMany(m => m.Commands).ToArray();

            if (!commands.Any())
            {
                await ReplyAsync("I failed to find any commands at all - none whatsoever, from anywhere. That's troubling");
                await logger.LogWarning("Could not find any commands", "HelpModule");
                return;
            }

            var prefix = config["prefix"];
            var embed = new EmbedBuilder()
                .WithColor(new Color(100, 149, 237))
                .WithDescription("These are the commands you can use. There ain't too many of them")
                .AddField(f =>
                {
                    f.Name = ";doc <documentation> <query>";
                    f.Value = "Queries a given documentation site. Use `;doc docs` to see all supported sites.";
                });

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

                embed.AddField(f =>
                {
                    f.Name = nameBuilder.ToString();
                    f.Value = valueBuilder.ToString();
                });
            }

            await ReplyAsync("", embed: embed.Build());
        }
    }
}
