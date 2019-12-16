using System;
using System.IO;
using System.Security;
using Application;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AppDomainTests
{
  [TestClass]
  public class Tests
  {
    private readonly string _hackerLibrary = Path.Combine(
      Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName,
      "HackerRealizationLibrary",
      "bin",
      "Debug",
      "HackerRealizationLibrary.dll"
    );

    private readonly string _goodLibrary = Path.Combine(
      Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName,
      "GoodRealizationLibrary",
      "bin",
      "Debug",
      "GoodRealizationLibrary.dll"
    );

    [TestMethod]
    public void HackerLibraryTest()
    {
      Sandboxer.RunInSandbox(sandboxer =>
      {
        foreach (var calculator in sandboxer.GetCalculators(_hackerLibrary))
        {
          Assert.ThrowsException<SecurityException>(() => calculator.Sum(2, 2));
          AppDomain.Unload(calculator.Domain);
        }
      });
    }

    [TestMethod]
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
