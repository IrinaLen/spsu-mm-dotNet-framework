using System;
using System.IO;

namespace Calculators
{
    [Serializable]
    public class DefaultCalculator : ICalculator.ICalculator 
    {
        public int Sum(int a, int b)
        {
            return a + b;
        }
    }
    
    [Serializable]
    public class BrokenCalculator : ICalculator.ICalculator 
    {
        public int Sum(int a, int b)
        {
            return 42;
        }
    }

    [Serializable]
    public class HackingCalculator : ICalculator.ICalculator
    {
        public HackingCalculator()
        {
            File.WriteAllText("sample.txt", "Hello world!");
        }
        public int Sum(int a, int b)
        {
            File.WriteAllText("sample.txt", "Hello world!");
            return a + b;
        }
    }
}