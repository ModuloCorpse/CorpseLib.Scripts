using CorpseLib.Scripts.Context;
using CorpseLib.Scripts.Memory;
using CorpseLib.Scripts.Parser;
using Environment = CorpseLib.Scripts.Context.Environment;

namespace CorpseLib.Scripts.Operations
{
    public class OrOperationNode : AOperationTreeNode
    {
        internal override bool IsBooleanOperation => true;

        protected override IMemoryValue Execute(Environment env, FunctionStack functionStack)
        {
            IMemoryValue leftValue = m_Children[0].CallOperation(env, functionStack);
            if (leftValue is LiteralValue leftLiteralValue && (bool)leftLiteralValue.Value == true)
                return new LiteralValue(true);
            IMemoryValue rightValue = m_Children[1].CallOperation(env, functionStack);
            return new LiteralValue(rightValue is LiteralValue rightLiteralValue && (bool)rightLiteralValue.Value);
        }

        protected override bool IsValid(ParsingContext parsingContext, string instructionStr)
        {
            if (m_Children[0].IsBooleanOperation)
            {
                if (m_Children[1].IsBooleanOperation)
                    return true;
                parsingContext.RegisterError($"Invalid {instructionStr}", "Invalid || operation : right side is not a boolean");
                return false;
            }
            parsingContext.RegisterError($"Invalid {instructionStr}", "Invalid || operation : left side is not a boolean");
            return false;
        }
    }
}
