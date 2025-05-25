using System.Collections;

namespace CorpseLib.Scripts
{
    public class ConversionTable : IEnumerable<KeyValuePair<int, string>>
    {
        private readonly Dictionary<int, string> m_Table = [];

        internal static int ConvertStr(string str) => str.GetHashCode();

        public int PushName(string name)
        {
            int id = ConvertStr(name);
            m_Table[id] = name;
            return id;
        }

        public string GetName(int id)
        {
            if (m_Table.TryGetValue(id, out var name))
                return name;
            return string.Empty;
        }

        public IEnumerator<KeyValuePair<int, string>> GetEnumerator() => m_Table.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)m_Table).GetEnumerator();
    }
}
