using CorpseLib.Scripts.Type.Primitive;
using CorpseLib.Scripts.Type;
using System.Diagnostics.CodeAnalysis;
using CorpseLib.Scripts.Context;
using CorpseLib.Scripts.Instruction;
using Environment = CorpseLib.Scripts.Context.Environment;

namespace CorpseLib.Scripts
{
    public class OldEnvironment
    {
        private class TemplateInstanceHolder
        {
            private class TemplateInstanceHolderTreeNode
            {
                private readonly Dictionary<int, TemplateInstanceHolderTreeNode> m_TemplateInstances = [];
                private ATypeInstance? m_TypeInstance = null;

                public ATypeInstance? SearchTypeInstance(TypeInfo[] typeInfos, int idx)
                {
                    if (idx >= typeInfos.Length)
                        return m_TypeInstance;
                    else if (m_TemplateInstances.TryGetValue(typeInfos[idx].ID, out TemplateInstanceHolderTreeNode? node))
                        return node.SearchTypeInstance(typeInfos, idx + 1);
                    return null;
                }

                public void InsertTypeInstance(TypeInfo[] typeInfos, int idx, ATypeInstance value)
                {
                    if (idx >= typeInfos.Length)
                    {
                        m_TypeInstance = value;
                        return;
                    }
                    else if (m_TemplateInstances.TryGetValue(typeInfos[idx].ID, out TemplateInstanceHolderTreeNode? node))
                        node.InsertTypeInstance(typeInfos, idx + 1, value);
                    else
                    {
                        TemplateInstanceHolderTreeNode newNode = new();
                        m_TemplateInstances[typeInfos[idx].ID] = newNode;
                        newNode.InsertTypeInstance(typeInfos, idx + 1, value);
                    }
                }

                public void RemoveTypeInstance(TypeInfo[] typeInfos, int idx)
                {
                    if (idx >= typeInfos.Length)
                    {
                        m_TypeInstance = null;
                        return;
                    }
                    else if (m_TemplateInstances.TryGetValue(typeInfos[idx].ID, out TemplateInstanceHolderTreeNode? node))
                    {
                        node.RemoveTypeInstance(typeInfos, idx + 1);
                        if (node.m_TypeInstance == null && node.m_TemplateInstances.Count == 0)
                            m_TemplateInstances.Remove(typeInfos[idx].ID);
                    }
                }

                public void AppendToList(List<ATypeInstance> list)
                {
                    if (m_TypeInstance != null)
                        list.Add(m_TypeInstance);
                    foreach (TemplateInstanceHolderTreeNode node in m_TemplateInstances.Values)
                        node.AppendToList(list);
                }
            }

            private readonly Dictionary<int, TemplateInstanceHolderTreeNode> m_TemplateInstances = [];

            public bool TryGetValue(TypeInfo typeInfo, [MaybeNullWhen(false)] out ATypeInstance value)
            {
                if (m_TemplateInstances.TryGetValue(typeInfo.ID, out TemplateInstanceHolderTreeNode? node))
                {
                    ATypeInstance? ret = node.SearchTypeInstance(typeInfo.TemplateTypes, 0);
                    if (ret != null)
                    {
                        value = ret;
                        return true;
                    }
                }
                value = null;
                return false;
            }

            public void Add(TypeInfo typeInfo, ATypeInstance value)
            {
                if (m_TemplateInstances.TryGetValue(typeInfo.ID, out TemplateInstanceHolderTreeNode? node))
                    node.InsertTypeInstance(typeInfo.TemplateTypes, 0, value);
                else
                {
                    TemplateInstanceHolderTreeNode newNode = new();
                    m_TemplateInstances[typeInfo.ID] = newNode;
                    newNode.InsertTypeInstance(typeInfo.TemplateTypes, 0, value);
                }
            }

            public void Remove(TypeInfo typeInfo)
            {
                if (m_TemplateInstances.TryGetValue(typeInfo.ID, out TemplateInstanceHolderTreeNode? node))
                    node.RemoveTypeInstance(typeInfo.TemplateTypes, 0);
            }

            public ATypeInstance[] Values
            {
                get
                {
                    List<ATypeInstance> ret = [];
                    foreach (TemplateInstanceHolderTreeNode node in m_TemplateInstances.Values)
                        node.AppendToList(ret);
                    return [.. ret];
                }
            }
        }

        private class TemplateDefinitionHolder
        {
            private class TemplateDefinitionHolderTreeNode
            {
                private readonly Dictionary<int, TypeDefinition> m_TemplateDefinitions = [];

                public bool TryGetValue(TypeInfo typeInfo, [MaybeNullWhen(false)] out TypeDefinition value) => m_TemplateDefinitions.TryGetValue(typeInfo.TemplateTypes.Length, out value);
                public void Add(TypeDefinition type) => m_TemplateDefinitions[type.Templates.Length] = type;
                public TypeDefinition[] Values => [.. m_TemplateDefinitions.Values];
            }

            private readonly Dictionary<int, TemplateDefinitionHolderTreeNode> m_TemplateDefinitions = [];
            public bool TryGetValue(TypeInfo typeInfo, [MaybeNullWhen(false)] out TypeDefinition value)
            {
                if (m_TemplateDefinitions.TryGetValue(typeInfo.ID, out TemplateDefinitionHolderTreeNode? node))
                    return node.TryGetValue(typeInfo, out value);
                value = null;
                return false;
            }

            public void Add(TypeDefinition type)
            {
                if (m_TemplateDefinitions.TryGetValue(type.Signature.ID, out TemplateDefinitionHolderTreeNode? node))
                    node.Add(type);
                else
                {
                    TemplateDefinitionHolderTreeNode newNode = new();
                    m_TemplateDefinitions[type.Signature.ID] = newNode;
                    newNode.Add(type);
                }
            }

            public TypeDefinition[] Values
            {
                get
                {
                    List<TypeDefinition> ret = [];
                    foreach (TemplateDefinitionHolderTreeNode node in m_TemplateDefinitions.Values)
                        ret.AddRange(node.Values);
                    return [.. ret];
                }
            }
        }

        private readonly TemplateDefinitionHolder m_TemplateDefinitions = new();
        private readonly TemplateInstanceHolder m_TemplateTypeInstances = new();
        private readonly Dictionary<int, ATypeInstance> m_TypeInstances = [];
        private readonly Dictionary<int, AFunction> m_Functions = [];
        private readonly Dictionary<int, Namespace> m_Namespaces = [];
        private readonly List<AInstruction> m_Instructions = [];
        public AInstruction[] Instructions => [.. m_Instructions];

        public TypeDefinition[] Definitions => [.. m_TemplateDefinitions.Values];
        public ATypeInstance[] TemplateTypeInstances => m_TemplateTypeInstances.Values;
        public ATypeInstance[] Instances => [.. m_TypeInstances.Values];
        public AFunction[] Functions => [.. m_Functions.Values];
        public Namespace[] Namespaces => [.. m_Namespaces.Values];

        public void AddInstruction(AInstruction instruction) => m_Instructions.Add(instruction);

        public void AddTypeDefinition(TypeDefinition type) => m_TemplateDefinitions.Add(type);

        public void AddType(ATypeInstance type) => m_TypeInstances[type.TypeInfo.ID] = type;

        internal void AddTemplateTypeInstance(TypeInfo typeInfo, ATypeInstance type) => m_TemplateTypeInstances.Add(typeInfo, type);
        internal void RemoveTemplateTypeInstance(TypeInfo typeInfo) => m_TemplateTypeInstances.Remove(typeInfo);

        public bool AddFunction(AFunction function)
        {
            if (m_Functions.ContainsKey(function.Signature.ID))
                return false;
            m_Functions[function.Signature.ID] = function;
            return true;
        }

        public bool AddNamespace(Namespace @namespace)
        {
            if (m_Namespaces.ContainsKey(@namespace.ID))
                return false;
            m_Namespaces[@namespace.ID] = @namespace;
            return true;
        }

        private Namespace? SearchNamespace(int namespaceID)
        {
            if (m_Namespaces.TryGetValue(namespaceID, out Namespace? ns))
                return ns;
            return null;
        }

        private ATypeInstance? InternalInstantiate(TypeInfo typeInfo, int namespaceIdx, bool isArray)
        {
            if (typeInfo.NamespacesID.Length > namespaceIdx)
            {
                if (m_Namespaces.TryGetValue(typeInfo.NamespacesID[namespaceIdx], out Namespace? @namespace))
                    return @namespace.InternalInstantiate(typeInfo, namespaceIdx + 1, isArray);
                return null;
            }
            if (isArray)
            {
                ATypeInstance? arrayType = InternalInstantiate(typeInfo, namespaceIdx, false);
                if (arrayType != null)
                    return new ArrayType(arrayType);
                return null;
            }
            else if (typeInfo.TemplateTypes.Length > 0)
            {
                if (m_TemplateTypeInstances.TryGetValue(typeInfo, out ATypeInstance? templateObjectInstance))
                    return templateObjectInstance;
                else if (m_TemplateDefinitions.TryGetValue(typeInfo, out TypeDefinition? ret))
                    return ret.Instantiate(typeInfo, this);
                return null;
            }
            else if (Types.TryGet(typeInfo.ID, out ATypeInstance? primitiveInstance))
                return primitiveInstance;
            else if (m_TypeInstances.TryGetValue(typeInfo.ID, out ATypeInstance? objectInstance))
                return objectInstance;
            return null;
        }

        public ATypeInstance? Instantiate(string type, ConversionTable conversionTable)
        {
            OperationResult<TypeInfo> typeInfo = TypeInfo.ParseStr(type, conversionTable);
            if (!typeInfo)
                return null;
            return Instantiate(typeInfo.Result!);
        }

        public ATypeInstance? Instantiate(TypeInfo typeInfo) => InternalInstantiate(typeInfo, 0, typeInfo.IsArray);

        internal object? InternalExecute(Environment env)
        {
            FunctionStack stack = new();
            foreach (AInstruction instruction in m_Instructions)
            {
                if (instruction is Break || instruction is Continue)
                    return new();
                else
                {
                    instruction.ExecuteInstruction(env, stack);
                    if (stack.HasReturn)
                        return new();
                }
            }
            return stack.ReturnValue;
        }
    }
}
