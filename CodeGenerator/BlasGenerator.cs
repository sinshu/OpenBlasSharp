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
        private static readonly string blasNetlibSrcDirectory = @"..\..\..\..\OpenBLAS\blas-netlib-src";

        public static void Run()
        {
            var dstDir = Directory.CreateDirectory("blas");

            var functions = BlasFunction.FromHeaderFile(cblasHeaderFile);

            foreach (var function in functions)
            {
                var srcPath = Path.Combine(blasNetlibSrcDirectory, function.Name.Substring(6) + ".f");
                var description = FunctionDescription.Empty;

                try
                {
                    description = new FunctionDescription(srcPath);
                    Console.WriteLine(function.Name + " ... OK");
                }
                catch (Exception e)
                {
                    Console.WriteLine(function.Name + " ... " + e.Message);
                }

                ProcessLapackFunction(function, description, dstDir.FullName);
            }
        }

        private static void ProcessLapackFunction(BlasFunction function, FunctionDescription description, string dstDir)
        {
            var csFuncName = ToPascalCase(function.Name.Substring(6));
            var dstFile = Path.Combine(dstDir, csFuncName + ".cs");
            using (var writer = new StreamWriter(dstFile, false, Encoding.UTF8))
            {
                writer.WriteLine("#pragma warning disable CS1591");
                writer.WriteLine();
                writer.WriteLine("using System;");
                writer.WriteLine("using System.Numerics;");
                writer.WriteLine("using System.Runtime.InteropServices;");
                writer.WriteLine();
                writer.WriteLine("namespace OpenBlasSharp");
                writer.WriteLine("{");
                writer.WriteLine("    public static partial class Blas");
                writer.WriteLine("    {");

                foreach (var arg in function.Arguments)
                {
                    writer.WriteLine("        /// <summary>");
                    if (description.Purpose.Count > 0)
                    {
                        foreach (var line in description.Purpose)
                        {
                            writer.WriteLine("        /// " + WebUtility.HtmlEncode(line));
                        }
                    }
                    else
                    {
                        writer.WriteLine("        /// No description available.");
                    }
                    writer.WriteLine("        /// </summary>");

                    writer.WriteLine("        /// <param name=\"" + ToCamelCase(arg.Name) + "\">");
                    var doc = description.GetParam(arg.Name);
                    if (doc != null)
                    {
                        var first = WebUtility.HtmlEncode(doc.Description[0]);
                        if (first.Last() != '.')
                        {
                            first += ".";
                        }
                        writer.WriteLine("        /// " + GetInOut(doc.Type) + " " + first);
                        foreach (var line in doc.Description.Skip(1))
                        {
                            writer.WriteLine("        /// " + WebUtility.HtmlEncode(line));
                        }
                    }
                    writer.WriteLine("        /// </param>");
                }

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

        private static string GetInOut(FunctionDescription.ParamType type)
        {
            switch (type)
            {
                case FunctionDescription.ParamType.In:
                    return "[in]";
                case FunctionDescription.ParamType.Out:
                    return "[out]";
                case FunctionDescription.ParamType.InOut:
                    return "[in,out]";
                default:
                    throw new Exception();
            }
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
