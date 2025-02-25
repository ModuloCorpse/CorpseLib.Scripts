namespace CorpseLib.Scripts
{
    public abstract class AFunction
    {
        private readonly FunctionSignature m_Signature;

        public FunctionSignature Signature => m_Signature;

        protected AFunction(FunctionSignature signature) => m_Signature = signature;

        internal object? Call(object?[] parameters)
        {
            Environment environment = new();
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
                environment.AddVariable(param.ID, argument);
                //Fill environment with parameters
                ++i;
            }
            if (i > parameters.Length)
            {
                return null;
            }
            return InternalExecute(environment);
        }

        internal abstract object? InternalExecute(Environment environment);
    }
}
