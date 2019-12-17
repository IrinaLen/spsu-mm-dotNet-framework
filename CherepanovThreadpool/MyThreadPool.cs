using System;
using System.Collections.Concurrent;
using System.Threading;

namespace CherepanovThreadpool
{
    public class MyThreadPool : IDisposable
    {
        public readonly int ThreadNumber;
        public bool IsDisposed { get; private set; }
        private object _disposeLock = new object();
        private ConcurrentQueue<ITask> taskQueue = new ConcurrentQueue<ITask>();
        CancellationTokenSource tokenSource = new CancellationTokenSource();
        CancellationToken token;

        public MyThreadPool(int ThreadNumber)
        {
            this.ThreadNumber = ThreadNumber;
            token = tokenSource.Token;

            for (int i = 0; i < ThreadNumber; i++)
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
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }
                }

                if (taskQueue.TryDequeue(out ITask task))
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
                taskQueue.Enqueue(myTask);
                myTask.IsInThreadpool = true;
            }
        }

        public void Dispose()
        {
            lock (_disposeLock)
            {
                if (IsDisposed) return;
                while (!taskQueue.IsEmpty)
                {
                    if (taskQueue.TryDequeue(out ITask task))
                    {
                        task.ThreadpoolIsDisposed = true;
                    }
                }
                IsDisposed = true;
                tokenSource.Cancel();
                GC.SuppressFinalize(this);
            }
        }
    }
}
