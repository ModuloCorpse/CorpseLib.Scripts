using CorpseLib.Scripts.Memories;

namespace CorpseLib.Scripts.Parser.Instruction.Expressions
{
    public class LiteralExpression(IMemoryValue value) : AExpression(false)
    {
        public IMemoryValue Value = value;

        internal override void Dump(ConversionTable conversionTable, string str)
        {
            Console.Write(str);
            Console.Write("- Literal: ");
            ScriptBuilder sb = new(conversionTable);
            ScriptWriter.AppendMemoryValue(sb, Value);
            Console.WriteLine(sb.ToString());
        }
    }
}
