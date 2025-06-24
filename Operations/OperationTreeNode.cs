using CorpseLib.Scripts.Parser;
using Environment = CorpseLib.Scripts.Context.Environment;

namespace CorpseLib.Scripts.Operations
{
    public abstract class AOperationTreeNode
    {
        private readonly List<AOperationTreeNode> m_Children = [];
        private TypeInfo? m_ReturnType = null;

        protected AOperationTreeNode[] Children => [..m_Children];

        internal bool Is(TypeInfo instance, ParsingContext parsingContext)
        {
            if (m_ReturnType == null)
                m_ReturnType = EvaluateReturnType(parsingContext);
            return m_ReturnType != null && m_ReturnType == instance;
        }
        protected abstract TypeInfo? EvaluateReturnType(ParsingContext parsingContext);

        internal object CallOperation(Environment env, FunctionStack functionStack) => Execute(env, functionStack);
        protected abstract object Execute(Environment env, FunctionStack functionStack);
    }
}
