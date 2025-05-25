using CorpseLib.Scripts.Type;

namespace CorpseLib.Scripts
{
    public abstract class AVoidFunction(FunctionSignature signature) : AFunction((signature.ReturnType == Types.VOID) ? signature : throw new ArgumentException("The signature must have a return type of void"))
    {
        internal override object? InternalExecute(Frame frame)
        {
            Execute(frame);
            return new();
        }

        protected abstract void Execute(Frame frame);
    }
}
