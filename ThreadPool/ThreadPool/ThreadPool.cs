namespace ThreadPool
{
  public class ThreadPool
  {
    private const int DefaultPoolSize = 4;

    public ThreadPool(int poolSize)
    {
    }
    
    public ThreadPool() : this(DefaultPoolSize)
    {
    }

  }
}
