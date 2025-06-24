using CorpseLib.Scripts.Context;
using CorpseLib.Scripts.Instructions;
using CorpseLib.Scripts.Type;
using Environment = CorpseLib.Scripts.Context.Environment;

namespace CorpseLib.Scripts.Parser
{
    public class ParsingContext
    {
        private readonly Environment m_Environment = new();
        private readonly ConversionTable m_ConversionTable = new();
        private readonly List<int> m_Namespaces = [];
        private readonly List<string> m_Warnings = [];
        private string m_Error = string.Empty;
        private bool m_HasErrors = false;

        public ConversionTable ConversionTable => m_ConversionTable;
        public int[] Namespaces => [.. m_Namespaces];
        public string[] Warnings => [..m_Warnings];
        public string Error => m_Error;
        public bool HasErrors => m_HasErrors;

        public void RegisterWarning(string warning, string description) => m_Warnings.Add($"WARNING : {warning} : {description}");

        public void RegisterError(string error, string description)
        {
            m_Error = $"ERROR : {error} : {description}";
            m_HasErrors = true;
        }

        public void PushInstruction(AInstruction instruction, int[] namespaces, int[] tags, int[] comments) => m_Environment.AddInstruction(instruction, namespaces, tags, comments);

        public void PushTypeDefinition(TypeDefinition typeDefinition, int[] tags, int[] comments) => m_Environment.AddTypeDefinition(typeDefinition, tags, comments);

        public void PushType(ATypeInstance type) => m_Environment.AddType(type);

        public int PushNamespace(string @namespace, int[] tags, int[] comments)
        {
            int namespaceID = m_ConversionTable.PushName(@namespace);
            m_Environment.AddNamespace([..m_Namespaces], namespaceID, tags, comments);
            m_Namespaces.Add(namespaceID);
            return namespaceID;
        }

        public void PopNamespace() => m_Namespaces.RemoveAt(m_Namespaces.Count - 1);

        public int PushName(string name) => m_ConversionTable.PushName(name);

        public ParserResult CreateParserResult(Script script, List<AComment> comments)
        {
            if (m_HasErrors)
                return new ParserResult(m_Error);
            return new ParserResult(script, m_Environment, m_ConversionTable, comments);
        }
    }
}
