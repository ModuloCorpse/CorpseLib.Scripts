using System.Text;

namespace CorpseLib.Scripts.Instruction
{
    public class DoWhileInstruction(string condition, List<AInstruction> body) : AInstruction
    {
        private readonly List<AInstruction> m_Body = body;
        private readonly string m_Condition = condition;

        private bool EvaluateCondition(Environment env, FunctionStack functionStack, string condition)
        {
            return false;
        }

        protected override void Execute(Environment env, FunctionStack functionStack)
        {
            do
            {
                Environment doWhileEnvironment = new(env);
                foreach (AInstruction instruction in m_Body)
                {
                    if (instruction is Break)
                        return;
                    else if (instruction is Continue)
                        break;
                    else
                    {
                        instruction.ExecuteInstruction(doWhileEnvironment, functionStack);
                        if (functionStack.HasReturn)
                            return;
                    }
                }
            } while (EvaluateCondition(env, functionStack, m_Condition));
        }

        public override string ToScriptString(ConversionTable conversionTable)
        {
            StringBuilder builder = new("do");
            if (m_Body.Count > 1)
                builder.Append(" {");
            foreach (AInstruction instruction in m_Body)
            {
                builder.Append(' ');
                builder.Append(instruction.ToScriptString(conversionTable));
            }
            if (m_Body.Count > 1)
                builder.Append(" }");
            builder.Append(" while(");
            builder.Append(m_Condition);
            builder.Append(')');
            return builder.ToString();
        }
    }
}
