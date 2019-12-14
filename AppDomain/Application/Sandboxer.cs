using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using InterfaceLibrary;

namespace Application
{
  public class Sandboxer : MarshalByRefObject
  {
    public static void RunInSandbox(Action<Sandboxer> action)
    {
      var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
      var adSetup = new AppDomainSetup {ApplicationBase = Path.GetFullPath(baseDir)};
      var sandboxAd = AppDomain.CreateDomain("Sandbox", null, adSetup);
      var sandbox = (Sandboxer)sandboxAd.CreateInstanceFromAndUnwrap(
        typeof(Sandboxer).Assembly.Location,
        typeof(Sandboxer).FullName
      );
      action.Invoke(sandbox);
      AppDomain.Unload(sandboxAd);
    }
    public IEnumerable<CalculatorProxy> GetCalculators(string pathToAssembly)
    {
      var assembly = Assembly.LoadFrom(pathToAssembly);
      var baseDir = Path.GetDirectoryName(pathToAssembly);
      List<CalculatorProxy> calculators = new List<CalculatorProxy>();
      foreach (var typeInfo in assembly.DefinedTypes)
      {
        if (typeInfo.ImplementedInterfaces.Contains(typeof(ICalculator)))
        {
          var permSet = new PermissionSet(PermissionState.None);
          permSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
          var adSetup = new AppDomainSetup {ApplicationBase = baseDir};
          var ad = AppDomain.CreateDomain($"AD {typeInfo.FullName}", null, adSetup, permSet);
          var calc = (CalculatorProxy)Activator.CreateInstanceFrom(
            ad, 
            typeof(CalculatorProxy).Assembly.Location,
            typeof(CalculatorProxy).FullName
          ).Unwrap();
          calc.AssemblyName = assembly.FullName;
          calc.TypeName = typeInfo.FullName;
          
          calculators.Add(calc);
        }
      }

      return calculators.ToArray();
    }
  }
}
