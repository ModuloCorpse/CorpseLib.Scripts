namespace CorpseLib.Scripts.Parser.Instruction
{
    public class ExpressionToken(string token)
    {
        private readonly string m_Token = token;
        private readonly bool m_IsUnaryMutation = token == "++" || token == "--";
        private readonly bool m_IsUnaryOperator = token == "-" || token == "+" || token == "!" || token == "~";
        private readonly bool m_IsCompoundOperator = token == "+=" || token == "-=" || token == "*=" || token == "/=" || token == "%=" || token == "&=" || token == "|=" || token == "^=" || token == "<<=" || token == ">>=";
        private readonly bool m_IsIdentifier = char.IsLetter(token[0]) || token[0] == '_';
        private readonly bool m_IsLiteral = char.IsDigit(token[0]) || token[0] == '.' || token[0] == '\'' || token[0] == '"' || token == "null";

        public string Token => m_Token;
        public bool IsUnaryMutation => m_IsUnaryMutation;
        public bool IsUnaryOperator => m_IsUnaryOperator;
        public bool IsCompoundOperator => m_IsCompoundOperator;
        public bool IsIdentifier => m_IsIdentifier;
        public bool IsLiteral => m_IsLiteral;
    }
}
