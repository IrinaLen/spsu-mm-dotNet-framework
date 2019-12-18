using System;
using System.IO;
using System.Security;

namespace GuardApplication
{
    public class Program
    {
        private static readonly string _root = Path.Combine("..", "..", "..");
        private static readonly string _dumb = "DumbCalculatorLibrary";
        private static readonly string _ultra = "UltraCalculatorLibrary";

        public static void Main(string[] args)
        {
            if (args.Length == 0) args = new[] {GetPathToAssembly(_dumb), GetPathToAssembly(_ultra)};

            PrintCurrentDomain();
            Console.WriteLine(
                "Explore assemblies and run every ICalculator.Sum(42 + 27) in separate domains:\n\t" +
                $"{string.Join("\n\t", args)}"
            );

            GuardDomain.Execute("General Guardian", guardian =>
            {
                foreach (var path in args)
                {
                    if (!File.Exists(path))
                    {
                        PrintDelimiter();
                        Console.WriteLine($"Assembly {path} does not exists. Skip it");
                    }

                    foreach (var calculator in guardian.LoadFromAssembly(path))
                    {
                        try
                        {
                            PrintCurrentDomain();
                            Console.WriteLine($"\tDomain from inside calculator: {calculator.Domain.FriendlyName}");
                            var result = calculator.Sum(42, 27);
                            Console.WriteLine($"\t\tResult {calculator.TypeName}.Sum(42, 27) = {result}");
                        }
                        catch (SecurityException e)
                        {
                            PrintDelimiter();
                            Console.WriteLine($"wow, someone was catched: {calculator.Domain.FriendlyName}");
                            Console.WriteLine(e.Message);
                        }
                        finally
                        {
                            AppDomain.Unload(calculator.Domain);
                        }
                    }
                }
            });
            Console.ReadKey();
        }

        #region utils

        public static string GetPathToBase(string assembly)
        {
            var bin = Path.Combine(_root, assembly, "bin");
            var conf = Directory.Exists(Path.Combine(bin, "Debug"))
                ? "Debug"
                : "Release";
            return Path.GetFullPath(Path.Combine(bin, conf));
        }

        public static string GetPathToAssembly(string assembly)
        {
            var path = GetPathToBase(assembly);
            return Path.Combine(path, assembly + ".dll");
        }

        public static void PrintCurrentDomain()
        {
            PrintDelimiter();
            Console.WriteLine($"\t\t{AppDomain.CurrentDomain.FriendlyName}");
            PrintDelimiter();
        }

        public static void PrintDelimiter()
        {
            Console.WriteLine("==============================================================================");
        }

        #endregion
    }
}