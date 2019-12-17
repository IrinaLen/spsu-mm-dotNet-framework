using System;

namespace CherepanovThreadpool
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start main");
            MyThreadPool myThreadPool = new MyThreadPool(2);
            var myTask = new MyTask<int>(() => {
                return 2;
            });
            myThreadPool.Enqueue(myTask);
            Console.WriteLine(myTask.Result);

            var newTask = myTask.ContinueWith(ret => {
                return ret.ToString() + " is two";
            });
            myThreadPool.Enqueue(newTask);
            Console.WriteLine(newTask.Result);

            var brokenTask = newTask.ContinueWith(ret =>
            {
                throw new Exception("My pretty exception");
                return 45;
            });
            try
            {
                Console.WriteLine(brokenTask.Result);
            }
            catch (Exception ex)
            {
                if (ex is AggregateException)
                {
                    Console.WriteLine("We catched right exception!");
                    Console.WriteLine(ex);
                }
                else
                {
                    throw ex;
                }
            }
            Console.WriteLine("End main");

            Console.ReadKey();
            myThreadPool.Dispose();
        }
    }
}
