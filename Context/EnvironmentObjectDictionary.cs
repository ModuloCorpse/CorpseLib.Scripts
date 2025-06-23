namespace CorpseLib.Scripts.Context
{
    public class EnvironmentObjectDictionary
    {
        private readonly Dictionary<int, EnvironmentObject> m_Objects = [];
        private readonly List<InstructionObject> m_InstructionObjects = [];

        public bool IsEmpty => m_Objects.Count == 0;
        public EnvironmentObject[] Objects => [..m_Objects.Values];
        public InstructionObject[] InstructionObjects => [.. m_InstructionObjects];

        public bool Add(EnvironmentObject objToAdd, Signature signature, int namespaceIdx)
        {
            if (namespaceIdx == signature.Namespaces.Length)
                return m_Objects.TryAdd(signature.ID, objToAdd);
            else if (m_Objects.TryGetValue(signature.Namespaces[namespaceIdx], out var obj) && obj is NamespaceObject namespaceObject)
                return namespaceObject.Add(objToAdd, signature, namespaceIdx + 1);
            return false;
        }

        public bool Add(InstructionObject instructionToAdd, int[] namespaces, int namespaceIdx)
        {
            if (namespaceIdx == namespaces.Length)
            {
                m_InstructionObjects.Add(instructionToAdd);
                return true;
            }
            else if (m_Objects.TryGetValue(namespaces[namespaceIdx], out var obj) && obj is NamespaceObject namespaceObject)
                return namespaceObject.Add(instructionToAdd, namespaces, namespaceIdx + 1);
            return false;
        }

        public EnvironmentObject? Get(Signature signature, int namespaceIdx)
        {
            if (namespaceIdx == signature.Namespaces.Length)
            {
                if (m_Objects.TryGetValue(signature.ID, out var obj))
                    return obj;
                return null;
            }
            else if (m_Objects.TryGetValue(signature.Namespaces[namespaceIdx], out var obj) && obj is NamespaceObject namespaceObject)
                return namespaceObject.Get(signature, namespaceIdx + 1);
            return null;
        }

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
