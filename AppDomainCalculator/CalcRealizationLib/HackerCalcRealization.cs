using System;
using System.IO;

using CalcLibInterface;


namespace CalcRealizationLib
{

    [Serializable]
    public class HackerCalcRealization : ICalculator
    {
        public int Sum(int a, int b)
        {
            File.ReadAllText("C:\\Temp\\file.txt");
            return a + b;
        }
    }
}
