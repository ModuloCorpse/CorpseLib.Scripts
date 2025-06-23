namespace CorpseLib.Scripts.Parser.Instruction
{
    public class BinaryExpression(string op, AExpression left, AExpression right) : AExpression
    {
        public string Operator = op;
        public AExpression Left = left, Right = right;

        internal override void Dump(ConversionTable conversionTable, string str)
        {
            Console.Write(str);
            Console.Write("- Binary: ");
            Console.WriteLine(Operator);
            Left.Dump(conversionTable, str + "   ");
            Right.Dump(conversionTable, str + "   ");
        }
    }
}
