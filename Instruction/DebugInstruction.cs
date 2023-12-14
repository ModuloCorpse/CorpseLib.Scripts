namespace CorpseLib.Scripts.Instruction
{
    public class DebugInstruction(string instruction) : AInstruction
    {
        private readonly string m_Instruction = instruction;
        protected override void Execute(Environment env, FunctionStack functionStack) => Console.WriteLine(m_Instruction);
        public override string ToString() => string.Format("{0};", m_Instruction);
    }
}
