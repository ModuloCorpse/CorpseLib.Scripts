using Environment = CorpseLib.Scripts.Context.Environment;

namespace CorpseLib.Scripts.Parser
{
    //TODO : Rework how parser work to be able to parse some element after others and parse in order :
    // - Imports
    // - namespaces
    // - struct declarations
    // - globals
    // - functions
    public static class ScriptParser
    {
        public static ParserResult ParseScript(string str, Environment env)
        {
            ParsingContext parsingContext = new();
            List<AComment> comments = CommentAndTagParser.TrimComments(ref str, parsingContext);
            if (parsingContext.HasErrors)
                return new(parsingContext.Error);
            int i = 0;
            Shell.Helper.TrimCommand(ref str);
            Script script = new();
            //TODO Parse imports/include
            NamespaceParser.LoadNamespaceContent(script, str, parsingContext);
            foreach (AComment comment in comments)
            {
                Console.WriteLine("===== Comment {0} =====", i++);
                Console.WriteLine(comment);
            }
            return parsingContext.CreateParserResult(script, comments);
        }

        public static ParserResult ParseScript(string str) => ParseScript(str, new Environment());
    }
}
