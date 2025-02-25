using System.Text;

namespace CorpseLib.Scripts.Instruction
{
    public class IfInstruction(string condition, List<AInstruction> body) : AInstruction
    {
        private readonly List<Tuple<string, List<AInstruction>>> m_Elifs = [];
        private readonly List<AInstruction> m_ElseBody = [];
        private readonly List<AInstruction> m_Body = body;
        private readonly string m_Condition = condition;

        internal void AddElif(string condition, List<AInstruction> body) => m_Elifs.Add(new(condition, body));
        internal void SetElseBody(List<AInstruction> body) => m_ElseBody.AddRange(body);

        private bool EvaluateCondition(Environment env, FunctionStack functionStack, string condition)
        {
            //TODO
            return false;
        }

        private List<AInstruction> GetInstructions(Environment env, FunctionStack functionStack)
        {
            if (EvaluateCondition(env, functionStack, m_Condition))
                return m_Body;
            foreach (var elif in m_Elifs)
            {
                if (EvaluateCondition(env, functionStack, elif.Item1))
                    return elif.Item2;
            }
            if (m_ElseBody.Count > 0)
                return m_ElseBody;
            return [];
        }

        protected override void Execute(Environment env, FunctionStack functionStack)
        {
            List<AInstruction> instructions = GetInstructions(env, functionStack);
            if (instructions.Count == 0)
                return;
            Environment ifEnvironment = new(env);
            foreach (AInstruction instruction in instructions)
            {
                if (instruction is Break || instruction is Continue)
                    return;
                else
                {
                    instruction.ExecuteInstruction(ifEnvironment, functionStack);
                    if (functionStack.HasReturn)
                        return;
                }
            }
        }

        public override string ToScriptString(ConversionTable conversionTable)
        {
            StringBuilder builder = new("if (");
            builder.Append(m_Condition);
            builder.Append(')');
            if (m_Body.Count > 1)
                builder.Append(" {");
            foreach (AInstruction instruction in m_Body)
            {
                builder.Append(' ');
                builder.Append(instruction.ToScriptString(conversionTable));
            }
            if (m_Body.Count > 1)
                builder.Append(" }");
            foreach (var elseIf in m_Elifs)
            {
                builder.Append(" elif (");
                builder.Append(elseIf.Item1);
                builder.Append(')');
                if (elseIf.Item2.Count > 1)
                    builder.Append(" {");
                foreach (AInstruction instruction in elseIf.Item2)
                {
                    builder.Append(' ');
                    builder.Append(instruction.ToScriptString(conversionTable));
                }
                if (elseIf.Item2.Count > 1)
                    builder.Append(" }");
            }
            if (m_ElseBody.Count > 0)
            {
                builder.Append(" else");
                if (m_ElseBody.Count > 1)
                    builder.Append(" {");
                foreach (AInstruction instruction in m_ElseBody)
                {
                    builder.Append(' ');
                    builder.Append(instruction.ToScriptString(conversionTable));
                }
                if (m_ElseBody.Count > 1)
                    builder.Append(" }");
            }
            return builder.ToString();
        }
    }
}
