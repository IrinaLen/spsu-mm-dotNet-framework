using System;
using System.Threading;

namespace ThreadPool
{
    public interface IMyTask<TResult>: IDisposable
    {
        bool IsCompleted { get; }

        TResult Result { get; }

        void Execute();

        IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> func);
    }

    public class MyTask<TResult> : IMyTask<TResult>
    {
        private bool _disposed;
        private TResult _result;
        private Exception _exception;
        private readonly Func<TResult> _func;
        
        private readonly object _lock = new object();
        private readonly object _disposeLock = new object();
        private readonly ManualResetEvent _mre = new ManualResetEvent(false);
        
        private MyTask(Func<TResult> func)
        {
            _func = func ?? throw new ArgumentNullException(nameof(func), "Illegal null func");
        }

        public static IMyTask<TResult> New(Func<TResult> func) => new MyTask<TResult>(func);

        public bool IsCompleted { get; private set; }

        public TResult Result
        {
            get
            {
                lock (_disposeLock)
                {
                    if (!_disposed)
                    {
                        _mre.WaitOne(); // wait for result of execution
                    }
                }

                lock (_lock)
                {
                    if (_exception != null)
                    {
                        throw new AggregateException("Task failed", _exception);
                    }

                    CheckDisposedBeforeCompleted("Unable to get result of task");
                    return _result;
                }
            }
        }

        public void Execute()
        {
            lock (_lock)
            {
                if (IsCompleted)
                {
                    return;
                }

                try
                {
                    CheckDisposedBeforeCompleted("Unable to execute task");
                    _result = _func();
                }
                catch (Exception e)
                {
                    _exception = e;
                }
                finally
                {
                    IsCompleted = true;
                    _mre.Set(); // allow to get result or exception of execution
                }
            }
        }

        public IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func), "Illegal null func");   
            }
            CheckDisposedBeforeCompleted("Unable to create chained task");
            return new MyTask<TNewResult>(() => func(Result));
        }

        public void Dispose()
        {
            lock (_lock)
            lock (_disposeLock)
            {
                if (_disposed)
                {
                    return;
                }
                _mre.Dispose();
                _disposed = true;
            }
        }

        private void CheckDisposedBeforeCompleted(string message)
        {
            // we can't get result of execution if task has been disposed before completion
            if (!IsCompleted && _disposed)
            {
                throw new ObjectDisposedException(nameof(_mre), $"Task has been disposed but not completed. {message}");
            }
        }
    }
}