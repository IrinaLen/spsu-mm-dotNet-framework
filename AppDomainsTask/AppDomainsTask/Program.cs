using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using CalculatorInterface;

namespace AppDomainsTask
{
    // Just convenient way of executing a piece of code (which returns something) in new domain
    public static class NewAppDomain
    {
        public static T Execute<T>(Func<T> action, string name)
        {
            AppDomain domain = null;

            try
            {
                domain = AppDomain.CreateDomain(name);

                var domainDelegate = (AppDomainDelegate)domain.CreateInstanceAndUnwrap(
                    typeof(AppDomainDelegate).Assembly.FullName,
                    typeof(AppDomainDelegate).FullName);

                return domainDelegate.Execute(action);
            }
            finally
            {
                if (domain != null)
                    AppDomain.Unload(domain);
            }
        }

        // To execute our Action, we need to create a new instance of Action in our new domain.
        // However, trying to call the CreateInstanceAndUnwrap method on the new application domain will throw an exception.
        // We need to create a new class that can be instantiated in the new application domain.
        // This new class AppDomainDelegate will act as a delgate and execute the Action that we pass to it.

        private class AppDomainDelegate: MarshalByRefObject
        {
            public T Execute<T>(Func<T> action)
            {
                return action();
            }
        }
    }

    // Need this for Activator when creating new unknown class which implements ICalculator
    public sealed class CalculatorWrapper: MarshalByRefObject, ICalculator
    {
        // This field is for testing actually
        public AppDomain HomeDomain = AppDomain.CurrentDomain;

        public string AssemblyName;
        public string TypeName;

        public int Sum(int a, int b)
        {
            var calcType = Assembly.Load(AssemblyName).GetType(TypeName);
            var calcConstructor = calcType.GetConstructor(new Type[] {});
            var calculator = (ICalculator)calcConstructor.Invoke(new object[] {});
            
            return calculator.Sum(a, b);
        }
    }

    class Program
    {
        static void Main()
        {
            Console.WriteLine($"Program started in: {AppDomain.CurrentDomain.FriendlyName}");

            // CalculatorLibrary.dll should never be loaded in main domain
            var calcs = NewAppDomain.Execute(() =>
            {
                Console.WriteLine($"CalculatorLibrary.dll will be loaded in: {AppDomain.CurrentDomain.FriendlyName}");

                // Store all created instances of found classes here
                List<CalculatorWrapper> calculators = new List<CalculatorWrapper>();

                // Load CalculatorLibrary.dll
                string pathToAssembly = @"../../../CalculatorLibrary/bin/Debug/CalculatorLibrary.dll";
                var assembly = Assembly.LoadFrom(pathToAssembly);

                var baseDir = Path.GetDirectoryName(pathToAssembly);

                // Find all implementations if ICalculator interface
                foreach (var type in assembly.DefinedTypes)
                {
                    if (type.ImplementedInterfaces.Contains(typeof(ICalculator)))
                    {
                        // Create new domain with restricted permissions to create instance of found class in it
                        AppDomainSetup setup = new AppDomainSetup { ApplicationBase = baseDir };
                        var permissions = new PermissionSet(PermissionState.None);
                        permissions.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
                        AppDomain domain = AppDomain.CreateDomain($"AppDomain {type.FullName}", null, setup, permissions);

                        var calculator = (CalculatorWrapper)Activator.CreateInstanceFrom(domain, typeof(CalculatorWrapper).Assembly.Location,
                            typeof(CalculatorWrapper).FullName).Unwrap();

                        calculator.AssemblyName = assembly.FullName;
                        calculator.TypeName = type.FullName;

                        calculators.Add(calculator);

                    }
                }
                return calculators;
            }, "SandboxDomain");

            // Lets test!
            Console.WriteLine("\nTesting...");
            Console.WriteLine("----------------");

            Console.WriteLine("Test 1: List all calculators and their domains\n");
            Console.WriteLine("Test 1: Start\n");

            foreach (var calc in calcs)
            {
                Console.WriteLine($"Calculator Name: {calc.TypeName}");
                Console.WriteLine($"Calculator Domain: {calc.HomeDomain.FriendlyName}\n");
            }
            Console.WriteLine("\nTest 1: Finish");

            Console.WriteLine("----------------");
            Console.WriteLine("");
            Console.WriteLine("----------------");
            Console.WriteLine("Test 2: test .Sum(10, 4) in each calculator");
            Console.WriteLine("For HackerCalculator exception is expected as it tries to write!\n");
            Console.WriteLine("Test 2: Start\n");

            foreach (var calc in calcs)
            {
                if (calc.TypeName == "CalculatorLibrary.HackerCalculator") {
                    Console.WriteLine($"Calculator Name: {calc.TypeName}");
                    try {
                        Console.WriteLine($"Calculator Sum(): {calc.Sum(10, 4)}\n");
                    }
                    catch (SecurityException)
                    {
                        Console.WriteLine("SecurityException was caught as expected!");
                    }

                }
                else {
                    Console.WriteLine($"Calculator Name: {calc.TypeName}");
                    Console.WriteLine($"Calculator Sum(): {calc.Sum(10, 4)}\n");
                }
            }
            
            Console.WriteLine("\nTest 2: Finish");
            Console.WriteLine("----------------");
            Console.WriteLine("");
        }
    }
}
