using Environment = CorpseLib.Scripts.Context.Environment;

namespace CorpseLib.Scripts.Instructions
{
    public abstract class AInstruction
    {
        internal void ExecuteInstruction(Environment env, FunctionStack instructionStack)
        {
            env.OpenScope();
            Execute(env, instructionStack);
            env.CloseScope();
        }

        protected abstract void Execute(Environment env, FunctionStack instructionStack);
    }
}
