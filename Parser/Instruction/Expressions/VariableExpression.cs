namespace CorpseLib.Scripts.Parser.Instruction.Expressions
{
    public class VariableExpression(int[] ids) : AExpression
    {
        public int[] IDs = ids;

        internal override void Dump(ConversionTable conversionTable, string str)
        {
            Console.Write(str);
            Console.Write("- Variable: ");
            Console.Write(string.Join('.', IDs.Select(conversionTable.GetName)));
            Console.WriteLine();
        }
    }
}
