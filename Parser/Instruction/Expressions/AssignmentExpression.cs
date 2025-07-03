namespace CorpseLib.Scripts.Parser.Instruction.Expressions
{
    public class AssignmentExpression(AExpression variable, AExpression value) : AExpression(true)
    {
        public AExpression Variable = variable;
        public AExpression Value = value;

        internal override void Dump(ConversionTable conversionTable, string str)
        {
            Console.Write(str);
            Console.WriteLine($"- Assignment");
            Variable.Dump(conversionTable, str + "   ");
            Value.Dump(conversionTable, str + "   ");
        }
    }
}
