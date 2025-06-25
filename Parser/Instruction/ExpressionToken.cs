namespace CorpseLib.Scripts.Parser.Instruction
{
    public class ExpressionToken(string token, Operator? @operator, bool isIdentifier, bool isLiteral)
    {
        private readonly Operator? m_Operator = @operator;
        private readonly string m_Token = token;
        private readonly bool m_IsIdentifier = isIdentifier;
        private readonly bool m_IsLiteral = isLiteral;

        public string Token => m_Token;
        public int Weight => m_Operator?.Weight ?? -1;
        public bool IsUnaryMutation => m_Operator?.IsMutation ?? false;
        public bool IsUnaryOperator => m_Operator?.IsUnary ?? false;
        public bool IsCompoundOperator => m_Operator?.IsCompoundable ?? false;
        public bool IsIdentifier => m_IsIdentifier;
        public bool IsLiteral => m_IsLiteral;
    }
}
