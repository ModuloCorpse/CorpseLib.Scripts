namespace CorpseLib.Scripts.Parameters
{
    public class ParameterType(int type, bool isStatic, bool isConst, bool isRef, int arrayCount)
    {
        private readonly int m_Type = type;
        private readonly int m_ArrayCount = arrayCount;
        private readonly bool m_IsStatic = isStatic;
        private readonly bool m_IsConst = isConst;
        private readonly bool m_IsRef = isRef;

        public int TypeID => m_Type;
        public int ArrayCount => m_ArrayCount;
        public bool IsStatic => m_IsStatic;
        public bool IsConst => m_IsConst;
        public bool IsRef => m_IsRef;
    }
}
