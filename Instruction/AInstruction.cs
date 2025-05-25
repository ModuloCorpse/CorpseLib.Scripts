namespace CorpseLib.Scripts.Instruction
{
    public abstract class AInstruction
    {
        internal void ExecuteInstruction(Frame frame, FunctionStack instructionStack)
        {
            Frame executionFrame = new(frame);
            Execute(executionFrame, instructionStack);
        }

        protected abstract void Execute(Frame frame, FunctionStack instructionStack);
    }
}
