using CorpseLib.Scripts.Context;
using CorpseLib.Scripts.Instructions;
using CorpseLib.Scripts.Operations;
using CorpseLib.Scripts.Type;
using System.Text;
using static CorpseLib.Scripts.Context.TypeDefinition;
using static CorpseLib.Scripts.Context.TypeObject;
using Environment = CorpseLib.Scripts.Context.Environment;

namespace CorpseLib.Scripts
{
    public static class ScriptWriter
    {
        public static void AppendTypeInstanceName(ScriptBuilder sb, Environment env, ParameterType type)
        {
            int typeIndex = type.TypeID;
            switch (typeIndex)
            {
                case 0: sb.Append("void"); break;
                case 1: sb.Append("bool"); break;
                case 2: sb.Append("char"); break;
                case 3: sb.Append("uchar"); break;
                case 4: sb.Append("short"); break;
                case 5: sb.Append("ushort"); break;
                case 6: sb.Append("int"); break;
                case 7: sb.Append("uint"); break;
                case 8: sb.Append("long"); break;
                case 9: sb.Append("ulong"); break;
                case 10: sb.Append("float"); break;
                case 11: sb.Append("double"); break;
                case 12: sb.Append("string"); break;
                default:
                {
                    ATypeInstance? typeInstance = env.GetTypeInstance(typeIndex);
                    if (typeInstance is ObjectType objectType)
                        AppendTypeInfo(sb, objectType.TypeInfo);
                    break;
                }
            }

            for (int i = 0; i < type.ArrayCount; i++)
                sb.Append("[]");
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
                    for (int i = 0; i != elements.Count; ++i)
                    {
                        object[] var = elements[i]!;
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

        private static void AppendParameter(ScriptBuilder sb, Environment env, Parameter parameter)
        {
            if (parameter.IsConst)
                sb.Append("const ");
            AppendTypeInstanceName(sb, env, parameter.Type);
            if (parameter.IsRef)
                sb.Append('&');
            sb.Append(' ');
            sb.AppendID(parameter.ID);
            if (parameter.DefaultValues != null)
            {
                sb.Append(" = ");
                AppendAnonymousValue(sb, parameter.DefaultValues);
            }
        }

        public static void AppendObjectType(ScriptBuilder sb, Environment env, ObjectType objectType)
        {
            sb.Append("struct ");
            AppendTypeInfo(sb, objectType.TypeInfo);
            sb.Append(" { ");
            foreach (Parameter attribute in objectType.Attributes)
            {
                AppendParameter(sb, env, attribute);
                sb.Append("; ");
            }
            sb.Append('}');
        }

        public static void AppendFunctionSignature(ScriptBuilder sb, Environment env, FunctionSignature functionSignature)
        {
            sb.Append("fct ");
            if (functionSignature.ReturnType.TypeID != 0)
            {
                AppendTypeInstanceName(sb, env, functionSignature.ReturnType);
                sb.Append(' ');
            }
            sb.AppendID(functionSignature.ID);
            sb.Append('(');
            int i = 0;
            foreach (Parameter p in functionSignature.Parameters)
            {
                if (i != 0)
                    sb.Append(", ");
                AppendParameter(sb, env, p);
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
                sb.Append("::");
            }
            sb.AppendID(signature.ID);
        }

        public static void AppendTypeInfo(ScriptBuilder sb, TypeInfo typeInfo)
        {
            if (typeInfo.IsStatic)
                sb.Append("static ");
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
            for (int i = 0; i != typeInfo.ArrayCount; ++i)
                sb.Append("[]");
            if (typeInfo.IsRef)
                sb.Append('&');
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

        private static void AppendBinaryOperationTreeNode(ScriptBuilder sb, AOperationTreeNode[] children, string @operator)
        {
            AppendOperationTreeNode(sb, children[0]);
            sb.Append($" {@operator} ");
            AppendOperationTreeNode(sb, children[1]);
        }

        private static void AppendOperationTreeNode(ScriptBuilder sb, AOperationTreeNode node)
        {
            AOperationTreeNode[] children = node.Children;
            if (node is LiteralOperationNode literalOperation)
                AppendAnonymousValue(sb, literalOperation.Value);
            else if (node is VariableOperationNode variableOperation)
            {

            }
            else if (node is CreateVariableOperationNode createVariableOperation)
            {

            }
            else if (node is EqualityOperationNode equalityOperationNode)
                AppendBinaryOperationTreeNode(sb, children, equalityOperationNode.IsNot ? "!=" : "==");
            else if (node is AndOperationNode andOperationNode)
                AppendBinaryOperationTreeNode(sb, children, "&&");
            else if (node is OrOperationNode orOperationNode)
                AppendBinaryOperationTreeNode(sb, children, "||");
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
            else if (instruction is ReturnInstruction returnInstruction)
            {
                AOperationTreeNode? returnValue = returnInstruction.Operations;
                if (returnValue == null)
                    sb.Append("return;");
                else
                {
                    sb.Append("return ");
                    AppendOperationTreeNode(sb, returnValue);
                sb.Append(';');
                }
            }
        }

        public static void AppendFunction(ScriptBuilder sb, Environment env, Function function)
        {
            AppendFunctionSignature(sb, env, function.Signature);
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

        private static void AppendNamespaceToScriptString(ScriptBuilder sb, Environment env, NamespaceObject @namespace)
        {
            sb.Append("namespace ");
            sb.AppendID(@namespace.ID);
            sb.AppendLine();
            sb.OpenScope();
            AppendEnvironmentDictionnaryToScriptString(sb, env, @namespace.Objects);
            sb.CloseScope();
        }

        private static void AppendEnvironmentObjectToScriptString(ScriptBuilder sb, Environment env, EnvironmentObject envObject)
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
                AppendNamespaceToScriptString(sb, env, namespaceObject);
            else if (envObject is FunctionObject functionObj && functionObj.Function is Function function)
                AppendFunction(sb, env, function);
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

        private static void AppendEnvironmentDictionnaryToScriptString(ScriptBuilder sb, Environment env, EnvironmentObjectDictionary dict)
        {
            int i = 0;
            foreach (var obj in dict.Objects)
            {
                if (i != 0)
                {
                    sb.AppendLine();
                    sb.AppendLine();
                }
                AppendEnvironmentObjectToScriptString(sb, env, obj);
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
                AppendEnvironmentObjectToScriptString(sb, env, instruction);
                n++;
            }
        }

        public static void ToScriptString(ScriptBuilder sb, Environment env)
        {
            AppendEnvironmentDictionnaryToScriptString(sb, env, env.Objects);
        }
    }
}
