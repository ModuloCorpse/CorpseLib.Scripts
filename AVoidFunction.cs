using CorpseLib.Scripts.Type;
using Environment = CorpseLib.Scripts.Context.Environment;

namespace CorpseLib.Scripts
{
    public abstract class AVoidFunction(FunctionSignature signature) : AFunction((signature.ReturnType == Types.VOID) ? signature : throw new ArgumentException("The signature must have a return type of void"))
    {
        internal override object? InternalExecute(Environment env, FunctionStack functionStack)
        {
            Execute(env, functionStack);
            return new();
        }

        protected abstract void Execute(Environment env, FunctionStack functionStack);
    }
}
