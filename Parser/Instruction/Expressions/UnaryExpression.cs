namespace CorpseLib.Scripts.Parser.Instruction.Expressions
{
    public class UnaryExpression(string op, AExpression operand) : AExpression
    {
        public string Operator = op;
        public AExpression Operand = operand;

        internal override void Dump(ConversionTable conversionTable, string str)
        {
            Console.Write(str);
            Console.Write("- Unary: ");
            Console.WriteLine(Operator);
            Operand.Dump(conversionTable, str + "   ");
        }
    }
}
