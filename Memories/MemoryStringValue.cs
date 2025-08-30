namespace CorpseLib.Scripts.Memories
{
    public class MemoryStringValue(string str) : AMemoryValue
    {
        private readonly char[] m_Value = str.ToCharArray();

        public AMemoryValue this[int index]
        {
            get => new MemoryLiteralValue(m_Value[index]);
            set {
                if (value is MemoryLiteralValue literalValue &&
                    Helper.ChangeType<char>(literalValue.Value, out var charValue))
                    m_Value[index] = charValue;
            }
        }

        public string Str => new(m_Value);
        public int Length => m_Value.Length;

        public override AMemoryValue Clone(Heap _) => new MemoryStringValue(new string(m_Value));

        public override bool Equals(AMemoryValue obj)
        {
            if (obj is MemoryStringValue stringValue)
                return EqualityComparer<char[]>.Default.Equals(m_Value, stringValue.m_Value);
            return false;
        }
    }
}
