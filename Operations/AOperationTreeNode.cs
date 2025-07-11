﻿using CorpseLib.Scripts.Memories;
using CorpseLib.Scripts.Parser;
using Environment = CorpseLib.Scripts.Context.Environment;

namespace CorpseLib.Scripts.Operations
{
    public abstract class AOperationTreeNode
    {
        protected readonly List<AOperationTreeNode> m_Children = [];

        internal AOperationTreeNode[] Children => [..m_Children];

        internal virtual bool IsBooleanOperation => false;

        internal bool Validate(ParsingContext parsingContext, string instructionStr)
        {
            foreach (AOperationTreeNode child in m_Children)
            {
                if (!child.Validate(parsingContext, instructionStr))
                    return false;
            }
            return IsValid(parsingContext, instructionStr);
        }

        protected abstract bool IsValid(ParsingContext parsingContext, string instructionStr);

        internal IMemoryValue CallOperation(Environment env, Memory memory) => Execute(env, memory);
        protected abstract IMemoryValue Execute(Environment env, Memory memory);
    }
}
