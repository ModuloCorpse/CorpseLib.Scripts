namespace CorpseLib.Scripts.Memories
{
    public abstract class AMemoryValue
    {
        private int m_Address = -1;

        public int Address => m_Address;

        internal void SetAddress(int address) => m_Address = address;

        public abstract AMemoryValue Clone(Heap heap);
        public abstract bool Equals(AMemoryValue other);
    }
}
