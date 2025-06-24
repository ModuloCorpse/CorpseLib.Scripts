using System.Text;

namespace CorpseLib.Scripts.Parser
{
    internal static class CommentAndTagParser
    {
        internal class CommentAndTags(int[] commentIDs, int[] tags)
        {
            public int[] CommentIDs = commentIDs;
            public int[] Tags = tags;
        }

        //TODO improve tags indexing and parsing
        private static int[] SplitTags(string tags, ParsingContext parsingContext)
        {
            List<int> result = [];
            bool inString = false;
            int inParameters = 0;
            StringBuilder builder = new();
            int i = 0;
            char stringChar = '\0';
            while (i < tags.Length)
            {
                char c = tags[i];
                if (inString || inParameters != 0)
                {
                    if (c == stringChar)
                        inString = false;
                    else if (c == '"' || c == '\'')
                    {
                        inString = true;
                        stringChar = c;
                    }
                    else if (!inString)
                    {
                        if (c == '(')
                            ++inParameters;
                        else if (c == ')')
                            --inParameters;
                    }
                    if (!inString && inParameters == 0)
                    {
                        while ((i + 1) < tags.Length && tags[i + 1] != ',')
                        {
                            ++i;
                            if (!char.IsWhiteSpace(tags[i]))
                                return [];
                        }
                    }
                    builder.Append(c);
                }
                else if (c == ',')
                {
                    if (builder.Length > 0)
                    {
                        result.Add(parsingContext.PushName(builder.ToString()));
                        builder.Clear();
                    }
                }
                else if (c == '"' || c == '\'')
                {
                    inString = true;
                    stringChar = c;
                    if (builder.Length > 0)
                    {
                        foreach (char builderChar in builder.ToString())
                        {
                            if (!char.IsWhiteSpace(tags[i]))
                                return [];
                        }
                        builder.Clear();
                    }
                }
                else if (c == '(')
                {
                    ++inParameters;
                    builder.Append(c);
                    if ((i + 1) < tags.Length && tags[i + 1] == ')')
                    {
                        --inParameters;
                        ++i;
                        c = tags[i];
                        builder.Append(c);
                    }
                }
                else if (c == '\\')
                {
                    ++i;
                    c = tags[i];
                    builder.Append(c);
                }
                else
                    builder.Append(c);
                ++i;
            }
            if (builder.Length > 0)
            {
                result.Add(parsingContext.PushName(builder.ToString()));
                builder.Clear();
            }
            return [.. result];
        }

        private static void LoadCommentAndTags(ref string str, List<int> commentIDs, List<int> tags, ParsingContext parsingContext)
        {
            if (str.StartsWith("/*"))
            {
                while (str.StartsWith("/*"))
                {
                    int endIndex = str.IndexOf("*/");
                    string comment = str[2..endIndex].Trim();
                    commentIDs.Add(int.Parse(comment));
                    str = str[(endIndex + 2)..].Trim();
                }
                LoadCommentAndTags(ref str, commentIDs, tags, parsingContext);
            }
            else if (str.StartsWith('['))
            {
                Tuple<string, string, string> tagsTuple = ParserHelper.IsolateScope(str, '[', ']', out bool tagFound);
                if (tagFound)
                {
                    tags.AddRange(SplitTags(tagsTuple.Item2, parsingContext));
                    str = tagsTuple.Item3;
                }
                else
                {
                    parsingContext.RegisterError("Invalid script", "Invalid tags");
                    return;
                }
                LoadCommentAndTags(ref str, commentIDs, tags, parsingContext);
            }
        }

        internal static CommentAndTags? LoadCommentAndTags(ref string str, ParsingContext parsingContext)
        {
            List<int> commentIDs = [];
            List<int> tags = [];
            LoadCommentAndTags(ref str, commentIDs, tags, parsingContext);
            if (parsingContext.HasErrors)
                return null;
            return new CommentAndTags([.. commentIDs], [.. tags]);
        }

        internal static List<AComment> TrimComments(ref string str, ParsingContext parsingContext)
        {
            bool inString = false;
            bool inMultiLineComment = false;
            bool inSingleLineComment = false;
            StringBuilder sb = new();
            StringBuilder commentBuilder = new();
            List<AComment> comments = [];
            for (int i = 0; i < str.Length; ++i)
            {
                char c = str[i];
                if (inMultiLineComment)
                {
                    if (c == '*' && i != str.Length - 1 && str[i + 1] == '/')
                    {
                        ++i;
                        inMultiLineComment = false;
                        string comment = commentBuilder.ToString().Trim();
                        int commentID = comments.Count;
                        comments.Add(new MultiLineComment(comment.Split('\n')));
                        sb.Append($"/*{commentID}*/");
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
                        string comment = commentBuilder.ToString().Trim();
                        int commentID = comments.Count;
                        comments.Add(new SingleLineComment(comment));
                        sb.Append($"/*{commentID}*/");
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
                parsingContext.RegisterError("Comments error", "Unclosing comment");
                return [];
            }
            if (inString)
            {
                parsingContext.RegisterError("Comments error", "Unclosing string");
                return [];
            }
            str = sb.ToString();
            return comments;
        }
    }
}
