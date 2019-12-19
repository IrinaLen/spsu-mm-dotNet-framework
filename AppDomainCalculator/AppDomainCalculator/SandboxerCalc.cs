using System;
using System.IO;
using System.Security;
using System.Security.Policy;
using System.Security.Permissions;
using System.Reflection;
using System.Runtime.Remoting;
using System.Collections.Generic;
using System.Linq;
using CalcLibInterface;

namespace myCalcApp
{
    public class SandboxerCalc : MarshalByRefObject
    {
        static void Main()
        {
            string pathToAssemblyFolder = @"..\..\..\CalcRealizationLib\bin\Debug";
            string assemblyName = "CalcRealizationLib";
            string calcClass = "CalcRealizationLib.CalcRealization";
            string hackerClass = "CalcRealizationLib.HackerCalcRealization";
            string methodToExecuteName = "Sum";
            Object[] parameters = { 45, 25 };

            Console.WriteLine("Result of Sum(45,25) is:");
            Console.WriteLine(ExecuteUntrustedCode(pathToAssemblyFolder, assemblyName, calcClass, methodToExecuteName, parameters));

            Console.WriteLine("Try to run hacker method:");
            try
            {
                ExecuteUntrustedCode(pathToAssemblyFolder, assemblyName, hackerClass, methodToExecuteName, parameters);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.InnerException);
                Console.WriteLine("AHA we catched a hacker!");
            }
            Console.ReadKey();
        }

        public static object ExecuteUntrustedCode(string pathToRealization, string assemblyName, string className, string methodToExecuteName, object[] parameters)
        {
            //Setting the AppDomainSetup
            AppDomainSetup adSetup = new AppDomainSetup();
            adSetup.ApplicationBase = Path.GetFullPath(pathToRealization);

            //Setting the permissions for the AppDomain.
            PermissionSet permSet = new PermissionSet(PermissionState.None);
            permSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));

            AppDomain newDomain = AppDomain.CreateDomain(pathToRealization + assemblyName + methodToExecuteName, null, adSetup, permSet);


            //Use CreateInstanceFrom to load an instance of the Sandboxer class into the
            //new AppDomain. 
            ObjectHandle handle = Activator.CreateInstanceFrom(
                newDomain, typeof(SandboxerCalc).Assembly.ManifestModule.FullyQualifiedName,
                typeof(SandboxerCalc).FullName
                );

            //Unwrap the new domain instance into a reference in this domain and use it to execute the 
            //untrusted code.
            SandboxerCalc newDomainInstance = (SandboxerCalc)handle.Unwrap();
            try
            {
                return newDomainInstance.SafetyExecuteUntrustedCode(assemblyName, className, methodToExecuteName, parameters);
            }
            catch(Exception ex)
            {
                throw ex;
            }
            finally
            {
                AppDomain.Unload(newDomain);
            }
        }

        public object SafetyExecuteUntrustedCode(string assemblyName, string typeName, string entryPoint, Object[] parameters)
        {
            Assembly asm = Assembly.Load(assemblyName);
            Type t = asm.GetType(typeName, true, true);

            object obj = Activator.CreateInstance(t);

            MethodInfo method = t.GetMethod(entryPoint);
            return method.Invoke(obj, parameters);
        }
    }
}
