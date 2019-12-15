using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace MyThreadPool
{
    public class MyThreadPool : IDisposable
    {
        public bool IsDisposed { get; private set; }

        public int NumThreads { get; }


        private readonly BlockingCollection<ITask> _jobQueue = new BlockingCollection<ITask>();

        private List<Thread> _threads = new List<Thread>();

        
        private readonly object _disposeLock = new object();

        public MyThreadPool(int numThreads = 1)
        {
            NumThreads = numThreads;

            for (var i = 0; i < numThreads; i++)
            {
                var thread = new Thread(Worker);
                thread.Start();
                _threads.Add(thread);
            }
        }

        private void Worker()
        {
            while (true)
            {
                try
                {
                    var job = _jobQueue.Take();
                    job.Execute();
                }
                catch (InvalidOperationException)
                {
                    return;
                }
                catch (Exception ex)
                {
                }
            }
        }

        public bool IsAllWorkersAlive()
        {
            return _threads.TrueForAll(thread => thread.IsAlive);
        }

        public void Enqueue<TResult>(IMyTask<TResult> a)
        {
            lock (_disposeLock)
            {
                if (IsDisposed)
                {
                    throw new AggregateException("Disposed already");
                }

                // enqueue all tasks that not enqueued 
                if (a.IsInThreadPool) return;
                //inc c add.Reset()
                var list = new LinkedList<ITask>();
                list.AddFirst(a);
                var tmp = list.First.Value.PreviousTask;
                while (tmp != null && !tmp.IsInThreadPool)
                {
                    list.AddFirst(tmp);
                    tmp = tmp.PreviousTask;
                }

                var current = list.First;

                while (current != null)
                {
                    current.Value.IsInThreadPool = true;
                    _jobQueue.Add(current.Value);
                    current = current.Next;
                }
            }
        }

        public void Dispose()
        {
            lock (_disposeLock)
            {
                if (IsDisposed) return;

                IsDisposed = true;
                //notify all workers that no more task would added to queue
                _jobQueue.CompleteAdding();

                foreach (var thread in _threads)
                {
                    thread.Join();
                }

                _jobQueue.Dispose();
                GC.SuppressFinalize(this);
            }
        }
    }
}