namespace CorpseLib.Scripts.Memories
{
    public class StringValue(string str) : IMemoryArray
    {
        private readonly char[] m_Value = str.ToCharArray();

        public IMemoryValue this[int index]
        {
            get => new LiteralValue(m_Value[index]);
            set {
                if (value is LiteralValue literalValue &&
                    Helper.ChangeType<char>(literalValue.Value, out var charValue))
                    m_Value[index] = charValue;
            }
        }

        public string Str => new(m_Value);
        public int Length => m_Value.Length;

        public IMemoryValue Clone() => new StringValue(new string(m_Value));

        public bool Equals(IMemoryValue obj)
        {
            if (obj is StringValue stringValue)
                return EqualityComparer<char[]>.Default.Equals(m_Value, stringValue.m_Value);
            return false;
        }
    }
}
