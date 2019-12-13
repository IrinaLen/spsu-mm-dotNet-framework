using System;
using System.IO;
using System.Security;
using System.Security.Permissions;


public class Program
{
    private static string _libraryPath = Path.Combine("..", "..", "..", "CalculatorLibrary", "bin", "Release");
    private static string _libraryName = "CalculatorLibrary";

    public static void Main(string[] args)
    {
        var a = 4;
        var b = 3;
        var goodCalculatorType = "CalculatorLibrary.CorrectCalculator";
        var newDomain = CreateAppDomain(_libraryPath, "Calc");
        var res = SumInAppDomain(newDomain, _libraryName, goodCalculatorType, a, b);
        Console.WriteLine($"Result is {res}");
    }

    public static AppDomain CreateAppDomain(string path, string newDomainName)

    {
        PermissionSet permissionSet = new PermissionSet(PermissionState.None);
        permissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
        AppDomainSetup domainSetup = new AppDomainSetup
        {
            ApplicationBase = path
        };

        return AppDomain.CreateDomain(newDomainName, null, domainSetup, permissionSet);
    }

    public static int SumInAppDomain(AppDomain appDomain, string assemblyName, string typeName, int a, int b)
    {
        ProxyObjectCalculator proxyCalculator = (ProxyObjectCalculator) Activator.CreateInstanceFrom(
            appDomain, typeof(ProxyObjectCalculator).Assembly.ManifestModule.FullyQualifiedName,
            typeof(ProxyObjectCalculator).FullName).Unwrap();

        return proxyCalculator.Sum(assemblyName, typeName, a, b);
    }

    class ProxyObjectCalculator : MarshalByRefObject
    {
        public int Sum(string assemblyName, string typeName, int a, int b)
        {
            var calculator = (ICalculatorLibrary.ICalculator) Activator.CreateInstance(assemblyName, typeName).Unwrap();
            return calculator.sum(a, b);
        }
    }
}