using System;
using System.IO;
using System.Linq;

namespace CodeGenerator
{
    public static class Program
    {
        private static readonly string lapackeHeaderFile = @"..\..\..\..\OpenBLAS\lapacke.h";
        private static readonly string lapackNetlibSrcDirectory = @"..\..\..\..\OpenBLAS\lapack-netlib-src";

        public static void Main(string[] args)
        {
            var functions = LapackFunction.FromHeaderFile(lapackeHeaderFile);
            foreach (var func in functions)
            {
                var srcPath = Path.Combine(lapackNetlibSrcDirectory, func.Name.Substring(8) + ".c");

                FunctionDescription desc;
                try
                {
                    desc = new FunctionDescription(srcPath);
                }
                catch (Exception e)
                {
                    Console.WriteLine(func.Name + " -> " + e.Message);
                    Console.WriteLine();
                    continue;
                }

                var ngArgs = func.Arguments.Where(arg => desc.GetParam(arg.Name) == null).ToArray();
                var ok1 = ngArgs.Length == 0;
                var ok2 = ngArgs.Length == 1 && ngArgs[0].Name == "matrix_layout";
                if (!ok1 && !ok2)
                {
                    Console.WriteLine(func.Name);
                    foreach (var arg in func.Arguments)
                    {
                        if (desc.GetParam(arg.Name) != null)
                        {
                            Console.WriteLine("    " + arg.Name + ": OK");
                        }
                        else
                        {
                            Console.WriteLine("    " + arg.Name + ": Not found!");
                        }
                    }
                    Console.WriteLine();
                }
            }
        }
    }
}
