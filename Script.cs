namespace CorpseLib.Scripts
{
    public class Script : Environment
    {
        private readonly ConversionTable m_ConversionTable = new();

        public ConversionTable ConversionTable => m_ConversionTable;
    }
}
