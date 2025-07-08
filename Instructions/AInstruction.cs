using CorpseLib.Scripts.Memories;
using Environment = CorpseLib.Scripts.Context.Environment;

namespace CorpseLib.Scripts.Instructions
{
    public abstract class AInstruction
    {
        internal void ExecuteInstruction(Environment env, Memory memory) => Execute(env, memory);

        protected abstract void Execute(Environment env, Memory memory);
    }
}
