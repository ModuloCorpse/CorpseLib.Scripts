using CorpseLib.Scripts.Memories;

namespace CorpseLib.Scripts.Parameters
{
    public class TemporaryStringValue(string str) : ITemporaryValue
    {
        private readonly string m_Value = str;

        public string Str => m_Value;

        public ITemporaryValue Clone() => new TemporaryStringValue(new string(m_Value));

        public AMemoryValue Allocate(Heap heap)
        {
            MemoryStringValue stringValue = new(m_Value);
            heap.Allocate(stringValue);
            return stringValue;
        }
    }
}
