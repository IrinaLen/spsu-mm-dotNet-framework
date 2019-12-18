using NUnit.Framework;
using UltraCalculatorLibrary;

namespace DomainsTest
{
    [TestFixture]
    public class Tests
    {
        // TODO
        [Test]
        public void Test1()
        {
            var ultra = new UltraCalculator();
            ultra.Sum(1, 2);
        }
    }
}