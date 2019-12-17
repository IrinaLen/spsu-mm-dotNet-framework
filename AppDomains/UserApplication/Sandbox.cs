using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using CalculatorInterfaceLibrary;

namespace UserApplication
{
    public sealed class Sandbox : MarshalByRefObject
    {
        public static void Run(Action<Sandbox> action)
        {
            var adSetup = new AppDomainSetup()
            {
                ApplicationBase = Path.GetFullPath(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
            };
            var sandboxAD = AppDomain.CreateDomain("Sandbox",null,adSetup);
            
            var sandbox = (Sandbox)sandboxAD.CreateInstanceFromAndUnwrap(
                typeof(Sandbox).Assembly.Location,
                typeof(Sandbox).FullName
            );
            action(sandbox);
            AppDomain.Unload(sandboxAD);
            
        }
        
        //method executes inside sandbox
        public void Invoke(Action<string> action,string str)
        {
            action(str);
        }
    }
    
}