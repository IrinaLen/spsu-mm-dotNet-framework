using System;
using System.Collections.Concurrent;
using System.Threading;

namespace ThreadPool
{
    public class MyThreadPool: IDisposable
    {
        private const int defaultSize = 4;
        private readonly BlockingCollection<Action> waitingTasks;
        public readonly Thread[] threads; // actually, public only for ThreadsNumber() test
        private bool _isDisposed = false;
        private readonly object _disposeLocker = new object();
        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();

        public MyThreadPool(int size)
        {
            waitingTasks = new BlockingCollection<Action>();
            threads = new Thread[size];

            for (var i = 0; i < size; i++)
            {
                var thread = new Thread(() => ConsumeTask(_tokenSource.Token)) { Name = "Thread #" + i.ToString() };
                threads[i] = thread;
                thread.Start();
            }
        }

        public MyThreadPool() : this(defaultSize)
        {
        }

        public void Enqueue<TResult>(MyTask<TResult> task)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("ThreadPool has been disposed!");
            }
            
            waitingTasks.Add(task.Execute);
        }

        private void ConsumeTask(CancellationToken token)
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();
                
                try
                {
                    var task = waitingTasks.Take();
                    task.Invoke();
                }
                catch (InvalidOperationException)
                {
                    return; // ThreadPool was disposed together with waitingTasks
                }
            }
        }

        public void Dispose()
        {
            lock (_disposeLocker)
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException("Disposing already disposed ThreadPool!");
                }
                _isDisposed = true;

                _tokenSource.Cancel();
                // Attempts to remove from the collection will not wait when the collection is empty
                waitingTasks.CompleteAdding();

                foreach (var thread in threads)
                {
                    thread.Join();
                }

                waitingTasks.Dispose();
            }
        }
    }
}
