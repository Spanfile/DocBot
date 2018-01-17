using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using DocBot.Utilities;
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
            var phantomJsPath = Path.GetFullPath(config["phantomjsPath"]);
            await logger.LogDebug($"Starting PhantomJS executable ({phantomJsPath})", "PhantomJsProvider");

            using (var proc = Process.Start(new ProcessStartInfo {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    FileName = phantomJsPath,
                    Arguments = $"\"{indexJs}\" {url}"
                }))
            {
                if (proc == null)
                {
                    await logger.LogError("No process started","PhantomJsProvider");
                    return null;
                }

                var exitCode = await proc.WaitForExitAsync();
                var output = await proc.StandardOutput.ReadToEndAsync();
                await logger.LogDebug($"PhantomJS executable exited with code {exitCode}", "PhantomJsProvider");

                if (exitCode == 0)
                    return output;

                await logger.LogError($"PhantomJS exited with non-zero code. Standard output:\n{output}",
                    "PhantomJsProvider");
                return null;
            }
        }
    }
}
