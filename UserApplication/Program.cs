using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using ICalculatorLibrary;


public class Program
{
    private static string _libraryPath = Path.Combine("..", "..", "..", "CalculatorLibrary", "bin", "Release");
    private static string _libraryName = "CalculatorLibrary";

    public static void Main(string[] args)
    {
        var a = 4;
        var b = 3;
        var newDomain = CreateAppDomain(_libraryPath, "Calc");
        GetProxyCalculator(newDomain).SumAll(typeof(ICalculator), _libraryName, a, b);
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

    public static ProxyObjectCalculator GetProxyCalculator(AppDomain appDomain)
    {
        ProxyObjectCalculator proxyCalculator = (ProxyObjectCalculator) Activator.CreateInstanceFrom(
            appDomain, typeof(ProxyObjectCalculator).Assembly.ManifestModule.FullyQualifiedName,
            typeof(ProxyObjectCalculator).FullName).Unwrap();

        return proxyCalculator;
    }

    public class ProxyObjectCalculator : MarshalByRefObject
    {
        public int Sum(string assemblyName, string typeName, int a, int b)
        {
            var calculator = (ICalculator) Activator.CreateInstance(assemblyName, typeName).Unwrap();
            return calculator.Sum(a, b);
        }

        public void SumAll(Type iType, string assemblyName, int a, int b)
        {
            Assembly.Load(assemblyName);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => iType.IsAssignableFrom(p) && p.IsClass);
            foreach (var type in types)
            {
                Console.WriteLine($"Calculator {type}:");

                var calc = (ICalculator) Activator.CreateInstance(type);
                try
                {
                    Console.WriteLine($"Result is {calc.Sum(a, b)}");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Result is{e.Message}");
                }
            }
        }
    }
}