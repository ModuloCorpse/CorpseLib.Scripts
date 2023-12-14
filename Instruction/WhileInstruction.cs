using System.Text;

namespace CorpseLib.Scripts.Instruction
{
    public class WhileInstruction(string condition, List<AInstruction> body) : AInstruction
    {
        private readonly List<AInstruction> m_Body = body;
        private readonly string m_Condition = condition;

        private bool EvaluateCondition(Environment env, FunctionStack functionStack, string condition)
        {
            return false;
        }

        protected override void Execute(Environment env, FunctionStack functionStack)
        {
            while (EvaluateCondition(env, functionStack, m_Condition))
            {
                env.OpenScope();
                foreach (AInstruction instruction in m_Body)
                {
                    if (instruction is Break)
                    {
                        env.CloseScope();
                        return;
                    }
                    else if (instruction is Continue)
                        break;
                    else
                    {
                        instruction.ExecuteInstruction(env, functionStack);
                        if (functionStack.HasReturn)
                        {
                            env.CloseScope();
                            return;
                        }
                    }
                }
                env.CloseScope();
            }
        }

        public override string ToString()
        {
            StringBuilder builder = new("while (");
            builder.Append(m_Condition);
            builder.Append(')');
            if (m_Body.Count > 1)
                builder.Append(" {");
            foreach (AInstruction instruction in m_Body)
            {
                builder.Append(' ');
                builder.Append(instruction.ToString());
            }
            if (m_Body.Count > 1)
                builder.Append(" }");
            return builder.ToString();
        }
    }
}
