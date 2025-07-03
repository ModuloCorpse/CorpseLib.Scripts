using CorpseLib.Scripts.Context;
using CorpseLib.Scripts.Parser;
using CorpseLib.Scripts.Type;
using Environment = CorpseLib.Scripts.Context.Environment;

namespace CorpseLib.Scripts.Operations
{
    public class CreateVariableOperationNode(ATypeInstance variableType, int variableID) : AOperationTreeNode
    {
        private readonly ATypeInstance m_VariableType = variableType;
        private readonly int m_VariableID = variableID;

        protected override object[] Execute(Environment env, FunctionStack functionStack)
        {
            throw new NotImplementedException();
        }

        protected override bool IsValid(ParsingContext parsingContext, string instructionStr)
        {
            throw new NotImplementedException();
        }
    }
}
