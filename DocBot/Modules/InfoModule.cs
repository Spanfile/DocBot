using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DocBot.Services;
using Microsoft.Extensions.Configuration;

namespace DocBot.Modules
{
    [Name("Info")]
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient discord;
        private readonly LoggingService logger;

        public InfoModule(DiscordSocketClient discord, LoggingService logger)
        {
            this.discord = discord;
            this.logger = logger;
        }

        [Command("info")]
        [Summary("Shows different diagnostic values for the bot")]
        public async Task Info(
            [Summary("Force a GC collection before calculating heap memory usage")]
            bool forceCollect = false)
        {
            long heap;
            if (forceCollect)
            {
                await logger.LogDebug("Forcing a GC collect before calculating heap size");
                heap = await Task.Factory.StartNew(() => GC.GetTotalMemory(true));
            }
            else
                heap = GC.GetTotalMemory(false);

            long proc;
            using (var currentProx = Process.GetCurrentProcess())
                proc = currentProx.PrivateMemorySize64;

            var embed = new EmbedBuilder()
                .WithColor(100, 149, 237)
                .AddInlineField("Gateway latency", $"{discord.Latency} ms")
                .AddInlineField("Heap memory use", $"{heap / 1_000_000f:##.00}MB")
                .AddInlineField("Process memory use", $"{proc / 1_000_000f:##.00}MB")
                .AddInlineField("Guilds", discord.Guilds.Count)
                .AddInlineField("Users", discord.Guilds.Sum(g => g.Users.Count))
                .WithTimestamp(DateTimeOffset.UtcNow);

            await ReplyAsync("", embed: embed.Build());
        }
    }
}
