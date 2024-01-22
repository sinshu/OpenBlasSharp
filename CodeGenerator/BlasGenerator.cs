using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace CodeGenerator
{
    public static class BlasGenerator
    {
        private static readonly string cblasHeaderFile = @"..\..\..\..\OpenBLAS\cblas.h";

        public static void Run()
        {
            var dstDir = Directory.CreateDirectory("blas");

            var functions = BlasFunction.FromHeaderFile(cblasHeaderFile);

            foreach (var f in functions)
            {
                Console.WriteLine(f);
            }
        }
    }
}