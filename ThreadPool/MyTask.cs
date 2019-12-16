using System;
using System.Threading;

namespace ThreadPool
{
    public class MyTask<TResult> : IMyTask<TResult>
    {
        ManualResetEvent _manualResetEvent = new ManualResetEvent(false);

        private Func<TResult> _function;

        private AggregateException _exception = null;

        private TResult _result;

        public MyTask(Func<TResult> func)
        {
            _function = func;
        }

        public bool IsCompleted { get; private set; }


        public TResult Result
        {
            get
            {
                _manualResetEvent.WaitOne();
                lock (this)
                {
                    if (_exception != null)
                    {
                        throw _exception;
                    }

                    return _result;
                }
            }
        }

        public IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> continueWithFunction)
        {
            return new MyTask<TNewResult>(() => continueWithFunction(Result));
        }

        public void Execute()
        {
            lock (this)
            {
                try
                {
                    _result = _function.Invoke();
                }
                catch (Exception e)
                {
                    _exception = new AggregateException("Task has thrown an exception", e);
                }

                IsCompleted = true;
                _manualResetEvent.Set();
            }
        }
    }
}