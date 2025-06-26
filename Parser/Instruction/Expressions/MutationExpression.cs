namespace CorpseLib.Scripts.Parser.Instruction.Expressions
{
    //TODO : OPTIMIZE
    //* Don't treat if the target is a literal and it's the only instruction
    //* If the target is a literal change literal value by the value it will get
    public class MutationExpression(AExpression target, Operator op, bool isPrefix) : AExpression
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
