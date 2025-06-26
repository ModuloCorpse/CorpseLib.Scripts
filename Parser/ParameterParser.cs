using CorpseLib.Scripts.Type.Primitive;
using CorpseLib.Scripts.Type;

namespace CorpseLib.Scripts.Parser
{
    internal static class ParameterParser
    {
        internal static string[] SplitParameter(string parameter, out bool isConst, ParsingContext parsingContext)
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

        internal static Parameter? ParseParameter(string parameter, ParsingContext parsingContext)
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
            ATypeInstance? parameterType = parsingContext.Instantiate(typeInfo.Result!);
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
                value = ValueParser.ParseValue(parameterParts[2], parsingContext);
            if (value != null)
                return new Parameter(parameterType, isConst, parsingContext.PushName(parameterParts[1]), parameterType.InternalConvert(value));
            else
                return new Parameter(parameterType, isConst, parsingContext.PushName(parameterParts[1]));
        }
    }
}
