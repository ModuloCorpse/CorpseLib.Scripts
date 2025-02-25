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
            Environment functionEnv = new(env);
            foreach (AInstruction instruction in m_Instructions)
            {
                if (instruction is Break || instruction is Continue)
                    return new();
                else
                {
                    instruction.ExecuteInstruction(functionEnv, stack);
                    if (stack.HasReturn)
                        return new();
                }
            }
            return stack.ReturnValue;
        }

        public string ToScriptString(ConversionTable conversionTable)
        {
            StringBuilder sb = new();
            sb.Append(Signature.ToScriptString(conversionTable));
            sb.Append(" {");
            foreach (AInstruction instruction in m_Instructions)
            {
                sb.Append(' ');
                sb.Append(instruction.ToScriptString(conversionTable));
            }
            sb.Append(" }");
            return sb.ToString();
        }

        internal void AddInstructions(List<AInstruction> instructions) => m_Instructions.AddRange(instructions);
    }
}
