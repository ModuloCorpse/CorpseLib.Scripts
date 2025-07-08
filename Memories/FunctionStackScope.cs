namespace CorpseLib.Scripts.Memories
{
    public class FunctionStackScope
    {
        private readonly FunctionStackScope? m_Parent = null;
        private readonly Dictionary<int, StackVariable> m_Variables = [];

        internal FunctionStackScope? Parent => m_Parent;

        public StackVariable[] AllVariables => (m_Parent == null) ? [..m_Variables.Values] : [..m_Parent.AllVariables, ..m_Variables.Values];
        public StackVariable[] Variables => [..m_Variables.Values];

        public FunctionStackScope() { }
        public FunctionStackScope(FunctionStackScope parent) => m_Parent = parent;

        public void AddVariable(int id, StackVariable value) => m_Variables[id] = value;

        public StackVariable? GetVariable(int id)
        {
            if (m_Variables.TryGetValue(id, out var value))
                return value;
            return m_Parent?.GetVariable(id);
        }
    }
}
