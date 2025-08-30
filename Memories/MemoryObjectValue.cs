namespace CorpseLib.Scripts.Memories
{
    public class MemoryObjectValue(int[] properties) : AMemoryValue
    {
        private readonly int[] m_Properties = properties;

        public int[] Properties => m_Properties;

        public override bool Equals(AMemoryValue? other)
        {
            if (other is MemoryObjectValue objectValue)
                return objectValue.m_Properties.Equals(m_Properties);
            return false;
        }

        public override AMemoryValue Clone(Heap heap)
        {
            int[] clonedValues = new int[m_Properties.Length];
            for (int i = 0; i < m_Properties.Length; i++)
                clonedValues[i] = heap.Duplicate(m_Properties[i]);
            return new MemoryObjectValue(clonedValues);
        }
    }
}
