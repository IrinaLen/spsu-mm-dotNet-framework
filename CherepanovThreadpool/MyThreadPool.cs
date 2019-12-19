using System;
using System.Collections.Concurrent;
using System.Threading;

namespace CherepanovThreadpool
{
    public class MyThreadPool : IDisposable
    {
        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private readonly CancellationToken _token;
        private object _disposeLock = new object();
        private ConcurrentQueue<ITask> _taskQueue = new ConcurrentQueue<ITask>();

        public readonly int ThreadNumber;
        public bool IsDisposed { get; private set; }

        public MyThreadPool(int threadNumber)
        {
            this.ThreadNumber = threadNumber;
            _token = _tokenSource.Token;

            for (int i = 0; i < threadNumber; i++)
            {
                var worker = new Thread(Worker);
                worker.Start();
            }
        }

        private void Worker()
        {
            while (true)
            {
                lock (_disposeLock)
                {
                    if (_token.IsCancellationRequested)
                    {
                        return;
                    }
                }

                if (_taskQueue.TryDequeue(out ITask task))
                {
                    task.Exec();
                }
            }
        }

        public void Enqueue<TResult>(IMyTask<TResult> myTask)
        {
            lock (_disposeLock)
            {
                if (IsDisposed)
                {
                    throw new AggregateException("Cant enqueue new task! Threadpool is disposed!!!");
                }
                _taskQueue.Enqueue(myTask);
                myTask.IsInThreadpool = true;
            }
        }

        public void Dispose()
        {
            lock (_disposeLock)
            {
                if (IsDisposed) return;
                while (!_taskQueue.IsEmpty)
                {
                    if (_taskQueue.TryDequeue(out ITask task))
                    {
                        task.ThreadpoolIsDisposed = true;
                    }
                }
                IsDisposed = true;
                _tokenSource.Cancel();
                GC.SuppressFinalize(this);
            }
        }
    }
}
