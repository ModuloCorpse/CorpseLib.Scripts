namespace CorpseLib.Scripts.Memories
{
    public class StackVariable(ParameterType type, int memoryAdress, bool isRef)
    {
        private readonly ParameterType m_Type = type;
        private int m_MemoryAddress = memoryAdress;
        private readonly bool m_IsRef = isRef;

        public ParameterType Type => m_Type;
        public int MemoryAddress
        {
            get => m_MemoryAddress;
            set => m_MemoryAddress = value;
        }
        public bool IsRef => m_IsRef;
    }
}
