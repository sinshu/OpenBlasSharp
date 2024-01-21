using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CodeGenerator
{
    public class LapackFunction
    {
        private static readonly string returnType = "lapack_int";
        private static readonly Regex regFunctionName = new Regex(@"lapack_int\s(LAPACKE_.+)\(");
        private static readonly Regex regArguments = new Regex(@"\((.+)\)");

        private string name;
        private IReadOnlyList<Argument> arguments;

        public LapackFunction(IEnumerable<string> lines)
        {
            var data = string.Concat(lines);

            if (!data.StartsWith("lapack_int LAPACKE_"))
            {
                throw new Exception("Something went wrong!");
            }

            if (data.Last() != ';')
            {
                throw new Exception("Something went wrong!");
            }

            var functionName = regFunctionName.Match(data).Groups[1].Value;
            var rawArguments = regArguments.Match(data).Groups[1].Value.Split(',', StringSplitOptions.TrimEntries);
            var arguments = rawArguments.Select(raw =>
            {
                var split = raw.Split(' ');
                string type;
                string name;
                if (split.Length == 2)
                {
                    type = split[0];
                    name = split[1];
                }
                else if (split.Length == 3)
                {
                    type = split[0] + " " + split[1];
                    name = split[2];
                }
                else
                {
                    throw new Exception("Something went wrong!");
                }
                return new Argument(type, name);
            }).ToArray();

            this.name = functionName;
            this.arguments = arguments;
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
                if (name.EndsWith("_work"))
                {
                    return false;
                }

                if (arguments.Any(arg => arg.Type.Contains("SELECT")))
                {
                    return false;
                }

                return true;
            }
        }

        public static IReadOnlyList<LapackFunction> FromHeaderFile(string path)
        {
            var readingFunction = false;
            var buffer = new List<string>();
            var functions = new List<LapackFunction>();
            foreach (var line in File.ReadLines(path))
            {
                if (line.StartsWith("lapack_int LAPACKE_"))
                {
                    readingFunction = true;
                }

                if (readingFunction)
                {
                    buffer.Add(line);

                    if (line.Last() == ';')
                    {
                        var function = new LapackFunction(buffer);
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
