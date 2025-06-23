namespace CorpseLib.Scripts.Parser.Instruction
{
    public class CompoundAssignmentExpression(int[] variableID, string op, AExpression right) : AExpression
    {
        public int[] VariableID = variableID;
        public string Operator = op;
        public AExpression Right = right;

        internal override void Dump(ConversionTable conversionTable, string str)
        {
            Console.Write(str);
            Console.Write($"- CompoundAssignment [Variable=");
            Console.Write(string.Join('.', VariableID.Select(conversionTable.GetName)));
            Console.WriteLine($",Operator=\"{Operator}=\"]");
            Right.Dump(conversionTable, str + "   ");
        }
    }
}
