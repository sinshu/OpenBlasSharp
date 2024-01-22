using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace CodeGenerator
{
    public static class LapackGenerator
    {
        private static readonly string lapackeHeaderFile = @"..\..\..\..\OpenBLAS\lapacke.h";
        private static readonly string lapackNetlibSrcDirectory = @"..\..\..\..\OpenBLAS\lapack-netlib-src";

        public static void Run()
        {
            var dstDir = Directory.CreateDirectory("lapack");

            var functions = LapackFunction.FromHeaderFile(lapackeHeaderFile);

            foreach (var function in functions)
            {
                var srcPath = Path.Combine(lapackNetlibSrcDirectory, function.Name.Substring(8) + ".f");
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

        private static void ProcessLapackFunction(LapackFunction function, FunctionDescription description, string dstDir)
        {
            var csFuncName = ToPascalCase(function.Name.Substring(8));
            var dstFile = Path.Combine(dstDir, csFuncName + ".cs");
            using (var writer = new StreamWriter(dstFile, false, Encoding.UTF8))
            {
                writer.WriteLine("using System;");
                writer.WriteLine("using System.Numerics;");
                writer.WriteLine("using System.Runtime.InteropServices;");
                writer.WriteLine();
                writer.WriteLine("namespace OpenBlasSharp");
                writer.WriteLine("{");
                writer.WriteLine("    public static partial class Lapack");
                writer.WriteLine("    {");

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

                foreach (var arg in function.Arguments)
                {
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

                if (description.GetParam("info") != null)
                {
                    var doc = description.GetParam("info")!;
                    writer.WriteLine("        /// <returns>");
                    foreach (var line in doc.Description.Skip(1))
                    {
                        writer.WriteLine("        /// " + WebUtility.HtmlEncode(line));
                    }
                    writer.WriteLine("        /// </returns>");
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

                writer.WriteLine("        [DllImport(OpenBlas.LibraryName, EntryPoint = \"" + function.Name + "\", CallingConvention = CallingConvention.Cdecl)]");
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
                case "const lapack_int*":
                    netType = "int*";
                    break;
                case "float":
                    netType = "float";
                    break;
                case "float*":
                    netType = "float*";
                    break;
                case "const float*":
                    netType = "float*";
                    break;
                case "double":
                    netType = "double";
                    break;
                case "double*":
                    netType = "double*";
                    break;
                case "const double*":
                    netType = "double*";
                    break;
                case "lapack_complex_float":
                    netType = "Complex32";
                    break;
                case "lapack_complex_float*":
                    netType = "Complex32*";
                    break;
                case "const lapack_complex_float*":
                    netType = "Complex32*";
                    break;
                case "lapack_complex_double":
                    netType = "Complex";
                    break;
                case "lapack_complex_double*":
                    netType = "Complex*";
                    break;
                case "const lapack_complex_double*":
                    netType = "Complex*";
                    break;
                case "char":
                    netType = "char";
                    break;
                case "char*":
                    netType = "ref char";
                    break;
                case "lapack_logical":
                    netType = "bool";
                    break;
                case "lapack_logical*":
                    netType = "bool*";
                    break;
                case "const lapack_logical*":
                    netType = "bool*";
                    break;
                default:
                    throw new Exception("Unknown type: " + arg.Type);
            }

            if (arg.Name == "params")
            {
                return netType + " @" + ToCamelCase(arg.Name);
            }
            else
            {
                return netType + " " + ToCamelCase(arg.Name);
            }
        }
    }
}
