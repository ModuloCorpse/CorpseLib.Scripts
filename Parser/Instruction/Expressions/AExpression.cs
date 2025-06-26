namespace CorpseLib.Scripts.Parser.Instruction.Expressions
{
    public abstract class AExpression(bool hasSideEffect)
    {
        private readonly bool m_HasSideEffects = hasSideEffect;

        public bool HasSideEffects => m_HasSideEffects;

        public void Dump(ConversionTable conversionTable) => Dump(conversionTable, string.Empty);
        internal abstract void Dump(ConversionTable conversionTable, string str);
    }
}
