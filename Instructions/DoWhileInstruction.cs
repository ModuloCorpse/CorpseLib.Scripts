using CorpseLib.Scripts.Memories;
using Environment = CorpseLib.Scripts.Context.Environment;

namespace CorpseLib.Scripts.Instructions
{
    public class DoWhileInstruction(Condition condition, List<AInstruction> body) : AConditionalInstruction(condition, body)
    {
        protected override void Execute(Environment env, Memory memory)
        {
            do
            {
                ScopedInstructions.EExecutionResult result = Body.Execute(env, memory);
                switch (result)
                {
                    case ScopedInstructions.EExecutionResult.Breaked:
                    case ScopedInstructions.EExecutionResult.Returned:
                        return; // Stop the loop
                    case ScopedInstructions.EExecutionResult.None:
                    case ScopedInstructions.EExecutionResult.Continued:
                        break; // Continue to the next iteration
                }
            } while (EvaluateCondition(env, memory));
        }
    }
}
