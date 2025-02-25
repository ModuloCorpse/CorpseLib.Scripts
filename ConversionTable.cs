namespace CorpseLib.Scripts
{
    public class ConversionTable
    {
        private readonly Dictionary<int, string> m_Table = [];

        public int PushName(string name)
        {
            int id = name.GetHashCode();
            m_Table[id] = name;
            return id;
        }

        public string GetName(int id)
        {
            if (m_Table.TryGetValue(id, out var name))
                return name;
            return string.Empty;
        }
    }
}
