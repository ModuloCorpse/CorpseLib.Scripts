using CorpseLib.Scripts.Memories;
using CorpseLib.Scripts.Parser;
using Environment = CorpseLib.Scripts.Context.Environment;

namespace CorpseLib.Scripts.Operations
{
    public class NotOperationNode : AOperationTreeNode
    {
        internal override bool IsBooleanOperation => true;

        protected override IMemoryValue Execute(Environment env, Memory memory)
        {
            IMemoryValue value = m_Children[0].CallOperation(env, memory);
            return new LiteralValue(value is LiteralValue literalValue && !(bool)literalValue.Value);
        }

        protected override bool IsValid(ParsingContext parsingContext, string instructionStr)
        {
            if (m_Children[0].IsBooleanOperation)
                return true;
            parsingContext.RegisterError($"Invalid {instructionStr}", "Invalid ! operation : right side is not a boolean");
            return false;
        }
    }
}
