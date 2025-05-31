using System.Text;

namespace CorpseLib.Scripts
{
    public abstract class AComment
    {
        public abstract void Append(StringBuilder sb);

        public override string ToString()
        {
            StringBuilder sb = new();
            Append(sb);
            return sb.ToString();
        }
    }

    public class SingleLineComment(string comment) : AComment
    {
        private readonly string m_Comment = comment;
        public string Comment => m_Comment;
        public override void Append(StringBuilder sb) => sb.AppendLine($"// {m_Comment}");
    }

    public class MultiLineComment(string[] comments) : AComment
    {
        private readonly string[] m_Comments = comments;
        public string[] Comments => m_Comments;
        public override void Append(StringBuilder sb)
        {
            sb.Append("/*");
            if (m_Comments.Length != 0)
                sb.Append(' ');
            int i = 0;
            foreach (var comment in m_Comments)
            {
                if (i != 0)
                    sb.Append('\n');
                sb.Append(comment);
                ++i;
            }
            sb.AppendLine(" */");
        }
    }
}
