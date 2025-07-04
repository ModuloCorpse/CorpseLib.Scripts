namespace CorpseLib.Scripts.Memory
{
    public class NullValue : IMemoryValue
    {
        public IMemoryValue Clone() => Heap.NULL;
        public bool Equals(IMemoryValue obj) => obj is NullValue;
    }
}
