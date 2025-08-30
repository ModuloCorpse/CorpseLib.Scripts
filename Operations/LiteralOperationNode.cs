using CorpseLib.Scripts.Memories;
using CorpseLib.Scripts.Parameters;
using CorpseLib.Scripts.Parser;
using Environment = CorpseLib.Scripts.Context.Environment;

namespace CorpseLib.Scripts.Operations
{
    public class LiteralOperationNode(ITemporaryValue value) : AOperationTreeNode
    {
        private readonly ITemporaryValue m_Value = value;

        internal ITemporaryValue Value => m_Value;

        internal override bool IsBooleanOperation => m_Value is TemporaryLiteralValue literalValue && literalValue.Value is bool;

        protected override AMemoryValue Execute(Environment env, Memory memory) => m_Value.Allocate(memory.Heap);
        protected override bool IsValid(ParsingContext _, string instructionStr) => true;
    }
}
