using System;
using ConsoleApplication1;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestAppDomain
{
    [TestClass]
    public class Tests
    {
        [TestMethod]
        public void TestMethod1()
        {
            int a = 4, b = 5;
            var results = Program.BaseClass().Run(a, b);
            foreach(var result in results)
            {
                if (result.implementationName == "DefaultCalculator")
                {
                    Assert.AreEqual(result.result.value, a + b);
                }
                else if (result.implementationName == "BrokenCalculator")
                {
                    Assert.AreNotEqual(result.result.value, a + b);
                }
                else if (result.implementationName == "HackingCalculator")
                {
                    Assert.AreEqual(result.result, null);
                }
            }
        }
    }
}
