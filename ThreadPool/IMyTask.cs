using System;

namespace ThreadPool
{
    public interface IMyTaskGeneral
    {
        bool IsCompleted { get; }
        void Execute();
    }

    public interface IMyTask<TResult> : IMyTaskGeneral
    {
        TResult Result { get; }
        IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> func);
    }
}