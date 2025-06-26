namespace CorpseLib.Scripts.Parser.Instruction.Expressions
{
    public class UnaryExpression(Operator op, AExpression target) : AExpression(target.HasSideEffects)
    {
        public Operator Operator = op;
        public AExpression Target = target;

        internal override void Dump(ConversionTable conversionTable, string str)
        {
            Console.Write(str);
            Console.Write("- Unary: ");
            Console.WriteLine(Operator.OperatorString);
            Target.Dump(conversionTable, str + "   ");
        }
    }
}
