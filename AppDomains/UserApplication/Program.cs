using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Threading;
using CalculatorInterfaceLibrary;
using MachineLearningCalculatorImplementationLibrary;

namespace UserApplication
{
    public class Program
    {
        private static readonly string UntrustedPath = Path.Combine("..", "..", "..");

        public static void Main(string[] args)
        {
            var untrustedAssemblies = new List<string>();
            string[] libraryNames =
                {"MachineLearningCalculatorImplementationLibrary", "SimpleCalculatorImplementationLibrary"};
            foreach (var name in libraryNames)
            {
                var path = Path.Combine(UntrustedPath, name, "bin");
                untrustedAssemblies.Add(Directory.Exists(Path.Combine(path, "Debug"))
                    ? Path.Combine(path, "Debug", name + ".dll")
                    : Path.Combine(path, "Release", name + ".dll"));
            }


            Console.WriteLine(AppDomain.CurrentDomain.FriendlyName);
            // safe area

            Sandbox.Run((sandbox) =>
            {
                Action<string> f = (str) =>
                {
                    var calcs = GetCalculatorProxyList(str);
                    foreach (var c in calcs)
                    {
                        Console.WriteLine(c.Sum(21, 21));
                    }
                };
                
                foreach (var assembly in untrustedAssemblies)
                {
                    try
                    {
                        sandbox.Invoke(f, assembly);
                    }
                    catch (SecurityException e)
                    {
                        Console.WriteLine(e.Message);
                    }
                    
                }
                
            });
        }

        public static AppDomain CreateRestrictedDomain(string path, string name)
        {
            PermissionSet permissionSet = new PermissionSet(PermissionState.None);
            permissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
            AppDomainSetup domainSetup = new AppDomainSetup
            {
                ApplicationBase = path
            };

            return AppDomain.CreateDomain(name, null, domainSetup, permissionSet);
        }

        public static List<CalculatorProxy> GetCalculatorProxyList(string libraryPath)
        {
            Console.WriteLine($"Executing in {AppDomain.CurrentDomain.FriendlyName} domain");
            var calculators = new List<CalculatorProxy>();
            var assembly = Assembly.LoadFrom(libraryPath);

            foreach (var type in assembly.GetTypes().Where(p => typeof(ICalculator).IsAssignableFrom(p)
                                                                && p.IsClass))
            {
                var ad = CreateRestrictedDomain(Path.GetDirectoryName(libraryPath),
                    $"{Path.GetFileName(libraryPath)} domain");
                var calc = (CalculatorProxy) Activator.CreateInstanceFrom(
                    ad,
                    typeof(CalculatorProxy).Assembly.Location,
                    typeof(CalculatorProxy).FullName
                ).Unwrap();
                calc.AssemblyName = assembly.FullName;
                calc.TypeName = type.FullName;

                calculators.Add(calc);
            }

            return calculators;
        }
    }
}