using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using ThreadPool.MyTask;

namespace ThreadPool
{
  public sealed class MyThreadPool : IDisposable
  {
    private const int DefaultPoolSize = 4;
    private readonly BlockingCollection<Action> _waitingTasks = new BlockingCollection<Action>();
    private readonly Thread[] _threads;
    private bool _isDisposed = false;
    private readonly CancellationTokenSource _cts = new CancellationTokenSource();

    private readonly object _disposeLockObj = new object();

    public int Size => _threads.Count(thread => thread.IsAlive);

    public MyThreadPool(int poolSize)
    {
      _threads = new Thread[poolSize];
      for (var i = 0; i < poolSize; ++i)
      {
        _threads[i] = new Thread(() => ThreadWork(_cts.Token));
        _threads[i].Start();
      }
    }

    public MyThreadPool() : this(DefaultPoolSize)
    {
    }

    public void Enqueue<TResult>(IMyTask<TResult> task)
    {
      if (task == null)
      {
        throw new ArgumentNullException(nameof(task), "Cannot run null in ThreadPool");
      }
      lock (_disposeLockObj)
      {
        if (_isDisposed)
        {
          throw new ObjectDisposedException("Cannot add task into ThreadPool because it's disposed");
        }
        _waitingTasks.Add(task.Run);
      }
    }

    public void Dispose()
    {
      lock (_disposeLockObj)
      {
        if (_isDisposed)
        {
          throw new ObjectDisposedException("Cannot dispose ThreadPool because it's already disposed");
        }
        _isDisposed = true;
        _cts.Cancel();
        _waitingTasks.CompleteAdding();
        foreach (var thread in _threads)
        {
          thread.Join();
        }
        _waitingTasks.Dispose();
        _cts.Dispose();
      }
    }

    private void ThreadWork(CancellationToken ct)
    {
      while (true)
      {
        try
        {
          var task = _waitingTasks.Take(ct);
          task.Invoke();
        }
        catch (OperationCanceledException)
        {
          break;
        }
        catch (Exception)
        {
          // ignored
        }
      }
      foreach (var task in _waitingTasks.GetConsumingEnumerable())
      {
        task.Invoke();
      }
    }
  }
}
