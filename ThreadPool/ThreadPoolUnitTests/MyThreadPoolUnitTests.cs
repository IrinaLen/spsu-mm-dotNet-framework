using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThreadPool;

namespace ThreadPoolUnitTests
{
    [TestClass]
    public class MyThreadPoolUnitTests
    {
        readonly int threadPoolSize = 4;

        [TestMethod]
        public void AddOneTask()
        {
            var threadPool = new MyThreadPool(threadPoolSize);
            var task = new MyTask<int>(() => 24 + 20);
            threadPool.Enqueue(task);
            
            Assert.AreEqual(44, task.Result);
            
            threadPool.Dispose();
            task.Dispose();
        }

        [TestMethod]
        public void AddManyTasks()
        {
            var threadPool = new MyThreadPool(threadPoolSize);
            List<MyTask<int>> tasks = new List<MyTask<int>>();

            for (int i = 0; i < 10; i++)
            {
                var task = new MyTask<int>(() =>
         
                    56 * 2 * 1
                );
                tasks.Add(task);
                threadPool.Enqueue(task);
            }
            foreach (var task in tasks)
            {
                Assert.AreEqual(task.Result, 112);
                task.Dispose();

            }
            threadPool.Dispose();

        }

        [TestMethod]
        public void ManyThreads()
        {
            var threadPoolSize = 500;
            var tasksCount = 10;
            var threadPool = new MyThreadPool(threadPoolSize);
            List<MyTask<int>> tasks = new List<MyTask<int>>();
            
            for (var i = 0; i < tasksCount; i++)
                {
                    var task = new MyTask<int>(() =>
                    {
                        Thread.Sleep(1000);
                        return 2 + 2;
                    });
                tasks.Add(task);

                threadPool.Enqueue(task);                }
                foreach (var task in tasks)
                {
                    Assert.AreEqual(4, task.Result);
                    task.Dispose();
                }
           
        }

        [TestMethod]
        public void FullContinueWith()
        {
            var threadPool = new MyThreadPool(threadPoolSize);

            MyTask<string> taskA = new MyTask<string>(() => "A");

            var taskB = taskA.ContinueWith(a =>
            {
                Thread.Sleep(1000);
                return $"{a}B";
            });

            var taskC = taskB.ContinueWith(ab =>
            {
               Thread.Sleep(2000);
                return $"{ab}C";
            });

            var taskD = taskB.ContinueWith(ab =>
            {
                Thread.Sleep(1000);
                return $"{ab}D";
            });

            var taskE = taskA.ContinueWith(a =>
            {
                Thread.Sleep(1000);
                return $"{a}E";
            });
            
            threadPool.Enqueue((MyTask<string>)taskD);
            threadPool.Enqueue((MyTask<string>)taskB);
            threadPool.Enqueue((MyTask<string>)taskA);
            threadPool.Enqueue((MyTask<string>)taskC);
            threadPool.Enqueue((MyTask<string>)taskE);

            Thread.Sleep(3000);

            Assert.AreEqual("A", taskA.Result);
            Assert.AreEqual("AB", taskB.Result);
            Assert.AreEqual("ABC", taskC.Result);
            Assert.AreEqual("ABD", taskD.Result);
            Assert.AreEqual("AE", taskE.Result);

            taskA.Dispose();
            taskB.Dispose();
            taskC.Dispose();
            taskD.Dispose();
            taskE.Dispose();
            threadPool.Dispose();
        }

        [TestMethod]
        public void ThreadsNumber()
        {
            var threadPool = new MyThreadPool(4);
            Assert.AreEqual(4, threadPool.threads.Length);

            var task = new MyTask<int>(() => 24 + 20);
            threadPool.Enqueue(task);

            Assert.AreEqual(4, threadPool.threads.Length);

            threadPool.Dispose();
            task.Dispose();
        }

        [TestMethod]
        public void IsCompleted()
        {
            var threadPool = new MyThreadPool(threadPoolSize);
            var task = new MyTask<int>(() => 10 + 14);

            Assert.AreEqual(false, task.IsCompleted);
            
            threadPool.Enqueue(task);
            Thread.Sleep(1000);

            Assert.AreEqual(24, task.Result);
            Assert.AreEqual(true, task.IsCompleted);

            threadPool.Dispose();
            task.Dispose();
        }
    }
}
