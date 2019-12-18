using System;
using System.IO;
using System.Reflection;
using System.Security;
using GuardApplication;
using NUnit.Framework;

namespace DomainsTest
{
    [TestFixture]
    public class Tests
    {
        private static readonly string _root = Directory.GetParent(TestContext.CurrentContext.TestDirectory).Parent.Parent.FullName;
        private static readonly string _dumb = "DumbCalculatorLibrary";
        private static readonly string _ultra = "UltraCalculatorLibrary";

        [Test]
        public void DumbCalculatorTest()
        {
            var path = Program.GetPathToAssembly(_dumb, _root);
            GuardDomain.Execute("General Guardian", guardian =>
            {
                foreach (var calculator in guardian.LoadFromAssembly(path))
                {
                    Console.WriteLine($"\tDomain from inside calculator: {calculator.Domain.FriendlyName}");
                    Assert.DoesNotThrow(() =>
                    {
                        var result = calculator.Sum(42, 27);
                        Console.WriteLine($"\t\tResult {calculator.TypeName}.Sum(42, 27) = {result}");
                    });
                    AppDomain.Unload(calculator.Domain);
                }
            });
        }
        
        [Test]
        public void UltraCalculatorTest()
        {
            var path = Program.GetPathToAssembly(_ultra, _root);
            GuardDomain.Execute("General Guardian", guardian =>
            {
                foreach (var calculator in guardian.LoadFromAssembly(path))
                {
                    Console.WriteLine($"\tDomain from inside calculator: {calculator.Domain.FriendlyName}");
                    Func<int> action = () =>
                    {
                        var result = calculator.Sum(42, 27);
                        Console.WriteLine($"\t\tResult {calculator.TypeName}.Sum(42, 27) = {result}");
                        return result;
                    };
                    switch (Environment.OSVersion.Platform)
                    {
                        // it seems like mono doesn't have implementation of Code Access Security
                        // so, permission restrictions doesn't work on mac os
                        case PlatformID.Unix:
                        case PlatformID.MacOSX:
                            var result = action();
                            Assert.AreEqual(result, Convert.ToInt32(Math.E + Math.PI));
                            break;
                        default:
                            Assert.Throws<SecurityException>(() => action());
                            break;
                    }
                }
            });
        }
    }
}