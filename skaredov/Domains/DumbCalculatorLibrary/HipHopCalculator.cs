using System;
using ICalculatorLibrary;

namespace DumbCalculatorLibrary
{
    public class HipCalculator: ICalculator
    {
        public int Sum(int a, int b) => a + b;
    }

    public class HopCalculator: ICalculator
    {
        public int Sum(int a, int b) => Convert.ToInt32(Math.Sqrt(a*a + b*b));
    }
}