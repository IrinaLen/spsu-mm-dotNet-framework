using System;
using System.Collections.Concurrent;
using System.Threading;

namespace ThreadPool
{
    public class ThreadPool : IDisposable
    {
        private BlockingCollection<IMyTaskGeneral> _queue = new BlockingCollection<IMyTaskGeneral>();
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private bool _disposed = false;

        public ThreadPool(int maxThreads = 42)
        {
            for (int i = 0; i < maxThreads; i++)
            {
                new Thread(() => Worker(_cancellationTokenSource.Token)).Start();
            }
        }

        private void Worker(CancellationToken token)
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();
                _queue.Take().Execute();
            }
        }

        public void Enqueue<TResult>(IMyTask<TResult> task)
        {
            lock (_queue)
            {
                _queue.Add(task);
            }
        }

        public void Dispose()
        {
            lock (this)
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;

                _cancellationTokenSource.Cancel();
                _queue.CompleteAdding();
            }
        }
    }
}