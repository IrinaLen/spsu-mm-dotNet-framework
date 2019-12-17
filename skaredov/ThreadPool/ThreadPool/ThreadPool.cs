using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace ThreadPool
{
    public class ThreadPool: IDisposable
    {
        private readonly List<Thread> _pool;
        private readonly object _lock = new object();
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly BlockingCollection<Action> _queue = new BlockingCollection<Action>();

        public int Capacity { get; }
        public bool IsDisposed => _cts.IsCancellationRequested;

        public ThreadPool(int capacity)
        {
            if (capacity <= 0) throw new ArgumentException("Invalid negative or zero capacity", nameof(capacity));

            Capacity = capacity;
            _pool = new List<Thread>(Capacity);

            for (var i = 0; i < capacity; i++)
            {
                var worker = new Thread(() => Worker(_cts.Token))
                {
                    IsBackground = true,
                    Name = $"worker#{i}"
                };
                worker.Start();
                _pool.Add(worker);
            }
        }

        public void Enqueue<TResult>(IMyTask<TResult> task)
        {
            CheckDisposed();
            if (task == null) throw new ArgumentNullException(nameof(task), "Illegal null task");
            _queue.Add(task.Execute);
        }

        public void Dispose()
        {
            lock (_lock)
            {
                if (IsDisposed) return;

                _cts.Cancel();
                _queue.CompleteAdding();
                _pool.ForEach(thread =>
                {
                    try
                    {
                        thread.Join();
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                });
                _pool.Clear();
                _queue.Dispose();
                _cts.Dispose();
            }
        }

        private void Worker(CancellationToken token)
        {
            try
            {
                // try to get and execute task from queue until token is canceled
                foreach (var task in _queue.GetConsumingEnumerable(token)) task();
            }
            catch (OperationCanceledException)
            {
                // calculate remaining tasks in queue and stop thread
                foreach (var task in _queue.GetConsumingEnumerable()) task();
            }
        }

        private void CheckDisposed()
        {
            lock (_lock)
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(nameof(ThreadPool), "The ThreadPool has been disposed.");
            }
        }
    }
}