using CorpseLib.Scripts.Type.Primitive;
using CorpseLib.Scripts.Type;
using System.Text;
using CorpseLib.Scripts.Instruction;

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
        private readonly List<string> m_Warnings = [];
        private string m_Error = string.Empty;
        private bool m_HasErrors = false;

        public List<string> Warnings => m_Warnings;
        public string Error => m_Error;
        public bool HasErrors => m_HasErrors;

        private void RegisterWarning(string warning, string description) => m_Warnings.Add(string.Format("WARNING : {0} : {1}", warning, description));

        private void RegisterError(string error, string description)
        {
            m_Error = string.Format("ERROR : {0} : {1}", error, description);
            m_HasErrors = true;
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

        private string[] SplitParameter(string parameter, out bool isConst)
        {
            isConst = false;
            if (string.IsNullOrEmpty(parameter))
            {
                RegisterError("Misformated parameter string", "Empty parameter");
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
                    RegisterError("Misformated parameter string", "Empty value");
                    return [];
                }
                string toSplit = parameter[..assignationIndex];
                if (toSplit.Length > 0 && toSplit[^1] == ' ')
                    toSplit = toSplit[..^1];
                string[] parameterParts = Shell.Helper.Split(toSplit, ' ');
                if (parameterParts.Length != 2)
                {
                    RegisterError("Misformated parameter string", "Parameter should be [type] [name] = [value]");
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
                    RegisterError("Misformated parameter string", "Parameter should be [type] [name]");
                    return [];
                }
                ret.AddRange(parameterParts);
            }
            return [.. ret];
        }

        private static OperationResult<int[]> SplitTemplate(string template, ConversionTable conversionTable)
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
                        result.Add(conversionTable.PushName(builder.ToString()));
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
                result.Add(conversionTable.PushName(builder.ToString()));
            return new([.. result]);
        }

        private static OperationResult<Tuple<int, int[]>> ExtractTypeName(string template, ConversionTable conversionTable)
        {
            int templateIdx = template.IndexOf('<');
            if (templateIdx >= 0)
            {
                OperationResult<int[]> templatesResult = SplitTemplate(template[(templateIdx + 1)..^1], conversionTable);
                if (templatesResult && templatesResult.Result != null)
                {
                    int[] templateTypesIDs = templatesResult.Result;
                    string templateName = template[..templateIdx];
                    return new(new(conversionTable.PushName(templateName), templateTypesIDs));
                }
                return new(templatesResult.Error, templatesResult.Description);
            }
            return new(new(conversionTable.PushName(template), []));
        }

        private Parameter? ParseParameter(string parameter, Environment env, ConversionTable conversionTable)
        {
            if (string.IsNullOrEmpty(parameter))
            {
                RegisterError("Misformated parameter string", "Empty parameter");
                return null;
            }
            //TODO : Do not loose constness (Maybe use a TypeInfo)
            string[] parameterParts = SplitParameter(parameter, out bool isConst);
            if (m_HasErrors)
                return null;
            ATypeInstance? parameterType = env.Instantiate(parameterParts[0], conversionTable);
            if (parameterType == null)
            {
                RegisterError("Unknown parameter type", parameterParts[0]);
                return null;
            }
            if (parameterType is VoidType)
            {
                RegisterError("Invalid script", "Parameter type cannot be void");
                return null;
            }
            if (parameterParts.Length == 3)
                return new Parameter(parameterType, isConst, conversionTable.PushName(parameterParts[1]), parameterType.InternalParse(parameterParts[2]));
            else
                return new Parameter(parameterType, isConst, conversionTable.PushName(parameterParts[1]));
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

        private FunctionSignature? ParseFunctionSignature(string parametersStr, string signatureStr, Environment env, ConversionTable conversionTable, out string functionName)
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
                    RegisterError("Misformated function signature string", string.Format("Bad signature : {0}", parametersStr));
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
                    Parameter? result = ParseParameter(parametersStrArr[i], env, conversionTable);
                    if (m_HasErrors)
                        return null;
                    parameters.Add(result!);
                }
            }
            ATypeInstance? returnType = env.Instantiate(returnTypeStr, conversionTable);
            if (returnType == null)
            {
                RegisterError("Misformated function signature string", string.Format("Unknown return type : {0}", returnTypeStr));
                return null;
            }
            return new(returnType, conversionTable.PushName(functionName), [.. parameters]);
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

        private Tuple<string, string> ParseKeyword(string functionName, string keyword, ref string str, bool shouldHaveCondition)
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
                    RegisterError(string.Format("Invalid function {0}", functionName), string.Format("Bad condition in {0}", keyword));
                    return new(string.Empty, string.Empty);
                }
            }
            if (str.Length == 0)
            {
                RegisterError(string.Format("Invalid function {0}", functionName), string.Format("Empty body in {0}", keyword));
                return new(string.Empty, string.Empty);
            }
            if (str[0] == '{')
            {
                tuple = IsolateScope(str, '{', '}', out found);
                if (!found)
                {
                    RegisterError(string.Format("Invalid function {0}", functionName), string.Format("Bad body in {0}", keyword));
                    return new(string.Empty, string.Empty);
                }
                string body = tuple.Item2;
                if (string.IsNullOrWhiteSpace(body))
                {
                    RegisterError(string.Format("Invalid function {0}", functionName), string.Format("Empty body in {0}", keyword));
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
                    RegisterError(string.Format("Invalid function {0}", functionName), string.Format("Bad body in {0}", keyword));
                    return new(string.Empty, string.Empty);
                }
                string body = instruction.Item1 + ';';
                if (string.IsNullOrWhiteSpace(body))
                {
                    RegisterError(string.Format("Invalid function {0}", functionName), string.Format("Empty body in {0}", keyword));
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

        private IfInstruction? LoadIf(string functionName, ref string str)
        {
            Tuple<string, string> keyWordParams = ParseKeyword(functionName, "if", ref str, true);
            if (m_HasErrors)
                return null;
            List<AInstruction> ifResult = FunctionLoadBody(functionName, keyWordParams.Item2);
            if (m_HasErrors)
                return null;
            IfInstruction ifInstruction = new(ParseCondition(keyWordParams.Item1), ifResult);
            while (str.StartsWith("elif"))
            {
                keyWordParams = ParseKeyword(functionName, "elif", ref str, true);
                if (m_HasErrors)
                    return null;
                List<AInstruction> elseIfResult = FunctionLoadBody(functionName, keyWordParams.Item2);
                if (m_HasErrors)
                    return null;
                ifInstruction.AddElif(ParseCondition(keyWordParams.Item1), elseIfResult);
            }
            if (str.StartsWith("else"))
            {
                keyWordParams = ParseKeyword(functionName, "else", ref str, false);
                if (m_HasErrors)
                    return null;
                List<AInstruction> elseResult = FunctionLoadBody(functionName, keyWordParams.Item2);
                if (m_HasErrors)
                    return null;
                ifInstruction.SetElseBody(elseResult);
            }
            return ifInstruction;
        }

        private List<AInstruction> FunctionLoadBody(string functionName, string body)
        {
            List<AInstruction> instructions = [];
            while (!string.IsNullOrEmpty(body))
            {
                if (body.StartsWith("else"))
                {
                    RegisterError(string.Format("Invalid function {0}", functionName), "else outside of if");
                    return [];
                }
                else if (body.StartsWith("if"))
                {
                    AInstruction? ifInstruction = LoadIf(functionName, ref body);
                    if (m_HasErrors)
                        return [];
                    instructions.Add(ifInstruction!);
                }
                else if (body.StartsWith("while"))
                {
                    Tuple<string, string> keyWordParams = ParseKeyword(functionName, "while", ref body, true);
                    if (m_HasErrors)
                        return [];
                    List<AInstruction> whileBody = FunctionLoadBody(functionName, keyWordParams.Item2);
                    if (m_HasErrors)
                        return [];
                    instructions.Add(new WhileInstruction(ParseCondition(keyWordParams.Item1), whileBody));
                }
                else if (body.StartsWith("do"))
                {
                    Tuple<string, string> keyWordParams = ParseKeyword(functionName, "do", ref body, false);
                    if (m_HasErrors)
                        return [];
                    List<AInstruction> doBody = FunctionLoadBody(functionName, keyWordParams.Item2);
                    if (m_HasErrors)
                        return [];
                    if (body.StartsWith("while"))
                    {
                        Tuple<string, string, string> tuple = IsolateScope(body, '(', ')', out bool found);
                        if (found)
                        {
                            body = tuple.Item3;
                            instructions.Add(new DoWhileInstruction(ParseCondition(tuple.Item2), doBody));
                        }
                        else
                        {
                            RegisterError(string.Format("Invalid function {0}", functionName), "Bad condition in do");
                            return [];
                        }
                    }
                    else
                    {
                        RegisterError(string.Format("Invalid function {0}", functionName), "do without while");
                        return [];
                    }
                }
                else if (body.StartsWith("for"))
                {
                    Tuple<string, string> keyWordParams = ParseKeyword(functionName, "for", ref body, true);
                    if (m_HasErrors)
                        return [];
                    List<AInstruction> forBody = FunctionLoadBody(functionName, keyWordParams.Item2);
                    if (m_HasErrors)
                        return [];
                    //TODO
                    RegisterError("Not Supported Yet", "For not supported yet, please use while for now");
                    return [];
                }
                else
                {
                    Tuple<string, string> instructionParams = NextInstruction(body, out bool found);
                    if (!found)
                    {
                        RegisterError(string.Format("Invalid function {0}", functionName), "Bad instruction");
                        return [];
                    }
                    body = instructionParams.Item2;
                    AInstruction? instruction = ParseInstruction(instructionParams.Item1);
                    if (m_HasErrors)
                        return [];
                    instructions.Add(instruction!);
                }
            }
            return instructions;
        }

        private void LoadStructure(string objectTypeName, string structContent, Environment env, ConversionTable conversionTable)
        {
            if (!string.IsNullOrEmpty(objectTypeName))
            {
                if (objectTypeName.Contains('<'))
                {
                    OperationResult<Tuple<int, int[]>> tupleResult = ExtractTypeName(objectTypeName, conversionTable);
                    if (!tupleResult)
                    {
                        RegisterError(tupleResult.Error, tupleResult.Description);
                        return;
                    }
                    int templateName = tupleResult.Result!.Item1;
                    int[] templateTypes = tupleResult.Result!.Item2;
                    if (templateTypes.Length == 0)
                    {
                        RegisterError("Invalid template", "No template given");
                        return;
                    }
                    TemplateDefinition templateDefinition = new(templateName, templateTypes);
                    env.AddTemplateDefinition(templateDefinition);
                    while (!string.IsNullOrEmpty(structContent))
                    {
                        Tuple<string, string> structAttribute = NextInstruction(structContent, out bool foundAttribute);
                        if (!foundAttribute)
                        {
                            RegisterError("Invalid script", string.Format("Bad structure definition for {0}", templateName));
                            return;
                        }
                        string[] parameterParts = SplitParameter(structAttribute.Item1, out bool isConst);
                        if (m_HasErrors)
                            return;
                        ATypeInstance? parameterType = env.Instantiate(parameterParts[0], conversionTable);
                        if (parameterType != null)
                        {
                            if (parameterType is VoidType)
                            {
                                RegisterError("Invalid script", "Parameter type cannot be void");
                                return;
                            }
                            if (parameterParts.Length == 3)
                                templateDefinition.AddAttributeDefinition(new Parameter(parameterType, isConst, conversionTable.PushName(parameterParts[1]), parameterType.InternalParse(parameterParts[2])));
                            else
                                templateDefinition.AddAttributeDefinition(new Parameter(parameterType, isConst, conversionTable.PushName(parameterParts[1])));
                        }
                        else
                        {
                            if (parameterParts.Length == 2 || parameterParts.Length == 3)
                            {
                                OperationResult<TypeInfo> attributeTypeInfo = TypeInfo.ParseStr(parameterParts[0], conversionTable);
                                if (!attributeTypeInfo)
                                {
                                    RegisterError(attributeTypeInfo.Error, attributeTypeInfo.Description);
                                    return;
                                }
                                int nameID = conversionTable.PushName(parameterParts[1]);
                                string? value = (parameterParts.Length == 3) ? parameterParts[2] : null;
                                templateDefinition.AddAttributeDefinition(attributeTypeInfo.Result!, nameID, value);
                            }
                            else
                            {
                                RegisterError("Invalid script", string.Format("Bad structure definition for {0}", templateName));
                                return;
                            }
                        }
                        structContent = structAttribute.Item2;
                    }
                }
                else
                {
                    OperationResult<TypeInfo> objectTypeTypeInfo = TypeInfo.ParseStr(objectTypeName, conversionTable);
                    if (!objectTypeTypeInfo)
                    {
                        RegisterError(objectTypeTypeInfo.Error, objectTypeTypeInfo.Description);
                        return;
                    }
                    TypeInfo typeInfo = objectTypeTypeInfo.Result!;
                    int[] namespaceIDs = (env is Namespace @namespace) ? @namespace.IDS : [];
                    TypeInfo realTypeInfo = new(typeInfo.IsConst, namespaceIDs, typeInfo.ID, typeInfo.TemplateTypes, typeInfo.IsArray);
                    ObjectType structDefinition = new(realTypeInfo);
                    env.AddType(structDefinition);
                    while (!string.IsNullOrEmpty(structContent))
                    {
                        Tuple<string, string> structAttribute = NextInstruction(structContent, out bool foundAttribute);
                        if (!foundAttribute)
                        {
                            RegisterError("Invalid script", string.Format("Bad structure definition for {0}", objectTypeName));
                            return;
                        }
                        Parameter? result = ParseParameter(structAttribute.Item1, env, conversionTable);
                        if (m_HasErrors)
                            return;
                        structDefinition.AddAttribute(result!);
                        structContent = structAttribute.Item2;
                    }
                }
            }
            else
            {
                RegisterError("Invalid script", "Structure has no name");
                return;
            }
        }

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

        private void LoadNamespaceContent(Environment @namespace, string str, ConversionTable conversionTable)
        {
            while (!string.IsNullOrEmpty(str))
            {
                if (str.StartsWith('['))
                {
                    Tuple<string, string, string> tagsTuple = IsolateScope(str, '[', ']', out bool tagFound);
                    if (tagFound)
                    {
                        string[] tags = SplitTags(tagsTuple.Item2);
                        //TODO Load tag
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
                        str = signatureSplit.Item3;
                        //TODO
                        FunctionSignature? functionSignatureResult = ParseFunctionSignature(signatureSplit.Item2, signatureSplit.Item1[4..], @namespace, conversionTable, out string functionSignatureName);
                        if (m_HasErrors)
                            return;
                        FunctionSignature functionSignature = functionSignatureResult!;
                        Tuple<string, string, string> scoped = IsolateScope(str, '{', '}', out bool found);
                        if (!found)
                        {
                            RegisterError("Invalid script", "Bad function definition");
                            return;
                        }
                        str = scoped.Item3;
                        Function function = new(functionSignature);
                        //TODO
                        List<AInstruction> functionBody = FunctionLoadBody(functionSignatureName, scoped.Item2);
                        if (m_HasErrors)
                            return;
                        function.AddInstructions(functionBody);
                        if (!@namespace.AddFunction(function))
                        {
                            RegisterError("Invalid script", string.Format("Function {0} already exist", functionSignatureName));
                            return;
                        }
                    }
                    else if (str.StartsWith("struct "))
                    {
                        Tuple<string, string, string> scoped = IsolateScope(str, '{', '}', out bool found);
                        if (!found)
                        {
                            RegisterError("Invalid script", "Bad structure definition");
                            return;
                        }
                        LoadStructure(scoped.Item1[7..], scoped.Item2, @namespace, conversionTable);
                        if (m_HasErrors)
                            return;
                        str = scoped.Item3;
                    }
                    else if (str.StartsWith("namespace "))
                    {
                        Tuple<string, string, string> scoped = IsolateScope(str, '{', '}', out bool found);
                        if (!found)
                        {
                            RegisterError("Invalid script", "Bad namespace definition");
                            return;
                        }
                        LoadNamespace(scoped.Item1[10..], scoped.Item2, @namespace, conversionTable);
                        if (m_HasErrors)
                            return;
                        str = scoped.Item3;
                    }
                    else
                    {
                        Tuple<string, string> instruction = NextInstruction(str, out bool found);
                        if (!found)
                        {
                            RegisterError("Invalid script", "Bad global definition");
                            return;
                        }
                        Parameter? global = ParseParameter(instruction.Item1, @namespace, conversionTable);
                        if (m_HasErrors)
                            return;
                        if (!@namespace.AddGlobal(global!))
                        {
                            RegisterError("Invalid script", string.Format("Variable {0} already exist", global!.ID));
                            return;
                        }
                        str = instruction.Item2;
                    }
                }
                else
                {
                    RegisterError("Invalid script", "Tags not associated to anything");
                    return;
                }
            }
        }

        private void LoadNamespace(string namespaceName, string namespaceContent, Environment env, ConversionTable conversionTable)
        {
            Namespace? parent = (env is Namespace namespc) ? namespc : null;
            Namespace @namespace = new(conversionTable.PushName(namespaceName), parent);
            LoadNamespaceContent(@namespace, namespaceContent, conversionTable);
            if (m_HasErrors)
                return;
            if (!env.AddNamespace(@namespace))
            {
                StringBuilder sb = new();
                ScriptWriter.AppendNamespaceName(ref sb, @namespace, conversionTable);
                RegisterError("Invalid script", string.Format("Namespace {0} already exist", sb.ToString()));
                return;
            }
            return;
        }

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
            if (m_HasErrors)
                return null;
            int i = 0;
            foreach (string comment in comments)
            {
                Console.WriteLine("===== Comment {0} =====", i++);
                Console.WriteLine(comment);
            }
            Shell.Helper.TrimCommand(ref str);
            Script script = new();
            //TODO Parse imports/include
            LoadNamespaceContent(script, str, script.ConversionTable);
            if (m_HasErrors)
                return null;
            return script;
        }
    }
}
