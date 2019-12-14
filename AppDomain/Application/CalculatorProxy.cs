using System;
using System.Reflection;
using InterfaceLibrary;

namespace Application
{
  public sealed class CalculatorProxy : MarshalByRefObject, ICalculator
  {
    public string AssemblyName { get; set; }
    public string TypeName { get; set; }
    public AppDomain Domain => AppDomain.CurrentDomain;

    public int Sum(int a, int b)
    {
      var calcType = Assembly.Load(AssemblyName).GetType(TypeName);
      var calcCtor = calcType.GetConstructor(new Type[] {});
      var calculator = (ICalculator)calcCtor.Invoke(new object[] {});
      return calculator.Sum(a, b);
    }
  }
}
