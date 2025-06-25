using System.Text;

namespace CorpseLib.Scripts.Parser.Instruction
{
    public class TokenReader
    {
        private readonly List<ExpressionToken> m_Tokens = [];
        private int m_Position = 0;

        public ExpressionToken? this[int offset]
        {
            get
            {
                if (m_Position + offset < m_Tokens.Count)
                    return m_Tokens[m_Position + offset];
                return null;
            }
        }

        public ExpressionToken? Current => m_Tokens.Count > m_Position ? m_Tokens[m_Position] : null;
        public bool HasNext => m_Position < m_Tokens.Count;

        public TokenReader(string input)
        {
            OperatorsContainer operatorsHelper = new();
            StringBuilder sb = new();
            int i = 0;
            while (i < input.Length)
            {
                char c = input[i];
                if (char.IsWhiteSpace(c))
                    i++;
                else if (char.IsLetter(c) || c == '_')
                {
                    sb.Clear();
                    sb.Append(c);
                    i++;
                    while (i < input.Length && (char.IsLetterOrDigit(input[i]) || input[i] == '_' ||
                        (sb.Length != 0 && (input[i] == '.' || input[i] == '<' || input[i] == '>'))))
                        sb.Append(input[i++]);
                    string token = sb.ToString();
                    if (token == "null")
                        m_Tokens.Add(new(sb.ToString(), null, false, true));
                    else
                        m_Tokens.Add(new(sb.ToString(), null, true, false));
                }
                else if (char.IsDigit(c))
                {
                    sb.Clear();
                    sb.Append(c);
                    i++;
                    while (i < input.Length && (char.IsDigit(input[i]) || input[i] == '.'))
                        sb.Append(input[i++]);
                    m_Tokens.Add(new(sb.ToString(), null, false, true));
                }
                else if (c == '.')
                {
                    sb.Clear();
                    sb.Append(c);
                    i++;
                    while (i < input.Length && char.IsDigit(input[i]))
                        sb.Append(input[i++]);
                    m_Tokens.Add(new(sb.ToString(), null, false, true));
                }
                else if (c == '"')
                {
                    sb.Clear();
                    sb.Append(c);
                    i++;
                    while (i < input.Length)
                    {
                        char ch = input[i++];
                        sb.Append(ch);
                        if (ch == '"' && sb[^2] != '\\')
                            break;
                    }
                    m_Tokens.Add(new(sb.ToString(), null, false, true));
                }
                else if (c == '\'')
                {
                    sb.Clear();
                    sb.Append(c);
                    i++;
                    while (i < input.Length)
                    {
                        char ch = input[i++];
                        sb.Append(ch);
                        if (ch == '\'' && sb[^2] != '\\')
                            break;
                    }
                    m_Tokens.Add(new(sb.ToString(), null, false, true));
                }
                else
                {
                    Operator? @operator = operatorsHelper.GetMatchingOperator(input, i, out int length);
                    if (@operator != null)
                    {
                        m_Tokens.Add(new(input.Substring(i, length), @operator, false, false));
                        i += length;
                    }
                    else
                    {
                        m_Tokens.Add(new(c.ToString(), null, false, false));
                        i++;
                    }
                }
            }
        }

        public void Pop() => m_Position++;
    }
}
