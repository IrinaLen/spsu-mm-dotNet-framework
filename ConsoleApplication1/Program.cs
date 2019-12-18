using System;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Linq;
using System.Collections.Generic;
using Calculators;

namespace ConsoleApplication1
{
    public class Program : MarshalByRefObject
    {
        public static void Main(string[] args)
        {
            var result = BaseClass().Run(5, 6);
            foreach (var current in result)
            {
                if (current.result == null)
                {
                    Console.WriteLine(current.implementationName + " is vulnerable");
                } else
                {
                    Console.WriteLine(current.implementationName + " result is: " + current.result.value);
                }
            }
            Console.Read();
        }

        public static Program BaseClass()
        {
            var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var setup = new AppDomainSetup { ApplicationBase = Path.GetFullPath(baseDir) };
            var wholeDomain = AppDomain.CreateDomain("Main Domain", null, setup);
            var curentClass = (Program)wholeDomain.CreateInstanceAndUnwrap(typeof(Program).Assembly.FullName, typeof(Program).FullName);
            return curentClass;
        }

        public List<TypeInfo> Implementations()
        {
            var solutionDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            var pathToLibrary = Path.Combine(solutionDirectory, "Calculators", "bin", "debug", "Calculators.dll");
            var assembly = Assembly.LoadFrom(pathToLibrary);
            var calculatorImplementations = assembly.DefinedTypes.Where((type) => type.ImplementedInterfaces.Contains(typeof(ICalculator.ICalculator)));

            return calculatorImplementations.ToList();
        }

        public PermissionSet Permission()
        {
            var perm = new PermissionSet(PermissionState.None);
            perm.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
            string path = Path.GetPathRoot(Directory.GetCurrentDirectory());
            perm.AddPermission(new FileIOPermission(FileIOPermissionAccess.NoAccess, path));
            return perm;
        }

        public AppDomainSetup DomainSetup() 
        {
            var setup = new AppDomainSetup();
            setup.ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            return setup;
        }

        public CalculatorResult Calculate(int a, int b, TypeInfo typeInfo, AppDomainSetup domainSetup, PermissionSet perm)
        {
            AppDomain securedDomain = AppDomain.CreateDomain(typeInfo.Name, null, domainSetup, perm);
            Type thirdParty = typeof(CurrentCalculator);
            var instance = securedDomain.CreateInstanceAndUnwrap(thirdParty.Assembly.FullName, thirdParty.FullName);
            var calculator = (CurrentCalculator)instance;
            var result = new CalculatorResult(typeInfo.Name);
            result.implementationName = typeInfo.Name;
            try
            {
                result.result = new CalculatorResult.ResultValue(calculator.calc(a, b, typeInfo));
            }
            catch (SecurityException exception)
            {
            }
            finally
            {
                AppDomain.Unload(securedDomain);
            }
            return result;
        }


        public List<CalculatorResult> Run(int a, int b)
        {

            var calculatorImplementations = Implementations();
            var perm = Permission();
            var domainSetup = DomainSetup();

            var result = new List<CalculatorResult>();

            foreach(var calc in calculatorImplementations)
            {
                result.Add(Calculate(a, b, calc, domainSetup, perm));
            }
            return result;
        }
    }

    public class CalculatorResult : MarshalByRefObject
    {
        public CalculatorResult(string name)
        {
            implementationName = name;
        }
        public class ResultValue : MarshalByRefObject
        {
            public ResultValue(int asvalue)
            {
                value = asvalue;
            }
            public int value;
        }

        public ResultValue result;

        public string implementationName;
    }

    public class CurrentCalculator : MarshalByRefObject
    {
        public int calc(int a, int b, TypeInfo implementation)
        {
            var calculator = (ICalculator.ICalculator)Activator.CreateInstance(implementation);
            return calculator.Sum(a, b);
        }
    }
}