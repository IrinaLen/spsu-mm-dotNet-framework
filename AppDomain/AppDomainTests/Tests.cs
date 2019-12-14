using System;
using System.IO;
using System.Security;
using Application;
using InterfaceLibrary;
using NUnit.Framework;

namespace AppDomainTests
{
  [TestFixture]
  public class Tests
  {
    private readonly string _hackerLibrary = Path.Combine(
      Directory.GetParent(TestContext.CurrentContext.TestDirectory).Parent.Parent.FullName,
      "HackerRealizationLibrary",
      "bin",
      "Debug",
      "HackerRealizationLibrary.dll"
    );

    private readonly string _goodLibrary =
      Path.Combine(
        Directory.GetParent(TestContext.CurrentContext.TestDirectory).Parent.Parent.FullName,
        "GoodRealizationLibrary",
        "bin",
        "Debug",
        "GoodRealizationLibrary.dll"
      );

    [Test]
    public void HackerLibraryTest()
    {
      Sandboxer.RunInSandbox(sandboxer =>
      {
        foreach (var calculator in sandboxer.GetCalculators(_hackerLibrary))
        {
          Assert.Throws(typeof(SecurityException), () => calculator.Sum(2, 2));
          AppDomain.Unload(calculator.Domain);
        }
      });
    }

    [Test]
    public void GoodLibraryTest()
    {
      Sandboxer.RunInSandbox(sandboxer =>
      {
        Console.WriteLine(AppDomain.CurrentDomain.BaseDirectory);
        foreach (var calculator in sandboxer.GetCalculators(_goodLibrary))
        {
          switch (calculator.TypeName)
          {
            case "GoodRealizationLibrary.NormalCalculator":
              Assert.AreEqual(4, calculator.Sum(2, 2));
              break;
            case "GoodRealizationLibrary.BrokenCalculator":
              Assert.AreEqual(42, calculator.Sum(2, 2));
              break;
          }
          AppDomain.Unload(calculator.Domain);
        }
      });
    }
  }
}
