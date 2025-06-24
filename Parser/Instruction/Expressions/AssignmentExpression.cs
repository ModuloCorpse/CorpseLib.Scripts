namespace CorpseLib.Scripts.Parser.Instruction.Expressions
{
    public class AssignmentExpression(TypeInfo? typeName, int[] variableID, AExpression value) : AExpression
    {
        public TypeInfo? TypeName = typeName;
        public int[] VariableID = variableID;
        public AExpression Value = value;

        internal override void Dump(ConversionTable conversionTable, string str)
        {
            Console.Write(str);
            Console.Write($"- Assignment: [Variable=");
            Console.Write(string.Join('.', VariableID.Select(conversionTable.GetName)));
            if (TypeName != null)
            {
                ScriptBuilder sb = new(conversionTable);
                ScriptWriter.AppendTypeInfo(sb, TypeName);
                Console.Write($",Type={TypeName}");
            }
            Console.WriteLine(']');
            Value.Dump(conversionTable, str + "   ");
        }
    }
}
