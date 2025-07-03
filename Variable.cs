namespace CorpseLib.Scripts
{
    public class Variable(ParameterType type, int memoryAdress)
    {
        private readonly ParameterType m_Type = type;
        private int m_MemoryAdress = memoryAdress;

        public ParameterType Type => m_Type;
        public int MemoryAdress
        {
            get => m_MemoryAdress;
            set => m_MemoryAdress = value;
        }

        public Variable(ParameterType type) : this(type, -1) { }
    }
}
