using CorpseLib.Scripts.Type;
using CorpseLib.Scripts.Type.Primitive;

namespace CorpseLib.Scripts.Context
{
    public class TypeDefinition
    {
        internal abstract class AAttributeDefinition(TypeInfo typeInfo, int[] tags, int[] comments, int id, object[]? value)
        {
            private readonly TypeInfo m_TypeInfo = typeInfo;
            private readonly int[] m_Tags = tags;
            private readonly int[] m_Comments = comments;
            private readonly int m_ID = id;
            private readonly object[]? m_Value = value;

            internal TypeInfo TypeInfo => m_TypeInfo;
            internal object[]? Value => m_Value;
            internal int ID => m_ID;

            public abstract Parameter? Instantiate(Dictionary<int, TypeInfo> templateTypeGiven, Environment env);
        }

        internal class AttributeDefinition(TypeInfo typeInfo, int[] tags, int[] comments, int id, object[]? value) : AAttributeDefinition(typeInfo, tags, comments, id, value)
        {
            public override Parameter? Instantiate(Dictionary<int, TypeInfo> templateTypeGiven, Environment env)
            {
                ATypeInstance? parameterType = env.Instantiate(TypeInfo);
                if (parameterType == null)
                    return null;
                if (parameterType is VoidType)
                    throw new ArgumentException("Parameter type cannot be void");
                if (Value != null)
                    return new(parameterType, TypeInfo.IsConst, ID, parameterType.InternalConvert(Value));
                else
                    return new(parameterType, TypeInfo.IsConst, ID);
            }
        }

        internal class TemplateAttributeDefinition(TypeInfo typeInfo, int[] tags, int[] comments, int nameID, object[]? value) : AAttributeDefinition(typeInfo, tags, comments, nameID, value)
        {
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
                TypeInfo resolvedTypeInfo = ConvertTemplatedType(TypeInfo, templateTypeGiven);
                ATypeInstance? parameterType = env.Instantiate(resolvedTypeInfo);
                if (parameterType == null)
                    return null;
                if (parameterType is VoidType)
                    throw new ArgumentException("Parameter type cannot be void");
                if (Value != null)
                    return new(parameterType, TypeInfo.IsConst, ID, parameterType.InternalConvert(Value));
                else
                    return new(parameterType, TypeInfo.IsConst, ID);
            }

        }

        //TODO Rework
        private readonly List<AAttributeDefinition> m_Attributes = [];
        private readonly Signature m_Signature;
        private readonly int[] m_Templates;

        public Signature Signature => m_Signature;
        public int[] Templates => m_Templates;
        internal AAttributeDefinition[] Attributes => [.. m_Attributes];

        internal TypeDefinition(Signature signature, int[] templates)
        {
            m_Signature = signature;
            m_Templates = templates;
        }

        private bool SearchAttribute(int id)
        {
            foreach (AAttributeDefinition attribute in m_Attributes)
            {
                if (attribute.ID == id)
                    return true;
            }
            return false;
        }

        internal bool AddAttribute(TypeInfo typeInfo, int[] tags, int[] comments, int id, object[]? value)
        {
            if (SearchAttribute(id))
                return false;
            m_Attributes.Add(new AttributeDefinition(typeInfo, tags, comments, id, value));
            return true;
        }

        internal bool AddTemplateAttribute(TypeInfo typeInfo, int[] tags, int[] comments, int id, object[]? value)
        {
            if (SearchAttribute(id))
                return false;
            m_Attributes.Add(new TemplateAttributeDefinition(typeInfo, tags, comments, id, value));
            return true;
        }

        internal ATypeInstance? Instantiate(TypeInfo typeInfo, TypeObject typeObject, Environment env)
        {
            if (typeInfo.TemplateTypes.Length != m_Templates.Length)
                throw new ArgumentException("Invalid number of template");
            Dictionary<int, TypeInfo> templateDictionary = [];
            for (int n = 0; n != m_Templates.Length; ++n)
                templateDictionary[m_Templates[n]] = typeInfo.TemplateTypes[n];
            ObjectType type = new(typeInfo);
            typeObject.AddTemplateTypeInstance(typeInfo, type);
            foreach (AAttributeDefinition attribute in m_Attributes)
            {
                Parameter? intance = attribute.Instantiate(templateDictionary, env);
                if (intance == null)
                {
                    typeObject.RemoveTemplateTypeInstance(typeInfo);
                    return null;
                }
                type.AddAttribute(intance);
            }
            return type;
        }
    }
}
