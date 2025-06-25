namespace CorpseLib.Scripts.Parser
{
    public class Operator(string @operator, int weight, bool isMutation, bool isCompoundable, bool isUnary)
    {
        private readonly string m_Operator = @operator;
        /// The weight of the operator, used to determine precedence in expressions
        private readonly int m_Weight = weight;
        /// Mutation operations are unary operations that modify the value of a variable, like ++ or --
        private readonly bool m_IsMutation = isMutation;
        /// Compound operations are binary operations that can be used in a compound assignment, like += or -=
        private readonly bool m_IsCompoundable = isCompoundable;
        /// Unary operations that can be used ion one value, like ! or -
        private readonly bool m_IsUnary = isUnary;

        public string OperatorString => m_Operator;
        public int Weight => m_Weight;
        public bool IsMutation => m_IsMutation;
        public bool IsCompoundable => m_IsCompoundable;
        public bool IsUnary => m_IsUnary;

        public int StartWith(string op, int offset)
        {
            int i = offset;
            int n = 0;
            while (n != m_Operator.Length)
            {
                if (op[i] != m_Operator[n])
                    return -1;
                i++;
                n++;
                if (i == op.Length && n != m_Operator.Length)
                    return -1;
            }
            if (m_IsCompoundable && op[i] == '=')
                return n + 1;
            return n;
        }
    }
}
