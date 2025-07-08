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

        protected override IMemoryValue Execute(Environment env, Memory memory)
        {
            IMemoryValue leftValue = m_Children[0].CallOperation(env, memory);
            IMemoryValue rightValue = m_Children[1].CallOperation(env, memory);
            if (m_IsNot)
                return new LiteralValue(!leftValue.Equals(rightValue));
            return new LiteralValue(leftValue.Equals(rightValue));
        }

        protected override bool IsValid(ParsingContext _, string instructionStr) => true;
    }
}
