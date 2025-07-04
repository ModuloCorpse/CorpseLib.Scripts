namespace CorpseLib.Scripts.Memory
{
    public class ObjectValue : IMemoryObject
    {
        private readonly SortedList<int, IMemoryValue> m_Value = [];

        public IMemoryValue[] Properties => [..m_Value.Values];

        private ObjectValue(SortedList<int, IMemoryValue> copy) => m_Value = copy;

        public ObjectValue() { }

        public void Set(int index, IMemoryValue value) => m_Value[index] = value;

        public bool Equals(IMemoryValue? other)
        {
            if (other is ObjectValue objectValue)
                return objectValue.m_Value.Equals(m_Value);
            return false;
        }

        public IMemoryValue Clone()
        {
            SortedList<int, IMemoryValue> clonedValues = [];
            foreach (var kvp in m_Value)
                clonedValues[kvp.Key] = kvp.Value.Clone();
            return new ObjectValue(clonedValues);
        }
    }
}
