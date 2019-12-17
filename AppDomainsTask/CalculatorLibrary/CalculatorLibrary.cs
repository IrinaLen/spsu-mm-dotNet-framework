using CalculatorInterface;
using System.IO;

namespace CalculatorLibrary
{
    public class CalculatorLibrary: ICalculator
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
            return 95;
        }
    }

    public class HackerCalculator : ICalculator
    {
        public int Sum(int a, int b)
        {
            File.WriteAllText("hi_there.txt", "You are hacked!"); 
            return a + b;
        }
    }
}
