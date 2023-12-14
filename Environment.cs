namespace CorpseLib.Scripts
{
    public class Environment
    {
        private readonly Environment? m_Parent = null;
        private readonly Dictionary<string, object?> m_Variables = [];
        private bool m_HasReturn = false;

        public Environment() { }
        public Environment(Environment parent) => m_Parent = parent;

        public void Return()
        {
            //TODO Handle value
            m_HasReturn = true;
        }

        public void SetVariable(string name, object? value) => m_Variables[name] = value;

        public object? GetVariable(string name)
        {
            Dictionary<string, object?> variables = m_Variables;
            string[] param = name.Split('.');
            for (int i = 0; i != param.Length; ++i)
            {
                if (variables.TryGetValue(param[i], out var value))
                {
                    if ((i + 1) != param.Length)
                    {
                        if (value is Dictionary<string, object?> dict)
                            variables = dict;
                        else
                            return null;
                    }
                    else
                        return value;
                }
            }
            return m_Parent?.GetVariable(name);
        }

        public void OpenScope() { }
        public void CloseScope() { }
    }
}
