namespace CorpseLib.Scripts.Parser.Instruction
{
    public abstract class AExpression
    {
        public void Dump(ConversionTable conversionTable) => Dump(conversionTable, string.Empty);
        internal abstract void Dump(ConversionTable conversionTable, string str);
    }
}
