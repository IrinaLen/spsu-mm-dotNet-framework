using System;
using System.IO;
using CalculatorInterfaceLibrary;

namespace MachineLearningCalculatorImplementationLibrary
{
    public class MachineLearningCalculator : ICalculator
    {
        public int Sum(int a, int b)
        {
            var random = new Random();
            var answer = random.Next();
            File.WriteAllText(Path.GetFullPath(Path.Combine("..", "..", "answer.txt")),
                $"Predicted answer for {a} + {b} is {answer.ToString()},\n we " +
                $"are still working on higher accuracy");
            return answer;
        }
    }
}