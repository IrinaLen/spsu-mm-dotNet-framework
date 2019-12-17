using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CherepanovThreadpool;
using System.Collections.Generic;
using System.Threading;

namespace TestCherepanovThreadpool
{
    [TestClass]
    public class TestMyThreadPool
    {

        [TestMethod]
        public void TestAddOneTask()
        {
            var myThreadPool = new MyThreadPool(1);
            var task = new MyTask<int>(() =>
            {
                return 25;
            });
            myThreadPool.Enqueue(task);
            Assert.AreEqual(25, task.Result);
            myThreadPool.Dispose();
        }

        [TestMethod]
        public void TestAddManyTasks()
        {
            int activeThreads = 40;
            int pendingMs = 10;
            var threads = new List<Thread>();
            var myThreadPool = new MyThreadPool(2);
            for (int i = 0; i < activeThreads; i++)
            {
                var thread = new Thread(() =>
                {
                    var task1 = new MyTask<int>(() =>
                    {
                        Thread.Sleep(pendingMs);
                        return 10;
                    });
                    var task2 = new MyTask<string>(() =>
                    {
                        Thread.Sleep(pendingMs);
                        return "asd";
                    });
                    var task3 = new MyTask<bool>(() =>
                    {
                        Thread.Sleep(pendingMs);
                        return true;
                    });
                    var task4 = new MyTask<float>(() =>
                    {
                        Thread.Sleep(pendingMs);
                        return 47;
                    });
                    var task5 = new MyTask<char>(() =>
                    {
                        Thread.Sleep(pendingMs);
                        return 'c';
                    });
                    myThreadPool.Enqueue(task1);
                    myThreadPool.Enqueue(task2);
                    myThreadPool.Enqueue(task3);
                    myThreadPool.Enqueue(task4);
                    myThreadPool.Enqueue(task5);
                    Assert.AreEqual(10, task1.Result);
                    Assert.AreEqual("asd", task2.Result);
                    Assert.AreEqual(true, task3.Result);
                    Assert.AreEqual(47, task4.Result);
                    Assert.AreEqual('c', task5.Result);
                });
                threads.Add(thread);
                thread.Start();
            }

            threads.ForEach(thread => thread.Join());
            myThreadPool.Dispose();
        }

        [TestMethod]
        public void TestStress()
        {
            MyThreadPool myThreadPool = new MyThreadPool(20);
            Assert.AreEqual(myThreadPool.ThreadNumber, 20);
            myThreadPool.Dispose();
        }



        [TestMethod]
        public void TestContinueWithImlicitEnqueue()
        {
            MyThreadPool myThreadPool = new MyThreadPool(2);
            var myTask = new MyTask<int>(() => { return 2; });

            var newTask = myTask.ContinueWith<string>(asd =>
            {
                return asd.ToString();
            });

            try
            {
                myThreadPool.Enqueue(newTask);
                var res = newTask.Result;
                Assert.Fail();
            }
            catch (Exception ex)
            {
                if (ex is AggregateException)
                {
                    //its ok!!!
                }
                else
                {
                    Assert.Fail();
                }
            }
            myThreadPool.Dispose();
        }

        [TestMethod]
        public void TestWithOneContinueWith()
        {
            Console.WriteLine("Start main");
            MyThreadPool myThreadPool = new MyThreadPool(2);
            var myTask = new MyTask<int>(() => { return 2; });
            myThreadPool.Enqueue(myTask);
            Assert.AreEqual(myTask.Result, 2);

            var newTask = myTask.ContinueWith<string>(asd =>
            {
                return asd.ToString();
            });
            myThreadPool.Enqueue(newTask);
            Assert.AreEqual(newTask.Result, "2");
            myThreadPool.Dispose();
        }

        [TestMethod]
        public void TestMultipleContinueWith()
        {
            var myThreadPool = new MyThreadPool(3);
            var task1 = new MyTask<string>(() =>
            {
                Thread.Sleep(1000);
                return "My ";
            });
            var task2 = task1.ContinueWith(ret =>
            {
                Thread.Sleep(2000);
                return ret + "pipe ";
            });
            var task3 = task2.ContinueWith(ret =>
            {
                Thread.Sleep(3000);
                return ret + "is ";
            });
            var task4 = task3.ContinueWith(ret =>
            {
                Thread.Sleep(1750);
                return ret + "working!!!";
            });
            myThreadPool.Enqueue(task1);
            myThreadPool.Enqueue(task2);
            myThreadPool.Enqueue(task3);
            myThreadPool.Enqueue(task4);
            Assert.AreEqual("My pipe is ", task3.Result);
            Assert.AreEqual("My pipe is working!!!", task4.Result);
            myThreadPool.Dispose();
        }

        [TestMethod]
        public void TestReturnAggregateException()
        {
            MyThreadPool myThreadPool = new MyThreadPool(2);
            var myTask = new MyTask<int>(() =>
            {
                throw new Exception("KERNEL PANIC!");
            });
            myThreadPool.Enqueue(myTask);
            try
            {
                Console.WriteLine(myTask.Result);
            }
            catch (Exception ex)
            {
                if (ex is AggregateException)
                {
                    //its ok!!!
                }
                else
                {
                    Assert.Fail();
                }
            }
            myThreadPool.Dispose();
        }
    }
}

