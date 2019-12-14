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
        private readonly Stopwatch _sp = new Stopwatch();
        private readonly Random _rnd = new Random(0);
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
            const int capacity = 5;
            Init(capacity);
            Assert.AreEqual(capacity, _pool.Capacity);
        }

        [Test]
        public void AddTaskTest()
        {
            Debug.WriteLine("==== AddTask ====");
            const int capacity = 1;
            Init(capacity);
            var task = GetTask(0);

            Debug.WriteLine("\tenqueue one task");
            _pool.Enqueue(task);

            Debug.WriteLine("\twait for result");
            _sp.Restart();
            Debug.WriteLine($"\tresult: {task.Result} in {_sp.ElapsedMilliseconds} ms");
            Debug.WriteLine("=================");
        }

        [Test]
        public void AddManyTaskTest()
        {
            Debug.WriteLine("==== AddManyTask ====");
            const int capacity = 5;
            Init(capacity);

            var tasks = new List<IMyTask<string>>();
            for (var i = 0; i < capacity << 1; i++)
            {
                tasks.Add(GetTask(i));
                Debug.WriteLine($"\tenqueue task #{i}");
                _pool.Enqueue(tasks.Last());
            }

            var sp = new Stopwatch();
            sp.Start();
            for (var i = 0; i < capacity << 1; i++)
            {
                Debug.WriteLine($"\twait for result #{i}");
                _sp.Restart();
                Debug.WriteLine($"\tresult #{i}: {tasks[i].Result} in {_sp.ElapsedMilliseconds} ms");
            }

            Debug.WriteLine($"\t total elapsed time: {sp.ElapsedMilliseconds}");
            Debug.WriteLine("=====================");
        }

        [Test]
        public void FailedTaskTest()
        {
            Debug.WriteLine("==== FailedTask ====");
            const int capacity = 1;
            Init(capacity);
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
            const int capacity = 3;
            Init(capacity);

            var task = GetTask(0);
            Debug.WriteLine("\tenqueue task #0");
            _pool.Enqueue(task);
            var tasks = new List<IMyTask<string>> {task};
            for (var i = 1; i < capacity << 1; i++)
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
            for (var i = 0; i < capacity << 1; i++)
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
            _sp.Reset();
            _pool.Dispose();
        }
    }
}