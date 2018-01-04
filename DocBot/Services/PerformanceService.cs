using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Discord.WebSocket;
using DocBot.Utilities;
using Microsoft.Extensions.Configuration;

namespace DocBot.Services
{
    public class PerformanceService : IDisposable
    {
        public float AverageLatency => latencies.Sum() / (float)latencies.Count;
        public float AverageHeapMemory => heapMemories.Sum() / (float)heapMemories.Count;
        public float AverageProcessMemory => processMemories.Sum() / (float) processMemories.Count;

        public int SampleTimeRange =>
            (heapMemories.Count + processMemories.Count) / 2 * collectionInterval;

        public int MaxSampleTimeRange => sampleLimit * collectionInterval;

        private readonly DiscordSocketClient discord;
        private readonly IConfigurationRoot config;
        private readonly LoggingService logger;

        private readonly System.Timers.Timer updateTimer;
        private readonly Process currentProcess;
        private readonly AsyncAutoResetEvent signal;
        private bool collectOnNextTick = false;

        private readonly int sampleLimit;
        private readonly int collectionInterval;

        private readonly LimitedStack<int> latencies;
        private readonly LimitedStack<long> heapMemories;
        private readonly LimitedStack<long> processMemories;

        public PerformanceService(DiscordSocketClient discord, LoggingService logger, IConfigurationRoot config)
        {
            this.discord = discord;
            this.logger = logger;
            this.config = config;

            currentProcess = Process.GetCurrentProcess();

            sampleLimit = int.Parse(this.config["perf:samples"]);
            collectionInterval = int.Parse(this.config["perf:collectionInterval"]);

            latencies = new LimitedStack<int>(sampleLimit);
            heapMemories = new LimitedStack<long>(sampleLimit);
            processMemories = new LimitedStack<long>(sampleLimit);

            signal = new AsyncAutoResetEvent();

            updateTimer = new System.Timers.Timer(collectionInterval) {AutoReset = true};
            updateTimer.Elapsed += UpdateTimerOnElapsedAsync;
            updateTimer.Start();

            discord.LatencyUpdated += DiscordOnLatencyUpdatedAsync;

            logger.LogDebug($"Collection interval: {collectionInterval} ms. Samples to collect: {sampleLimit}", "PerformanceService");
        }

        private async Task DiscordOnLatencyUpdatedAsync(int i, int i1)
        {
            latencies.Push(discord.Latency);
            await logger.LogDebug($"Gateway latency updated. Average latency: {AverageLatency} ms", "PerformanceService");
        }

        public Task WaitNextTick(bool collectOnNextTick = false)
        {
            this.collectOnNextTick = collectOnNextTick;
            return signal.WaitAsync();
        }

        private async void UpdateTimerOnElapsedAsync(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (collectOnNextTick)
            {
                collectOnNextTick = false;
                await logger.LogDebug("Forcing GC collection before perf collection", "PerformanceService");
                GC.Collect();
            }

            heapMemories.Push(GC.GetTotalMemory(false));
            processMemories.Push(currentProcess.PrivateMemorySize64);

            signal.Set();

            await logger.LogDebug("Performance collection finished\n\t" +
                                  $"Average heap memory: {AverageHeapMemory / 1_000_000f:.##} MB\n\t" +
                                  $"Average process memory: {AverageProcessMemory / 1_000_000f:.##} MB", "PerformanceService");
        }

        public void Dispose()
        {
            updateTimer?.Dispose();
            currentProcess?.Dispose();
        }
    }
}
