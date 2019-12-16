using System;
using System.Collections.Generic;
using System.Threading;
using MyThreadPool;
using NUnit.Framework;

namespace MyThreadPoolTests
{
    [TestFixture]
    public class Tests
    {
        [Test]
        public void AddOneTaskTest()
        {
            var tp = new MyThreadPool.MyThreadPool(4);
            var task = new MyThreadPool.MyTask<int>(()=>4*10+2);
            tp.Enqueue(task);
            Assert.AreEqual(42,task.Result);
            tp.Dispose();
        }

        [Test]
        public void AddManyTasksTest()
        {
            var tp = new MyThreadPool.MyThreadPool(4);
            int[] l = new int[42];
            for (int i = 0; i < 42; i++)
            {
                var j = i;
                var task = new MyTask<int>(()=>
                {
                l[j] = 42;
                return 0;
                });
                tp.Enqueue(task);
            }
            tp.Dispose();
            Assert.True(new List<int>(l).TrueForAll((i)=>i == 42));
        }

        [Test]
        public void ChainedContinueWithTest()
        {
            var tp = new MyThreadPool.MyThreadPool(4);
            int[] l = new int[42];
            var task = new MyTask<int>(() =>
            {
                l[0] = 42;
                return 0;
            });
            var taskContinuation = task.ContinueWith(_ =>
            {
                l[1] = 42;
                return 0;
            });
            for (int i = 2; i < 42; i++)
            {
                var j = i;
                taskContinuation = taskContinuation.ContinueWith((_) =>
                {
                    l[j] = 42;
                    return 0;
                });
                
            }
            tp.Enqueue(taskContinuation);
            tp.Dispose();
            Assert.True(new List<int>(l).TrueForAll((i)=>i == 42));
        }

        [Test]
        public void TaskThrowsTest()
        {
            var tp = new MyThreadPool.MyThreadPool(3);
            var task1 = new MyTask<int>(()=>0);
            var task2 = task1.ContinueWith((i) => i / 0);
            var task3 = task2.ContinueWith((i) => 42);
            tp.Enqueue(task3);
            var exception = Assert.Throws<AggregateException>(() =>
            {
                var result = task3.Result;
            });
            Assert.IsInstanceOf(typeof(DivideByZeroException), exception.InnerException.InnerException);
            tp.Dispose();
        }

        [Test]
        public void NumberOfThreadsTest()
        {
            var tp = new MyThreadPool.MyThreadPool(5);
            var list = new List<MyTask<int>>();
            var task1 = new MyTask<int>(()=>
            {   Thread.Sleep(400);
                return 12;
            });
            var task2 = new MyTask<int>(()=>
            {   Thread.Sleep(400);
                return 22;
            });
            var task3 = new MyTask<int>(()=>
            {   Thread.Sleep(400);
                return 32;
            });
            var task4 = new MyTask<int>(()=>
            {   Thread.Sleep(400);
                return 42;
            });
            var task5 = new MyTask<int>(()=>
            {   Thread.Sleep(400);
                return 52;
            });
            var task6 = new MyTask<int>(()=>
            {   Thread.Sleep(400);
                return 62;
            });
            list.Add(task1);
            list.Add(task2);
            list.Add(task3);
            list.Add(task4);
            list.Add(task5);
            list.Add(task6);
            foreach (var t in list)
            {
                tp.Enqueue(t);
            }
            Assert.AreEqual(5,tp.Count);
            tp.Dispose();
            Assert.AreEqual(0,tp.Count);
            Assert.AreEqual(5,tp.Capacity);

        }

        [Test]
        public void AddTasksParallelTest()
        {
            var tp = new MyThreadPool.MyThreadPool(10);
            var threadsNum = 42;
            int[] l = new int[42];
            var threads = new List<Thread>();
            for (int i = 0; i < threadsNum; i++)
            {
                int j = i;
                var thread = new Thread(()=>
                {
                    using (var mt = new MyTask<int>(() =>
                    {
                        l[j] = 42;
                        return 42;
                    }))
                    {
                        tp.Enqueue(mt);
                        Assert.AreEqual(42,mt.Result);
                    }
                    
                });
                threads.Add(thread);
            }

            foreach (var t in threads)
            {
                t.Start();
            }

            foreach (var t in threads)
            {
                t.Join();
            }
            Assert.True(new List<int>(l).TrueForAll((i)=>i == 42));
            tp.Dispose();
            
        }

        [Test]
        public void ExecuteOutOfPoolTest()
        {
            var tp = new MyThreadPool.MyThreadPool();
            var task = new MyTask<int>(()=>
            {
                Thread.Sleep(2000);
                return 42;
            });
            Assert.Throws<InvalidOperationException>(task.Execute);
            tp.Enqueue(task);
            tp.Dispose();
        }
        
        [Test]
        public void ResultOutOfPoolTest()
        {
            var tp = new MyThreadPool.MyThreadPool();
            var task = new MyTask<int>(()=>
            {
                Thread.Sleep(2000);
                return 42;
            });
            Assert.Throws<InvalidOperationException>(()=>
            {
                var res = task.Result;
            });
            tp.Enqueue(task);
            tp.Dispose();
        }
        
        [Test]
        public void EnqueueDisposedTest()
        {
            var tp = new MyThreadPool.MyThreadPool();
            var task = new MyTask<int>(()=>
            {
                Thread.Sleep(2000);
                return 42;
            });
            tp.Dispose();
            
            Assert.Throws<ObjectDisposedException>(()=>
            {
                tp.Enqueue(task);
            });
            task.Dispose();
        }

    }
    
}