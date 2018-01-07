using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace DocBot.Services.Documentation
{
    internal class PhantomJsProvider
    {
        private readonly LoggingService logger;
        private readonly IConfigurationRoot config;

        public PhantomJsProvider(LoggingService logger, IConfigurationRoot config)
        {
            this.logger = logger;
            this.config = config;
        }

        public async Task<string> FetchHtml(string url, string indexJs = "index.js")
        {
            var proc = new Process {
                StartInfo = new ProcessStartInfo {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    FileName = config["phantomjsPath"],
                    Arguments = $"\"{indexJs}\" {url}"
                }
            };

            await logger.LogDebug("Starting PhantomJS executable", "PhantomJsProvider");
            var exitTask = CreateWaitForExitTask(proc);
            proc.Start();

            var exitCode = await exitTask;
            var output = await proc.StandardOutput.ReadToEndAsync();
            await logger.LogDebug($"PhantomJS executable exited with code {exitCode}", "PhantomJsProvider");

            if (exitCode == 0)
                return output;

            await logger.LogDebug($"PhantomJS exited with non-zero code. Standard output:\n{output}", "PhantomJsProvider");
            return null;
        }
        
        private static Task<int> CreateWaitForExitTask(Process proc)
        {
            var tcs = new TaskCompletionSource<int>();
            proc.EnableRaisingEvents = true;
            proc.Exited += (s, e) => tcs.TrySetResult(proc.ExitCode);
            return tcs.Task;
        }
    }
}
