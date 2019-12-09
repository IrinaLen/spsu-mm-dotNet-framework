using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using MyThreadPool;

namespace MyThreadPoolTest
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void AddOneTaskTest()
        {
            var threadPool = new MyThreadPool.MyThreadPool(1);
            var task = new MyTask<int>(() => 5);
            threadPool.Enqueue(task);
            Assert.AreEqual(5, task.Result);
            threadPool.Dispose();
        }

        [Test]
        public void AddSeveralTaskParallelTest()
        {
            int activeThreads = 40;
            int pendingMs = 100;
            var threads = new List<Thread>();
            var threadPool = new MyThreadPool.MyThreadPool(3);
            for (int i = 0; i < activeThreads; i++)
            {
                var thread = new Thread(() =>
                {
                    var task1 = new MyTask<int>(() =>
                    {
                        Thread.Sleep(pendingMs);
                        return 1;
                    });
                    var task2 = new MyTask<float>(() =>
                    {
                        Thread.Sleep(pendingMs);
                        return 3;
                    });
                    var task3 = new MyTask<float>(() =>
                    {
                        Thread.Sleep(pendingMs);
                        return 4;
                    });
                    threadPool.Enqueue(task1);
                    threadPool.Enqueue(task2);
                    threadPool.Enqueue(task3);
                    Assert.AreEqual(1, task1.Result);
                    Assert.AreEqual(3, task2.Result);
                    Assert.AreEqual(4, task3.Result);
                });
                threads.Add(thread);
                thread.Start();
            }

            threads.ForEach(thread => thread.Join());
            threadPool.Dispose();
        }

        [Test]
        public void ContinueWithTest()
        {
            var threadPool = new MyThreadPool.MyThreadPool(3);
            var task1 = new MyTask<string>(() =>
            {
                Thread.Sleep(1500);
                return "Kotlin is Awesome! What do you think?";
            });
            var task2 = task1.ContinueWith(question =>
            {
                Thread.Sleep(2000);
                return "Yeap, better then Java at least";
            });
            var task3 = task2.ContinueWith(answer =>
            {
                Thread.Sleep(1000);
                return "Good!";
            });
            var task4 = task2.ContinueWith(answer =>
            {
                Thread.Sleep(1000);
                return "That a lie!!!";
            });
            threadPool.Enqueue(task3);
            threadPool.Enqueue(task4);
            Assert.AreEqual("Good!", task3.Result);
            Assert.AreEqual("That a lie!!!", task4.Result);
            threadPool.Dispose();
        }

        [Test]
        public void ContinueWithCheckAllThreadAliveTest()
        {
            int activeThreads = 10;
            int numWorkers = 100;
            int pendingMs = 10;
            var threads = new List<Thread>();
            var threadPool = new MyThreadPool.MyThreadPool(numWorkers);
            for (int i = 0; i < activeThreads; i++)
            {
                var thread = new Thread(() =>
                {
                    var task1 = new MyTask<int>(() =>
                    {
                        Thread.Sleep(pendingMs);
                        return 1;
                    });
                    var task2 = task1.ContinueWith(_ => 3);
                    var task3 = new MyTask<float>(() =>
                    {
                        Thread.Sleep(pendingMs);
                        return 4;
                    });
                    threadPool.Enqueue(task1);
                    threadPool.Enqueue(task2);
                    threadPool.Enqueue(task3);
                    Assert.AreEqual(1, task1.Result);
                    Assert.AreEqual(true, threadPool.IsAllWorkersAlive());
                    Assert.AreEqual(3, task2.Result);
                    Assert.AreEqual(true, threadPool.IsAllWorkersAlive());
                    Assert.AreEqual(4, task3.Result);
                    Assert.AreEqual(true, threadPool.IsAllWorkersAlive());
                });
                threads.Add(thread);
                thread.Start();
            }

            threads.ForEach(thread =>
            {
                Assert.AreEqual(true, threadPool.IsAllWorkersAlive());
                thread.Join();
            });
            Assert.AreEqual(true, threadPool.IsAllWorkersAlive());
            threadPool.Dispose();
        }
    }
}