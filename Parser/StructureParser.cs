using CorpseLib.Scripts.Context;
using CorpseLib.Scripts.Type;
using static CorpseLib.Scripts.Parser.CommentAndTagParser;
using System.Text;
using CorpseLib.Scripts.Memories;

namespace CorpseLib.Scripts.Parser
{
    internal static class StructureParser
    {
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
                            return new("Template error", $"Bad template {template}");
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
                        return new("Template error", $"Bad template {template}");
                }
                else if (c == '<')
                {
                    ++templateCount;
                    builder.Append(c);
                }
                else if (c == '>')
                    return new("Template error", $"Bad template {template}");
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

        private static void LoadStructure(string objectTypeName, string structContent, ParsingContext parsingContext, CommentAndTags commentAndTags)
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
                OperationResult<TypeInfo> objectTypeTypeInfo = TypeInfo.ParseStr(templateName, parsingContext);
                if (!objectTypeTypeInfo)
                {
                    parsingContext.RegisterError(objectTypeTypeInfo.Error, objectTypeTypeInfo.Description);
                    return;
                }
                TypeInfo typeInfo = objectTypeTypeInfo.Result!;
                TypeInfo realTypeInfo = new(typeInfo.IsStatic, typeInfo.IsConst, typeInfo.IsRef, parsingContext.Namespaces, typeInfo.ID, typeInfo.TemplateTypes, typeInfo.ArrayCount);
                ObjectType structDefinition = new(realTypeInfo);
                TypeDefinition typeDefinition = new(new Signature(parsingContext.Namespaces, realTypeInfo.ID), tupleResult.Result!.Item2);
                parsingContext.PushTypeDefinition(typeDefinition, commentAndTags.Tags, commentAndTags.CommentIDs);
                while (!string.IsNullOrEmpty(structContent))
                {
                    Tuple<string, string> structAttribute = ParserHelper.NextInstruction(structContent, out bool foundAttribute);
                    if (!foundAttribute)
                    {
                        parsingContext.RegisterError("Invalid script", $"Bad structure definition for {templateName}");
                        return;
                    }
                    string attribute = structAttribute.Item1;
                    CommentAndTags? attributeCommentAndTags = CommentAndTagParser.LoadCommentAndTags(ref attribute, parsingContext);
                    if (parsingContext.HasErrors)
                        return;
                    string[] parameterParts = ParameterParser.SplitParameter(attribute, parsingContext);
                    if (parsingContext.HasErrors)
                        return;
                    if (parameterParts.Length != 2 && parameterParts.Length != 3)
                    {
                        parsingContext.RegisterError("Invalid script", $"Bad structure definition for {templateName}");
                        return;
                    }
                    OperationResult<TypeInfo> attributeTypeInfoResult = TypeInfo.ParseStr(parameterParts[0], parsingContext);
                    if (!attributeTypeInfoResult)
                    {
                        parsingContext.RegisterError(attributeTypeInfoResult.Error, attributeTypeInfoResult.Description);
                        return;
                    }
                    TypeInfo attributeTypeInfo = attributeTypeInfoResult.Result!;
                    int nameID = parsingContext.PushName(parameterParts[1]);
                    IMemoryValue? value = (parameterParts.Length == 3) ? ValueParser.ParseValue(parameterParts[2], parsingContext) : null;
                    ParameterType? parameterType = parsingContext.Instantiate(attributeTypeInfo);
                    if (parameterType != null)
                    {
                        if (parameterType.TypeID == 0)
                        {
                            parsingContext.RegisterError("Invalid script", "Parameter type cannot be void");
                            return;
                        }
                        typeDefinition.AddAttribute(attributeTypeInfo, attributeCommentAndTags!.Tags, attributeCommentAndTags!.CommentIDs, nameID, value);
                        if (value != null)
                            structDefinition.AddAttribute(new Parameter(parameterType, nameID, value));
                        else
                            structDefinition.AddAttribute(new Parameter(parameterType, nameID, null));
                    }
                    else
                        typeDefinition.AddTemplateAttribute(attributeTypeInfo, attributeCommentAndTags!.Tags, attributeCommentAndTags!.CommentIDs, nameID, value);
                    structContent = structAttribute.Item2;
                }

                if (typeDefinition.Templates.Length == 0)
                    parsingContext.PushType(structDefinition);
            }
            else
            {
                parsingContext.RegisterError("Invalid script", "Structure has no name");
                return;
            }
        }

        internal static void LoadStructureContent(CommentAndTags commentAndTags, ref string str, ParsingContext parsingContext)
        {
            Tuple<string, string, string> scoped = ParserHelper.IsolateScope(str, '{', '}', out bool found);
            if (!found)
            {
                parsingContext.RegisterError("Invalid script", "Bad structure definition");
                return;
            }
            LoadStructure(scoped.Item1[7..], scoped.Item2, parsingContext, commentAndTags);
            if (parsingContext.HasErrors)
                return;
            str = scoped.Item3;
        }
    }
}
