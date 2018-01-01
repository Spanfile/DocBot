using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;

namespace DocBot.Modules
{
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

            await ReplyAsync("", false, builder.Build());
        }
    }
}
