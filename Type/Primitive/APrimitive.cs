namespace CorpseLib.Scripts.Type.Primitive
{
    public abstract class APrimitive<T>() : ARawPrimitive()
    {
        public override bool IsOfType(object[]? value)
        {
            if (value == null || value.Length == 0)
                return false;
            else if (Helper.ChangeType(value[0], out T? _))
                return true;
            return false;
        }

        public override object[]? Parse(string str) { try { return [ConvertFrom(str)!]; } catch { return null; } }

        public override string ToString(object[]? value)
        {
            if (value == null)
                throw new ArgumentException("Primitive has no value");
            if (value.Length == 0)
                return "null";
            else
            {
                if (Helper.ChangeType(value[0], out T? ret))
                    return ConvertTo(ret!);
            }
            throw new ArgumentException("Primitive value is not of the right type");
        }

        protected abstract T ConvertFrom(string str);
        protected virtual string ConvertTo(T value) => value!.ToString() ?? string.Empty;
    }

    public class BoolType : APrimitive<bool>
    {
        public BoolType() : base() { }
        protected override bool ConvertFrom(string str) => (str == "true");
        protected override string ConvertTo(bool value) => (value) ? "true" : "false";
    }

    public class ShortType : APrimitive<short>
    {
        public ShortType() : base() { }
        protected override short ConvertFrom(string str) => short.Parse(str);
    }

    public class UShortType : APrimitive<ushort>
    {
        public UShortType() : base() { }
        protected override ushort ConvertFrom(string str) => ushort.Parse(str);
    }

    public class CharType : APrimitive<sbyte>
    {
        public CharType() : base() { }
        protected override sbyte ConvertFrom(string str) => sbyte.Parse(str);
        protected override string ConvertTo(sbyte value) => string.Format("{0}", (char)value);
    }

    public class UCharType : APrimitive<byte>
    {
        public UCharType() : base() { }
        protected override byte ConvertFrom(string str) => byte.Parse(str);
        protected override string ConvertTo(byte value) => string.Format("{0}", (char)value);
    }

    public class IntType : APrimitive<int>
    {
        public IntType() : base() { }
        protected override int ConvertFrom(string str) => int.Parse(str);
    }

    public class UIntType : APrimitive<uint>
    {
        public UIntType() : base() { }
        protected override uint ConvertFrom(string str) => uint.Parse(str);
    }

    public class LongType : APrimitive<long>
    {
        public LongType() : base() { }
        protected override long ConvertFrom(string str) => long.Parse(str);
    }

    public class ULongType : APrimitive<ulong>
    {
        public ULongType() : base() { }
        protected override ulong ConvertFrom(string str) => ulong.Parse(str);
    }

    public class FloatType : APrimitive<float>
    {
        public FloatType() : base() { }
        protected override float ConvertFrom(string str) => float.Parse(str);
    }

    public class DoubleType : APrimitive<double>
    {
        public DoubleType() : base() { }
        protected override double ConvertFrom(string str) => double.Parse(str);
    }

    public class StringType : APrimitive<string>
    {
        public StringType() : base() { }
        protected override string ConvertFrom(string str)
        {
            if (str.Length > 0 && str[0] == '"')
                str = str[1..];
            if (str.Length > 0 && str[^1] == '"')
                str = str[..^1];
            return str;
        }
        protected override string ConvertTo(string str) => string.Format("\"{0}\"", str);
    }
}
