namespace CorpseLib.Scripts.Parser.Instruction.Expressions
{
    public class LiteralExpression(object[] value) : AExpression
    {
        public object[] Value = value;

        internal override void Dump(ConversionTable conversionTable, string str)
        {
            Console.Write(str);
            Console.Write("- Literal: ");
            ScriptBuilder sb = new(conversionTable);
            ScriptWriter.AppendAnonymousValue(sb, Value);
            Console.WriteLine(sb.ToString());
        }
    }
}
