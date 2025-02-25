namespace CorpseLib.Scripts.Instruction
{
    public abstract class AInstruction
    {
        internal void ExecuteInstruction(Environment env, FunctionStack instructionStack)
        {
            Environment executionEnv = new(env);
            Execute(executionEnv, instructionStack);
        }

        protected abstract void Execute(Environment env, FunctionStack instructionStack);
        public abstract string ToScriptString(ConversionTable conversionTable);
    }
}
