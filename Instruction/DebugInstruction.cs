namespace CorpseLib.Scripts.Instruction
{
    public class DebugInstruction(string instruction) : AInstruction
    {
        private readonly string m_Instruction = instruction;
        public string Instruction => m_Instruction;
        protected override void Execute(Frame frame, FunctionStack functionStack) => Console.WriteLine(m_Instruction);
    }
}
