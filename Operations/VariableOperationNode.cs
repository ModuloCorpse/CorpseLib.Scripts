using CorpseLib.Scripts.Context;
using CorpseLib.Scripts.Memory;
using CorpseLib.Scripts.Parser;
using Environment = CorpseLib.Scripts.Context.Environment;

namespace CorpseLib.Scripts.Operations
{
    public class VariableOperationNode : AOperationTreeNode
    {
        protected override IMemoryValue Execute(Environment env, FunctionStack functionStack)
        {
            throw new NotImplementedException();
        }

        protected override bool IsValid(ParsingContext parsingContext, string instructionStr)
        {
            throw new NotImplementedException();
        }
    }
}
