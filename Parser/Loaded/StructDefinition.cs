namespace CorpseLib.Scripts.Parser.Loaded
{
    public class StructDefinition(string[] tags, string name, string body)
    {
        public readonly string[] Tags = tags;
        public readonly string Name = name;
        public readonly string Body = body;
    }
}
