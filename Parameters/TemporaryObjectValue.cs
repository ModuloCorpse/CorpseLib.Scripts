using CorpseLib.Scripts.Memories;

namespace CorpseLib.Scripts.Parameters
{
    public class TemporaryObjectValue(ITemporaryValue[] value) : ITemporaryValue
    {
        private readonly ITemporaryValue[] m_Value = value;

        public ITemporaryValue[] Properties => m_Value;

        public AMemoryValue Allocate(Heap heap)
        {
            int[] clonedValues = new int[m_Value.Length];
            for (int i = 0; i < m_Value.Length; i++)
                clonedValues[i] = m_Value[i].Allocate(heap).Address;
            MemoryObjectValue memoryObjectValue = new(clonedValues);
            heap.Allocate(memoryObjectValue);
            return memoryObjectValue;
        }

        public ITemporaryValue Clone()
        {
            ITemporaryValue[] clonedValues = new ITemporaryValue[m_Value.Length];
            for (int i = 0; i < m_Value.Length; i++)
                clonedValues[i] = m_Value[i].Clone();
            return new TemporaryObjectValue(clonedValues);
        }
    }
}
