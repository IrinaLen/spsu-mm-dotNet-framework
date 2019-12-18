using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using ThreadPool;

namespace ThreadPoolTest
{
    public class Tests
    {
        private readonly Random _rnd = new Random(0);
        private readonly Stopwatch _sp = new Stopwatch();
        private int _capacity = 5;
        private ThreadPool.ThreadPool _pool;

        private void Init(int capacity)
        {
            Debug.WriteLine($"\tinit threadpool with cap: {capacity}");
            _pool = new ThreadPool.ThreadPool(capacity);
        }

        private IMyTask<string> GetTask(int number)
        {
            return MyTask<string>.New(() =>
            {
                Debug.WriteLine($"\t\trun task #{number}");
                Thread.Sleep(_rnd.Next(500));
                Debug.WriteLine($"\t\tend task #{number}");
                return $"task #{number}";
            });
        }

        [Test]
        public void CheckThreadsCount()
        {
            Init(_capacity);
            Assert.AreEqual(_capacity, _pool.Capacity);
        }

        [Test]
        public void AddTaskTest()
        {
            Debug.WriteLine("==== AddTask ====");
            _capacity = 1;
            Init(_capacity);
            var task = GetTask(0);
            Debug.WriteLine("\tenqueue one task");
            _pool.Enqueue(task);

            Debug.WriteLine("\twait for result");
            _sp.Restart();
            Debug.WriteLine($"\tresult: {task.Result} in {_sp.ElapsedMilliseconds} ms");
            Debug.WriteLine("=================");
        }

        [Test]
        public void AddManyTasksTest()
        {
            Debug.WriteLine("==== AddManyTasks ====");
            Init(_capacity);
            var tasks = new List<IMyTask<string>>();
            for (var i = 0; i < _capacity << 1; i++)
            {
                tasks.Add(GetTask(i));
                Debug.WriteLine($"\tenqueue task #{i}");
                _pool.Enqueue(tasks.Last());
            }

            var sp = new Stopwatch();
            sp.Start();
            for (var i = 0; i < _capacity << 1; i++)
            {
                Debug.WriteLine($"\twait for result #{i}");
                _sp.Restart();
                Debug.WriteLine($"\tresult #{i}: {tasks[i].Result} in {_sp.ElapsedMilliseconds} ms");
            }

            Debug.WriteLine($"\t total elapsed time: {sp.ElapsedMilliseconds}");
            Debug.WriteLine("=====================");
        }

        [Test]
        public void AddTasksAndDisposeTest()
        {
            Debug.WriteLine("==== AddTasksAndDispose ====");
            _capacity = 1;
            Init(_capacity);
            var task1 = MyTask<string>.New(() =>
            {
                Debug.WriteLine("\t\tinside task #1");
                Thread.Sleep(2000);
                Debug.WriteLine("\t\treturn from task #1");
                return "task #1";
            });
            var task2 = task1.ContinueWith(result =>
            {
                Debug.WriteLine("\t\tinside task #2");
                Thread.Sleep(100);
                Debug.WriteLine("\t\treturn from task #2");
                return $"task #2 after {result}";
            });
            Debug.WriteLine("\tenqueue task #1");
            _pool.Enqueue(task1);
            Debug.WriteLine("\tenqueue task #2");
            _pool.Enqueue(task2);

            _sp.Start();
            Debug.WriteLine("\twait for result #1");
            Debug.WriteLine($"\tresult #1: {task1.Result} in {_sp.ElapsedMilliseconds} ms");

            Debug.WriteLine("\tdispose threadpool");
            _pool.Dispose();

            Debug.WriteLine("\ttry to get result #2:");
            _sp.Restart();
            Debug.WriteLine($"\tresult #2: {task2.Result} in {_sp.ElapsedMilliseconds} ms");
            Debug.WriteLine("=====================");
        }

        [Test]
        public void FailedTaskTest()
        {
            Debug.WriteLine("==== FailedTask ====");
            _capacity = 1;
            Init(_capacity);
            var task = MyTask<string>.New(() =>
            {
                var list = new List<string> {"task #0"};
                return list[5];
            });

            Debug.WriteLine("\tenqueue one task");
            _pool.Enqueue(task);

            Debug.WriteLine("\twait for failed result");
            _sp.Restart();

            var exception = Assert.Throws<AggregateException>(() =>
            {
                Debug.WriteLine($"\tresult: {task.Result} in {_sp.ElapsedMilliseconds} ms");
            });

            Debug.WriteLine($"\tfailed result: {exception.Message}");
            Assert.NotNull(exception.InnerException);
            Assert.IsInstanceOf(typeof(ArgumentOutOfRangeException), exception.InnerException);
            Debug.WriteLine($"\twith inner exception: {exception.InnerException.Message}");
            Debug.WriteLine("=================");
        }

        [Test]
        public void ContinueWithTest()
        {
            Debug.WriteLine("==== ContinueWith ====");
            _capacity = 3;
            Init(_capacity);
            var task = GetTask(0);
            Debug.WriteLine("\tenqueue task #0");
            _pool.Enqueue(task);
            var tasks = new List<IMyTask<string>> {task};
            for (var i = 1; i < _capacity << 1; i++)
            {
                var number = i;
                task = task.ContinueWith(result =>
                {
                    Thread.Sleep(_rnd.Next(500));
                    return $"task #{number} after {result}";
                });
                tasks.Add(task);
                Debug.WriteLine($"\tenqueue task #{i}");
                _pool.Enqueue(tasks.Last());
            }

            var sp = new Stopwatch();
            sp.Start();
            for (var i = 0; i < _capacity << 1; i++)
            {
                Debug.WriteLine($"\twait for result #{i}");
                _sp.Restart();
                Debug.WriteLine($"\tresult #{i}: {tasks[i].Result} in {_sp.ElapsedMilliseconds} ms");
            }

            Debug.WriteLine($"\t total elapsed time: {sp.ElapsedMilliseconds}");
            Debug.WriteLine("=====================");
        }

        [TearDown]
        public void Dispose()
        {
            _capacity = 5;
            _sp.Reset();
            if (!_pool.IsDisposed) _pool.Dispose();
        }
    }
}