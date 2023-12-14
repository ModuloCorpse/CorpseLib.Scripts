using CorpseLib.Scripts.Type;

namespace CorpseLib.Scripts
{
    public class Variable(ATypeInstance type)
    {
        private readonly ATypeInstance m_Type = type;
        private object[]? m_Value = null;
        private bool m_IsDefault = false;

        public ATypeInstance Type => m_Type;
        public object[]? Values => m_Value;
        public object? Value => (m_Value != null && m_Value.Length == 1) ? m_Value[0] : null;

        public Variable(ATypeInstance type, object[]? value, bool isDefault) : this(type)
        {
            m_Value = value;
            m_IsDefault = isDefault;
        }

        internal bool IsDefault()
        {
            if (!m_IsDefault)
                return false;
            else
            {
                if (m_Value != null)
                {
                    foreach (object val in m_Value)
                    {
                        if (val is Variable variable && !variable.IsDefault())
                        {
                            m_IsDefault = false;
                            return false;
                        }
                    }
                }
                return true;
            }
        }

        public void SetValue(object[]? value)
        {
            m_Value = value;
            m_IsDefault = false;
        }

        public void SetValue(object? value)
        {
            m_Value = (value != null) ? [value] : null;
            m_IsDefault = false;
        }

        public Variable Duplicate()
        {
            if (m_Value == null)
                return new(m_Type, null, m_IsDefault);
            List<object> values = [];
            foreach (object val in m_Value)
            {
                if (val is Variable variable)
                    values.Add(variable.Duplicate());
                else
                    values.Add(val);
            }
            return new(m_Type, [.. values], m_IsDefault);
        }
    }
}
