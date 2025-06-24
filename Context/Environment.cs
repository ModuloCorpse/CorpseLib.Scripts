using CorpseLib.Scripts.Type.Primitive;
using CorpseLib.Scripts.Type;
using CorpseLib.Scripts.Instructions;

namespace CorpseLib.Scripts.Context
{
    public class Environment
    {
        private readonly EnvironmentObjectDictionary m_Objects = new();
        private VariableScope m_VariableScope = new();

        internal EnvironmentObjectDictionary Objects => m_Objects;

        public void OpenScope() => m_VariableScope = new(m_VariableScope);
        public void CloseScope()
        {
            if (m_VariableScope.Parent != null)
                m_VariableScope = m_VariableScope.Parent;
        }

        public void AddInstruction(AInstruction instruction, int[] namespaces, int[] tags, int[] comments)
        {
            InstructionObject instructionObject = new(instruction, tags, comments);
            m_Objects.Add(instructionObject, namespaces, 0);
        }

        public void AddVariable(int id, Variable value) => m_VariableScope.AddVariable(id, value);

        public Variable? GetVariable(int[] ids) => m_VariableScope.GetVariable(ids);

        public void ClearInvalid() => m_Objects.ClearInvalid();

        public bool AddNamespace(int[] namespaces, int namespaceID, int[] tags, int[] comments) => m_Objects.Add(new NamespaceObject(namespaceID, tags, comments), new(namespaces, namespaceID), 0);

        public bool AddFunction(int[] namespaces, AFunction function, int[] tags, int[] comments)
        {
            return m_Objects.Add(new FunctionObject(function, tags, comments), new(namespaces, function.Signature.ID), 0);
        }

        private TypeObject GetOrCreateTypeObject(Signature signature)
        {
            if (m_Objects.Get(signature, 0) is TypeObject existingTypeObject)
            {
                return existingTypeObject;
            }
            else
            {
                TypeObject typeObject = new(signature.ID, [], []);
                m_Objects.Add(typeObject, signature, 0);
                return typeObject;
            }
        }

        public void AddTypeDefinition(TypeDefinition type, int[] tags, int[] comments) => GetOrCreateTypeObject(type.Signature).AddTypeDefinition(type, tags, comments);
        public void AddType(ATypeInstance type)
        {
            if (m_Objects.Get(type.TypeInfo.Signature, 0) is TypeObject existingTypeObject)
                existingTypeObject.AddTypeInstance(type);
        }

        private EnvironmentObject? Get(Signature signature) => m_Objects.Get(signature, 0);

        public Signature? Search<T>(int[] namespaceContext, Signature searchSignature, List<Signature> searchedSignatures) where T : EnvironmentObject
        {
            List<int> namespaces = [..namespaceContext, ..searchSignature.Namespaces];
            EnvironmentObject? ret = null;
            while (ret == null)
            {
                Signature signature = new([..namespaces], searchSignature.ID);
                searchedSignatures.Add(signature);
                ret = Get(signature);
                if (ret != null && ret is T)
                    return signature;
                if (namespaces.Count == searchSignature.Namespaces.Length)
                    return null;
                namespaces.RemoveAt(namespaces.Count - 1 - searchSignature.Namespaces.Length);
            }
            return null;
        }

        public ATypeInstance? Instantiate(TypeInfo typeInfo)
        {
            if (typeInfo.NamespacesID.Length == 0 && Types.TryGet(typeInfo.ID, out ATypeInstance? primitiveInstance))
            {
                if (typeInfo.IsArray)
                    return new ArrayType(primitiveInstance!);
                return primitiveInstance!;
            }
            if (Get(typeInfo.Signature) is not TypeObject typeObject)
                return null;
            ATypeInstance? ret = typeObject.Instantiate(typeInfo, this);
            if (ret != null && typeInfo.IsArray)
                return new ArrayType(ret!);
            return ret;
        }
    }
}
