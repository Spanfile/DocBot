using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace DocBot.Services.Documentation
{
    internal class PhantomJsProvider
    {
        private readonly IConfigurationRoot config;

        public PhantomJsProvider(IConfigurationRoot config)
        {
            this.config = config;
        }

        public async Task<string> FetchHtml(string url, string indexJs = "index.js")
        {
            var proc = new Process {
                StartInfo = new ProcessStartInfo {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    FileName = config["phantomjsPath"],
                    Arguments = $"\"{indexJs}\" {url}"
                }
            };

            var exitTask = GetWaitForExitAsyncTask(proc);
            proc.Start();

            var output = await proc.StandardOutput.ReadToEndAsync();
            await exitTask;

            return output;
        }

        private static Task GetWaitForExitAsyncTask(Process proc)
        {
            var tcs = new TaskCompletionSource<object>();
            proc.EnableRaisingEvents = true;
            proc.Exited += (s, e) => tcs.TrySetResult(null);
            return tcs.Task;
        }
    }
}
