using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DocBot.Services;

namespace DocBot.Modules
{
    [Name("info")]
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient discord;
        private readonly LoggingService logger;
        private readonly PerformanceService perf;

        public InfoModule(DiscordSocketClient discord, LoggingService logger, PerformanceService perf)
        {
            this.discord = discord;
            this.logger = logger;
            this.perf = perf;
        }

        [Command("diagnostic"), Alias("diag")]
        [Summary("Shows different diagnostic values for the bot")]
        public async Task Diagnostic(
            [Summary("If set, wait until the next performance service tick, collect unused memory and show diagnostic info. Requires bot ownership")]
            bool forceCollect = false)
        {
            EmbedBuilder embed;
            IUserMessage msg = null;

            if (forceCollect)
            {
                var ownerId = (await discord.GetApplicationInfoAsync()).Owner.Id;
                if (Context.User.Id != ownerId)
                    return;

                embed = new EmbedBuilder()
                    .WithColor(100, 149, 237)
                    .WithDescription("Waiting for next performance service tick...");
                msg = await ReplyAsync("", embed: embed.Build());
                await perf.WaitNextTick(true);
            }

            embed = new EmbedBuilder()
                .WithColor(100, 149, 237)
                .AddInlineField("Gateway latency", $"{perf.AverageLatency:.##} ms")
                .AddInlineField("Heap memory use", $"{perf.AverageHeapMemory / 1_000_000f:.##}MB")
                .AddInlineField("Process memory use", $"{perf.AverageProcessMemory / 1_000_000f:.##}MB")
                .AddInlineField("GC max generations", $"{GC.MaxGeneration}")
                .AddInlineField("Generation 0 collections", $"{GC.CollectionCount(0)}")
                .AddInlineField("Guilds", discord.Guilds.Count)
                .AddInlineField("Users", discord.Guilds.Sum(g => g.Users.Count))
                .WithTimestamp(DateTimeOffset.UtcNow)
                .WithDescription("Diagnostic data collected over previous " +
                                 $"{perf.MaxSampleTimeRange / 1000}s");

            if (msg != null)
                await msg.ModifyAsync(f => f.Embed = embed.Build());
            else
                await ReplyAsync("", embed: embed.Build());
        }
    }
}
