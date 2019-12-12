using System;

namespace ThreadPool.MyTask
{
  public interface IMyTask<out TResult> : IDisposable
  {
    TResult Result { get; }
    void Run();
    IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> continuation);
  }
}
