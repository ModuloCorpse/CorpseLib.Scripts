using Environment = CorpseLib.Scripts.Context.Environment;

namespace CorpseLib.Scripts
{
    public abstract class AFunction(FunctionSignature signature)
    {
        private readonly FunctionSignature m_Signature = signature;

        public FunctionSignature Signature => m_Signature;

        internal object? Call(Environment env, object?[] parameters)
        {
            FunctionStack stack = new();
            int i = 0;
            while (i != m_Signature.Parameters.Length)
            {
                Parameter param = m_Signature.Parameters[i];
                object? paramValue = parameters[i];
                Variable? argument = null;
                if (paramValue == null)
                    argument = param.Instantiate();
                else if (param.Type.IsOfType([paramValue]))
                    argument = param.Instantiate([paramValue]);
                stack.AddVariable(param.ID, argument!);
                ++i;
            }
            if (i > parameters.Length)
            {
                return null;
            }
            return InternalExecute(env, stack);
        }

        internal abstract object? InternalExecute(Environment env, FunctionStack stack);
    }
}
