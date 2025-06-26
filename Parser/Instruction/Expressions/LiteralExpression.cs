namespace CorpseLib.Scripts.Parser.Instruction.Expressions
{
    public class LiteralExpression(object[] value) : AExpression
    {
        public object[] Value = value;

        internal override void Dump(ConversionTable conversionTable, string str)
        {
            Console.Write(str);
            Console.Write("- Literal: ");
            if (Value.Length == 0)
                Console.WriteLine("null");
            else if (Value.Length == 1 && Value[0] is not string)
                Console.WriteLine($"{Value[0]} ({Value[0].GetType().Name})");
            else if (Value.Length == 1 && Value[0] is string strValue)
                Console.WriteLine($"\"{strValue}\" (String)");
            else
            {
                ScriptBuilder sb = new(conversionTable);
                ScriptWriter.AppendAnonymousValue(sb, Value);
                Console.WriteLine(sb.ToString());
            }
        }
    }
}
