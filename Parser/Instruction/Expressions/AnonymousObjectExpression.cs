namespace CorpseLib.Scripts.Parser.Instruction.Expressions
{
    public class AnonymousObjectExpression(List<AExpression> parameters, bool isArray) : AExpression(true)
    {
        public List<AExpression> Parameters = parameters;
        public bool IsArray = isArray;

        internal override void Dump(ConversionTable conversionTable, string str)
        {
            Console.Write(str);
            Console.Write("- Anonymous ");
            Console.WriteLine(IsArray ? "array" : "object");
            foreach (AExpression param in Parameters)
                param.Dump(conversionTable, str + "   ");
        }
    }
}
