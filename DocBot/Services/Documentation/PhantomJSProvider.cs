using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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

        public async Task<string> FetchHtml(string url, string jsFile = "fetchPage.js", params string[] extraArgs)
        {
            var phantomJsPath = Path.GetFullPath(config["phantomjsPath"]);
            var proc = new Process {
                StartInfo = new ProcessStartInfo {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    FileName = phantomJsPath,
                    Arguments = $"\"{jsFile}\" \"{url}\" \"{config["useragent"]}\" {string.Join(" ", extraArgs.Select(a => $"\"{a}\""))}"
                }
            };

            await logger.LogDebug($"Starting PhantomJS executable ({phantomJsPath}), using {Path.GetFullPath(jsFile)} to fetch {url}", "PhantomJsProvider");
            proc.Start();

            var outputBuilder = new StringBuilder();
            proc.OutputDataReceived += (s, e) => outputBuilder.Append(e.Data);
            proc.ErrorDataReceived += async (s, e) => await logger.LogError($"PhantomJS error: {e.Data}");
            proc.BeginOutputReadLine();

            proc.WaitForExit();

            var output = outputBuilder.ToString();
            var exitCode = proc.ExitCode;
            proc.Dispose();

            await logger.LogDebug($"PhantomJS executable exited with code {exitCode}", "PhantomJsProvider");

            if (exitCode == 0)
                return output;

            await logger.LogError($"PhantomJS exited with non-zero code. Standard output:\n{output}",
                "PhantomJsProvider");
            return null;
        }
    }
}
