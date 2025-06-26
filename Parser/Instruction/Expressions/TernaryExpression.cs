namespace CorpseLib.Scripts.Parser.Instruction.Expressions
{
    public class TernaryExpression(AExpression condition, AExpression @true, AExpression @false) : AExpression
    {
        public AExpression Condition = condition;
        public AExpression True = @true;
        public AExpression False = @false;

        internal override void Dump(ConversionTable conversionTable, string str)
        {
            Console.Write(str);
            Console.WriteLine("- TernaryExpression");
            Condition.Dump(conversionTable, str + "   ");
            True.Dump(conversionTable, str + "   ");
            False.Dump(conversionTable, str + "   ");
        }
    }
}
