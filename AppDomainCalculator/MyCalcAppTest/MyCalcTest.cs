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
        private static Object[] parameters = { 45, 25 };
        private int rightAnswer = 70;

        [TestMethod]
        public void RunGoodRealization()
        {
            Assert.AreEqual(SandboxerCalc.ExecuteUntrustedCode(_pathToAssemblyFolder, _assemblyName, _calcClass, _methodToExecuteName, parameters), rightAnswer);
        }

        [TestMethod]
        public void RunHackerRealization()
        {
            try
            {
                SandboxerCalc.ExecuteUntrustedCode(_pathToAssemblyFolder, _assemblyName, _hackerClass, _methodToExecuteName, parameters);
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
            Assert.AreEqual(SandboxerCalc.ExecuteUntrustedCode(_pathToAssemblyFolder, _assemblyName, _calcClass, _methodToExecuteName, parameters), rightAnswer);
            try
            {
                SandboxerCalc.ExecuteUntrustedCode(_pathToAssemblyFolder, _assemblyName, _hackerClass, _methodToExecuteName, parameters);
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