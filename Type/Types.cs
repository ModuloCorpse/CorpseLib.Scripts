using CorpseLib.Scripts.Type.Primitive;
using System.Diagnostics.CodeAnalysis;

namespace CorpseLib.Scripts.Type
{
    public class Types
    {
        public static readonly ATypeInstance VOID = new VoidType();
        public static readonly ATypeInstance BOOL = new BoolType();
        public static readonly ATypeInstance CHAR = new CharType();
        public static readonly ATypeInstance UCHAR = new UCharType();
        public static readonly ATypeInstance SHORT = new ShortType();
        public static readonly ATypeInstance USHORT = new UShortType();
        public static readonly ATypeInstance INT = new IntType();
        public static readonly ATypeInstance UINT = new UIntType();
        public static readonly ATypeInstance LONG = new LongType();
        public static readonly ATypeInstance ULONG = new ULongType();
        public static readonly ATypeInstance FLOAT = new FloatType();
        public static readonly ATypeInstance DOUBLE = new DoubleType();
        public static readonly ATypeInstance STRING = new StringType();

        internal static bool TryGet(string type, [MaybeNullWhen(false)] out ATypeInstance? instance)
        {
            instance = type switch
            {
                "void" => VOID,
                "bool" => BOOL,
                "char" => CHAR,
                "uchar" => UCHAR,
                "short" => SHORT,
                "ushort" => USHORT,
                "int" => INT,
                "uint" => UINT,
                "long" => LONG,
                "ulong" => ULONG,
                "float" => FLOAT,
                "double" => DOUBLE,
                "string" => STRING,
                _ => null
            };
            return instance != null;
        }
    }
}
