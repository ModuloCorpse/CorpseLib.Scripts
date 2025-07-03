namespace CorpseLib.Scripts.Parser.Instruction.Expressions
{
    // TODO : Optimize away if value is variable or literal and if the variable is not used in the script
    public class CreateVariableExpression(TypeInfo typeName, int variableID, AExpression? value) : AExpression(true)
    {
        public TypeInfo TypeName = typeName;
        public int VariableID = variableID;
        public AExpression? Value = value;

        internal override void Dump(ConversionTable conversionTable, string str)
        {
            Console.Write(str);
            Console.Write($"- Create Variable: [Variable=");
            Console.Write(conversionTable.GetName(VariableID));
            ScriptBuilder sb = new(conversionTable);
            ScriptWriter.AppendTypeInfo(sb, TypeName);
            Console.Write($",Type={sb}");
            Console.WriteLine(']');
            Value?.Dump(conversionTable, str + "   ");
        }
    }
}
