using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using myCalcApp;
using System.Security;

namespace MyCalcAppTest
{
    [TestClass]
    public class MyCalcTest
    {
        private readonly string _pathToAssemblyFolder = @"..\..\..\CalcRealizationLib\bin\Debug";
        private readonly string _assemblyName = "CalcRealizationLib";
        private readonly string _calcClass = "CalcRealizationLib.CalcRealization";
        private readonly string _hackerClass = "CalcRealizationLib.HackerCalcRealization";
        private readonly string _methodToExecuteName = "Sum";
        private static Object[] _parameters = { 45, 25 };
        private int _rightAnswer = 70;

        [TestMethod]
        public void RunGoodRealization()
        {
            Assert.AreEqual(SandboxerCalc.ExecuteUntrustedCode(_pathToAssemblyFolder, _assemblyName, _calcClass, _methodToExecuteName, _parameters), _rightAnswer);
        }

        [TestMethod]
        public void RunHackerRealization()
        {
            try
            {
                SandboxerCalc.ExecuteUntrustedCode(_pathToAssemblyFolder, _assemblyName, _hackerClass, _methodToExecuteName, _parameters);
                Assert.Fail();
            }
            catch (TargetInvocationException tex)
            {
                if (tex.InnerException is SecurityException)
                {
                    //its allright!
                }
                else
                {
                    Assert.Fail();
                }
            }
        }

        [TestMethod]
        public void RunBothRealizations()
        {
            Assert.AreEqual(SandboxerCalc.ExecuteUntrustedCode(_pathToAssemblyFolder, _assemblyName, _calcClass, _methodToExecuteName, _parameters), _rightAnswer);
            try
            {
                SandboxerCalc.ExecuteUntrustedCode(_pathToAssemblyFolder, _assemblyName, _hackerClass, _methodToExecuteName, _parameters);
                Assert.Fail();
            }
            catch (TargetInvocationException tex)
            {
                if (tex.InnerException is SecurityException)
                {
                    //its allright!
                }
                else
                {
                    Assert.Fail();
                }
            }
        }
    }
}