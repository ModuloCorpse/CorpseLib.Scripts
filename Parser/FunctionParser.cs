using CorpseLib.Scripts.Instructions;
using CorpseLib.Scripts.Parser.Instruction;
using CorpseLib.Scripts.Type;
using System.Text;
using static CorpseLib.Scripts.Parser.CommentAndTagParser;

namespace CorpseLib.Scripts.Parser
{
    internal static class FunctionParser
    {
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

        private static FunctionSignature? ParseFunctionSignature(string parametersStr, string signatureStr, ParsingContext parsingContext, out string functionName)
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
                        parsingContext.RegisterError("Misformated function signature string", $"Bad signature : {parametersStr}");
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
                    Parameter? result = ParameterParser.ParseParameter(parametersStrArr[i], parsingContext);
                    if (parsingContext.HasErrors || result == null)
                        return null;
                    bool isOptimized = false;
                    bool isConst = result.IsConst;
                    bool isRef = result.IsRef;

                    if (result.IsStatic)
                    {
                        isOptimized = true;
                        parsingContext.RegisterWarning("Static parameter in function signature", $"Static parameter has been optimized without staticness in {functionName}");
                    }
                    if (isConst && !isRef)
                    {
                        isRef = true;
                        isOptimized = true;
                        parsingContext.RegisterWarning("Const without reference in function signature", $"Const parameter has been optimized with a reference in {functionName}");
                    }
                    if (isOptimized)
                        parameters.Add(new Parameter(new ParameterType(result.TypeID, false, isConst, isRef, result.ArrayCount), result.ID, result.DefaultValue));
                    else
                        parameters.Add(result);
                }
            }

            OperationResult<TypeInfo> returnTypeInfo = TypeInfo.ParseStr(returnTypeStr, parsingContext);
            if (!returnTypeInfo)
            {
                parsingContext.RegisterError(returnTypeInfo.Error, returnTypeInfo.Description);
                return null;
            }
            ParameterType? returnType = parsingContext.Instantiate(returnTypeInfo.Result!);
            if (returnType == null)
            {
                parsingContext.RegisterError("Misformated function signature string", $"Unknown return type : {returnTypeStr}");
                return null;
            }
            return new(returnType, parsingContext.PushName(functionName), [.. parameters]);
        }

        private static Tuple<string, string> ParseKeyword(string errorMessage, string keyword, ref string str, bool shouldHaveCondition, ParsingContext parsingContext)
        {
            str = str[(keyword.Length)..];
            while (str.Length > 0 && str[0] == ' ')
                str = str[1..];
            string condition = string.Empty;
            Tuple<string, string, string> tuple;
            bool found;
            if (shouldHaveCondition)
            {
                tuple = ParserHelper.IsolateScope(str, '(', ')', out found);
                if (found)
                {
                    condition = tuple.Item2;
                    str = tuple.Item3;
                }
                else
                {
                    parsingContext.RegisterError(errorMessage, $"Bad condition in {keyword}");
                    return new(string.Empty, string.Empty);
                }
            }
            if (str.Length == 0)
            {
                parsingContext.RegisterError(errorMessage, $"Empty body in {keyword}");
                return new(string.Empty, string.Empty);
            }
            if (str[0] == '{')
            {
                tuple = ParserHelper.IsolateScope(str, '{', '}', out found);
                if (!found)
                {
                    parsingContext.RegisterError(errorMessage, $"Bad body in {keyword}");
                    return new(string.Empty, string.Empty);
                }
                string body = tuple.Item2;
                if (string.IsNullOrWhiteSpace(body))
                {
                    parsingContext.RegisterError(errorMessage, $"Empty body in {keyword}");
                    return new(string.Empty, string.Empty);
                }
                str = tuple.Item3;
                return new(condition, body);
            }
            else
            {
                Tuple<string, string> instruction = ParserHelper.NextInstruction(str, out found);
                if (!found)
                {
                    parsingContext.RegisterError(errorMessage, $"Bad body in {keyword}");
                    return new(string.Empty, string.Empty);
                }
                string body = instruction.Item1 + ';';
                if (string.IsNullOrWhiteSpace(body))
                {
                    parsingContext.RegisterError(errorMessage, $"Empty body in {keyword}");
                    return new(string.Empty, string.Empty);
                }
                str = instruction.Item2;
                return new(condition, body);
            }
        }

        private static Condition ParseCondition(string condition)
        {
            //TODO
            return new Condition(condition);
        }

        private static IfInstruction? LoadIf(string errorMessage, ref string str, ParsingContext parsingContext)
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

        internal static AInstruction? LoadInstruction(string errorMessage, ref string body, ParsingContext parsingContext)
        {
            CommentAndTags? commentAndTags = CommentAndTagParser.LoadCommentAndTags(ref body, parsingContext);
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
                CommentAndTags? doWhileCommentAndTags = CommentAndTagParser.LoadCommentAndTags(ref body, parsingContext);
                //TODO : Handle comments and tags
                if (parsingContext.HasErrors)
                    return null;
                if (body.StartsWith("while"))
                {
                    Tuple<string, string, string> tuple = ParserHelper.IsolateScope(body, '(', ')', out bool found);
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
                Tuple<string, string> instructionParams = ParserHelper.NextInstruction(body, out bool found);
                if (!found)
                {
                    parsingContext.RegisterError(errorMessage, "Bad instruction");
                    return null;
                }
                body = instructionParams.Item2;
                AInstruction? instruction = InstructionParser.Parse(instructionParams.Item1, parsingContext);
                if (parsingContext.HasErrors)
                    return null;
                return instruction;
            }
        }

        private static List<AInstruction> FunctionLoadBody(string errorMessage, string body, ParsingContext parsingContext)
        {
            List<AInstruction> instructions = [];
            while (!string.IsNullOrEmpty(body))
            {
                AInstruction? instruction = LoadInstruction(errorMessage, ref body, parsingContext);
                if (parsingContext.HasErrors)
                    return [];
                if (instruction != null)
                    instructions.Add(instruction);
            }
            return instructions;
        }

        internal static void LoadFunctionContent(CommentAndTags commentAndTags, ref string str, ParsingContext parsingContext)
        {
            //TODO : Handle comments and tags
            Tuple<string, string, string> signatureSplit = ParserHelper.IsolateScope(str, '(', ')', out bool parametersFound);
            if (!parametersFound)
            {
                parsingContext.RegisterError("Misformated function signature string", "Bad parenthesis");
                return;
            }
            str = signatureSplit.Item3;
            //TODO
            FunctionSignature? functionSignatureResult = ParseFunctionSignature(signatureSplit.Item2, signatureSplit.Item1[4..], parsingContext, out string functionSignatureName);
            if (parsingContext.HasErrors)
                return;
            FunctionSignature functionSignature = functionSignatureResult!;
            Tuple<string, string, string> scoped = ParserHelper.IsolateScope(str, '{', '}', out bool found);
            if (!found)
            {
                parsingContext.RegisterError("Invalid script", "Bad function definition");
                return;
            }
            str = scoped.Item3;
            Function function = new(functionSignature);
            //TODO
            List<AInstruction> functionBody = FunctionLoadBody($"Invalid function {functionSignatureName}", scoped.Item2, parsingContext);
            if (parsingContext.HasErrors)
                return;
            function.AddInstructions(functionBody);
            if (!parsingContext.PushFunction(function, commentAndTags.Tags, commentAndTags.CommentIDs))
            {
                parsingContext.RegisterError("Invalid script", $"Function {functionSignatureName} already exist");
                return;
            }
        }
    }
}
