using CorpseLib.Scripts.Memories;
using CorpseLib.Scripts.Parameters;
using CorpseLib.Scripts.Parser;
using Environment = CorpseLib.Scripts.Context.Environment;

namespace CorpseLib.Scripts.Operations
{
    public class CreateVariableOperationNode(ParameterType variableType, int variableID) : AOperationTreeNode
    {
        private readonly ParameterType m_VariableTypeID = variableType;
        private readonly int m_VariableID = variableID;

        protected override AMemoryValue Execute(Environment env, Memory memory)
        {
            AMemoryValue value = (m_Children.Count == 1) ? m_Children[0].CallOperation(env, memory) : new MemoryNullValue();
            memory.AddVariable(m_VariableID, m_VariableTypeID, value);
            return value;
        }

        protected override bool IsValid(ParsingContext parsingContext, string instructionStr) => m_Children.Count switch
        {
            0 or 1 => true,
            _ => false
        };
    }
}
