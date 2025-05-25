using CorpseLib.Scripts.Type.Primitive;

namespace CorpseLib.Scripts.Type
{
    public class TemplateDefinition
    {
        internal abstract class AAttributeDefinition
        {
            public abstract int GetNameID();
            public abstract Parameter? Instantiate(Dictionary<int, TypeInfo> templateTypeGiven, Environment env);
        }

        internal class ParameterAttributeDefinition(Parameter parameter) : AAttributeDefinition
        {
            private readonly Parameter m_Parameter = parameter;
            internal Parameter Parameter => m_Parameter;
            public override int GetNameID() => m_Parameter.ID;
            public override Parameter? Instantiate(Dictionary<int, TypeInfo> templateTypeGiven, Environment env) => m_Parameter;
        }

        internal class TemplateAttributeDefinition(TypeInfo typeInfo, int nameID, string? value) : AAttributeDefinition
        {
            private readonly TypeInfo m_TypeInfo = typeInfo;
            private readonly int m_ID = nameID;
            private readonly string? m_Value = value;

            internal TypeInfo TypeInfo => m_TypeInfo;
            internal string? Value => m_Value;

            public override int GetNameID() => m_ID;

            private static TypeInfo ConvertTemplatedType(TypeInfo typeInfo, Dictionary<int, TypeInfo> templateTypeGiven)
            {
                TypeInfo baseType;
                if (templateTypeGiven.TryGetValue(typeInfo.ID, out TypeInfo? template))
                    baseType = template!;
                else
                    baseType = typeInfo;
                List<TypeInfo> newTemplates = [];
                foreach (TypeInfo subTemplate in baseType.TemplateTypes)
                {
                    if (templateTypeGiven.TryGetValue(subTemplate.ID, out TypeInfo? subTemplateType))
                        newTemplates.Add(ConvertTemplatedType(subTemplateType!, templateTypeGiven));
                    else
                        newTemplates.Add(ConvertTemplatedType(subTemplate, templateTypeGiven));
                }
                return new(baseType.IsConst, baseType.NamespacesID, baseType.ID, [..newTemplates], baseType.IsArray);
            }

            public override Parameter? Instantiate(Dictionary<int, TypeInfo> templateTypeGiven, Environment env)
            {
                TypeInfo resolvedTypeInfo = ConvertTemplatedType(m_TypeInfo, templateTypeGiven);
                ATypeInstance? parameterType = env.Instantiate(resolvedTypeInfo);
                if (parameterType == null)
                    return null;
                if (parameterType is VoidType)
                    throw new ArgumentException("Parameter type cannot be void");
                if (m_Value != null)
                    return new(parameterType, m_TypeInfo.IsConst, m_ID, parameterType.InternalParse(m_Value));
                else
                    return new(parameterType, m_TypeInfo.IsConst, m_ID);
            }

        }

        //TODO Rework
        private readonly List<AAttributeDefinition> m_Attributes = [];
        private readonly int[] m_Templates;
        private readonly int m_ID;

        public int ID => m_ID;
        public int[] Templates => m_Templates;
        internal AAttributeDefinition[] Attributes => [.. m_Attributes];

        internal TemplateDefinition(int id, int[] templates)
        {
            m_ID = id;
            m_Templates = templates;
        }

        private bool SearchAttribute(int id)
        {
            foreach (AAttributeDefinition attribute in m_Attributes)
            {
                if (attribute.GetNameID() == id)
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

        internal bool AddAttributeDefinition(TypeInfo typeInfo, int nameID, string? value)
        {
            if (SearchAttribute(nameID))
                return false;
            m_Attributes.Add(new TemplateAttributeDefinition(typeInfo, nameID, value));
            return true;
        }

        internal ATypeInstance? Instantiate(TypeInfo typeInfo, Environment env)
        {
            if (typeInfo.TemplateTypes.Length != m_Templates.Length)
                throw new ArgumentException("Invalid number of template");
            Dictionary<int, TypeInfo> templateDictionary = [];
            for (int n = 0; n != m_Templates.Length; ++n)
                templateDictionary[m_Templates[n]] = typeInfo.TemplateTypes[n];
            ObjectType type = new(typeInfo);
            env.AddTemplateTypeInstance(typeInfo, type);
            foreach (AAttributeDefinition attribute in m_Attributes)
            {
                Parameter? intance = attribute.Instantiate(templateDictionary, env);
                if (intance == null)
                {
                    env.RemoveTemplateTypeInstance(typeInfo);
                    return null;
                }
                type.AddAttribute(intance);
            }
            return type;
        }
    }
}
