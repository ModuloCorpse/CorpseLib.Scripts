namespace CorpseLib.Scripts.Memories
{
    public class MemoryArrayValue(int[] arrayValue) : AMemoryValue
    {
        private readonly int[] m_Value = arrayValue;

        public int this[int index]
        {
            get => m_Value[index];
            set => m_Value[index] = value;
        }

        public int Length => m_Value.Length;

        public override bool Equals(AMemoryValue? other)
        {
            if (other is MemoryArrayValue arrayValue)
                return EqualityComparer<int[]>.Default.Equals(m_Value, arrayValue.m_Value);
            return false;
        }

        public override AMemoryValue Clone(Heap heap)
        {
            int[] clonedValues = new int[m_Value.Length];
            for (int i = 0; i < m_Value.Length; i++)
                clonedValues[i] = heap.Duplicate(m_Value[i]);
            return new MemoryArrayValue(clonedValues);
        }
    }
}
