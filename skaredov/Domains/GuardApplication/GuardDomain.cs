using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using ICalculatorLibrary;

namespace GuardApplication
{
    public sealed class GuardDomain: MarshalByRefObject
    {
        // execute any action in safe environment
        public static void Execute(string name, Action<GuardDomain> action)
        {
            Program.PrintCurrentDomain();
            Console.WriteLine($"\t1. Create general restricted domain '{name}'");
            AppDomain domain = null;
            try
            {
                var setup = new AppDomainSetup
                {
                    ApplicationBase = Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                };
                domain = AppDomain.CreateDomain(name, null, setup);
                var guardian = (GuardDomain) domain.CreateInstanceFromAndUnwrap(typeof(GuardDomain).Assembly.Location, typeof(GuardDomain).FullName);

                Console.WriteLine("\t2. Run safe execution inside it");
                action(guardian);
            }
            finally
            {
                if (domain != null)
                    AppDomain.Unload(domain);
            }
        }

        public IEnumerable<GuardCalculator> LoadFromAssembly(string path)
        {
            Program.PrintCurrentDomain();
            Console.WriteLine($"\tGuardCalculator.LoadFromAssembly: {path}");
            var calculators = new List<GuardCalculator>();
            var assembly = Assembly.LoadFrom(path);
            foreach (var type in assembly.DefinedTypes)
            {
                if (!type.ImplementedInterfaces.Contains(typeof(ICalculator))) continue;
                Console.WriteLine($"\tLoad {type.FullName}");
                var calculator = GetInstanceInDomain<GuardCalculator>(type.FullName, Path.GetDirectoryName(path));
                calculator.TypeName = type.FullName;
                calculator.AssemblyName = assembly.FullName;
                calculators.Add(calculator);
            }

            return calculators.ToArray();
        }

        public T GetInstanceInDomain<T>(string name, string path)
        {
            var setup = new AppDomainSetup
            {
                ApplicationBase = path
            };
            // it seems like mono doesn't have implementation of Code Access Security
            // so, permission restrictions doesn't work on mac os
            var permission = new PermissionSet(PermissionState.None);
            permission.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));

            var domain = AppDomain.CreateDomain(name, null, setup, permission);
            var instance = (T) Activator.CreateInstanceFrom(domain, typeof(T).Assembly.Location, typeof(T).FullName).Unwrap();
            return instance;
        }
    }
}