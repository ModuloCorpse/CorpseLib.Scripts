using CorpseLib.Scripts.Memories;
using CorpseLib.Scripts.Parser;
using Environment = CorpseLib.Scripts.Context.Environment;

namespace CorpseLib.Scripts.Operations
{
    public class VariableOperationNode : AOperationTreeNode
    {
        protected override AMemoryValue Execute(Environment env, Memory memory)
        {
            throw new NotImplementedException();
        }

        protected override bool IsValid(ParsingContext parsingContext, string instructionStr)
        {
            //TODO Check if variable exist in parsing context, if not register as waiting global
            throw new NotImplementedException();
        }
    }
}
