using CorpseLib.Scripts.Memories;
using CorpseLib.Scripts.Parser;
using Environment = CorpseLib.Scripts.Context.Environment;

namespace CorpseLib.Scripts.Operations
{
    public class NotOperationNode : AOperationTreeNode
    {
        internal override bool IsBooleanOperation => true;

        protected override AMemoryValue Execute(Environment env, Memory memory)
        {
            AMemoryValue value = m_Children[0].CallOperation(env, memory);
            return new MemoryLiteralValue(value is MemoryLiteralValue literalValue && !(bool)literalValue.Value);
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
