namespace CorpseLib.Scripts
{
    public class ScriptBuilder(ConversionTable conversionTable) : FormattedStringBuilder<StringBuilderFormat>()
    {
        private readonly ConversionTable m_ConversionTable = conversionTable;
        private readonly List<AComment> m_Comments = [];

        public void SetComments(IEnumerable<AComment> comments)
        {
            m_Comments.Clear();
            m_Comments.AddRange(comments);
        }

        public void AppendComment(int commentID)
        {
            if (commentID >= 0 && commentID < m_Comments.Count)
            {
                AComment comment = m_Comments[commentID];
                comment.Append(this);
            }
        }

        public void AppendID(int id) => Append(m_ConversionTable.GetName(id));

        public void OpenScope()
        {
            m_Builder.Append('{');
            Indent();
            AppendLine();
        }

        public void CloseScope()
        {
            Unindent();
            AppendLine();
            m_Builder.Append('}');
        }
    }
}
