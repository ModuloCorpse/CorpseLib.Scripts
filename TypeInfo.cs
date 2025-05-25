using System.Text;

namespace CorpseLib.Scripts
{
    public class TypeInfo(bool isConst, int[] namespaces, int id, TypeInfo[] templates, bool isArray)
    {
        private readonly TypeInfo[] m_TemplateTypes = templates;
        private readonly int[] m_NamespacesID = namespaces;
        private readonly int m_ID = id;
        private readonly bool m_IsArray = isArray;
        private readonly bool m_IsConst = isConst;

        public TypeInfo[] TemplateTypes => m_TemplateTypes;
        public int[] NamespacesID => m_NamespacesID;
        public int ID => m_ID;
        public bool IsArray => m_IsArray;
        public bool IsConst => m_IsConst;

        public static OperationResult<string[]> SplitTemplate(string template)
        {
            List<string> result = [];
            StringBuilder builder = new();
            int templateCount = 0;
            foreach (char c in template)
            {
                if (templateCount != 0)
                {
                    if (c == '>')
                    {
                        --templateCount;
                        if (templateCount < 0)
                            return new("Template error", string.Format("Bad template {0}", template));
                    }
                    else if (c == '<')
                        ++templateCount;
                    if (!char.IsWhiteSpace(c))
                        builder.Append(c);
                }
                else if (c == ',')
                {
                    if (builder.Length > 0)
                    {
                        result.Add(builder.ToString());
                        builder.Clear();
                    }
                    else
                        return new("Template error", string.Format("Bad template {0}", template));
                }
                else if (c == '<')
                {
                    ++templateCount;
                    builder.Append(c);
                }
                else if (c == '>')
                    return new("Template error", string.Format("Bad template {0}", template));
                else
                    builder.Append(c);
            }
            if (builder.Length > 0)
                result.Add(builder.ToString());
            return new([.. result]);
        }

        public static OperationResult<TypeInfo> ParseStr(string typeToParse, ConversionTable conversionTable)
        {
            string str = typeToParse.Trim();
            bool isConst = false;
            if (str.StartsWith("const "))
            {
                isConst = true;
                str = str[6..];
            }
            bool isArray = false;
            if (str.EndsWith("[]"))
            {
                isArray = true;
                str = str[..^2].Trim();
            }
            List<TypeInfo> templateTypes = [];
            int templateIdx = str.IndexOf('<');
            if (templateIdx >= 0)
            {
                string template = str[(templateIdx + 1)..];
                str = str[..templateIdx];
                if (template.EndsWith('>'))
                {
                    OperationResult<string[]> templateTypesStr = SplitTemplate(template[..^1]);
                    if (!templateTypesStr)
                        return templateTypesStr.Cast<TypeInfo>();
                    foreach (string templateTypeStr in templateTypesStr.Result!)
                    {
                        OperationResult<TypeInfo> templateTypeInfo = ParseStr(templateTypeStr, conversionTable);
                        if (!templateTypeInfo)
                            return templateTypeInfo;
                        templateTypes.Add(templateTypeInfo.Result!);
                    }
                }
                else
                    return new("Parsed type error", string.Format("Non-closing template in {0}", typeToParse));
            }
            List<int> namespaceIDs = [];
            int idx = str.IndexOf('.');
            while (idx != -1)
            {
                string namespaceName = str[..idx];
                if (string.IsNullOrEmpty(namespaceName))
                    return new("Parsed type error", string.Format("Empty namespace in {0}", typeToParse));
                namespaceIDs.Add(conversionTable.PushName(namespaceName));
                str = str[(idx + 1)..];
                idx = str.IndexOf('.');
            }
            return new(new(isConst, [.. namespaceIDs], conversionTable.PushName(str), [.. templateTypes], isArray));
        }

        public override bool Equals(object? obj) => obj is TypeInfo type &&
            EqualityComparer<TypeInfo[]>.Default.Equals(m_TemplateTypes, type.m_TemplateTypes) &&
            EqualityComparer<int[]>.Default.Equals(m_NamespacesID, type.m_NamespacesID) &&
            m_ID == type.m_ID &&
            m_IsArray == type.m_IsArray &&
            m_IsConst == type.m_IsConst;

        public override int GetHashCode() => HashCode.Combine(m_TemplateTypes, m_NamespacesID, m_ID, m_IsArray, m_IsConst);
    }
}
