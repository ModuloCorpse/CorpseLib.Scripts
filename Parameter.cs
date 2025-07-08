using CorpseLib.Scripts.Memories;

namespace CorpseLib.Scripts
{
    public class Parameter(ParameterType type, int id, IMemoryValue? defaultValue)
    {
        private readonly ParameterType m_Type = type;
        private readonly int m_ID = id;
        private readonly IMemoryValue? m_DefaultValue = defaultValue;

        public ParameterType Type => m_Type;
        public int TypeID => m_Type.TypeID;
        public int ID => m_ID;
        public IMemoryValue? DefaultValue => m_DefaultValue;
        public bool HaveDefaultValue => m_DefaultValue != null;
        public int ArrayCount => m_Type.ArrayCount;
        public bool IsStatic => m_Type.IsStatic;
        public bool IsConst => m_Type.IsConst;
        public bool IsRef => m_Type.IsRef;

        public Parameter(ParameterType type, int id) : this(type, id, null) { }

        public override bool Equals(object? obj) => obj is Parameter parameter && m_Type == parameter.m_Type && m_ID == parameter.m_ID && m_DefaultValue == parameter.m_DefaultValue;
        public override int GetHashCode() => HashCode.Combine(m_Type, m_ID, m_DefaultValue);
    }
}
