using Environment = CorpseLib.Scripts.Context.Environment;

namespace CorpseLib.Scripts.Parser
{
    public class ParserResult
    {
        private readonly Environment? m_Environment = null;
        private readonly ConversionTable m_ConversionTable = new();
        private readonly List<AComment> m_Comments = [];
        private readonly List<string> m_Warnings = [];
        private readonly string m_Error = string.Empty;
        private readonly bool m_HasErrors = false;

        public Environment? Environment => m_Environment;
        public ConversionTable ConversionTable => m_ConversionTable;
        public AComment[] Comments => [.. m_Comments];
        public string[] Warnings => [.. m_Warnings];
        public string Error => m_Error;
        public bool HasErrors => m_HasErrors;

        internal ParserResult(Environment environment, ConversionTable conversionTable, List<AComment> comments)
        {
            m_Environment = environment;
            m_ConversionTable = conversionTable;
            m_Comments = comments;
        }

        internal ParserResult(string error)
        {
            m_Error = error;
            m_HasErrors = true;
        }

        internal void AddWarnings(List<string> warnings) => m_Warnings.AddRange(warnings);
    }
}
