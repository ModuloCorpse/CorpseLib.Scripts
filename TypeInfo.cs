using CorpseLib.Scripts.Context;
using CorpseLib.Scripts.Parser;
using System.Text;

namespace CorpseLib.Scripts
{
    public class TypeInfo(bool isStatic, bool isConst, bool isRef, int[] namespaces, int id, TypeInfo[] templates, int arrayCount) : IEquatable<TypeInfo?>
    {
        private readonly TypeInfo[] m_TemplateTypes = templates;
        private readonly Signature m_Signature = new(namespaces, id);
        private readonly int m_ArrayCount = arrayCount;
        private readonly bool m_IsStatic = isStatic;
        private readonly bool m_IsConst = isConst;
        private readonly bool m_IsRef = isRef;

        public TypeInfo[] TemplateTypes => m_TemplateTypes;
        public Signature Signature => m_Signature;
        public int[] NamespacesID => m_Signature.Namespaces;
        public int ID => m_Signature.ID;
        public int ArrayCount => m_ArrayCount;
        public bool IsStatic => m_IsStatic;
        public bool IsConst => m_IsConst;
        public bool IsRef => m_IsRef;

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
                            return new("Template error", $"Bad template {template}");
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
                        return new("Template error", $"Bad template {template}");
                }
                else if (c == '<')
                {
                    ++templateCount;
                    builder.Append(c);
                }
                else if (c == '>')
                    return new("Template error", $"Bad template {template}");
                else
                    builder.Append(c);
            }
            if (builder.Length > 0)
                result.Add(builder.ToString());
            return new([.. result]);
        }

        public static OperationResult<TypeInfo> ParseStr(string typeToParse, ParsingContext parsingContext)
        {
            string str = typeToParse.Trim();
            int isConst = 0;
            int isStatic = 0;
            while (str.StartsWith("static ") || str.StartsWith("const "))
            {
                if (str.StartsWith("const "))
                {
                    ++isConst;
                    str = str[6..];
                }
                else if (str.StartsWith("static "))
                {
                    ++isStatic;
                    str = str[7..];
                }
            }
            if (isConst > 1)
                parsingContext.RegisterWarning("Multiple const", $"Type '{typeToParse}' has multiple const definitions");
            if (isStatic > 1)
                parsingContext.RegisterWarning("Multiple static", $"Type '{typeToParse}' has multiple static definitions");

            bool isRef = false;
            while (str.EndsWith('&'))
            {
                isRef = true;
                str = str[..^1];
            }
            int arrayCount = 0;
            while (str.EndsWith("[]"))
            {
                ++arrayCount;
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
                        OperationResult<TypeInfo> templateTypeInfo = ParseStr(templateTypeStr, parsingContext);
                        if (!templateTypeInfo)
                            return templateTypeInfo;
                        templateTypes.Add(templateTypeInfo.Result!);
                    }
                }
                else
                    return new("Parsed type error", $"Non-closing template in {typeToParse}");
            }
            List<int> namespaceIDs = [];
            int idx = str.IndexOf("::");
            while (idx != -1)
            {
                string namespaceName = str[..idx];
                if (string.IsNullOrEmpty(namespaceName))
                    return new("Parsed type error", $"Empty namespace in {typeToParse}");
                namespaceIDs.Add(parsingContext.PushName(namespaceName));
                str = str[(idx + 2)..];
                idx = str.IndexOf("::");
            }
            return new(new(isStatic > 0, isConst > 0, isRef, [.. namespaceIDs], parsingContext.PushName(str), [.. templateTypes], arrayCount));
        }

        public override bool Equals(object? obj) => Equals(obj as TypeInfo);
        public bool Equals(TypeInfo? other) => other is not null &&
            EqualityComparer<TypeInfo[]>.Default.Equals(m_TemplateTypes, other.m_TemplateTypes) &&
            EqualityComparer<Signature>.Default.Equals(m_Signature, other.m_Signature) &&
            m_ArrayCount == other.m_ArrayCount && m_IsConst == other.m_IsConst;

        public override int GetHashCode() => HashCode.Combine(m_TemplateTypes, m_Signature, m_ArrayCount, m_IsConst);
        public static bool operator ==(TypeInfo? left, TypeInfo? right) => EqualityComparer<TypeInfo>.Default.Equals(left, right);
        public static bool operator !=(TypeInfo? left, TypeInfo? right) => !(left == right);
    }
}
