using CorpseLib.Scripts.Memories;
using CorpseLib.Scripts.Parser;
using Environment = CorpseLib.Scripts.Context.Environment;

namespace CorpseLib.Scripts.Operations
{
    public class EqualityOperationNode(bool isNot) : AOperationTreeNode
    {
        private readonly bool m_IsNot = isNot;

        public bool IsNot => m_IsNot;

        internal override bool IsBooleanOperation => true;

        protected override AMemoryValue Execute(Environment env, Memory memory)
        {
            AMemoryValue leftValue = m_Children[0].CallOperation(env, memory);
            AMemoryValue rightValue = m_Children[1].CallOperation(env, memory);
            if (m_IsNot)
                return new MemoryLiteralValue(!leftValue.Equals(rightValue));
            return new MemoryLiteralValue(leftValue.Equals(rightValue));
        }

        protected override bool IsValid(ParsingContext _, string instructionStr) => true;
    }
}
