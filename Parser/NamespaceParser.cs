using CorpseLib.Scripts.Instructions;
using CommentAndTags = CorpseLib.Scripts.Parser.CommentAndTagParser.CommentAndTags;

namespace CorpseLib.Scripts.Parser
{
    internal static class NamespaceParser
    {
        internal static void LoadNamespaceContent(string str, ParsingContext parsingContext)
        {
            while (!string.IsNullOrEmpty(str))
            {
                CommentAndTags? commentAndTags = CommentAndTagParser.LoadCommentAndTags(ref str, parsingContext);
                if (parsingContext.HasErrors)
                    return;

                if (!string.IsNullOrEmpty(str))
                {
                    if (str.StartsWith("fct "))
                        FunctionParser.LoadFunctionContent(commentAndTags!, ref str, parsingContext);
                    else if (str.StartsWith("struct "))
                        StructureParser.LoadStructureContent(commentAndTags!, ref str, parsingContext);
                    else if (str.StartsWith("namespace "))
                    {
                        Tuple<string, string, string> scoped = ParserHelper.IsolateScope(str, '{', '}', out bool found);
                        if (!found)
                        {
                            parsingContext.RegisterError("Invalid script", "Bad namespace definition");
                            return;
                        }
                        string namespaceName = scoped.Item1[10..];
                        if (!parsingContext.PushNamespace(namespaceName, commentAndTags!.Tags, commentAndTags!.CommentIDs))
                        {
                            parsingContext.RegisterError("Invalid script", $"Namespace {ScriptWriter.GenerateNamespaceName(parsingContext.Namespaces, parsingContext.ConversionTable)} already exist");
                            return;
                        }
                        LoadNamespaceContent(scoped.Item2, parsingContext);
                        parsingContext.PopNamespace();
                        if (parsingContext.HasErrors)
                            return;
                        str = scoped.Item3;
                    }
                    else
                    {
                        AInstruction? instruction = FunctionParser.LoadInstruction("Invalid script instructions", ref str, parsingContext);
                        if (parsingContext.HasErrors || instruction == null)
                            return;
                        //TODO : Handle specifically global variables instructions
                        //TODO : Handle namespaces
                        parsingContext.PushInstruction(instruction, commentAndTags!.Tags, commentAndTags.CommentIDs);
                    }
                }
                else
                {
                    parsingContext.RegisterError("Invalid script", "Tags not associated to anything");
                    return;
                }
            }
        }
    }
}
