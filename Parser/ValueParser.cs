﻿using CorpseLib.Scripts.Memories;
using System.Text;

namespace CorpseLib.Scripts.Parser
{
    public static class ValueParser
    {
        internal static Tuple<string, string> NextElement(string str, char expectedOpen, char expectedClose)
        {
            int nbOpen = 0;
            StringBuilder builder = new();
            for (int i = 0; i != str.Length; ++i)
            {
                char c = str[i];
                if (c == expectedClose)
                {
                    --nbOpen;
                    builder.Append(c);
                    if (nbOpen == 0)
                    {
                        while ((i + 1) < str.Length && str[i + 1] != ',')
                        {
                            ++i;
                            if (!char.IsWhiteSpace(str[i]))
                                throw new ArgumentException("Invalid element");
                        }
                    }
                }
                else if (c == expectedOpen)
                {
                    ++nbOpen;
                    builder.Append(c);
                }
                else if (c == ',' && nbOpen == 0)
                {
                    ++i;
                    while (i < str.Length && char.IsWhiteSpace(str[i]))
                        ++i;
                    string ret = str[i..];
                    string element = builder.ToString();
                    if (ret.Length > 0 && ret[0] == ' ')
                        ret = ret[1..];
                    return new(element, ret);
                }
                else
                    builder.Append(c);
            }
            return new(str, string.Empty);
        }

        private static Tuple<string, string> NextString(string str)
        {
            bool inString = false;
            StringBuilder builder = new();
            for (int i = 0; i != str.Length; ++i)
            {
                char c = str[i];
                if (inString && c == '"')
                {
                    inString = false;
                    builder.Append(c);
                    while ((i + 1) < str.Length && str[i + 1] != ',')
                    {
                        ++i;
                        if (!char.IsWhiteSpace(str[i]))
                            throw new ArgumentException("Invalid element");
                    }
                }
                else if (c == '"')
                {
                    inString = true;
                    builder.Append(c);
                }
                else if (c == ',' && !inString)
                {
                    ++i;
                    while (i < str.Length && char.IsWhiteSpace(str[i]))
                        ++i;
                    string ret = str[i..];
                    string element = builder.ToString();
                    if (ret.Length > 0 && ret[0] == ' ')
                        ret = ret[1..];
                    return new(element, ret);
                }
                else
                    builder.Append(c);
            }
            return new(str, string.Empty);
        }

        private static Tuple<string, string> IsolateFirstElem(string str)
        {
            for (int i = 0; i < str.Length; ++i)
            {
                if (str[i] == ',')
                {
                    string elem = str[..i];
                    string ret = str[(i + 1)..];
                    if (ret.Length > 0 && ret[0] == ' ')
                        ret = ret[1..];
                    return new(elem, ret);
                }
            }
            return new(str, string.Empty);
        }

        public static IMemoryValue ParseValue(string str, ParsingContext parsingContext)
        {
            if (str == "null")
                return Heap.NULL;
            if (str.Length > 2 && str[0] == '{' && str[^1] == '}')
            {
                str = str[1..^1];
                if (str.Length > 0 && str[^1] == ' ')
                    str = str[..^1];
                if (str.Length > 0 && str[0] == ' ')
                    str = str[1..];
                List<IMemoryValue> variables = [];
                while (!string.IsNullOrEmpty(str))
                {
                    if (str[0] == '[')
                    {
                        Tuple<string, string> split = NextElement(str, '[', ']');
                        variables.Add(ParseValue(split.Item1, parsingContext));
                        str = split.Item2;
                    }
                    else if (str[0] == '{')
                    {
                        Tuple<string, string> split = NextElement(str, '{', '}');
                        variables.Add(ParseValue(split.Item1, parsingContext));
                        str = split.Item2;
                    }
                    else if (str[0] == '"')
                    {
                        Tuple<string, string> split = NextString(str);
                        variables.Add(ParseValue(split.Item1, parsingContext));
                        str = split.Item2;
                    }
                    else
                    {
                        Tuple<string, string> split = IsolateFirstElem(str);
                        variables.Add(ParseValue(split.Item1, parsingContext));
                        str = split.Item2;
                    }
                }
                return new AnonymousObjectValue([.. variables]);
            }
            else if (str.Length > 2 && str[0] == '[' && str[^1] == ']')
            {
                str = str[1..^1];
                if (str.Length > 0 && str[^1] == ' ')
                    str = str[..^1];
                if (str.Length > 0 && str[0] == ' ')
                    str = str[1..];
                List<IMemoryValue> variables = [];
                while (!string.IsNullOrEmpty(str))
                {
                    if (str[0] == '[')
                    {
                        Tuple<string, string> split = NextElement(str, '[', ']');
                        variables.Add(ParseValue(split.Item1, parsingContext));
                        str = split.Item2;
                    }
                    else if (str[0] == '{')
                    {
                        Tuple<string, string> split = NextElement(str, '{', '}');
                        variables.Add(ParseValue(split.Item1, parsingContext));
                        str = split.Item2;
                    }
                    else if (str[0] == '"')
                    {
                        Tuple<string, string> split = NextString(str);
                        variables.Add(ParseValue(split.Item1, parsingContext));
                        str = split.Item2;
                    }
                    else
                    {
                        Tuple<string, string> split = IsolateFirstElem(str);
                        variables.Add(ParseValue(split.Item1, parsingContext));
                        str = split.Item2;
                    }
                }
                return new ArrayValue([.. variables]);
            }
            else if (str.Length > 2 && str[0] == '"' && str[^1] == '"')
                return new StringValue(str[1..^1]);
            else if (str.Length > 2 && str[0] == '\'' && str[^1] == '\'')
            {
                if (str.Length == 3)
                    return new LiteralValue(str[1]);
                else if (str.Length == 4 && str[1] == '\\')
                    return new LiteralValue(str[2]);
                else
                {
                    parsingContext.RegisterWarning("Wrong delimiter for string", $"String delimited with char delimiter : {str}");
                    return new StringValue(str[1..^1]);
                }
            }
            else
            {
                if (str == "true")
                    return new LiteralValue(true);
                else if (str == "false")
                    return new LiteralValue(false);
                else if (str.Contains('.'))
                {
                    if (str.Length > 2 && str[0] == '-' && str[1] == '.')
                        str = $"-0,{str[2..]}";
                    else if (str.Length > 1 && str[0] == '.')
                        str = $"0,{str[1..]}";
                    else
                        str = str.Replace('.', ',');
                    if (double.TryParse(str, out double value))
                    {
                        if (value >= float.MinValue && value <= float.MaxValue)
                            return new LiteralValue((float)value);
                        else
                            return new LiteralValue(value);
                    }
                    parsingContext.RegisterError("Invalid script", $"Cannot parse float value : {str}");
                    return Heap.NULL;
                }
                else
                {
                    if (str[0] == '-')
                    {
                        if (long.TryParse(str, out long value))
                        {
                            if (value >= sbyte.MinValue && value <= sbyte.MaxValue)
                                return new LiteralValue((sbyte)value);
                            else if (value >= short.MinValue && value <= short.MaxValue)
                                return new LiteralValue((short)value);
                            else if (value >= int.MinValue && value <= int.MaxValue)
                                return new LiteralValue((int)value);
                            else
                                return new LiteralValue(value);
                        }
                    }
                    else
                    {
                        if (ulong.TryParse(str, out ulong value))
                        {
                            if (value >= byte.MinValue && value <= byte.MaxValue)
                                return new LiteralValue((byte)value);
                            else if (value >= ushort.MinValue && value <= ushort.MaxValue)
                                return new LiteralValue((ushort)value);
                            else if (value >= uint.MinValue && value <= uint.MaxValue)
                                return new LiteralValue((uint)value);
                            else
                                return new LiteralValue(value);
                        }
                    }
                }
            }
            return new StringValue(str); //We consider it a string not delimited as some split can remove " from strings
        }
    }
}
