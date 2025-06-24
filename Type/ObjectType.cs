using System.Text;

namespace CorpseLib.Scripts.Type
{
    public class ObjectType(TypeInfo typeInfo) : ATypeInstance(typeInfo)
    {
        private readonly List<Parameter> m_Attributes = [];

        public Parameter[] Attributes => [..m_Attributes];

        internal bool AddAttribute(Parameter attributeToAdd)
        {
            foreach (Parameter attribute in m_Attributes)
            {
                if (attribute.ID == attributeToAdd.ID)
                    return false;
            }
            m_Attributes.Add(attributeToAdd);
            return true;
        }

        private static bool CheckObject(object[] value)
        {
            foreach (object obj in value)
            {
                if (obj is not object[])
                    return false;
            }
            return true;
        }

        public override object[]? Convert(object[] str)
        {
            if (str.Length == 0)
                return [];
            if (!CheckObject(str))
                return null;
            int i = 0;
            List<Variable> variables = [];
            foreach (Parameter attribute in m_Attributes)
            {
                if (i < str.Length)
                    variables.Add(m_Attributes[i].Convert((object[])str[i]) ?? throw new ArgumentException("Invalid object"));
                else
                    variables.Add(m_Attributes[i].Instantiate() ?? throw new ArgumentException("Invalid object"));
                ++i;
            }
            return [..variables];
        }

        public override bool IsOfType(object[]? value)
        {
            if (value == null)
                return false;
            int i = 0;
            foreach (Parameter attribute in m_Attributes)
            {
                if (value.Length < i)
                {
                    if (!attribute.Type.IsOfType([value[i]]))
                        return false;
                }
                else
                {
                    if (!attribute.HaveDefaultValue)
                        return false;
                }
                ++i;
            }
            return true;
        }
    }
}
