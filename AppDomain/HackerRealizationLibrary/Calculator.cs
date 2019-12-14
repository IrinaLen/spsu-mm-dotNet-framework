using System.IO;
using InterfaceLibrary;

namespace RealizationLibrary
{
  
  public class HackerCalculator : ICalculator
  {
    public int Sum(int a, int b)
    {
      File.WriteAllText("hack.txt", "hacked");
      return a + b;
    }
  }
}
