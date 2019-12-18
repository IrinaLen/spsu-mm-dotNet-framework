using System;
using Application;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestAppDomain
{
    [TestClass]
    public class Tests
    {
        [TestMethod]
        public void TestAll()
        {
            int a = 4, b = 5;
            var results = Program.BaseClass().Run(a, b);
            foreach(var result in results)
            {
                if (result.implementationName == "DefaultCalculator")
                {
                    Assert.AreEqual(result.Result.Value, a + b);
                }
                else if (result.implementationName == "BrokenCalculator")
                {
                    Assert.AreNotEqual(result.Result.Value, a + b);
                }
                else if (result.implementationName == "HackingCalculator")
                {
                    Assert.AreEqual(result.Result, null);
                }
            }
        }
    }
}
