using CorpseLib.Scripts.Parser.Loaded;
using System.Text;

namespace CorpseLib.Scripts.Parser
{
    internal class ScriptFileParser : ParserBase
    {
        private List<string> TrimComments(ref string str)
        {
            bool inString = false;
            bool inMultiLineComment = false;
            bool inSingleLineComment = false;
            StringBuilder sb = new();
            StringBuilder commentBuilder = new();
            List<string> comments = [];
            for (int i = 0; i < str.Length; ++i)
            {
                char c = str[i];
                if (inMultiLineComment)
                {
                    if (c == '*' && i != str.Length - 1 && str[i + 1] == '/')
                    {
                        ++i;
                        inMultiLineComment = false;
                        comments.Add(commentBuilder.ToString().Trim());
                        commentBuilder.Clear();
                    }
                    else
                        commentBuilder.Append(c);
                }
                else if (inSingleLineComment)
                {
                    if (c == '\n')
                    {
                        commentBuilder.Append(c);
                        inSingleLineComment = false;
                        comments.Add(commentBuilder.ToString().Trim());
                        commentBuilder.Clear();
                    }
                    else
                        commentBuilder.Append(c);
                }
                else if (inString)
                {
                    sb.Append(c);
                    if (c == '\\' && i != str.Length - 1)
                    {
                        sb.Append(str[i + 1]);
                        ++i;
                    }
                    if (c == '"')
                        inString = false;
                }
                else if (c == '\\' && i != str.Length - 1)
                {
                    sb.Append(str[i + 1]);
                    ++i;
                }
                else if (c == '/' && i != str.Length - 1 && str[i + 1] == '*')
                {
                    ++i;
                    inMultiLineComment = true;
                }
                else if (c == '/' && i != str.Length - 1 && str[i + 1] == '/')
                {
                    ++i;
                    inSingleLineComment = true;
                }
                else if (c == '"')
                {
                    sb.Append(c);
                    inString = true;
                }
                else
                    sb.Append(c);
            }
            if (inMultiLineComment)
            {
                RegisterError("Comments error", "Unclosing comment");
                return [];
            }
            if (inString)
            {
                RegisterError("Comments error", "Unclosing string");
                return [];
            }
            str = sb.ToString();
            return comments;
        }

        public Script? ParseScript(string str)
        {
            List<string> comments = TrimComments(ref str);
            if (HasError)
                return null;
            int i = 0;
            foreach (string comment in comments)
            {
                Console.WriteLine("===== Comment {0} =====", i++);
                Console.WriteLine(comment);
            }
            Shell.Helper.TrimCommand(ref str);

            NamespaceDefinition scriptNamespace = new([], string.Empty);

            List<NamespaceDefinition> imports = [];
            //TODO Load import definitions

            DefinitionParser definitionParser = new();
            definitionParser.LoadNamespaceDefinition(scriptNamespace, str);
            RegisterOtherBase(definitionParser);
            if (HasError)
                return null;

            //Parse structure definitions

            //Parse globals definitions

            // Parse functions
            return null;
        }
    }
}
