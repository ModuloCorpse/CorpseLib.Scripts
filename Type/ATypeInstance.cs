namespace CorpseLib.Scripts.Type
{
    public abstract class ATypeInstance
    {
        private readonly Namespace? m_Namespace;
        private readonly string m_Name;

        public string Name => m_Name;

        public string GetFullName()
        {
            string parentName = m_Namespace?.GetName() ?? string.Empty;
            if (!string.IsNullOrEmpty(parentName))
                return string.Format("{0}.{1}", parentName, m_Name);
            return m_Name;
        }

        internal ATypeInstance(Namespace? @namespace, string name)
        {
            m_Namespace = @namespace;
            m_Name = name;
        }

        internal object[]? InternalParse(string str)
        {
            if (str == "null")
                return [];
            return Parse(str);
        }

        public abstract object[]? Parse(string str);
        public abstract string ToString(object[]? value);
    }
}
