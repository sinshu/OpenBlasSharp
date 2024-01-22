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

            foreach (var function in functions)
            {
                ProcessLapackFunction(function, dstDir.FullName);
            }
        }

        private static void ProcessLapackFunction(BlasFunction function, string dstDir)
        {
            var csFuncName = ToPascalCase(function.Name.Substring(6));
            var dstFile = Path.Combine(dstDir, csFuncName + ".cs");
            using (var writer = new StreamWriter(dstFile, false, Encoding.UTF8))
            {
                writer.WriteLine("using System;");
                writer.WriteLine("using System.Numerics;");
                writer.WriteLine("using System.Runtime.InteropServices;");
                writer.WriteLine();
                writer.WriteLine("namespace OpenBlasSharp");
                writer.WriteLine("{");
                writer.WriteLine("    public static partial class Blas");
                writer.WriteLine("    {");

                writer.WriteLine("        [DllImport(OpenBlas.LibraryName, EntryPoint = \"" + function.Name + "\", CallingConvention = CallingConvention.Cdecl)]");
                writer.WriteLine("        public static extern unsafe " + GetNetType(function.ReturnType) + " " + csFuncName + "(");
                writer.Write("            " + GetNetType(function.Arguments[0].Type) + " " + function.Arguments[0].Name);
                foreach (var arg in function.Arguments.Skip(1))
                {
                    writer.WriteLine(",");
                    writer.Write("            " + GetNetType(arg.Type) + " " + arg.Name);
                }
                writer.WriteLine(");");

                writer.WriteLine("    }");
                writer.WriteLine("}");
            }
        }

        private static string ToPascalCase(string value)
        {
            var split = value.Split('_');
            var sb = new StringBuilder();
            foreach (var item in split)
            {
                var head = item.Substring(0, 1);
                var tail = item.Substring(1);
                sb.Append(head.ToUpper());
                sb.Append(tail);
            }
            return sb.ToString();
        }

        private static string ToCamelCase(string value)
        {
            var split = value.Split('_');
            var sb = new StringBuilder();
            sb.Append(split[0]);
            foreach (var item in split.Skip(1))
            {
                var head = item.Substring(0, 1);
                var tail = item.Substring(1);
                sb.Append(head.ToUpper());
                sb.Append(tail);
            }
            return sb.ToString();
        }

        private static string GetNetType(string arg)
        {
            switch (arg)
            {
                case "blasint":
                    return "int";
                case "float":
                    return "float";
                case "float*":
                    return "float*";
                case "double":
                    return "double";
                case "double*":
                    return "double*";
                case "void":
                    return "void";
                case "void*":
                    return "void*";
                case "enum CBLAS_ORDER":
                    return "Order";
                case "enum CBLAS_TRANSPOSE":
                    return "Transpose";
                case "enum CBLAS_UPLO":
                    return "Uplo";
                case "enum CBLAS_DIAG":
                    return "Diag";
                case "enum CBLAS_SIDE":
                    return "Side";
                case "openblas_complex_float":
                    return "Complex32";
                case "openblas_complex_double":
                    return "Complex";
                case "CBLAS_INDEX":
                    return "UIntPtr";
                default:
                    throw new Exception("Unknown type: " + arg);
            }
        }
    }
}
