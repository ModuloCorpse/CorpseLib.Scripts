namespace CorpseLib.Scripts.Parser
{
    internal class ParserBase
    {
        private readonly List<string> m_Warnings = [];
        private string m_Error = string.Empty;
        private bool m_HasError = false;

        public string[] Warnings => [..m_Warnings];
        public string Error => m_Error;
        public bool HasError => m_HasError;

        protected void RegisterWarning(string warning, string description) => m_Warnings.Add(string.Format("WARNING : {0} : {1}", warning, description));

        protected void RegisterError(string error, string description)
        {
            m_Error = string.Format("ERROR : {0} : {1}", error, description);
            m_HasError = true;
        }

        protected void RegisterOtherBase(ParserBase other)
        {
            m_Warnings.AddRange(other.Warnings);
            if (!m_HasError)
            {
                m_Error = other.m_Error;
                m_HasError = other.m_HasError;
            }
        }

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
    }
}
