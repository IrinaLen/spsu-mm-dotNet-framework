using System;

namespace CherepanovThreadpool
{
    public interface IMyTask<TResult> : ITask
    {
        bool IsCompleted { get; }
        TResult Result { get; }
        IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> f);
    }
}
