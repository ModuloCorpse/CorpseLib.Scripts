using CorpseLib.Scripts.Instruction;

namespace CorpseLib.Scripts.Context
{
    public class InstructionObject(AInstruction instruction, int[] tags, int[] comments) : EnvironmentObject(0, tags, comments)
    {
        private readonly AInstruction m_Instruction = instruction;
        public AInstruction Instruction => m_Instruction;
        public override bool IsValid() => true;
    }
}
