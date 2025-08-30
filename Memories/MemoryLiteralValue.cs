namespace CorpseLib.Scripts.Memories
{
    public class MemoryLiteralValue(object value) : AMemoryValue
    {
        public object Value = value;

        public override AMemoryValue Clone(Heap _)
        {
            object clone = Value;
            return new MemoryLiteralValue(clone);
        }

        public override bool Equals(AMemoryValue? other)
        {
            if (other is MemoryLiteralValue literalValue)
                return Value.Equals(literalValue.Value);
            return false;
        }
    }
}
