using System;
using System.IO;
using System.Security;
using System.Security.Permissions;
using Calculators;

namespace ConsoleApplication1
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var perm = new PermissionSet(PermissionState.None);
            perm.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
            string path = Path.GetPathRoot(Directory.GetCurrentDirectory());
            perm.AddPermission(new FileIOPermission(FileIOPermissionAccess.NoAccess, path));
            var setup = new AppDomainSetup();
            setup.ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            
            
            AppDomain securedDomain = AppDomain.CreateDomain("securedDomain", null, setup, perm);
            Type thirdParty = typeof(HackingCalculator);
            securedDomain.CreateInstanceAndUnwrap(thirdParty.Assembly.FullName, thirdParty.FullName);
            AppDomain.Unload(securedDomain);
        }
    }
}