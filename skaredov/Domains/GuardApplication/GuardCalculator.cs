using System;
using System.Reflection;
using ICalculatorLibrary;

namespace GuardApplication
{
    // proxy class for untrusted calculators
    public class GuardCalculator: MarshalByRefObject, ICalculator
    {
        public string AssemblyName { get; set; }
        public string TypeName { get; set; }
        public AppDomain Domain => AppDomain.CurrentDomain;

        public int Sum(int a, int b)
        {
            Program.PrintCurrentDomain();
            Console.WriteLine($"\tGuardCalculator.Sum with assembly {AssemblyName}, type {TypeName}");
            var type = Assembly.Load(AssemblyName).GetType(TypeName);
            var constructor = type.GetConstructor(new Type[] {});
            var calculator = (ICalculator) constructor.Invoke(new object[] {});
            return calculator.Sum(a, b);
        }
    }
}