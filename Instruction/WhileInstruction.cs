using Environment = CorpseLib.Scripts.Context.Environment;

namespace CorpseLib.Scripts.Instruction
{
    public class WhileInstruction(Condition condition, List<AInstruction> body) : AConditionalInstruction(condition, body)
    {
        protected override void Execute(Environment env, FunctionStack functionStack)
        {
            while (EvaluateCondition(env, functionStack))
            {
                ScopedInstructions.EExecutionResult result = Body.Execute(env, functionStack);
                switch (result)
                {
                    case ScopedInstructions.EExecutionResult.Breaked:
                    case ScopedInstructions.EExecutionResult.Returned:
                        return; // Stop the loop
                    case ScopedInstructions.EExecutionResult.None:
                    case ScopedInstructions.EExecutionResult.Continued:
                        break; // Continue to the next iteration
                }
            }
        }
    }
}
