namespace CorpseLib.Scripts.Parser.Instruction.Expressions
{
    //TODO : OPTIMIZE
    //* Convert this to a pre-unary mutation expression if it's a post-unary, the target is a variable and it's the only instruction
    //* Don't treat if the target is a literal and it's the only instruction
    public class UnaryMutationExpression(AExpression target, string op, bool isPrefix) : AExpression
    {
        public AExpression Target = target;
        public bool IsIncrement = (op == "++");
        public bool IsPrefix = isPrefix;

        internal override void Dump(ConversionTable conversionTable, string str)
        {
            Console.Write(str);
            Console.Write("- ");
            Console.Write(IsPrefix ? "Pre" : "Post");
            Console.Write(" Unary Mutation: ");
            Console.WriteLine(IsIncrement ? "++" : "--");
            Target.Dump(conversionTable, str + "   ");
        }
    }
}
