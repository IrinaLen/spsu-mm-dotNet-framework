using System;
using System.Reflection;
using System.Runtime.InteropServices;
using CalculatorInterfaceLibrary;

namespace UserApplication
{
    //loaded into its own app domain
    public class CalculatorProxy : MarshalByRefObject, ICalculator
    {
        public string AssemblyName { get; set; }
        public string TypeName { get; set; }
        
        public int Sum(int a, int b)
        {
            Console.WriteLine($"Executing Sum inside {AppDomain.CurrentDomain.FriendlyName} domain");
            return ((ICalculator)Activator.CreateInstance(AssemblyName, TypeName).Unwrap()).Sum(a,b);
        }
    }
}