namespace CorpseLib.Scripts.Parser.Loaded
{
    public class FunctionDefinition(string[] tags, string signature, string parameters, string body)
    {
        public readonly string[] Tags = tags;
        public readonly string Signature = signature;
        public readonly string SignatureParameters = parameters;
        public readonly string Body = body;
    }
}
