using System;

namespace ThreadPool.Exceptions
{
  public class ThreadPoolDisposedException : Exception
  {
    public ThreadPoolDisposedException() : base("Cannot to add task into ThreadPool because it's disposed")
    {
    }
  }
}
