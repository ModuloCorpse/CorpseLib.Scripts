using CorpseLib.Scripts.Type;

namespace CorpseLib.Scripts
{
    public abstract class AVoidFunction : AFunction
    {
        protected AVoidFunction(string functionName, Parameter[] parameters) : base(new(Types.VOID, functionName, parameters)) { }

        internal override object? InternalExecute(Environment environment)
        {
            Execute(environment);
            return new();
        }

        protected abstract void Execute(Environment environment);
    }
}
