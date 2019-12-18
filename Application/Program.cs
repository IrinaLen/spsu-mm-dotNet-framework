using System;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Linq;
using System.Collections.Generic;

namespace Application
{
    public class Program : MarshalByRefObject
    {
        public static void Main(string[] args)
        {
            var results = BaseClass().Run(5, 6);
            foreach (var result in results)
            {
                if (result.Result == null)
                {
                    Console.WriteLine(result.implementationName + " is vulnerable");
                } else
                {
                    Console.WriteLine(result.implementationName + " result is: " + result.Result.Value);
                }
            }
            Console.Read();
        }

        public static Program BaseClass()
        {
            var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var appDomainSetup = new AppDomainSetup { ApplicationBase = Path.GetFullPath(baseDir) };
            var appDomain = AppDomain.CreateDomain("Main Domain", null, appDomainSetup);
            var instance = (Program)appDomain.CreateInstanceAndUnwrap(typeof(Program).Assembly.FullName, typeof(Program).FullName);
            return instance;
        }

        public List<TypeInfo> GetCalculatorImplementations()
        {
            var solutionDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            var pathToLibrary = Path.Combine(solutionDirectory, "Calculators", "bin", "debug", "Calculators.dll");
            var assembly = Assembly.LoadFrom(pathToLibrary);
            var calculatorImplementations = assembly.DefinedTypes.Where((type) => type.ImplementedInterfaces.Contains(typeof(ICalculator.ICalculator)));
            return calculatorImplementations.ToList();
        }

        public PermissionSet GetPermissions()
        {
            var permissions = new PermissionSet(PermissionState.None);
            permissions.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
            string rootDirectory = Path.GetPathRoot(Directory.GetCurrentDirectory());
            permissions.AddPermission(new FileIOPermission(FileIOPermissionAccess.NoAccess, rootDirectory));
            return permissions;
        }

        public AppDomainSetup GetDomainSetup() 
        {
            var appDomainSetup = new AppDomainSetup();
            appDomainSetup.ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            return appDomainSetup;
        }

        public CalculatorResult Calculate(int a, int b, TypeInfo typeInfo, AppDomainSetup appDomainSetup, PermissionSet permissions)
        {
            AppDomain appDomain = AppDomain.CreateDomain(typeInfo.Name, null, appDomainSetup, permissions);
            Type thirdParty = typeof(CurrentCalculator);
            var instance = appDomain.CreateInstanceAndUnwrap(thirdParty.Assembly.FullName, thirdParty.FullName);
            var calculator = (CurrentCalculator)instance;
            var result = new CalculatorResult(typeInfo.Name);
            result.implementationName = typeInfo.Name;
            try
            {
                result.Result = new CalculatorResult.ResultValue(calculator.calc(a, b, typeInfo));
            }
            catch (SecurityException exception)
            {
            }
            finally
            {
                AppDomain.Unload(appDomain);
            }
            return result;
        }


        public List<CalculatorResult> Run(int a, int b)
        {

            var calculatorImplementations = GetCalculatorImplementations();
            var permissions = GetPermissions();
            var appDomainSetup = GetDomainSetup();

            var result = new List<CalculatorResult>();

            foreach(var calculator in calculatorImplementations)
            {
                result.Add(Calculate(a, b, calculator, appDomainSetup, permissions));
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
                Value = asvalue;
            }
            public int Value;
        }

        public ResultValue Result;

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