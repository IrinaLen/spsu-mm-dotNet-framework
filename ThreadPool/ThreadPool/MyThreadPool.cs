using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ThreadPool.Exceptions;
using ThreadPool.MyTask;

namespace ThreadPool
{
  public sealed class MyThreadPool : IDisposable
  {
    private const int DefaultPoolSize = 4;
    private readonly EmptyPoolPolitics _emptyPoolPolitics;
    private readonly ConcurrentDictionary<int, Worker> _allWorkers = new ConcurrentDictionary<int, Worker>();
    private readonly ConcurrentQueue<Worker> _waitingWorkers = new ConcurrentQueue<Worker>();
    private readonly ConcurrentQueue<Action> _waitingTasks = new ConcurrentQueue<Action>();
    private volatile int _isDisposed = 0;
    private volatile int _size = 0;
    
    public int Size => _size;

    public int AliveCount => _allWorkers.Count(pair => pair.Value.Thread.IsAlive);
    
    public MyThreadPool(int poolSize, EmptyPoolPolitics emptyPoolPolitics = EmptyPoolPolitics.Wait)
    {
      _emptyPoolPolitics = emptyPoolPolitics;
      for (int i = 0; i < poolSize; i++)
      {
        AddWorker(null);
      }
    }

    public MyThreadPool(EmptyPoolPolitics emptyPoolPolitics = EmptyPoolPolitics.Wait) : this(DefaultPoolSize,
      emptyPoolPolitics)
    {
    }

    public void Enqueue<TResult>(IMyTask<TResult> task)
    {
      if (_isDisposed > 0)
      {
        throw new ThreadPoolDisposedException();
      }
      _waitingTasks.Enqueue(task.Run);
      if (_waitingWorkers.TryDequeue(out var worker))
      {
        TrySetNextTask(worker);
      }
      else
      {
        switch (_emptyPoolPolitics)
        {
          case EmptyPoolPolitics.Wait:
            break;
          case EmptyPoolPolitics.Error:
            throw new ThreadPoolIsFullException();
          case EmptyPoolPolitics.Extend:
            if (_waitingTasks.TryDequeue(out var waitingTask))
            {
              AddWorker(waitingTask);
            }

            break;
        }
      }
    }

    public void Dispose()
    {
      Interlocked.CompareExchange(ref _isDisposed, 1, 0);
      while (!_waitingWorkers.IsEmpty)
      {
        if (_waitingWorkers.TryDequeue(out var worker))
        {
          worker.Stop();
        }
      }
    }

    private void AddWorker(Action task)
    {
      Interlocked.Increment(ref _size);
      var worker = new Worker(task, this);
      var workerThread = worker.Thread;
      workerThread.Start();
      if (task == null)
      {
        _waitingWorkers.Enqueue(worker);
      }

      if (!_allWorkers.TryAdd(worker.Thread.ManagedThreadId, worker))
      {
        throw new ThreadPoolCannotAddThread();
      }
    }

    private void TrySetNextTask(Worker worker)
    {
      if (_waitingTasks.TryDequeue(out var task))
      {
        if (_isDisposed == 0)
        {
          worker.Task = task;
        }
        else
        {
          worker.Stop();
        }
      }
      else
      {
        _waitingWorkers.Enqueue(worker);
      }
    }

    private class Worker
    {
      internal readonly Thread Thread;
      
      private MyThreadPool _context;
      private readonly EventWaitHandle _hasTask;

      internal Action Task
      {
        set
        {
          _task = value;
          _hasTask.Set();
        }
      }

      private volatile int _isStopped = 0;
      private Action _task;

      public Worker(Action firstTask, MyThreadPool context)
      {
        _context = context;
        _task = firstTask;
        Thread = new Thread(Run);
        _hasTask = new AutoResetEvent(firstTask != null);
      }

      private void Run()
      {
        while (true)
        {
          _hasTask.WaitOne();
          if (_isStopped > 0)
          {
            ClearResources();
            break;
          }

          _task.Invoke();
          _context.TrySetNextTask(this);
        }
      }

      internal void Stop()
      {
        Interlocked.Increment(ref _isStopped);
        _hasTask.Set();
      }

      private void ClearResources()
      {
        _hasTask.Dispose();
        _context = null;
        Task = null;
      }
    }
  }
}
