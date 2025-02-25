namespace CorpseLib.Scripts.Parser.Loaded
{
    public class NamespaceDefinition(string[] tags, string name)
    {
        public readonly List<NamespaceDefinition> NamespaceDefinitions = [];
        public readonly List<StructDefinition> StructDefinitions = [];
        public readonly List<GlobalDefinition> GlobalDefinitions = [];
        public readonly List<FunctionDefinition> FunctionDefinitions = [];
        public readonly string[] Tags = tags;
        public readonly string Name = name;
    }
}
