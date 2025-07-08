using CorpseLib.Scripts.Memories;
using Environment = CorpseLib.Scripts.Context.Environment;

namespace CorpseLib.Scripts
{
    public abstract class AVoidFunction(FunctionSignature signature) : AFunction((signature.ReturnType.TypeID == 0) ? signature : throw new ArgumentException("The signature must have a return type of void"))
    {
        internal override object? InternalExecute(Environment env, Memory memory)
        {
            Execute(env, memory);
            return new();
        }

        protected abstract void Execute(Environment env, Memory memory);
    }
}
