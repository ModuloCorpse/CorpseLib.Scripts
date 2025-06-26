namespace CorpseLib.Scripts.Parser.Instruction.Expressions
{
    public class UnaryExpression(Operator op, AExpression operand) : AExpression
    {
        public Operator Operator = op;
        public AExpression Operand = operand;

        internal override void Dump(ConversionTable conversionTable, string str)
        {
            Console.Write(str);
            Console.Write("- Unary: ");
            Console.WriteLine(Operator.OperatorString);
            Operand.Dump(conversionTable, str + "   ");
        }
    }
}
