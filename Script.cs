namespace CorpseLib.Scripts
{
    public class Script : Namespace
    {
        private readonly ConversionTable m_ConversionTable = new();

        public ConversionTable ConversionTable => m_ConversionTable;

        public Script() : base(0) { }
    }
}
