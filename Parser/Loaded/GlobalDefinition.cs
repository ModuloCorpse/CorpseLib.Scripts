namespace CorpseLib.Scripts.Parser.Loaded
{
    public class GlobalDefinition(string[] tags, string type, string name, string? value)
    {
        public readonly string[] Tags = tags;
        public readonly string Type = type;
        public readonly string Name = name;
        public readonly string? Value = value;
    }
}
