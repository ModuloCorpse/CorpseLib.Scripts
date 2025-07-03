namespace CorpseLib.Scripts.Type
{
    public class ObjectType(TypeInfo typeInfo) : ATypeInstance(typeInfo, false)
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
    }
}
