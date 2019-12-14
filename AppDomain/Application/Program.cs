using System;
using System.IO;
using System.Reflection;
using System.Security;

namespace Application
{
  public class Program
  {
    private static readonly string[] _pathToAssembly =
    {
      @"..\..\..\GoodRealizationLibrary\bin\Debug\GoodRealizationLibrary.dll",
      @"..\..\..\HackerRealizationLibrary\bin\Debug\HackerRealizationLibrary.dll",
    };

    public static void Main(string[] args)
    {
      Sandboxer.RunInSandbox(sandboxer =>
      {
        foreach (var path in _pathToAssembly)
        {
          foreach (var calculator in sandboxer.GetCalculators(path))
          {
            try
            {
              Console.WriteLine(calculator.Domain.FriendlyName);
              Console.WriteLine(calculator.Sum(2, 2));
            }
            catch (SecurityException e)
            {
              Console.WriteLine(e.Message);
            }
            finally
            {
              AppDomain.Unload(calculator.Domain);
            }
          }
        }
      });
    }
  }
}
