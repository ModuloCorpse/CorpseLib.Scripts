
namespace CorpseLib.Scripts.Memories
{
    public class FunctionStack
    {
        private FunctionStackScope m_VariableScope = new();
        private AMemoryValue? m_ReturnValue = null;
        private bool m_HasReturn = false;

        public AMemoryValue? ReturnValue => m_ReturnValue;
        public bool HasReturn => m_HasReturn;

        public StackVariable[] AllVariables => m_VariableScope.AllVariables;
        public StackVariable[] Variables => m_VariableScope.Variables;

        public void OpenScope() => m_VariableScope = new(m_VariableScope);
        public List<StackVariable> CloseScope()
        {
            List<StackVariable> variablesToFree = [];
            if (m_VariableScope.Parent != null)
            {
                variablesToFree.AddRange(m_VariableScope.Variables);
                m_VariableScope = m_VariableScope.Parent;
            }
            return variablesToFree;
        }

        public void AddVariable(int id, StackVariable value) => m_VariableScope.AddVariable(id, value);

        public StackVariable? GetVariable(int id) => m_VariableScope.GetVariable(id);

        public void Return() => m_HasReturn = true;

        public void Return(AMemoryValue value)
        {
            m_HasReturn = true;
            m_ReturnValue = value;
        }
    }
}
