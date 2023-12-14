using CorpseLib.Scripts.Type;

namespace CorpseLib.Scripts
{
    public class Parameter(ATypeInstance type, bool isConst, string name, object[]? defaultValue)
    {
        private readonly ATypeInstance m_Type = type;
        private readonly string m_Name = name;
        private readonly object[]? m_DefaultValue = defaultValue;
        private readonly bool m_IsConst = isConst;

        public ATypeInstance Type => m_Type;
        public string Name => m_Name;
        public object[]? DefaultValues => m_DefaultValue;
        public object? DefaultValue => (m_DefaultValue != null && m_DefaultValue.Length == 1) ? m_DefaultValue[0] : null;
        public bool HaveDefaultValue => m_DefaultValue != null;
        public bool IsConst => m_IsConst;

        public Parameter(ATypeInstance type, bool isConst, string name) : this(type, isConst, name, null) { }

        public Variable? Instantiate()
        {
            if (m_DefaultValue != null)
                return new(m_Type, m_DefaultValue, true);
            return new(m_Type);
        }

        public Variable? Instantiate(string value) => new(m_Type, m_Type.InternalParse(value), false);

        public override bool Equals(object? obj) => obj is Parameter parameter && m_Type == parameter.m_Type && m_Name == parameter.m_Name && m_DefaultValue == parameter.m_DefaultValue;
        public override int GetHashCode() => HashCode.Combine(m_Type, m_Name, m_DefaultValue);
        public override string ToString()
        {
            if (m_IsConst)
            {
                if (m_DefaultValue == null)
                    return string.Format("const {0} {1}", m_Type.GetFullName(), m_Name);
                return string.Format("const {0} {1} = {2}", m_Type.GetFullName(), m_Name, m_Type.ToString(m_DefaultValue));
            }
            if (m_DefaultValue == null)
                return string.Format("{0} {1}", m_Type.GetFullName(), m_Name);
            return string.Format("{0} {1} = {2}", m_Type.GetFullName(), m_Name, m_Type.ToString(m_DefaultValue));
        }
    }
}
