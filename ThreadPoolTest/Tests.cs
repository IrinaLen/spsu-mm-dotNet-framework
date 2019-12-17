using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ThreadPool;

namespace ThreadPoolTest
{
    [TestClass]
    public class Tests
    {
        private const int SimpleResult = 42;

        private int SimpleFunction()
        {
            Thread.Sleep(3);
            return SimpleResult;
        }

        [TestMethod]
        public void OneTask()
        {
            var threadPool = new ThreadPool.ThreadPool(1);
            IMyTask<int> task = new MyTask<int>(SimpleFunction);
            threadPool.Enqueue(task);
            Assert.IsFalse(task.IsCompleted);
            Assert.AreEqual(SimpleResult, task.Result);
            Assert.IsTrue(task.IsCompleted);
            threadPool.Dispose();
        }

        [TestMethod]
        public void TestException()
        {
            var threadPool = new ThreadPool.ThreadPool(1);
            var task = new MyTask<int>(() => { throw new NotImplementedException(); });
            threadPool.Enqueue(task);
            Assert.ThrowsException<AggregateException>(() =>
            {
                var result = task.Result;
            });
            threadPool.Dispose();
        }

        [TestMethod]
        public void ManyTasks()
        {
            int threadsNumber = 4;
            int tasksNumber = 40;
            var threadPool = new ThreadPool.ThreadPool(threadsNumber);
            var tasks = new List<IMyTask<int>>();
            for (int i = 0; i < tasksNumber; i++)
            {
                tasks.Add(new MyTask<int>(SimpleFunction));
                threadPool.Enqueue(tasks.Last());
            }

            foreach (var task in tasks)
            {
                Assert.AreEqual(SimpleResult, task.Result);
            }

            threadPool.Dispose();
        }

        [TestMethod]
        public void DisposedTest()
        {
            var threadPool = new ThreadPool.ThreadPool(1);
            var taskToFinish = new MyTask<int>(SimpleFunction);
            var taskToReject = new MyTask<int>(SimpleFunction);
            threadPool.Enqueue(taskToFinish);
            threadPool.Dispose();

            Assert.AreEqual(SimpleResult, taskToFinish.Result);
            Assert.ThrowsException<InvalidOperationException>(() => threadPool.Enqueue(taskToReject));
        }

        [TestMethod]
        public void ContinueWith()
        {
            var threadPool = new ThreadPool.ThreadPool(1);
            var tasks = new List<IMyTask<int>>();
            tasks.Add(new MyTask<int>(SimpleFunction));
            threadPool.Enqueue(tasks.Last());
            for (int i = 0; i < 4; i++)
            {
                var newTask = tasks.Last().ContinueWith((number) => number + 1);
                tasks.Add(newTask);
                threadPool.Enqueue(tasks.Last());
            }

            Assert.AreEqual(SimpleResult + 4, tasks.Last().Result);
            threadPool.Dispose();
        }

        [TestMethod]
        public void NumberOfThreadsTest()
        {
            int threadsNumber = 20;
            int tasksNumber = 100;
            var threadPool = new ThreadPool.ThreadPool(threadsNumber);
            var tasks = new List<IMyTask<int>>();
            for (int i = 0; i < tasksNumber; i++)
            {
                tasks.Add(new MyTask<int>(() =>
                {
                    Thread.Sleep(3);
                    return Thread.CurrentThread.ManagedThreadId;
                }));
                threadPool.Enqueue(tasks.Last());
            }

            var threadIds = new HashSet<int>();
            foreach (var task in tasks)
            {
                threadIds.Add(task.Result);
            }

            Assert.AreEqual(threadsNumber, threadIds.Count);

            threadPool.Dispose();
        }
    }
}