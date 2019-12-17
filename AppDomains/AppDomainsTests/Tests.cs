using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using NUnit.Framework;
using UserApplication;

namespace AppDomainsTests
{
    [TestFixture]
    public class Tests
    {
        private readonly string[] libraryNames =
            {"MachineLearningCalculatorImplementationLibrary", "SimpleCalculatorImplementationLibrary"};

        private List<String> _UntrustedAssemblies
        {
            get
            {
                var untrustedPath = Directory.GetParent(TestContext.CurrentContext.TestDirectory).Parent.Parent.FullName;
                var untrustedAssemblies = new List<string>();
                foreach (var name in libraryNames)
                {
                    var path = Path.Combine(untrustedPath, name, "bin");
                    untrustedAssemblies.Add(Directory.Exists(Path.Combine(path, "Debug"))
                        ? Path.Combine(path, "Debug", name + ".dll")
                        : Path.Combine(path, "Release", name + ".dll"));
                }

                return untrustedAssemblies;
            }
        }
        
        [Test]
        public void FileSystemAccessTest()
        {
            
            Assert.Throws<SecurityException>(()=>Program.GetCalculatorProxyList(_UntrustedAssemblies[0])[0]
                .Sum(21,21));
        }
        
        [Test]
        public void CalculatorTest()
        {
            
            Assert.AreEqual(42,Program.GetCalculatorProxyList(_UntrustedAssemblies[1])[0]
                .Sum(21,21));
        }
        
        
    }
}