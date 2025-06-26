namespace CorpseLib.Scripts.Parser.Instruction.Expressions
{
    public class MutationExpression(AExpression target, Operator op, bool isPrefix) : AExpression(target is not LiteralExpression)
    {
        public AExpression Target = target;
        public Operator Operator = op;
        public bool IsPrefix = isPrefix;

        internal override void Dump(ConversionTable conversionTable, string str)
        {
            Console.Write(str);
            Console.Write("- ");
            Console.Write(IsPrefix ? "Pre" : "Post");
            Console.Write(" Mutation: ");
            Console.WriteLine(Operator.OperatorString);
            Target.Dump(conversionTable, str + "   ");
        }
    }
}
