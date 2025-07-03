using CorpseLib.Scripts.Context;
using CorpseLib.Scripts.Parser;
using Environment = CorpseLib.Scripts.Context.Environment;

namespace CorpseLib.Scripts.Operations
{
    public class LiteralOperationNode(object[] value) : AOperationTreeNode
    {
        private readonly object[] m_Value = value;

        internal object[] Value => m_Value;

        internal override bool IsBooleanOperation => m_Value.Length == 1 && m_Value[0] is bool;

        protected override object[] Execute(Environment env, FunctionStack functionStack) => m_Value;
        protected override bool IsValid(ParsingContext _, string instructionStr) => true;
    }
}
