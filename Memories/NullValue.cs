namespace CorpseLib.Scripts.Memories
{
    public class NullValue : IMemoryValue
    {
        public IMemoryValue Clone() => Heap.NULL;
        public bool Equals(IMemoryValue obj) => obj is NullValue;
    }
}
