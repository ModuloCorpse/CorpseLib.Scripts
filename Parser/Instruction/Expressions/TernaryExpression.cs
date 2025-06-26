namespace CorpseLib.Scripts.Parser.Instruction.Expressions
{
    //TODO : Optimize if condition is always true or false by returning directly true or false expression
    public class TernaryExpression(AExpression condition, AExpression @true, AExpression @false) : AExpression(condition.HasSideEffects || @true.HasSideEffects || @false.HasSideEffects)
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
