using CorpseLib.Scripts.Type;
using System.Diagnostics.CodeAnalysis;

namespace CorpseLib.Scripts.Context
{
    public class TypeObject(int id, int[] tags, int[] comments) : EnvironmentObject(id, tags, comments)
    {
        private class TypeInstanceHolder
        {
            private class TypeInstanceHolderTreeNode
            {
                private readonly Dictionary<int, TypeInstanceHolderTreeNode> m_TypeInstances = [];
                private ATypeInstance? m_Value = null;

                public ATypeInstance? SearchTypeInstance(TypeInfo[] typeInfos, int idx)
                {
                    if (idx >= typeInfos.Length)
                        return m_Value;
                    else if (m_TypeInstances.TryGetValue(typeInfos[idx].ID, out TypeInstanceHolderTreeNode? node))
                        return node.SearchTypeInstance(typeInfos, idx + 1);
                    return null;
                }

                public void InsertTypeInstance(TypeInfo[] typeInfos, int idx, ATypeInstance value)
                {
                    if (idx >= typeInfos.Length)
                    {
                        m_Value = value;
                        return;
                    }
                    else if (m_TypeInstances.TryGetValue(typeInfos[idx].ID, out TypeInstanceHolderTreeNode? node))
                        node.InsertTypeInstance(typeInfos, idx + 1, value);
                    else
                    {
                        TypeInstanceHolderTreeNode newNode = new();
                        m_TypeInstances[typeInfos[idx].ID] = newNode;
                        newNode.InsertTypeInstance(typeInfos, idx + 1, value);
                    }
                }

                public void RemoveTypeInstance(TypeInfo[] typeInfos, int idx)
                {
                    if (idx >= typeInfos.Length)
                    {
                        m_Value = null;
                        return;
                    }
                    else if (m_TypeInstances.TryGetValue(typeInfos[idx].ID, out TypeInstanceHolderTreeNode? node))
                    {
                        node.RemoveTypeInstance(typeInfos, idx + 1);
                        if (node.m_Value == null && node.m_TypeInstances.Count == 0)
                            m_TypeInstances.Remove(typeInfos[idx].ID);
                    }
                }

                public void AppendToList(List<ATypeInstance> list)
                {
                    if (m_Value != null)
                        list.Add(m_Value);
                    foreach (TypeInstanceHolderTreeNode node in m_TypeInstances.Values)
                        node.AppendToList(list);
                }
            }

            private readonly Dictionary<int, TypeInstanceHolderTreeNode> m_TemplateInstances = [];

            public bool TryGetValue(TypeInfo typeInfo, [MaybeNullWhen(false)] out ATypeInstance value)
            {
                if (m_TemplateInstances.TryGetValue(typeInfo.ID, out TypeInstanceHolderTreeNode? node))
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
                if (m_TemplateInstances.TryGetValue(typeInfo.ID, out TypeInstanceHolderTreeNode? node))
                    node.InsertTypeInstance(typeInfo.TemplateTypes, 0, value);
                else
                {
                    TypeInstanceHolderTreeNode newNode = new();
                    m_TemplateInstances[typeInfo.ID] = newNode;
                    newNode.InsertTypeInstance(typeInfo.TemplateTypes, 0, value);
                }
            }

            public void Remove(TypeInfo typeInfo)
            {
                if (m_TemplateInstances.TryGetValue(typeInfo.ID, out TypeInstanceHolderTreeNode? node))
                    node.RemoveTypeInstance(typeInfo.TemplateTypes, 0);
            }

            public ATypeInstance[] Values
            {
                get
                {
                    List<ATypeInstance> ret = [];
                    foreach (TypeInstanceHolderTreeNode node in m_TemplateInstances.Values)
                        node.AppendToList(ret);
                    return [.. ret];
                }
            }
        }

        public class TypeDefinitionObject(TypeDefinition typeDefinition, int[] tags, int[] comments)
        {
            private readonly TypeDefinition m_TypeDefinition = typeDefinition;
            private readonly int[] m_Tags = tags;
            private readonly int[] m_Comments = comments;

            public TypeDefinition TypeDefinition => m_TypeDefinition;
            public int[] Tags => m_Tags;
            public int[] Comments => m_Comments;
        }

        private readonly TypeInstanceHolder m_TypeInstances = new();

        private readonly Dictionary<int, TypeDefinitionObject> m_TypeDefinitions = [];

        public bool TryGetValue(TypeInfo typeInfo, [MaybeNullWhen(false)] out TypeDefinition value)
        {
            if (m_TypeDefinitions.TryGetValue(typeInfo.TemplateTypes.Length, out TypeDefinitionObject? ret))
            {
                value = ret.TypeDefinition;
                return true;
            }
            value = null;
            return false;
        }
        public TypeDefinitionObject[] Values => [.. m_TypeDefinitions.Values];

        public void AddTypeInstance(ATypeInstance type) => m_TypeInstances.Add(type.TypeInfo, type);
        public void AddTypeDefinition(TypeDefinition type, int[] tags, int[] comments) => m_TypeDefinitions[type.Templates.Length] = new(type, tags, comments);

        internal void AddTemplateTypeInstance(TypeInfo typeInfo, ATypeInstance type) => m_TypeInstances.Add(typeInfo, type);
        internal void RemoveTemplateTypeInstance(TypeInfo typeInfo) => m_TypeInstances.Remove(typeInfo);

        public ATypeInstance? Instantiate(TypeInfo typeInfo, Environment env)
        {
            if (m_TypeInstances.TryGetValue(typeInfo, out ATypeInstance? templateObjectInstance))
                return templateObjectInstance;
            else if (m_TypeDefinitions.TryGetValue(typeInfo.TemplateTypes.Length, out TypeDefinitionObject? ret))
                return ret.TypeDefinition.Instantiate(typeInfo, this, env);
            return null;
        }

        public override bool IsValid() => true;
    }
}
