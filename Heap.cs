using System.Collections;

namespace CorpseLib.Scripts
{
    public class Heap : IEnumerable<KeyValuePair<int, object[]>>
    {
        public static object[] CloneObjects(object[] toClone)
        {
            List<object> clonedObjects = [];
            foreach (object item in toClone)
            {
                if (item is List<object[]> list)
                {
                    List<object[]> clonedList = [];
                    foreach (var arrayItem in list)
                        clonedList.Add(CloneObjects(arrayItem));
                    clonedObjects.Add(clonedList);
                }
                else if (item is object[] array)
                    clonedObjects.Add(CloneObjects(array));
                else
                {
                    object clone = item;
                    clonedObjects.Add(clone);
                }
            }
            return [..clonedObjects];
        }

        private readonly List<int> m_FreeAdress = [];
        private readonly SortedList<int, object[]?> m_Memory = [];
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

        public int Allocate(object[] objects)
        {
            int index = GetNextFreeIndex();
            m_Memory[index] = objects;
            return index;
        }

        public int Duplicate(int index)
        {
            object[]? existing = Get(index);
            if (existing == null)
                return -1;
            object[] clone = CloneObjects(existing);
            return Allocate(clone);
        }

        public object[]? Get(int index)
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

        public IEnumerator<KeyValuePair<int, object[]>> GetEnumerator() => ((IEnumerable<KeyValuePair<int, object[]>>)m_Memory).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)m_Memory).GetEnumerator();
    }
}
