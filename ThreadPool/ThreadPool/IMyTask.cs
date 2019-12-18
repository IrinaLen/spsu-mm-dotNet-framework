using System;

namespace ThreadPool
{
    public interface IMyTask<out TResult>: IDisposable
    {
        TResult Result { get; }
        Boolean IsCompleted { get; }
        void Execute();
        IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> continuation);
    }
}
