using System;
using System.IO;

namespace Calculators
{
    public class DefaultCalculator : ICalculator.ICalculator 
    {
        public int Sum(int a, int b)
        {
            return a + b;
        }
    }
    
    public class BrokenCalculator : ICalculator.ICalculator 
    {
        public int Sum(int a, int b)
        {
            return 42;
        }
    }

    public class HackingCalculator : ICalculator.ICalculator
    {
        public int Sum(int a, int b)
        {
            File.WriteAllText("sample.txt", "Hello world!");
            return a + b;
        }
    }
}