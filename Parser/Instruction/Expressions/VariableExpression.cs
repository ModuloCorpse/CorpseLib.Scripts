namespace CorpseLib.Scripts.Parser.Instruction.Expressions
{
    public class VariableExpression(int id) : AExpression(false)
    {
        public int ID = id;

        internal override void Dump(ConversionTable conversionTable, string str)
        {
            Console.Write(str);
            Console.Write("- Variable: ");
            Console.Write(conversionTable.GetName(ID));
            Console.WriteLine();
        }
    }
}
