namespace CorpseLib.Scripts.Type
{
    public abstract class ATypeInstance
    {
        private readonly Namespace? m_Namespace;
        private readonly int m_ID;

        public int ID => m_ID;

        public string GetFullName(ConversionTable conversionTable)
        {
            if (m_Namespace != null)
                return string.Format("{0}.{1}", m_Namespace.GetName(conversionTable), conversionTable.GetName(m_ID));
            return conversionTable.GetName(m_ID);
        }

        internal ATypeInstance(Namespace? @namespace, int id)
        {
            m_Namespace = @namespace;
            m_ID = id;
        }

        internal object[]? InternalParse(string str)
        {
            if (str == "null")
                return [];
            return Parse(str);
        }

        public abstract bool IsOfType(object[]? value);
        public abstract object[]? Parse(string str);
        public abstract string ToString(object[]? value);
    }
}
