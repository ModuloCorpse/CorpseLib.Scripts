using CorpseLib.Scripts.Parameters;

namespace CorpseLib.Scripts.Parser.Instruction.Expressions
{
    public class LiteralExpression(ITemporaryValue value) : AExpression(false)
    {
        public ITemporaryValue Value = value;

        internal override void Dump(ConversionTable conversionTable, string str)
        {
            Console.Write(str);
            Console.Write("- Literal: ");
            ScriptBuilder sb = new(conversionTable);
            ScriptWriter.AppendParameterValue(sb, Value);
            Console.WriteLine(sb.ToString());
        }
    }
}
