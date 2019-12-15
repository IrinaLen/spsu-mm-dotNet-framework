using System;

namespace MyThreadPool
{
    //simple running example
    public class Example
    {

        public static void Main()
        {
            var threadPool = new MyThreadPool(1);
            var task1= new MyTask<string>(() => "ROck!");
            threadPool.Enqueue(task1);
            var task2 = task1.ContinueWith((rock) => rock + "!!!!");
            threadPool.Enqueue(task2);
            Console.WriteLine(task1.Result);
            Console.WriteLine(task2.Result);
            threadPool.Dispose();
            task1.Dispose();
            task2.Dispose();
            Console.WriteLine("Press any to exit");
            Console.ReadKey();

        }
        
    }
}