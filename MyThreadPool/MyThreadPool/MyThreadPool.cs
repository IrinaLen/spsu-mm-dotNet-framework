using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MyThreadPool
{
    public class MyThreadPool:IDisposable
    {
        private readonly List<Thread> _workers;

        private readonly object _disposeLocker = new object();
        private bool _isDisposed = false;
        
        private readonly BlockingCollection<Action> _bc = new BlockingCollection<Action>();
        
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private CancellationToken _ct;

        public int Count => _workers.Count(thread => thread.IsAlive);
        public int Capacity { get;}

        public MyThreadPool(int threadNum = 1)
        {
            if (threadNum <= 0) throw new ArgumentException("The number of threads should" +
                                                            " be greater of equal to 1");
            Capacity = threadNum;
            _ct = _cts.Token;
            _workers = new List<Thread>();
            for (var i = 0; i < threadNum; i++)
            {
                var worker = new Thread(() => Worker(_ct)) {IsBackground = true};
                worker.Start();
                _workers.Add(worker);
            }
            
        }

        private void Worker(CancellationToken token)
        {
            try
            {
                foreach (var job in _bc.GetConsumingEnumerable(token))
                {
                    job.Invoke();

                }
            }
            catch (OperationCanceledException)
            {
                //finish work
                foreach (var job in _bc.GetConsumingEnumerable())
                {
                    job.Invoke();

                }
            }
            
        }

        public void Enqueue<TResult>(IMyTask<TResult> task)
        {
            lock (_disposeLocker)
            {
                if (_isDisposed) throw new ObjectDisposedException("Thread pool has been disposed.");
                //thread-safe since only one task can be enqueued 
                if (task.IsCompleted || task.IsInThreadPool || task.IsFailed) return;
                //collect intermediate tasks
                var tasks = new LinkedList<IMyTaskBase>();
                IMyTaskBase current = task;
                while (current != null)
                {
                    tasks.AddFirst(current);
                    current = current.PreviousTask;
                }

                foreach (var t in tasks)
                {
                    if (!t.IsInThreadPool && !t.IsCompleted)
                    {
                        t.IsInThreadPool = true;
                        _bc.Add(t.Execute);
                    }
                }
                
            }
        }

        public void Dispose()
        {
            lock (_disposeLocker)
            {
                if (_isDisposed)
                {
                    return;
                }

                _isDisposed = true;
                _bc.CompleteAdding();
                _cts.Cancel();
                foreach (var w in _workers)
                {
                    w.Join();
                }
                _workers.Clear();
                _bc.Dispose();
                _cts.Dispose();
            }
        }
    }
}