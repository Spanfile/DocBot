using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace DocBot.Utilities
{
    internal static class ProcessExtensions
    {
        public static Task<int> WaitForExitAsync(this Process proc)
        {
            var tcs = new TaskCompletionSource<int>();
            proc.EnableRaisingEvents = true;
            proc.Exited += (s, e) => tcs.SetResult(proc.ExitCode);
            return tcs.Task;
        }
    }
}
