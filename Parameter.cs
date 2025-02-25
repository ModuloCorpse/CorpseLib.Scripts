using CorpseLib.Scripts.Type;

namespace CorpseLib.Scripts
{
    public class Parameter(ATypeInstance type, bool isConst, int id, object[]? defaultValue)
    {
        private readonly ATypeInstance m_Type = type;
        private readonly int m_ID = id;
        private readonly object[]? m_DefaultValue = defaultValue;
        private readonly bool m_IsConst = isConst;

        public ATypeInstance Type => m_Type;
        public int ID => m_ID;
        public object[]? DefaultValues => m_DefaultValue;
        public object? DefaultValue => (m_DefaultValue != null && m_DefaultValue.Length == 1) ? m_DefaultValue[0] : null;
        public bool HaveDefaultValue => m_DefaultValue != null;
        public bool IsConst => m_IsConst;

        public Parameter(ATypeInstance type, bool isConst, int id) : this(type, isConst, id, null) { }

        public Variable? Instantiate()
        {
            if (m_DefaultValue != null)
                return new(m_Type, m_DefaultValue, true);
            return new(m_Type);
        }

        public Variable? Instantiate(object[]? value)
        {
            if (value != null)
                return new(m_Type, value, false);
            return Instantiate();
        }

        public Variable? Instantiate(string value) => new(m_Type, m_Type.InternalParse(value), false);

        public override bool Equals(object? obj) => obj is Parameter parameter && m_Type == parameter.m_Type && m_ID == parameter.m_ID && m_DefaultValue == parameter.m_DefaultValue;
        public override int GetHashCode() => HashCode.Combine(m_Type, m_ID, m_DefaultValue);
        public string ToScriptString(ConversionTable conversionTable)
        {
            if (m_IsConst)
            {
                if (m_DefaultValue == null)
                    return string.Format("const {0} {1}", m_Type.GetFullName(conversionTable), conversionTable.GetName(m_ID));
                return string.Format("const {0} {1} = {2}", m_Type.GetFullName(conversionTable), conversionTable.GetName(m_ID), m_Type.ToString(m_DefaultValue));
            }
            if (m_DefaultValue == null)
                return string.Format("{0} {1}", m_Type.GetFullName(conversionTable), conversionTable.GetName(m_ID));
            return string.Format("{0} {1} = {2}", m_Type.GetFullName(conversionTable), conversionTable.GetName(m_ID), m_Type.ToString(m_DefaultValue));
        }
    }
}
