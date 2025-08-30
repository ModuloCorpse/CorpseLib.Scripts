namespace CorpseLib.Scripts.Memories
{
    public class MemoryNullValue : AMemoryValue
    {
        public override AMemoryValue Clone(Heap _) => new MemoryNullValue();
        public override bool Equals(AMemoryValue obj) => obj is MemoryNullValue;
    }
}
