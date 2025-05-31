namespace CorpseLib.Scripts.Context
{
    public class Environment
    {
        private readonly Dictionary<int, EnvironmentObject> m_Objects = [];
        private VariableScope m_VariableScope = new();

        public void OpenScope() => m_VariableScope = new(m_VariableScope);
        public void CloseScope()
        {
            if (m_VariableScope.Parent != null)
                m_VariableScope = m_VariableScope.Parent;
        }

        public void AddVariable(int id, Variable value) => m_VariableScope.AddVariable(id, value);

        public Variable? GetVariable(int[] ids) => m_VariableScope.GetVariable(ids);

        public void ClearInvalid()
        {
            List<int> invalidKeys = [];
            foreach (var obj in m_Objects.Values)
            {
                if (obj is NamespaceObject childNamespaceObject)
                {
                    childNamespaceObject.ClearInvalid();
                    if (!childNamespaceObject.IsValid())
                        invalidKeys.Add(childNamespaceObject.ID);
                }
                else if (!obj.IsValid())
                    invalidKeys.Add(obj.ID);
            }
            foreach (var key in invalidKeys)
                m_Objects.Remove(key);
        }
    }
}
