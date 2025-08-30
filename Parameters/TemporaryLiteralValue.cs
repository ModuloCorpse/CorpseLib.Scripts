using CorpseLib.Scripts.Memories;

namespace CorpseLib.Scripts.Parameters
{
    public class TemporaryLiteralValue(object value) : ITemporaryValue
    {
        private readonly object m_Value = value;

        public object Value => m_Value;

        public ITemporaryValue Clone()
        {
            object clone = Value;
            return new TemporaryLiteralValue(clone);
        }

        public AMemoryValue Allocate(Heap heap)
        {
            MemoryLiteralValue literalValue = new(m_Value);
            heap.Allocate(literalValue);
            return literalValue;
        }
    }
}
