using System.Text;
using CorpseLib.Scripts.Instruction;

namespace CorpseLib.Scripts
{
    public class Function(FunctionSignature signature) : AFunction(signature)
    {
        private readonly List<AInstruction> m_Instructions = [];

        internal override object? InternalExecute(Environment env)
        {
            FunctionStack stack = new();
            env.OpenScope();
            foreach (AInstruction instruction in m_Instructions)
            {
                if (instruction is Break || instruction is Continue)
                {
                    env.CloseScope();
                    return new();
                }
                else
                {
                    instruction.ExecuteInstruction(env, stack);
                    if (stack.HasReturn)
                    {
                        env.CloseScope();
                        return new();
                    }
                }
            }
            env.CloseScope();
            return stack.ReturnValue;
        }

        public override string ToString()
        {
            StringBuilder sb = new();
            sb.Append(Signature.ToString());
            sb.Append(" {");
            foreach (AInstruction instruction in m_Instructions)
            {
                sb.Append(' ');
                sb.Append(instruction.ToString());
            }
            sb.Append(" }");
            return sb.ToString();
        }

        internal void AddInstructions(List<AInstruction> instructions) => m_Instructions.AddRange(instructions);
    }
}
