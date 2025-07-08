namespace CorpseLib.Scripts.Memories
{
    public class LiteralValue(object value) : IMemoryValue
    {
        public object Value = value;

        public IMemoryValue Clone()
        {
            object clone = Value;
            return new LiteralValue(clone);
        }

        public bool Equals(IMemoryValue? other)
        {
            if (other is LiteralValue literalValue)
                return Value.Equals(literalValue.Value);
            return false;
        }
    }
}
