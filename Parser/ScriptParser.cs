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
        public static ParsingContext ParseScript(string str, Environment env)
        {
            ParsingContext parsingContext = new(env, []);
            CommentAndTagParser.TrimComments(ref str, parsingContext);
            if (parsingContext.HasErrors)
                return parsingContext;
            Shell.Helper.TrimCommand(ref str);
            //TODO Parse imports/include
            NamespaceParser.LoadNamespaceContent(str, parsingContext);
            return parsingContext;
        }

        public static ParsingContext ParseScript(string str) => ParseScript(str, new Environment());
    }
}
