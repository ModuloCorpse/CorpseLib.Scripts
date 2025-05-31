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

        internal object[]? InternalConvert(object[] value)
        {
            if (value.Length == 0)
                return [];
            return Convert(value);
        }

        public abstract bool IsOfType(object[]? value);
        public abstract object[]? Convert(object[] str);
    }
}
