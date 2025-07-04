namespace CorpseLib.Scripts.Memory
{
    public class ArrayValue(IMemoryValue[] arrayValue) : IMemoryArray
    {
        private readonly IMemoryValue[] m_Value = arrayValue;

        public IMemoryValue this[int index]
        {
            get => m_Value[index];
            set => m_Value[index] = value;
        }

        public int Length => m_Value.Length;

        public bool Equals(IMemoryValue? other)
        {
            if (other is ArrayValue arrayValue)
                return EqualityComparer<IMemoryValue[]>.Default.Equals(m_Value, arrayValue.m_Value);
            return false;
        }

        public IMemoryValue Clone()
        {
            IMemoryValue[] clonedValues = new IMemoryValue[m_Value.Length];
            for (int i = 0; i < m_Value.Length; i++)
                clonedValues[i] = m_Value[i].Clone();
            return new ArrayValue(clonedValues);
        }
    }
}
