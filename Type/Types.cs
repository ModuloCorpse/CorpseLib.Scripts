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
        private static readonly Dictionary<int, ATypeInstance> m_Table = new() {
            { ConversionTable.ConvertStr("void"), VOID },
            { ConversionTable.ConvertStr("bool"), BOOL },
            { ConversionTable.ConvertStr("char"), CHAR },
            { ConversionTable.ConvertStr("uchar"), UCHAR },
            { ConversionTable.ConvertStr("short"), SHORT },
            { ConversionTable.ConvertStr("ushort"), USHORT },
            { ConversionTable.ConvertStr("int"), INT },
            { ConversionTable.ConvertStr("uint"), UINT },
            { ConversionTable.ConvertStr("long"), LONG },
            { ConversionTable.ConvertStr("ulong"), ULONG },
            { ConversionTable.ConvertStr("float"), FLOAT },
            { ConversionTable.ConvertStr("double"), DOUBLE },
            { ConversionTable.ConvertStr("string"), STRING },
        };

        internal static bool TryGet(int type, [MaybeNullWhen(false)] out ATypeInstance? instance) => m_Table.TryGetValue(type, out instance);
    }
}
