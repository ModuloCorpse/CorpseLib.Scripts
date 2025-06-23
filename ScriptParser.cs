using CorpseLib.Scripts.Type.Primitive;
using CorpseLib.Scripts.Type;
using System.Text;
using CorpseLib.Scripts.Context;
using CorpseLib.Scripts.Instruction;
using CorpseLib.Scripts.Parser;
using Environment = CorpseLib.Scripts.Context.Environment;

namespace CorpseLib.Scripts
{
    //TODO : Rework how parser work to be able to parse some element after others and parse in order :
    // - Imports
    // - namespaces
    // - struct declarations
    // - globals
    // - functions
    public class ScriptParser
    {
        private class CommentAndTags(int[] commentIDs, int[] tags)
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

        private void LoadCommentAndTags(ref string str, List<int> commentIDs, List<int> tags, ParsingContext parsingContext)
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
                Tuple<string, string, string> tagsTuple = IsolateScope(str, '[', ']', out bool tagFound);
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

        private CommentAndTags? LoadCommentAndTags(ref string str, ParsingContext parsingContext)
        {
            List<int> commentIDs = [];
            List<int> tags = [];
            LoadCommentAndTags(ref str, commentIDs, tags, parsingContext);
            if (parsingContext.HasErrors)
                return null;
            return new CommentAndTags([.. commentIDs], [.. tags]);
        }

        private static string RemoveFirst(string str, string toRemove)
        {
            str = str[(toRemove.Length)..];
            while (str.Length > 0 && str[0] == ' ')
                str = str[1..];
            return str;
        }

        internal static Tuple<string, string, string> IsolateScope(string str, char open, char close, out bool found)
        {
            int openScopeIdx = str.IndexOf(open);
            if (openScopeIdx != -1)
            {
                int closeScopeIdx = openScopeIdx;
                int scopeCount = 0;
                while (closeScopeIdx < str.Length)
                {
                    if (str[closeScopeIdx] == open)
                        ++scopeCount;
                    else if (str[closeScopeIdx] == close)
                    {
                        --scopeCount;
                        if (scopeCount == 0)
                        {
                            string beforeScope = str[..openScopeIdx];
                            if (beforeScope.Length > 0 && beforeScope[^1] == ' ')
                                beforeScope = beforeScope[..^1];

                            string inScope = str[(openScopeIdx + 1)..closeScopeIdx];
                            if (inScope.Length > 0 && inScope[^1] == ' ')
                                inScope = inScope[..^1];
                            if (inScope.Length > 0 && inScope[0] == ' ')
                                inScope = inScope[1..];

                            string afterScope = ((closeScopeIdx + 1) < str.Length) ? str[(closeScopeIdx + 1)..] : string.Empty;
                            if (afterScope.Length > 0 && afterScope[0] == ' ')
                                afterScope = afterScope[1..];
                            found = true;
                            return new(beforeScope, inScope, afterScope);
                        }
                    }
                    ++closeScopeIdx;
                }
            }
            found = false;
            return new(str, string.Empty, string.Empty);
        }

        internal static Tuple<string, string> NextInstruction(string str, out bool found)
        {
            bool inString = false;
            char stringChar = '\0';
            for (int i = 0; i != str.Length; ++i)
            {
                char c = str[i];
                if (inString)
                {
                    if (c == stringChar)
                        inString = false;
                }
                else if (c == '"' || c == '\'')
                {
                    inString = true;
                    stringChar = c;
                }
                else if (c == ';')
                {
                    found = true;
                    string instruction = str[..i];
                    if (instruction.Length > 0 && instruction[^1] == ' ')
                        instruction = instruction[..^1];
                    string ret = string.Empty;
                    if ((i + 1) != str.Length)
                        ret = str[(i + 1)..];
                    if (ret.Length > 0 && ret[0] == ' ')
                        ret = ret[1..];
                    return new(instruction, ret);
                }
            }
            found = false;
            return new(string.Empty, string.Empty);
        }

        internal static Tuple<string, string> NextString(string str)
        {
            bool inString = false;
            StringBuilder builder = new();
            for (int i = 0; i != str.Length; ++i)
            {
                char c = str[i];
                if (inString && c == '"')
                {
                    inString = false;
                    builder.Append(c);
                    while ((i + 1) < str.Length && str[i + 1] != ',')
                    {
                        ++i;
                        if (!char.IsWhiteSpace(str[i]))
                            throw new ArgumentException("Invalid element");
                    }
                }
                else if (c == '"')
                {
                    inString = true;
                    builder.Append(c);
                }
                else if (c == ',' && !inString)
                {
                    ++i;
                    while (i < str.Length && char.IsWhiteSpace(str[i]))
                        ++i;
                    string ret = str[i..];
                    string element = builder.ToString();
                    if (ret.Length > 0 && ret[0] == ' ')
                        ret = ret[1..];
                    return new(element, ret);
                }
                else
                    builder.Append(c);
            }
            return new(str, string.Empty);
        }

        internal static Tuple<string, string> NextElement(string str, char expectedOpen, char expectedClose)
        {
            int nbOpen = 0;
            StringBuilder builder = new();
            for (int i = 0; i != str.Length; ++i)
            {
                char c = str[i];
                if (c == expectedClose)
                {
                    --nbOpen;
                    builder.Append(c);
                    if (nbOpen == 0)
                    {
                        while ((i + 1) < str.Length && str[i + 1] != ',')
                        {
                            ++i;
                            if (!char.IsWhiteSpace(str[i]))
                                throw new ArgumentException("Invalid element");
                        }
                    }
                }
                else if (c == expectedOpen)
                {
                    ++nbOpen;
                    builder.Append(c);
                }
                else if (c == ',' && nbOpen == 0)
                {
                    ++i;
                    while (i < str.Length && char.IsWhiteSpace(str[i]))
                        ++i;
                    string ret = str[i..];
                    string element = builder.ToString();
                    if (ret.Length > 0 && ret[0] == ' ')
                        ret = ret[1..];
                    return new(element, ret);
                }
                else
                    builder.Append(c);
            }
            return new(str, string.Empty);
        }

        private string[] SplitParameter(string parameter, out bool isConst, ParsingContext parsingContext)
        {
            isConst = false;
            if (string.IsNullOrEmpty(parameter))
            {
                parsingContext.RegisterError("Misformated parameter string", "Empty parameter");
                return [];
            }
            if (parameter.StartsWith("const "))
            {
                parameter = parameter[6..];
                isConst = true;
            }
            List<string> ret = [];
            int assignationIndex = parameter.IndexOf('=');
            if (assignationIndex != -1)
            {
                string value = parameter[(assignationIndex + 1)..];
                if (value.Length > 0 && value[0] == ' ')
                    value = value[1..];
                if (string.IsNullOrEmpty(value))
                {
                    parsingContext.RegisterError("Misformated parameter string", "Empty value");
                    return [];
                }
                string toSplit = parameter[..assignationIndex];
                if (toSplit.Length > 0 && toSplit[^1] == ' ')
                    toSplit = toSplit[..^1];
                string[] parameterParts = Shell.Helper.Split(toSplit, ' ');
                if (parameterParts.Length != 2)
                {
                    parsingContext.RegisterError("Misformated parameter string", "Parameter should be [type] [name] = [value]");
                    return [];
                }
                ret.AddRange(parameterParts);
                ret.Add(value);
            }
            else
            {
                string[] parameterParts = Shell.Helper.Split(parameter, ' ');
                if (parameterParts.Length != 2)
                {
                    parsingContext.RegisterError("Misformated parameter string", "Parameter should be [type] [name]");
                    return [];
                }
                ret.AddRange(parameterParts);
            }
            return [.. ret];
        }

        private static OperationResult<int[]> SplitTemplate(string template, ParsingContext parsingContext)
        {
            List<int> result = [];
            StringBuilder builder = new();
            int templateCount = 0;
            foreach (char c in template)
            {
                if (templateCount != 0)
                {
                    if (c == '>')
                    {
                        --templateCount;
                        if (templateCount < 0)
                            return new("Template error", string.Format("Bad template {0}", template));
                    }
                    else if (c == '<')
                        ++templateCount;
                    if (!char.IsWhiteSpace(c))
                        builder.Append(c);
                }
                else if (c == ',')
                {
                    if (builder.Length > 0)
                    {
                        result.Add(parsingContext.PushName(builder.ToString()));
                        builder.Clear();
                    }
                    else
                        return new("Template error", string.Format("Bad template {0}", template));
                }
                else if (c == '<')
                {
                    ++templateCount;
                    builder.Append(c);
                }
                else if (c == '>')
                    return new("Template error", string.Format("Bad template {0}", template));
                else if (!char.IsWhiteSpace(c))
                    builder.Append(c);
            }
            if (builder.Length > 0)
                result.Add(parsingContext.PushName(builder.ToString()));
            return new([.. result]);
        }

        private static OperationResult<Tuple<string, int[]>> ExtractTypeName(string template, ParsingContext parsingContext)
        {
            int templateIdx = template.IndexOf('<');
            if (templateIdx >= 0)
            {
                OperationResult<int[]> templatesResult = SplitTemplate(template[(templateIdx + 1)..^1], parsingContext);
                if (templatesResult && templatesResult.Result != null)
                {
                    int[] templateTypesIDs = templatesResult.Result;
                    string templateName = template[..templateIdx];
                    return new(new(templateName, templateTypesIDs));
                }
                return new(templatesResult.Error, templatesResult.Description);
            }
            return new(new(template, []));
        }

        private Parameter? ParseParameter(string parameter, OldEnvironment env, ParsingContext parsingContext)
        {
            if (string.IsNullOrEmpty(parameter))
            {
                parsingContext.RegisterError("Misformated parameter string", "Empty parameter");
                return null;
            }
            //TODO : Do not loose constness (Maybe use a TypeInfo)
            string[] parameterParts = SplitParameter(parameter, out bool isConst, parsingContext);
            if (parsingContext.HasErrors)
                return null;
            OperationResult<TypeInfo> typeInfo = TypeInfo.ParseStr(parameterParts[0], parsingContext.ConversionTable);
            if (!typeInfo)
            {
                parsingContext.RegisterError(typeInfo.Error, typeInfo.Description);
                return null;
            }
            ATypeInstance? parameterType = env.Instantiate(typeInfo.Result!);
            if (parameterType == null)
            {
                parsingContext.RegisterError("Unknown parameter type", parameterParts[0]);
                return null;
            }
            if (parameterType is VoidType)
            {
                parsingContext.RegisterError("Invalid script", "Parameter type cannot be void");
                return null;
            }
            object[]? value = null;
            if (parameterParts.Length == 3)
                value = ParseValue(parameterParts[2], parsingContext);
            if (value != null)
                return new Parameter(parameterType, isConst, parsingContext.PushName(parameterParts[1]), parameterType.InternalConvert(value));
            else
                return new Parameter(parameterType, isConst, parsingContext.PushName(parameterParts[1]));
        }

        private static string[] FunctionSignatureParameterSplit(string content)
        {
            List<string> result = [];
            bool inString = false;
            int inObject = 0;
            int inArray = 0;
            StringBuilder builder = new();
            int i = 0;
            char stringChar = '\0';
            while (i < content.Length)
            {
                char c = content[i];
                if (inString || inObject != 0 || inArray != 0)
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
                        if (c == '[')
                            ++inArray;
                        else if (c == ']')
                            --inArray;
                        else if (c == '{')
                            ++inObject;
                        else if (c == '}')
                            --inObject;
                    }
                    if (!inString && inObject == 0 && inArray == 0)
                    {
                        while ((i + 1) < content.Length && content[i + 1] != ',')
                        {
                            ++i;
                            if (!char.IsWhiteSpace(content[i]))
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
                            if (!char.IsWhiteSpace(content[i]))
                                return [];
                        }
                        builder.Clear();
                    }
                }
                else if (c == '{')
                {
                    ++inObject;
                    builder.Append(c);
                    if ((i + 1) < content.Length && content[i + 1] == '}')
                    {
                        --inObject;
                        ++i;
                        c = content[i];
                        builder.Append(c);
                    }
                }
                else if (c == '[')
                {
                    ++inArray;
                    builder.Append(c);
                    if ((i + 1) < content.Length && content[i + 1] == ']')
                    {
                        --inArray;
                        ++i;
                        c = content[i];
                        builder.Append(c);
                    }
                }
                else if (c == '\\')
                {
                    ++i;
                    c = content[i];
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

        private FunctionSignature? ParseFunctionSignature(string parametersStr, string signatureStr, OldEnvironment env, ParsingContext parsingContext, out string functionName)
        {
            string[] signatureParts = signatureStr.Split(' ');
            string returnTypeStr;
            switch (signatureParts.Length)
            {
                case 1:
                {
                    returnTypeStr = "void";
                    functionName = signatureParts[0];
                    break;
                }
                case 2:
                {
                    returnTypeStr = signatureParts[0];
                    functionName = signatureParts[1];
                    break;
                }
                default:
                {
                    parsingContext.RegisterError("Misformated function signature string", string.Format("Bad signature : {0}", parametersStr));
                    functionName = string.Empty;
                    return null;
                }
            }
            List<Parameter> parameters = [];
            if (!string.IsNullOrEmpty(parametersStr))
            {
                string[] parametersStrArr = FunctionSignatureParameterSplit(parametersStr);
                for (int i = 0; i != parametersStrArr.Length; ++i)
                {
                    Parameter? result = ParseParameter(parametersStrArr[i], env, parsingContext);
                    if (parsingContext.HasErrors)
                        return null;
                    parameters.Add(result!);
                }
            }

            OperationResult<TypeInfo> returnTypeInfo = TypeInfo.ParseStr(returnTypeStr, parsingContext.ConversionTable);
            if (!returnTypeInfo)
            {
                parsingContext.RegisterError(returnTypeInfo.Error, returnTypeInfo.Description);
                return null;
            }
            ATypeInstance? returnType = env.Instantiate(returnTypeInfo.Result!);
            if (returnType == null)
            {
                parsingContext.RegisterError("Misformated function signature string", string.Format("Unknown return type : {0}", returnTypeStr));
                return null;
            }
            return new(returnType, parsingContext.PushName(functionName), [.. parameters]);
        }

        private AInstruction? ParseInstruction(string instruction)
        {
            if (instruction == "break")
                return new Break();
            else if (instruction == "continue")
                return new Continue();
            //TODO
            return new DebugInstruction(instruction);
        }

        private Tuple<string, string> ParseKeyword(string errorMessage, string keyword, ref string str, bool shouldHaveCondition, ParsingContext parsingContext)
        {
            str = RemoveFirst(str, keyword);
            string condition = string.Empty;
            Tuple<string, string, string> tuple;
            bool found;
            if (shouldHaveCondition)
            {
                tuple = IsolateScope(str, '(', ')', out found);
                if (found)
                {
                    condition = tuple.Item2;
                    str = tuple.Item3;
                }
                else
                {
                    parsingContext.RegisterError(errorMessage, string.Format("Bad condition in {0}", keyword));
                    return new(string.Empty, string.Empty);
                }
            }
            if (str.Length == 0)
            {
                parsingContext.RegisterError(errorMessage, string.Format("Empty body in {0}", keyword));
                return new(string.Empty, string.Empty);
            }
            if (str[0] == '{')
            {
                tuple = IsolateScope(str, '{', '}', out found);
                if (!found)
                {
                    parsingContext.RegisterError(errorMessage, string.Format("Bad body in {0}", keyword));
                    return new(string.Empty, string.Empty);
                }
                string body = tuple.Item2;
                if (string.IsNullOrWhiteSpace(body))
                {
                    parsingContext.RegisterError(errorMessage, string.Format("Empty body in {0}", keyword));
                    return new(string.Empty, string.Empty);
                }
                str = tuple.Item3;
                return new(condition, body);
            }
            else
            {
                Tuple<string, string> instruction = NextInstruction(str, out found);
                if (!found)
                {
                    parsingContext.RegisterError(errorMessage, string.Format("Bad body in {0}", keyword));
                    return new(string.Empty, string.Empty);
                }
                string body = instruction.Item1 + ';';
                if (string.IsNullOrWhiteSpace(body))
                {
                    parsingContext.RegisterError(errorMessage, string.Format("Empty body in {0}", keyword));
                    return new(string.Empty, string.Empty);
                }
                str = instruction.Item2;
                return new(condition, body);
            }
        }

        private Condition ParseCondition(string condition)
        {
            //TODO
            return new Condition(condition);
        }

        private IfInstruction? LoadIf(string errorMessage, ref string str, ParsingContext parsingContext)
        {
            Tuple<string, string> keyWordParams = ParseKeyword(errorMessage, "if", ref str, true, parsingContext);
            if (parsingContext.HasErrors)
                return null;
            List<AInstruction> ifResult = FunctionLoadBody(errorMessage, keyWordParams.Item2, parsingContext);
            if (parsingContext.HasErrors)
                return null;
            IfInstruction ifInstruction = new(ParseCondition(keyWordParams.Item1), ifResult);
            while (str.StartsWith("elif"))
            {
                keyWordParams = ParseKeyword(errorMessage, "elif", ref str, true, parsingContext);
                if (parsingContext.HasErrors)
                    return null;
                List<AInstruction> elseIfResult = FunctionLoadBody(errorMessage, keyWordParams.Item2, parsingContext);
                if (parsingContext.HasErrors)
                    return null;
                ifInstruction.AddElif(ParseCondition(keyWordParams.Item1), elseIfResult);
            }
            if (str.StartsWith("else"))
            {
                keyWordParams = ParseKeyword(errorMessage, "else", ref str, false, parsingContext);
                if (parsingContext.HasErrors)
                    return null;
                List<AInstruction> elseResult = FunctionLoadBody(errorMessage, keyWordParams.Item2, parsingContext);
                if (parsingContext.HasErrors)
                    return null;
                ifInstruction.SetElseBody(elseResult);
            }
            return ifInstruction;
        }

        private AInstruction? LoadInstruction(string errorMessage, ref string body, ParsingContext parsingContext)
        {
            CommentAndTags? commentAndTags = LoadCommentAndTags(ref body, parsingContext);
            //TODO : Handle comments and tags
            if (parsingContext.HasErrors)
                return null;
            if (body.StartsWith("else"))
            {
                parsingContext.RegisterError(errorMessage, "else outside of if");
                return null;
            }
            else if (body.StartsWith("if"))
            {
                AInstruction? ifInstruction = LoadIf(errorMessage, ref body, parsingContext);
                if (parsingContext.HasErrors)
                    return null;
                return ifInstruction!;
            }
            else if (body.StartsWith("while"))
            {
                Tuple<string, string> keyWordParams = ParseKeyword(errorMessage, "while", ref body, true, parsingContext);
                if (parsingContext.HasErrors)
                    return null;
                List<AInstruction> whileBody = FunctionLoadBody(errorMessage, keyWordParams.Item2, parsingContext);
                if (parsingContext.HasErrors)
                    return null;
                return new WhileInstruction(ParseCondition(keyWordParams.Item1), whileBody);
            }
            else if (body.StartsWith("do"))
            {
                Tuple<string, string> keyWordParams = ParseKeyword(errorMessage, "do", ref body, false, parsingContext);
                if (parsingContext.HasErrors)
                    return null;
                List<AInstruction> doBody = FunctionLoadBody(errorMessage, keyWordParams.Item2, parsingContext);
                if (parsingContext.HasErrors)
                    return null;
                CommentAndTags? doWhileCommentAndTags = LoadCommentAndTags(ref body, parsingContext);
                //TODO : Handle comments and tags
                if (parsingContext.HasErrors)
                    return null;
                if (body.StartsWith("while"))
                {
                    Tuple<string, string, string> tuple = IsolateScope(body, '(', ')', out bool found);
                    if (found)
                    {
                        body = tuple.Item3;
                        return new DoWhileInstruction(ParseCondition(tuple.Item2), doBody);
                    }
                    else
                    {
                        parsingContext.RegisterError(errorMessage, "Bad condition in do");
                        return null;
                    }
                }
                else
                {
                    parsingContext.RegisterError(errorMessage, "do without while");
                    return null;
                }
            }
            else if (body.StartsWith("for"))
            {
                Tuple<string, string> keyWordParams = ParseKeyword(errorMessage, "for", ref body, true, parsingContext);
                if (parsingContext.HasErrors)
                    return null;
                List<AInstruction> forBody = FunctionLoadBody(errorMessage, keyWordParams.Item2, parsingContext);
                if (parsingContext.HasErrors)
                    return null;
                //TODO
                parsingContext.RegisterError("Not Supported Yet", "For not supported yet, please use while for now");
                return null;
            }
            else
            {
                Tuple<string, string> instructionParams = NextInstruction(body, out bool found);
                if (!found)
                {
                    parsingContext.RegisterError(errorMessage, "Bad instruction");
                    return null;
                }
                body = instructionParams.Item2;
                AInstruction? instruction = ParseInstruction(instructionParams.Item1);
                if (parsingContext.HasErrors)
                    return null;
                return instruction!;
            }
        }

        private List<AInstruction> FunctionLoadBody(string errorMessage, string body, ParsingContext parsingContext)
        {
            List<AInstruction> instructions = [];
            while (!string.IsNullOrEmpty(body))
            {
                AInstruction? instruction = LoadInstruction(errorMessage, ref body, parsingContext);
                if (parsingContext.HasErrors)
                    return [];
                instructions.Add(instruction!);
            }
            return instructions;
        }

        private static Tuple<string, string> IsolateFirstElem(string str)
        {
            for (int i = 0; i < str.Length; ++i)
            {
                if (str[i] == ',')
                {
                    string elem = str[..i];
                    string ret = str[(i + 1)..];
                    if (ret.Length > 0 && ret[0] == ' ')
                        ret = ret[1..];
                    return new(elem, ret);
                }
            }
            return new(str, string.Empty);
        }

        private object[] ParseValue(string str, ParsingContext parsingContext)
        {
            if (str == "null")
                return [];
            if (str.Length > 2 && str[0] == '{' && str[^1] == '}')
            {
                str = str[1..^1];
                if (str.Length > 0 && str[^1] == ' ')
                    str = str[..^1];
                if (str.Length > 0 && str[0] == ' ')
                    str = str[1..];
                List<object[]> variables = [];
                while (!string.IsNullOrEmpty(str))
                {
                    if (str[0] == '[')
                    {
                        Tuple<string, string> split = NextElement(str, '[', ']');
                        variables.Add(ParseValue(split.Item1, parsingContext));
                        str = split.Item2;
                    }
                    else if (str[0] == '{')
                    {
                        Tuple<string, string> split = NextElement(str, '{', '}');
                        variables.Add(ParseValue(split.Item1, parsingContext));
                        str = split.Item2;
                    }
                    else if (str[0] == '"')
                    {
                        Tuple<string, string> split = NextString(str);
                        variables.Add(ParseValue(split.Item1, parsingContext));
                        str = split.Item2;
                    }
                    else
                    {
                        Tuple<string, string> split = IsolateFirstElem(str);
                        variables.Add(ParseValue(split.Item1, parsingContext));
                        str = split.Item2;
                    }
                }
                return [.. variables];
            }
            else if (str.Length > 2 && str[0] == '[' && str[^1] == ']')
            {
                str = str[1..^1];
                if (str.Length > 0 && str[^1] == ' ')
                    str = str[..^1];
                if (str.Length > 0 && str[0] == ' ')
                    str = str[1..];
                if (str[0] == '[')
                {
                    List<object[]> variables = [];
                    while (!string.IsNullOrEmpty(str))
                    {
                        Tuple<string, string> split = NextElement(str, '[', ']');
                        variables.Add(ParseValue(split.Item1, parsingContext));
                        str = split.Item2;
                    }
                    return [variables];
                }
                else if (str[0] == '{')
                {
                    List<object[]> variables = [];
                    while (!string.IsNullOrEmpty(str))
                    {
                        Tuple<string, string> split = NextElement(str, '{', '}');
                        variables.Add(ParseValue(split.Item1, parsingContext));
                        str = split.Item2;
                    }
                    return [variables];
                }
                else
                {
                    List<object[]> variables = [];
                    string[] elements = Shell.Helper.Split(str, ',');
                    foreach (string element in elements)
                        variables.Add(ParseValue(element, parsingContext));
                    return [variables];
                }
            }
            else if (str.Length > 2 && str[0] == '"' && str[^1] == '"')
                return [str[1..^1]];
            else if (str.Length > 2 && str[0] == '\'' && str[^1] == '\'')
            {
                if (str.Length == 3)
                    return [str[1]];
                else
                {
                    parsingContext.RegisterError("Invalid script", string.Format("Invalid char : {0}", str));
                    return [];
                }
            }
            else
            {
                if (str == "true")
                    return [true];
                else if (str == "false")
                    return [false];
                else if (str.Contains('.'))
                {
                    if (double.TryParse(str, out double value))
                    {
                        if (value >= float.MinValue && value <= float.MaxValue)
                            return [(float)value];
                        else
                            return [value];
                    }
                    parsingContext.RegisterError("Invalid script", string.Format("Cannot parse float value : {0}", str));
                    return [];
                }
                else
                {
                    if (str[0] == '-')
                    {
                        if (long.TryParse(str, out long value))
                        {
                            if (value >= sbyte.MinValue && value <= sbyte.MaxValue)
                                return [(sbyte)value];
                            else if (value >= short.MinValue && value <= short.MaxValue)
                                return [(short)value];
                            else if (value >= int.MinValue && value <= int.MaxValue)
                                return [(int)value];
                            else
                                return [value];
                        }
                    }
                    else
                    {
                        if (ulong.TryParse(str, out ulong value))
                        {
                            if (value >= byte.MinValue && value <= byte.MaxValue)
                                return [(byte)value];
                            else if (value >= ushort.MinValue && value <= ushort.MaxValue)
                                return [(ushort)value];
                            else if (value >= uint.MinValue && value <= uint.MaxValue)
                                return [(uint)value];
                            else
                                return [value];
                        }
                    }
                }
            }
            return [str]; //We consider it a string not delimited as some split can remove " from strings
        }

        private void LoadStructure(string objectTypeName, string structContent, OldEnvironment env, ParsingContext parsingContext, CommentAndTags commentAndTags)
        {
            if (!string.IsNullOrEmpty(objectTypeName))
            {
                OperationResult<Tuple<string, int[]>> tupleResult = ExtractTypeName(objectTypeName, parsingContext);
                if (!tupleResult)
                {
                    parsingContext.RegisterError(tupleResult.Error, tupleResult.Description);
                    return;
                }
                string templateName = tupleResult.Result!.Item1;
                OperationResult<TypeInfo> objectTypeTypeInfo = TypeInfo.ParseStr(templateName, parsingContext.ConversionTable);
                if (!objectTypeTypeInfo)
                {
                    parsingContext.RegisterError(objectTypeTypeInfo.Error, objectTypeTypeInfo.Description);
                    return;
                }
                TypeInfo typeInfo = objectTypeTypeInfo.Result!;
                TypeInfo realTypeInfo = new(typeInfo.IsConst, parsingContext.Namespaces, typeInfo.ID, typeInfo.TemplateTypes, typeInfo.IsArray);
                ObjectType structDefinition = new(realTypeInfo);
                TypeDefinition typeDefinition = new(new Signature(parsingContext.Namespaces, realTypeInfo.ID), tupleResult.Result!.Item2);
                parsingContext.PushTypeDefinition(typeDefinition, commentAndTags.Tags, commentAndTags.CommentIDs);
                env.AddTypeDefinition(typeDefinition);
                while (!string.IsNullOrEmpty(structContent))
                {
                    Tuple<string, string> structAttribute = NextInstruction(structContent, out bool foundAttribute);
                    if (!foundAttribute)
                    {
                        parsingContext.RegisterError("Invalid script", string.Format("Bad structure definition for {0}", templateName));
                        return;
                    }
                    string attribute = structAttribute.Item1;
                    CommentAndTags? attributeCommentAndTags = LoadCommentAndTags(ref attribute, parsingContext);
                    if (parsingContext.HasErrors)
                        return;
                    string[] parameterParts = SplitParameter(attribute, out bool _, parsingContext);
                    if (parsingContext.HasErrors)
                        return;
                    if (parameterParts.Length != 2 && parameterParts.Length != 3)
                    {
                        parsingContext.RegisterError("Invalid script", string.Format("Bad structure definition for {0}", templateName));
                        return;
                    }
                    OperationResult<TypeInfo> attributeTypeInfoResult = TypeInfo.ParseStr(parameterParts[0], parsingContext.ConversionTable);
                    if (!attributeTypeInfoResult)
                    {
                        parsingContext.RegisterError(attributeTypeInfoResult.Error, attributeTypeInfoResult.Description);
                        return;
                    }
                    TypeInfo attributeTypeInfo = attributeTypeInfoResult.Result!;
                    int nameID = parsingContext.PushName(parameterParts[1]);
                    object[]? value = (parameterParts.Length == 3) ? ParseValue(parameterParts[2], parsingContext) : null;
                    ATypeInstance? parameterType = env.Instantiate(attributeTypeInfo);
                    if (parameterType != null)
                    {
                        if (parameterType is VoidType)
                        {
                            parsingContext.RegisterError("Invalid script", "Parameter type cannot be void");
                            return;
                        }
                        typeDefinition.AddAttribute(attributeTypeInfo, attributeCommentAndTags!.Tags, attributeCommentAndTags!.CommentIDs, nameID, value);
                        if (value != null)
                            structDefinition.AddAttribute(new Parameter(parameterType, attributeTypeInfo.IsConst, nameID, parameterType.InternalConvert(value)));
                        else
                            structDefinition.AddAttribute(new Parameter(parameterType, attributeTypeInfo.IsConst, nameID, null));
                    }
                    else
                        typeDefinition.AddTemplateAttribute(attributeTypeInfo, attributeCommentAndTags!.Tags, attributeCommentAndTags!.CommentIDs, nameID, value);
                    structContent = structAttribute.Item2;
                }

                if (typeDefinition.Templates.Length == 0)
                {
                    env.AddType(structDefinition);
                    parsingContext.PushType(structDefinition);
                }
            }
            else
            {
                parsingContext.RegisterError("Invalid script", "Structure has no name");
                return;
            }
        }

        private void LoadNamespaceContent(OldEnvironment @namespace, string str, ParsingContext parsingContext)
        {
            while (!string.IsNullOrEmpty(str))
            {
                CommentAndTags? commentAndTags = LoadCommentAndTags(ref str, parsingContext);
                if (parsingContext.HasErrors)
                    return;

                if (!string.IsNullOrEmpty(str))
                {
                    if (str.StartsWith("fct "))
                    {
                        //TODO : Handle comments and tags
                        Tuple<string, string, string> signatureSplit = IsolateScope(str, '(', ')', out bool parametersFound);
                        if (!parametersFound)
                        {
                            parsingContext.RegisterError("Misformated function signature string", "Bad parenthesis");
                            return;
                        }
                        str = signatureSplit.Item3;
                        //TODO
                        FunctionSignature? functionSignatureResult = ParseFunctionSignature(signatureSplit.Item2, signatureSplit.Item1[4..], @namespace, parsingContext, out string functionSignatureName);
                        if (parsingContext.HasErrors)
                            return;
                        FunctionSignature functionSignature = functionSignatureResult!;
                        Tuple<string, string, string> scoped = IsolateScope(str, '{', '}', out bool found);
                        if (!found)
                        {
                            parsingContext.RegisterError("Invalid script", "Bad function definition");
                            return;
                        }
                        str = scoped.Item3;
                        Function function = new(functionSignature);
                        //TODO
                        List<AInstruction> functionBody = FunctionLoadBody(string.Format("Invalid function {0}", functionSignatureName), scoped.Item2, parsingContext);
                        if (parsingContext.HasErrors)
                            return;
                        function.AddInstructions(functionBody);
                        if (!@namespace.AddFunction(function))
                        {
                            parsingContext.RegisterError("Invalid script", string.Format("Function {0} already exist", functionSignatureName));
                            return;
                        }
                    }
                    else if (str.StartsWith("struct "))
                    {
                        Tuple<string, string, string> scoped = IsolateScope(str, '{', '}', out bool found);
                        if (!found)
                        {
                            parsingContext.RegisterError("Invalid script", "Bad structure definition");
                            return;
                        }
                        LoadStructure(scoped.Item1[7..], scoped.Item2, @namespace, parsingContext, commentAndTags!);
                        if (parsingContext.HasErrors)
                            return;
                        str = scoped.Item3;
                    }
                    else if (str.StartsWith("namespace "))
                    {
                        Tuple<string, string, string> scoped = IsolateScope(str, '{', '}', out bool found);
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
                        AInstruction? instruction = LoadInstruction("Invalid script instructions", ref str, parsingContext);
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

        private void LoadNamespace(string namespaceName, string namespaceContent, OldEnvironment env, ParsingContext parsingContext)
        {
            Namespace? parent = (env is Namespace namespc) ? namespc : null;
            Namespace @namespace = new(parsingContext.PushName(namespaceName), parent);
            LoadNamespaceContent(@namespace, namespaceContent, parsingContext);
            if (parsingContext.HasErrors)
                return;
            if (!env.AddNamespace(@namespace))
            {
                parsingContext.RegisterError("Invalid script", string.Format("Namespace {0} already exist", ScriptWriter.GenerateNamespaceName(parsingContext.Namespaces, parsingContext.ConversionTable)));
                return;
            }
            return;
        }

        private List<AComment> TrimComments(ref string str, ParsingContext parsingContext)
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

        public ParserResult ParseScript(string str, Environment env)
        {
            ParsingContext parsingContext = new();
            List<AComment> comments = TrimComments(ref str, parsingContext);
            if (parsingContext.HasErrors)
                return new(parsingContext.Error);
            int i = 0;
            Shell.Helper.TrimCommand(ref str);
            Script script = new();
            //TODO Parse imports/include
            LoadNamespaceContent(script, str, parsingContext);
            foreach (AComment comment in comments)
            {
                Console.WriteLine("===== Comment {0} =====", i++);
                Console.WriteLine(comment);
            }
            return parsingContext.CreateParserResult(script, comments);
        }

        public ParserResult ParseScript(string str) => ParseScript(str, new Environment());
    }
}
