namespace CorpseLib.Scripts.Parser.Instruction
{
    public class AssignmentExpression(string? typeName, int[] variableID, AExpression value) : AExpression
    {
        //TODO : Switch to ParsedType
        public string? TypeName = typeName;
        public int[] VariableID = variableID;
        public AExpression Value = value;

        internal override void Dump(ConversionTable conversionTable, string str)
        {
            Console.Write(str);
            Console.Write($"- Assignment: [Variable=");
            Console.Write(string.Join('.', VariableID.Select(conversionTable.GetName)));
            Console.WriteLine($",Type={TypeName}]");
            Value.Dump(conversionTable, str + "   ");
        }
    }
}
