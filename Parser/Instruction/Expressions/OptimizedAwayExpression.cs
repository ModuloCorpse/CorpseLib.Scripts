namespace CorpseLib.Scripts.Parser.Instruction.Expressions
{
    internal class OptimizedAwayExpression : AExpression
    {
        internal override void Dump(ConversionTable conversionTable, string str)
        {
            Console.Write(str);
            Console.WriteLine("- Optimized Away");
        }
    }
}
