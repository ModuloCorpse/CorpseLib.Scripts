using CorpseLib.Scripts.Context;
using CorpseLib.Scripts.Parser;
using Environment = CorpseLib.Scripts.Context.Environment;

namespace CorpseLib.Scripts.Operations
{
    public class NotOperationNode : AOperationTreeNode
    {
        internal override bool IsBooleanOperation => true;

        protected override object[] Execute(Environment env, FunctionStack functionStack)
        {
            object[] value = m_Children[0].CallOperation(env, functionStack);
            return [!(bool)value[0]];
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
