namespace CorpseLib.Scripts.Type
{
    public abstract class ATypeInstance
    {
        private readonly TypeInfo m_TypeInfo;
        private readonly bool m_IsPrimitive;

        public TypeInfo TypeInfo => m_TypeInfo;
        public bool IsPrimitive => m_IsPrimitive;

        internal ATypeInstance(TypeInfo typeInfo, bool isPrimitive)
        {
            m_TypeInfo = typeInfo!;
            m_IsPrimitive = isPrimitive;
        }
    }
}
