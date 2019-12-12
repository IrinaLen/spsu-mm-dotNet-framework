using System;
using System.IO;
using System.Security;
using System.Security.Policy;
using System.Security.Permissions;
using System.Reflection;
using System.Runtime.Remoting;

namespace myCalcApp
{
    class Sandboxer : MarshalByRefObject
    {
        const string pathToUntrusted = @"..\..\..\..\CalcRealizationLib\CalcRealizationLib\bin\Debug";
        const string untrustedAssembly = "CalcRealizationLib";
        const string untrustedClass = "CalcRealizationLib.CalcRealization";
        const string harmlessMethod = "Sum";
        const string maliciousMethod = "ReadFileSystem";
        private static Object[] parameters = { 45, 25 };
        private static Object[] emptyParameter = { };

        static void Main()
        {
            //Setting the AppDomainSetup
            AppDomainSetup adSetup = new AppDomainSetup();
            adSetup.ApplicationBase = Path.GetFullPath(pathToUntrusted);

            //Setting the permissions for the AppDomain.
            PermissionSet permSet = new PermissionSet(PermissionState.None);
            permSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));

            AppDomain newDomain = AppDomain.CreateDomain("Sandbox", null, adSetup, permSet);
            Console.WriteLine("PermissionSet of newDomain:");
            Console.WriteLine(newDomain.PermissionSet);
            Console.WriteLine("Press enter");
            Console.ReadLine();

            //Use CreateInstanceFrom to load an instance of the Sandboxer class into the
            //new AppDomain. 
            ObjectHandle handle = Activator.CreateInstanceFrom(
                newDomain, typeof(Sandboxer).Assembly.ManifestModule.FullyQualifiedName,
                typeof(Sandboxer).FullName
                );

            //Unwrap the new domain instance into a reference in this domain and use it to execute the 
            //untrusted code.
            Sandboxer newDomainInstance = (Sandboxer)handle.Unwrap();
            
            Console.WriteLine("Try to use " + harmlessMethod + " of " + parameters[0].ToString() + " and " + parameters[1].ToString());
            newDomainInstance.ExecuteUntrustedCode(untrustedAssembly, untrustedClass, harmlessMethod, parameters);
            Console.WriteLine("Press enter");
            Console.ReadLine();

            Console.WriteLine("Try to run hacker method");
            newDomainInstance.ExecuteUntrustedCode(untrustedAssembly, untrustedClass, maliciousMethod, emptyParameter);
            Console.WriteLine("Press enter");
            Console.ReadLine();
        }
        public void ExecuteUntrustedCode(string assemblyName, string typeName, string entryPoint, Object[] parameters)
        {
            Assembly asm = Assembly.Load(assemblyName);
            Type t = asm.GetType(typeName, true, true);

            object obj = Activator.CreateInstance(t);

            MethodInfo method = t.GetMethod(entryPoint);
            try
            {
                object result = method.Invoke(obj, parameters);
                Console.WriteLine("Result is:" + result.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("AHA! WE CATCH THE HACKER!");
                Console.WriteLine("SecurityException caught:\n{0}", ex.ToString());
            }
        }
    }
}
