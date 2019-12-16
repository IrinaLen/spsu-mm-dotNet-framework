using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThreadPool;
using ThreadPool.MyTask;

namespace ThreadPoolTest
{
  [TestClass]
  public class Tests
  {
    private const int ThreadPoolSize = 4;

    [TestMethod]
    public void AddOneTaskTest()
    {
      using (var tp = new MyThreadPool(ThreadPoolSize))
      {
        using (var task = new MyTask<int>(() => 2 + 2))
        {
          tp.Enqueue(task);
          Assert.AreEqual(4, task.Result);
        }
      }
    }

    [TestMethod]
    public void RunLotsOfThreadsTest()
    {
      var tpSize = 42;
      var tasksCount = 10;
      using (var tp = new MyThreadPool(tpSize))
      {
        List<IMyTask<int>> tasks = new List<IMyTask<int>>();
        for (var i = 0; i < tasksCount; i++)
        {
          var task = new MyTask<int>(() =>
          {
            Thread.Sleep(100);
            return 2 + 2;
          });
          tp.Enqueue(task);
          tasks.Add(task);
        }
        foreach (var task in tasks)
        {
          Assert.AreEqual(4, task.Result);
          task.Dispose();
        }
      }
    }

    [TestMethod]
    public void DisposeThreadPoolTwiceTest()
    {
      var tp = new MyThreadPool(ThreadPoolSize);
      tp.Dispose();
      Assert.ThrowsException<ObjectDisposedException>(() => tp.Dispose());
    }

    [TestMethod]
    public void AddNullTaskTest()
    {
      using (var tp = new MyThreadPool(ThreadPoolSize))
      {
        Assert.ThrowsException<ArgumentNullException>(() => tp.Enqueue((IMyTask<int>)null));
      }
    }

    [TestMethod]
    public void AddFailedTaskTest()
    {
      using (var tp = new MyThreadPool(ThreadPoolSize))
      {
        using (var task = new MyTask<int>(() =>
        {
          IMyTask<int> t = null;
          return t.Result;
        }))
        {
          tp.Enqueue(task);
          var exception = Assert.ThrowsException<AggregateException>(() =>
          {
            var taskResult = task.Result;
          });
          Assert.IsInstanceOfType(exception.InnerException, typeof(NullReferenceException));
        }
      }
    }

    [TestMethod]
    public void AddMoreTasksThanThreadPoolSizeTest()
    {
      const int tasksCount = 8;
      using (var tp = new MyThreadPool(ThreadPoolSize))
      {
        var tasks = new List<MyTask<int>>();
        for (var i = 0; i < tasksCount; ++i)
        {
          var task = new MyTask<int>(() =>
          {
            Thread.Sleep(200);
            return 2 + 2;
          });
          tp.Enqueue(task);
          tasks.Add(task);
        }

        tasks.ForEach(task =>
        {
          Assert.AreEqual(4, task.Result);
          task.Dispose();
        });
      }
    }

    [TestMethod]
    public void AddTasksInParallelTest()
    {
      const int parallelThreadsCount = 40;
      using (var tp = new MyThreadPool(ThreadPoolSize))
      {
        var threads = new List<Thread>();
        for (var i = 0; i < parallelThreadsCount; i++)
        {
          var thread = new Thread(() =>
          {
            var j = i;
            using (var task = new MyTask<int>(() => j))
            {
              tp.Enqueue(task);
              Assert.AreEqual(j, task.Result);
            }
          });
          threads.Add(thread);
          thread.Start();
        }

        threads.ForEach(thread => thread.Join());
      }
    }

    [TestMethod]
    public void AddTasksAndCheckThreadsCountTest()
    {
      using (var tp = new MyThreadPool(ThreadPoolSize))
      {
        using (var task1 = new MyTask<int>(() =>
        {
          Thread.Sleep(500);
          return 2 + 2;
        }))
        using (var task2 = new MyTask<int>(() =>
        {
          Thread.Sleep(500);
          return 2 + 2;
        }))
        {
          tp.Enqueue(task1);
          tp.Enqueue(task2);
          Assert.AreEqual(ThreadPoolSize, tp.Size);
        }
      }
    }

    [TestMethod]
    public void AddTasksAndDisposeThreadPoolTest()
    {
      // IMyTask<int> task1, task2;
      using (var tp = new MyThreadPool(ThreadPoolSize))
      {
        var task1 = new MyTask<int>(() =>
        {
          Thread.Sleep(500);
          return 2 + 2;
        });
        var task2 = new MyTask<int>(() =>
        {
          Thread.Sleep(500);
          return 2 + 2;
        });
        tp.Enqueue(task1);
        tp.Enqueue(task2);
        Assert.AreEqual(4, task1.Result);
        Assert.AreEqual(4, task2.Result);
        task1.Dispose();
        task2.Dispose();
      }
    }

    [TestMethod]
    public void EnqueueIntoDisposedThreadPoolTest()
    {
      var tp = new MyThreadPool(ThreadPoolSize);
      var task1 = new MyTask<int>(() =>
      {
        Thread.Sleep(500);
        return 2 + 2;
      });
      tp.Enqueue(task1);
      tp.Dispose();
      using (var task2 = new MyTask<int>(() =>
      {
        Thread.Sleep(500);
        return 2 + 2;
      }))
      {
        Assert.ThrowsException<ObjectDisposedException>(() => tp.Enqueue(task2));
      }

      task1.Dispose();
    }

    [TestMethod]
    public void ContinueWithTest()
    {
      using (var tp = new MyThreadPool(ThreadPoolSize))
      {
        using (var taskA = new MyTask<string>(() => "A"))
        using (var taskB = taskA.ContinueWith(a =>
        {
          Thread.Sleep(2000);
          return $"{a}B";
        }))
        using (var taskC = taskB.ContinueWith(ab =>
        {
          Thread.Sleep(1000);
          return $"{ab}C";
        }))
        using (var taskD = taskB.ContinueWith(ab =>
        {
          Thread.Sleep(1000);
          return $"{ab}D";
        }))
        {
          tp.Enqueue(taskD);
          tp.Enqueue(taskC);
          tp.Enqueue(taskB);
          tp.Enqueue(taskA);
          Assert.AreEqual("ABC", taskC.Result);
          Assert.AreEqual("ABD", taskD.Result);
        }
      }
    }
  }
}
