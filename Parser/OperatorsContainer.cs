namespace CorpseLib.Scripts.Parser
{
    public class OperatorsContainer
    {
        private readonly List<Operator> m_Operators = [];

        public OperatorsContainer()
        {
            //Assignment operator
            AddOperator("=", 0, false, false, false);

            //Mutation operators
            AddOperator("++", 0, true, false, false);
            AddOperator("--", 0, true, false, false);

            //Compoundable operators
            AddOperator("+", 5, false, true, false);
            AddOperator("-", 5, false, true, true);
            AddOperator("*", 6, false, true, false);
            AddOperator("/", 6, false, true, false);
            AddOperator("%", 6, false, true, false);
            AddOperator("|", 7, false, true, false);
            AddOperator("&", 7, false, true, false);
            AddOperator("<<", 7, false, true, false);
            AddOperator(">>", 7, false, true, false);

            //Comparison operators
            AddOperator("==", 3, false, false, false);
            AddOperator("!=", 3, false, false, false);
            AddOperator("||", 1, false, false, false);
            AddOperator("&&", 2, false, false, false);
            AddOperator("<", 4, false, false, false);
            AddOperator("<=", 4, false, false, false);
            AddOperator(">", 4, false, false, false);
            AddOperator(">=", 4, false, false, false);

            //Unary operators
            AddOperator("!", 0, false, false, true);

            //Ternary operators
            AddOperator("?", 0, false, false, false);
            AddOperator(":", 0, false, false, false);
        }

        public void AddOperator(string @operator, int weight, bool isMutation, bool isCompoundable, bool isUnary) => m_Operators.Add(new Operator(@operator, weight, isMutation, isCompoundable, isUnary));

        public Operator? GetMatchingOperator(string op, int offset, out int length)
        {
            Operator? matchingOperator = null;
            length = 0;
            foreach (var oper in m_Operators)
            {
                int n = oper.StartWith(op, offset);
                if (n > length)
                {
                    matchingOperator = oper;
                    length = n;
                }
            }
            return matchingOperator;
        }
    }
}
