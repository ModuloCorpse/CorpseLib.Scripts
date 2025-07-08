using CorpseLib.Scripts.Memories;
using CorpseLib.Scripts.Operations;
using Environment = CorpseLib.Scripts.Context.Environment;

namespace CorpseLib.Scripts.Instructions
{
    public class ReturnInstruction(AOperationTreeNode? operations) : AInstruction
    {
        private readonly AOperationTreeNode? m_Operations = operations;

        internal AOperationTreeNode? Operations => m_Operations;

        protected override void Execute(Environment env, Memory memory)
        {
            if (m_Operations == null)
                memory.Return();
            else
                memory.Return(m_Operations.CallOperation(env, memory));
        }
    }
}
