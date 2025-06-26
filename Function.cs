using CorpseLib.Scripts.Instructions;
using Environment = CorpseLib.Scripts.Context.Environment;

namespace CorpseLib.Scripts
{
    public class Function(FunctionSignature signature) : AFunction(signature)
    {
        private readonly List<AInstruction> m_Instructions = [];

        public AInstruction[] Instructions => [..m_Instructions];

        internal override object? InternalExecute(Environment env, FunctionStack stack)
        {
            foreach (AInstruction instruction in m_Instructions)
            {
                if (instruction is Break || instruction is Continue)
                    return new();
                else
                {
                    instruction.ExecuteInstruction(env, stack);
                    if (stack.HasReturn)
                        return new();
                }
            }
            return stack.ReturnValue;
        }

        internal void AddInstructions(List<AInstruction> instructions) => m_Instructions.AddRange(instructions);
    }
}
