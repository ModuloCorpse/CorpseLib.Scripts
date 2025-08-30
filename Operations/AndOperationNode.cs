using CorpseLib.Scripts.Memories;
using CorpseLib.Scripts.Parser;
using Environment = CorpseLib.Scripts.Context.Environment;

namespace CorpseLib.Scripts.Operations
{
    public class AndOperationNode : AOperationTreeNode
    {
        internal override bool IsBooleanOperation => true;

        protected override AMemoryValue Execute(Environment env, Memory memory)
        {
            AMemoryValue leftValue = m_Children[0].CallOperation(env, memory);
            if (leftValue is MemoryLiteralValue leftLiteralValue && (bool)leftLiteralValue.Value == false)
                return new MemoryLiteralValue(false);
            AMemoryValue rightValue = m_Children[1].CallOperation(env, memory);
            return new MemoryLiteralValue(rightValue is MemoryLiteralValue rightLiteralValue && (bool)rightLiteralValue.Value);
        }

        protected override bool IsValid(ParsingContext parsingContext, string instructionStr)
        {
            if (m_Children[0].IsBooleanOperation)
            {
                if (m_Children[1].IsBooleanOperation)
                    return true;
                parsingContext.RegisterError($"Invalid {instructionStr}", "Invalid && operation : right side is not a boolean");
                return false;
            }
            parsingContext.RegisterError($"Invalid {instructionStr}", "Invalid && operation : left side is not a boolean");
            return false;
        }
    }
}
