using CorpseLib.Scripts.Context;
using CorpseLib.Scripts.Instructions;
using CorpseLib.Scripts.Parameters;
using CorpseLib.Scripts.Type;
using Environment = CorpseLib.Scripts.Context.Environment;

namespace CorpseLib.Scripts.Parser
{
    public class ParsingContext(Environment environment, List<AComment> comments)
    {
        private readonly Environment m_Environment = environment;
        private readonly Environment m_LoadedEnvironment = new();
        private readonly ConversionTable m_ConversionTable = new();
        private readonly List<AComment> m_Comments = comments;
        private readonly List<string> m_Warnings = [];
        private readonly List<int> m_Namespaces = [];
        private string m_Error = string.Empty;
        private bool m_HasErrors = false;

        public Environment LoadedEnvironment => m_LoadedEnvironment;
        public ConversionTable ConversionTable => m_ConversionTable;
        public AComment[] Comments => [..m_Comments];
        public string[] Warnings => [.. m_Warnings];
        public int[] Namespaces => [.. m_Namespaces];
        public string Error => m_Error;
        public bool HasErrors => m_HasErrors;

        internal int AddComment(AComment comment)
        {
            m_Comments.Add(comment);
            return m_Comments.Count - 1;
        }

        public void ClearWarnings() => m_Warnings.Clear();
        public void ClearErrors()
        {
            m_Error = string.Empty;
            m_HasErrors = false;
        }

        internal void RegisterWarning(string warning, string description) => m_Warnings.Add($"WARNING : {warning} : {description}");

        internal void RegisterError(string error, string description)
        {
            m_Error = $"ERROR : {error} : {description}";
            m_HasErrors = true;
        }

        internal bool PushFunction(AFunction function, int[] tags, int[] comments) =>
            m_Environment.AddFunction([.. m_Namespaces], function, tags, comments) &&
            m_LoadedEnvironment.AddFunction([.. m_Namespaces], function, tags, comments);

        internal void PushInstruction(AInstruction instruction, int[] tags, int[] comments)
        {
            m_Environment.AddInstruction(instruction, [.. m_Namespaces], tags, comments);
            m_LoadedEnvironment.AddInstruction(instruction, [.. m_Namespaces], tags, comments);
        }

        internal void PushTypeDefinition(TypeDefinition typeDefinition, int[] tags, int[] comments)
        {
            m_Environment.AddTypeDefinition(typeDefinition, tags, comments);
            m_LoadedEnvironment.AddTypeDefinition(typeDefinition, tags, comments);
        }

        internal void PushType(ATypeInstance type)
        {
            m_Environment.AddType(type);
            m_LoadedEnvironment.AddType(type);
        }

        internal bool PushNamespace(string @namespace, int[] tags, int[] comments)
        {
            int namespaceID = m_ConversionTable.PushName(@namespace);
            if (m_Environment.AddNamespace([.. m_Namespaces], namespaceID, tags, comments) &&
                m_LoadedEnvironment.AddNamespace([.. m_Namespaces], namespaceID, tags, comments))
            {
                m_Namespaces.Add(namespaceID);
                return true;
            }
            return false;
        }

        internal void PopNamespace() => m_Namespaces.RemoveAt(m_Namespaces.Count - 1);

        internal int PushName(string name) => m_ConversionTable.PushName(name);

        internal ParameterType? Instantiate(TypeInfo typeInfo) => m_LoadedEnvironment.Instantiate(typeInfo, [..m_Namespaces]);
    }
}
