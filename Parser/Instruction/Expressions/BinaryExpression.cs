namespace CorpseLib.Scripts.Parser.Instruction.Expressions
{
    public class BinaryExpression(Operator op, AExpression left, AExpression right) : AExpression
    {
        public Operator Operator = op;
        public AExpression Left = left;
        public AExpression Right = right;

        internal override void Dump(ConversionTable conversionTable, string str)
        {
            Console.Write(str);
            Console.Write("- Binary: ");
            Console.WriteLine(Operator.OperatorString);
            Left.Dump(conversionTable, str + "   ");
            Right.Dump(conversionTable, str + "   ");
        }
    }
}
