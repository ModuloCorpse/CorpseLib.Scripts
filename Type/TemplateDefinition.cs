using CorpseLib.Scripts.Type.Primitive;
using System.Text;

namespace CorpseLib.Scripts.Type
{
    public class TemplateDefinition
    {
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

        public static OperationResult<Tuple<string, string[]>> ExtractTypeName(string template)
        {
            int templateIdx = template.IndexOf('<');
            if (templateIdx >= 0)
            {
                OperationResult<string[]> templatesResult = SplitTemplate(template[(templateIdx + 1)..^1]);
                if (templatesResult && templatesResult.Result != null)
                {
                    string[] templates = templatesResult.Result;
                    StringBuilder templateNameBuilder = new(template[..(templateIdx + 1)]);
                    for (int i = 1; i < templates.Length; ++i)
                        templateNameBuilder.Append(',');
                    templateNameBuilder.Append('>');
                    return new(new(templateNameBuilder.ToString(), templates));
                }
                return new(templatesResult.Error, templatesResult.Description);
            }
            return new(new(template, []));
        }

        private abstract class AAttributeDefinition
        {
            public abstract int GetID();
            public abstract string ToScriptString(ConversionTable conversionTable);
            public abstract Parameter? Instantiate(bool isConst, Dictionary<string, string> templates, Namespace @namespace);
        }

        private class ParameterAttributeDefinition(Parameter parameter) : AAttributeDefinition
        {
            private readonly Parameter m_Parameter = parameter;
            public override int GetID() => m_Parameter.ID;
            public override string ToScriptString(ConversionTable conversionTable) => m_Parameter.ToScriptString(conversionTable);
            public override Parameter? Instantiate(bool isConst, Dictionary<string, string> templates, Namespace @namespace) => m_Parameter;
        }

        private class TemplateAttributeDefinition : AAttributeDefinition
        {
            private readonly string m_Type;
            private readonly int m_ID;
            private readonly string? m_Value = null;

            public TemplateAttributeDefinition(string[] parameter)
            {
                m_Type = parameter[0];
                m_ID = parameter[1].GetHashCode();
                if (parameter.Length == 3)
                    m_Value = parameter[2];
            }
            public override int GetID() => m_ID;
            public override string ToScriptString(ConversionTable conversionTable)
            {
                StringBuilder builder = new();
                builder.Append(m_Type);
                builder.Append(' ');
                builder.Append(conversionTable.GetName(m_ID));
                if (m_Value != null)
                {
                    builder.Append(" = ");
                    builder.Append(m_Value);
                }
                return builder.ToString();
            }

            private static OperationResult<string> ReplaceTemplateInType(string type, Dictionary<string, string> templates)
            {
                if (type.Contains('<'))
                {
                    StringBuilder builder = new();
                    OperationResult<Tuple<string, string[]>> tupleResult = ExtractTypeName(type);
                    if (!tupleResult)
                        return new(tupleResult.Error, tupleResult.Description);
                    Tuple<string, string[]> tuple = tupleResult.Result!;
                    builder.Append(tuple.Item1[..(tuple.Item1.IndexOf('<') + 1)]);
                    int i = 0;
                    foreach (string templateType in tuple.Item2)
                    {
                        if (i != 0)
                            builder.Append(", ");
                        OperationResult<string> templateRealType = ReplaceTemplateInType(templateType, templates);
                        if (!templateRealType)
                            return templateRealType;
                        builder.Append(templateRealType.Result);
                    }
                    builder.Append('>');
                    return new(builder.ToString());
                }
                else if (templates.TryGetValue(type, out string? realType))
                    return new(realType);
                else
                    return new(type);
            }

            public override Parameter? Instantiate(bool isConst, Dictionary<string, string> templates, Namespace @namespace)
            {
                OperationResult<string> templateRealType = ReplaceTemplateInType(m_Type, templates);
                if (!templateRealType)
                    return null;
                ATypeInstance? parameterType = @namespace.Instantiate(templateRealType.Result!);
                if (parameterType == null)
                    return null;
                if (parameterType is VoidType)
                    throw new ArgumentException("Parameter type cannot be void");
                if (m_Value != null)
                    return new(parameterType, isConst, m_ID, parameterType.InternalParse(m_Value));
                else
                    return new(parameterType, isConst, m_ID);
            }

        }

        //TODO Rework
        private readonly List<AAttributeDefinition> m_Attributes = [];
        private readonly string[] m_Templates;
        private readonly string m_Name;

        public string Name => m_Name;

        internal TemplateDefinition(string name, string[] templates)
        {
            m_Name = name;
            m_Templates = templates;
        }

        private bool SearchAttribute(int id)
        {
            foreach (AAttributeDefinition attribute in m_Attributes)
            {
                if (attribute.GetID() == id)
                    return true;
            }
            return false;
        }

        internal bool AddAttributeDefinition(Parameter parameter)
        {
            if (SearchAttribute(parameter.ID))
                return false;
            m_Attributes.Add(new ParameterAttributeDefinition(parameter));
            return true;
        }

        internal bool AddAttributeDefinition(string[] parameter)
        {
            if (SearchAttribute(parameter[1].GetHashCode()))
                return false;
            m_Attributes.Add(new TemplateAttributeDefinition(parameter));
            return true;
        }

        internal ATypeInstance? Instantiate(bool isConst, string[] templates, Namespace @namespace)
        {
            if (templates.Length != m_Templates.Length)
                throw new ArgumentException("Invalid number of template");
            Dictionary<string, string> templateDictionary = [];
            for (int n = 0; n != templates.Length; ++n)
                templateDictionary[m_Templates[n]] = templates[n];
            StringBuilder builder = new(Name[..(Name.IndexOf('<') + 1)]);
            int i = 0;
            foreach (string template in m_Templates)
            {
                if (i != 0)
                    builder.Append(", ");
                builder.Append(templates[i]);
                ++i;
            }
            builder.Append('>');
            ObjectType type = new(@namespace, builder.ToString());
            @namespace.AddTemplateTypeInstance(type.Name.Replace(" ", ""), type);
            foreach (AAttributeDefinition attribute in m_Attributes)
            {
                Parameter? intance = attribute.Instantiate(isConst, templateDictionary, @namespace);
                if (intance == null)
                {
                    @namespace.RemoveTemplateTypeInstance(type.Name.Replace(" ", ""));
                    return null;
                }
                type.AddAttribute(intance);
            }
            return type;
        }

        public string ToScriptString(ConversionTable conversionTable)
        {
            StringBuilder stringBuilder = new("struct ");
            stringBuilder.Append(Name[..(Name.IndexOf('<') + 1)]);
            int i = 0;
            foreach (string template in m_Templates)
            {
                if (i != 0)
                    stringBuilder.Append(", ");
                stringBuilder.Append(template);
                ++i;
            }
            stringBuilder.Append('>');
            stringBuilder.Append(" { ");
            foreach (AAttributeDefinition attribute in m_Attributes)
            {
                stringBuilder.Append(attribute.ToScriptString(conversionTable));
                stringBuilder.Append("; ");
            }
            stringBuilder.Append('}');
            return stringBuilder.ToString();
        }
    }
}
