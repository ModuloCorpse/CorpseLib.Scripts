namespace CorpseLib.Scripts.Parser
{
    internal static class ParserHelper
    {
        internal static Tuple<string, string, string> IsolateScope(string str, char open, char close, out bool found)
        {
            int openScopeIdx = str.IndexOf(open);
            if (openScopeIdx != -1)
            {
                int closeScopeIdx = openScopeIdx;
                int scopeCount = 0;
                while (closeScopeIdx < str.Length)
                {
                    if (str[closeScopeIdx] == open)
                        ++scopeCount;
                    else if (str[closeScopeIdx] == close)
                    {
                        --scopeCount;
                        if (scopeCount == 0)
                        {
                            string beforeScope = str[..openScopeIdx];
                            if (beforeScope.Length > 0 && beforeScope[^1] == ' ')
                                beforeScope = beforeScope[..^1];

                            string inScope = str[(openScopeIdx + 1)..closeScopeIdx];
                            if (inScope.Length > 0 && inScope[^1] == ' ')
                                inScope = inScope[..^1];
                            if (inScope.Length > 0 && inScope[0] == ' ')
                                inScope = inScope[1..];

                            string afterScope = ((closeScopeIdx + 1) < str.Length) ? str[(closeScopeIdx + 1)..] : string.Empty;
                            if (afterScope.Length > 0 && afterScope[0] == ' ')
                                afterScope = afterScope[1..];
                            found = true;
                            return new(beforeScope, inScope, afterScope);
                        }
                    }
                    ++closeScopeIdx;
                }
            }
            found = false;
            return new(str, string.Empty, string.Empty);
        }

        internal static Tuple<string, string> NextInstruction(string str, out bool found)
        {
            bool inString = false;
            char stringChar = '\0';
            for (int i = 0; i != str.Length; ++i)
            {
                char c = str[i];
                if (inString)
                {
                    if (c == stringChar)
                        inString = false;
                }
                else if (c == '"' || c == '\'')
                {
                    inString = true;
                    stringChar = c;
                }
                else if (c == ';')
                {
                    found = true;
                    string instruction = str[..i];
                    if (instruction.Length > 0 && instruction[^1] == ' ')
                        instruction = instruction[..^1];
                    string ret = string.Empty;
                    if ((i + 1) != str.Length)
                        ret = str[(i + 1)..];
                    if (ret.Length > 0 && ret[0] == ' ')
                        ret = ret[1..];
                    return new(instruction, ret);
                }
            }
            found = false;
            return new(string.Empty, string.Empty);
        }
    }
}
