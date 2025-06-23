namespace CorpseLib.Scripts.Parser.Instruction
{
    public class LiteralExpression(string value) : AExpression
    {
        public string Value = value;

        internal override void Dump(ConversionTable conversionTable, string str)
        {
            Console.Write(str);
            Console.Write("- Literal: ");
            Console.WriteLine(Value);
        }
    }
}
