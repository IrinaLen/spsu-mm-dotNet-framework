using System;
using InterfaceLibrary;

namespace GoodRealizationLibrary
{
  public class NormalCalculator : ICalculator
  {
    public int Sum(int a, int b)
    {
      return a + b;
    }
  }
  
  public class BrokenCalculator : ICalculator
  {
    public int Sum(int a, int b)
    {
      return 42;
    }
  }
}
