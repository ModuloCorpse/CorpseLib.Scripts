using CorpseLib.Scripts.Memories;
using Environment = CorpseLib.Scripts.Context.Environment;

namespace CorpseLib.Scripts.Instructions
{
    public abstract class AConditionalInstruction(Condition condition, List<AInstruction> body) : AInstruction
    {
        private readonly Condition m_Condition = condition;
        private readonly ScopedInstructions m_Body = new(body);

        public ScopedInstructions Body => m_Body;
        public Condition Condition => m_Condition;

        protected bool EvaluateCondition(Environment env, Memory memory)
        {
            return false;
        }
    }
}
