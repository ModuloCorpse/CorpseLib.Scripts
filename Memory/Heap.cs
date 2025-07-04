using System.Collections;

namespace CorpseLib.Scripts.Memory
{
    public class Heap : IEnumerable<KeyValuePair<int, IMemoryValue>>
    {
        public static readonly IMemoryValue NULL = new NullValue();
        private readonly List<int> m_FreeAdress = [];
        private readonly SortedList<int, IMemoryValue> m_Memory = [];
        private int m_Adress = 0;

        private int GetNextFreeIndex()
        {
            if (m_FreeAdress.Count > 0)
            {
                int index = m_FreeAdress[0];
                m_FreeAdress.RemoveAt(0);
                return index;
            }
            return m_Adress++;
        }

        public int Allocate(IMemoryValue objects)
        {
            int index = GetNextFreeIndex();
            m_Memory[index] = objects;
            return index;
        }

        public int Duplicate(int index)
        {
            IMemoryValue? existing = Get(index);
            if (existing == null)
                return -1;
            return Allocate(existing.Clone());
        }

        public IMemoryValue? Get(int index)
        {
            if (index < 0 || index >= m_Adress)
                return null;
            return m_Memory[index];
        }

        public void Free(int index)
        {
            if (index >= 0 && index < m_Adress)
            {
                if (m_Memory.Remove(index))
                    m_FreeAdress.Add(index);
            }
        }

        public IEnumerator<KeyValuePair<int, IMemoryValue>> GetEnumerator() => ((IEnumerable<KeyValuePair<int, IMemoryValue>>)m_Memory).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)m_Memory).GetEnumerator();
    }
}
