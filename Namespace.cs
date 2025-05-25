namespace CorpseLib.Scripts
{
    public class Namespace(int id) : Environment
    {
        private readonly Namespace? m_Parent = null;
        private readonly int m_ID = id;

        public int ID => m_ID;
        public int[] IDS
        {
            get
            {
                List<int> ids = [];
                if (m_Parent != null)
                    ids.AddRange(m_Parent.IDS);
                ids.Add(m_ID);
                return [..ids];
            }
        }

        public Namespace(int id, Namespace? parent) : this(id) => m_Parent = parent;
    }
}
