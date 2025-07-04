﻿using CorpseLib.Scripts.Context;
using CorpseLib.Scripts.Operations;
using Environment = CorpseLib.Scripts.Context.Environment;

namespace CorpseLib.Scripts.Instructions
{
    public class ReturnInstruction(AOperationTreeNode? operations) : AInstruction
    {
        private readonly AOperationTreeNode? m_Operations = operations;

        internal AOperationTreeNode? Operations => m_Operations;

        protected override void Execute(Environment env, FunctionStack instructionStack)
        {
            if (m_Operations == null)
                instructionStack.Return();
            else
                instructionStack.Return(m_Operations.CallOperation(env, instructionStack));
        }
    }
}
