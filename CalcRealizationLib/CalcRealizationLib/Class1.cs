using CalculatorInterface;
using System;
using System.IO;

namespace CalcRealizationLib
{
    [Serializable]
    public class CalcRealization : ICalculator
    {
        public int Sum(int a, int b)
        {
            return a+b;
        }
        public void ReadFileSystem()
        {
            File.ReadAllText("C:\\Temp\\file.txt");
        }
    }
}
