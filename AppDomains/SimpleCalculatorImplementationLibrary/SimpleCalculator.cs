using CalculatorInterfaceLibrary;

namespace SimpleCalculatorImplementationLibrary
{
    public class SimpleCalculator : ICalculator
    {
        public int Sum(int a, int b)
        {
            return a + b;
        }
    }
}