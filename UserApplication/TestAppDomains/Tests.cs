using System;
using System.IO;
using System.Reflection;
using System.Security;
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
        private string _typeNameCorrectCalculator = "CalculatorLibrary.CorrectCalculator";
        private string _typeNameOddCalculator = "CalculatorLibrary.OddCalculator";
        private string _typeNameUnsafeCalculator = "CalculatorLibrary.UnsafeCalculator";

        [Test]
        public void TestAppDomainUnsafeReadFilePrevent()
        {
            var appDomainUnsafeCalculator = Program.CreateAppDomain(_calculatorLibraryPath, "UnsafeCalculator");
            try
            {
                var ex = Program.SumInAppDomain(appDomainUnsafeCalculator, _assemblyLibraryName,
                    _typeNameUnsafeCalculator,
                    5, 6);
            }
            catch (SecurityException e)
            {
                Assert.Pass();
            }

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
            var resOdd = Program.SumInAppDomain(appDomainOddCalculator, _assemblyLibraryName, _typeNameOddCalculator, 5,
                6);
            var resCorrect = Program.SumInAppDomain(appDomainCorrectCalculator, _assemblyLibraryName,
                _typeNameCorrectCalculator, 5, 6);
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Assert.AreEqual(true, assembly.GetName().Name != _assemblyLibraryName);
            }

            Assert.AreEqual(resOdd, 42);
            Assert.AreEqual(resCorrect, 11);
        }
    }
}