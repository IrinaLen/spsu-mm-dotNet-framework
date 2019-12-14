using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace ThreadPool
{
    public class ThreadPool: IDisposable
    {
        public int Capacity { get; }
        private bool _disposed;
        private readonly object _lock = new object();
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        
        private readonly List<Thread> _pool;
        private readonly BlockingCollection<Action> _queue = new BlockingCollection<Action>();

        public ThreadPool(int capacity)
        {
            if (capacity <= 0)
            {
                throw new ArgumentException("Invalid negative or zero capacity", nameof(capacity));
            }

            Capacity = capacity;
            _pool = new List<Thread>(Capacity);

            for (int i = 0; i < capacity; i++)
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
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task), "Illegal null task");
            }
            _queue.Add(task.Execute);
        }

        public void Dispose()
        {
            lock (_lock)
            {
                if (_disposed)
                {
                    return;
                }

                _cts.Cancel();
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
                _queue.CompleteAdding();
                _queue.Dispose();
                _cts.Dispose();
                _disposed = true;
            }
        }

        private void Worker(CancellationToken token)
        {
            while (true)
            {
                try
                {
                    CheckDisposed();
                    if (token.IsCancellationRequested)
                    {
                        throw new OperationCanceledException($"Thread {Thread.CurrentThread.Name} have been canceled", token);
                    }

                    var task = _queue.Take(token);
                    task();
                }
                catch (Exception)
                {
                    return;
                }
            }
        }
        
        private void CheckDisposed()
        {
            lock (_lock)
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException(nameof(ThreadPool), "The collection has been disposed.");
                }   
            }
        }
    }
}