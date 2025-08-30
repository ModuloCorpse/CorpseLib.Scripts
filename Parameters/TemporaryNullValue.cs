using CorpseLib.Scripts.Memories;

namespace CorpseLib.Scripts.Parameters
{
    public class TemporaryNullValue : ITemporaryValue
    {
        public ITemporaryValue Clone() => new TemporaryNullValue();

        public AMemoryValue Allocate(Heap heap)
        {
            MemoryNullValue nullValue = new();
            heap.Allocate(nullValue);
            return nullValue;
        }
    }
}
