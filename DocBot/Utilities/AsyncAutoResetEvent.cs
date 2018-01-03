using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocBot.Utilities
{
    internal class AsyncAutoResetEvent
    {
        private static readonly Task completed = Task.FromResult(true);
        private readonly Queue<TaskCompletionSource<bool>> waits = new Queue<TaskCompletionSource<bool>>();

        public Task WaitAsync()
        {
            lock (waits)
            {
                var taskCompletionSource = new TaskCompletionSource<bool>();
                waits.Enqueue(taskCompletionSource);
                return taskCompletionSource.Task;
            }
        }

        public void Set()
        {
            TaskCompletionSource<bool> toRelease = null;

            lock (waits)
            {
                if (waits.Count > 0)
                    toRelease = waits.Dequeue();
            }

            toRelease?.SetResult(true);
        }
    }
}
