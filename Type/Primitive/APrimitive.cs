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

        public override object[]? Convert(object[] value)
        {
            try
            {
                if (value.Length == 0)
                    return [];
                else if (value.Length == 1)
                    return [ConvertFrom(value[0])!];
            }
            catch { }
            return null;
        }

        protected virtual T ConvertFrom(object value) => Helper.Cast<T>(value)!;
    }

    public class BoolType() : APrimitive<bool>() { }
    public class ShortType() : APrimitive<short>() { }
    public class UShortType() : APrimitive<ushort>() { }
    public class CharType() : APrimitive<sbyte>() { }
    public class UCharType() : APrimitive<byte>() { }
    public class IntType() : APrimitive<int>() { }
    public class UIntType() : APrimitive<uint>() { }
    public class LongType() : APrimitive<long>() { }
    public class ULongType() : APrimitive<ulong>() { }
    public class FloatType() : APrimitive<float>() { }
    public class DoubleType() : APrimitive<double>() { }

    public class StringType() : APrimitive<string>()
    {
        protected override string ConvertFrom(object value)
        {
            string str = (value as string)!;
            if (str.Length > 0 && str[0] == '"')
                str = str[1..];
            if (str.Length > 0 && str[^1] == '"')
                str = str[..^1];
            return str;
        }
    }
}
