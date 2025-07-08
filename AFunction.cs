using CorpseLib.Scripts.Memories;
using Environment = CorpseLib.Scripts.Context.Environment;

namespace CorpseLib.Scripts
{
    public abstract class AFunction(FunctionSignature signature)
    {
        private readonly FunctionSignature m_Signature = signature;

        public FunctionSignature Signature => m_Signature;

        internal object? Call(Environment env, object?[] parameters)
        {
            //TODO
            Memory memory = new();
            /*int i = 0;
            while (i != m_Signature.Parameters.Length)
            {
                Parameter param = m_Signature.Parameters[i];
                object? paramValue = parameters[i];
                Variable argument;
                if (paramValue == null)
                    argument = param.Instantiate();
                else
                    argument = param.Instantiate([paramValue]);
                stack.AddVariable(param.ID, argument!);
                ++i;
            }
            if (i > parameters.Length)
            {
                return null;
            }*/
            return InternalExecute(env, memory);
        }

        internal abstract object? InternalExecute(Environment env, Memory memory);
    }
}
