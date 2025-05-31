namespace CorpseLib.Scripts.Context
{
    public class VariableScope
    {
        private readonly VariableScope? m_Parent = null;
        private readonly Dictionary<int, Variable> m_Variables = [];

        internal VariableScope? Parent => m_Parent;

        public VariableScope() { }
        public VariableScope(VariableScope parent) => m_Parent = parent;

        public void AddVariable(int id, Variable value) => m_Variables[id] = value;

        public Variable? GetVariable(int[] ids)
        {
            if (ids.Length == 0)
                return null;
            if (m_Variables.TryGetValue(ids[0], out var value))
            {
                for (int i = 1; i != ids.Length; ++i)
                {
                    Variable? subValue = value.GetSubValue(ids[i]);
                    if (subValue != null)
                        value = subValue;
                    else
                        return null;
                }
                return value;
            }
            return m_Parent?.GetVariable(ids);
        }
    }
}
