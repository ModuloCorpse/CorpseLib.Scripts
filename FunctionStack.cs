using CorpseLib.Scripts.Context;

namespace CorpseLib.Scripts
{
    public class FunctionStack
    {
        private VariableScope m_VariableScope = new();
        private object? m_ReturnValue = null;
        private bool m_HasReturn = false;

        public object? ReturnValue => m_ReturnValue;
        public bool HasReturn => m_HasReturn;

        public void OpenScope() => m_VariableScope = new(m_VariableScope);
        public void CloseScope()
        {
            if (m_VariableScope.Parent != null)
                m_VariableScope = m_VariableScope.Parent;
        }

        public void AddVariable(int id, Variable value) => m_VariableScope.AddVariable(id, value);

        public Variable? GetVariable(int[] ids) => m_VariableScope.GetVariable(ids);

        public void Return() => m_HasReturn = true;

        public void Return(object? value)
        {
            m_HasReturn = true;
            m_ReturnValue = value;
        }
    }
}
