using System;
using System.IO;
using System.Threading;

namespace MyThreadPool
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var tp = new MyThreadPool(3);
            IMyTask<int> task = new MyTask<int>(()=>
            {
                Console.WriteLine($"31 id is {Thread.CurrentThread.ManagedThreadId}");
                Thread.Sleep(2000);
                return 31;
            });
            IMyTask<int> task2 = new MyTask<int>(()=>
            {
                Console.WriteLine($"32 id is {Thread.CurrentThread.ManagedThreadId}");
                Thread.Sleep(2000);
                return 32;
            });
            IMyTask<int> task3 = new MyTask<int>(()=>
            {
                Console.WriteLine($"33 id is {Thread.CurrentThread.ManagedThreadId}");
                Thread.Sleep(2000);
                return 33;
            });
            
            tp.Enqueue(task.ContinueWith((i)=>
            {
                Console.WriteLine($"{i+1} id is {Thread.CurrentThread.ManagedThreadId}");
                return i + 1;
            }));
            tp.Enqueue(task.ContinueWith((i)=>
            {
                Console.WriteLine($"{i+99} id is {Thread.CurrentThread.ManagedThreadId}");
                return i + 99;
            }));
            tp.Enqueue(task2);
            tp.Enqueue(task3);
            tp.Dispose();
            Console.WriteLine(tp.Count);
            Console.WriteLine(task.Result);
            Console.WriteLine(task3.Result);
        }
    }
}