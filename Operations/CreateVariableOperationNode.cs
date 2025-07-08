using CorpseLib.Scripts.Memories;
using CorpseLib.Scripts.Parser;
using Environment = CorpseLib.Scripts.Context.Environment;

namespace CorpseLib.Scripts.Operations
{
    public class CreateVariableOperationNode(int variableTypeID, int variableID) : AOperationTreeNode
    {
        private readonly int m_VariableTypeID = variableTypeID;
        private readonly int m_VariableID = variableID;

        protected override IMemoryValue Execute(Environment env, Memory memory)
        {
            throw new NotImplementedException();
        }

        protected override bool IsValid(ParsingContext parsingContext, string instructionStr)
        {
            throw new NotImplementedException();
        }
    }
}
