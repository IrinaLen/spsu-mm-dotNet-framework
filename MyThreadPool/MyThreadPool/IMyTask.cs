using System;

namespace MyThreadPool
{
    //Base interface for MyTask since C# does not allow some tricks with gererics
    public interface IMyTaskBase
    {
        //tells whether the task is completed or not
        bool IsCompleted { get; }
        
        //tells whether the task is failed
        bool IsFailed { get; }
        
        //tells whether the task is in threadpool, should never be set by the user
        bool IsInThreadPool { get; set; }
        //runs the task, should be called only from the inside of a threadpool
        void Execute();
        //Link to chain multiple tasks
        IMyTaskBase PreviousTask { get; }
    }
    public interface IMyTask<out TResult> : IMyTaskBase
    {
        
        //Query the Result of the task. Should be called after the task is enqueued
        TResult Result { get; }
        
        //Continuation
        IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult,TNewResult> func);
    }
}