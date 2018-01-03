using System;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using Discord.WebSocket;
using DocBot.Collections;
using Microsoft.Extensions.Configuration;

namespace DocBot.Services
{
    internal class PerformanceService : IDisposable
    {
        public float AverageLatency => latencies.Sum() / (float)latencies.Count;
        public float AverageHeapMemory => heapMemories.Sum() / (float)heapMemories.Count;
        public float AverageProcessMemory => processMemories.Sum() / (float) processMemories.Count;

        public int LatencySampleTimeRange => latencies.Count * collectionInterval;
        public int HeapMemorySampleTimeRange => heapMemories.Count * collectionInterval;
        public int ProcessMemorySampleTimeRange => processMemories.Count * collectionInterval;

        private readonly DiscordSocketClient discord;
        private readonly IConfigurationRoot config;
        private readonly LoggingService logger;

        private readonly Timer updateTimer;
        private readonly Process currentProcess;

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

            var sampleLimit = int.Parse(this.config["perf:samples"]);
            collectionInterval = int.Parse(this.config["perf:collectionInterval"]);

            latencies = new LimitedStack<int>(sampleLimit);
            heapMemories = new LimitedStack<long>(sampleLimit);
            processMemories = new LimitedStack<long>(sampleLimit);

            logger.LogDebug($"Collection interval: {collectionInterval} ms. Samples to collect: {sampleLimit}");

            updateTimer = new Timer(collectionInterval) {AutoReset = true};
            updateTimer.Elapsed += UpdateTimerOnElapsed;
            updateTimer.Start();
        }

        private async void UpdateTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            latencies.Push(discord.Latency);
            heapMemories.Push(GC.GetTotalMemory(false));
            processMemories.Push(currentProcess.PrivateMemorySize64);

            await logger.LogDebug("Collection finished.\n\t" +
                                  $"Average latency: {AverageLatency} ms\n\t" +
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
