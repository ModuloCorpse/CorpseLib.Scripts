using CorpseLib.Scripts.Type;

namespace CorpseLib.Scripts
{
    public class ParsedType(bool isConst, int[] namespaces, int type, ParsedType[] templates, bool isArray)
    {
        private readonly ParsedType[] m_TemplateTypes = templates;
        private readonly int[] m_NamespacesID = namespaces;
        private readonly int m_TypeID = type;
        private readonly bool m_IsArray = isArray;
        private readonly bool m_IsConst = isConst;

        public ParsedType[] TemplateTypes => m_TemplateTypes;
        public int[] NamespacesID => m_NamespacesID;
        public int TypeID => m_TypeID;
        public bool IsArray => m_IsArray;
        public bool IsConst => m_IsConst;

        public static OperationResult<ParsedType> ParseStr(string typeToParse)
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
            List<ParsedType> templateTypes = [];
            int templateIdx = str.IndexOf('<');
            if (templateIdx >= 0)
            {
                string template = str[(templateIdx + 1)..];
                str = str[..templateIdx];
                if (template.EndsWith('>'))
                {
                    OperationResult<string[]> templateTypesStr = TemplateDefinition.SplitTemplate(template[..^1]);
                    if (!templateTypesStr)
                        return templateTypesStr.Cast<ParsedType>();
                    foreach (string templateTypeStr in templateTypesStr.Result!)
                    {
                        OperationResult<ParsedType> templateParsedType = ParseStr(templateTypeStr);
                        if (!templateParsedType)
                            return templateParsedType;
                        templateTypes.Add(templateParsedType.Result!);
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
                namespaceIDs.Add(namespaceName.GetHashCode());
                str = str[(idx + 1)..];
                idx = str.IndexOf('.');
            }
            return new(new(isConst, [.. namespaceIDs], str.GetHashCode(), [.. templateTypes], isArray));
        }
    }
}
