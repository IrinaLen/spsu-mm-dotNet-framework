﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using ICalculatorLibrary;


public class Program
{
    private static string _libraryPath = Path.Combine("..", "..", "..", "CalculatorLibrary", "bin", "Release");
    private static string _libraryName = "CalculatorLibrary.dll";
    private static string _assemblyName = "CalculatorLibrary";

    public static void Main()
    {
        var a = 4;
        var b = 3;
        AppDomainSetup domainSetup = new AppDomainSetup
        {
            ApplicationBase = Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
        };

        var newDomain = AppDomain.CreateDomain("Calc", null, domainSetup);

        GetProxyCalculator(newDomain)
            .SumAll(typeof(ICalculator), _assemblyName, Path.Combine(_libraryPath, _libraryName), a, b);
        Console.ReadKey();
    }

    public static AppDomain CreateAppDomainRestricted(string path, string newDomainName)

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
        return GetProxyCalculator(appDomain).Sum(assemblyName, typeName, a, b);
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

        private int SumInner(string assemblyName, Type type, int a, int b)
        {
            Assembly.Load(assemblyName);
            Console.WriteLine($"Actual Sum called in {AppDomain.CurrentDomain.FriendlyName} domain ");
            var calc = (ICalculator) Activator.CreateInstance(type);
            return calc.Sum(a, b);
        }

        public void SumAll(Type iType, string assemblyName, string assemblyPath, int a, int b)
        {
            Assembly.LoadFrom(assemblyPath);
            Console.WriteLine($"SumAll called in {AppDomain.CurrentDomain} domain");
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => iType.IsAssignableFrom(p) && p.IsClass);
            foreach (var type in types)
            {
                var appDomain = CreateAppDomainRestricted(_libraryPath, $"type{type}");
                var calc = GetProxyCalculator(appDomain);

                Console.WriteLine($"Calculator {type}:");

                try
                {
                    Console.WriteLine($"Result is {calc.SumInner(assemblyName, type, a, b)}");
                }
                catch (SecurityException e)
                {
                    Console.WriteLine($"Some malicious code try to write to your filesystem");
                }
            }
        }
    }
}