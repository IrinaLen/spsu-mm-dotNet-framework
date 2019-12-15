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
}