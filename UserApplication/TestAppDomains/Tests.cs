using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace TestAppDomains
{
    [TestFixture]
    public class Tests
    {
        private string _calculatorLibraryPath =
            Path.Combine(new[]
            {
                Directory.GetParent(TestContext.CurrentContext.TestDirectory).Parent.Parent.Parent.FullName,
                "CalculatorLibrary", "bin", "Release"
            });

        private string _assemblyLibraryName = "CalculatorLibrary";
        private string typeNameCorrectCalculator = "CalculatorLibrary.CorrectCalculator";
        private string typeNameOddCalculator = "CalculatorLibrary.OddCalculator";
        private string typeNameUnsafeCalculator = "CalculatorLibrary.UnsafeCalculator";

        [Test]
        public void TestAppDomainUnsafeReadFilePrevent()
        {
            var appDomainUnsafeCalculator = Program.CreateAppDomain(_calculatorLibraryPath, "UnsafeCalculator");
            //try
            var ex = Program.SumInAppDomain(appDomainUnsafeCalculator, _assemblyLibraryName, typeNameUnsafeCalculator,
                5, 6);
            //catch
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Assert.AreEqual(true, assembly.GetName().Name != _assemblyLibraryName);
            }
        }

        [Test]
        public void TestAppDomainSeveralDomains()
        {
            var appDomainOddCalculator = Program.CreateAppDomain(_calculatorLibraryPath, "OddCalculator");
            var appDomainCorrectCalculator = Program.CreateAppDomain(_calculatorLibraryPath, "CorrectCalculator");
            var resOdd = Program.SumInAppDomain(appDomainOddCalculator, _assemblyLibraryName, typeNameOddCalculator, 5,
                6);
            var resCorrect = Program.SumInAppDomain(appDomainCorrectCalculator, _assemblyLibraryName,
                typeNameCorrectCalculator, 5, 6);
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Assert.AreEqual(true, assembly.GetName().Name != _assemblyLibraryName);
            }

            Assert.AreEqual(resOdd, 42);
            Assert.AreEqual(resCorrect, 11);
        }
    }
}