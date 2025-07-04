namespace CorpseLib.Scripts.Parser.Instruction.Expressions
{
    public class LengthExpression(AExpression target) : AExpression(false)
    {
        public AExpression Target = target;

        internal override void Dump(ConversionTable conversionTable, string str)
        {
            Console.Write(str);
            Console.WriteLine("- Length");
            Target.Dump(conversionTable, str + "   ");
        }
    }
}
