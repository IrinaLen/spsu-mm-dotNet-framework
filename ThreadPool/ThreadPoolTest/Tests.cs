using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using ThreadPool;
using ThreadPool.MyTask;

namespace ThreadPoolTest
{
  [TestFixture]
  public class ThreadPoolTests
  {
    [Test]
    public void AddOneTaskTest()
    {
      var threadPoolSize = 4;
      using (var tp = new MyThreadPool(threadPoolSize))
      {
        IMyTask<int> task = new MyTask<int>(() => 2 + 2);
        tp.Enqueue(task);
        Assert.AreEqual(4, task.Result);
      }
    }
    
    [Test]
    public void AddMoreTasksThanThreadPoolSize()
    {
      var threadPoolSize = 4;
      var tasksCount = 8;
      using (var tp = new MyThreadPool(threadPoolSize))
      {
        var tasks = new List<IMyTask<int>>();
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
        tasks.ForEach(task => Assert.AreEqual(4, task.Result));
      }
    }

    [Test]
    public void AddSeveralTaskInParallelTest()
    {
      var threadPoolSize = 4;
      var parallelThreadsCount = 40;
      using (var tp = new MyThreadPool(threadPoolSize))
      {
        var threads = new List<Thread>();
        for (var i = 0; i < parallelThreadsCount; i++)
        {
          var thread = new Thread(() =>
          {
            var j = i;
            var task1 = new MyTask<int>(() => j);
            tp.Enqueue(task1);
            Assert.AreEqual(j, task1.Result);
          });
          threads.Add(thread);
          thread.Start();
        }

        threads.ForEach(thread => thread.Join());
      }
    }

    [Test]
    public void AddSeveralTaskCheckThreadsCount()
    {
      var threadPoolSize = 4;
      var parallelThreadsCount = 40;
      using (var tp = new MyThreadPool(threadPoolSize))
      {
        var threads = new List<Thread>();
        for (var i = 0; i < parallelThreadsCount; i++)
        {
          var thread = new Thread(() =>
          {
            var j = i;
            var task1 = new MyTask<int>(() =>
            {
              Thread.Sleep(100);
              return j;
            });
            tp.Enqueue(task1);
            Assert.AreEqual(j, task1.Result);
            Assert.AreEqual(threadPoolSize, tp.Size);
            Assert.AreEqual(threadPoolSize, tp.AliveCount);
          });
          threads.Add(thread);
          thread.Start();
        }

        threads.ForEach(thread => thread.Join());
      }
    }

    [Test]
    public void ContinueWithTest()
    {
      var threadPoolSize = 4;
      using (var tp = new MyThreadPool(threadPoolSize))
      {
        var questionTask = new MyTask<string>(() => "Kotlin is Awesome! What do you think?");
        var answerTask = questionTask.ContinueWith(question =>
        {
          Thread.Sleep(2000);
          return $"{question} - Yeap, better then Java at least";
        });
        var resultTask1 = answerTask.ContinueWith(answer =>
        {
          Thread.Sleep(1000);
          return $"{answer} - Good!";
        });
        var resultTask2 = answerTask.ContinueWith(answer =>
        {
          Thread.Sleep(1000);
          return $"{answer} - That a lie!!!";
        });
        tp.Enqueue(resultTask2);
        tp.Enqueue(resultTask1);
        tp.Enqueue(answerTask);
        tp.Enqueue(questionTask);
        Assert.AreEqual("Kotlin is Awesome! What do you think? - Yeap, better then Java at least - Good!", resultTask1.Result);
        Assert.AreEqual("Kotlin is Awesome! What do you think? - Yeap, better then Java at least - That a lie!!!", resultTask2.Result);
      }
    }
  }
}
