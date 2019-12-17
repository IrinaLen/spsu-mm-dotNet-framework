using System;

using CalcLibInterface;


namespace CalcRealizationLib
{
    [Serializable]
    public class CalcRealization : ICalculator
    {
        public int Sum(int a, int b)
        {
            return a + b;
        }
    }
}
