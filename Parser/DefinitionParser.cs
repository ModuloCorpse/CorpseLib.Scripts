using CorpseLib.Scripts.Parser.Loaded;
using System.Text;

namespace CorpseLib.Scripts.Parser
{
    internal class DefinitionParser : ParserBase
    {
        private static string[] SplitTags(string tags)
        {
            List<string> result = [];
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
                        result.Add(builder.ToString());
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
                result.Add(builder.ToString());
                builder.Clear();
            }
            return [.. result];
        }

        public void LoadNamespaceDefinition(NamespaceDefinition lastDefinition, string str)
        {
            while (!string.IsNullOrEmpty(str))
            {
                string[] tags = [];
                if (str.StartsWith('['))
                {
                    Tuple<string, string, string> tagsTuple = IsolateScope(str, '[', ']', out bool tagFound);
                    if (tagFound)
                    {
                        tags = SplitTags(tagsTuple.Item2);
                        str = tagsTuple.Item3;
                    }
                    else
                    {
                        RegisterError("Invalid script", "Invalid tags");
                        return;
                    }
                }

                if (!string.IsNullOrEmpty(str))
                {
                    if (str.StartsWith("fct "))
                    {
                        Tuple<string, string, string> signatureSplit = IsolateScope(str, '(', ')', out bool parametersFound);
                        if (!parametersFound)
                        {
                            RegisterError("Misformated function signature string", "Bad parenthesis");
                            return;
                        }
                        string signatureName = signatureSplit.Item1[4..];
                        string signatureParameters = signatureSplit.Item2;
                        str = signatureSplit.Item3;
                        Tuple<string, string, string> scoped = IsolateScope(str, '{', '}', out bool found);
                        if (!found)
                        {
                            RegisterError("Invalid script", "Bad function definition");
                            return;
                        }
                        string functionBody = scoped.Item2;
                        str = scoped.Item3;
                        lastDefinition.FunctionDefinitions.Add(new(tags, signatureName, signatureParameters, functionBody));
                    }
                    else if (str.StartsWith("struct "))
                    {
                        Tuple<string, string, string> scoped = IsolateScope(str, '{', '}', out bool found);
                        if (!found)
                        {
                            RegisterError("Invalid script", "Bad structure definition");
                            return;
                        }
                        string structName = scoped.Item1[7..];
                        string structBody = scoped.Item2;
                        str = scoped.Item3;
                        StructDefinition newStructDefinition = new(tags, structName, structBody);
                        foreach (StructDefinition namespaceDefinition in lastDefinition.StructDefinitions)
                        {
                            if (namespaceDefinition.Name == newStructDefinition.Name)
                            {
                                RegisterError("Invalid script", string.Format("Structure {0} already exist", newStructDefinition.Name));
                                return;
                            }
                        }
                        lastDefinition.StructDefinitions.Add(newStructDefinition);
                    }
                    else if (str.StartsWith("namespace "))
                    {
                        Tuple<string, string, string> scoped = IsolateScope(str, '{', '}', out bool found);
                        if (!found)
                        {
                            RegisterError("Invalid script", "Bad namespace definition");
                            return;
                        }
                        NamespaceDefinition childNamespace = new(tags, scoped.Item1[10..]);
                        foreach (NamespaceDefinition namespaceDefinition in lastDefinition.NamespaceDefinitions)
                        {
                            if (namespaceDefinition.Name == childNamespace.Name)
                            {
                                RegisterError("Invalid script", string.Format("Namespace {0} already exist", childNamespace.Name));
                                return;
                            }
                        }
                        lastDefinition.NamespaceDefinitions.Add(childNamespace);
                        LoadNamespaceDefinition(childNamespace, scoped.Item2);
                        if (HasError)
                            return;
                        str = scoped.Item3;
                    }
                    else
                    {
                        /*Tuple<string, string> instruction = NextInstruction(str, out bool found);
                        if (!found)
                        {
                            RegisterError("Invalid script", "Bad global definition");
                            return;
                        }
                        Parameter? global = ParseParameter(instruction.Item1, @namespace);
                        if (HasError)
                            return;
                        if (!@namespace.AddGlobal(global!))
                        {
                            RegisterError("Invalid script", string.Format("Variable {0} already exist", global!.ID));
                            return;
                        }
                        str = instruction.Item2;*/
                    }
                }
                else
                {
                    RegisterError("Invalid script", "Tags not associated to anything");
                    return;
                }
            }
        }
    }
}
