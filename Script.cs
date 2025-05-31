namespace CorpseLib.Scripts
{
    public class Script : OldEnvironment
    {
        private readonly ConversionTable m_ConversionTable = new();

        public ConversionTable ConversionTable => m_ConversionTable;
    }
}
