using CorpseLib.Scripts.Context;
using CorpseLib.Scripts.Memory;
using CorpseLib.Scripts.Parser;
using Environment = CorpseLib.Scripts.Context.Environment;

namespace CorpseLib.Scripts.Operations
{
    public class LiteralOperationNode(IMemoryValue value) : AOperationTreeNode
    {
        private readonly IMemoryValue m_Value = value;

        internal IMemoryValue Value => m_Value;

        internal override bool IsBooleanOperation => m_Value is LiteralValue literalValue && literalValue.Value is bool;

        protected override IMemoryValue Execute(Environment env, FunctionStack functionStack) => m_Value;
        protected override bool IsValid(ParsingContext _, string instructionStr) => true;
    }
}
