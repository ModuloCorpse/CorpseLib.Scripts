using CorpseLib.Scripts.Memories;

namespace CorpseLib.Scripts.Parameters
{
    public class TemporaryArrayValue(ITemporaryValue[] arrayValue) : ITemporaryValue
    {
        private readonly ITemporaryValue[] m_Value = arrayValue;

        public ITemporaryValue this[int index]
        {
            get => m_Value[index];
            set => m_Value[index] = value;
        }

        public int Length => m_Value.Length;

        public ITemporaryValue Clone()
        {
            ITemporaryValue[] clonedValues = new ITemporaryValue[m_Value.Length];
            for (int i = 0; i < m_Value.Length; i++)
                clonedValues[i] = m_Value[i].Clone();
            return new TemporaryArrayValue(clonedValues);
        }

        public AMemoryValue Allocate(Heap heap)
        {
            int[] clonedValues = new int[m_Value.Length];
            for (int i = 0; i < m_Value.Length; i++)
                clonedValues[i] = m_Value[i].Allocate(heap).Address;
            MemoryArrayValue memoryArrayValue = new(clonedValues);
            heap.Allocate(memoryArrayValue);
            return memoryArrayValue;
        }
    }
}
