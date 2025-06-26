namespace CorpseLib.Scripts.Context
{
    public class NamespaceObject(int id, int[] tags, int[] comments) : EnvironmentObject(id, tags, comments)
    {
        private readonly EnvironmentObjectDictionary m_Objects = new();

        public EnvironmentObjectDictionary Objects => m_Objects;

        public override bool IsValid() => !m_Objects.IsEmpty;
        public void ClearInvalid() => m_Objects.ClearInvalid();
        public bool Add(EnvironmentObject obj, Signature signature, int namespaceIdx) => m_Objects.Add(obj, signature, namespaceIdx);
        public bool Add(InstructionObject instruction, int[] namespaces, int namespaceIdx) => m_Objects.Add(instruction, namespaces, namespaceIdx);
        public EnvironmentObject? Get(Signature signature, int namespaceIdx) => m_Objects.Get(signature, namespaceIdx);
    }
}
