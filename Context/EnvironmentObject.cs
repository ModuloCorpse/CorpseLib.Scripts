namespace CorpseLib.Scripts.Context
{
    public abstract class EnvironmentObject(int id, int[] tags, int[] comments)
    {
        private readonly int[] m_Comments = comments;
        private readonly int[] m_Tags = tags;
        private readonly int m_ID = id;

        public int[] Comments => m_Comments;
        public int[] Tags => m_Tags;
        public int ID => m_ID;

        public abstract bool IsValid();
    }
}
