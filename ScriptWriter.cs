using CorpseLib.Scripts.Context;
using CorpseLib.Scripts.Instruction;
using CorpseLib.Scripts.Type;
using CorpseLib.Scripts.Type.Primitive;
using System.Text;
using static CorpseLib.Scripts.Context.TypeDefinition;

namespace CorpseLib.Scripts
{
    public static class ScriptWriter
    {
        public static void AppendValue(ref StringBuilder sb, ATypeInstance typeInstance, object[]? value)
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
                        AppendValue(ref sb, arrayType.ElementType, var.Values);
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
                            AppendValue(ref sb, var.Type, var.Values);
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
                    sb.Append('"').Append(tmp).Append('"');
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

        private static void AppendParameter(ref StringBuilder sb, Parameter parameter, ConversionTable conversionTable)
        {
            if (parameter.IsConst)
                sb.Append("const ");
            AppendTypeInstanceName(ref sb, parameter.Type, conversionTable);
            sb.Append(' ');
            sb.Append(conversionTable.GetName(parameter.ID));
            if (parameter.DefaultValues != null)
            {
                sb.Append(" = ");
                AppendValue(ref sb, parameter.Type, parameter.DefaultValues);
            }
        }

        public static void AppendObjectType(ref StringBuilder sb, ObjectType objectType, ConversionTable conversionTable)
        {
            sb.Append("struct ");
            AppendTypeInfo(ref sb, objectType.TypeInfo, conversionTable);
            sb.Append(" { ");
            foreach (Parameter attribute in objectType.Attributes)
            {
                AppendParameter(ref sb, attribute, conversionTable);
                sb.Append("; ");
            }
            sb.Append('}');
        }

        public static void AppendTypeInstanceName(ref StringBuilder sb, ATypeInstance typeInstance, ConversionTable conversionTable)
        {
            if (typeInstance is ObjectType objectType)
                AppendTypeInfo(ref sb, objectType.TypeInfo, conversionTable);
            else if (typeInstance is ArrayType arrayType)
            {
                AppendTypeInstanceName(ref sb, arrayType.ElementType, conversionTable);
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

        public static void AppendFunctionSignature(ref StringBuilder sb, FunctionSignature functionSignature, ConversionTable conversionTable)
        {
            sb.Append("fct ");
            if (functionSignature.ReturnType is not VoidType)
            {
                AppendTypeInstanceName(ref sb, functionSignature.ReturnType, conversionTable);
                sb.Append(' ');
            }
            sb.Append(string.Format("{0}(", conversionTable.GetName(functionSignature.ID)));
            int i = 0;
            foreach (Parameter p in functionSignature.Parameters)
            {
                if (i != 0)
                    sb.Append(", ");
                AppendParameter(ref sb, p, conversionTable);
                ++i;
            }
            sb.Append(')');
        }

        public static void AppendTypeDefinitionName(ref StringBuilder sb, TypeDefinition typeDefinition, ConversionTable conversionTable)
        {
            sb.Append(conversionTable.GetName(typeDefinition.ID));
            int[] templates = typeDefinition.Templates;
            if (templates.Length == 0)
                return;
            sb.Append('<');
            int i = 0;
            foreach (int template in templates)
            {
                if (i != 0)
                    sb.Append(", ");
                sb.Append(conversionTable.GetName(template));
                ++i;
            }
            sb.Append('>');
        }

        public static void AppendTypeInfo(ref StringBuilder sb, TypeInfo typeInfo, ConversionTable conversionTable)
        {
            if (typeInfo.IsConst)
                sb.Append("const ");
            foreach (int namespaceID in typeInfo.NamespacesID)
            {
                sb.Append(conversionTable.GetName(namespaceID));
                sb.Append('.');
            }
            sb.Append(conversionTable.GetName(typeInfo.ID));
            if (typeInfo.TemplateTypes.Length > 0)
            {
                sb.Append('<');
                int i = 0;
                foreach (TypeInfo templatedType in typeInfo.TemplateTypes)
                {
                    if (i != 0)
                        sb.Append(", ");
                    AppendTypeInfo(ref sb, templatedType, conversionTable);
                    ++i;
                }
                sb.Append('>');
            }
            if (typeInfo.IsArray)
                sb.Append("[]");
        }

        public static void AppendTypeDefinition(ref StringBuilder sb, TypeDefinition typeDefinition, ConversionTable conversionTable)
        {
            sb.Append("struct ");
            AppendTypeDefinitionName(ref sb, typeDefinition, conversionTable);
            sb.Append(" { ");
            foreach (AAttributeDefinition attribute in typeDefinition.Attributes)
            {
                AppendTypeInfo(ref sb, attribute.TypeInfo, conversionTable);
                sb.Append(' ');
                sb.Append(conversionTable.GetName(attribute.ID));
                if (attribute.Value != null)
                {
                    sb.Append(" = ");
                    sb.Append(attribute.Value);
                }
                sb.Append("; ");
            }
            sb.Append('}');
        }

        private static void AppendCondition(ref StringBuilder sb, Condition condition, ConversionTable conversionTable)
        {
            sb.Append(condition.ConditionStr);
        }

        private static void AppendScopedInstructions(ref StringBuilder sb, ScopedInstructions instructions, ConversionTable conversionTable)
        {
            if (instructions.Count > 1)
                sb.Append(" {");
            foreach (AInstruction instruction in instructions.Instructions)
            {
                sb.Append(' ');
                AppendInstruction(ref sb, instruction, conversionTable);
            }
            if (instructions.Count > 1)
                sb.Append(" }");
        }

        private static void AppendDoWhile(ref StringBuilder sb, DoWhileInstruction doWhileInstruction, ConversionTable conversionTable)
        {
            sb.Append("do");
            AppendScopedInstructions(ref sb, doWhileInstruction.Body, conversionTable);
            sb.Append(" while(");
            AppendCondition(ref sb, doWhileInstruction.Condition, conversionTable);
            sb.Append(')');
        }

        private static void AppendWhile(ref StringBuilder sb, WhileInstruction whileInstruction, ConversionTable conversionTable)
        {
            sb.Append("while(");
            AppendCondition(ref sb, whileInstruction.Condition, conversionTable);
            sb.Append(')');
            AppendScopedInstructions(ref sb, whileInstruction.Body, conversionTable);
        }

        private static void AppendIf(ref StringBuilder sb, IfInstruction ifInstruction, ConversionTable conversionTable)
        {
            sb.Append("if (");
            AppendCondition(ref sb, ifInstruction.Condition, conversionTable);
            sb.Append(')');
            AppendScopedInstructions(ref sb, ifInstruction.Body, conversionTable);
            foreach (var elseIf in ifInstruction.Elifs)
            {
                sb.Append(" elif (");
                AppendCondition(ref sb, elseIf.Condition, conversionTable);
                sb.Append(')');
                AppendScopedInstructions(ref sb, elseIf.Body, conversionTable);
            }
            if (ifInstruction.ElseBody.Count > 0)
            {
                sb.Append(" else");
                AppendScopedInstructions(ref sb, ifInstruction.ElseBody, conversionTable);
            }
        }

        public static void AppendInstruction(ref StringBuilder sb, AInstruction instruction, ConversionTable conversionTable)
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
                AppendDoWhile(ref sb, doWhileInstruction, conversionTable);
            else if (instruction is WhileInstruction whileInstruction)
                AppendWhile(ref sb, whileInstruction, conversionTable);
            else if (instruction is IfInstruction ifInstruction)
                AppendIf(ref sb, ifInstruction, conversionTable);
        }

        public static void AppendFunction(ref StringBuilder sb, Function function, ConversionTable conversionTable)
        {
            AppendFunctionSignature(ref sb, function.Signature, conversionTable);
            sb.Append(" {");
            foreach (AInstruction instruction in function.Instructions)
            {
                sb.Append(' ');
                AppendInstruction(ref sb, instruction, conversionTable);
            }
            sb.Append(" }");
        }

        public static void AppendNamespaceName(ref StringBuilder sb, Namespace @namespace, ConversionTable conversionTable)
        {
            int i = 0;
            foreach (int namespaceID in @namespace.IDS)
            {
                if (i != 0)
                    sb.Append('.');
                sb.Append(conversionTable.GetName(namespaceID));
                ++i;
            }
        }

        public static void ToScriptString(ref StringBuilder sb, Namespace @namespace, ConversionTable conversionTable)
        {
            sb.Append("namespace ");
            sb.Append(conversionTable.GetName(@namespace.ID));
            sb.Append(" { ");
            //TODO
            sb.Append('}');
        }
    }
}
