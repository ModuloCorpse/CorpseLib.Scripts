using Environment = CorpseLib.Scripts.Context.Environment;

namespace CorpseLib.Scripts.Instruction
{
    public abstract class AConditionalInstruction(Condition condition, List<AInstruction> body) : AInstruction
    {
        private readonly Condition m_Condition = condition;
        private readonly ScopedInstructions m_Body = new(body);

        public ScopedInstructions Body => m_Body;
        public Condition Condition => m_Condition;

        protected bool EvaluateCondition(Environment env, FunctionStack functionStack)
        {
            return false;
        }
    }
}
