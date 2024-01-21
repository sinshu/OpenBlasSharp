using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace CodeGenerator
{
    public static class Program
    {
        private static readonly string lapackeHeaderFile = @"..\..\..\..\OpenBLAS\lapacke.h";
        private static readonly string lapackNetlibSrcDirectory = @"..\..\..\..\OpenBLAS\lapack-netlib-src";

        public static void Main(string[] args)
        {
            var dstDir = Directory.CreateDirectory("lapack");

            var functions = LapackFunction.FromHeaderFile(lapackeHeaderFile);

            foreach (var function in functions.Where(func => func.Name.EndsWith("dgesvd")))
            {
                var srcPath = Path.Combine(lapackNetlibSrcDirectory, function.Name.Substring(8) + ".c");

                FunctionDescription description;
                try
                {
                    description = new FunctionDescription(srcPath);
                    ProcessLapackFunction(function, description, dstDir.FullName);
                    Console.WriteLine(function.Name + " ... OK");
                }
                catch (Exception e)
                {
                    Console.WriteLine(function.Name + " ... " + e.Message);
                }
            }
        }

        private static void ProcessLapackFunction(LapackFunction function, FunctionDescription description, string dstDir)
        {
            var csFuncName = ToPascalCase(function.Name.Substring(8));
            var dstFile = Path.Combine(dstDir, csFuncName + ".cs");
            using (var writer = new StreamWriter(dstFile, false, Encoding.UTF8))
            {
                writer.WriteLine("using System.Numerics;");
                writer.WriteLine("using System.Runtime.InteropServices;");
                writer.WriteLine();
                writer.WriteLine("namespace OpenBlasSharp");
                writer.WriteLine("{");
                writer.WriteLine("    public static partial class Lapack");
                writer.WriteLine("    {");

                writer.WriteLine("        /// <summary>");
                foreach (var line in description.Purpose)
                {
                    writer.WriteLine("        /// " + WebUtility.HtmlEncode(line));
                }
                writer.WriteLine("        /// </summary>");
                foreach (var arg in function.Arguments)
                {
                    writer.WriteLine("        /// <param name=\"" + ToCamelCase(arg.Name) + "\">");
                    var doc = description.GetParam(arg.Name);
                    if (doc != null)
                    {
                        writer.WriteLine("        /// " + GetInOut(doc.Type) + " " + WebUtility.HtmlEncode(doc.Description[0]) + ".");
                        foreach (var line in doc.Description.Skip(1))
                        {
                            writer.WriteLine("        /// " + WebUtility.HtmlEncode(line));
                        }
                    }
                    else
                    {
                        if (arg.Name == "matrix_layout")
                        {
                            writer.WriteLine("        /// Specifies the matrix layout.");
                        }
                        else
                        {
                            writer.WriteLine("        /// No description available.");
                        }
                    }
                    writer.WriteLine("        /// </param>");
                }
                if (description.Remarks.Count > 0)
                {
                    writer.WriteLine("        /// <remarks>");
                    foreach (var line in description.Remarks)
                    {
                        writer.WriteLine("        /// " + WebUtility.HtmlEncode(line));
                    }
                    writer.WriteLine("        /// </remarks>");
                }

                writer.WriteLine("        [DllImport(\"libopenblas\", EntryPoint = \"" + function.Name + "\", CallingConvention = CallingConvention.Cdecl)]");
                writer.WriteLine("        public static extern unsafe LapackInfo " + csFuncName + "(");
                writer.Write("            " + GetNetType(function.Arguments[0]));
                foreach (var arg in function.Arguments.Skip(1))
                {
                    writer.WriteLine(",");
                    writer.Write("            " + GetNetType(arg));
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

        private static string GetNetType(LapackFunction.Argument arg)
        {
            if (arg.Type == "int" && arg.Name == "matrix_layout")
            {
                return "MatrixLayout matrixLayout";
            }

            string netType;
            switch (arg.Type)
            {
                case "int":
                    netType = "int";
                    break;
                case "lapack_int":
                    netType = "int";
                    break;
                case "lapack_int*":
                    netType = "int*";
                    break;
                case "char":
                    netType = "char";
                    break;
                case "double*":
                    netType = "double*";
                    break;
                default:
                    throw new Exception("Unknown type: " + arg.Type);
            }

            return netType + " " + ToCamelCase(arg.Name);
        }
    }
}
