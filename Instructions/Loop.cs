using CorpseLib.Scripts.Memories;
using Environment = CorpseLib.Scripts.Context.Environment;

namespace CorpseLib.Scripts.Instructions
{
    public class Break : AInstruction { protected override void Execute(Environment env, Memory memory) { } }

    public class Continue : AInstruction { protected override void Execute(Environment env, Memory memory) { } }
}
