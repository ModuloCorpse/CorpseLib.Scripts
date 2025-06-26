namespace CorpseLib.Scripts.Parser.Instruction.Expressions
{
    public class FunctionCallExpression(int[] functionID, List<AExpression> args) : AExpression(true)
    {
        public int[] FunctionID = functionID;
        public List<AExpression> Arguments = args;

        internal override void Dump(ConversionTable conversionTable, string str)
        {
            Console.Write(str);
            Console.Write("- Function Call: ");
            Console.Write(string.Join("::", FunctionID.Select(conversionTable.GetName)));
            Console.WriteLine();
            foreach (AExpression arg in Arguments)
                arg.Dump(conversionTable, str + "   ");
        }
    }
}
