namespace CorpseLib.Scripts.Type
{
    public abstract class ATypeInstance
    {
        private readonly TypeInfo? m_TypeInfo;

        public TypeInfo TypeInfo => m_TypeInfo!;

        internal ATypeInstance(TypeInfo? typeInfo)
        {
            m_TypeInfo = typeInfo;
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
