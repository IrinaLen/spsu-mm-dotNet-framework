using System;
using System.Collections.Generic;
using System.Threading;

namespace ThreadPool
{
    public class MyTask<TResult>: IMyTask<TResult>
    {
        private Boolean isCompleted;
        private TResult result;
        private readonly Func<TResult> function;
        private readonly List<Exception> caughtExceptions = new List<Exception>();
        private readonly ManualResetEvent _ready = new ManualResetEvent(false);

        public MyTask(Func<TResult> func)
        {
            function = func;
            isCompleted = false;
        }
        public TResult Result
        {
            get
            {
                _ready.WaitOne();
                if (caughtExceptions.Count != 0)
                {
                    throw new AggregateException(caughtExceptions);
                }

                return result;
            }
        }

        public Boolean IsCompleted
        {
            get
            {
                return isCompleted;
            }
        }

        public void Execute()
        {
            try
            {
                result = function.Invoke();
            }
            catch (AggregateException ae)
            {
                foreach (Exception e in ae.InnerExceptions)
                {
                    caughtExceptions.Add(e);
                }
            }
            catch (Exception e)
            {
                caughtExceptions.Add(e);
            }
            finally
            {
                _ready.Set();
                isCompleted = true;
            }
        }

        public IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> continuation)
        {
            return new MyTask<TNewResult>(() =>
            {
                var prevResult = Result;
                return continuation.Invoke(prevResult);
            });
        }

        public void Dispose()
        {
            _ready?.Dispose();
        }
    }
}
