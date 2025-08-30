using CorpseLib.Scripts.Memories;
using CorpseLib.Scripts.Operations;
using Environment = CorpseLib.Scripts.Context.Environment;

namespace CorpseLib.Scripts.Instructions
{
    public class OperationInstruction(AOperationTreeNode operations) : AInstruction
    {
        private readonly AOperationTreeNode m_Operations = operations;

        protected override void Execute(Environment env, Memory memory)
        {
            m_Operations.CallOperation(env, memory);
        }
    }
}
