using CorpseLib.Scripts.Context;
using CorpseLib.Scripts.Parser;
using Environment = CorpseLib.Scripts.Context.Environment;

namespace CorpseLib.Scripts.Operations
{
    public class AndOperationNode : AOperationTreeNode
    {
        internal override bool IsBooleanOperation => true;

        protected override object[] Execute(Environment env, FunctionStack functionStack)
        {
            object[] leftValue = m_Children[0].CallOperation(env, functionStack);
            if ((bool)leftValue[0] == false)
                return [false];
            object[] rightValue = m_Children[1].CallOperation(env, functionStack);
            return [(bool)rightValue[0]];
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
