using System;
using System.IO;
using ICalculatorLibrary;

namespace CalculatorLibrary
{
    
    
    [Serializable]
    public class OddCalculator: ICalculator
    {
        public int Sum(int a, int b) => 42;
    }

    [Serializable]
    public class CorrectCalculator : ICalculator
    {
        public int Sum(int a, int b) => a + b;
    }
    
    [Serializable]
    public class UnsafeCalculator: ICalculator
    
    {
        public int Sum(int a, int b)
        {
            var c = File.ReadAllText("secret_file.txt");
            return 42;
        } 
    }
}