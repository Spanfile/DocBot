using System;

namespace DocBot.Services
{
    public class BotInfoService
    {
        public Version Version { get; }
        public TimeSpan Uptime => DateTimeOffset.UtcNow - StartTime;
        public DateTimeOffset StartTime { get; }

        public BotInfoService(Version version)
        {
            Version = version;
            StartTime = DateTimeOffset.UtcNow;
        }
    }
}
