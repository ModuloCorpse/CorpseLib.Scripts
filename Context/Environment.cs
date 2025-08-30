using CorpseLib.Scripts.Type;
using CorpseLib.Scripts.Instructions;
using CorpseLib.Scripts.Parameters;

namespace CorpseLib.Scripts.Context
{
    public class Environment
    {
        private readonly EnvironmentObjectDictionary m_Objects = new();
        private readonly List<ATypeInstance> m_TypeTable = [..Types.PRIMITIVE_TYPES];

        public EnvironmentObjectDictionary Objects => m_Objects;

        public void AddInstruction(AInstruction instruction, int[] namespaces, int[] tags, int[] comments)
        {
            InstructionObject instructionObject = new(instruction, tags, comments);
            m_Objects.Add(instructionObject, namespaces, 0);
        }

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
        public int AddType(ATypeInstance type)
        {
            TypeInfo typeInfo = type.TypeInfo;
            //TODO Add it's Type Index instead
            if (m_Objects.Get(typeInfo.Signature, 0) is TypeObject existingTypeObject)
            {
                int typeIndex = m_TypeTable.Count;
                m_TypeTable.Add(type);
                existingTypeObject.AddTypeInstance(typeInfo, typeIndex);
                return typeIndex;
            }
            return -1;
        }

        public ATypeInstance? GetTypeInstance(int typeIndex)
        {
            if (typeIndex < 0 || typeIndex >= m_TypeTable.Count)
                return null;
            return m_TypeTable[typeIndex];
        }

        public void RemoveType(ATypeInstance type)
        {
            if (m_Objects.Get(type.TypeInfo.Signature, 0) is TypeObject existingTypeObject)
                existingTypeObject.RemoveType(type.TypeInfo);
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

        private int GetTypeInfoIndex(TypeInfo typeInfo)
        {
            if (typeInfo.NamespacesID.Length == 0 && Types.TryGet(typeInfo.ID, out int primitiveInstance))
                return primitiveInstance;
            if (Get(typeInfo.Signature) is not TypeObject typeObject)
                return -1;
            return typeObject.Instantiate(typeInfo, this);
        }

        public ParameterType? Instantiate(TypeInfo typeInfo)
        {
            int instanceIdx = GetTypeInfoIndex(typeInfo);
            if (instanceIdx == -1)
                return null;
            return new(instanceIdx, typeInfo.IsStatic, typeInfo.IsConst, typeInfo.IsRef, typeInfo.ArrayCount);
        }

        public ParameterType? Instantiate(TypeInfo typeInfo, int[] namespaces)
        {
            if (typeInfo.NamespacesID.Length == 0 && Types.TryGet(typeInfo.ID, out int primitiveInstance))
                return new(primitiveInstance, typeInfo.IsStatic, typeInfo.IsConst, typeInfo.IsRef, typeInfo.ArrayCount);
            List<Signature> searchedSignatures = [];
            Signature? signature = Search<TypeObject>(namespaces, typeInfo.Signature, searchedSignatures);
            if (signature != null)
            {
                TypeInfo resolvedTypeInfo = new(typeInfo.IsStatic, typeInfo.IsConst, typeInfo.IsRef, signature.Namespaces, signature.ID, typeInfo.TemplateTypes, typeInfo.ArrayCount);
                return Instantiate(resolvedTypeInfo);
            }
            return null;
        }
    }
}
