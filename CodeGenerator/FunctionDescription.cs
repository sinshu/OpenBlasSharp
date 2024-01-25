using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CodeGenerator
{
    public class FunctionDescription
    {
        private static readonly Regex regParamName = new Regex(@"param\[.+\]\s+([A-Z1-9_]+)\s*");
        private static readonly Regex regParamType = new Regex(@"param\[(.+)\]\s+[A-Z1-9_]+\s*");

        private static readonly FunctionDescription empty = new FunctionDescription();

        private string[] purpose;
        private Param[] paramList;
        private string[] remarks;

        public FunctionDescription(string path)
        {
            var state = ReadState.None;
            var count = 0;

            var purpose = new List<string>();

            string? paramName = null;
            string? paramType = null;
            var paramDescription = new List<string>();
            var paramList = new List<Param>();

            var remarks = new List<string>();

            foreach (var rawLine in File.ReadLines(path))
            {
                string line;
                if (rawLine.Length <= 3)
                {
                    line = "";
                }
                else
                {
                    line = rawLine.Substring(3);
                }

                if (line.Contains("Purpose:"))
                {
                    state = ReadState.Purpose;
                    count = 0;
                }
                else if (line.Contains("Arguments:"))
                {
                    state = ReadState.Arguments;
                    count = 0;
                }
                else if (line.Contains("Further Details:"))
                {
                    state = ReadState.FurtherDetails;
                    count = 0;
                }

                switch (state)
                {
                    case ReadState.Purpose:
                        if (count == 0 && line.Contains(@"\verbatim"))
                        {
                            count = 1;
                        }
                        else if (count == 1 && line.Trim() == "")
                        {
                            count = 2;
                        }
                        else if (count == 2)
                        {
                            if (line.Contains(@"\endverbatim"))
                            {
                                state = ReadState.None;
                            }
                            else
                            {
                                purpose.Add(line);
                            }
                        }

                        break;

                    case ReadState.Arguments:
                        if (count == 0 && line.Contains(@"\param["))
                        {
                            paramName = regParamName.Match(line).Groups[1].Value;
                            if (paramName == "")
                            {
                                throw new Exception("Failed to read the param name!");
                            }
                            paramType = regParamType.Match(line).Groups[1].Value;
                            if (paramType == "")
                            {
                                throw new Exception("Failed to read the param type!");
                            }
                            count = 1;
                        }
                        else if (count == 1 && line.Contains(@"\verbatim"))
                        {
                            count = 2;
                        }
                        else if (count == 2)
                        {
                            if (line.Contains(@"\endverbatim"))
                            {
                                ParamType type;
                                switch (paramType!)
                                {
                                    case "in": type = ParamType.In; break;
                                    case "out": type = ParamType.Out; break;
                                    case "in,out": type = ParamType.InOut; break;
                                    default: throw new Exception("Invalid param type!");
                                }
                                var param = new Param(paramName!, type, paramDescription.ToArray());
                                paramList.Add(param);
                                count = 0;
                                paramName = null;
                                paramType = null;
                                paramDescription = new List<string>();
                            }
                            else
                            {
                                paramDescription.Add(line.Trim());
                            }
                        }

                        break;

                    case ReadState.FurtherDetails:
                        if (count == 0 && line.Contains(@"\verbatim"))
                        {
                            count = 1;
                        }
                        else if (count == 1 && line.Trim() == "")
                        {
                            count = 2;
                        }
                        else if (count == 1 && line.Contains(@"\endverbatim"))
                        {
                            state = ReadState.None;
                        }
                        else if (count == 2)
                        {
                            if (line.Contains(@"\endverbatim"))
                            {
                                state = ReadState.None;
                            }
                            else
                            {
                                remarks.Add(line);
                            }
                        }

                        break;
                }
            }

            this.purpose = ToArray(purpose);
            this.paramList = paramList.ToArray();
            this.remarks = ToArray(remarks);
        }

        private FunctionDescription()
        {
            this.purpose = Array.Empty<string>();
            this.paramList = Array.Empty<Param>();
            this.remarks = Array.Empty<string>();
        }

        public Param? GetParam(string name)
        {
            name = name.ToLower();

            foreach (var param in paramList)
            {
                if (param.Name.ToLower() == name)
                {
                    return param;
                }
            }

            return null;
        }

        private string[] ToArray(List<string> lines)
        {
            if (lines.Count == 0)
            {
                return Array.Empty<string>();
            }

            var result = new List<string>();
            var prev = "";
            foreach (var line in lines)
            {
                var trimmed = line.Trim();

                if (prev == "" && trimmed == "")
                {
                    continue;
                }

                if (trimmed == "")
                {
                    result.Add("");
                }
                else
                {
                    result.Add(line);
                }

                prev = trimmed;
            }

            while (result.Last() == "")
            {
                result.RemoveAt(result.Count - 1);
            }

            return result.ToArray();
        }

        public IReadOnlyList<string> Purpose => purpose;
        public IReadOnlyList<Param> ParamList => paramList;
        public IReadOnlyList<string> Remarks => remarks;

        public static FunctionDescription Empty => empty;



        private enum ReadState
        {
            None,
            Purpose,
            Arguments,
            FurtherDetails,
        }

        public enum ParamType
        {
            In,
            Out,
            InOut,
        }

        public class Param
        {
            private string name;
            private ParamType type;
            private IReadOnlyList<string> description;

            public Param(string name, ParamType type, IReadOnlyList<string> description)
            {
                this.name = name;
                this.type = type;
                this.description = description;
            }

            public string Name => name;
            public ParamType Type => type;
            public IReadOnlyList<string> Description => description;
        }
    }
}
