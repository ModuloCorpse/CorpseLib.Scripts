namespace CorpseLib.Scripts.Parser.Instruction.Expressions
{
    public class ArrayExpression(AExpression target, AExpression index) : AExpression(target.HasSideEffects || index.HasSideEffects)
    {
        public AExpression TargetArray = target;
        public AExpression Index = index;

        internal override void Dump(ConversionTable conversionTable, string str)
        {
            Console.Write(str);
            Console.WriteLine("- Array");
            TargetArray.Dump(conversionTable, str + "   ");
            Index.Dump(conversionTable, str + "   ");
        }
    }
}
