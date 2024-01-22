using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CodeGenerator
{
    public class BlasFunction
    {
        private static readonly Regex regFunctionName = new Regex(@"^[^\s]+\s+(cblas.+)\(");
        private static readonly Regex regArguments = new Regex(@"\((.+)\)");
        private static readonly Regex regReturnType = new Regex(@"^[^\s]+");

        private string name;
        private IReadOnlyList<Argument> arguments;
        private string returnType;

        public BlasFunction(IEnumerable<string> lines)
        {
            var data = string.Concat(lines);

            // Dirty hack!
            data = data.Replace("float*b", "float *b");

            if (data.Last() != ';')
            {
                throw new Exception("Something went wrong!");
            }

            var functionName = regFunctionName.Match(data).Groups[1].Value;
            var rawArguments = regArguments.Match(data).Groups[1].Value.Split(',', StringSplitOptions.TrimEntries);
            var arguments = rawArguments.Select(raw =>
            {
                var split = raw.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                string type = string.Join(' ', split.Take(split.Length - 1).Where(value => value != "OPENBLAS_CONST"));
                string name = split.Last();
                if (name.StartsWith("*"))
                {
                    type += "*";
                    name = name.Substring(1);
                }
                return new Argument(type, name);
            }).ToArray();
            var returnType = regReturnType.Match(data).Value;

            this.name = functionName;
            this.arguments = arguments;
            this.returnType = returnType;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(returnType);
            sb.Append(" ");
            sb.Append(name);
            sb.Append("(");
            sb.Append(arguments.First());
            foreach (var argument in arguments.Skip(1))
            {
                sb.Append(", ");
                sb.Append(argument);
            }
            sb.Append(");");
            return sb.ToString();
        }

        private bool IsSuppoerted
        {
            get
            {
                return arguments.All(arg => arg.Name != "...");
            }
        }

        public static IReadOnlyList<BlasFunction> FromHeaderFile(string path)
        {
            var readingFunction = false;
            var buffer = new List<string>();
            var functions = new List<BlasFunction>();
            foreach (var line in File
                .ReadLines(path)
                .SkipWhile(line => line != "typedef CBLAS_ORDER CBLAS_LAYOUT;")
                .TakeWhile(line => line != "/*** BFLOAT16 and INT8 extensions ***/")
                .Where(line => !line.StartsWith("/***"))
                .Select(line => line.Trim()))
            {
                if (regFunctionName.Match(line).Success)
                {
                    readingFunction = true;
                }

                if (readingFunction)
                {
                    buffer.Add(line);

                    if (line.Last() == ';')
                    {
                        var function = new BlasFunction(buffer);
                        if (function.IsSuppoerted)
                        {
                            functions.Add(function);
                        }

                        readingFunction = false;
                        buffer.Clear();
                    }
                }
            }

            return functions;
        }

        public string Name => name;
        public IReadOnlyList<Argument> Arguments => arguments;
        public string ReturnType => returnType;



        public class Argument
        {
            private string type;
            private string name;

            public Argument(string type, string name)
            {
                this.type = type;
                this.name = name;
            }

            public override string ToString()
            {
                return type + " " + name;
            }

            public string Type => type;
            public string Name => name;
        }
    }
}
