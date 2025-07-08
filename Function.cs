using CorpseLib.Scripts.Instructions;
using CorpseLib.Scripts.Memories;
using Environment = CorpseLib.Scripts.Context.Environment;

namespace CorpseLib.Scripts
{
    public class Function(FunctionSignature signature) : AFunction(signature)
    {
        private readonly List<AInstruction> m_Instructions = [];

        public AInstruction[] Instructions => [..m_Instructions];

        internal override object? InternalExecute(Environment env, Memory memory)
        {
            foreach (AInstruction instruction in m_Instructions)
            {
                if (instruction is Break || instruction is Continue)
                    return new();
                else
                {
                    instruction.ExecuteInstruction(env, memory);
                    if (memory.HasReturn)
                        return new();
                }
            }
            return memory.ReturnValue;
        }

        internal void AddInstructions(List<AInstruction> instructions) => m_Instructions.AddRange(instructions);
    }
}
