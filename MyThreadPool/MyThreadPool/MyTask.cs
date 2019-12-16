using System;
using System.Collections.Generic;
using System.Threading;

namespace MyThreadPool
{
    
    public class MyTask<TResult> : IMyTask<TResult>, IDisposable
    {
        private readonly ManualResetEvent _mre = new ManualResetEvent(false);
        private readonly object _locker = new object();
        private readonly List<Exception> _exceptions = new List<Exception>(); 
        private TResult _result;
        private readonly Func<TResult> _func; 
        public bool IsCompleted { get; private set; }
        public bool IsFailed { get; private set; }
        //user should not set this property
        public bool IsInThreadPool { get; set; }
        public IMyTaskBase PreviousTask { get;}

        public MyTask(Func<TResult> func)
        {
            _func = func ?? throw new ArgumentNullException(nameof(func),"Null function parameter.");
        }
        
        private MyTask(Func<TResult> func, IMyTaskBase prev):this(func)
        {
            PreviousTask = prev;
        }
        public TResult Result
        {
            get
            {
                if (!IsInThreadPool && !IsCompleted && !IsFailed)
                    throw new InvalidOperationException("Result should be invoked after enqueue.");
                _mre.WaitOne();
                if (_exceptions.Count > 0)
                {
                    IsFailed = true;
                    throw new AggregateException(_exceptions);
                }
                return _result;
            }
            private set => _result = value;
        }
        //the method is allowed to be invoked only inside a threadpool
        public void Execute()
        {
            lock (_locker)
            {
                if (!IsInThreadPool)
                    throw new InvalidOperationException($"User should never call {nameof(Execute)} explicitly.");

                try
                {
                    Result = _func();
                    IsCompleted = true;
                }
                catch (Exception e)
                {
                    _exceptions.Add(e);
                    IsFailed = true;
                }
                finally
                {
                    IsInThreadPool = false;
                    _mre.Set();
                }
            }

        }

        public IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> func)
        {
            if (func == null) throw  new ArgumentNullException(nameof(func),"Null function parameter.");
            return new MyTask<TNewResult>(() => func(Result),this);
        }

        //use only when mre is not needed anymore
        public void Dispose()
        {
            _mre?.Dispose();
        }
    }
}