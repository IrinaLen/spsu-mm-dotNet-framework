using System;
using System.Threading;

namespace MyThreadPool
{
    public class MyTask<TResult> : IMyTask<TResult>, IDisposable
    {
        public bool IsCompleted { get; private set; }
        public bool IsFailed { get; private set; }

        public Func<TResult> Job { get; }


        private bool IsInThreadPool { get; set; }

        bool ITask.IsInThreadPool
        {
            get => this.IsInThreadPool;
            set => this.IsInThreadPool = value;
        }

        private ITask PreviousTask { get; }

        ITask ITask.PreviousTask => this.PreviousTask;


        private readonly ManualResetEvent _manualResetEvent = new ManualResetEvent(false);


        private TResult _result;

        public TResult Result
        {
            get
            {
                if (!IsInThreadPool) throw new AggregateException("Task not in ThreadPool");

                _manualResetEvent.WaitOne();
                if (IsCompleted) return _result;
                if (IsFailed) throw _exception;

                throw new Exception("Impossible");
            }
            private set => _result = value;
        }


        private Exception _exception = null;

        private readonly object _executeLock = new object();


        public MyTask(Func<TResult> job)
        {
            IsCompleted = false;
            Job = job;
        }


        private MyTask(Func<TResult> job, ITask prev)
        {
            IsCompleted = false;
            Job = job;
            PreviousTask = prev;
        }


        public IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> func)
        {
            return new MyTask<TNewResult>(() => func.Invoke(Result), this);
        }


        private void _execute()
        {
            if (IsCompleted || IsFailed) return; // already computed
            try
            {
                Result = Job.Invoke();
                IsCompleted = true;
                _manualResetEvent.Set();
            }
            catch (Exception e)
            {
                _exception = new AggregateException("AggregateException", e);
                IsFailed = true;
                _manualResetEvent.Set();
            }
        }

        public void Execute()
        {
            if (!IsInThreadPool) throw new AggregateException("User could not explicitly call this method");

            lock (_executeLock)
            {
                if (IsCompleted || IsFailed) return; // already computed

                try
                {
                    Result = Job.Invoke();
                    IsCompleted = true;
                    _manualResetEvent.Set();
                }
                catch (Exception e)
                {
                    _exception = new AggregateException("AggregateException", e);
                    IsFailed = true;
                    _manualResetEvent.Set();
                }
            }
        }

        public void Dispose()
        {
            _manualResetEvent?.Dispose();
        }
    }
}