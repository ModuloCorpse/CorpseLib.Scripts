using System.Collections;

namespace CorpseLib.Scripts.Memories
{
    public class Heap : IEnumerable<Heap.MemorySlot>
    {
        public class MemorySlot(AMemoryValue value, int address)
        {
            private readonly AMemoryValue m_Value = value;
            private readonly int m_Address = address;
            private int m_Reference = 1;

            public AMemoryValue Value => m_Value;
            public int Address => m_Address;
            public int ReferenceCount => m_Reference;

            public void IncreaseReference() => m_Reference++;
            public void DecreaseReference() => m_Reference--;
        }

        private readonly List<int> m_TemporaryAdresses = [];
        private readonly List<int> m_FreeAdresses = [];
        private readonly List<MemorySlot> m_Memory = [];
        private int m_Adress = 0;

        private int InterpolationSearchIndex(int key, out bool found)
        {
            int low = 0;
            int high = m_Memory.Count - 1;
            while (low <= high)
            {
                int lowKey = m_Memory[low].Address;
                int highKey = m_Memory[high].Address;
                if (key < lowKey)
                {
                    found = false;
                    return low;
                }
                if (key > highKey)
                {
                    found = false;
                    return high + 1;
                }
                if (highKey == lowKey)
                {
                    if (lowKey == key)
                    {
                        found = true;
                        return low;
                    }
                    found = false;
                    return key < lowKey ? low : low + 1;
                }
                int pos = low + (int)(((long)(key - lowKey) * (high - low)) / (highKey - lowKey));
                int posKey = m_Memory[pos].Address;
                if (posKey == key)
                {
                    found = true;
                    return pos;
                }
                if (posKey < key)
                    low = pos + 1;
                else
                    high = pos - 1;
            }
            found = false;
            return low;
        }

        public int Allocate(AMemoryValue value)
        {
            if (value.Address != -1)
            {
                int slotIndex = InterpolationSearchIndex(value.Address, out bool slotFound);
                if (slotFound)
                    m_Memory[slotIndex].IncreaseReference();
                return value.Address;
            }
            int address;
            if (m_FreeAdresses.Count > 0)
            {
                address = m_FreeAdresses[0];
                m_FreeAdresses.RemoveAt(0);
            }
            else
                address = m_Adress++;
            int index = InterpolationSearchIndex(address, out bool found);
            if (!found)
            {
                m_Memory.Insert(index, new(value, address));
                value.SetAddress(address);
            }
            return index;
        }

        public int Duplicate(int index)
        {
            AMemoryValue? existing = Get(index);
            if (existing == null)
                return -1;
            return Allocate(existing.Clone(this));
        }

        public AMemoryValue? Get(int address)
        {
            int index = InterpolationSearchIndex(address, out bool found);
            return found ? m_Memory[index].Value : null;
        }

        public void Free(int address)
        {
            if (address >= 0 && address < m_Adress)
            {
                int index = InterpolationSearchIndex(address, out bool found);
                if (found)
                {
                    MemorySlot slot = m_Memory[index];
                    slot.DecreaseReference();
                    if (slot.ReferenceCount == 0)
                    {
                        m_Memory.RemoveAt(index);
                        m_FreeAdresses.Add(index);
                        if (slot.Value is MemoryArrayValue arrayValue)
                        {
                            for (int i = 0; i != arrayValue.Length; i++)
                                Free(arrayValue[i]);
                        }
                        else if (slot.Value is MemoryObjectValue objectValue)
                        {
                            foreach (int propertyAddress in objectValue.Properties)
                                Free(propertyAddress);
                        }
                    }
                }
            }
        }

        public IEnumerator<MemorySlot> GetEnumerator() => ((IEnumerable<MemorySlot>)m_Memory).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)m_Memory).GetEnumerator();
    }
}
