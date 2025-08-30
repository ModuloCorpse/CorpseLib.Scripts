using CorpseLib.Scripts.Memories;

namespace CorpseLib.Scripts.Parameters
{
    public interface ITemporaryValue
    {
        public ITemporaryValue Clone();
        public AMemoryValue Allocate(Heap heap);
    }
}
