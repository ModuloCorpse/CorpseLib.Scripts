namespace CorpseLib.Scripts.Parser.Instruction.Expressions
{
    public class AssignmentExpression(int[] variableID, AExpression value) : AExpression(true)
    {
        public int[] VariableID = variableID;
        public AExpression Value = value;

        internal override void Dump(ConversionTable conversionTable, string str)
        {
            Console.Write(str);
            Console.Write($"- Assignment: ");
            Console.WriteLine(string.Join('.', VariableID.Select(conversionTable.GetName)));
            Value.Dump(conversionTable, str + "   ");
        }
    }
}
