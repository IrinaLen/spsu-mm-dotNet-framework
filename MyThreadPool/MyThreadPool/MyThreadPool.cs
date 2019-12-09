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

        internal List<Thread> _threads = new List<Thread>();

        
        private readonly ReaderWriterLockSlim _readerWriterLock = new ReaderWriterLockSlim();

        
        private readonly object _disposeLock = new object();

        private volatile bool _isDisposedInsideWorker;

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
                catch (Exception ex)
                {
                    if (ex is ObjectDisposedException)
                    {
                        return;
                    }

                    if (ex is InvalidOperationException)
                    {
                        // now queue is empty and complete -> need to clear
                        //lock and only one object claer
                        if (_isDisposedInsideWorker) return;
                        lock (_disposeLock)
                        {
                            if (_isDisposedInsideWorker) return;
                            _isDisposedInsideWorker = true;
                        }

                        return;
                    }
                }
            }
        }

        public bool IsAllWorkersAlive()
        {
            return _threads.TrueForAll(thread => thread.IsAlive);
        }

        public void Enqueue<TResult>(IMyTask<TResult> a)
        {
            _readerWriterLock.EnterReadLock();

            if (IsDisposed)
            {
                _readerWriterLock.ExitReadLock();
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

            _readerWriterLock.ExitReadLock();
        }

        public void Dispose()
        {
            if (IsDisposed) return;
            _readerWriterLock.EnterWriteLock();
            if (IsDisposed)
            {
                _readerWriterLock.ExitWriteLock();
                return;
            }

            IsDisposed = true;
            //notify all workers that no more task would added to queue
            _jobQueue.CompleteAdding();
            _readerWriterLock.ExitWriteLock();
        }
    }
}