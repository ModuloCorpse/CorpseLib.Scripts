using CorpseLib.Scripts.Instructions;
using CommentAndTags = CorpseLib.Scripts.Parser.CommentAndTagParser.CommentAndTags;

namespace CorpseLib.Scripts.Parser
{
    internal static class NamespaceParser
    {
        private static void LoadNamespace(string namespaceName, string namespaceContent, OldEnvironment env, ParsingContext parsingContext)
        {
            Namespace? parent = (env is Namespace namespc) ? namespc : null;
            Namespace @namespace = new(parsingContext.PushName(namespaceName), parent);
            LoadNamespaceContent(@namespace, namespaceContent, parsingContext);
            if (parsingContext.HasErrors)
                return;
            if (!env.AddNamespace(@namespace))
            {
                parsingContext.RegisterError("Invalid script", $"Namespace {ScriptWriter.GenerateNamespaceName(parsingContext.Namespaces, parsingContext.ConversionTable)} already exist");
                return;
            }
            return;
        }

        internal static void LoadNamespaceContent(OldEnvironment @namespace, string str, ParsingContext parsingContext)
        {
            while (!string.IsNullOrEmpty(str))
            {
                CommentAndTags? commentAndTags = CommentAndTagParser.LoadCommentAndTags(ref str, parsingContext);
                if (parsingContext.HasErrors)
                    return;

                if (!string.IsNullOrEmpty(str))
                {
                    if (str.StartsWith("fct "))
                        FunctionParser.LoadFunctionContent(commentAndTags!, @namespace, ref str, parsingContext);
                    else if (str.StartsWith("struct "))
                        StructureParser.LoadStructureContent(commentAndTags!, @namespace, ref str, parsingContext);
                    else if (str.StartsWith("namespace "))
                    {
                        Tuple<string, string, string> scoped = ParserHelper.IsolateScope(str, '{', '}', out bool found);
                        if (!found)
                        {
                            parsingContext.RegisterError("Invalid script", "Bad namespace definition");
                            return;
                        }
                        string namespaceName = scoped.Item1[10..];
                        int namespaceID = parsingContext.PushNamespace(namespaceName, commentAndTags!.Tags, commentAndTags!.CommentIDs);
                        LoadNamespace(namespaceName, scoped.Item2, @namespace, parsingContext);
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
                        @namespace.AddInstruction(instruction);
                        parsingContext.PushInstruction(instruction, parsingContext.Namespaces, commentAndTags!.Tags, commentAndTags.CommentIDs);
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
