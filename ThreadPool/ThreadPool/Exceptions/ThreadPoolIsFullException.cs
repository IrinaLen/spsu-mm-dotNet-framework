using System;

namespace ThreadPool.Exceptions
{
  public class ThreadPoolIsFullException : Exception
  {
    public ThreadPoolIsFullException() : base("Cannot to add task into ThreadPool because it's full")
    {
    }
  }
}
