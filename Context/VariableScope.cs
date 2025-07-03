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

        public Variable? GetVariable(int id)
        {
            if (m_Variables.TryGetValue(id, out var value))
                return value;
            return m_Parent?.GetVariable(id);
        }
    }
}
