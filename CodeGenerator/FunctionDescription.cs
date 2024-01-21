using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace CodeGenerator
{
    public class FunctionDescription
    {
        private static readonly Regex regParamName = new Regex(@"param\[.+\]\s+([A-Z1-9_]+)\s+");
        private static readonly Regex regParamType = new Regex(@"param\[(.+)\]\s+[A-Z1-9_]+\s+");

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

            foreach (var line in File.ReadLines(path))
            {
                if (line == @"/* > \par Purpose: */")
                {
                    state = ReadState.Purpose;
                    count = 0;
                }
                else if (line == @"/*  Arguments: */")
                {
                    state = ReadState.Arguments;
                    count = 0;
                }
                else if (line == @"/* > \par Further Details: */")
                {
                    state = ReadState.FurtherDetails;
                    count = 0;
                }

                switch (state)
                {
                    case ReadState.Purpose:
                        if (count == 0 && line == @"/* > \verbatim */")
                        {
                            count = 1;
                        }
                        else if (count == 1 && line == @"/* > */")
                        {
                            count = 2;
                        }
                        else if (count == 2)
                        {
                            if (line == @"/* > \endverbatim */")
                            {
                                state = ReadState.None;
                            }
                            else
                            {
                                purpose.Add(Trim(line));
                            }
                        }

                        break;

                    case ReadState.Arguments:
                        if (count == 0 && line.StartsWith(@"/* > \param["))
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
                        else if (count == 1 && line == @"/* > \verbatim */")
                        {
                            count = 2;
                        }
                        else if (count == 2)
                        {
                            if (line == @"/* > \endverbatim */")
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
                                paramDescription.Add(TrimParam(line));
                            }
                        }

                        break;

                    case ReadState.FurtherDetails:
                        if (count == 0 && line == @"/* > \verbatim */")
                        {
                            count = 1;
                        }
                        else if (count == 1 && line == @"/* > */")
                        {
                            count = 2;
                        }
                        else if (count == 1 && line == @"/* > \endverbatim */")
                        {
                            state = ReadState.None;
                        }
                        else if (count == 2)
                        {
                            if (line == @"/* > \endverbatim */")
                            {
                                state = ReadState.None;
                            }
                            else
                            {
                                remarks.Add(Trim(line));
                            }
                        }

                        break;
                }
            }

            if (purpose.Count == 0)
            {
                throw new Exception("Failed to get the purpose!");
            }

            this.purpose = purpose.ToArray();
            this.paramList = paramList.ToArray();
            this.remarks = remarks.ToArray();
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

        private static string Trim(string line)
        {
            if (line == "")
            {
                return "";
            }

            if (line == "/* > */")
            {
                return "";
            }

            if (line == " */")
            {
                return "";
            }

            if (line.StartsWith("/* > ") && line.EndsWith(" */"))
            {
                return line.Substring(5, line.Length - 8);
            }

            if (line.StartsWith("/* ") && line.EndsWith(" */"))
            {
                return line.Substring(3, line.Length - 6);
            }

            if (line.StartsWith("/* > "))
            {
                return line.Substring(5, line.Length - 5);
            }

            throw new Exception("Failed to trim the line!");
        }

        private static string TrimParam(string line)
        {
            if (line == "")
            {
                return "";
            }

            if (line == "/* > */")
            {
                return "";
            }

            if (line.StartsWith("/* > ") && line.EndsWith(" */"))
            {
                return line.Substring(5, line.Length - 8).Trim();
            }

            throw new Exception("Failed to trim the param line!");
        }

        public IReadOnlyList<string> Purpose => purpose;
        public IReadOnlyList<Param> ParamList => paramList;
        public IReadOnlyList<string> Remarks => remarks;



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
