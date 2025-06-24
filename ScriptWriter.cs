using CorpseLib.Scripts.Context;
using CorpseLib.Scripts.Instructions;
using CorpseLib.Scripts.Type;
using CorpseLib.Scripts.Type.Primitive;
using System.Text;
using static CorpseLib.Scripts.Context.TypeDefinition;
using static CorpseLib.Scripts.Context.TypeObject;
using Environment = CorpseLib.Scripts.Context.Environment;

namespace CorpseLib.Scripts
{
    public static class ScriptWriter
    {
        public static void AppendValue(ScriptBuilder sb, ATypeInstance typeInstance, object[]? value)
        {
            if (value == null || typeInstance is VoidType)
                return;
            if (value.Length == 0)
            {
                sb.Append("null");
                return;
            }
            else if (typeInstance is ArrayType arrayType)
            {
                sb.Append('[');
                for (int i = 0; i != value.Length; ++i)
                {
                    if (i != 0)
                        sb.Append(',');
                    sb.Append(' ');
                    if (value[i] is Variable var)
                        AppendValue(sb, arrayType.ElementType, var.Values);
                    else
                        throw new ArgumentException("Array is not valid");
                }
                if (value.Length != 0)
                    sb.Append(' ');
                sb.Append(']');
            }
            else if (typeInstance is ObjectType objectType)
            {
                sb.Append('{');
                for (int i = 0; i != value.Length; ++i)
                {
                    if (value[i] is Variable var)
                    {
                        if (!var.IsDefault())
                        {
                            if (i != 0)
                                sb.Append(',');
                            sb.Append(' ');
                            AppendValue(sb, var.Type, var.Values);
                        }
                    }
                    else
                        throw new ArgumentException("Object is not valid");
                }
                if (value.Length != 0)
                    sb.Append(' ');
                sb.Append('}');
            }
            else if (typeInstance is ARawPrimitive && value.Length == 1)
            {
                object tmp = value[0];
                if (typeInstance is StringType)
                {
                    sb.Append('"');
                    sb.Append(tmp);
                    sb.Append('"');
                }
                else if (typeInstance is BoolType && Helper.ChangeType(tmp, out bool b))
                    sb.Append(b ? "true" : "false");
                //Using byte and sbyte to implicitly convert string to char
                else if (typeInstance is CharType && Helper.ChangeType(tmp, out sbyte sB))
                    sb.Append((char)sB);
                else if (typeInstance is UCharType && Helper.ChangeType(tmp, out byte c))
                    sb.Append((char)c);
                else
                    sb.Append(tmp);
            }
        }

        private static void AppendParameter(ScriptBuilder sb, Parameter parameter)
        {
            if (parameter.IsConst)
                sb.Append("const ");
            AppendTypeInstanceName(sb, parameter.Type);
            sb.Append(' ');
            sb.AppendID(parameter.ID);
            if (parameter.DefaultValues != null)
            {
                sb.Append(" = ");
                AppendValue(sb, parameter.Type, parameter.DefaultValues);
            }
        }

        public static void AppendObjectType(ScriptBuilder sb, ObjectType objectType)
        {
            sb.Append("struct ");
            AppendTypeInfo(sb, objectType.TypeInfo);
            sb.Append(" { ");
            foreach (Parameter attribute in objectType.Attributes)
            {
                AppendParameter(sb, attribute);
                sb.Append("; ");
            }
            sb.Append('}');
        }

        public static void AppendTypeInstanceName(ScriptBuilder sb, ATypeInstance typeInstance)
        {
            if (typeInstance is ObjectType objectType)
                AppendTypeInfo(sb, objectType.TypeInfo);
            else if (typeInstance is ArrayType arrayType)
            {
                AppendTypeInstanceName(sb, arrayType.ElementType);
                sb.Append("[]");
            }
            else if (typeInstance is VoidType)
                sb.Append("void");
            else if (typeInstance is BoolType)
                sb.Append("bool");
            else if (typeInstance is ShortType)
                sb.Append("short");
            else if (typeInstance is UShortType)
                sb.Append("ushort");
            else if (typeInstance is CharType)
                sb.Append("char");
            else if (typeInstance is UCharType)
                sb.Append("uchar");
            else if (typeInstance is IntType)
                sb.Append("int");
            else if (typeInstance is UIntType)
                sb.Append("uint");
            else if (typeInstance is LongType)
                sb.Append("long");
            else if (typeInstance is ULongType)
                sb.Append("ulong");
            else if (typeInstance is FloatType)
                sb.Append("float");
            else if (typeInstance is DoubleType)
                sb.Append("double");
            else if (typeInstance is StringType)
                sb.Append("string");
        }

        public static void AppendFunctionSignature(ScriptBuilder sb, FunctionSignature functionSignature)
        {
            sb.Append("fct ");
            if (functionSignature.ReturnType is not VoidType)
            {
                AppendTypeInstanceName(sb, functionSignature.ReturnType);
                sb.Append(' ');
            }
            sb.AppendID(functionSignature.ID);
            sb.Append('(');
            int i = 0;
            foreach (Parameter p in functionSignature.Parameters)
            {
                if (i != 0)
                    sb.Append(", ");
                AppendParameter(sb, p);
                ++i;
            }
            sb.Append(')');
        }

        public static void AppendTypeDefinitionName(ScriptBuilder sb, TypeDefinition typeDefinition)
        {
            sb.AppendID(typeDefinition.Signature.ID);
            int[] templates = typeDefinition.Templates;
            if (templates.Length == 0)
                return;
            sb.Append('<');
            int i = 0;
            foreach (int template in templates)
            {
                if (i != 0)
                    sb.Append(", ");
                sb.AppendID(template);
                ++i;
            }
            sb.Append('>');
        }

        public static void AppendSignature(ScriptBuilder sb, Signature signature)
        {
            foreach (int namespaceID in signature.Namespaces)
            {
                sb.AppendID(namespaceID);
                sb.Append('.');
            }
            sb.AppendID(signature.ID);
        }

        public static void AppendTypeInfo(ScriptBuilder sb, TypeInfo typeInfo)
        {
            if (typeInfo.IsConst)
                sb.Append("const ");
            AppendSignature(sb, typeInfo.Signature);
            if (typeInfo.TemplateTypes.Length > 0)
            {
                sb.Append('<');
                int i = 0;
                foreach (TypeInfo templatedType in typeInfo.TemplateTypes)
                {
                    if (i != 0)
                        sb.Append(", ");
                    AppendTypeInfo(sb, templatedType);
                    ++i;
                }
                sb.Append('>');
            }
            if (typeInfo.IsArray)
                sb.Append("[]");
        }

        private static bool IsObject(object[] value)
        {
            foreach (object obj in value)
            {
                if (obj is not object[])
                    return false;
            }
            return true;
        }

        public static void AppendAnonymousValue(ScriptBuilder sb, object[]? value)
        {
            if (value == null)
                return;
            if (value.Length == 0)
            {
                sb.Append("null");
                return;
            }
            else if (IsObject(value))
            {
                sb.Append('{');
                for (int i = 0; i != value.Length; ++i)
                {
                    object[] var = (value[i] as object[])!;
                    if (i != 0)
                        sb.Append(',');
                    sb.Append(' ');
                    AppendAnonymousValue(sb, var);
                }
                if (value.Length != 0)
                    sb.Append(' ');
                sb.Append('}');
            }
            else if (value.Length == 1)
            {
                object tmp = value[0];
                if (tmp is List<object[]> elements)
                {
                    sb.Append('[');
                    for (int i = 0; i != value.Length; ++i)
                    {
                        object[] var = (value[i] as object[])!;
                        if (i != 0)
                            sb.Append(',');
                        sb.Append(' ');
                        AppendAnonymousValue(sb, var);
                    }
                    if (value.Length != 0)
                        sb.Append(' ');
                    sb.Append(']');
                }
                else if (tmp is string)
                {
                    sb.Append('"');
                    sb.Append(tmp);
                    sb.Append('"');
                }
                else if (tmp is bool && Helper.ChangeType(tmp, out bool b))
                    sb.Append(b ? "true" : "false");
                //Using sbyte to implicitly convert string to char
                else if (tmp is char && Helper.ChangeType(tmp, out sbyte sB))
                    sb.Append((char)sB);
                else
                    sb.Append(tmp);
            }
        }

        public static void AppendTypeDefinition(ScriptBuilder sb, TypeDefinition typeDefinition)
        {
            sb.Append("struct ");
            AppendTypeDefinitionName(sb, typeDefinition);
            sb.AppendLine();
            sb.OpenScope();
            int i = 0;
            foreach (AAttributeDefinition attribute in typeDefinition.Attributes)
            {
                if (i != 0)
                    sb.AppendLine();
                AppendTypeInfo(sb, attribute.TypeInfo);
                sb.Append(' ');
                sb.AppendID(attribute.ID);
                if (attribute.Value != null)
                {
                    sb.Append(" = ");
                    AppendAnonymousValue(sb, attribute.Value);
                }
                sb.Append(';');
                ++i;
            }
            sb.CloseScope();
        }

        private static void AppendCondition(ScriptBuilder sb, Condition condition)
        {
            sb.Append(condition.ConditionStr);
        }

        private static void AppendInstructions(ScriptBuilder sb, AInstruction[] instructions)
        {
            int i = 0;
            foreach (AInstruction instruction in instructions)
            {
                if (i != 0)
                    sb.AppendLine();
                AppendInstruction(sb, instruction);
                i++;
            }
        }

        private static void AppendScopedInstructions(ScriptBuilder sb, ScopedInstructions instructions)
        {
            if (instructions.Count > 1)
                sb.OpenScope();
            else
            {
                sb.Indent();
                sb.AppendLine();
            }
            AppendInstructions(sb, instructions.Instructions);
            if (instructions.Count > 1)
                sb.CloseScope();
            else
                sb.Unindent();
        }

        private static void AppendDoWhile(ScriptBuilder sb, DoWhileInstruction doWhileInstruction)
        {
            sb.Append("do");
            AppendScopedInstructions(sb, doWhileInstruction.Body);
            sb.AppendLine();
            sb.Append("while(");
            AppendCondition(sb, doWhileInstruction.Condition);
            sb.Append(')');
        }

        private static void AppendWhile(ScriptBuilder sb, WhileInstruction whileInstruction)
        {
            sb.Append("while(");
            AppendCondition(sb, whileInstruction.Condition);
            sb.Append(')');
            AppendScopedInstructions(sb, whileInstruction.Body);
        }

        private static void AppendIf(ScriptBuilder sb, IfInstruction ifInstruction)
        {
            sb.Append("if (");
            AppendCondition(sb, ifInstruction.Condition);
            sb.Append(')');
            AppendScopedInstructions(sb, ifInstruction.Body);
            foreach (var elseIf in ifInstruction.Elifs)
            {
                sb.AppendLine();
                sb.Append("elif (");
                AppendCondition(sb, elseIf.Condition);
                sb.Append(')');
                AppendScopedInstructions(sb, elseIf.Body);
            }
            if (ifInstruction.ElseBody.Count > 0)
            {
                sb.AppendLine();
                sb.Append("else");
                AppendScopedInstructions(sb, ifInstruction.ElseBody);
            }
        }

        public static void AppendInstruction(ScriptBuilder sb, AInstruction instruction)
        {
            if (instruction is DebugInstruction debugInstruction)
            {
                sb.Append(debugInstruction.Instruction);
                sb.Append(';');
            }
            else if (instruction is Break)
                sb.Append("break;");
            else if (instruction is Continue)
                sb.Append("continue;");
            else if (instruction is DoWhileInstruction doWhileInstruction)
                AppendDoWhile(sb, doWhileInstruction);
            else if (instruction is WhileInstruction whileInstruction)
                AppendWhile(sb, whileInstruction);
            else if (instruction is IfInstruction ifInstruction)
                AppendIf(sb, ifInstruction);
        }

        public static void AppendFunction(ScriptBuilder sb, Function function)
        {
            AppendFunctionSignature(sb, function.Signature);
            AInstruction[] instructions = function.Instructions;
            if (instructions.Length == 0)
                sb.Append(" {}");
            else
            {
                sb.AppendLine();
                sb.OpenScope();
                AppendInstructions(sb, instructions);
                sb.CloseScope();
            }
        }

        public static string GenerateNamespaceName(int[] namespaceIDs, ConversionTable conversionTable)
        {
            StringBuilder sb = new();
            int i = 0;
            foreach (int namespaceID in namespaceIDs)
            {
                if (i != 0)
                    sb.Append('.');
                sb.Append(conversionTable.GetName(namespaceID));
                ++i;
            }
            return sb.ToString();
        }

        private static void AppendNamespaceToScriptString(ScriptBuilder sb, NamespaceObject @namespace)
        {
            sb.Append("namespace ");
            sb.AppendID(@namespace.ID);
            sb.AppendLine();
            sb.OpenScope();
            AppendEnvironmentDictionnaryToScriptString(sb, @namespace.Objects);
            sb.CloseScope();
        }

        private static void AppendEnvironmentObjectToScriptString(ScriptBuilder sb, EnvironmentObject envObject)
        {
            int[] comments = envObject.Comments;
            if (comments.Length != 0)
            {
                int n = 0;
                foreach (int comment in comments)
                {
                    if (n != 0)
                        sb.AppendLine();
                    sb.AppendComment(comment);
                    ++n;
                }
            }

            int[] tags = envObject.Tags;
            if (tags.Length != 0)
            {
                sb.Append('[');
                int n = 0;
                foreach (int tag in tags)
                {
                    if (n != 0)
                        sb.Append(", ");
                    sb.AppendID(tag);
                    ++n;
                }
                sb.Append("] ");
            }

            if (envObject is NamespaceObject namespaceObject)
                AppendNamespaceToScriptString(sb, namespaceObject);
            else if (envObject is FunctionObject functionObj && functionObj.Function is Function function)
                AppendFunction(sb, function);
            else if (envObject is InstructionObject instructionObject)
                AppendInstruction(sb, instructionObject.Instruction);
            else if (envObject is TypeObject typeObject)
            {
                int n = 0;
                foreach (TypeDefinitionObject typeDefinition in typeObject.Values)
                {
                    if (n != 0)
                    {
                        sb.AppendLine();
                        sb.AppendLine();
                    }
                    AppendTypeDefinition(sb, typeDefinition.TypeDefinition);
                    ++n;
                }
            }
        }

        private static void AppendEnvironmentDictionnaryToScriptString(ScriptBuilder sb, EnvironmentObjectDictionary dict)
        {
            int i = 0;
            foreach (var obj in dict.Objects)
            {
                if (i != 0)
                {
                    sb.AppendLine();
                    sb.AppendLine();
                }
                AppendEnvironmentObjectToScriptString(sb, obj);
                ++i;
            }

            InstructionObject[] instructions = dict.InstructionObjects;
            if (instructions.Length != 0)
            {
                sb.AppendLine();
                sb.AppendLine();
            }
            int n = 0;
            foreach (InstructionObject instruction in instructions)
            {
                if (n != 0)
                    sb.AppendLine();
                AppendEnvironmentObjectToScriptString(sb, instruction);
                n++;
            }
        }

        public static void ToScriptString(ScriptBuilder sb, Environment env)
        {
            AppendEnvironmentDictionnaryToScriptString(sb, env.Objects);
        }
    }
}
