using System;
using System.Collections.Generic;
using System.Threading;

namespace ThreadPool.MyTask
{
  public class MyTask<TResult> : IMyTask<TResult>
  {
    private readonly List<Exception> _exceptions = new List<Exception>();
    private TResult _result;
    private readonly Func<TResult> _func;
    private readonly ManualResetEvent _ready = new ManualResetEvent(false);

    public TResult Result
    {
      get
      {
        _ready.WaitOne();
        if (_exceptions.Count > 0)
        {
          throw new AggregateException(_exceptions);
        }

        return _result;
      }
      internal set => _result = value;
    }

    public MyTask(Func<TResult> func)
    {
      _func = func;
    }

    public void Run()
    {
      try
      {
        _result = _func.Invoke();
      }
      catch (AggregateException ae)
      {
        foreach (Exception e in ae.InnerExceptions)
        {
          _exceptions.Add(e);
        }
      }
      catch (Exception e)
      {
        _exceptions.Add(e);
      }
      finally
      {
        _ready.Set();
      }
    }

    public IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> continuation)
    {
      return new MyTask<TNewResult>(() =>
      {
        var oldResult = this.Result;
        return continuation.Invoke(oldResult);
      });
    }
  }
}
