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
                private int m_Value = -1;

                public int SearchTypeInstance(TypeInfo[] typeInfos, int idx)
                {
                    if (idx >= typeInfos.Length)
                        return m_Value;
                    else if (m_TypeInstances.TryGetValue(typeInfos[idx].ID, out TypeInstanceHolderTreeNode? node))
                        return node.SearchTypeInstance(typeInfos, idx + 1);
                    return -1;
                }

                public void InsertTypeInstance(TypeInfo[] typeInfos, int idx, int value)
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
                        m_Value = -1;
                        return;
                    }
                    else if (m_TypeInstances.TryGetValue(typeInfos[idx].ID, out TypeInstanceHolderTreeNode? node))
                    {
                        node.RemoveTypeInstance(typeInfos, idx + 1);
                        if (node.m_Value == -1 && node.m_TypeInstances.Count == 0)
                            m_TypeInstances.Remove(typeInfos[idx].ID);
                    }
                }
            }

            private readonly Dictionary<int, TypeInstanceHolderTreeNode> m_TemplateInstances = [];

            public bool TryGetValue(TypeInfo typeInfo, [MaybeNullWhen(false)] out int value)
            {
                if (m_TemplateInstances.TryGetValue(typeInfo.ID, out TypeInstanceHolderTreeNode? node))
                {
                    int ret = node.SearchTypeInstance(typeInfo.TemplateTypes, 0);
                    if (ret != -1)
                    {
                        value = ret;
                        return true;
                    }
                }
                value = -1;
                return false;
            }

            public void Add(TypeInfo typeInfo, int value)
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

        public void AddTypeInstance(TypeInfo type, int index) => m_TypeInstances.Add(type, index);
        public void AddTypeDefinition(TypeDefinition type, int[] tags, int[] comments) => m_TypeDefinitions[type.Templates.Length] = new(type, tags, comments);
        public void RemoveType(TypeInfo typeInfo) => m_TypeInstances.Remove(typeInfo);

        public int Instantiate(TypeInfo typeInfo, Environment env)
        {
            if (m_TypeInstances.TryGetValue(typeInfo, out int templateObjectInstanceIndex))
                return templateObjectInstanceIndex;
            else if (m_TypeDefinitions.TryGetValue(typeInfo.TemplateTypes.Length, out TypeDefinitionObject? ret))
                return ret.TypeDefinition.Instantiate(typeInfo, env);
            return -1;
        }

        public override bool IsValid() => true;
    }
}
