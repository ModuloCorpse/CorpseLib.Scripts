namespace CorpseLib.Scripts.Instruction
{
    public class ScopedInstructions
    {
        public enum EExecutionResult
        {
            Continued,
            Breaked,
            Returned,
            None
        }

        private readonly List<AInstruction> m_Instructions;

        public ScopedInstructions(List<AInstruction> instructions) => m_Instructions = instructions;
        public ScopedInstructions() => m_Instructions = [];

        public AInstruction[] Instructions => [..m_Instructions];
        public bool IsEmpty => m_Instructions.Count == 0;
        public int Count => m_Instructions.Count;

        public void AddInstruction(AInstruction instruction) => m_Instructions.Add(instruction);
        public void AddInstructions(IEnumerable<AInstruction> instructions) => m_Instructions.AddRange(instructions);

        public EExecutionResult Execute(Frame frame, FunctionStack functionStack)
        {
            Frame subFrame = new(frame);
            foreach (AInstruction instruction in m_Instructions)
            {
                if (instruction is Break)
                    return EExecutionResult.Breaked;
                else if (instruction is Continue)
                    return EExecutionResult.Continued;
                else
                {
                    instruction.ExecuteInstruction(subFrame, functionStack);
                    if (functionStack.HasReturn)
                        return EExecutionResult.Returned;
                }
            }
            return EExecutionResult.None;
        }
    }
}
