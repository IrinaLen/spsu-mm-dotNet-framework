using System;
using System.Threading;

namespace CherepanovThreadpool
{
    public class MyTask<TResult> : IMyTask<TResult>
    {
        private readonly ManualResetEvent _manualResetEvent = new ManualResetEvent(false);
        private readonly Object _executionLock = new object();
        private TResult _result;
        private Exception _exception;
        private bool _isCompleted = false;
        private bool _isInThreadpool = false;
        private ITask _prevTask = null;
        private bool _threadpoolIsDisposed = false;
        private readonly Func<TResult> _func;

        public TResult Result
        {
            get
            {
                if (!IsInThreadpool)
                {
                    throw new AggregateException("Task not in ThreadPool");
                }
                _manualResetEvent.WaitOne();
                lock (_executionLock)
                {
                    if (_exception != null) throw new AggregateException("Task failed", _exception);
                    return _result;
                }
            }
            private set
            {
                _result = value;
            }
        }

        public bool IsCompleted
        {
            get => _isCompleted;
            set => _isCompleted = value;
        }
        public bool IsInThreadpool
        {
            get => _isInThreadpool;
            set => _isInThreadpool = value;
        }

        public ITask PrevTask
        {
            get => _prevTask;
            set => _prevTask = value;
        }

        public bool ThreadpoolIsDisposed
        {
            get => _threadpoolIsDisposed;
            set => _threadpoolIsDisposed = value;
        }

        public MyTask(Func<TResult> f)
        {
            _func = f;
        }

        public MyTask(Func<TResult> f, ITask prevTask)
        {
            _func = f;
            PrevTask = prevTask;
        }

        public void Exec()
        {
            lock (_executionLock)
            {
                try
                {
                    if (PrevTask != null && !PrevTask.IsInThreadpool)
                    {
                        _exception = new AggregateException("Previous task is not in ThreadPool");
                        return;
                    }
                    IsCompleted = true;
                    Result = _func.Invoke();

                }
                catch (Exception e)
                {
                    _exception = e;
                }
                finally
                {
                    _manualResetEvent.Set();
                }
            }
        }

        public IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> f)
        {
            return new MyTask<TNewResult>(() => f.Invoke(Result), this);
        }
    }
}
