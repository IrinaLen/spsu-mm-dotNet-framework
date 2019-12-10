using System;

namespace ThreadPool.Exceptions
{
  public class ThreadPoolCannotAddThread : Exception
  {
    public ThreadPoolCannotAddThread() : base("Cannot to add new worker thread to pool")
    {
    }
  }
}
