using System;
using System.Threading;
using System.Threading.Tasks;

namespace MyThreadPool
{
    public interface ITask
    {
        bool IsInThreadPool { get; set; }
        bool IsCompleted { get; }

        bool IsFailed { get; }
        ITask PreviousTask { get; }

        /// <summary>
        /// Executes stored Job if it is not yet started.
        /// Job computed only once.
        /// If Job is not in threadPool then exception should be thrown
        /// till job that already computed or failed or in threadPool.
        /// <exception cref="AggregateException">User could not call method explicitly</exception>
        /// </summary>
        void Execute();
    }


    public interface IMyTask<out TResult> : ITask
    {
        /// <summary>
        /// If job is not in any ThreadPool then exception should be thrown
        /// if job in ThreadPool (scheduled) but not started yet then this call be blocking for calling thread
        /// <returns>Returns result of processed task if its completed or throws exception if not</returns>
        /// <exception cref="AggregateException"> <c>if (IsFailed==true)</c> </exception>>
        /// </summary>
        TResult Result { get; }

        Func<TResult> Job { get; }
        IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> func);
    }
}