namespace CorpseLib.Scripts
{
    public abstract class AFunction
    {
        private readonly FunctionSignature m_Signature;

        public FunctionSignature Signature => m_Signature;

        protected AFunction(FunctionSignature signature) => m_Signature = signature;

        internal object? Call(object?[] parameters)
        {
            Frame frame = new();
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
                frame.AddVariable(param.ID, argument!);
                //Fill frame with parameters
                ++i;
            }
            if (i > parameters.Length)
            {
                return null;
            }
            return InternalExecute(frame);
        }

        internal abstract object? InternalExecute(Frame frame);
    }
}
