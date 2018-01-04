using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace DocBot.Services.Documentation
{
    // ReSharper disable once InconsistentNaming
    internal class PhantomJSProvider
    {
        private readonly IConfigurationRoot config;

        public PhantomJSProvider(IConfigurationRoot config)
        {
            this.config = config;
        }

        // ReSharper disable once InconsistentNaming
        public async Task<string> FetchHTML(string url, string indexJs = "index.js")
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
